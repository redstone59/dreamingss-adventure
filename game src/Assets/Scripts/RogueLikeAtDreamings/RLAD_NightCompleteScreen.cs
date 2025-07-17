using System.Collections;
using RogueLikeAtDreamings.Structs;
using TMPro;
using UnityEngine;

namespace RogueLikeAtDreamings.NightCompleteScreen
{
    public class RLAD_NightCompleteScreen : MonoBehaviour
    {
        public TextMeshProUGUI timeText;
        public TextMeshProUGUI bonusText;
        public TextMeshProUGUI totalPayoutText;

        public AudioSource bells;
        public GameObject shopButtonObject;

        public RLADShop shop;

        public void Awake()
        {
            timeText.text = "";
            bonusText.text = "";
            totalPayoutText.text = "Total payout: $75";
            shopButtonObject.SetActive(false);
        }

        public void Update() {}

        public IEnumerator PayoutAnimation(RLAD_Time clock, RLADPlayer player, RLAD_NightResults results, int actualPayout, int nightlyPay)
        {
            bells.Play();
            bonusText.text = "";
            totalPayoutText.color = Color.clear;
            shopButtonObject.SetActive(false);
            clock.currentTime--;
            clock.UpdateDisplayText();
            timeText.text = clock.displayText.text;

            yield return new WaitForSeconds(1f);
            
            while (timeText.text.Length > 0)
            {
                timeText.text = timeText.text[..^1];
                Debug.Log(timeText.text);
                yield return new WaitForSeconds(0.5f);
            }

            clock.currentTime++;
            clock.UpdateDisplayText();
            string finalTime = clock.displayText.text;

            foreach (char c in finalTime)
            {
                timeText.text += c;
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(1f);

            bonusText.text += $"Paycheque - ${nightlyPay}\n";
            yield return new WaitForSeconds(1f);

            if (results.challengePayout != null)
            {
                string challengeName = player.currentChallenge.positive.name;
                string amount = $"{((int)results.challengePayout < 0 ? "-" : "")}${Mathf.Abs((int)results.challengePayout)}";
                bonusText.text += $"{challengeName} - {amount}\n";
                yield return new WaitForSeconds(1f);
            }

            if (results.bonus != 0)
            {
                bonusText.text += $"Retention Bonus - ${results.bonus}\n";
                yield return new WaitForSeconds(1f);
            }

            if (results.maintenanceCosts != 0)
            {
                bonusText.text += $"Maintenance costs - -${results.maintenanceCosts}\n";
                yield return new WaitForSeconds(0.5f);
            }

            if (results.lureSystemUses != 0)
            {
                bonusText.text += $"Blu-ray replacement costs - -${10 * results.lureSystemUses}\n";
                yield return new WaitForSeconds(0.5f);
            }

            if (results.debtInterest != 0)
            {
                bonusText.text += $"Your debt has accrued ${results.debtInterest} of interest.";
                yield return new WaitForSeconds(0.5f);
            }

            if (player.nightsCompleted % 5 == 0)
            {
                bonusText.text += "You've received a raise!\n";
                yield return new WaitForSeconds(1f);
            }

            totalPayoutText.text = $"Total payout: ${actualPayout}";
            totalPayoutText.color = Color.white;

            yield return new WaitForSeconds(1);

            shopButtonObject.SetActive(true);
            yield return null;
        }

        public void OpenShop()
        {
            shop.gameObject.SetActive(true);
            shop.Refresh();
        }
    }
}