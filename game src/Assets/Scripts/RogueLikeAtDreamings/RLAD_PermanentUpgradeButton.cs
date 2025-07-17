using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RLADUpgradeButtons : MonoBehaviour
{
    public RLADBlueprint blueprint;

    public TextMeshProUGUI label;
    public TextMeshProUGUI level;

    public Button upgradeButton;
    public TextMeshProUGUI cost;

    private Color purple;

    // Start is called before the first frame update
    void Start()
    {
        purple = new Color(0.5f, 0, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitialiseButton(PermanentUpgradeTypes type,
                                 RLADPlayer player,
                                 Func<float, int> costFunction, 
                                 string name
                                 )
    {
        upgradeButton.onClick.AddListener(
            () => 
            {
                int upgradeVar = type switch
                {
                    PermanentUpgradeTypes.Door     => player.permanentUpgrades.doorStrength,
                    PermanentUpgradeTypes.Power    => player.permanentUpgrades.powerDrainage,
                    PermanentUpgradeTypes.Time     => player.permanentUpgrades.timeSpeed,
                    PermanentUpgradeTypes.Plushies => player.permanentUpgrades.numPlushies,
                    PermanentUpgradeTypes.Internet => player.permanentUpgrades.internetSpeed,
                    _ => throw new Exception("Invalid type for button initialisation.")
                };

                int price = costFunction(upgradeVar);
                if (blueprint.isActive || price > player.money)
                {
                    return;
                }


                player.money -= price;

                SetLevelText(++upgradeVar);
                cost.text = "$" + costFunction(upgradeVar + 1).ToString();

                switch (type) // I wish I could combine the switch statement and expression but I don't think I can
                {
                    case PermanentUpgradeTypes.Door:
                        player.permanentUpgrades.doorStrength++;
                        break;
                    case PermanentUpgradeTypes.Time:
                        player.permanentUpgrades.timeSpeed++;
                        break;
                    case PermanentUpgradeTypes.Power:
                        player.permanentUpgrades.powerDrainage++;
                        break;
                    case PermanentUpgradeTypes.Plushies:
                        player.permanentUpgrades.numPlushies++;
                        break;
                    case PermanentUpgradeTypes.Internet:
                        player.permanentUpgrades.internetSpeed++;
                        break;
                    default:
                        throw new Exception("how did you get here");
                }
            }
        );
        cost.text = "$" + costFunction(0).ToString();
        label.text = name;
    }

    public Color BigColourLerp(float t, params Color[] colours)
    {
        if (t >= colours.Length - 1)
        {
            return colours[^1];
        }

        if (t <= 0)
        {
            return colours[0];
        }

        float fractionalPart = t - (int)t;
        return Color.Lerp(colours[(int)t], colours[(int)t + 1], fractionalPart);
    }

    public void SetLevelText(int newLevel)
    {
        level.text = "lv. " + (newLevel + 1).ToString();
        level.color = BigColourLerp((float)(newLevel / 10f),
                                    Color.red,
                                    Color.yellow,
                                    purple,
                                    Color.magenta,
                                    Color.green
                                    );
    }
}

