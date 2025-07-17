using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RogueLikeAtDreamings
{
    public enum Rarity
    {
        Common,
        Rare,
        UltraRare
    }

    public struct Patch
    {
        public string name;
        public string description;
        public bool isNegative;
        public Rarity rarity;

        public readonly override bool Equals(object other)
        {
            if (other is Patch patch)
            {
                return patch.name == name;
            }
            return false;
        }

        public readonly override int GetHashCode()
        {
            Debug.Log("Hash code called for.");
            return 0;
        }

        public static bool operator==(Patch a, Patch b) => a.Equals(b);
        public static bool operator!=(Patch a, Patch b) => !a.Equals(b);
    }

    public class Patches
    {
        static string Item(object obj)
        {
            const string itemColour = "blue";
            if (obj is RLAD_Items item)
                return $"<color={itemColour}>{RLADShopItem.GetUpgradeData(item).Item2}</color>";
            else if (obj is RLAD_PurchasablePermanentUpgrades perma)
                return $"<color={itemColour}>{RLADShopItem.GetUpgradeData(perma).Item2}</color>";
            else if (obj is TemporaryUpgradeTypes temp)
                return $"<color={itemColour}>{RLADShopItem.GetUpgradeData(temp).Item2}</color>";
            else
                throw new ArgumentException($"Invalid shop item of type {obj.GetType()}!");
        }

        public static List<Patch> GetAllPatches()
        {
            Patches patches = new();
            var everyPatch = patches.GetType()
                                    .GetFields()
                                    .Select(field => field.GetValue(patches))
                                    .ToArray();

            List<Patch> allPatches = new();

            foreach (var field in everyPatch)
            {
                if (field is Patch patch)
                {
                    allPatches.Add(patch);
                }
                continue;
            }

            return allPatches;
        }

        public static Patch PenaltyRates = new()
        {
            name = "Penalty Rates",
            description = "Receive 1.5x pay for the night.",
            rarity = Rarity.Common
        };

        public static Patch PartTime = new()
        {
            name = "Part Time",
            description = "The night ends at 3AM.",
            rarity = Rarity.Common
        };

        public static Patch ExtraData = new()
        {
            name = "Extra Data",
            description = "Start the night with an extra 10GB of data.",
            rarity = Rarity.Common
        };

        public static Patch ClearanceSale = new()
        {
            name = "Clearance Sale",
            description = "Receive a 25% discount on the next shop.",
            rarity = Rarity.Common
        };

        public static Patch Boxset = new()
        {
            name = "Movie Night",
            description = $"Receive 2 {Item(RLAD_Items.SingleBluray)}s for free.",
            rarity = Rarity.Common
        };

        public static Patch DiceRoll = new()
        {
            name = "Overstock",
            description = "Receive 2 extra restocks in the next shop.",
            rarity = Rarity.Common
        };

        public static Patch LongerNight = new()
        {
            name = "Cover Shift",
            description = "You're gonna have to cover an extra 2 hours, sorry!",
            rarity = Rarity.Common,
            isNegative = true
        };

        public static Patch EnergyDrinks = new()
        {
            name = "Caffeine Before Bed",
            description = "Dreaming and Gaster move 40% faster this night.",
            rarity = Rarity.Common,
            isNegative = true
        };

        public static Patch Torrent = new()
        {
            name = "Torrent",
            description = "Dreaming wants to listen to a new album, so there will be some data drain at the start of the night.",
            rarity = Rarity.Common,
            isNegative = true
        };

        public static Patch NoRecharge = new()
        {
            name = "Forgetful",
            description = "You forgot to recharge the flash beacon overnight! You did get it to 20% before you left, though.",
            rarity = Rarity.Common,
            isNegative = true
        };

        public static Patch TimeDilation = new()
        {
            name = "Time Dilation",
            description = "Somehow you're travelling at relativistic speeds? Time is moving faster than normal?",
            rarity = Rarity.Rare
        };

        public static Patch TheChallenge = new()
        {
            name = "The Challenge",
            description = "The night's only a minute long, but Dreaming and Gaster can move extremely fast.",
            rarity = Rarity.Rare
        };

        public static Patch ElevatorMalfunction = new()
        {
            name = "Elevator Malfunction",
            description = "Yeah, the doors are all jammed! You're going to have to take the stairs.",
            rarity = Rarity.Rare
        };

        public static Patch Liquidation = new()
        {
            name = "Liquidation",
            description = "Get a 50% discount on the next shop.",
            rarity = Rarity.Rare
        };

        public static Patch OverstockPlus = new()
        {
            name = "Overstock+",
            description = "Receive 5 extra restocks in the next shop.",
            rarity = Rarity.Rare
        };

        public static Patch Overtime = new()
        {
            name = "Overtime",
            description = "You're going to have to cover part of my shift. It's only 3 hours, you'll be fine!",
            rarity = Rarity.Rare,
            isNegative = true
        };

        public static Patch VideoMalfunction = new()
        {
            name = "Video Malfunction",
            description = "Yeah, it's been bugging out all day today. You can work with it, though.",
            rarity = Rarity.Rare,
            isNegative = true
        };

        public static Patch Underpayment = new()
        {
            name = "Underpayment",
            description = "Where did a third of your paycheque go?",
            rarity = Rarity.Rare,
            isNegative = true
        };

        public static Patch Gastathon = new()
        {
            name = "Gastathon",
            description = "For every sub, he'll stream for another 5 minutes! It'll drain your internet, though.",
            rarity = Rarity.Rare,
            isNegative = true
        };

        public static Patch AllTheGlyphs = new()
        {
            name = "Hiero, Petro, and Astero",
            description = "Go back 3 nights.",
            rarity = Rarity.UltraRare
        };

        public static Patch AccountingError = new()
        {
            name = "Accounting Error",
            description = "Receive your retention bonus early!",
            rarity = Rarity.UltraRare
        };

        public static Patch ForgiveDebt = new()
        {
            name = "Forgive Debt",
            description = "After this night, your debt will be wiped.",
            rarity = Rarity.UltraRare
        };

        public static Patch NoNegative = new()
        {
            name = "No Negative",
            description = "Literally no downsides.",
            rarity = Rarity.UltraRare,
            isNegative = true
        };

        public static Patch AssetSeizure = new()
        {
            name = "Asset Seizure",
            description = "All your money is gone! You're going to have to work your way back up the legal way.",
            rarity = Rarity.UltraRare,
            isNegative = true
        };

        public static Patch MaxMode = new()
        {
            name = "Max Mode",
            description = "Dreaming and Gaster's AI level are maxed out.",
            rarity = Rarity.UltraRare,
            isNegative = true
        };

        public static Patch Aggressive = new()
        {
            name = "Aggressive",
            description = "Dreaming and Gaster's aggression are increased heavily (they'll have a higher chance of going to the Basement)",
            rarity = Rarity.UltraRare,
            isNegative = true
        };

        public static Patch DoubleNight = new()
        {
            name = "Double Night",
            description = "Yeah, the other person got laid off, you're gonna have to cover their shift.",
            rarity = Rarity.UltraRare,
            isNegative = true
        };

        public static Patch LaserPointer = new()
        {
            name = "Tunnel Vision",
            description = "Those eyes don't work like they used to!",
            rarity = Rarity.UltraRare,
            isNegative = true
        };
    }
}