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
        MinigameData currentMinigameSaveData = SaveSystem.GetMinigameData();

        bool isPB = time < (hardMode ? currentMinigameSaveData.hard.bestTime : currentMinigameSaveData.normal.bestTime);

        if (hardMode)
        {
            currentMinigameSaveData.hard.bestTime = Mathf.Min(
                currentMinigameSaveData.hard.bestTime,
                time
            );
        }
        else
        {
            currentMinigameSaveData.normal.bestTime = Mathf.Min(
                currentMinigameSaveData.normal.bestTime,
                time
            );
        }

        SaveSystem.SetMinigameData(currentMinigameSaveData);
        return isPB;
    }

    public static void AddToSavedScore(int minigameScore)
    {
        bool hardMode = PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0;
        MinigameData currentMinigameSaveData = SaveSystem.GetMinigameData();

        if (hardMode)
        {
            currentMinigameSaveData.hard.highScore = Mathf.Max(
                currentMinigameSaveData.hard.highScore,
                minigameScore
            );
        }
        else
        {
            currentMinigameSaveData.normal.highScore = Mathf.Max(
                currentMinigameSaveData.normal.highScore,
                minigameScore
            );
        }
        SaveSystem.SetMinigameData(currentMinigameSaveData);

        if (PlayerPrefs.GetInt(PlayerPrefKeys.DontSaveProgress, 0) != 0)
            return;

        SaveSystem.GameData.totalScore += minigameScore;

        if (hardMode)
        {
            SaveSystem.GameData.bestScoreHard = Mathf.Max(
                SaveSystem.GameData.bestScoreHard,
                SaveSystem.GameData.totalScore
            );
        }
        else
        {
            SaveSystem.GameData.bestScoreNormal = Mathf.Max(
                SaveSystem.GameData.bestScoreNormal,
                SaveSystem.GameData.totalScore
            );
        }
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

        SaveSystem.GameData.savedLevel++;

        SaveSystem.GameData.highestSavedLevel = Mathf.Max(
            SaveSystem.GameData.savedLevel,
            SaveSystem.GameData.highestSavedLevel
        );

        SaveSystem.WriteSaveFile();
    }

    public static int GetLevelIndex(string sceneName) { return Array.IndexOf(sceneNames, sceneName); }
    public static string GetLevelAtIndex(int index) { return sceneNames[index]; }
}