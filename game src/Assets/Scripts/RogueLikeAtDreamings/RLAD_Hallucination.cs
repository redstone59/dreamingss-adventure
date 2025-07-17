using System.Collections;
using UnityEngine;

namespace RogueLikeAtDreamings.Hallucinations
{
    public class Hallucination : MonoBehaviour
    {
        public GameObject[] hallucinationObjects;
        public bool currentlyActive;
        public AudioSource garbledness;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            currentlyActive = false;
            foreach (GameObject gameObject in hallucinationObjects)
                gameObject.SetActive(false);
        }

        public void Attempt(int night)
        {
            if (currentlyActive) return;
            float probability = 0.5f * Mathf.Pow(1.5f, Mathf.Pow(1.05f, night));
            probability = Mathf.Min(probability, 1);
            if (Random.Range(0, 1f) < probability)
            {
                float length = 0.3f * (1 + night / 30f);
                length = Mathf.Min(length, 3);
                float intensity = 0.2f * (2 - Mathf.Pow(1.05f, Mathf.Pow(1.08f, night)));
                StartCoroutine(Hallucinate(length, intensity));
            }
        }

        public IEnumerator Hallucinate(float length, float intensity)
        {
            currentlyActive = true;
            garbledness.Play();
            int i = 0;
            for (float timeElapsed = 0; timeElapsed <= length;)
            {
                float randomMultiplier = Random.Range(0.8f, 1.2f);
                float thisLength = randomMultiplier * intensity;
                
                foreach (GameObject gameObject in hallucinationObjects)
                    gameObject.SetActive(false);
                
                if (timeElapsed == 0 || Random.Range(0, 1f) <= 0.5f)
                {
                    GameObject hallucinationObject = hallucinationObjects[Random.Range(0, hallucinationObjects.Length)];
                    hallucinationObject.SetActive(true);
                }
                
                yield return new WaitForSeconds(thisLength);
                timeElapsed += thisLength;
                if (++i >= 100) break;
            }
            currentlyActive = false;
            garbledness.Pause();
            foreach (GameObject gameObject in hallucinationObjects)
                gameObject.SetActive(false);

            yield return null;
        }
    }
}