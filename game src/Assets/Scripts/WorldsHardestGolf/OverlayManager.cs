using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OverlayManager : MonoBehaviour
{
    public TextMeshProUGUI strokeText;
    public TextMeshProUGUI deathText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI transitionText;
    public GameObject normalTransitionImage;
    public GameObject finalLevelTransitionImage;

    public AchievementManager achievementManager;

    private bool loadNextLevel = false;
    // Start is called before the first frame update
    void Start()
    {
        strokeText.text = "STROKES: 0";
        deathText.text = "DEATHS: 0";
        levelText.text = "1/5";
        scoreText.text = "";
        normalTransitionImage.SetActive(false);
        finalLevelTransitionImage.SetActive(false);

        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateDeathCount(int deaths)
    {
        deathText.text = string.Format("DEATHS: {0}", deaths);
    }

    public void UpdateStrokeCount(int strokes)
    {
        strokeText.text = string.Format("STROKES: {0}", strokes);
    }
    
    public void UpdateLevelCount(int level/*, bool hardMode*/)
    {
        if (level > 5) level = 5;
        levelText.text = string.Format("{0}/{1}", level, /*hardMode ? */5/* : 3*/);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
    }

    public void ShowTransitionCard(string levelString, bool isFinalLevel)
    {
        if (isFinalLevel)
        {
            finalLevelTransitionImage.SetActive(true);
        }
        else
        {
            normalTransitionImage.SetActive(true);
        }

        transitionText.text = levelString;
    }

    public void HideTransitionCard()
    {
        finalLevelTransitionImage.SetActive(false);
        normalTransitionImage.SetActive(false);
        transitionText.text = "";
    }

    public IEnumerator FinalScoreAnimation(int strokes, int deaths, int strokePenalty, int deathPenalty, int maxScore, SoundManager soundManager)
    {
        strokeText.text = "";
        deathText.text = "";
        levelText.text = "";

        string currentText = "";

        normalTransitionImage.SetActive(true);

        int incrementValue = (int)(strokes / 10 / 5); // 10 seconds divided by 5 (FixedUpdate frequency)
        if (incrementValue == 0) incrementValue++;

        for (int i = 0; i < strokes; i += incrementValue)
        {
            if (i > strokes)
            {
                i = strokes;

            }
            scoreText.text = string.Format("STROKES: {0}", i);
            yield return new WaitForFixedUpdate();
        }

        scoreText.text = string.Format("STROKES: {0}", strokes);

        if (achievementManager != null)
        {
            if (strokes <= 400) achievementManager.UnlockAchievement(AllAchievements.Par);
            if (strokes <= 300) achievementManager.UnlockAchievement(AllAchievements.Birdie);
            if (strokes <= 250) achievementManager.UnlockAchievement(AllAchievements.Eagle);
            if (strokes <= 200) achievementManager.UnlockAchievement(AllAchievements.Albatross);
        }

        StartCoroutine(PreloadSimon());
        yield return new WaitForSeconds(1);

        incrementValue = (int)(deaths / 10 / 10); // 10 seconds divided by 10 (wait period)
        if (incrementValue == 0) incrementValue++;
        currentText = scoreText.text;

        for (int i = 0; i < deaths + incrementValue; i += incrementValue)
        {
            scoreText.text = string.Format(currentText + "\nDEATHS: {0}", i);
            yield return new WaitForSeconds(0.1f);
        }

        scoreText.text = string.Format(currentText + "\nDEATHS: {0}", deaths);

        if (achievementManager != null)
        {
            if (deaths <= 25) achievementManager.UnlockAchievement(AllAchievements.Putter);
            if (deaths <= 10) achievementManager.UnlockAchievement(AllAchievements.Iron);
            if (deaths <= 5) achievementManager.UnlockAchievement(AllAchievements.Wood);
            if (deaths <= 0) achievementManager.UnlockAchievement(AllAchievements.Driver);
        }

        yield return new WaitForSeconds(1);

        scoreText.text += "\n\nFINAL SCORE: ";

        yield return new WaitForSeconds(1);

        int finalScore = Mathf.Max(0, maxScore - strokes * strokePenalty - deaths * deathPenalty);
        scoreText.text += string.Format("{0}", finalScore);

        if (achievementManager != null && finalScore <= 20000)
            achievementManager.UnlockAchievement(AllAchievements.Bogey);

        LevelOrder.AddToSavedScore(finalScore);
        LevelOrder.IncrementSavedLevel();

        yield return new WaitForSeconds(3);
        while (achievementManager.AchievementsInQueue) yield return null;
        loadNextLevel = true;
    }

    private IEnumerator PreloadSimon()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(LevelOrder.GetNextLevel("WorldsHardestGolf"));
        asyncLoad.allowSceneActivation = false;

        while (!loadNextLevel) yield return null;

        asyncLoad.allowSceneActivation = true;
    }
}