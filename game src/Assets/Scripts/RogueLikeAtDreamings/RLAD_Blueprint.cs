using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using RogueLikeAtDreamings.Rooms;

public class RLADBlueprint : MonoBehaviour
{
    public RectTransform rectTransform;
    public AudioSource audioSource;
    public AudioClip swipeInNoise;
    public AudioClip swipeOutNoise;

    public Vector3 offScreenPos;
    public Vector3 offToTheRightPos;
    public Vector3 onScreenPos;
    public float swipeLength;

    public bool hasSelectedRoom;
    public Room selectedRoom;

    public Texture[] handwrittenItemNames;
    public RawImage handwrittenItemName;

    public GameObject theatreButton;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        hasSelectedRoom = false;
        selectedRoom = Room.LivingRoom;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        StartCoroutine(SwipeAnimation(offScreenPos, 
                                      onScreenPos, 
                                      (x) => Mathf.Pow(x - 1, 3) + 1,
                                      swipeInNoise
                                      )
                       );
    }

    public void SetSelectedRoom(int index)
    {
        Room[] allRoom = Enum.GetValues(typeof(Room))
                               .Cast<Room>()
                               .ToArray();
        Debug.Log(Enum.GetNames(typeof(Room))[index]);
        selectedRoom = allRoom[index];
        hasSelectedRoom = true;
    }

    [ContextMenu("Swipe Out")]
    public void SwipeOut()
    {
        StartCoroutine(SwipeAnimation(onScreenPos,
                                      offToTheRightPos,
                                      (x) => -x * (x - 2),
                                      swipeOutNoise
                                      )
                       );
    }

    private IEnumerator SwipeAnimation(Vector3 inital, Vector3 final, Func<float, float> animationCurve, AudioClip sound = null)
    {
        int lengthInPframes = (int)(swipeLength / Time.fixedDeltaTime);
        if (sound != null)
        {
            audioSource.clip = sound;
            audioSource.Play();
        }

        for (int pframesElapsed = 0;
             pframesElapsed < lengthInPframes;
             pframesElapsed++
             )
        {
            float x = pframesElapsed / (float)lengthInPframes;
            rectTransform.localPosition =
                Vector3.Lerp(inital, 
                             final, 
                             animationCurve(x)
                             );
            yield return new WaitForFixedUpdate();
        }
        rectTransform.localPosition = final;
    }

    public void SelectRoom(RLADPlayer player, TemporaryUpgradeTypes type)
    {
        theatreButton.SetActive(player.permanentUpgrades.hasHomeTheatre);
        handwrittenItemName.texture = handwrittenItemNames[(int)type];
        gameObject.SetActive(true);
        StartCoroutine(WaitUntilRoomSelected(player, type));
    }

    private IEnumerator WaitUntilRoomSelected(RLADPlayer player, TemporaryUpgradeTypes type)
    {
        while (!hasSelectedRoom)
            yield return null;
        
        //switch (type)
        //{
            //case TemporaryUpgradeTypes.BananaPeel:
            //    player.temporaryUpgrades.bananaPeel = selectedRoom;
            //    break;
        //}

        hasSelectedRoom = false;
        SwipeOut();
        yield return new WaitForSeconds(swipeLength);
        gameObject.SetActive(false);
    }

    public bool isActive { get { return gameObject.activeSelf; } }
}