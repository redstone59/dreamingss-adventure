using UnityEngine;
using UnityEngine.UI;

using RogueLikeAtDreamings;

namespace RogueLikeAtDreamings.Tabs
{
    public class Tabs : MonoBehaviour
    {
        public GameObject[] tabObjects;
        public Button[] buttons;
        
        public ElevatorControl elevatorControl;
        public ItemSelectionTab itemSelection;

        private void Start()
        {
            ChangeTab(0);
            elevatorControl.Initialise();
        }

        public void ChangeTab(int index)
        {
            for (int i = 0; i < tabObjects.Length; i++)
            {
                tabObjects[i].transform.localScale = Vector3.zero;
                buttons[i].interactable = true;
            }
            tabObjects[index].transform.localScale = Vector3.one * (index == 0 ? 0.5f : 0.4f);
            buttons[index].interactable = false;

            elevatorControl.UpdateText();
            itemSelection.Refresh();
        }
    }
}