using UnityEngine;
using UnityEngine.UI;

namespace RogueLikeAtDreamings.IntroScreen
{
    public class IntroScreen : MonoBehaviour
    {
        public Toggle jumpscareToggle;
        public GameObject warningText;
        public Button startGame;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            if (SaveSystem.IsDemo())
            {
                startGame.onClick.Invoke();
                return;
            }
            jumpscareToggle.isOn = PlayerPrefs.GetInt("JumpscareSoundSubstituted", 0) != 0;
            if (SaveSystem.GameData.highestSavedLevel > LevelOrder.GetLevelIndex("RogueLikeAtDreamings"))
                warningText.SetActive(true);
        }
        
        void Update() {}

        public void ToggleJumpscares(bool substituted)
        {
            PlayerPrefs.SetInt("JumpscareSoundSubstituted", substituted ? 1 : 0);
        }
    }
}