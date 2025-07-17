using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using RogueLikeAtDreamings.Structs;
using RogueLikeAtDreamings.Rooms;

[Serializable]
public struct AnimatronicPositions
{
    public GameObject far;
    public GameObject close;

    public readonly void SetFar()
    {
        close.SetActive(false);
        far.SetActive(true);
    }

    public readonly void SetClose()
    {
        close.SetActive(true);
        far.SetActive(false);
    }

    public readonly void SetHidden()
    {
        close.SetActive(false);
        far.SetActive(false);
    }
}

public class RLAD_Office : MonoBehaviour
{
    public float position = 0.5f;
    public float cameraMoveSpeed = 1.8f;
    public float _flashBeaconLength = 0.5f;
    public Func<bool> FlashCallback;

    public Image officeImage;
    public Image _darkness;

    public RectTransform lookLeft;
    public RectTransform lookRight;
    public RLAD_Camera cameraObject;

    public bool canOpenCamera;

    public AnimatronicPositions dreamingPositions;
    public AnimatronicPositions gasterPositions;

    public RLADAnimatronic _dreaming;
    public RLADAnimatronic _gaster;

    public Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        officeImage.transform.localPosition = new(Mathf.Lerp(80, -80, position), 0, 0);
        if (Input.GetKeyDown(KeyCode.Space)) FlashBeacon();
    }

    public void UpdateAnimatronicPositions(RLADAnimatronic dreaming, RLADAnimatronic gaster)
    {
        if (dreaming.CurrentRoom == Room.Basement_Far)
            dreamingPositions.SetFar();
        else if (dreaming.CurrentRoom == Room.Basement_Close)
            dreamingPositions.SetClose();
        else
            dreamingPositions.SetHidden();
        
        if (gaster.CurrentRoom == Room.Basement_Far)
            gasterPositions.SetFar();
        else if (gaster.CurrentRoom == Room.Basement_Close)
            gasterPositions.SetClose();
        else
            gasterPositions.SetHidden();
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        Vector2 currentMousePos = Input.mousePosition;

        if (cameraObject.active)
            return;

        if (RectTransformContainsPoint(lookLeft, currentMousePos))
            position -= cameraMoveSpeed * Time.fixedDeltaTime;
        
        if (RectTransformContainsPoint(lookRight, currentMousePos))
            position += cameraMoveSpeed * Time.fixedDeltaTime;
        
        position = Mathf.Clamp01(position);
    }

    private void FlashBeacon()
    {
        if (_darkness.color.a != 1 || cameraObject.active == true) return;
        if (!FlashCallback()) return;
        UpdateAnimatronicPositions(_dreaming, _gaster);
        StartCoroutine(FlashAnimation());
    }

    private IEnumerator FlashAnimation()
    {
        for (float timeElapsed = 0; timeElapsed < _flashBeaconLength; timeElapsed += Time.deltaTime)
        {
            Color color = new(1, 1, 1, Mathf.Lerp(0, 1, Mathf.Pow(timeElapsed / _flashBeaconLength, 0.5f)));
            _darkness.color = color;
            yield return null;
        }
        _darkness.color = Color.white;
    }

    private bool RectTransformContainsPoint(RectTransform rect, Vector3 point)
    {
        Vector2 localPosition = rect.InverseTransformPoint(point);
        return rect.rect.Contains(localPosition);
    }

    public void EnableCamera()
    {
        if (!canOpenCamera) return;
        cameraObject.active = true;
    }
}
