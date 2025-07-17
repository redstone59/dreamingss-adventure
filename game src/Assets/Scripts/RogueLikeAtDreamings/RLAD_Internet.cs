using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace RogueLikeAtDreamings.Internet
{
    public class Internet : MonoBehaviour
    {
        private float _timeCap;
        private float _timeUsed;
        public float DataMultiplier;
        public float DataJiggle;
        public float DailyGigabytes = 15;
        public float WarningThreshold;

        public Action LowDataCallback;
        private bool _hasSentWarning;
        public GameObject[] warningSignsObjects;

        public TextMeshProUGUI UsageText;

        void Update()
        {
            if (_timeCap == 0 || _timeUsed >= _timeCap) return;
            float jiggleThisFrame = 1 + UnityEngine.Random.Range(-DataJiggle, DataJiggle);
            _timeUsed += Time.deltaTime * DataMultiplier * jiggleThisFrame;
            _timeUsed = Mathf.Min(_timeUsed, _timeCap);
            UsageText.text = $"{PercentageUsedToString()}/{DailyGigabytes:G29}GB.";
            if (!_hasSentWarning && AtLowData())
            {
                LowDataCallback?.Invoke();
                StartCoroutine(FlashWarning());
                _hasSentWarning = true;
            }
        }

        public string PercentageUsedToString()
        {
            float usage = _timeUsed / _timeCap;
            float bytesUsed = Mathf.Round(DailyGigabytes * 1e9f * usage);

            return ConvertBytesToString(bytesUsed);
        }

        public string ConvertBytesToString(float bytesUsed)
        {
            int exponent = Mathf.FloorToInt(Mathf.Log10(bytesUsed));

            string[] siPrefixes = { "", "K", "M", "G", "T", "P", "E", "Z", "Y", "R", "Q" };
            int index = exponent / 3; // This floors because of how int.operator/ works.

            return $"{bytesUsed / Mathf.Pow(10f, index * 3):N2}{siPrefixes[index]}B";
        }

        public void Initialise(float initialCap)
        {
            _timeCap = initialCap;
            _timeUsed = 0;
            foreach (GameObject warningObject in warningSignsObjects)
                warningObject.SetActive(false);
        }

        public void AddData(float gigs)
        {
            float timePerGigabyte = _timeCap / DailyGigabytes;
            _timeCap += gigs * timePerGigabyte;
            DailyGigabytes += gigs;
            _hasSentWarning = false;
            foreach (GameObject warningObject in warningSignsObjects)
                warningObject.SetActive(OutOfData());
        }

        public bool AtLowData()
        {
            return (DailyGigabytes * _timeUsed / _timeCap) >= DailyGigabytes - WarningThreshold;
        }

        public bool OutOfData()
        {
            return _timeUsed >= _timeCap;
        }

        private IEnumerator FlashWarning()
        {
            float timeElapsed = 0;
            bool show = true;
            if (_hasSentWarning) yield break;
            while (AtLowData() && !OutOfData())
            {
                if (timeElapsed < 0.5f)
                {
                    timeElapsed += Time.deltaTime;
                    yield return null;
                    continue;
                }
                foreach (GameObject warningObject in warningSignsObjects)
                {
                    Debug.Log($"Setting {warningObject.name} to {(show ? "active" : "inactive")}");
                    warningObject.SetActive(show);
                }
                show = !show;
                timeElapsed = 0;
                yield return null;
            }
            foreach (GameObject warningObject in warningSignsObjects)
                warningObject.SetActive(OutOfData());
        }
    }
}