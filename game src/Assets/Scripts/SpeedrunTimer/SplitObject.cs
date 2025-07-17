using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace Speedrun
{
    public class SplitObject : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public TextMeshProUGUI deltaText;
        public TextMeshProUGUI splitTimeThisRun;
        public Image background;

        public float splitOnPB = float.PositiveInfinity;
        public float bestSplit = float.PositiveInfinity;

        private float _splitThisRun = float.PositiveInfinity;
        public float SplitThisRun
        {
            get { return _splitThisRun; }
            private set { _splitThisRun = value; }
        }

        public void Clear()
        {
            label.text = "";
            deltaText.text = "";
            splitTimeThisRun.text = "";
        }

        public string GetSplitKey(string sceneName, bool onHardMode, int index)
        {
            return $"Split_{sceneName}_{(onHardMode ? "Hard" : "Normal")}_{index}";
        }

        public void Initialise(string sceneName, bool onHardMode, int index, string splitName)
        {
            string mainKey = GetSplitKey(sceneName, onHardMode, index);

            splitOnPB = PlayerPrefs.GetFloat($"{mainKey}_PB", float.PositiveInfinity);
            bestSplit = PlayerPrefs.GetFloat($"{mainKey}_BestSplit", float.PositiveInfinity);

            label.text = splitName;
            deltaText.text = "";
            DisplaySplitTime(splitOnPB);
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

        public void Split(float time)
        {
            DisplaySplitTime(time);
            _splitThisRun = time;

            if (!float.IsPositiveInfinity(splitOnPB))
            {
                float delta = splitOnPB - time;

                string sign = delta >= 0 ? "-" : "+";
                TimeSpan span = TimeSpan.FromSeconds(Mathf.Abs(delta));

                if (span.TotalMinutes >= 1)
                    deltaText.text = $"{sign}{(int)span.TotalMinutes}:{span:ss}";
                else
                    deltaText.text = $"{sign}{span:%s\\.ff}";

                deltaText.color = false && time < bestSplit
                                      ? new Color(0.93f, 0.75f, 0.01f)
                                      : delta < 0
                                            ? Color.red
                                            : Color.green;
            }
        }
    }
}