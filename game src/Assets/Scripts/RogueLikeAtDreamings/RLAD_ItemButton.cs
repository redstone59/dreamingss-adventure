using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System;

namespace RogueLikeAtDreamings.Tabs
{
    public class ItemButton : MonoBehaviour
    {
        private Button _button;
        public TextMeshProUGUI ItemNameText;
        public TextMeshProUGUI AffectsRoomText;
        public Action<RLAD_Items> UseItemCallback;
        public int offset = 0;

        public bool visible
        {
            get { return gameObject.activeSelf; }
            set
            {
                ItemNameText.color = value ? Color.black : Color.clear;
                AffectsRoomText.color = value ? Color.black : Color.clear;
                gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            _button = GetComponent<Button>();
        }

        public void UpdateItem(RLAD_Items item)
        {
            string affects;
            (ItemNameText.text, affects) = GetItemLabels(item);
            AffectsRoomText.text = $"Affects: {affects}";

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(
                    () =>
                    {
                        UseItemCallback(item);
                        Debug.Log($"Used {item}.");
                    }
                );
            }
            else
            {
                Debug.Log($"Button for object {gameObject.name} under parent {transform.parent.name} is null! Reassigning...");
                _button = GetComponent<Button>();
            }
        }

        public (string name, string affects) GetItemLabels(RLAD_Items item)
        {
            return item switch {
                RLAD_Items.SingleBluray => ("A Single Bluray", "Basement"),
                RLAD_Items.BurnerPhone => ("Burner Phone", "Everywhere"),
                RLAD_Items.Fertiliser => ("Fertiliser", "Veranda"),
                RLAD_Items.CakePremix => ("Cake Premix", "Basement"),
                RLAD_Items.SpaghettiDelivery => ("Food Delivery", "Everywhere"),
                _ => throw new Exception($"Invalid item {item}")
            };
        }
    }
}