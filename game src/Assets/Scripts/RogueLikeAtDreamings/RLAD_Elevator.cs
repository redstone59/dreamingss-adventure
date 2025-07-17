using UnityEngine;

namespace RogueLikeAtDreamings.Elevator
{
    public enum Floor
    {
        Basement,
        Ground,
        Upper
    }

    public class Elevator : MonoBehaviour
    {
        private Floor _floor;
        public AudioSource chimes;
        public AudioClip upChime;
        public AudioClip downChime;
        public bool muted;

        public float IdleTime;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            IdleTime += Time.deltaTime;
        }

        public Floor floor
        {
            get { return _floor; }
            set
            {
                if (IdleTime < 0) return;
                if (_floor != value && !muted)
                {
                    IdleTime = 0;
                    chimes.Stop();

                    chimes.clip = _floor == Floor.Basement ?   upChime :
                                  _floor == Floor.Upper    ? downChime :
                                   value == Floor.Basement ? downChime : upChime;
                    
                    chimes.volume = _floor == Floor.Ground   ? 1f   : 
                                    _floor == Floor.Basement ? 1.2f : 0.8f;

                    chimes.Play();
                }
                _floor = value;
            }
        }
    }
}