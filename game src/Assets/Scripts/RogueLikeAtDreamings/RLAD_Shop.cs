using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RogueLikeAtDreamings;
using RogueLikeAtDreamings.DebtPaymentScreen;
using RogueLikeAtDreamings.Tabs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RLADShop : MonoBehaviour
{
    public RLADPlayer player;
    public RLADShopItem shopItem1,
                        shopItem2,
                        shopItem3,
                        shopItem4;
    public Challenge[] challenges;
    public TextMeshProUGUI moneyText;
    public Button rerollButton;
    public int rerollsLeft;
    public RLAD_DebtPaymentScreen debt;
    public AchievementManager achievementManager;

    public GameObject challengeParent;
    public ItemSelectionTab inventoryView;
    public TextMeshProUGUI inventoryToggleText;
    private bool _showingInventory;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        Refresh();
        rerollsLeft = 1;
        foreach (RLADShopItem item in new[] { shopItem1, shopItem2, shopItem3, shopItem4 })
        {
            item.OnItemPurchaseCallback = inventoryView.Refresh;
        }
        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        string moneyString;
        if (player.money >= 0)
            moneyString = "$" + player.money.ToString();
        else
            moneyString = "-$" + (-player.money).ToString();

        if (moneyString != moneyText.text)
            moneyText.text = moneyString;
    }

    [ContextMenu("Refresh Shop")]
    public void RefreshShopItems()
    {
        rerollsLeft--;
        rerollButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Restock{(rerollsLeft > 1 ? $" x{rerollsLeft}" : "")}";
        if (rerollsLeft <= 0) rerollButton.interactable = false;

        List<TemporaryUpgradeTypes> types = Enum.GetValues(typeof(TemporaryUpgradeTypes))
                                                .Cast<TemporaryUpgradeTypes>()
                                                .ToList()
                                                .GetRandomItems(4);

        List<RLAD_Items> items = Enum.GetValues(typeof(RLAD_Items))
                                     .Cast<RLAD_Items>()
                                     .ToList()
                                     .GetRandomItems(4);

        List<RLAD_PurchasablePermanentUpgrades> availablePermanentUpgrades = new();
        //if (!player.permanentUpgrades.hasHomeTheatre)
        //    availablePermanentUpgrades.Add(RLAD_PurchasablePermanentUpgrades.HomeTheatre);
        if (!player.permanentUpgrades.hasLuringSystem)
            availablePermanentUpgrades.Add(RLAD_PurchasablePermanentUpgrades.LuringSystem);
        //if (!player.permanentUpgrades.solarSystem)
        //    availablePermanentUpgrades.Add(RLAD_PurchasablePermanentUpgrades.SolarSystem);
        if (!player.permanentUpgrades.nes)
            availablePermanentUpgrades.Add(RLAD_PurchasablePermanentUpgrades.NintendoEntertainmentSystem);
        if (!player.permanentUpgrades.fiberConnection)
            availablePermanentUpgrades.Add(RLAD_PurchasablePermanentUpgrades.FiberOpticConnection);
        if (!player.permanentUpgrades.membership)
            availablePermanentUpgrades.Add(RLAD_PurchasablePermanentUpgrades.ShopMembership);

        bool permanentUpgradesAvailable = availablePermanentUpgrades.Count != 0;
        RLAD_PurchasablePermanentUpgrades? randomPermanentUpgrade = permanentUpgradesAvailable ?
                                                                    availablePermanentUpgrades[UnityEngine.Random.Range(0, availablePermanentUpgrades.Count)] :
                                                                    null;

        IList bweh() => UnityEngine.Random.Range(0, 1f) < 0.4f ? types : items;
        bool swapForcedItems = UnityEngine.Random.Range(0, 1f) < 0.5f;
        IList force(bool isForced) => isForced ? types : items;

        shopItem1.Initialise(player, force(swapForcedItems)[0]);
        shopItem2.Initialise(player, force(!swapForcedItems)[1]);
        shopItem3.Initialise(player, bweh()[2]);
        shopItem4.Initialise(player, permanentUpgradesAvailable ?
                                     (RLAD_PurchasablePermanentUpgrades)randomPermanentUpgrade :
                                     bweh()[3]
                            );
    }

    [ContextMenu("Reload Shop")]
    public void Refresh()
    {
        rerollButton.interactable = true;
        rerollsLeft = player.permanentUpgrades.membership ? 3 : 1;
        if (player.currentChallenge?.positive == Patches.DiceRoll) rerollsLeft += 2;
        if (player.currentChallenge?.positive == Patches.OverstockPlus) rerollsLeft += 5;

        SetInventoryView(false);
        inventoryView.index = 0;

        rerollsLeft++; // Cancels out when RefreshShopItems() is called
        RefreshShopItems();

        List<Patch> alreadySelectedPatches = new();
        foreach (Challenge challenge in challenges)
        {
            challenge.checkbox.isOn = false;
            challenge.checkbox.onValueChanged.RemoveAllListeners();
            challenge.SelectRandomChallenge(alreadySelectedPatches);
            alreadySelectedPatches.Add(challenge.positive);
            alreadySelectedPatches.Add(challenge.negative);
            challenge.checkbox.onValueChanged.AddListener(
                (ticked) =>
                {
                    if (ticked)
                    {
                        foreach (Challenge chal in challenges)
                        {
                            if (chal == challenge) continue;
                            chal.checkbox.isOn = false;
                        }
                    }
                    player.currentChallenge = ticked ? challenge : null;
                }
            );
        }

        if (achievementManager != null)
        {
            if (player.money >= 1000)
                achievementManager.UnlockAchievement(AllAchievements.Thousandaire);
        }

        if (player.debt != 0) // Yes, this would show negative debt values. If it does show that, something has fucked up.
        {
            debt.gameObject.SetActive(true);
            debt.Appear();
        }
        else
        {
            debt.gameObject.SetActive(false);
        }
    }

    public void SetInventoryView(bool showInventory)
    {
        if (showInventory)
        {
            inventoryView.gameObject.SetActive(true);
            inventoryView.Refresh();
            challengeParent.SetActive(false);
        }
        else
        {
            inventoryView.gameObject.SetActive(false);
            challengeParent.SetActive(true);
        }
        inventoryToggleText.text = showInventory ? "Inventory" : "Challenges";
        _showingInventory = showInventory;
    }

    public void ToggleInventoryView()
    {
        SetInventoryView(!_showingInventory);
    }
}

public static class ListExtensions
{
    public static List<T> GetRandomItems<T>(this List<T> list, int number)
    {
        List<T> resultant = list;

        do
        {
            int randomIndex = UnityEngine.Random.Range(0, resultant.Count);
            resultant.RemoveAt(randomIndex);
        }
        while (resultant.Count > number);
        
        return resultant.Shuffle();
    }

    public static List<T> Shuffle<T>(this List<T> list)
    {
        List<T> remaining = list;
        List<T> resultant = new();

        do
        {
            int randomIndex = UnityEngine.Random.Range(0, remaining.Count);
            resultant.Add(remaining[randomIndex]);
            remaining.RemoveAt(randomIndex);
        }
        while (remaining.Count > 0);

        return resultant;
    }
}