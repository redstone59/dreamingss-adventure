using System.Collections;
using System.Collections.Generic;
using Keys;
using Speedrun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimonManager : MonoBehaviour
{
    public SimonScript[] simons;
    public List<int> sequence;
    public Clips videoClips;
    public int sequenceIndex = 0;
    public int stage = 0;
    public int[] stageScores;
    public int totalScore = 0;
    public int incorrectGuesses = 0;

    public GameObject scaryImage;
    public AudioSource scaryNoise;
    public AudioClip screamSound;
    public AudioClip substituteSound;

    public TextMeshProUGUI scaryText;

    public AchievementManager achievementManager;
    public SpeedrunTimer speedrunTimer;
    private bool loadNextLevel = false;

    public GameObject notFinishedObject;
    public float idleTime;

    // Start is called before the first frame update
    void Start()
    {
        stageScores = new int[simons.Length];
        speedrunTimer = GameObject.Find("Speedrun Timer").GetComponent<SpeedrunTimer>();
        if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0)
            speedrunTimer.Initialise("Maxout%", "Simon");
        else
            speedrunTimer.Initialise("Scream%", "Simon", "IT'S GOD DAMN GREEN!!", "ah!");

        speedrunTimer.BeginTimer();
        StartCoroutine(NextSimon());
        scaryImage.SetActive(false);
        scaryText.text = "";
        scaryText.gameObject.SetActive(false);
        notFinishedObject.SetActive(false);
        idleTime = 0;

        if (PlayerPrefs.GetInt("JumpscareSoundSubstituted", 0) != 0)
            scaryNoise.clip = substituteSound;
        else
            scaryNoise.clip = screamSound;

        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentSimon != null && currentSimon.playing)
            idleTime = 0;
        else
            idleTime += Time.deltaTime;
        notFinishedObject.SetActive(idleTime >= 15);
    }

    void ShowSimonStage()
    {
        foreach (SimonScript simon in simons)
            simon.gameObject.SetActive(false);

        currentSimon.gameObject.SetActive(true);
    }

    public void AddToSimonSequence()
    {
        int sequenceMinimum = sequence.Count == 0 ? 1 : 0; // Prevent ITS GODDAMN GREEEEN on first roll
        sequence.Add(Random.Range(sequenceMinimum, 4));
        currentSimon.StartRound(sequence.ToArray());
    }

    public void HitButton(int number)
    {
        idleTime = 0;
        if (sequence[sequenceIndex] == number)
        {
            sequenceIndex++;
            if (sequenceIndex == sequence.Count)
            {
                if (++stageScores[stage] == 30)
                    achievementManager.UnlockAchievement(AllAchievements.YourePrettyGoodAtThis);
                else if (stageScores[stage] == 50)
                    achievementManager.UnlockAchievement(AllAchievements.SomeoneDidntGetTheMemo);
                else if (stageScores[stage] == 100)
                    achievementManager.UnlockAchievement(AllAchievements.Jesus);

                totalScore++;
                if (totalScore >= 100)
                {
                    stage = 100;
                    StartCoroutine(NextSimon());
                }
                else
                {
                    sequenceIndex = 0;
                    incorrectGuesses = 0;
                    currentSimon.Celebrate();
                    AddToSimonSequence();
                }
            }
            else
                currentSimon.Flash(number);
        }
        else
        {
            currentSimon.IncorrectHit();
            sequenceIndex = 0;
            incorrectGuesses++;
            if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode) != 0 || incorrectGuesses >= 3)
            {
                stage++;
                StartCoroutine(NextSimon());
            }
            else
                currentSimon.StartRound(sequence.ToArray());
        }
    }

    public IEnumerator NextSimon()
    {
        if ((stage != 0 && PlayerPrefs.GetInt(PlayerPrefKeys.HardMode) != 0) || stage >= simons.Length)
        {
            yield return new WaitForSeconds(0.1f);
            if (PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) == 0 || totalScore >= 100)
                speedrunTimer.Split();
            StartCoroutine(ScaryExclamationMark());
            yield break;
        }

        if (stage != 0) speedrunTimer.Split();
        currentSimon.playing = true;
        videoClips.PlayNextClip();
        yield return new WaitForSeconds(1f);
        while (videoClips.player.isPlaying) yield return null;
        stageScores[stage] = 0;
        sequenceIndex = 0;
        incorrectGuesses = 0;
        sequence.Clear();
        ShowSimonStage();
        yield return new WaitForSeconds(0.5f);
        currentSimon.playing = false;
        AddToSimonSequence();
    }

    public IEnumerator ScaryExclamationMark()
    {
        scaryNoise.Play();
        scaryImage.SetActive(true);
        StartCoroutine(PreloadDreamingOverIt());
        yield return new WaitForSeconds(3f);
        
        if (achievementManager != null)
            achievementManager.UnlockAchievement(AllAchievements.Boo);

        int i = 1;
        scaryText.gameObject.SetActive(true);
        foreach (int score in stageScores)
        {
            scaryText.text += string.Format("Simon {0}: ", i++);
            yield return new WaitForSeconds(2f);
            scaryText.text += score.ToString();
            scaryText.text += "\n";
            yield return new WaitForSeconds(2f);
        }
        scaryText.text += "\nTotal Score: ";
        yield return new WaitForSeconds(3f);
        scaryText.text += Mathf.Min(100, totalScore).ToString();

        if (achievementManager != null)
        {
            if (totalScore == 0) achievementManager.UnlockAchievement(AllAchievements.Dementia);
            if (totalScore >= 1) achievementManager.UnlockAchievement(AllAchievements.YouMadeAnEffort);
            if (totalScore >= 10) achievementManager.UnlockAchievement(AllAchievements.YouCanRemember);
            if (totalScore >= 30) achievementManager.UnlockAchievement(AllAchievements.IUhDontThinkYouRealiseHowLittleYouScore);
            if (totalScore >= 100) achievementManager.UnlockAchievement(AllAchievements.TheWorldsMostInsubstantialMaxout);
        }

        LevelOrder.AddToSavedScore(totalScore);
        LevelOrder.IncrementSavedLevel();
        
        yield return new WaitForSeconds(5f);
        while (achievementManager.AchievementsInQueue) yield return null;
        loadNextLevel = true;
    }

    public SimonScript currentSimon { get { return stage >= simons.Length ? null : simons[stage]; } }

    public void OnApplicationFocus(bool focus)
    {
        if (!focus) achievementManager.UnlockAchievement(AllAchievements.Notetaker);
    }

    private IEnumerator PreloadDreamingOverIt()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(LevelOrder.GetNextLevel("SimonScream"));
        asyncLoad.allowSceneActivation = false;

        while (!loadNextLevel) yield return null;

        asyncLoad.allowSceneActivation = true;
    }
}
