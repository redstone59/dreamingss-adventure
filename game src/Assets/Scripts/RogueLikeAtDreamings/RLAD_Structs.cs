using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using RogueLikeAtDreamings.Rooms;
using System.Collections;

namespace RogueLikeAtDreamings.Structs
{
    [Serializable]
    public struct RLAD_Time
    {
        public int currentTime;
        public int finishTime;
        public float defaultTimeRate;
        public float secondsPassed;
        public TMP_Text displayText;

        public readonly void UpdateDisplayText()
        {
            string hour = (currentTime % 12).ToString();
            if (hour == "0") hour = "12";
            bool afternoon = ((currentTime / 12) & 1) == 1;

            displayText.text = hour + (afternoon ? " PM" : " AM");
        }
    }

    [Serializable]
    public struct RLAD_Power
    {
        public float currentUsage;
        public float baseCapacity;
        public float powerUsed;
        public float passiveDrain;

        public TMP_Text displayText;

        public readonly void UpdateDisplayText(float maxCapacity)
        {
            float percentLeft = (1 - powerUsed / maxCapacity) * 100;
            displayText.text = $"Power: {percentLeft:N0}%";
        }
    }

    [Serializable]
    public struct RLAD_Map
    {
        public Sprite theatre;
        public Sprite noTheatre;
        public Image selector;
        public GameObject button;

        public RectTransform textTransform;
        public Vector3 textLocationNoTheatre;
        public Vector3 textLocationTheatre;
    }

    [Serializable]
    public struct RLAD_NightResults
    {
        public RLAD_NightResults(bool filler = true)
        {
            nightPayout = 0;
            challengePayout = null;
            lureSystemUses = 0;
            maintenanceCosts = 0;
            bonus = 0;
            debtInterest = 0;
        }

        public int nightPayout;
        public int? challengePayout;
        public int lureSystemUses;
        public int maintenanceCosts;
        public int bonus;
        public int debtInterest;
    }

    [Serializable]
    public struct RLAD_DuctTape
    {
        public bool grabbed;
        public int usesLeft;

        public Button useButton;
        public Button toggle;
        public TextMeshProUGUI toggleText;

        public void Toggle()
        {
            grabbed = !grabbed;
            useButton.gameObject.SetActive(grabbed);
            toggleText.text = grabbed ?
                              "put down duct tape" :
                              "pick up duct tape"  ;
        }
    }

    [Serializable]
    public struct DreamingState
    {
        public bool seekingKitchen;
        public bool hasDownload;
        public Room? targetRoom;
        public float lastAggression;
        public Room runAway;
        public bool hasHitContact;
    }

    [Serializable]
    public struct GasterState
    {
        public bool spaghettified;
    }

    [Serializable]
    public struct RLAD_Lure
    {
        public Button button;
        public TextMeshProUGUI cost;

        public readonly void SetActive(bool active)
        {
            button.gameObject.SetActive(active);
            cost.gameObject.SetActive(active);
        }

        public readonly void UpdateCost(int value)
        {
            cost.text = $"Replacement costs: ${value:N0}";
        }
    }

    [Serializable]
    public struct RLAD_SFX
    {
        public AudioSource bananaPeel;
        public AudioSource doorHit;

        public readonly IEnumerator HitDoor(int times)
        {
            float currentVolume = doorHit.volume;
            for (int i = 0; i < times; i++)
            {
                doorHit.volume = currentVolume * UnityEngine.Random.Range(0.9f, 1.1f);
                doorHit.Play();
                while (doorHit.isPlaying) yield return null;
            }
            doorHit.volume = currentVolume;
            yield return null;
        }
    }

    [Serializable]
    public struct RLAD_Ambience
    {
        public AudioSource main;
        public AudioSource danger;
    }

    [Serializable]
    public struct RLAD_FlashBeacon
    {
        public int uses;
        public int maxUses;
        public TextMeshProUGUI battery;

        public void Use()
        {
            uses++;
            battery.text = $"Battery: {(1 - (float)uses / maxUses) * 100:N0}%";
        }
    }
}