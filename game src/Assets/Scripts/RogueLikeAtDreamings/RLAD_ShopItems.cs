using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RLADShopItem : MonoBehaviour
{
    public RLADBlueprint blueprint;
    public RLADPhone phone;

    public TextMeshProUGUI itemName;
    public TextMeshProUGUI description;
    public TextMeshProUGUI category;

    public Button buyButton;
    public TextMeshProUGUI costText;

    public Action OnItemPurchaseCallback;

    public AchievementManager achievementManager;

    // Start is called before the first frame update
    void Start()
    {
        if (SaveSystem.IsDemo()) return;
        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Initialise(RLADPlayer player, object item)
    {
        if (item is TemporaryUpgradeTypes temp)
            Initialise(player, temp);
        else if (item is RLAD_PurchasablePermanentUpgrades perma)
            Initialise(player, perma);
        else if (item is RLAD_Items bweh)
            Initialise(player, bweh);
        else
            throw new System.Exception($"Invalid object of type {item.GetType()} passed in!");
    }

    public void Initialise(RLADPlayer player, TemporaryUpgradeTypes type)
    {
        AudioSource transaction = GetComponentInChildren<AudioSource>();
        (int cost, string name, string itemDescription) = GetUpgradeData(type);
        cost = Mathf.RoundToInt((1 - player.discount) * cost);
        (costText.text, itemName.text, description.text) = ("$" + cost.ToString(), name, itemDescription);
        category.text = "Automatic Item";
        buyButton.interactable = true;
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(
            () =>
            {
                if (blueprint.isActive || cost > player.money)
                {
                    return;
                }

                player.money -= cost;
                buyButton.interactable = false;
                transaction.Play();
                
                switch (type)
                {
                    case TemporaryUpgradeTypes.AdobeOneDay:
                        player.temporaryUpgrades.adobeOneDay = true;
                        break;
                    //case TemporaryUpgradeTypes.BananaPeel:
                    //    blueprint.SelectRoom(player, TemporaryUpgradeTypes.BananaPeel);
                    //    break;
                    case TemporaryUpgradeTypes.ClickteamOneDay:
                        player.temporaryUpgrades.clickteamOneDay = true;
                        break;
                    //case TemporaryUpgradeTypes.ComingOver:
                    //    phone.SelectContact();
                    //    break;
                    case TemporaryUpgradeTypes.MultiplayerSteamGame:
                        player.temporaryUpgrades.multiplayerSteamGame = 3;
                        break;
                    case TemporaryUpgradeTypes.PrescriptionMedication:
                        player.temporaryUpgrades.prescriptionMedication = true;
                        break;
                    //case TemporaryUpgradeTypes.DuctTape:
                    //    player.temporaryUpgrades.ductTape = true;
                    //    break;
                    //case TemporaryUpgradeTypes.BatteryPack:
                    //    player.temporaryUpgrades.batteryPack = true;
                    //    break;
                    case TemporaryUpgradeTypes.PrepaidSimCard:
                        player.temporaryUpgrades.extraData = true;
                        break;
                    case TemporaryUpgradeTypes.MaintenanceGuy:
                        player.temporaryUpgrades.maintenanceGuy = true;
                        break;
                    default:
                        throw new System.Exception("how");
                }
            }
        );
    }

    public void Initialise(RLADPlayer player, RLAD_Items type)
    {
        AudioSource transaction = GetComponentInChildren<AudioSource>();
        (int cost, string name, string itemDescription) = GetUpgradeData(type);
        cost = Mathf.RoundToInt((1 - player.discount) * cost);
        (costText.text, itemName.text, description.text) = ("$" + cost.ToString(), name, itemDescription);
        category.text = "Manual Item";
        buyButton.interactable = true;
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(
            () =>
            {
                if (blueprint.isActive || cost > player.money)
                {
                    return;
                }

                player.money -= cost;
                buyButton.interactable = false;
                transaction.Play();

                player.inventory.Add(type);
                OnItemPurchaseCallback();
            }
        );
    }

    public void Initialise(RLADPlayer player, RLAD_PurchasablePermanentUpgrades type)
    {
        AudioSource transaction = GetComponentInChildren<AudioSource>();
        (int cost, string name, string itemDescription) = GetUpgradeData(type);
        cost = Mathf.RoundToInt((1 - player.discount) * cost);
        (costText.text, itemName.text, description.text) = ("$" + cost.ToString(), name, itemDescription);
        category.text = "Permanent Upgrade";
        buyButton.interactable = true;
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(
            () =>
            {
                if (blueprint.isActive || cost > player.money)
                {
                    return;
                }

                player.money -= cost;
                buyButton.interactable = false;
                transaction.Play();

                if (achievementManager != null)
                    achievementManager.UnlockAchievement(AllAchievements.Investor);

                switch (type)
                {
                    case RLAD_PurchasablePermanentUpgrades.LuringSystem:
                        player.permanentUpgrades.hasLuringSystem = true;
                        break;
                    //case RLAD_PurchasablePermanentUpgrades.HomeTheatre:
                    //    player.permanentUpgrades.hasHomeTheatre = true;
                    //    break;
                    //case RLAD_PurchasablePermanentUpgrades.SolarSystem:
                    //    player.permanentUpgrades.solarSystem = true;
                    //    break;
                    case RLAD_PurchasablePermanentUpgrades.FiberOpticConnection:
                        player.permanentUpgrades.fiberConnection = true;
                        break;
                    case RLAD_PurchasablePermanentUpgrades.NintendoEntertainmentSystem:
                        player.permanentUpgrades.nes = true;
                        break;
                    case RLAD_PurchasablePermanentUpgrades.ShopMembership:
                        player.permanentUpgrades.membership = true;
                        break;
                    default:
                        throw new System.Exception("how");
                }
            }
        );
    }

    public static (int, string, string) GetUpgradeData(TemporaryUpgradeTypes type)
    {
        return type switch
        {
            TemporaryUpgradeTypes.AdobeOneDay            => (95, "Premiere 1 Day Licence", "Dreaming will waste time editing a video, but will grow angry at the numerous crashes."),
            //TemporaryUpgradeTypes.BananaPeel             => (1, "Banana Peel", "If anyone slips on the banana peel, the fall will create a loud noise, alerting you to their location."),
            TemporaryUpgradeTypes.ClickteamOneDay        => (75, "Clickteam 1 Day Licence", "Dreaming will waste time porting a game to PC, but he can grow frustrated with the decompiler."),
            //TemporaryUpgradeTypes.ComingOver             => (35, "Burner Phone", "Allows you to invite a contact over for the night, distracting or revealing Dreaming in the process."),
            TemporaryUpgradeTypes.MultiplayerSteamGame   => (60, "Multiplayer Steam Game", "Dreaming and Gaster will spend more time in their rooms playing the game for three days."),
            TemporaryUpgradeTypes.PrescriptionMedication => (40, "Prescription", "Benzodiazepine has been proven to reduce hallucinogenic effects in patients. Don't worry about the side effects."),
            //TemporaryUpgradeTypes.DuctTape               => (25, "Duct Tape", "Allows you to repair the door somewhat."),
            //TemporaryUpgradeTypes.BatteryPack            => (250, "3", "Keeps the power going for some time after a blackout."),
            TemporaryUpgradeTypes.SleepingPills          => (60, "Sleeping Pills", "Gaster will be asleep for the start of the night."),
            TemporaryUpgradeTypes.PrepaidSimCard         => (25, "Prepaid SIM Card", "Gives an extra 15GB worth of data for the night."),
            TemporaryUpgradeTypes.MaintenanceGuy         => (80, "Elevator Technician", "The elevator will be out of service for the night."),
            _ => throw new System.Exception($"Invalid temporary upgrade {type}!"),
        };
    }

    public static (int, string, string) GetUpgradeData(RLAD_Items type)
    {
        return type switch {
            RLAD_Items.CakePremix        => (20, "Cake Premix", "Throwing this at Dreaming will cause make him go to the Kitchen."),
            RLAD_Items.SingleBluray      => (15, "A Single Blu-ray", "Throwing this at Dreaming will make him put the box into his collection."),
            RLAD_Items.BurnerPhone       => (50, "Burner Phone", "Allows you to dial the landline, distracting the person closest to you."),
            RLAD_Items.Fertiliser        => (30, "Fertiliser", "Causes the lawn to become overgrown, distracting the person closest to you."),
            RLAD_Items.SpaghettiDelivery => (60, "Food Delivery", "Deliver a bowl of spaghetti to the house, distracting and freezing Gaster for 2 hours."),
            _ => throw new System.Exception($"Invalid item {type}!")
        };
    }

    public static (int, string, string) GetUpgradeData(RLAD_PurchasablePermanentUpgrades type)
    {
        return type switch
        {
            RLAD_PurchasablePermanentUpgrades.LuringSystem => (400, "Criterion Collection", "Allows you to lure Dreaming to certain rooms by leaving Blu-rays on the ground. Adds replacement cost per usage."),
            //RLAD_PurchasablePermanentUpgrades.HomeTheatre  => (500, "Home Theatre", "Whenever Dreaming is in this room, he will stop to watch a movie. Adds maintenance cost every night."),
            //RLAD_PurchasablePermanentUpgrades.SolarSystem  => (375, "Solar System", "Permanently upgrades your power capacity by 50%. Can fail due to weather conditions."),
            RLAD_PurchasablePermanentUpgrades.FiberOpticConnection => (400, "Fiber Optic Internet", "Permanently lowers internet usage and permanently adds 10GB of data."),
            RLAD_PurchasablePermanentUpgrades.NintendoEntertainmentSystem => (350, "Old Console", "Gaster will not notice internet outages as quickly."),
            RLAD_PurchasablePermanentUpgrades.ShopMembership => (250, "Shop Membership", "Every shop will have 3 restocks."),
            _ => throw new System.Exception($"Invalid permanent upgrade {type}!"),
        };
    }
}
