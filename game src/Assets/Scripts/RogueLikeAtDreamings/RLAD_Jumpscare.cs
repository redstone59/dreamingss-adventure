using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RogueLikeAtDreamings.Jumpscare
{
    public class Jumpscare : MonoBehaviour
    {
        public AudioSource source;
        public AudioClip jumpscareNoise;
        public AudioClip substituteNoise;
        public Image image;

        public Sprite dreamingSprite;
        public Sprite gasterSprite;

        public float animationLength;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            source = GetComponent<AudioSource>();
            source.clip = PlayerPrefs.GetInt("JumpscareSoundSubstituted", 0) != 0 ? substituteNoise : jumpscareNoise;
            StartCoroutine(JumpscareAnimation());
        }

        private IEnumerator JumpscareAnimation()
        {
            source.Play();
            float timeElapsed = 0;
            do
            {
                float percentDone = timeElapsed / animationLength;
                float t = 1 - Mathf.Pow(1 - percentDone, 3); // Cubic ease
                transform.localScale = Mathf.Lerp(0, 8, t) * Vector3.one;
                yield return new WaitForEndOfFrame();
                timeElapsed += Time.deltaTime;
            }
            while (timeElapsed < jumpscareNoise.length);
            while (source.isPlaying) yield return null;
        }
    }
}