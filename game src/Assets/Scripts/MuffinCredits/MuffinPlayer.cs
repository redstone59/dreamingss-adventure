using MuffinCredits.JumpKingPlayer;
using MuffinCredits.TutorialPlayer;
using Speedrun;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MuffinPlayer : MonoBehaviour
{
    public AchievementManager achievementManager;
    public Autoscroll cameraScroll;

    public Rigidbody2D dreamingBody;
    public SpriteRenderer dreamingSprite;
    public TilemapCollider2D ladderCollider;
    public TilemapCollider2D groundCollider;
    public TilemapCollider2D deathCollider;
    public AudioSource no;

    public bool inTutorial = true;
    public float tutorialEndYPos;
    public float achievementYPos;
    public TutorialPlayer tutorialPlayer;
    public JumpKingPlayer jumpKingPlayer;

    public SpeedrunTimer speedrunTimer;
    public int numberOfSplits = 0;

    // Start is called before the first frame update
    void Start()
    {
        dreamingBody = GetComponent<Rigidbody2D>();
        dreamingSprite = GetComponent<SpriteRenderer>();

        InitialisePlayers();

        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
        if (achievementManager == null) return;
        speedrunTimer = GameObject.Find("Speedrun Timer").GetComponent<SpeedrunTimer>();
        speedrunTimer.Initialise(
            "Beat the Game",
            "Tutorial Zone",
            "Jump King Tutorial Zone",
            "The Red Corridor",
            "Pick A Way",
            "Don't Choke!"
        );
        numberOfSplits = 0;

        if (PlayerPrefs.GetInt("DontSaveProgress", 0) != 0) return;

        int numberOfWins = PlayerPrefs.GetInt("NumberOfVictories", 0);
        if (PlayerPrefs.GetInt("LeftSTA", 0) != 0)
        {
            PlayerPrefs.SetInt("NumberOfVictories", numberOfWins + 1);
            PlayerPrefs.SetInt("HighestSavedLevel", 9999);
            achievementManager.UnlockAchievement(AllAchievements.Adventured);
            if (PlayerPrefs.GetInt("HardMode", 0) != 0)
                achievementManager.UnlockAchievement(AllAchievements.Retired);
            PlayerPrefs.DeleteKey("LeftSTA");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (inTutorial && transform.localPosition.y >= tutorialEndYPos)
            inTutorial = false;
        else if (transform.localPosition.y >= achievementYPos)
            achievementManager.UnlockAchievement(AllAchievements.Hiker);

        CheckForSplits();

        if (inTutorial)
            tutorialPlayer.Update();
        else
            jumpKingPlayer.Update();
    }

    private void CheckForSplits()
    {
        switch (numberOfSplits)
        {
            case 0:
                if (transform.localPosition.y >= tutorialEndYPos)
                {
                    speedrunTimer.Split();
                    numberOfSplits++;
                }
                break;
            case 1:
                if (transform.localPosition.y >= -275f)
                {
                    speedrunTimer.Split();
                    numberOfSplits++;
                }
                break;
            case 2:
                if (transform.localPosition.y >= -237f)
                {
                    speedrunTimer.Split();
                    numberOfSplits++;
                }
                break;
            case 3:
                if (transform.localPosition.y >= -216f)
                {
                    speedrunTimer.Split();
                    numberOfSplits++;
                }
                break;
            case 4:
                if (transform.localPosition.y >= achievementYPos)
                {
                    speedrunTimer.Split();
                    numberOfSplits++;
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (dreamingBody.IsTouching(deathCollider))
        {
            speedrunTimer.FailedRun();
            no.Play();
            GameObject.Find("Main Camera").GetComponent<Autoscroll>().OnDreamingDeath(dreamingBody.transform.position.y);
            dreamingBody.gameObject.SetActive(false);
            bool pastHalfway = (transform.position.y - cameraScroll.initialPosition.y) / (cameraScroll.endPosition.y - cameraScroll.initialPosition.y) >= 0.5;
            if (achievementManager != null && pastHalfway)
                achievementManager.UnlockAchievement(AllAchievements.AllForNaught);
        }

        if (inTutorial)
            tutorialPlayer.FixedUpdate();
        else
            jumpKingPlayer.FixedUpdate();
    }

    private void InitialisePlayers()
    {
        tutorialPlayer.dreamingBody = dreamingBody;
        tutorialPlayer.dreamingSprite = dreamingSprite;
        tutorialPlayer.ladderCollider = ladderCollider;
        tutorialPlayer.groundCollider = groundCollider;

        jumpKingPlayer.dreamingBody = dreamingBody;
        jumpKingPlayer.dreamingSprite = dreamingSprite;
        jumpKingPlayer.ladderCollider = ladderCollider;
        jumpKingPlayer.groundCollider = groundCollider;
    }

    public void StartTimer()
    {
        speedrunTimer.BeginTimer();
    }
}