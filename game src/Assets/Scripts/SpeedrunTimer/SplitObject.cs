using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Keys;

namespace Speedrun
{
    public class SplitObject : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public TextMeshProUGUI deltaText;
        public TextMeshProUGUI splitTimeThisRun;
        public Image background;

        public float splitOnPB = float.PositiveInfinity;
        public float pbDelta = float.PositiveInfinity;
        public float bestDelta = float.PositiveInfinity;

        private float _splitThisRun = float.PositiveInfinity;
        public float SplitThisRun
        {
            get { return _splitThisRun; }
            private set { _splitThisRun = value; }
        }

        private int _index;

        public void Clear()
        {
            label.text = "";
            deltaText.text = "";
            splitTimeThisRun.text = "";
        }

        public void Initialise(string sceneName, bool onHardMode, int index, string splitName)
        {
            splitOnPB = SaveSystem.GetPBSplit(index, onHardMode);
            pbDelta = SaveSystem.GetPBDelta(index, onHardMode);
            bestDelta = SaveSystem.GetBestDelta(index, onHardMode);

            label.text = splitName;
            deltaText.text = "";
            DisplaySplitTime(splitOnPB);

            _index = index;
        }

        private void DisplaySplitTime(float time)
        {
            if (float.IsPositiveInfinity(time))
            {
                splitTimeThisRun.text = "";
                return;
            }

            TimeSpan span = TimeSpan.FromSeconds(time);

            if (span.TotalHours >= 1)
                splitTimeThisRun.text = $"{(int)span.TotalHours}:{span:mm\\:ss\\.fff}";
            else if (span.TotalMinutes >= 1)
                splitTimeThisRun.text = span.ToString(@"%m\:ss\.fff");
            else
                splitTimeThisRun.text = span.ToString(@"%s\.fff");
        }

        public void Split(float time, float previousSplitTime)
        {
            DisplaySplitTime(time);
            _splitThisRun = time;

            float splitLength = time - previousSplitTime;
            bool goldSplit = splitLength < bestDelta;
            Debug.Log($"Gold split? {splitLength} < {bestDelta} => {goldSplit}");

            if (!float.IsPositiveInfinity(splitOnPB))
            {
                bool improvedDelta = splitLength < pbDelta;
                bool improvedTime = time < splitOnPB;
                Debug.Log($"Improved delta? {splitLength} < {bestDelta} => {goldSplit}");
                Debug.Log($"Improved time? {splitLength} < {bestDelta} => {goldSplit}");

                float delta = time - splitOnPB;

                string sign = delta >= 0 ? "+" : "-";
                TimeSpan span = TimeSpan.FromSeconds(Mathf.Abs(delta));

                if (span.TotalMinutes >= 1)
                    deltaText.text = $"{sign}{(int)span.TotalMinutes}:{span:ss}";
                else
                    deltaText.text = $"{sign}{span:%s\\.ff}";

                if (goldSplit) deltaText.text = $"<u>{deltaText.text}</u>";

                deltaText.color = !float.IsPositiveInfinity(bestDelta) && goldSplit
                                      ? Color.yellow
                                      : improvedTime
                                            ? Color.green
                                            : Color.red;

                // If it's an improved time but slower split or a worse time with a better split, differentiate visually.
                if (!goldSplit && (improvedDelta ^ improvedTime))
                    deltaText.color = Color.Lerp(deltaText.color, Color.white, 0.4f);
            }

            if (goldSplit)
            {
                MinigameData data = SaveSystem.GetMinigameData();
                if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0)
                {
                    data.hard.bestDeltas[_index] = splitLength;
                }
                else
                {
                    data.normal.bestDeltas[_index] = splitLength;
                }
                SaveSystem.SetMinigameData(data);
            }
        }
    }
}