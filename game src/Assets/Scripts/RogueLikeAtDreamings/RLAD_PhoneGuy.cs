using UnityEngine;

namespace RogueLikeAtDreamings
{
    public class PhoneGuy : MonoBehaviour
    {
        public AudioClip[] phoneCalls;
        public AudioSource source;

        void Update()
        {
            if (gameObject.activeSelf && !source.isPlaying)
                gameObject.SetActive(false);
        }

        public void Call(int index)
        {
            if (index >= phoneCalls.Length) return;

            gameObject.SetActive(true);
            source.clip = phoneCalls[index];
            source.Play();
        }
    }
}