using System;
using UnityEngine;
using Keys;
using UnityEngine.SceneManagement;

public static class LevelOrder
{
    public static string[] sceneNames = new string[]
    {
        "FuckingDreamingAndGasterTennis",
        "WorldsHardestGolf",
        "RogueLikeAtDreamings",
        "SimonScream",
        "MyWayToTheGrave",
        "SayThatAnswer",
        "MuffinCredits"
    };

    public static bool FinishSpeedrun(float time)
    {
        bool hardMode = PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0;
        string highScoreSuffix = hardMode ? "HardMode" : "NormalMode";
        float bestTimeThisScene = PlayerPrefs.GetFloat($"BestTime_{SceneManager.GetActiveScene().name}_{highScoreSuffix}", float.PositiveInfinity);
        if (time < bestTimeThisScene)
        {
            PlayerPrefs.SetFloat($"BestTime_{SceneManager.GetActiveScene().name}_{highScoreSuffix}", time);
            return true;
        }
        return false;
    }

    public static void AddToSavedScore(int minigameScore)
    {
        bool hardMode = PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0;
        string highScoreSuffix = hardMode ? "HardMode" : "NormalMode";
        int highscoreThisScene = PlayerPrefs.GetInt($"HighScore_{SceneManager.GetActiveScene().name}_{highScoreSuffix}", 0);
        if (minigameScore > highscoreThisScene)
        {
            PlayerPrefs.SetInt($"HighScore_{SceneManager.GetActiveScene().name}_{highScoreSuffix}", minigameScore);
        }

        if (PlayerPrefs.GetInt(PlayerPrefKeys.DontSaveProgress, 0) != 0)
            return;

        int totalScore = PlayerPrefs.GetInt(PlayerPrefKeys.TotalScore, 0);
        int currentScore = totalScore + minigameScore;
        PlayerPrefs.SetInt(PlayerPrefKeys.TotalScore, currentScore);
        int bestScore = PlayerPrefs.GetInt(
                            hardMode
                                ? PlayerPrefKeys.BestScoreHard
                                : PlayerPrefKeys.BestScoreNormal,
                            0
                        );
        if (currentScore > bestScore)
            PlayerPrefs.SetInt(
                hardMode
                    ? PlayerPrefKeys.BestScoreHard
                    : PlayerPrefKeys.BestScoreNormal,
                currentScore
            );
    }

    public static string GetNextLevel(string sceneName)
    {
        int levelIndex = Array.IndexOf(sceneNames, sceneName);
        bool goToTitleScreen = levelIndex == -1 || PlayerPrefs.GetInt(PlayerPrefKeys.DontSaveProgress, 0) != 0;
        return goToTitleScreen ? "TitleScreen" : sceneNames[levelIndex + 1];
    }

    public static void IncrementSavedLevel()
    {
        if (PlayerPrefs.GetInt(PlayerPrefKeys.DontSaveProgress, 0) != 0)
        {
            PlayerPrefs.SetInt(PlayerPrefKeys.DontSaveProgress, 0);
            return;
        }

        int currentLevel = PlayerPrefs.GetInt(PlayerPrefKeys.SavedLevel);
        PlayerPrefs.SetInt(PlayerPrefKeys.SavedLevel, currentLevel + 1);

        int highestSavedLevel = PlayerPrefs.GetInt(PlayerPrefKeys.HighestSavedLevel, 0);
        if (currentLevel > highestSavedLevel)
            PlayerPrefs.SetInt(PlayerPrefKeys.HighestSavedLevel, currentLevel);

        PlayerPrefs.Save();
    }

    public static int GetLevelIndex(string sceneName) { return Array.IndexOf(sceneNames, sceneName); }
    public static string GetLevelAtIndex(int index) { return sceneNames[index]; }
}