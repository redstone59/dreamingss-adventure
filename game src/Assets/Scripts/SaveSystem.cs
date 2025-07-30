using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Keys;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct MinigameScore
{
    public int highScore;
    public float bestTime;
    public int attempts;
    public int completedRuns;
    public float[] splits;     // Times on the run corresponding to the `bestTime` run.
    public float[] pbDeltas;   // Delta between splits corresponding to the `bestTime` run.
    public float[] bestDeltas; // Delta between splits used for gold splits / sum of best.

    public readonly float GetSumOfBest()
    {
        float sob = 0;
        foreach (float split in bestDeltas)
        {
            sob += split;
        }
        return sob;
    }

    public readonly float[] GetBestPaceSplits(float[] currentSplits)
    {
        List<float> splits = new(currentSplits);
        throw new NotImplementedException("SOB comparison is not implemented.");
    }
}

public struct MinigameData
{
    public string sceneName;
    public MinigameScore normal;
    public MinigameScore hard;
    public int achievements;
}

public struct WholeGameData
{
    public int totalScore;
    public int savedLevel;
    public int highestSavedLevel;
    public int numberofVictories;
    public int bestScoreNormal;
    public int bestScoreHard;
    public Dictionary<string, int> accessoryValues;
    public int outsideOfMinigameAchievements;
}

static class SaveSystemKeys
{
    public const string GAME_PREFIX = "g_";
    public const string GAME_TOTAL_SCORE = "g_ts";
    public const string GAME_SAVED_LEVEL = "g_sl";
    public const string GAME_HIGHEST_SAVED_LEVEL = "g_hsl";
    public const string GAME_NUMBER_OF_VICTORIES = "g_v";
    public const string GAME_BEST_SCORE_NORMAL = "g_bsn";
    public const string GAME_BEST_SCORE_HARD = "g_bsh";
    public const string GAME_MISC_ACHIEVEMENTS = "g_ooma";

    public const string MINIGAME_PREFIX = "mg_";
    public const string MINIGAME_ACHIEVEMENTS = "ach";
    public const string MINIGAME_NORMAL_BLOCK = "n";
    public const string MINIGAME_HARD_BLOCK = "h";

    public const string BLOCK_HIGH_SCORE = "hs";
    public const string BLOCK_BEST_TIME = "bt";
    public const string BLOCK_ATTEMPTS = "a";
    public const string BLOCK_COMPLETED_RUNS = "c";
    public const string BLOCK_SPLITS = "s";
    public const string BLOCK_DELTAS = "pbd";
    public const string BLOCK_BEST_DELTAS = "bd";

    // Minigame data is saved as follows:
    // mg_fdagt(ach:achievements, n:{normal}, h:{hard});
    // where {normal} and {hard} follow the structure
    // {hs:high_score,bt:best_time,a:attempts,s:[splits],pbd:[pb_deltas],bd:[best_deltas]}
    public static string GenerateMinigameKey(string scene)
    {
        return MINIGAME_PREFIX + string.Join("", scene.Where(c => char.IsUpper(c))).ToLower();
    }

    public static bool IsGameKey(string key) => key.StartsWith(GAME_PREFIX);
    public static bool IsMinigameKey(string key) => key.StartsWith(MINIGAME_PREFIX);
    public static bool IsAccessoryKey(string key) => !(IsGameKey(key) || IsMinigameKey(key));
}

public static class SaveSystem
{
    public static WholeGameData GameData;
    public static List<MinigameData> MinigameData;
    private const string SAVE_FILE_NAME = "save_data.dssa";
    private const string EDITOR_SAVE_FILE_NAME = "editor_save_data.dssa";
    private static string SaveFile
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, Application.isEditor ? EDITOR_SAVE_FILE_NAME : SAVE_FILE_NAME);
        }
    }

    private static bool _hasLoaded = false;

    public static MinigameData GetMinigameData()
    {
        return MinigameData[Array.IndexOf(LevelOrder.sceneNames, SceneManager.GetActiveScene().name)];
    }

    public static void SetMinigameData(MinigameData value)
    {
        MinigameData[Array.IndexOf(LevelOrder.sceneNames, SceneManager.GetActiveScene().name)] = value;
    }

    public static float GetPBSplit(int index, bool onHardMode)
    {
        MinigameData minigameData = GetMinigameData();
        float[] currentSplits = onHardMode ? minigameData.hard.splits : minigameData.normal.splits;

        if (index >= currentSplits.Length) return float.PositiveInfinity;
        return currentSplits[index];
    }

    public static float GetPBDelta(int index, bool onHardMode)
    {
        MinigameData minigameData = GetMinigameData();
        float[] currentSplits = onHardMode ? minigameData.hard.pbDeltas : minigameData.normal.pbDeltas;

        if (index >= currentSplits.Length) return float.PositiveInfinity;
        return currentSplits[index];
    }

    public static float GetBestDelta(int index, bool onHardMode)
    {
        MinigameData minigameData = GetMinigameData();
        float[] currentSplits = onHardMode ? minigameData.hard.bestDeltas : minigameData.normal.bestDeltas;

        if (index >= currentSplits.Length) return float.PositiveInfinity;
        return currentSplits[index];
    }

    public static void LoadSaveFile()
    {
        if (IsDemo())
        {
            InitialiseSaveData();
            return;
        }
        if (_hasLoaded) return;
        _hasLoaded = true;
        InitialiseSaveData();
        if (!File.Exists(SaveFile))
        {
            ResetSaveData();
            return;
        }
        string toParse = File.ReadAllText(SaveFile).Decompress();
        Debug.Log($"Decompressed save file is: {toParse}");

        int totalEntries = toParse.Count(c => c == ';');
        for (int entryIndex = 0; entryIndex < totalEntries; entryIndex++)
        {
            string entry = GetSaveEntry(entryIndex, toParse)[0..^1]; // Remove trailing ';'
            var splitEntry = entry.Split(':', 2);
            if (splitEntry.Length != 2)
            {
                Debug.Log($"Invalid save data entry '{entry}' with split length of {splitEntry.Length}!");
                continue;
            }

            string key = splitEntry[0];
            string value = splitEntry[1];

            if (SaveSystemKeys.IsGameKey(key)) ParseGameEntry(key, value);
            else if (SaveSystemKeys.IsMinigameKey(key)) ParseMinigameEntry(entry);
            else GameData.accessoryValues.Add(key, int.Parse(value));
        }
    }

    private static string GetSaveEntry(int index, string text)
    {
        StringBuilder builder = new(128);
        int currentIndex = 0;

        foreach (char c in text)
        {
            if (currentIndex != index)
            {
                if (c == ';') currentIndex++;
                continue;
            }

            builder.Append(c);
            if (c == ';') break;
        }

        return builder.ToString();
    }

    private static void ParseGameEntry(string key, string v)
    {
        Debug.Log($"Parsing entry '{key}' with value '{v}'...");
        if (!int.TryParse(v, out int value))
        {
            Debug.Log($"Value {v} cannot be parsed as an integer!");
            return;
        }

        switch (key)
        {
            case SaveSystemKeys.GAME_TOTAL_SCORE:
                GameData.totalScore = value;
                break;
            case SaveSystemKeys.GAME_SAVED_LEVEL:
                GameData.savedLevel = value;
                break;
            case SaveSystemKeys.GAME_HIGHEST_SAVED_LEVEL:
                GameData.highestSavedLevel = value;
                break;
            case SaveSystemKeys.GAME_NUMBER_OF_VICTORIES:
                GameData.numberofVictories = value;
                break;
            case SaveSystemKeys.GAME_BEST_SCORE_NORMAL:
                GameData.bestScoreNormal = value;
                break;
            case SaveSystemKeys.GAME_BEST_SCORE_HARD:
                GameData.bestScoreHard = value;
                break;
            case SaveSystemKeys.GAME_MISC_ACHIEVEMENTS:
                GameData.outsideOfMinigameAchievements = value;
                break;
        }
    }

    private static void ParseMinigameEntry(string entry)
    {
        var splitEntry = entry.Split('(', 2);
        if (splitEntry.Length != 2)
        {
            Debug.Log($"Invalid save data entry '{entry}' with split length of {splitEntry.Length}!");
            return;
        }

        string minigameKey = splitEntry[0];
        string block = splitEntry[1][0..^1]; // Remove trailing ')'
        Debug.Log(block);

        MinigameData parsed = new();

        string achievements = Regex.Match(block, @$"{SaveSystemKeys.MINIGAME_ACHIEVEMENTS}:(?'achievement'\d+)").Groups["achievement"].Value;
        if (!int.TryParse(achievements, out parsed.achievements))
            throw new Exception($"Invalid saved achievement value '{achievements}' for key {minigameKey}!");

        foreach (Match match in Regex.Matches(block, @"(?'key'\w+):{(?'block'[\w\W]*?)}"))
        {
            string key = match.Groups["key"].Value;
            string value = match.Groups["block"].Value;
            if (key == SaveSystemKeys.MINIGAME_NORMAL_BLOCK)
                parsed.normal = ParseScoreBlock(value);
            else if (key == SaveSystemKeys.MINIGAME_HARD_BLOCK)
                parsed.hard = ParseScoreBlock(value);
        }


        MinigameData[GetMinigameIndexFromKey(minigameKey)] = parsed;
    }

    private static MinigameScore ParseScoreBlock(string block)
    {
        MinigameScore parsed = new();

        foreach (Match match in Regex.Matches(block, @"(?'key'\w+):(?'value'(-?\d+(.\d+))|\[.*?\]|-?Infinity)"))
        {
            string key = match.Groups["key"].Value;
            string value = match.Groups["value"].Value;
            bool valueIsList = value[0] == '[';

            void ThrowIfList()
            {
                if (valueIsList)
                    throw new Exception($"List found in key {key}, expected number");
            }

            void ThrowIfNotList()
            {
                if (!valueIsList)
                    throw new Exception($"Number found in key {key}, expected list");
            }

            switch (key)
            {
                case SaveSystemKeys.BLOCK_HIGH_SCORE:
                    ThrowIfList();
                    parsed.highScore = int.Parse(value);
                    break;
                case SaveSystemKeys.BLOCK_BEST_TIME:
                    ThrowIfList();
                    parsed.bestTime = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case SaveSystemKeys.BLOCK_ATTEMPTS:
                    ThrowIfList();
                    parsed.attempts = int.Parse(value);
                    break;
                case SaveSystemKeys.BLOCK_COMPLETED_RUNS:
                    ThrowIfList();
                    parsed.attempts = int.Parse(value);
                    break;
                case SaveSystemKeys.BLOCK_SPLITS:
                    ThrowIfNotList();
                    parsed.splits = ParseFloatList(value);
                    break;
                case SaveSystemKeys.BLOCK_DELTAS:
                    ThrowIfNotList();
                    parsed.pbDeltas = ParseFloatList(value);
                    break;
                case SaveSystemKeys.BLOCK_BEST_DELTAS:
                    ThrowIfNotList();
                    parsed.bestDeltas = ParseFloatList(value);
                    break;
            }
        }

        return parsed;
    }

    private static float[] ParseFloatList(string list)
    {
        if (list.Length <= 2) return new float[0];

        list = list[1..^1]; // Remove surrounding square braces
        Debug.Log(list);
        if (list == "") return new float[0];
        return list.Split(',').Select(value => float.Parse(value, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
    }

    public static int GetMinigameIndexFromKey(string key)
    {
        for (int i = 0; i < LevelOrder.sceneNames.Length; i++)
        {
            if (key == SaveSystemKeys.GenerateMinigameKey(LevelOrder.sceneNames[i]))
                return i;
        }

        throw new KeyNotFoundException($"Key {key} does not lead to any existing minigame!");
    }

    public static void WriteSaveFile()
    {
        if (IsDemo()) return;
        Debug.Log("Writing save...");
        StringBuilder builder = new(16384);

        void WriteValue(string key, IFormattable value) => builder.Append($"{key}:{value};");

        var saveKeysValuePairs = new (string key, int value)[]
        {
            (SaveSystemKeys.GAME_TOTAL_SCORE, GameData.totalScore),
            (SaveSystemKeys.GAME_SAVED_LEVEL, GameData.savedLevel),
            (SaveSystemKeys.GAME_HIGHEST_SAVED_LEVEL, GameData.highestSavedLevel),
            (SaveSystemKeys.GAME_NUMBER_OF_VICTORIES, GameData.numberofVictories),
            (SaveSystemKeys.GAME_BEST_SCORE_NORMAL, GameData.bestScoreNormal),
            (SaveSystemKeys.GAME_BEST_SCORE_HARD, GameData.bestScoreHard),
            (SaveSystemKeys.GAME_MISC_ACHIEVEMENTS, GameData.outsideOfMinigameAchievements)
        };
        foreach ((string key, int value) in saveKeysValuePairs)
        {
            WriteValue(key, value);
        }

        foreach ((string key, int value) in GameData.accessoryValues)
        {
            WriteValue(key, value);
        }

        foreach (string scene in LevelOrder.sceneNames)
        {
            builder.Append($"{SaveSystemKeys.GenerateMinigameKey(scene)}({GenerateMinigameSaveValue(scene)});");
        }

        File.WriteAllText(SaveFile, builder.ToString().Compress());
        Debug.Log("Save file written!");
    }

    private static string GenerateMinigameSaveValue(string scene)
    {
        int index = LevelOrder.GetLevelIndex(scene);
        MinigameData save = MinigameData[index];
        string inner = "";
        inner += $"{SaveSystemKeys.MINIGAME_ACHIEVEMENTS}:{save.achievements},";
        inner += $"{SaveSystemKeys.MINIGAME_NORMAL_BLOCK}:{{" + FormatMinigameScores(save.normal) + "},";
        inner += $"{SaveSystemKeys.MINIGAME_HARD_BLOCK}:{{" + FormatMinigameScores(save.hard) + "}";
        return inner;
    }

    private static string FormatMinigameScores(MinigameScore score)
    {
        string resultant = "";

        resultant += $"{SaveSystemKeys.BLOCK_HIGH_SCORE}:{score.highScore},";
        resultant += $"{SaveSystemKeys.BLOCK_BEST_TIME}:{score.bestTime},";
        resultant += $"{SaveSystemKeys.BLOCK_ATTEMPTS}:{score.attempts},";
        resultant += $"{SaveSystemKeys.BLOCK_COMPLETED_RUNS}:{score.completedRuns},";
        resultant += $"{SaveSystemKeys.BLOCK_SPLITS}:[" + string.Join(',', score.splits.Select(v => v.ToString())) + "],";
        resultant += $"{SaveSystemKeys.BLOCK_DELTAS}:[" + string.Join(',', score.pbDeltas.Select(v => v.ToString())) + "],";
        resultant += $"{SaveSystemKeys.BLOCK_BEST_DELTAS}:[" + string.Join(',', score.bestDeltas.Select(v => v.ToString())) + "]";

        return resultant;
    }

    public static void InitialiseSaveData()
    {
        GameData = new WholeGameData
        {
            totalScore = 0,
            savedLevel = -1,
            highestSavedLevel = 0,
            numberofVictories = 0,
            bestScoreNormal = 0,
            bestScoreHard = 0,
            accessoryValues = new(),
            outsideOfMinigameAchievements = 0
        };

        MinigameData = new(LevelOrder.sceneNames.Length);

        for (int i = 0; i < LevelOrder.sceneNames.Length; i++)
        {
            MinigameData.Add(new MinigameData
            {
                sceneName = LevelOrder.sceneNames[i],
                normal = new MinigameScore
                {
                    highScore = 0,
                    bestTime = float.PositiveInfinity,
                    attempts = 0,
                    completedRuns = 0,
                    splits = new float[0],
                    pbDeltas = new float[0],
                    bestDeltas = new float[0]
                },
                hard = new MinigameScore
                {
                    highScore = 0,
                    bestTime = float.PositiveInfinity,
                    attempts = 0,
                    completedRuns = 0,
                    splits = new float[0],
                    pbDeltas = new float[0],
                    bestDeltas = new float[0]
                },
                achievements = 0
            });
        }
    }

    public static void ResetSaveData()
    {
        InitialiseSaveData();
        WriteSaveFile();
    }

    public static void ResetSplitsAndTimes()
    {
        if (!_hasLoaded)
        {
            Debug.Log("Unable to wipe splits since save has not loaded.");
            return;
        }

        for (int i = 0; i < MinigameData.Count; i++)
        {
            MinigameData data = MinigameData[i];
            WipeSplitsFromMinigameScore(ref data.normal);
            WipeSplitsFromMinigameScore(ref data.hard);
            MinigameData[i] = data;
        }

        WriteSaveFile();
    }

    private static void WipeSplitsFromMinigameScore(ref MinigameScore score)
    {
        score.attempts = 0;
        score.completedRuns = 0;
        score.bestTime = float.PositiveInfinity;
        score.splits = new float[0];
        score.pbDeltas = new float[0];
        score.bestDeltas = new float[0];
    }

    public static void CheckForLegacySaveData()
    {
        if (PlayerPrefs.GetInt(PlayerPrefKeys.HasHitNewGameOrContinue, 0) == 0)
            return;

        Debug.Log("Legacy save data detected! Porting to new save system...");
        GameData = new WholeGameData
        {
            totalScore = PlayerPrefs.GetInt(PlayerPrefKeys.TotalScore, 0),
            savedLevel = PlayerPrefs.GetInt(PlayerPrefKeys.SavedLevel, 0),
            highestSavedLevel = PlayerPrefs.GetInt(PlayerPrefKeys.HighestSavedLevel, 0),
            numberofVictories = PlayerPrefs.GetInt(PlayerPrefKeys.NumberOfVictories, 0),
            bestScoreNormal = PlayerPrefs.GetInt(PlayerPrefKeys.BestScoreNormal, 0),
            bestScoreHard = PlayerPrefs.GetInt(PlayerPrefKeys.BestScoreHard, 0),
            accessoryValues = new Dictionary<string, int>(),
            outsideOfMinigameAchievements = PlayerPrefs.GetInt(AchievementKeys.OutsideOfMinigame, 0)
        };

        if (PlayerPrefs.GetInt(PlayerPrefKeys.SayThatAnswer.HasSeenSuperUltraMegaRareSecondDreaming, 0) != 0)
        {
            GameData.accessoryValues.Add(PlayerPrefKeys.SayThatAnswer.HasSeenSuperUltraMegaRareSecondDreaming, 1);
        }

        GameData.accessoryValues.Add(
            PlayerPrefKeys.SayThatAnswer.GamesNoEgg,
            PlayerPrefs.GetInt(PlayerPrefKeys.SayThatAnswer.GamesNoEgg, 0)
        );

        string[] indexToKeyArray = new[] {
            AchievementKeys.FuckingDreamingAndGasterTennis,
            AchievementKeys.WorldsHardestGolf,
            AchievementKeys.RogueLikeAtDreamings,
            AchievementKeys.Simon,
            AchievementKeys.MyWayToTheGrave,
            AchievementKeys.SayThatAnswer,
            AchievementKeys.MuffinCredits
        };
        MinigameData = new(LevelOrder.sceneNames.Length);
        for (int i = 0; i < LevelOrder.sceneNames.Length; i++)
        {
            string scene = LevelOrder.GetLevelAtIndex(i);

            // why can't i just do tuple deconstruction in the struct initialisation
            float[] nSplits, nDeltas, nBestDeltas;
            (nSplits, nDeltas, nBestDeltas) = GetSplitsFromLegacySave(scene, false);

            float[] hSplits, hDeltas, hBestDeltas;
            (hSplits, hDeltas, hBestDeltas) = GetSplitsFromLegacySave(scene, true);

            MinigameData.Add(new MinigameData
            {
                sceneName = scene,
                normal = new MinigameScore
                {
                    highScore = PlayerPrefs.GetInt($"HighScore_{scene}_NormalMode", 0),
                    bestTime = PlayerPrefs.GetFloat($"BestTime_{scene}_NormalMode", 0),
                    attempts = PlayerPrefs.GetInt($"Splits_{scene}_Attempts", 0),
                    splits = nSplits,
                    pbDeltas = nDeltas,
                    bestDeltas = nBestDeltas
                },
                hard = new MinigameScore
                {
                    highScore = PlayerPrefs.GetInt($"HighScore_{scene}_HardMode", 0),
                    bestTime = PlayerPrefs.GetFloat($"BestTime_{scene}_HardMode", 0),
                    attempts = PlayerPrefs.GetInt($"Splits_{scene}_Attempts_HardMode", 0),
                    splits = hSplits,
                    pbDeltas = hDeltas,
                    bestDeltas = hBestDeltas
                },
                achievements = PlayerPrefs.GetInt(indexToKeyArray[i], 0)
            });
        }

        WriteSaveFile();
        RemoveLegacySaveData();
    }

    private static (
        float[] splits,
        float[] pbDeltas,
        float[] bestDeltas
    ) GetSplitsFromLegacySave(string scene, bool hard)
    {
        List<float> splits = new();
        List<float> pbDeltas = new();
        // Best splits weren't saved in the last iteration of the PlayerPrefs save system.

        for (int i = 0; i < 10; i++)
        {
            string splitKey = $"Split_{scene}_{(hard ? "Hard" : "Normal")}_{i}_PB";
            if (!PlayerPrefs.HasKey(splitKey)) break;

            float splitTime = PlayerPrefs.GetFloat(splitKey, float.PositiveInfinity);
            float previousSplitTime = i == 0 ? 0f : splits[i - 1];
            splits.Add(splitTime);
            pbDeltas.Add(splitTime - previousSplitTime);
        }

        return (splits.ToArray(), pbDeltas.ToArray(), pbDeltas.ToArray());
    }

    private static void RemoveLegacySaveData()
    {
        PlayerPrefs.DeleteKey(PlayerPrefKeys.TotalScore);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.SavedLevel);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.HighestSavedLevel);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.KickedOutLastOpen);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.NumberOfVictories);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.HasHitNewGameOrContinue);

        // Resetting Achievements
        PlayerPrefs.DeleteKey(AchievementKeys.OutsideOfMinigame);
        PlayerPrefs.DeleteKey(AchievementKeys.FuckingDreamingAndGasterTennis);
        PlayerPrefs.DeleteKey(AchievementKeys.WorldsHardestGolf);
        PlayerPrefs.DeleteKey(AchievementKeys.RogueLikeAtDreamings);
        PlayerPrefs.DeleteKey(AchievementKeys.Simon);
        PlayerPrefs.DeleteKey(AchievementKeys.MyWayToTheGrave);
        PlayerPrefs.DeleteKey(AchievementKeys.SayThatAnswer);
        PlayerPrefs.DeleteKey(AchievementKeys.MuffinCredits);

        // Resetting High Scores and Best Times
        for (int i = 0; i < LevelOrder.sceneNames.Length; i++)
        {
            string scene = LevelOrder.GetLevelAtIndex(i);
            PlayerPrefs.DeleteKey($"HighScore_{scene}_NormalMode");
            PlayerPrefs.DeleteKey($"HighScore_{scene}_HardMode");

            PlayerPrefs.DeleteKey($"BestTime_{scene}_NormalMode");
            PlayerPrefs.DeleteKey($"BestTime_{scene}_HardMode");
            PlayerPrefs.DeleteKey($"Splits_{scene}_Attempts");
            PlayerPrefs.DeleteKey($"Splits_{scene}_Attempts_HardMode");

            for (int j = 0; j < 10; j++)
            {
                PlayerPrefs.DeleteKey($"Split_{scene}_Normal_{j}_PB");
                PlayerPrefs.DeleteKey($"Split_{scene}_Hard_{j}_PB");
            }
        }
        PlayerPrefs.DeleteKey(PlayerPrefKeys.BestScoreNormal);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.BestScoreHard);

        // Resetting minigame-specific vars
        PlayerPrefs.DeleteKey(PlayerPrefKeys.SayThatAnswer.GamesNoEgg);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.SayThatAnswer.HasSeenSuperUltraMegaRareSecondDreaming);

        Debug.Log("Removed all legacy save data.");
    }

    public static int GetAccessoryValue(string key, int? defaultValue = 0)
    {
        if (!GameData.accessoryValues.ContainsKey(key))
        {
            if (defaultValue == null)
                throw new KeyNotFoundException($"Save data does not contain key {key}.");
            else
                return (int)defaultValue;
        }

        return GameData.accessoryValues[key];
    }

    public static void SetAccessoryValue(string key, int value)
    {
        GameData.accessoryValues[key] = value;
    }

    public static void SaveSplits(float[] splits, float[] deltas)
    {
        MinigameData data = GetMinigameData();
        if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0)
        {
            data.hard.splits = splits;
            data.hard.pbDeltas = deltas;
        }
        else
        {
            data.normal.splits = splits;
            data.normal.pbDeltas = deltas;
        }
        SetMinigameData(data);
        WriteSaveFile();
    }

    public static void UnlockAll()
    {
        GameData.numberofVictories = Mathf.Max(1, GameData.numberofVictories);
        GameData.highestSavedLevel = Mathf.Max(9999, GameData.highestSavedLevel);
        GameData.savedLevel = LevelOrder.GetLevelIndex(LevelOrder.sceneNames[^1]);
        WriteSaveFile();
    }

    public static bool IsDemo()
    {
        return Application.version.ToLower().Contains("demo") || !Application.productName.Contains("adventure");
    }
}

// Thank you to fubo on StackOverflow for this string compression code!
// See here: https://stackoverflow.com/a/17993002
static class StringCompression
{
    /// <summary>
    /// Compresses the string.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    public static string Compress(this string text)
    {
        if (Application.isEditor) return text;
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        var memoryStream = new MemoryStream();
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
        {
            gZipStream.Write(buffer, 0, buffer.Length);
        }

        memoryStream.Position = 0;

        var compressedData = new byte[memoryStream.Length];
        memoryStream.Read(compressedData, 0, compressedData.Length);

        var gZipBuffer = new byte[compressedData.Length + 4];
        Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
        Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
        return Convert.ToBase64String(gZipBuffer);
    }

    /// <summary>
    /// Decompresses the string.
    /// </summary>
    /// <param name="compressedText">The compressed text.</param>
    /// <returns></returns>
    public static string Decompress(this string compressedText)
    {
        if (Application.isEditor) return compressedText;
        byte[] gZipBuffer = Convert.FromBase64String(compressedText);
        using (var memoryStream = new MemoryStream())
        {
            int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
            memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

            var buffer = new byte[dataLength];

            memoryStream.Position = 0;
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                gZipStream.Read(buffer, 0, buffer.Length);
            }

            return Encoding.UTF8.GetString(buffer);
        }
    }
}