using System;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Keys;
using Speedrun;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu instance;
    public GameObject menu;
    public GameObject[] visibleOnTitle;
    public GameObject[] visibleOnMinigames;

    public Button[] levelSelectButtons;
    public AudioMixerAdjust mixer;

    public ResolutionAdjust resolution;
    public TextMeshProUGUI resolutionHeight;
    public TMP_Dropdown fullScreenDropdown;
    public Toggle jumpscareToggle;
    public Toggle hardModeToggle;
    public Toggle speedrunToggle;
    public TextMeshProUGUI statsText;
    public GameObject timerSettings;

    public TMP_Dropdown speedrunVertical;
    public TMP_Dropdown speedrunHorizontal;
    public Slider speedrunOpacity;

    public TextMeshProUGUI versionNumberText;

    public CursorLockMode previousLockState;
    public bool previousCursorVisibility;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        if (instance) DestroyImmediate(gameObject);
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            fullScreenDropdown.itemText.text = Screen.fullScreenMode switch
            {
                FullScreenMode.Windowed => "Windowed",
                FullScreenMode.MaximizedWindow => "Maximised Window",
                FullScreenMode.FullScreenWindow => "Borderless Fullscreen",
                FullScreenMode.ExclusiveFullScreen => "Exclusive Fullscreen",
                _ => throw new ArgumentException("what")
            };
            jumpscareToggle.isOn = PlayerPrefs.GetInt(PlayerPrefKeys.JumpscareSoundSubstituted, 0) != 0;
            speedrunToggle.isOn = PlayerPrefs.GetInt(PlayerPrefKeys.SpeedrunMode, 0) != 0;
            timerSettings.SetActive(speedrunToggle.isOn);

            menu.SetActive(false);
            mixer.LoadAllVolumes();

            versionNumberText.text = $"dreamings's adventure v{Application.version}";

            ReloadTimerSettings();
            SetLevelSelectButtons();
            Reload();
        }
    }

    public void ReloadTimerSettings()
    {
        SpeedrunTimer speedrunTimer = GameObject.Find("Speedrun Timer").GetComponent<SpeedrunTimer>();
        speedrunTimer.LoadSavedSettings();
        speedrunVertical.value = (int)speedrunTimer.verticalAlignment;
        speedrunVertical.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = speedrunVertical.value switch
        {
            0 => "Top",
            1 => "Centre",
            2 => "Bottom",
            _ => "err"
        };

        speedrunHorizontal.value = (int)speedrunTimer.horizontalAlignment;
        speedrunHorizontal.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = speedrunHorizontal.value switch
        {
            0 => "Left",
            1 => "Centre",
            2 => "Right",
            _ => "err"
        };

        speedrunOpacity.value = speedrunTimer.minimisedOpacity;
    }

    public void Reload()
    {
        resolutionHeight.text = Screen.height.ToString();
        hardModeToggle.isOn = PlayerPrefs.GetInt("HardModeNextGame", 0) != 0;
        speedrunToggle.isOn = PlayerPrefs.GetInt(PlayerPrefKeys.SpeedrunMode, 0) != 0;
        timerSettings.SetActive(speedrunToggle.isOn);

        bool enableExtraSettings = PlayerPrefs.GetInt("NumberOfVictories", 0) != 0;
        hardModeToggle.gameObject.SetActive(enableExtraSettings);
        speedrunToggle.gameObject.SetActive(enableExtraSettings);

        UpdateStatsText();
    }

    public string BoldIf(string s, bool predicate) => $"{(predicate ? "<b>" : "")}{s}{(predicate ? "</b>" : "")}";

    public void UpdateStatsText()
    {
        bool isOnHardMode = PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0;
        int totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        statsText.text = $"{BoldIf("Current Score", isOnHardMode)}: {totalScore:N0}\n";

        int totalVictories = PlayerPrefs.GetInt("NumberOfVictories", 0);
        statsText.text += $"Number of wins: {totalVictories:N0}\n";

        bool showHardMode = PlayerPrefs.GetInt(PlayerPrefKeys.HardModeNextGame, 0) != 0;
        int bestScore = PlayerPrefs.GetInt(
            showHardMode
                ? PlayerPrefKeys.BestScoreHard
                : PlayerPrefKeys.BestScoreNormal,
            0
        );
        statsText.text += $"{BoldIf("Best Score", showHardMode)}: {bestScore:N0}\n";
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        Reload();
        if (SceneManager.GetActiveScene().name == "TitleScreen")
        {
            foreach (GameObject obj in visibleOnTitle) obj.SetActive(true);
            foreach (GameObject obj in visibleOnMinigames) obj.SetActive(false);
            return;
        }

        foreach (GameObject obj in visibleOnTitle) obj.SetActive(false);
        foreach (GameObject obj in visibleOnMinigames) obj.SetActive(true);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menu.activeSelf)
            {
                RestoreCursorStatus();
            }
            else
            {
                SetLevelSelectButtons();
                previousLockState = Cursor.lockState;
                previousCursorVisibility = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            menu.SetActive(!menu.activeSelf);
        }
    }

    public void RestoreCursorStatus()
    {
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisibility;
    }

    public void GoToTitleScreen()
    {
        if (InputManager.Initialised) InputManager.Destroy();
        SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
    }

    public void SetLevelSelectButtons()
    {
        int highestSavedLevel = PlayerPrefs.GetInt(PlayerPrefKeys.HighestSavedLevel, 0);
        for (int i = 0; i < levelSelectButtons.Length; i++)
        {
            bool dontShowButton = !PlayerPrefs.HasKey(PlayerPrefKeys.HasHitNewGameOrContinue) || i > highestSavedLevel;
            levelSelectButtons[i].gameObject.SetActive(!dontShowButton);
            if (dontShowButton)
            {
                continue;
            }

            string sceneName = LevelOrder.GetLevelAtIndex(i);
            levelSelectButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = sceneName;
            levelSelectButtons[i].onClick.RemoveAllListeners();
            levelSelectButtons[i].onClick.AddListener(() =>
            {
                Debug.Log($"clicked {sceneName}");
                PlayerPrefs.SetInt("DontSaveProgress", 1);

                int onHardMode = PlayerPrefs.GetInt("HardMode", 0);
                PlayerPrefs.SetInt("WasOnHardMode", onHardMode);

                int enableHardMode = PlayerPrefs.GetInt("HardModeNextGame", 0);
                PlayerPrefs.SetInt("HardMode", enableHardMode);

                SceneManager.LoadScene(sceneName);
                menu.SetActive(false);
            });

            bool viewBestTimes = PlayerPrefs.GetInt(PlayerPrefKeys.SpeedrunMode, 0) != 0 || i == levelSelectButtons.Length - 1;
            bool viewHardModeScores = PlayerPrefs.GetInt(PlayerPrefKeys.HardModeNextGame, 0) != 0;

            string bestScoreOrTime = "placeholder";
            if (viewBestTimes)
            {
                float bestTime = PlayerPrefs.GetFloat($"BestTime_{sceneName}_{(viewHardModeScores ? "HardMode" : "NormalMode")}", float.PositiveInfinity);
                bestScoreOrTime = bestTime >= TimeSpan.MaxValue.TotalSeconds
                                        ? "None set!"
                                        : $"{(int)TimeSpan.FromSeconds(bestTime).TotalMinutes}:{TimeSpan.FromSeconds(bestTime):ss'.'fff}";
            }
            else
            {
                int bestScore = PlayerPrefs.GetInt($"HighScore_{sceneName}_{(viewHardModeScores ? "HardMode" : "NormalMode")}", 0);
                bestScoreOrTime = $"{bestScore:N0}";
            }

            levelSelectButtons[i].transform
                                    .Find("Text (TMP)")
                                    .Find("Text (TMP) (1)")
                                    .GetComponent<TextMeshProUGUI>()
                                    .text = $"{BoldIf(viewBestTimes ? "Best Time" : "Best Score", viewHardModeScores)}:\n{bestScoreOrTime}";
        }
    }

    public void ResetMinigame()
    {
        if (InputManager.Initialised) InputManager.Destroy();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        menu.SetActive(false);
    }

    public void UpdateJumpscareToggle(bool substituted)
    {
        PlayerPrefs.SetInt(PlayerPrefKeys.JumpscareSoundSubstituted, substituted ? 1 : 0);
    }

    public void UpdateHardModeToggle(bool enabled)
    {
        PlayerPrefs.SetInt(PlayerPrefKeys.HardModeNextGame, enabled ? 1 : 0);
        SetLevelSelectButtons();
        UpdateStatsText();
    }

    public void UpdateSpeedrunnerModeToggle(bool enabled)
    {
        PlayerPrefs.SetInt(PlayerPrefKeys.SpeedrunMode, enabled ? 1 : 0);
        timerSettings.SetActive(enabled);
        SetLevelSelectButtons();
    }
}