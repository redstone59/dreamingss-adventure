using TMPro;
using UnityEngine;
using UnityEngine.UI;

using AchievementEnums;
using Achievements;

public class AchievementBox : MonoBehaviour
{
    public RawImage icon;
    public RawImage lockImage;
    public TextMeshProUGUI achievementName;
    public TextMeshProUGUI description;

    private Achievement nullOrPlaceholder;

    void Awake()
    {
        nullOrPlaceholder = new()
        {
            name = "???",
            description = "???",
            image = Resources.Load<Texture>("Achievements/Achievement Images/Generic/hidden"),
            key = AchievementKeys.OutsideOfMinigame,
            bitPosition = (int)OutsideOfMinigame.NullOrPlaceholder
        };
        lockImage.color = Color.clear;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        
    }

    public void UpdateBox(Achievement? achievement)
    {
        if (achievement == null)
        {
            achievementName.text = "";
            description.text = "";
            icon.texture = Resources.Load<Texture>("Achievements/Achievement Images/Generic/hidden");
            icon.color = Color.clear;
            lockImage.color = Color.clear;
            return;
        }

        Achievement toUpdate = achievement.GetValueOrDefault(nullOrPlaceholder);

        if (!AchievementUtils.HasUnlocked(toUpdate))
        {
            description.text = "???";

            if (toUpdate.hidden || !toUpdate.condition())
            {
                achievementName.text = "?";
                icon.texture = nullOrPlaceholder.image;
                icon.color = Color.white;
                lockImage.color = Color.clear;
            }
            else
            {
                achievementName.text = toUpdate.name;
                icon.texture = toUpdate.image;
                icon.color = Color.gray;
                lockImage.color = Color.white;
            }
        }
        else
        {
            achievementName.text = toUpdate.name;
            if (toUpdate.image == null)
                icon.texture = Resources.Load<Texture>("Achievements/Achievement Images/Generic/PlaceholderImage");
            else
                icon.texture = toUpdate.image;
            icon.color = Color.white;
            description.text = toUpdate.description;
            lockImage.color = Color.clear;
        }
    }
}