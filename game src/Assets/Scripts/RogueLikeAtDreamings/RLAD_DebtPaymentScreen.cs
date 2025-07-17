using RogueLikeAtDreamings.Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueLikeAtDreamings.DebtPaymentScreen
{
    public class RLAD_DebtPaymentScreen : MonoBehaviour
    {
        public RLADPlayer player;
        public Slider debtSlider;
        public TextMeshProUGUI oweText;
        public TextMeshProUGUI payText;

        public void Appear()
        {
            oweText.text = $"You owe the internet company:\n<size=72>${player.debt:N0}</size>";
            debtSlider.value = Mathf.RoundToInt(player.debt * (6 / 5f));
            debtSlider.maxValue = Mathf.Min(player.debt, player.money);
        }

        public void Pay()
        {
            player.debt -= Mathf.FloorToInt(debtSlider.value);
            player.money -= Mathf.FloorToInt(debtSlider.value);
            gameObject.SetActive(false);
        }

        void Update()
        {
            payText.text = $"Paying ${debtSlider.value:N0}";
        }
    }
}