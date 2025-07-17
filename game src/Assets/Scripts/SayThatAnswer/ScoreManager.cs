using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI    scoreText;
    public int                minigameScore          = 0;

    public const float        SPEED_BONUS_TIME       = 10f;
    public const float        SPEED_BONUS_MULTIPLIER = 1000f;
    public const int          ANSWER_REWARD          = 10000;
    public const float        REBUTT_PENALTY         = 0.75f;
    public const int          EARLY_ANSWER_BONUS     = 5000;

    void Start()
    {
        scoreText.text = "";
    }

    void Update()
    {
    }

    public void ScoreCorrectAnswer(bool wasEarly, float timeTakenToAnswer)
    {
        if (wasEarly)
        {
            minigameScore += EARLY_ANSWER_BONUS;
        }
        
        if (timeTakenToAnswer < SPEED_BONUS_TIME)
        {
            float difference = SPEED_BONUS_TIME - timeTakenToAnswer;
            minigameScore += (int)(difference * SPEED_BONUS_MULTIPLIER);
        }

        minigameScore += ANSWER_REWARD;
        Debug.Log(minigameScore);
    }

    public void Penalise()
    {
        minigameScore -= (int)(ANSWER_REWARD * REBUTT_PENALTY);
        minigameScore = Mathf.Max(0, minigameScore);
        Debug.Log(minigameScore);
    }

    public void AddToSavedScore()
    {
        LevelOrder.AddToSavedScore(minigameScore);
        Debug.Log(minigameScore);
    }

    public IEnumerator VictoryScoreAnimation(bool disappear)
    {
        yield return new WaitForSeconds(3.2f);
        int incrementPerFrame = (int)(minigameScore / 2 * Time.fixedDeltaTime);
        if (incrementPerFrame == 0) incrementPerFrame++;

        int displayedScore = 0;
        while (displayedScore < minigameScore)
        {
            displayedScore += incrementPerFrame;
            scoreText.text = displayedScore.ToString("N0");
            yield return new WaitForFixedUpdate();
        }
        scoreText.text = minigameScore.ToString("N0");
        yield return new WaitForSeconds(1.25f);
        if (disappear) scoreText.text = "";
    }
}
