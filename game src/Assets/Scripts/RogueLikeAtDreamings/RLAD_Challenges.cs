using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RogueLikeAtDreamings;
using RogueLikeAtDreamings.Tooltips;

namespace RogueLikeAtDreamings
{
    public class Challenge : MonoBehaviour
    {
        public TextMeshProUGUI patches;
        public TextMeshProUGUI rarityText;
        public TooltipInformation tooltip;
        public Toggle checkbox;
        public Patch positive;
        public Patch negative;

        public string[] rarityStrings;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            rarityStrings = new[]
            {
                "Common",
                "<color=blue>Rare</color>",
                "<color=red><i>Ultra Rare!!</i></color>"
            };            
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            tooltip.attached = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            
        }

        [ContextMenu("Reroll Challenge")]
        public void SelectRandomChallenge(List<Patch> excluded)
        {
            float roll = UnityEngine.Random.Range(0f, 1f);
            Rarity rarity = roll <= 0.02f
                              ? Rarity.UltraRare
                              : roll <= 0.10f
                                  ? Rarity.Rare
                                  : Rarity.Common;
            rarityText.text = rarityStrings[(int)rarity];

            List<Patch> patchesOfThisRarity = (from patch in Patches.GetAllPatches() 
                                               where patch.rarity == rarity 
                                               select patch).ToList();
            foreach (Patch excludedPatch in excluded)
            {
                patchesOfThisRarity.Remove(excludedPatch);
            }

            List<Patch> positivePatches = (from patch in patchesOfThisRarity
                                           where !patch.isNegative
                                           select patch).ToList();

            List<Patch> negativePatches = (from patch in patchesOfThisRarity
                                           where patch.isNegative
                                           select patch).ToList();

            positive = positivePatches[UnityEngine.Random.Range(0, positivePatches.Count)];
            negative = negativePatches[UnityEngine.Random.Range(0, negativePatches.Count)];

            patches.text = $"{positive.name}\n<color=red>{negative.name}</color>";
            tooltip.text = $"{positive.description}\n<color=red>{negative.description}</color>";
        }
    }
}