using System.Collections;
using System.Collections.Generic;
using Keys;
using UnityEngine;

public class Autoscroll : MonoBehaviour
{
    public Camera mainCamera;
    public Vector3 initialPosition;
    public Vector3 finalPosition;
    public Vector3 endPosition;
    public float panLength;
    public float jumpKingTimeLimit;
    public float timeElapsed;

    public Transform followTransform;
    private float highestMidpoint;
    public AudioSource music;

    public bool hasHitEnter = false;
    private bool _dreamingIsDead;
    private float _deathY;
    private float _deathTime;

    private bool speedrunModeActivated;
    private bool forceAutoscroll = false;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        timeElapsed = 0;
        speedrunModeActivated = PlayerPrefs.GetInt(PlayerPrefKeys.DontSaveProgress, 0) != 0 && !forceAutoscroll;
        highestMidpoint = speedrunModeActivated ? transform.position.y : finalPosition.y;
        _dreamingIsDead = false;
        _deathY = 0;
        _deathTime = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasHitEnter)
        {
            hasHitEnter = Input.GetKey(KeyCode.Return);
            return;
        }

        float percentage = timeElapsed / panLength;
        timeElapsed += Time.deltaTime;
        if (!_dreamingIsDead && (percentage > 1 || speedrunModeActivated))
        {
            UpdateMidpoint();
            if (!speedrunModeActivated)
            {
                float jumpKingPercentage = (timeElapsed - panLength) / jumpKingTimeLimit;
                Vector3 hurryUpPosition = Vector3.Lerp(finalPosition, endPosition, jumpKingPercentage);
                Vector3 newPosition = mainCamera.transform.position;
                newPosition.y = Mathf.Max(highestMidpoint, hurryUpPosition.y);
                mainCamera.transform.position = newPosition;
            }
            else
            {
                Vector3 newPosition = mainCamera.transform.position;
                newPosition.y = highestMidpoint;
                mainCamera.transform.position = newPosition;
            }
            return;
        }
        else if (_dreamingIsDead)
        {
            float songPercentage = music.isPlaying ? (music.time - _deathTime) / (music.clip.length - 30 - _deathTime) : 1;
            songPercentage = Mathf.Clamp01(songPercentage);
            mainCamera.transform.position = Vector3.Lerp(new(0, _deathY, -10), endPosition, songPercentage);

            bool canFastForward = music.time < music.clip.length - 35 && speedrunModeActivated && Input.GetKey(KeyCode.Space);
            music.pitch = canFastForward ? 10 : 1;
            if (speedrunModeActivated && Input.GetKeyDown(KeyCode.Space) && Input.GetKey(KeyCode.LeftControl))
            {
                music.time = music.clip.length - 35;
            }
            return;
        }
        mainCamera.transform.position = Vector3.Lerp(initialPosition, finalPosition, percentage);
    }

    private Vector3 GetCameraCentrePoint(float point)
    {
        return mainCamera.ScreenToWorldPoint(new(Screen.width / 2, Screen.height * point));
    }

    private void UpdateMidpoint()
    {
        float currentY = followTransform.gameObject.activeSelf ? followTransform.position.y : 0;
        float currentCentre = GetCameraCentrePoint(0.5f).y;
        if (currentY <= currentCentre) return;

        highestMidpoint = Mathf.Max(currentY, highestMidpoint);
    }

    public void OnDreamingDeath(float y)
    {
        _dreamingIsDead = true;
        _deathY = transform.position.y;
        _deathTime = timeElapsed;
        if (!music.isPlaying)
        {
            float percentComplete = (_deathY - initialPosition.y) / (endPosition.y - initialPosition.y);
            music.time = Mathf.Lerp(0, music.clip.length - 50, percentComplete);
            music.Play();
        }
    }
}
