using System.Collections;
using System.Collections.Generic;
using Speedrun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FDAGT_GameManager : MonoBehaviour
{
    public AchievementManager achievementManager;
    public FDAGT_ReactiveMusic music;

    public Rigidbody2D puck;
    public Collider2D dreamingGoal;
    public Collider2D gasterGoal;

    public const int scoreToWin = 5;

    public Rigidbody2D dreaming;
    public int dreamingScore = 0;
    public TextMeshProUGUI dreamingScoreText;

    public Rigidbody2D gaster;
    public int gasterScore = 0;
    public TextMeshProUGUI gasterScoreText;

    public TextMeshProUGUI victoryText;
    public bool scoringAnimationPlaying = false;

    private bool loadNextLevel;
    public GameObject cutToBlackObject;
    public GameObject tutorialText;

    private FDAGT_Puck puckScript;
    private bool fastBallBonusAchieved = false;

    public SpeedrunTimer speedrunTimer;

    // Start is called before the first frame update
    void Start()
    {
        music = GameObject.Find("Reactive Music").GetComponent<FDAGT_ReactiveMusic>();
        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
        speedrunTimer = GameObject.Find("Speedrun Timer").GetComponent<SpeedrunTimer>();
        speedrunTimer.Initialise("Beat the Game", "1 Point", "2 Points", "3 Points", "4 Points", "Victory");

        puckScript = puck.gameObject.GetComponent<FDAGT_Puck>();

        dreamingScoreText.text = "0";
        gasterScoreText.text = "0";
        victoryText.text = "";

        if (PlayerPrefs.GetInt("HardMode", 0) > 0)
        {
            dreaming.freezeRotation = false;
        }

        speedrunTimer.BeginTimer();
        ResetGame();
    }

    // Update is called once per frame
    void Update()
    {
        Cursor.lockState = victoryText.text == "" ? CursorLockMode.Confined : CursorLockMode.None;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if (scoringAnimationPlaying) return;

        fastBallBonusAchieved = fastBallBonusAchieved || puck.velocity.magnitude > 20;

        if (puck.velocity.magnitude >= 40)
            achievementManager.UnlockAchievement(AllAchievements.SafetyHazard);

        if (dreamingGoal.OverlapPoint(puck.position))
            ScoreAnimation(false);
        else if (gasterGoal.OverlapPoint(puck.position))
        {
            ScoreAnimation(true);
            
            if (puckScript.lastHitterCollision == "Opponent")
                achievementManager.UnlockAchievement(AllAchievements.Zugzwang);

            if (puckScript.collisionsWithPlayer <= 1)
                achievementManager.UnlockAchievement(AllAchievements.BareMinimumKing);
        }
    }

    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        puck.velocity = Vector2.zero;
        puck.angularVelocity = 0;
        puck.rotation = 0;
        puck.position = Vector2.zero;

        puckScript.collisionsWithPlayer = 0;
        puckScript.lastHitterCollision = null;

        dreaming.velocity = Vector2.zero;
        dreaming.position = new(-5.6f, 0);

        float newRotation;
        if (PlayerPrefs.GetInt("HardMode", 0) != 0)
        {
            newRotation = 90 * Random.Range(0, 4);
            newRotation += Random.Range(0, 4) switch {
                0 => 0,
                1 => 30,
                2 => 45,
                3 => 60,
                _ => 0
            };
        }
        else
            newRotation = 0;

        dreaming.rotation = newRotation;
        dreaming.angularVelocity = 0;
        
        gaster.velocity = Vector2.zero;
        gaster.position = new(5.6f, 0);

        StartCoroutine(RoundStartAnimation());
    }

    public IEnumerator RoundStartAnimation()
    {
        bool pushTowardGaster = dreamingScore == gasterScore ? Random.Range(0, 1f) > 0.5f : dreamingScore > gasterScore;
        puck.velocity = new(2 * (pushTowardGaster ? 1 : -1), 0);
        
        UpdateMusicVolumes();
        music.PlayMusic();
        yield return null;
    }

    public void UpdateMusicVolumes()
    {
        // Bass
        if (dreamingScore == 0 && gasterScore == 0)
        {
            music.intensityBass.volume = 0;
        }
        else
        {
            music.intensityBass.volume = music.musicMasterVolume;
        }

        // Warning Siren and Overtones
        music.warningSiren.volume = gasterScore == scoreToWin - 1 ? music.musicMasterVolume * 0.5f : 0;
        music.overtones.volume = dreamingScore >= scoreToWin -2 ? music.musicMasterVolume : 0;
    }

    public void ScoreAnimation(bool dreamingScored)
    {
        scoringAnimationPlaying = true;
        tutorialText.SetActive(false);
        victoryText.text = dreamingScored ? "dreaming" : "gaster";

        if (dreamingScored) speedrunTimer.Split();

        bool someoneHasWon = (dreamingScored ? dreamingScore : gasterScore) + 1 >= scoreToWin;
        if (someoneHasWon)
        {
            music.PlayVictoryStinger();

            victoryText.text += "\nwins!";
            StartCoroutine(FinalScoreIncrease(dreamingScored));
            
            string nextLevel;
            if (dreamingScored)
            {
                Cursor.lockState = CursorLockMode.None;
                nextLevel = LevelOrder.GetNextLevel("FuckingDreamingAndGasterTennis");
            }
            else
                nextLevel = "TitleScreen";
            
            loadNextLevel = false;
            StartCoroutine(PreloadLevel(nextLevel));
            return;
        }

        music.PlayPointStinger();
        StartCoroutine(ScoreIncreaseAnimation(dreamingScored));

        victoryText.text += "\nscored!";
        
        //StartCoroutine(WindowsMovieMakerAssText());
    }

    public IEnumerator ScoreIncreaseAnimation(bool dreamingScored)
    {

        yield return new WaitForSecondsRealtime(2.5f);
        
        if (dreamingScored)
            dreamingScoreText.text = (++dreamingScore).ToString();
        else
            gasterScoreText.text = (++gasterScore).ToString();
        
        if (achievementManager != null)
        {
            if (dreamingScore == 4 && gasterScore == 4)
                achievementManager.UnlockAchievement(AllAchievements.ImagineTheNerves);
        }

        yield return new WaitForSecondsRealtime(1.78f);
        scoringAnimationPlaying = false;
        victoryText.text = "";
        ResetGame();
    }

    public IEnumerator FinalScoreIncrease(bool dreamingScored)
    {
        yield return new WaitForSecondsRealtime(4.96f);
        if (dreamingScored)
            dreamingScoreText.text = scoreToWin.ToString();
        else
            gasterScoreText.text = scoreToWin.ToString();
        
        if (achievementManager != null)
        {
            if (dreamingScored)
            {
                if (gasterScore == 0)
                    achievementManager.UnlockAchievement(AllAchievements.Sweep);
                else if (gasterScore == scoreToWin - 1)
                    achievementManager.UnlockAchievement(AllAchievements.NervesOvercame);
            }
            else if (!dreamingScored)
            {
                if (dreamingScore == 0)
                    achievementManager.UnlockAchievement(AllAchievements.Swept);
                else if (dreamingScore == scoreToWin - 1)
                    achievementManager.UnlockAchievement(AllAchievements.NervesImagined);
            }
        }

        yield return new WaitForSecondsRealtime(3.7f);
        cutToBlackObject.SetActive(true);

        yield return new WaitForSecondsRealtime(4);
        int minigameScore = scoreToWin - gasterScore;
        minigameScore *= 7500;
        if (fastBallBonusAchieved) minigameScore += 2500;
        LevelOrder.AddToSavedScore(minigameScore * 3); // Balance scores from other games which are all around mid 100k maximum (except simon)
        LevelOrder.IncrementSavedLevel();
        loadNextLevel = true;
    }

    public IEnumerator WindowsMovieMakerAssText()
    {
        int numberOfFixedUpdates = (int)(3 / Time.fixedDeltaTime);
        float progress;
        int elapsedUpdates = 0;
        while (elapsedUpdates < numberOfFixedUpdates)
        {
            progress = elapsedUpdates / numberOfFixedUpdates;
            victoryText.color = new(1, 1, 1, 1 - progress);
            victoryText.gameObject.transform.localScale = (0.25f + progress) * new Vector3(1, 1, 1);
            yield return new WaitForFixedUpdate();
            elapsedUpdates++;
        }
        yield return null;
    }

    public IEnumerator PreloadLevel(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!loadNextLevel) yield return null;

        asyncLoad.allowSceneActivation = true;
    }
}
