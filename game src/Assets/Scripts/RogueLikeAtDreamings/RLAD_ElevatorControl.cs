using System.Collections;
using RogueLikeAtDreamings.Elevator;
using RogueLikeAtDreamings.Rooms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueLikeAtDreamings.Tabs
{
    public class ElevatorControl : MonoBehaviour
    {
        public Elevator.Elevator elevator;
        public TextMeshProUGUI currentFloor;
        public Button upButton;
        public Button downButton;
        private bool _moving = false;

        public RLADAnimatronic _gaster;
        public RLADAnimatronic _dreaming;

        private bool Occupied()
        {
            return _gaster.CurrentRoom == Room.Elevator || _dreaming.CurrentRoom == Room.Elevator;
        }

        public void GoUp()
        {
            if (Occupied() || _moving || elevator.floor == Floor.Upper) return;
            elevator.floor = elevator.floor == Floor.Basement ? 
                                               Floor.Ground   : 
                                               Floor.Upper    ;
            UpdateText();
            StartCoroutine(StartCooldown());
        }

        public void GoDown()
        {
            if (Occupied() || _moving || elevator.floor == Floor.Basement) return;
            elevator.floor = elevator.floor == Floor.Upper    ?
                                               Floor.Ground   :
                                               Floor.Basement ;
            UpdateText();
            StartCoroutine(StartCooldown());
        }

        public void UpdateText()
        {
            currentFloor.text = $"{elevator.floor}";
        }

        public IEnumerator StartCooldown()
        {
            if (_moving) yield break;
            
            _moving = true;
            upButton.interactable = false;
            downButton.interactable = false;
            
            yield return new WaitForSecondsRealtime(3);
            
            _moving = false;
        }

        private void Update()
        {
            if (_moving) return;

            UpdateText();
            upButton.interactable = elevator.IdleTime >= 1 && !Occupied() && elevator.floor != Floor.Upper;
            downButton.interactable = elevator.IdleTime >= 1 && !Occupied() && elevator.floor != Floor.Basement;
        }

        public void Initialise()
        {
            UpdateText();
            _moving = false;
        }
    }
}