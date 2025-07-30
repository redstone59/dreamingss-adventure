using RogueLikeAtDreamings;
using RogueLikeAtDreamings.Elevator;
using RogueLikeAtDreamings.Hallucinations;
using RogueLikeAtDreamings.Internet;
using RogueLikeAtDreamings.Jumpscare;
using RogueLikeAtDreamings.NightCompleteScreen;
using RogueLikeAtDreamings.Rooms;
using RogueLikeAtDreamings.Structs;
using RogueLikeAtDreamings.ScoreScreen;
using RogueLikeAtDreamings.Tabs;

using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Speedrun;
using Keys;

public class RLAD_GameManager : MonoBehaviour
{
    public RLADAnimatronic _dreaming;
    public DreamingState _dreamingState;
    public RLADAnimatronic _gaster;
    public GasterState _gasterState;

    public RLADPlayer player;

    public RLAD_Time clock;
    private float timeRateThisNight;
    private bool unfreezeNextHour = false;
    private bool powerHasCutThisNight;

    public TextMeshProUGUI nightDisplayText;

    public RLAD_Power power;
    public RLAD_Camera gameCamera;
    private float capacityThisNight;
    private float passiveDrainMultiplier;

    public RLAD_Office office;
    public RLAD_Map map;

    public Hallucination hallucination;
    public int successfulMoves;

    public Jumpscare jumpscare;
    public ScoreScreen scoreScreen;
    public RLAD_NightResults results;
    public RLAD_NightCompleteScreen nightComplete;

    public RLAD_Lure lureSystem;
    public Internet internet;
    private float defaultInternetSpeed;

    public RLAD_SFX sfx;
    public RLAD_Ambience ambience;

    public Elevator elevator;

    public Tabs tabs;
    public TextMeshProUGUI debtText;
    public Button[] buyDataButtons;

    public RLAD_FlashBeacon beacon;

    public bool hasSeenYep = false;
    public Image yep;

    public GameObject laserPointer;
    public AchievementManager achievementManager;
    public SpeedrunTimer speedrunTimer;

    public PhoneGuy phoneGuy;

    public float scalingMultiplier;

    public int night;
    private bool inPlay = false;

    // Start is called before the first frame update
    void Start()
    {
        _dreaming.SetManager(this);
        _dreaming.moveFunction = DreamingMovementCheck;
        _dreaming.NonAttackTargets = new Room[]
        {
            Room.Ensuite,
            Room.BlurayCollection,
            Room.Kitchen,
            Room.FrontDoor,
            Room.Veranda
        };
        office._dreaming = _dreaming;

        _gaster.SetManager(this);
        _gaster.moveFunction = GasterMovementCheck;
        _gaster.NonAttackTargets = new Room[]
        {
            Room.Kitchen,
            Room.Bathroom,
            Room.LivingRoom,
            Room.Laundry
        };

        office._gaster = _gaster;

        office.FlashCallback = OnOfficeFlash;
        night = 0;

        tabs.itemSelection.SetCallback(UseItemCallback);

        if (SaveSystem.IsDemo()) return;

        scalingMultiplier = PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0
                                ? 1.5f
                                : 1f;

        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();

        speedrunTimer = GameObject.Find("Speedrun Timer").GetComponent<SpeedrunTimer>();
        speedrunTimer.Initialise("Complete 5 Nights", "Night 1", "Night 2", "Night 3", "Night 4", "Night 5");
    }

    // Update is called once per frame
    void Update()
    {
        if (!inPlay) return;
        clock.secondsPassed += Time.deltaTime;
        //power.UpdateDisplayText(capacityThisNight);

        debtText.text = $"${player.debt:N0}";
        EnableBuyDataButtons();
        CheckAnimatronicInBasement();
        CheckElevatorIdle();
        UpdateAmbience();

        if (clock.currentTime < clock.finishTime)
        {
            _dreaming.UpdatePatience();
            _gaster.UpdatePatience();
        }

        if (power.powerUsed >= capacityThisNight)
        {
            OnPowerCutInitial();

            gameCamera.active = false;
        }
        else
        {
            float currentUsage = power.currentUsage + power.passiveDrain * passiveDrainMultiplier;
            if (gameCamera.active) currentUsage += 0.2f;
            power.powerUsed += Time.deltaTime * currentUsage;
        }

        if (clock.secondsPassed >= timeRateThisNight)
        {
            if (_dreaming.CurrentRoom == Room.Basement_PointOfNoReturn || _gaster.CurrentRoom == Room.Basement_PointOfNoReturn) return;

            // Use of `for` loop to ensure hour events are processed properly in case of severe lag (if two hours pass at once)
            for (int hoursPassed = (int)(clock.secondsPassed / timeRateThisNight); 
                 hoursPassed > 0; 
                 hoursPassed--)
            {
                clock.currentTime++;
                clock.secondsPassed -= timeRateThisNight;
                OnHourPassed();
            }

            clock.UpdateDisplayText();
        }
    }

    void FixedUpdate()
    {
        // he like stream or dinner and a viddy
        if ((_gaster.CurrentRoom == Room.GastersRoom || _gaster.CurrentRoom == Room.DiningRoom) && internet.OutOfData())
        {
            _gaster.Target = Room.Basement_Dead;
            _gaster.ForceMovementOpportunity();
        }
    }

    private void UpdateAmbience()
    {
        bool aboutToDie = _dreaming.CurrentRoom == Room.Basement_PointOfNoReturn ||
                          _dreaming.CurrentRoom == Room.Basement_Dead            ||
                          _gaster.CurrentRoom   == Room.Basement_PointOfNoReturn ||
                          _gaster.CurrentRoom   == Room.Basement_Dead            ;
        
        if (aboutToDie)
        {
            ambience.danger.mute = true;
            ambience.main.mute = true;
            return;
        }
        ambience.main.mute = false;
        ambience.danger.mute = RoomUtilities.GetFloor(_dreaming.CurrentRoom, this) == Floor.Upper &&
                               RoomUtilities.GetFloor(_gaster.CurrentRoom, this) == Floor.Upper &&
                               !internet.OutOfData();
    }

    private void CheckAnimatronicInBasement()
    {
        bool gasterInBasement = RoomUtilities.GetFloor(_gaster.CurrentRoom, this) == Floor.Basement;
        bool dreamingInBasement = RoomUtilities.GetFloor(_dreaming.CurrentRoom, this) == Floor.Basement;

        //ambience.volume = gasterInBasement || dreamingInBasement ? 0 : 0.4f;
    }

    private void CheckElevatorIdle()
    {
        Floor gasterFloor = RoomUtilities.GetFloor(_gaster.CurrentRoom, this);
        Floor dreamingFloor = RoomUtilities.GetFloor(_dreaming.CurrentRoom, this);
        bool noOneInElevator = !(_dreaming.CurrentRoom == Room.Elevator || _gaster.CurrentRoom == Room.Elevator);
        float neededIdle = elevator.floor == Floor.Basement ? 15 : 40; // Discourage keeping the elevator at the basement

        if (!noOneInElevator) { elevator.IdleTime = 0; }
        else if (noOneInElevator && elevator.IdleTime >= neededIdle)
        {
            elevator.IdleTime = 0;

            elevator.floor = internet.OutOfData() ? gasterFloor : dreamingFloor;
        }
    }

    private void OnHourPassed()
    {
        bool willDie = jumpscare.gameObject.activeSelf || _dreaming.CurrentRoom == Room.Basement_Dead || _gaster.CurrentRoom == Room.Basement_Dead;
        if (clock.currentTime >= clock.finishTime && !willDie)
            EndNight();
        
        AnimatronicOnHourPassed();
        InternetOnHourPassed();
    }

    private void InternetOnHourPassed()
    {
        if (clock.currentTime == 2 && player.temporaryUpgrades.sleepingPills)
        {
            defaultInternetSpeed /= 2 / 3f;
        }
        if (internet.DataMultiplier == defaultInternetSpeed)
        {
            int chanceForDataDrain = (night + 1) switch
            {
                     1 => 25,
                2 or 3 => 20,
                     4 => 18,
                     5 => 10,
                 <= 10 => 8,
                 <= 20 => 5,
                     _ => 3
            };
            if (Random.Range(0, chanceForDataDrain) == 0)
            {
                internet.DataMultiplier *= night switch {
                    0 => 1.5f,
                    1 => 1.5f,
                    2 => 5 / 3f,
                    3 => 5 / 3f,
                    4 => 1.75f,
                    _ => Random.Range(1.5f, 1.5f + 0.1f * (night + 1))
                };
            }
        }
        else
        {
            float chanceForDrainToEnd = night switch {
                   0 => 1,
                   1 => 1,
                   2 => 0.9f,
                   3 => 0.85f,
                   4 => 0.8f,
                < 10 => 0.75f,
                < 15 => 0.6f,
                < 20 => 0.5f,
                   _ => 0.4f
            };
            if (Random.Range(0, 1f) <= chanceForDrainToEnd)
                internet.DataMultiplier = defaultInternetSpeed;
        }
    }

    private void AnimatronicOnHourPassed()
    {
        int chanceOfAggressionIncrease = night switch {
            <= 3  => 20,
            <= 4  => 15,
            <= 5  => 10,
            <= 7  => 8,
            <= 10 => 5,
            <= 20 => 3,
            <  25 => 2,
            >= 25 => 1
        };
        if (Random.Range(0, chanceOfAggressionIncrease) == 0)
            _dreaming.aggression += PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0 ? 0.05f : 0.02f;
        if ((clock.currentTime - 2) % 12 == 0 || (clock.currentTime - 3) % 12 == 0) _dreaming.AILevel += 5;

        if (night == 0 && clock.currentTime == 4) // make it possible to die on the first night
        {
            _dreaming.Target = Room.Basement_Dead;
            _dreaming.AILevel = 80;
            _dreaming.opportunitySpeed = 150;
        }

        if (unfreezeNextHour)
        {
            _dreaming.Freeze(false);
            unfreezeNextHour = false;
        }
    }


    public void OnPowerCutInitial()
    {
        if (powerHasCutThisNight) return;
        _dreaming.aggression = 1;
        _dreaming.opportunitySpeed /= 2;
        _dreaming.opportunityWiggle /= 2;
        powerHasCutThisNight = true;
        //ambience.Stop();
    }

    public void StartNight()
    {
        if (night == 0) speedrunTimer?.BeginTimer();

        results = new()
        {
            // Assuming a rate of $12/hr
            nightPayout = (int)(72 * (3 + night / 5) / 3), 
            challengePayout = null,
            lureSystemUses = 0,
            maintenanceCosts = 0,
            bonus = (night + 1) % 5 == 0 ? (night + 1) / 5 * 300 : 0,
            debtInterest = Mathf.RoundToInt(player.debt * 0.2f)
        };

        player.debt = Mathf.RoundToInt(player.debt * 1.2f);

        office.canOpenCamera = true;
        gameCamera.active = false;

        if (player.permanentUpgrades.hasHomeTheatre) results.maintenanceCosts += 20;
        if (player.permanentUpgrades.solarSystem) results.maintenanceCosts += 5;

        _dreaming.NewMovementOpportunity();
        _gaster.NewMovementOpportunity();
        
        if (player?.currentChallenge?.positive == Patches.AllTheGlyphs)
        {
            night = Mathf.Max(0, night - 3);
        }

        InitialiseTime();
        InitialisePower();
        InitialiseAmbience();

        beacon.uses = 0;
        beacon.maxUses = night == 0 ? 120 : 65;

        InitialiseDreaming();
        InitialiseGaster();
        AddChallengeModifications();
        InitialiseInternet();
        UseUpgrades();

        // Lower internet usage in Hard Mode due to lower pay (yes im nerfing hard mode fuck you)
        if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0)
            defaultInternetSpeed *= 0.85f;

        beacon.uses--;
        beacon.Use();

        nightDisplayText.text = $"Night {night + 1:N0}";
        
        elevator.muted = true;
        elevator.floor = Floor.Upper;
        elevator.muted = false;
        elevator.IdleTime = 0;

        _dreaming.Freeze(false);
        _gaster.Freeze(false);

        tabs.ChangeTab(0);
        gameCamera.transparentStaticAnimation.isPlaying = false;
        gameCamera.heavyStaticAnimation.isPlaying = false;
        gameCamera.fertilisedVeranda = false;
        gameCamera.disabled = player != null && player.currentChallenge != null && player.currentChallenge.negative == Patches.VideoMalfunction;
        
        if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) == 0 && (night + 1 == 2 || night + 1 == 3))
        {
            // Prevent the player from dying if they didn't buy anything from the Shop
            player.inventory.Add(RLAD_Items.SingleBluray);
        }

        if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) == 0 && night < 4)
            StartCoroutine(WaitBeforePhoneRings());

        successfulMoves = 0;
        inPlay = true;
    }

    private IEnumerator WaitBeforePhoneRings()
    {
        yield return new WaitForSecondsRealtime(1 + Random.Range(0f, 2f));

        phoneGuy.Call(night);
    }

    public void EndNight()
    {
        _dreaming.Freeze(true);
        ambience.main.Stop();
        ambience.danger.Stop();
        speedrunTimer?.Split();
        phoneGuy.source.Stop();

        int nightlyPay = PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0
                             ? 50 + 10 * (night / 5)
                             : 75 + 25 * (night / 5);
        int actualPayout = nightlyPay
                         + results.bonus
                         + (results.challengePayout != null ? (int)results.challengePayout : 0)
                         - (results.maintenanceCosts + 10 * results.lureSystemUses);
        player.money += actualPayout;

        if (achievementManager != null)
        {
            if (night + 1 >= 1) // Just in case
                achievementManager.UnlockAchievement(AllAchievements.TheFirstNight);
            if (night + 1 >= 5)
                achievementManager.UnlockAchievement(AllAchievements.YouDidIt);
            if (night + 1 >= 7)
                achievementManager.UnlockAchievement(AllAchievements.Overachiever);

            if (beacon.uses == 0 || (player.currentChallenge?.negative == Patches.NoRecharge && beacon.uses == beacon.maxUses / 5))
                achievementManager.UnlockAchievement(AllAchievements.Greenrunner);
            if (player.currentChallenge != null)
                achievementManager.UnlockAchievement(AllAchievements.OneHundredAndTenPercent);
        }

        night++;
        player.nightsCompleted++;

        nightComplete.gameObject.SetActive(true);
        StartCoroutine(nightComplete.PayoutAnimation(clock, player, results, actualPayout, nightlyPay));
        gameCamera.ResetFloor();
        gameCamera.active = false;

        if (player.currentChallenge?.positive == Patches.ForgiveDebt)
        {
            player.debt = 0;
        }

        if ((night + 1) % 5 == 0) hasSeenYep = false;
        inPlay = false;
    }

    private void UseUpgrades()
    {
        float oneHourGameTime = timeRateThisNight;

        // Start of night stun items
        if (player.temporaryUpgrades.clickteamOneDay)
        {
            _dreaming.Stun((int)(oneHourGameTime * 2 / Time.fixedDeltaTime));
            player.temporaryUpgrades.clickteamOneDay = false;
        }
        if (player.temporaryUpgrades.adobeOneDay)
        {
            _dreaming.Stun((int)(oneHourGameTime * 2 / Time.fixedDeltaTime));
            _dreaming.aggression *= 1.1f;
            
            player.temporaryUpgrades.adobeOneDay = false;
        }
        if (player.temporaryUpgrades.multiplayerSteamGame > 0)
        {
            int stunTime = (int)(oneHourGameTime / Time.fixedDeltaTime);
            _dreaming.Stun(stunTime);
            _gaster.Stun(stunTime);
            player.temporaryUpgrades.multiplayerSteamGame--;
        }
        if (player.temporaryUpgrades.sleepingPills)
        {
            _gaster.Stun((int)(2 * oneHourGameTime / Time.fixedDeltaTime));
            defaultInternetSpeed *= 2 / 3f;
            player.temporaryUpgrades.sleepingPills = false;
        }
        
        // Internet data items
        if (player.temporaryUpgrades.extraData)
        {
            internet.AddData(15);
            player.temporaryUpgrades.extraData = false;
        }
        if (player.permanentUpgrades.fiberConnection)
        {
            internet.AddData(10);
        }

        // Room-specific items
        if (player.temporaryUpgrades.maintenanceGuy)
        {
            elevator.IdleTime = int.MinValue;
            elevator.floor = Floor.Basement;
            player.temporaryUpgrades.maintenanceGuy = false;
        }

        lureSystem.SetActive(player.permanentUpgrades.hasLuringSystem);
        lureSystem.UpdateCost(0);
    }

    private void AddChallengeModifications()
    {
        player.discount = 0;
        gameCamera.disabled = false;
        laserPointer.SetActive(false);
        if (player.currentChallenge == null) return;
        player.challengesCompleted += 1;
        Patch positive, negative;
        (positive, negative) = (player.currentChallenge.positive, player.currentChallenge. negative);

        if (positive == Patches.PenaltyRates)
        {
            results.challengePayout = Mathf.FloorToInt(0.5f * (75 + results.bonus));
        }
        else if (positive == Patches.PartTime)
        {
            clock.finishTime -= 3;
            // 0.5f * 0.8f because the night length is halved and 0.8f is the nerf
            _dreaming.opportunitySpeed = (int)(_dreaming.opportunitySpeed * 0.5f * 0.8f);
            _dreaming.opportunityWiggle = (int)(_dreaming.opportunityWiggle * 0.5f * 0.8f);
            _gaster.opportunitySpeed = (int)(_gaster.opportunitySpeed * 0.5f * 0.8f);
            _gaster.opportunityWiggle = (int)(_gaster.opportunityWiggle * 0.5f * 0.8f);
        }
        else if (positive == Patches.ExtraData)
        {
            internet.AddData(10);
        }
        else if (positive == Patches.ClearanceSale)
        {
            player.discount = 0.25f;
        }
        else if (positive == Patches.TimeDilation)
        {
            timeRateThisNight *= 0.6f;
        }
        else if (positive == Patches.TheChallenge)
        {
            timeRateThisNight = 60f / clock.finishTime;
            _dreaming.opportunitySpeed = (int)(0.5f / Time.fixedDeltaTime);
            _dreaming.opportunityWiggle = 0;
            _dreaming.PatienceMultiplier = 0.6f;
            _gaster.opportunitySpeed = (int)(0.5f / Time.fixedDeltaTime);
            _gaster.opportunityWiggle = 0;
            _gaster.PatienceMultiplier = 0.6f;
        }
        else if (positive == Patches.ElevatorMalfunction)
        {
            _dreaming.PreferElevator = false;
            _gaster.PreferElevator = false;
        }
        else if (positive == Patches.Liquidation)
        {
            player.discount = 0.5f;
        }
        // Hiero, Petro, and Astero are handled before this.
        else if (positive == Patches.AccountingError)
        {
            results.bonus += (night + 6) / 5 * 300;
        }
        else if (positive == Patches.Boxset)
        {
            player.inventory.Add(RLAD_Items.SingleBluray);
            player.inventory.Add(RLAD_Items.SingleBluray);
        }

        if (negative == Patches.LongerNight)
        {
            clock.finishTime += 2;
        }
        else if (negative == Patches.EnergyDrinks)
        {
            _dreaming.opportunitySpeed = (int)(_dreaming.opportunitySpeed * 0.6f);
            _dreaming.opportunityWiggle = (int)(_dreaming.opportunityWiggle * 0.6f);
            _gaster.opportunitySpeed = (int)(_gaster.opportunitySpeed * 0.6f);
            _gaster.opportunityWiggle = (int)(_gaster.opportunityWiggle * 0.6f);
        }
        else if (negative == Patches.Torrent)
        {
            internet.DataMultiplier *= 3;
        }
        else if (negative == Patches.Overtime)
        {
            clock.finishTime += 3;
        }
        else if (negative == Patches.VideoMalfunction)
        {
            gameCamera.disabled = true;
        }
        else if (negative == Patches.Underpayment)
        {
            results.nightPayout *= 2;
            results.nightPayout /= 3;
            results.challengePayout *= 2;
            results.challengePayout /= 3;
            results.bonus *= 2;
            results.bonus /= 3;
        }
        else if (negative == Patches.Gastathon)
        {
            defaultInternetSpeed = 2;
        }
        else if (negative == Patches.AssetSeizure)
        {
            player.money = 0;
        }
        else if (negative == Patches.MaxMode)
        {
            _dreaming.AILevel = 100;
            _gaster.AILevel = 100;
        }
        else if (negative == Patches.Aggressive)
        {
            _dreaming.aggression = Mathf.Max(0.8f, _dreaming.aggression);
            if (_dreaming.aggression > 0.8f) _dreaming.aggression = 1;
            _gaster.aggression = Mathf.Max(0.8f, _gaster.aggression);
            if (_gaster.aggression > 0.8f) _gaster.aggression = 1;
        }
        else if (negative == Patches.DoubleNight)
        {
            clock.finishTime *= 2;
        }
        else if (negative == Patches.LaserPointer)
        {
            laserPointer.SetActive(true);
        }
        else if (negative == Patches.NoRecharge)
        {
            beacon.uses = (int)(beacon.maxUses * (1 - 0.2f));
        }
    }    

    private void InitialiseInternet()
    {
        internet.Initialise(
            timeRateThisNight * (night switch {
                0 => 6 - (Random.Range(0, 10) == 0 ? 1 : 0),
                1 => 5 + Random.Range(-0.5f, 0.5f),
                2 => 4 + Random.Range(-1f, 1f),
                3 => 2 + Random.Range(0f, 1.5f),
                4 => 1,
                _ => Random.Range(0.75f, 2.5f)
            })
        );
        internet.DailyGigabytes = 15;
        defaultInternetSpeed = player.permanentUpgrades.fiberConnection ? 2 / 3f : 1;
        internet.DataMultiplier = night == 0 ? 0.05f : defaultInternetSpeed;
        internet.DataJiggle = night + 1 >= 3 ? 0.4f : 0.2f;
        internet.WarningThreshold = (night + 1) switch
        {
            <=  3 => 1,
            <=  5 => 1.5f,
            <  10 => 2,
            >= 10 => 3
        };

        foreach (GameObject warningObject in internet.warningSignsObjects)
            warningObject.SetActive(false);
    }

    private void InitialiseDreaming()
    {
        _dreaming.CurrentRoom = Room.DreamingsRoom;
        _dreaming.Target = Room.Hallway;
        _dreaming.AILevel = (int)((night + 1) switch {
                1 => PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0 ? 5 : 0,
                2 => 15,
                3 => 20,
                4 => 35,
                5 => 50,
            <  30 => 50 - 0.074f * (night - 4) * (night - 56),
            >= 30 => 100
        });
        _dreaming.AILevel = Mathf.FloorToInt(scalingMultiplier * _dreaming.AILevel * Mathf.Pow(0.995f, player.permanentUpgrades.numPlushies));
        Debug.Log($"AI Level: {_dreaming.AILevel}");
        _dreaming.opportunitySpeed = (night + 1) switch {
                1 => (int)(7.5f / Time.fixedDeltaTime),
                2 => (int)(6f / Time.fixedDeltaTime),
                3 => (int)(5f / Time.fixedDeltaTime),
            <=  5 => (int)(4f / Time.fixedDeltaTime),
            <  35 => Mathf.RoundToInt(80 * Mathf.Pow(0.8f, Mathf.Pow(1.09f, night - 5)) * 0.05f / Time.fixedDeltaTime), // correction factor since i just assumed fixedDeltaTime = 0.05f 
            >= 35 => 1
        };
        Debug.Log($"Opportunity speed: {_dreaming.opportunitySpeed} fixed updates ({_dreaming.opportunitySpeed * Time.fixedDeltaTime}s)");
        _dreaming.opportunityWiggle = (int)((night + 1) switch {
            <   5 => 0,
            <  30 => 0.0032f * Mathf.Pow(night - 5, 3),
            >= 30 => 20
        });
        Debug.Log($"Opportunity wiggle: +/-{_dreaming.opportunityWiggle} fixed updates");
        _dreaming.aggression = scalingMultiplier * (night + 1) switch {
            <= 5 => -0.01f * night * (night - 20),
            <= 10 => 0.05f * night + 0.25f,
            <= 19 => 0.75f,
            <  30 => (night - 20) / 40f + 0.75f,
            >= 30 => 1
        };
        Debug.Log($"Aggression: {_dreaming.aggression}");
        _dreaming.PatienceMultiplier = night switch {
               0 => 3f,
               1 => 2.5f,
               2 => 2.5f,
               3 => 2f,
               4 => 1.5f,
            < 10 => 1.3f,
            < 15 => 1,
            < 30 => 0.8f,
               _ => 0.4f
        };
        _dreaming.InBasement = false;
        _dreamingState = new()
        {
            lastAggression = _dreaming.aggression
        };

        _dreaming.PreferElevator = night + 1 >= 2;

        int startStunMinutes = (night + 1) switch {
            <  3 => 0,
            <  5 => 1,
            < 10 => 3,
            < 30 => 5,
                _ => 1,
        };
        int stunLength = (int)(startStunMinutes * timeRateThisNight / (60 * Time.fixedDeltaTime));
        _dreaming.Stun(stunLength);
    }

    private void InitialiseGaster()
    {
        _gaster.CurrentRoom = Room.GastersRoom;
        _gaster.Target = Room.Hallway;
        _gaster.AILevel = night == 0
                            ? 0
                            : PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0
                                  ? 100
                                  : 80;
        _gaster.opportunitySpeed = night switch {
               0 => (int)((4 * 60 + 55) / Time.fixedDeltaTime), // 4:55
               1 => 530,                // good framerule
               2 => 500,
               3 => 430,                // sluggy
               4 => 319,                // gaster
               5 => 300,
               6 => 200,
            < 10 => 150,
            < 20 => 100,
            < 25 => 75,
               _ => 50
        };
        if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0 && night < 6)
        {
            _gaster.opportunitySpeed = (int)(5 / Time.fixedDeltaTime);
        }
        _gaster.PatienceMultiplier = 1;
        _gaster.InBasement = false;
        _gaster.PreferElevator = PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0 || night + 1 > 2;
        _gaster.opportunityWiggle = 20;
        _gaster.aggression = 0;

        _gasterState = new()
        {
            spaghettified = false
        };

        if (night + 1 >= 3)
        {
            float stunTimeHours = night + 1 >= 20
                                    ? 4
                                    : night + 1 >= 10
                                        ? 2
                                        : night + 1 >= 3
                                            ? 1
                                            : 0;
            _gaster.Stun(Mathf.RoundToInt(stunTimeHours * timeRateThisNight / Time.fixedDeltaTime));
        }
    }

    private void InitialiseAmbience()
    {
        ambience.main.Play();
        ambience.danger.Play();
        ambience.danger.mute = true;
    }

    private void InitialisePower()
    {
        power.powerUsed = 0;
        power.currentUsage = 0;
        capacityThisNight = 1.5f * power.baseCapacity * timeRateThisNight * clock.finishTime * Mathf.Pow(1.04f, -night);
        //if (player.currentChallenge == RLADChallengeTypes.RollingBlackout)
        //{
        //    capacityThisNight *= 0.5f;
        //}
        passiveDrainMultiplier = 0.05f * Mathf.Pow(1.15f, night) * Mathf.Pow(14 / 15f, player.permanentUpgrades.powerDrainage * 0.2f);
        powerHasCutThisNight = false;

        if (player.permanentUpgrades.solarSystem && Random.Range(0, 10) != 0) // Solar system can flunk out due to weather or whatever
        {
            power.powerUsed -= 0.1f * capacityThisNight; // Add 10% capacity
        }
    }

    private void InitialiseTime()
    {
        clock.currentTime = 0;
        clock.secondsPassed = 0;
        clock.finishTime = 6;

        // Make first, uneventful night go faster
        float timeRate = night == 0 ? 30 : clock.defaultTimeRate;
        // Exponential time slowing depending on the night, linear time slowdown depending on time upgrade
        int n = (night - 2) / 3;
        timeRateThisNight = timeRate * Mathf.Pow(1.03f, n) / (1 + 0.1f * player.permanentUpgrades.timeSpeed);

        //if (player.currentChallenge == RLADChallengeTypes.TimeDilationFast)
        //    timeRateThisNight *= 0.5f;
        //else if (player.currentChallenge == RLADChallengeTypes.TimeDilationSlow)
        //    timeRateThisNight *= 2f;
        
        clock.UpdateDisplayText();
    }

    public (bool canMove, bool reroll) DreamingMovementCheck(Room potentialRoom)
    {
        if (player.temporaryUpgrades.bananaPeel == potentialRoom)
        {
            sfx.bananaPeel.volume = 0.1f / RoomUtilities.GetDistanceBetween(this, Room.Basement_Far, potentialRoom);
            sfx.bananaPeel.Play();
            _dreaming.Stun((int)(timeRateThisNight / 60f));
            _dreaming.aggression += 0.1f;
            _dreaming.AILevel += 5;
            player.temporaryUpgrades.bananaPeel = null;
        }

        switch (potentialRoom)
        {
            case Room.Elevator:
                Floor dreamingFloor = RoomUtilities.GetFloor(_dreaming.CurrentRoom, this);
                Floor targetFloor = RoomUtilities.GetFloor(_dreaming.Target, this);
                int floorDifference = Mathf.Abs((int)dreamingFloor - (int)targetFloor);
                
                if (floorDifference == 0) return (false, true);
                
                int twoMinutes = (int)(timeRateThisNight / 30f);
                elevator.floor = targetFloor;
                _dreaming.Stun(twoMinutes * floorDifference);
                break;
            case Room.Veranda:
                if (_dreaming.Target == Room.Veranda && gameCamera.fertilisedVeranda)
                {
                    gameCamera.fertilisedVeranda = false;
                }
                break;
            case Room.Basement_Far:
                _dreaming.InBasement = true;
                _dreaming.Patience = Random.Range(0, night >= 4 ? 1f : 2f);
                break;
            case Room.Basement_Dead:
                Jumpscare();
                break;
        }

        if (RoomUtilities.GetFloor(potentialRoom, this) != Floor.Basement)
            gameCamera.HeavyStatic(potentialRoom, _dreaming.CurrentRoom);
        int hallucinationEvery = player.temporaryUpgrades.prescriptionMedication ? 125 : 10;
        if (_dreaming.InBasement && ++successfulMoves % hallucinationEvery == 0)
            hallucination.Attempt(night);
        return (true, false);
    }

    public (bool canMove, bool reroll) GasterMovementCheck(Room potentialRoom)
    {
        switch (potentialRoom)
        {
            case Room.GastersRoom:
                bool gasterNoticesOutage = !player.permanentUpgrades.nes || clock.currentTime >= clock.finishTime - 2;
                if (gasterNoticesOutage && internet.OutOfData() && _gaster.Target == Room.GastersRoom)
                {
                    _gaster.Target = Room.Basement_Dead;
                    _gaster.aggression = 1;
                }
                break;
            case Room.Elevator:
                Floor gasterFloor = RoomUtilities.GetFloor(_gaster.CurrentRoom, this);
                Floor targetFloor = RoomUtilities.GetFloor(_gaster.Target, this);
                int floorDifference = Mathf.Abs((int)gasterFloor - (int)targetFloor);
                
                if (floorDifference == 0) return (false, true);
                
                int twoMinutes = (int)(timeRateThisNight / 30f);
                elevator.floor = targetFloor;
                _gaster.Stun(twoMinutes * floorDifference);
                break;
           case Room.DiningRoom:
                if (_gasterState.spaghettified)
                {
                    _gaster.Stun(Mathf.FloorToInt(timeRateThisNight * 2 * 20));
                    _gasterState.spaghettified = false;
                }
                break;
            case Room.Veranda:
                if (_gaster.Target == Room.Veranda && gameCamera.fertilisedVeranda)
                {
                    gameCamera.fertilisedVeranda = false;
                }
                break;
            case Room.Basement_Far:
                _gaster.InBasement = true;
                _gaster.Patience = 1f;
                break;
            case Room.Basement_Dead:
                Jumpscare();
                break;
        }

        // Hacky but it forces Gaster to go back to his room.
        bool gasterRoomNotAtEndOfTargetQueue = _gaster.TargetQueue.Count == 0 || _gaster.TargetQueue[^1] != Room.GastersRoom;
        bool notAtOrGoingToGastersRoom = potentialRoom != Room.GastersRoom && _gaster.CurrentRoom != Room.GastersRoom;
        if (notAtOrGoingToGastersRoom && gasterRoomNotAtEndOfTargetQueue)
        {
            _gaster.TargetQueue.Add(Room.GastersRoom);
        }

        Debug.Log(string.Join(", ", _gaster.TargetQueue));

        if (_gaster.Target != Room.GastersRoom && _gaster.CurrentRoom == _gaster.Target)
        {
            _gaster.Stun(Mathf.FloorToInt(timeRateThisNight * Random.Range(0.5f, 1.2f) / Time.fixedDeltaTime));
        }
        else if (_gaster.Target == Room.GastersRoom && _gaster.CurrentRoom == _gaster.Target)
        {
            int longestUntilNextBreak = night + 1 >= 20
                                          ? 6
                                          : night + 1 >= 10
                                              ? 5
                                              : night + 1 >= 5
                                                  ? 4
                                                  : 3;
            _gaster.Stun(Mathf.FloorToInt(timeRateThisNight * Random.Range(2, longestUntilNextBreak) / Time.fixedDeltaTime));
        }

        Debug.Log($"gaster moved to {potentialRoom}");
        if (RoomUtilities.GetFloor(potentialRoom, this) != Floor.Basement)
            gameCamera.HeavyStatic(potentialRoom, _gaster.CurrentRoom);
        return (true, false);
    }

    public void Jumpscare()
    {
        if (scoreScreen.gameObject.activeSelf || nightComplete.gameObject.activeSelf) return;

        hallucination.gameObject.SetActive(false);
        _dreaming.Stun(int.MaxValue);
        _dreaming.Freeze(true);
        _gaster.Stun(int.MaxValue);
        _gaster.Freeze(true);
        StartCoroutine(HangWhileOnCamera());
    }

    public IEnumerator HangWhileOnCamera()
    {
        float timeElapsed = 0;
        float tenMinutesGameTime = timeRateThisNight / 6;
        while (gameCamera.active && timeElapsed < tenMinutesGameTime)
        {
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        gameCamera.active = false;
        jumpscare.gameObject.SetActive(true);
        yield return null;
        jumpscare.image.sprite = _gaster.CurrentRoom == Room.Basement_Dead ? jumpscare.gasterSprite : jumpscare.dreamingSprite;
        while (jumpscare.source.isPlaying) yield return null;
        ambience.main.Stop();
        ambience.danger.Stop();
        jumpscare.gameObject.SetActive(false);
        scoreScreen.gameObject.SetActive(true);
        StartCoroutine(scoreScreen.ScoreAnimation(player));
    }

    public void UseLureSystem()
    {
        int roomDistance = RoomUtilities.GetDistanceBetween(this, _dreaming.CurrentRoom, gameCamera.SelectedRoom);
        Debug.Log(roomDistance);
        if (roomDistance > 3) return;

        _dreaming.Target = gameCamera.SelectedRoom;
        lureSystem.UpdateCost(10 * ++results.lureSystemUses);
        results.lureSystemUses++;
    }

    public bool OnOfficeFlash()
    {
        System.Func<RLADAnimatronic, bool> cantBePushed = (animatronic) => animatronic.CurrentRoom == Room.Basement_Far 
                                                                        || animatronic.CurrentRoom == Room.Basement_PointOfNoReturn;
        
        if (beacon.uses >= beacon.maxUses) return false;
        beacon.Use();

        yep.color = Color.clear;
        if (!hasSeenYep && Random.Range(0, night > 2 ? 250 : 100) == 0)
        {
            yep.color = Color.white;
            hasSeenYep = true;
        }

        float pushback = player.currentChallenge?.positive == Patches.TheChallenge
                             ? 2.3f
                             : night + 1 < 3
                                   ? 1.5f
                                   : 1.15f;
        if (_dreaming.InBasement)
        {
            if (!(night + 1 >= 5 && cantBePushed(_dreaming)))
                _dreaming.Patience -= pushback + Time.deltaTime;
        }
        if (_gaster.InBasement)
        {
            if (!(night + 1 >= 5 && cantBePushed(_gaster)))
                _gaster.Patience -= pushback + Time.deltaTime;
        }
        return true;
    }

    public void BuyData(int index)
    {
        int cost = new int[] { 5, 18, 25, 40, 175, 1000 }[index];
        if (player.debt + cost >= 10 * player.money) return;
        player.debt += cost;
        internet.AddData(new float[] { 0.5f, 3f, 5f, 10f, 50f, 1000f }[index]);
    }

    public void EnableBuyDataButtons()
    {
        int[] costs = { 5, 18, 25, 40, 175, 1000 };
        for (int i = 0; i < buyDataButtons.Length; i++)
        {
            Button button = buyDataButtons[i];
            int cost = costs[i];
            button.interactable = player.debt + cost < 10 * player.money;
        }
    }

    public void UseItemCallback(RLAD_Items item)
    {
        if (!player.HasItem(item)) return;
        
        int gasterDistance = RoomUtilities.GetDistanceBetween(this, _gaster.CurrentRoom, Room.Basement_Dead);
        int dreamingDistance = RoomUtilities.GetDistanceBetween(this, _dreaming.CurrentRoom, Room.Basement_Dead);
        RLADAnimatronic closest = gasterDistance < dreamingDistance ? _gaster : _dreaming;

        switch (item)
        {
            case RLAD_Items.SingleBluray:
                if (_dreaming.InBasement)
                {
                    if (_dreaming.CurrentRoom != Room.Basement_PointOfNoReturn)
                    {
                        _dreaming.CurrentRoom = Room.Stairwell_Bottom;
                        _dreaming.Target = Room.BlurayCollection;
                        _dreaming.InBasement = false;
                    }
                }
                break;
            case RLAD_Items.CakePremix:
                if (_dreaming.InBasement)
                {
                    if (_dreaming.CurrentRoom != Room.Basement_PointOfNoReturn)
                    {
                        _dreaming.CurrentRoom = Room.Stairwell_Bottom;
                        _dreaming.Target = Room.Kitchen;
                        _dreaming.InBasement = false;
                    }
                }
                break;
            case RLAD_Items.BurnerPhone:
                closest.Target = Room.LivingRoom;
                if (closest.InBasement)
                {
                    if (closest.CurrentRoom != Room.Basement_PointOfNoReturn)
                    {
                        closest.CurrentRoom = Room.Stairwell_Bottom;
                    }
                    closest.InBasement = false;
                }
                break;
            case RLAD_Items.Fertiliser:
                closest.Target = Room.Veranda;
                gameCamera.fertilisedVeranda = true;
                if (gameCamera.SelectedRoom == Room.Veranda)
                {
                    gameCamera.HeavyStatic(Room.Veranda);
                    gameCamera.ChangeCurrentCamera((int)Room.Veranda);
                }
                if (closest.InBasement)
                {
                    if (closest.CurrentRoom != Room.Basement_PointOfNoReturn)
                    {
                        closest.CurrentRoom = Room.Stairwell_Bottom;
                    }
                    closest.InBasement = false;
                }
                break;
            case RLAD_Items.SpaghettiDelivery:
                _gaster.TargetQueue.Add(Room.FrontDoor);
                _gaster.TargetQueue.Add(Room.DiningRoom);
                _gasterState.spaghettified = true;
                _gaster.ForceTargetQueue();
                if (_gaster.InBasement)
                {
                    if (_gaster.CurrentRoom != Room.Basement_PointOfNoReturn)
                    {
                        _gaster.CurrentRoom = Room.Stairwell_Bottom;
                    }
                    _gaster.InBasement = false;
                }
                break;
        }
    }
}