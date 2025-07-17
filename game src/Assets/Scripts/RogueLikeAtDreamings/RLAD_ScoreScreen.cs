using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RogueLikeAtDreamings.ScoreScreen
{
    public class ScoreScreen : MonoBehaviour
    {
        public TextMeshProUGUI labels;
        public TextMeshProUGUI numbers;
        public Image background;
        public AchievementManager achievementManager;
        
        private int GetScore(RLADPlayer player)
        {
            int flatScore = player.nightsCompleted * 10000;
            int frugalScore = 50 * Mathf.Max(0, player.money - player.debt);
            // A player that beats night 5 is guaranteed a score of 50,000.
            float challengeMultiplier = 1;
            for (int i = 0; i < player.challengesCompleted; i++)
            {
                // Diminishing returns for every 4 challenges completed (4 challenges -> 2x, 8 challenges -> 2.5x, 12 -> 2.83x, 16 -> 3.08x)
                challengeMultiplier += 1f / (4 * (1 + i / 4));
            }

            return Mathf.FloorToInt((flatScore + frugalScore) * challengeMultiplier);
        }

        private string GetLabel(int lines)
        {
            string label = "";

            label += lines >= 0 ? "Nights completed:\n" : "\n";
            label += lines >= 1 ? "Money saved:\n" : "\n";
            label += lines >= 2 ? "Challenges completed:\n" : "\n";
            label += lines >= 3 ? "Money owed:\n\n" : "\n\n";
            label += lines >= 4 ? "Total score:" : " ";

            return label;
        }
        
        public string GetValues(int lines, ITuple values)
        {
            string label = "";

            label += lines >= 0 ? $"{values[0]:N0}\n" : "\n";
            label += lines >= 1 ? $"${values[1]:N0}\n" : "\n";
            label += lines >= 2 ? $"{values[2]:N0}\n" : "\n";
            label += lines >= 3 ? $"${values[3]}\n\n" : "\n\n";
            label += lines >= 4 ? $"{values[4]:N0}" : " ";

            return label;
        }

        public IEnumerator ScoreAnimation(RLADPlayer player)
        {
            background.color = Color.red;
            for (float timeElapsed = 0; timeElapsed < 3; timeElapsed += Time.deltaTime)
            {
                background.color = Color.Lerp(Color.red, Color.black, timeElapsed / 3);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(1);

            ITuple values = (player.nightsCompleted, player.money, player.challengesCompleted, player.debt, GetScore(player));
            for (int i = 0; i < 5; i++)
            {
                labels.text = GetLabel(i);
                numbers.text = GetValues(i, values);

                if (i == 4 && achievementManager != null && player.nightsCompleted == 0)
                    achievementManager.UnlockAchievement(AllAchievements.IsNeverUsuallyThatBadInAnyOfTheGames);
                
                yield return new WaitForSeconds(3);
            }

            string nextScene = LevelOrder.GetNextLevel(SceneManager.GetActiveScene().name);
            LevelOrder.AddToSavedScore(GetScore(player));
            LevelOrder.IncrementSavedLevel();
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }

        void Start()
        {
            labels.text = "";
            numbers.text = "";
            achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
        }
    }
}