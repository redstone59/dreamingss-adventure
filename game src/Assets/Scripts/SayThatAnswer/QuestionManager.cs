using System.Collections;
using System.Collections.Generic;
using Speedrun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestionManager : MonoBehaviour
{
    public AchievementManager achievementManager;
    public List<Question> questions;
    public List<Question> hardModeQuestions;
    public TextManager textManager;
    public AnswerManager answerManager;
    public ContestantManager contestant;
    public ScoreManager scoreManager;

    public AudioManager audioSource;
    public AudioClip buzzerNoise;
    public SpriteRenderer playerBackground;
    public SpriteRenderer opponentBackground;
    public Color buzzedColour;
    public GameObject victoryVideo;
    public GameObject losingBackground;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI opponentScoreText;

    public int playerScore;
    public int opponentScore;
    public const int scoreToWin = 6;
    public float timeToAnswer = 30f;

    public GameObject fcbText;
    public GameObject buzzInText;

    // Game variables

    private Question currentQuestion;
    private List<int> chosenQuestions;
    private bool backgroundMusicChange = true;
    private bool buzzed = false;
    private bool playerBuzz = false;
    private bool gotWrongOnce = false;
    private bool typing = false;
    private bool tookTooLong = false;

    // Used for minigame scoring

    private bool playerEarlyBuzz = false;
    private float playerAnswerTime = 0f;

    // Achievement variables

    private bool noWrongAnswers = true;
    private bool allEarlyBuzzes = true;

    private int questionNumber;

    public SpeedrunTimer speedrunTimer;

    // Start is called before the first frame update
    void Start()
    {
        chosenQuestions = new();
        playerScore = 0;
        opponentScore = 0;
        fcbText.SetActive(false);
        buzzInText.SetActive(false);
        questionNumber = 0;
        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
        speedrunTimer = GameObject.Find("Speedrun Timer").GetComponent<SpeedrunTimer>();
        speedrunTimer.Initialise("Beat the Game", "1 Point", "2 Points", "3 Points", "4 Points", "5 Points", "\"Say\"ing That Answer!");
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ContextMenu("Play Random Question")]
    public void PlayRandomQuestion()
    {
        bool onNormalMode = PlayerPrefs.GetInt("HardMode", 0) == 0;
        bool noScore = playerScore == 0 && opponentScore == 0;
        fcbText.SetActive(onNormalMode && noScore);

        if (questionNumber == 0) speedrunTimer.BeginTimer();

        bool gotUltraRareDreaming = contestant.name == contestant.superUltraMegaRareSecondDreaming.name;
        if (achievementManager != null && gotUltraRareDreaming)
            achievementManager.UnlockAchievement(AllAchievements.HuhQuestionMark);

        if (chosenQuestions.Count == questions.Count) chosenQuestions = new();

        if (playerScore >= scoreToWin)
        {
            Debug.Log("wahoo");
            return;
        }
        else if (opponentScore >= scoreToWin)
        {
            if (achievementManager != null && playerScore == scoreToWin - 1)
                achievementManager.UnlockAchievement(AllAchievements.SoClose);

            StartCoroutine(NotVictoryQuiteTheOppositeActually());
            return;
        }

        buzzed = false;
        playerBuzz = false;
        gotWrongOnce = false;
        tookTooLong = false;
        playerBackground.color = Color.black;
        opponentBackground.color = Color.black;
        playerAnswerTime = 0f;
        playerEarlyBuzz = false;
        if (backgroundMusicChange) ChangeBackgroundMusic();

        List<Question> currentQuestions = PlayerPrefs.GetInt("HardMode", 0) == 0 ? questions : hardModeQuestions;

        int randomIndex = -1;
        while (chosenQuestions.Contains(randomIndex) || randomIndex == -1) randomIndex = Random.Range(0, currentQuestions.Count);
        chosenQuestions.Add(randomIndex);

        currentQuestion = currentQuestions[randomIndex];
        questionNumber++;
        answerManager.validAnswers = currentQuestion.validAnswers.ToArray();
        answerManager.invalidAnswers = currentQuestion.invalidAnswers.ToArray();
        textManager.ShowText(currentQuestion.text, currentQuestion.audio.length);
        audioSource.clip = currentQuestion.audio;
        audioSource.Play();
        StartCoroutine(WaitForSpace());
        StartCoroutine(WaitForContestantBuzz());
        if (noScore) StartCoroutine(WaitForQuestionEnd());
    }

    public IEnumerator WaitForQuestionEnd()
    {
        while (audioSource.isPlaying) yield return null;
        if (buzzed) yield break;
        buzzInText.SetActive(true);
    }

    public IEnumerator WaitForContestantBuzz()
    {
        bool dontBuzzEarly = PlayerPrefs.GetInt("HardMode", 0) == 0 && playerScore == 0 && opponentScore == 0;
        bool isEarlyGuess = !dontBuzzEarly && contestant.IsBuzzingEarly();
        float questionLength = currentQuestion.audio.length;
        int buzzedOnQuestion = questionNumber;

        if (isEarlyGuess)
        {
            yield return new WaitForSeconds(contestant.PercentageQuestionHeard() * questionLength);
            if (buzzed || buzzedOnQuestion != questionNumber) yield break;

            StartCoroutine(ContestantBuzzer());
            yield break;
        }

        yield return new WaitForSeconds(questionLength + contestant.BuzzTime());
        if (buzzed || buzzedOnQuestion != questionNumber) yield break;
        buzzInText.SetActive(false);
        StartCoroutine(ContestantBuzzer());
    }

    public void ChangeBackgroundMusic()
    {
        switch (playerScore)
        {
            case 0:
            case 1:
            case 2:
                audioSource.PlayBackgroundMusic(0);
                break;
            case 3:
            case 4:
                audioSource.PlayBackgroundMusic(1);
                break;
            case 5:
                audioSource.PlayBackgroundMusic(2);
                break;
        }
        backgroundMusicChange = false;
    }

    private IEnumerator OtherContestantAnswer()
    {
        yield return null;
        gotWrongOnce = true;
        playerBackground.color = Color.black;
        opponentBackground.color = Color.black;
        textManager.questionText.text = currentQuestion.text.Replace('\u2018', '\'')
                                                            .Replace('\u2019', '\'')
                                                            .Replace('\u201c', '\"')
                                                            .Replace('\u201d', '\"');

        if (playerBuzz) opponentBackground.color = buzzedColour;
        else playerBackground.color = buzzedColour;

        if (!answerManager.isEnabled)
        {
            answerManager.answerTextBox.gameObject.SetActive(true);
            answerManager.EnableInputBox(!playerBuzz);
        }

        if (playerBuzz)
        {
            bool isCorrect = contestant.IsCorrect();
            StartCoroutine(OpponentTypingAnimation(isCorrect));
            yield return new WaitForSeconds(0.1f);
            while (typing) yield return null;

            answerManager.DisableInputBox();

            if (isCorrect)
            {
                StartCoroutine(CorrectVoicelines());
                if (playerBuzz) opponentScore++;
                else playerScore++;

                playerScoreText.text = playerScore.ToString();
                opponentScoreText.text = opponentScore.ToString();
                scoreManager.Penalise();
            }
            else
            {
                StartCoroutine(IncorrectVoicelines());
            }
        }
        else
        {
            playerBuzz = true;
            playerEarlyBuzz = false;
            playerAnswerTime = 9999f;
        }
    }

    private IEnumerator WaitForSpace()
    {
        while (!buzzed && !Input.GetKey(KeyCode.Space)) yield return null;
        if (buzzed) yield break;
        buzzInText.SetActive(false);
        buzzed = true;
        playerBuzz = true;
        playerBackground.color = buzzedColour;

        if (!textManager.stopText) textManager.stopText = true;
        if (audioSource.isPlaying)
        {
            playerEarlyBuzz = true;
            audioSource.Stop();
        }
        else
            allEarlyBuzzes = false;

        audioSource.clip = buzzerNoise;
        audioSource.Play();
        if (!answerManager.isEnabled)
        {
            answerManager.answerTextBox.gameObject.SetActive(true);
            answerManager.EnableInputBox();
        }
        StartCoroutine(EasyAnticheat());
    }

    public IEnumerator EasyAnticheat()
    {
        float timeUntilBoot = Time.time + timeToAnswer;
        int buzzedOnQuestion = questionNumber;
        while (buzzed && Time.time <= timeUntilBoot)
        {
            playerAnswerTime += Time.deltaTime;
            yield return null;
        }
        if (!buzzed || questionNumber != buzzedOnQuestion) yield break;
        tookTooLong = true;

        if (achievementManager != null && !(victoryVideo.activeSelf || losingBackground.activeSelf))
        {
            achievementManager.UnlockAchievement(AllAchievements.AwayFromKeyboard);
        }

        audioSource.PlayRandomLine(audioSource.slowLines);
        answerManager.DisableInputBox();
        yield return new WaitForSeconds(0.1f);
        while (audioSource.isPlaying) yield return null;

        if (gotWrongOnce)
        {
            audioSource.PlayRandomLine(audioSource.transitionLines);
            yield return new WaitForSeconds(0.1f);
            while (audioSource.isPlaying) yield return null;
            PlayRandomQuestion();
        }
        else
        {
            audioSource.PlayRandomLine(audioSource.changeLines);
            yield return new WaitForSeconds(0.1f);
            while (audioSource.isPlaying) yield return null;
            StartCoroutine(OtherContestantAnswer());
        }
    }

    [ContextMenu("Contestant Buzz")]
    public void ContestantAnswer()
    {
        StartCoroutine(ContestantBuzzer());
    }

    private IEnumerator ContestantBuzzer()
    {
        buzzed = true;
        opponentBackground.color = buzzedColour;

        if (!textManager.stopText) textManager.stopText = true;
        if (audioSource.isPlaying) audioSource.Stop();
        audioSource.clip = buzzerNoise;
        audioSource.Play();
        if (!answerManager.isEnabled)
        {
            answerManager.answerTextBox.gameObject.SetActive(true);
            answerManager.EnableInputBox(false);
        }

        yield return new WaitForSeconds(answerManager.animationTime);

        bool isCorrect = contestant.IsCorrect();
        StartCoroutine(OpponentTypingAnimation(isCorrect));
        yield return new WaitForSeconds(0.1f);
        while (typing) yield return null;

        answerManager.DisableInputBox();

        if (isCorrect)
        {
            StartCoroutine(CorrectVoicelines());
            opponentScoreText.text = (++opponentScore).ToString();
            scoreManager.Penalise();
        }
        else
        {
            StartCoroutine(IncorrectVoicelines());
        }
    }

    private IEnumerator OpponentTypingAnimation(bool isCorrect)
    {
        typing = true;
        int index = Random.Range(0, answerManager.validAnswers.Length);
        string contestantAnswer = answerManager.validAnswers[index];
        if (!isCorrect)
            contestantAnswer = answerManager.CreateBadAnswer(contestantAnswer, Mathf.CeilToInt(contestantAnswer.Length * contestant.garbledness));

        foreach (char c in contestantAnswer)
        {
            answerManager.answerTextBox.text += c;
            yield return new WaitForSeconds(0.15f + Random.Range(0f, 0.35f));
        }
        typing = false;
    }

    public void OnTextBoxEnter()
    {
        bool notPlayingBuzzerNoise = audioSource.isPlaying && audioSource.clip != buzzerNoise;
        if (!playerBuzz || tookTooLong || notPlayingBuzzerNoise) return;
        bool wasCorrect = answerManager.ContainsAnswer(answerManager.answerTextBox.text,
                                                       answerManager.validAnswers,
                                                       answerManager.invalidAnswers
                                                       );
        answerManager.DisableInputBox();

        if (wasCorrect)
        {
            scoreManager.ScoreCorrectAnswer(playerEarlyBuzz, playerAnswerTime);
            speedrunTimer.Split();
            StartCoroutine(CorrectVoicelines());
            playerScoreText.text = (++playerScore).ToString();
            if (playerScore == 3 || playerScore == 5) backgroundMusicChange = true;
        }
        else
        {
            noWrongAnswers = false;
            StartCoroutine(IncorrectVoicelines());
        }
    }

    public IEnumerator CorrectVoicelines()
    {
        audioSource.PlayRandomLine(audioSource.correctLines);
        yield return new WaitForSeconds(0.1f);
        while (audioSource.isPlaying) yield return null;

        if (playerScore >= scoreToWin)
        {
            StartCoroutine(Victory());
            yield break;
        }

        audioSource.PlayRandomLine(audioSource.transitionLines);
        yield return new WaitForSeconds(0.1f);
        while (audioSource.isPlaying) yield return null;

        PlayRandomQuestion();
    }

    public IEnumerator IncorrectVoicelines()
    {
        audioSource.PlayRandomLine(audioSource.wrongLines);
        yield return new WaitForSeconds(0.1f);
        while (audioSource.isPlaying) yield return null;

        if (gotWrongOnce)
        {
            audioSource.PlayRandomLine(audioSource.transitionLines);
            yield return new WaitForSeconds(0.1f);
            while (audioSource.isPlaying) yield return null;
            PlayRandomQuestion();
        }
        else
        {
            audioSource.PlayRandomLine(audioSource.changeLines);
            yield return new WaitForSeconds(0.1f);
            while (audioSource.isPlaying) yield return null;
            StartCoroutine(OtherContestantAnswer());
        }
    }

    public IEnumerator Victory()
    {
        playerScoreText.gameObject.SetActive(false);
        opponentScoreText.gameObject.SetActive(false);
        audioSource.musicSource.Stop();
        textManager.questionText.text = "";

        if (achievementManager != null)
        {
            achievementManager.UnlockAchievement(AllAchievements.Studious);
            if (noWrongAnswers)
                achievementManager.UnlockAchievement(AllAchievements.Baller);
            if (noWrongAnswers && allEarlyBuzzes)
                achievementManager.UnlockAchievement(AllAchievements.CircularBaller);
            if (noWrongAnswers && allEarlyBuzzes && opponentScore == 0)
                achievementManager.UnlockAchievement(AllAchievements.FilledCircularBaller);
        }

        losingBackground.SetActive(false);
        victoryVideo.SetActive(true);
        StartCoroutine(scoreManager.VictoryScoreAnimation(true));

        yield return new WaitForSeconds(15f);

        scoreManager.AddToSavedScore();
        GoToNextLevel();
    }

    public IEnumerator NotVictoryQuiteTheOppositeActually()
    {
        playerScoreText.gameObject.SetActive(false);
        opponentScoreText.gameObject.SetActive(false);
        audioSource.musicSource.Stop();
        textManager.questionText.text = "";

        victoryVideo.SetActive(false);
        losingBackground.SetActive(true);
        scoreManager.minigameScore = 0;
        StartCoroutine(scoreManager.VictoryScoreAnimation(false));

        StopCoroutine(EasyAnticheat());
        yield return new WaitForSeconds(20f);

        GoToNextLevel();
    }

    private void GoToNextLevel()
    {
        string nextScene = LevelOrder.GetNextLevel("SayThatAnswer");
        LevelOrder.IncrementSavedLevel();
        PlayerPrefs.SetInt("LeftSTA", 1);
        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }
}
