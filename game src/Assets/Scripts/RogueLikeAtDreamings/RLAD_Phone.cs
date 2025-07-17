using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RLADPhone : MonoBehaviour
{
    public RLADPlayer player;

    public RectTransform rectTransform;

    public AudioSource swipeSource;
    public AudioClip swipeInNoise;
    public AudioClip swipeOutNoise;
    
    public Button button; 
    public AudioSource beepSource;
    
    public Vector3 offScreenPos;
    public Vector3 onScreenPos;
    public float swipeLength;

    public bool hasSelectedContact;
    //public RLAD_Contacts selectedContact;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        hasSelectedContact = false;
        //selectedContact = RLAD_Contacts.Amy;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        StartCoroutine(SwipeAnimation(offScreenPos,
                                      onScreenPos,
                                      (x) => -x * (x - 2),
                                      swipeInNoise
                                      )
                       );
    }

    private IEnumerator SwipeAnimation(Vector3 inital, Vector3 final, Func<float, float> animationCurve, AudioClip sound = null)
    {
        int lengthInPframes = (int)(swipeLength / Time.fixedDeltaTime);
        if (sound != null)
        {
            swipeSource.clip = sound;
            swipeSource.Play();
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

    public void SelectContact()
    {
        gameObject.SetActive(true);
        StartCoroutine(WaitUntilContactSelected());
    }

    private IEnumerator WaitUntilContactSelected()
    {
        while (!hasSelectedContact)
            yield return null;
        
        //player.temporaryUpgrades.comingOver = selectedContact;
        StartCoroutine(SwipeAnimation(onScreenPos,
                                      offScreenPos,
                                      (x) => -x * (x - 2),
                                      swipeOutNoise)
                       );

        hasSelectedContact = false;
        yield return new WaitForSeconds(swipeLength + 2);
        gameObject.SetActive(false);
    }

    public void SetSelectedRoom(int index)
    {
        //RLAD_Contacts[] allRLAD_Contacts = Enum.GetValues(typeof(RLAD_Contacts))
        //                             .Cast<RLAD_Contacts>()
        //                             .ToArray();
        //Debug.Log(Enum.GetNames(typeof(RLAD_Contacts))[index]);
        //selectedContact = allRLAD_Contacts[index];
        hasSelectedContact = true;
    }

    public bool isActive { get { return gameObject.activeSelf; } }
}