using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    public AudioSource achievementNoise;
    public List<Achievement> achievementQueue;

    public float slideLength = 0.2f;
    public float holdLength = 2.6f;

    public RectTransform popupTransform;
    public float popupHiddenY = -116.7f;
    public float popupVisibleY = 3.1417675f;

    public RawImage achievementImage;
    public TextMeshProUGUI achievementName;
    public TextMeshProUGUI achievementDescription;

    private bool showingAchievements = false;
    public bool AchievementsInQueue
    {
        get { return achievementQueue.Count > 0 || showingAchievements; }
    }

    public static AchievementManager instance;

    // Start is called before the first frame update
    void Start()
    {
        if (instance || SaveSystem.IsDemo()) DestroyImmediate(gameObject);
        else
        {
            DontDestroyOnLoad(gameObject);
            achievementNoise = GetComponent<AudioSource>();

            Vector3 startPosition = popupTransform.anchoredPosition;
            startPosition.y = popupHiddenY;
            popupTransform.anchoredPosition = startPosition;
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!showingAchievements && achievementQueue.Count > 0)
            StartCoroutine(AchievementAnimation());
    }

    public void UnlockAchievement(Achievement achievement)
    {
        int currentAchievementValue = SceneManager.GetActiveScene().name == "TitleScreen"
                                          ? SaveSystem.GameData.outsideOfMinigameAchievements
                                          : SaveSystem.GetMinigameData().achievements;
        int value = 1 << achievement.bitPosition;
        if ((currentAchievementValue & value) >= 1) return; // If the achievement is already unlocked, don't continue

        if (SceneManager.GetActiveScene().name == "TitleScreen")
            SaveSystem.GameData.outsideOfMinigameAchievements = currentAchievementValue | value;
        else
        {
            MinigameData minigameData = SaveSystem.GetMinigameData();
            minigameData.achievements = currentAchievementValue | value;
            SaveSystem.SetMinigameData(minigameData);
        }
        SaveSystem.WriteSaveFile();
        achievementQueue.Add(achievement);
    }

    public IEnumerator AchievementAnimation()
    {
        if (showingAchievements) yield break;
        showingAchievements = true;
        while (achievementQueue.Count > 0)
        {
            Achievement displayedAchievement = achievementQueue[0];

            achievementName.text = displayedAchievement.name;
            achievementDescription.text = displayedAchievement.description;
            if (displayedAchievement.image != null)
                achievementImage.texture = displayedAchievement.image;
            else
                achievementImage.texture = Resources.Load<Texture>("Achievements/Achievement Images/Generic/PlaceholderImage");

            achievementNoise.Play();
            StartCoroutine(SlideAnimation(popupHiddenY, popupVisibleY, slideLength));
            yield return new WaitForSeconds(slideLength + holdLength);
            StartCoroutine(SlideAnimation(popupVisibleY, popupHiddenY, slideLength));
            yield return new WaitForSeconds(slideLength);

            achievementQueue.RemoveAt(0);
            yield return new WaitForSeconds(0.5f);
        }
        showingAchievements = false;
    }

    public IEnumerator SlideAnimation(float y1, float y2, float length)
    {
        float timeElapsed = 0;
        float animationCompletion;
        Vector3 newPosition;

        while (timeElapsed < length)
        {
            animationCompletion = timeElapsed / length;
            float yLerp = Mathf.Lerp(y1, y2, animationCompletion);

            newPosition = popupTransform.anchoredPosition;
            newPosition.y = yLerp;
            popupTransform.anchoredPosition = newPosition;

            yield return new WaitForFixedUpdate();
            timeElapsed += Time.fixedDeltaTime;
        }

        newPosition = popupTransform.anchoredPosition;
        newPosition.y = y2;
        popupTransform.anchoredPosition = newPosition;
    }

    [ContextMenu("Add Dummy Achievement to Queue")]
    public void AddDummyAchievement()
    {
        Achievement dummy = new()
        {
            name = "test achievement 123",
            description = "This is a test achievement designed to test if the new animation works. kthxbye",
            image = null,
            bitPosition = 128,
            key = AchievementKeys.Simon
        };
        achievementQueue.Add(dummy);
    }
}
