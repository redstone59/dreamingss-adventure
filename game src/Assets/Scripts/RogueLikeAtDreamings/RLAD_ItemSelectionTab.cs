using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueLikeAtDreamings.Tabs
{  
    public class ItemSelectionTab : MonoBehaviour
    {
        public ItemButton[] selectionButtons;
        public RLADPlayer player;
        public int index;

        private void Start()
        {
            for (int i = 0; i < selectionButtons.Length; i++)
                selectionButtons[i].offset = i;
        }

        public void SetCallback(Action<RLAD_Items> func)
        {
            foreach (ItemButton button in selectionButtons)
            {
                button.UseItemCallback = (item) => {
                    func(item);
                    player.inventory.RemoveAt(index + button.offset);
                    Refresh();
                };
            }
        }

        public void Scroll(int delta)
        {
            int previousIndex = index;

            int max = Mathf.Max(0, player.inventory.Count - selectionButtons.Length);
            index += delta;
            index = Mathf.Clamp(index, 0, max);

            //if (max != 0 && index == previousIndex) return;

            for (int i = 0; i < selectionButtons.Length; i++)
            {
                if (index + i + 1 > player.inventory.Count)
                {
                    selectionButtons[i].visible = false;
                }
                else
                {
                    selectionButtons[i].visible = true;
                    selectionButtons[i].UpdateItem(player.inventory[index + i]);
                }
            }
        }

        public void Refresh() => Scroll(0);
    }
}