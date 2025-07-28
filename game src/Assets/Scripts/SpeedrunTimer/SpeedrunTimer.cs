using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Keys;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Speedrun
{
    public enum VerticalAlignment
    {
        Top,
        Centre,
        Bottom
    }

    public enum HorizontalAlignment
    {
        Left,
        Centre,
        Right
    }

    public class SpeedrunTimer : MonoBehaviour
    {
        private float currentTime = 0;
        private float splitTime = 0;
        private bool running = false;
        private int splitIndex = 0;
        public SplitObject[] splitObjects;
        private int splitCount;

        public TextMeshProUGUI totalTimeText;
        public TextMeshProUGUI fractionalTimeText;

        public TextMeshProUGUI totalSplitTimeText;
        public TextMeshProUGUI fractionalSplitTimeText;

        public TextMeshProUGUI currentMinigameText;
        public TextMeshProUGUI categoryText;
        public TextMeshProUGUI attemptsText;

        public VerticalAlignment verticalAlignment;
        public float[] verticalPositions;
        public HorizontalAlignment horizontalAlignment;
        public float[] horizontalPositions;
        public float minimisedOpacity = 1;
        private bool _minimised;
        public bool Minimised
        {
            get { return _minimised; }
            private set { _minimised = value; }
        }

        public GameObject timerChild;
        public GameObject topObject;
        public GameObject middleObject;
        public GameObject bottomObject;
        public Image timerBackground;
        public static SpeedrunTimer instance;

        // Start is called before the first frame update
        void Start()
        {
            if (instance) DestroyImmediate(gameObject);
            else
            {
                DontDestroyOnLoad(gameObject);
                currentTime = 0;
                _minimised = false;
                LoadSavedSettings();

                // Stop timer on scene change
                SceneManager.activeSceneChanged += (_, next) =>
                {
                    if (running && next.name == "TitleScreen")
                    {
                        FailedRun();
                    }
                    else if (running)
                    {
                        running = false;
                        ResetTimers();
                    }
                    SaveSystem.WriteSaveFile();
                };

                instance = this;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleMinimise();
            }
            if (!running) return;

            currentTime += Time.deltaTime;
            splitTime += Time.deltaTime;
            UpdateTimeText(currentTime, totalTimeText, fractionalTimeText);
            UpdateTimeText(splitTime, totalSplitTimeText, fractionalSplitTimeText);
        }

        private void ToggleMinimise()
        {
            _minimised = !_minimised;

            if (_minimised)
            {
                topObject.SetActive(false);
                middleObject.SetActive(false);
                bottomObject.transform.localPosition = new(0, verticalAlignment == VerticalAlignment.Top ? 92.5f : -92.5f, 0);
            }
            else
            {
                topObject.SetActive(true);
                middleObject.SetActive(true);
                bottomObject.transform.localPosition = new(0, -92.5f, 0);
            }

            Color bwah = timerBackground.color;
            bwah.a = _minimised ? minimisedOpacity : 1;
            timerBackground.color = bwah;
        }

        private void UpdateTimeText(float time, TextMeshProUGUI total, TextMeshProUGUI fractional)
        {
            TimeSpan currentSpan = TimeSpan.FromSeconds(time);

            if (currentSpan.TotalHours >= 1)
            {
                total.text = $"{(int)currentSpan.TotalHours}:{currentSpan:mm\\:ss}";
            }
            else if (currentSpan.TotalMinutes >= 1)
            {
                total.text = currentSpan.ToString(@"%m\:ss");
            }
            else
            {
                total.text = currentSpan.ToString(@"%s");
            }

            fractional.text = currentSpan.ToString(@"'.'fff");

            SplitObject currentObject = splitIndex == splitCount - 1
                                            ? splitObjects[^1]
                                            : splitObjects[splitIndex];
            Color timerColour = currentTime > currentObject.splitOnPB ? Color.red : Color.white;
            total.color = timerColour;
            fractional.color = timerColour;
        }

        public void Initialise(string category, params string[] splitNames)
        {
            ResetTimers();
            timerChild.SetActive(PlayerPrefs.GetInt(PlayerPrefKeys.SpeedrunMode, 0) != 0);
            SetPosition();

            splitCount = splitNames.Length;

            string currentScene = SceneManager.GetActiveScene().name;
            bool onHardMode = PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0;
            MinigameData minigameData = SaveSystem.GetMinigameData();

            // Add spaces before each capital letter in the scene name
            currentMinigameText.text = Regex.Replace(currentScene, "([a-z])([A-Z])", "$1 $2");

            categoryText.text = category;
            if (onHardMode) categoryText.text += " (Hard)";

            int numberOfAttempts, completedRuns;
            if (onHardMode)
            {
                numberOfAttempts = minigameData.hard.attempts;
                completedRuns = minigameData.hard.completedRuns;
            }
            else
            {
                numberOfAttempts = minigameData.normal.attempts;
                completedRuns = minigameData.normal.attempts;
            }
            
            attemptsText.text = $"{completedRuns:N0}/{numberOfAttempts:N0}";

            InitialiseSavedSplits(ref minigameData, splitNames.Length, onHardMode);

            SaveSystem.SetMinigameData(minigameData);

            foreach (SplitObject splitObject in splitObjects)
            {
                splitObject.Clear();
            }

            // The below line is so the last split is on the bottom line.
            splitObjects[^1].Initialise(currentScene, onHardMode, splitCount - 1, splitNames[^1]);
            for (int i = 0; i < splitCount - 1; i++)
            {
                splitObjects[i].Initialise(currentScene, onHardMode, i, splitNames[i]);
            }
        }

        private void InitialiseSavedSplits(ref MinigameData minigameData, int requiredLength, bool onHardMode)
        {
            // Initialise saved splits if set to blank array or incorrectly set.
            if (onHardMode && minigameData.hard.splits.Length != requiredLength)
            {
                minigameData.hard.attempts = 0;
                minigameData.hard.splits = Enumerable.Repeat(float.PositiveInfinity, requiredLength).ToArray();
                minigameData.hard.pbDeltas = Enumerable.Repeat(float.PositiveInfinity, requiredLength).ToArray();
                minigameData.hard.bestDeltas = Enumerable.Repeat(float.PositiveInfinity, requiredLength).ToArray();
            }
            else if (minigameData.normal.splits.Length != requiredLength)
            {
                minigameData.normal.attempts = 0;
                minigameData.normal.splits = Enumerable.Repeat(float.PositiveInfinity, requiredLength).ToArray();
                minigameData.normal.pbDeltas = Enumerable.Repeat(float.PositiveInfinity, requiredLength).ToArray();
                minigameData.normal.bestDeltas = Enumerable.Repeat(float.PositiveInfinity, requiredLength).ToArray();
            }
        }

        public void BeginTimer()
        {
            currentTime = 0;
            splitTime = 0;
            splitIndex = 0;
            running = true;
            totalTimeText.color = Color.white;
            fractionalTimeText.color = Color.white;

            MinigameData data = SaveSystem.GetMinigameData();
            int attempts, completedRuns;
            if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0)
            {
                attempts = ++data.hard.attempts;
                completedRuns = data.hard.completedRuns;
            }
            else
            {
                attempts = ++data.normal.attempts;
                completedRuns = data.normal.completedRuns;
            }
            SaveSystem.SetMinigameData(data);

            attemptsText.text = $"{completedRuns:N0}/{attempts:N0}";
        }

        public void FailedRun()
        {
            running = false;
            fractionalTimeText.color = Color.grey;
            totalTimeText.color = Color.grey;
            fractionalSplitTimeText.color = Color.grey;
            totalSplitTimeText.color = Color.grey;
        }

        public void ResetTimers()
        {
            running = false;
            currentTime = 0;
            splitTime = 0;
            UpdateTimeText(currentTime, totalTimeText, fractionalTimeText);
            UpdateTimeText(splitTime, totalSplitTimeText, fractionalSplitTimeText);
        }

        public void Split()
        {
            if (!running) return;
            splitTime = 0;

            float previousSplitTime = splitIndex == 0 ? 0 : splitObjects[splitIndex - 1].SplitThisRun;

            if (splitIndex == splitCount - 1)
            {
                splitObjects[^1].Split(currentTime, previousSplitTime);
            }
            else
            {
                splitObjects[splitIndex].Split(currentTime, previousSplitTime);
            }

            if (++splitIndex >= splitCount)
            {
                running = false;
                Color completedRun = new(0, 0.8f, 1);
                totalTimeText.color = completedRun;
                fractionalTimeText.color = completedRun;
                totalSplitTimeText.color = completedRun;
                fractionalSplitTimeText.color = completedRun;
                if (_minimised) ToggleMinimise();

                MinigameData data = SaveSystem.GetMinigameData();

                int completedRuns, numberOfAttempts;
                if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0)
                {
                    completedRuns = ++data.hard.completedRuns;
                    numberOfAttempts = data.hard.attempts;
                }
                else
                {
                    completedRuns = ++data.normal.completedRuns;
                    numberOfAttempts = data.normal.attempts;
                }
                attemptsText.text = $"{completedRuns:N0}/{numberOfAttempts:N0}";

                SaveSystem.SetMinigameData(data);

                if (LevelOrder.FinishSpeedrun(currentTime))
                    SaveSplits();
            }
        }

        public void SaveSplits()
        {
            List<float> splitsThisRun = new(16);
            List<float> deltasThisRun = new(16);
            float previousSplitTime = 0;

            for (int i = 0; i < splitCount; i++)
            {
                SplitObject splitObject = i == splitCount - 1
                                              ? splitObjects[^1]
                                              : splitObjects[i];

                splitsThisRun.Add(splitObject.SplitThisRun);
                deltasThisRun.Add(splitObject.SplitThisRun - previousSplitTime);
                previousSplitTime = splitObject.SplitThisRun;
            }

            SaveSystem.SaveSplits(splitsThisRun.ToArray(), deltasThisRun.ToArray());
        }

        // Settings

        public void ChangeVerticalAlignment(int index)
        {
            // 0 -> Top
            // 1 -> Centre
            // 2 -> Bottom
            PlayerPrefs.SetInt("Timer_vertical_alignment", index);
            verticalAlignment = index switch
            {
                0 => VerticalAlignment.Top,
                1 => VerticalAlignment.Centre,
                _ => VerticalAlignment.Bottom
            };
            SetPosition();
        }

        public void ChangeHorizontalAlignment(int index)
        {
            // 0 -> Left
            // 1 -> Centre
            // 2 -> Right
            PlayerPrefs.SetInt("Timer_horizontal_alignment", index);
            horizontalAlignment = index switch
            {
                0 => HorizontalAlignment.Left,
                1 => HorizontalAlignment.Centre,
                _ => HorizontalAlignment.Right
            };
            SetPosition();
        }

        public void ChangeMinimisedOpacity(float value)
        {
            PlayerPrefs.SetFloat("Timer_minimised_opacity", value);
            minimisedOpacity = value;
            if (_minimised)
            {
                Color bweh = timerBackground.color;
                bweh.a = minimisedOpacity;
                timerBackground.color = bweh;
            }
        }

        public void LoadSavedSettings()
        {
            verticalAlignment = PlayerPrefs.GetInt("Timer_vertical_alignment", 2) switch
            {
                0 => VerticalAlignment.Top,
                1 => VerticalAlignment.Centre,
                _ => VerticalAlignment.Bottom
            };
            horizontalAlignment = PlayerPrefs.GetInt("Timer_horizontal_alignment", 2) switch
            {
                0 => HorizontalAlignment.Left,
                1 => HorizontalAlignment.Centre,
                _ => HorizontalAlignment.Right
            };
            SetPosition();
            minimisedOpacity = PlayerPrefs.GetFloat("Timer_minimised_opacity", 0.75f);
        }

        private void SetPosition()
        {
            timerChild.transform.localPosition = new(horizontalPositions[(int)horizontalAlignment], verticalPositions[(int)verticalAlignment]);
            // Update timer position if its already minimised (e.g. changing from bottom/centre to top)
            if (Minimised)
            {
                ToggleMinimise();
                ToggleMinimise();
            }
        }
    }
}