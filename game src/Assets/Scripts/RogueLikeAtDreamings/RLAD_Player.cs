using System.Collections.Generic;
using UnityEngine;
using RogueLikeAtDreamings;

public class RLADPlayer : MonoBehaviour
{
    public int nightsCompleted = 0;
    public int money = 7;
    public int debt = 0;
    public int challengesCompleted = 0;
    public float discount;
    public RLADPermanentUpgrades permanentUpgrades;
    public RLADTemporaryUpgrades temporaryUpgrades;
    public Challenge currentChallenge;
    public List<RLAD_Items> inventory;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        permanentUpgrades = new();
        temporaryUpgrades = new();
        inventory = new()
        {
            Capacity = 16
        };
        currentChallenge = null;
    }

    public bool HasUpgrade(TemporaryUpgradeTypes type)
    {
        switch (type)
        {
            case TemporaryUpgradeTypes.AdobeOneDay:
                return temporaryUpgrades.adobeOneDay;
            case TemporaryUpgradeTypes.ClickteamOneDay:
                return temporaryUpgrades.clickteamOneDay;
            //case TemporaryUpgradeTypes.ComingOver:
            //    return temporaryUpgrades.comingOver != null;
            case TemporaryUpgradeTypes.PrescriptionMedication:
                return temporaryUpgrades.prescriptionMedication;
            case TemporaryUpgradeTypes.MultiplayerSteamGame:
                return temporaryUpgrades.multiplayerSteamGame > 0;
        }

        throw new System.Exception("how did you get here");
    }

    public bool HasItem(RLAD_Items item)
    {
        return inventory.Contains(item);
    }

    public void AddInterest(float percent)
    {
        debt = Mathf.RoundToInt(debt * (1 + percent));
    }
}