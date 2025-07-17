using System.Collections;
using System.Collections.Generic;
using Keys;
using UnityEngine;

public class ContestantManager : MonoBehaviour
{
    public Contestant[] contestants;
    public Contestant superUltraMegaRareSecondDreaming;
    private Contestant chosenContestant;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Contestant contestant in contestants) contestant.contestantImage.SetActive(false);
        if (Random.Range(0, 100) == 0 || ForceRareDreaming())
        {
            chosenContestant = superUltraMegaRareSecondDreaming;
            PlayerPrefs.SetInt(PlayerPrefKeys.SayThatAnswer.HasSeenSuperUltraMegaRareSecondDreaming, 1);
        }
        else chosenContestant = contestants[Random.Range(0, contestants.Length)];
        chosenContestant.contestantImage.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool ForceRareDreaming()
    {
        if (PlayerPrefs.GetInt(PlayerPrefKeys.SayThatAnswer.HasSeenSuperUltraMegaRareSecondDreaming, 0) != 0) return false;

        int timesPlayed = PlayerPrefs.GetInt(PlayerPrefKeys.SayThatAnswer.GamesNoEgg, 0) + 1;
        PlayerPrefs.SetInt(PlayerPrefKeys.SayThatAnswer.GamesNoEgg, timesPlayed);

        return timesPlayed >= 4;
    }

    public void SetActive(bool isActive)
    {
        chosenContestant.contestantImage.SetActive(true);
    }

    public float BuzzTime()
    {
        return chosenContestant.agility + Random.Range(-chosenContestant.variation, chosenContestant.variation);
    }

    public bool IsBuzzingEarly()
    {
        return Random.Range(0f, 1f) <= chosenContestant.chanceEarlyGuess;
    }

    public float PercentageQuestionHeard()
    {
        return chosenContestant.earlyAgility * (1 + Random.Range(-chosenContestant.earlyVariance, chosenContestant.earlyVariance));
    }

    public bool IsCorrect()
    {
        return Random.Range(0f, 1f) <= chosenContestant.chanceCorrectAnswer;
    }

    new public string name { get { return chosenContestant.name; } }
    public float agility { get { return chosenContestant.agility; } }
    public float variation { get { return chosenContestant.variation; } }
    public float chanceCorrectAnswer { get { return chosenContestant.chanceCorrectAnswer; } }
    public float garbledness { get { return chosenContestant.garbledness; } }
    public List<AudioClip> announcerLines { get { return chosenContestant.announcerLines; } }
    public GameObject contestantImage { get { return chosenContestant.contestantImage; } }
}
