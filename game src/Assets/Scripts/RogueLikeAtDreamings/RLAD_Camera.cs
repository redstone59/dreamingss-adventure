using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using RogueLikeAtDreamings.Rooms;

[Serializable]
public struct CameraData
{
    public Sprite roomTexture;
    public string name;

    public Vector3 dreamingPosition;
    public float dreamingScale;

    public Vector3 gasterPosition;
    public float gasterScale;
}

public class RLAD_Camera : MonoBehaviour
{
    public RLADAnimatronic dreaming;
    public Image dreamingImage;

    public RLADAnimatronic gaster;
    public Image gasterImage;

    public int cameraPanLoopIndex = 0;
    public int cameraPanLoopLength;
    public float leftmostCameraX;
    
    public RectTransform currentCamTransform;
    public Image currentCameraImage;
    public TextMeshProUGUI cameraRoomText;

    public CameraData[] rooms;
    private int currentRoom;
    public Room SelectedRoom { get; private set; }

    public GameObject _firstFloor;
    public Button _firstFloorToggle;
    public GameObject _secondFloor;
    public Button _secondFloorToggle;

    private Room _lastUpperRoom;
    private Room _lastLowerRoom;

    public Image transparentStatic;
    public Sprite[] transparentStaticFrames;
    public RogueLikeAtDreamings.Animation transparentStaticAnimation;

    public Image heavyStatic;
    public Sprite[] heavyStaticFrames;
    public RogueLikeAtDreamings.Animation heavyStaticAnimation;

    public AudioSource staticNoise;
    public float staticVolume;

    public Sprite verandaLongGrassImage;
    public bool fertilisedVeranda;

    public bool disabled;

    public Image hideCamera;

    void Awake()
    {
        currentRoom = (int)Room.DreamingsRoom;
        SelectedRoom = Room.DreamingsRoom;
        transparentStaticAnimation = new(
            transparentStatic,
            transparentStaticFrames,
            1f / 60
        );
        heavyStaticAnimation = new(
            heavyStatic,
            heavyStaticFrames,
            1 / 60f
        );
        staticNoise.mute = true;
    }

    // Update is called once per frame
    void Update()
    {
        staticNoise.mute = !active;
        if (!active) return;
        if (!transparentStaticAnimation.isPlaying)
        {
            StartCoroutine(transparentStaticAnimation.PlayLoop(new(1, 1, 1, .18f)));
        }
        bool dreamingInRoom = currentRoom == (int)dreaming.CurrentRoom;
        bool gasterInRoom = currentRoom == (int)gaster.CurrentRoom;
        bool cameraOnBathroom = currentRoom == (int)Room.Bathroom ||
                                currentRoom == (int)Room.Ensuite;
        if (disabled || (cameraOnBathroom && (dreamingInRoom || gasterInRoom)))
        {
            hideCamera.color = Color.white;
        }
        else
        {   
            hideCamera.color = Color.clear;
            dreamingImage.gameObject.SetActive(dreamingInRoom);
            gasterImage.gameObject.SetActive(gasterInRoom);
        }
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        UpdateCameraPan();
    }

    public bool active
    {
        get { return transform.localScale == Vector3.one; }
        set
        {
            if (value) ChangeCurrentCamera(currentRoom);
            transform.localScale = value ? Vector3.one : Vector3.zero;
        }
    }

    private void UpdateCameraPan()
    {
        cameraPanLoopIndex = (cameraPanLoopIndex + 1) % cameraPanLoopLength;
        
        Vector3 nextPosition = currentCamTransform.anchoredPosition;
        
        float lerpIndex = cameraPanLoopIndex % (cameraPanLoopLength / 2);
        lerpIndex /= cameraPanLoopLength / 2;

        if (cameraPanLoopIndex >= cameraPanLoopLength / 2)
            nextPosition.x = 720 - Mathf.Lerp(0, 160, lerpIndex);
        else
            nextPosition.x = 720 - Mathf.Lerp(0, 160, 1 - lerpIndex);

        currentCamTransform.anchoredPosition = nextPosition;
    }

    public void ChangeCurrentCamera(int cameraIndex)
    {
        CameraData roomData = rooms[cameraIndex];
        currentRoom = cameraIndex;
        SelectedRoom = (Room)cameraIndex;
        
        currentCameraImage.sprite = roomData.roomTexture;
        if (SelectedRoom == Room.Veranda && fertilisedVeranda)
            currentCameraImage.sprite = verandaLongGrassImage;
        cameraRoomText.text = roomData.name;

        dreamingImage.transform.localPosition = roomData.dreamingPosition;
        dreamingImage.transform.localScale = Vector3.one * roomData.dreamingScale;
        dreamingImage.gameObject.SetActive((int)dreaming.CurrentRoom == cameraIndex);

        gasterImage.transform.localPosition = roomData.gasterPosition;
        gasterImage.transform.localScale = Vector3.one * roomData.gasterScale;
        gasterImage.gameObject.SetActive((int)gaster.CurrentRoom == cameraIndex);
    }

    public void ChangeFloor()
    {
        if (_firstFloor.activeSelf)
        {
            _firstFloor.SetActive(false);
            _secondFloor.SetActive(true);
            _firstFloorToggle.interactable = true;
            _secondFloorToggle.interactable = false;

            _lastUpperRoom = SelectedRoom;
            ChangeCurrentCamera((int)_lastLowerRoom);
        }
        else
        {
            _firstFloor.SetActive(true);
            _secondFloor.SetActive(false);
            _firstFloorToggle.interactable = false;
            _secondFloorToggle.interactable = true;

            _lastLowerRoom = SelectedRoom;
            ChangeCurrentCamera((int)_lastUpperRoom);
        }
    }

    public void ResetFloor()
    {
        _firstFloorToggle.interactable = false;
        _secondFloorToggle.interactable = false;
        _firstFloor.SetActive(true);
        ChangeFloor();
        _lastUpperRoom = Room.DreamingsRoom;
        _lastLowerRoom = Room.LivingRoom;
        ChangeCurrentCamera((int)Room.DreamingsRoom);
    }

    public void HeavyStatic(params Room[] affectedRooms)
    {
        bool hit = false;
        foreach (Room room in affectedRooms)
        {
            if (room == SelectedRoom)
            {
                hit = true;
                break;
            }
        }
        if (!hit) return;
        
        heavyStaticAnimation.isPlaying = false;
        staticNoise.mute = false;
        staticNoise.volume = staticVolume;
        StartCoroutine(heavyStaticAnimation.PlayLoop(Color.white));
        StartCoroutine(HeavyStaticFadeout());
        ChangeCurrentCamera(currentRoom);
    }

    private IEnumerator HeavyStaticFadeout()
    {
        yield return new WaitForSeconds(0.7f);

        float staticLength = 0.5f + UnityEngine.Random.Range(-.2f, .2f);
        for (float timeElapsed = 0; timeElapsed < staticLength; timeElapsed += Time.deltaTime)
        {
            heavyStatic.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), timeElapsed / staticLength);
            staticNoise.volume = staticVolume * (1 - timeElapsed / staticLength);
            yield return null;
        }

        heavyStaticAnimation.isPlaying = false;
        staticNoise.mute = true;
    }
}