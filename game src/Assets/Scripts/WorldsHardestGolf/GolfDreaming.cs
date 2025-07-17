using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Speedrun;
using UnityEngine;

public class GolfDreaming : MonoBehaviour
{
    public SpriteRenderer dreaming;
    public Rigidbody2D dreamingBody;
    public Camera mainCamera;
    public LevelManager levelManager;
    public SoundManager soundManager;
    public OverlayManager overlayManager;
    public PowerArrow powerArrow;

    private Vector3 initialMousePosition;
    private Vector3 finalMousePosition;
    
    private bool isDying;
    private bool inShot = false;
    private bool canMove = true;
    private float fadeOutTime;

    public float fadeOutLength = 1f;
    public float friction = 0.25f;
    public float maxForce = 500f;
    public float cursorScaleFactor = 75f;
    public float currentScale = 1;

    public SpeedrunTimer speedrunTimer;

    // Start is called before the first frame update
    void Start()
    {
        dreamingBody = GetComponent<Rigidbody2D>();
        isDying = false;
        speedrunTimer = GameObject.Find("Speedrun Timer").GetComponent<SpeedrunTimer>();
        speedrunTimer.Initialise("Beat the Game", "You don't know", "It gets harder", "Don't get scared, now", "Precision", "Last one");
        speedrunTimer.BeginTimer();
        StartCoroutine(TransitionCard());
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove && !isDying && dreamingBody.velocity == Vector2.zero)
        {
            if (!inShot && Input.GetMouseButtonDown(0))
            {
                initialMousePosition = Input.mousePosition;
                currentScale = 720f / Screen.height;
                inShot = true;
            }
            else if (inShot && Input.GetMouseButtonUp(0))
            {
                finalMousePosition = Input.mousePosition;
                Vector3 directionForce = finalMousePosition - initialMousePosition;
                directionForce *= currentScale / cursorScaleFactor;
                directionForce = Vector3.ClampMagnitude(directionForce, maxForce);
                dreamingBody.AddForce(directionForce, ForceMode2D.Impulse);
                dreaming.color = new Color(1, 0.2f, 0.2f, 1);
                soundManager.PlayBallHitSoundEffect();
                overlayManager.UpdateStrokeCount(++levelManager.strokes);
                powerArrow.Hide();
                inShot = false;
            }

            if (inShot)
            {
                bool cursorToLeft = Input.mousePosition.x - initialMousePosition.x < 0;
                powerArrow.SetPosition(dreaming.transform.position, (Input.mousePosition - initialMousePosition) * (currentScale / cursorScaleFactor), maxForce, cursorToLeft);
            }
        }
        else if (isDying)
        {
            fadeOutTime += Time.deltaTime;
            float fadePercentage = fadeOutTime / fadeOutLength;
            Color invisible = new(1, 1, 1, 0);
            dreaming.color = Color.Lerp(Color.white, invisible, fadePercentage);
            if (fadePercentage >= 1)
                ResetPlayer();
        }
    }

    void FixedUpdate()
    {
        float frictionalForce = friction;
        frictionalForce *= dreamingBody.velocity.magnitude;
        frictionalForce *= Time.fixedDeltaTime;
        dreamingBody.velocity = Vector2.MoveTowards(dreamingBody.velocity, Vector2.zero, frictionalForce);

        MoveCamera();

        if (dreamingBody.velocity.magnitude <= 0.1f) 
        { 
            dreamingBody.velocity = Vector2.zero;
            dreaming.color = Color.white;
        }
    }

    public void CollidedWithEnemy()
    {
        if (!isDying)
        {
            dreamingBody.velocity = Vector2.zero;
            fadeOutTime = 0f;
            isDying = true;
            soundManager.PlayDeadNoise();
            overlayManager.UpdateDeathCount(++levelManager.deaths);

            if (PlayerPrefs.GetInt("HardMode", 0) != 0)
            {
                levelManager.coins = 0;
                levelManager.ReloadSavedCoins();
            }
        }
    }

    public void CollidedWithCoin()
    {
        soundManager.PlayCoinNoise();
        levelManager.coins++;
    }

    public void CollidedWithExit()
    {
        if (levelManager.AllCoinsCollected())
        {
            speedrunTimer.Split();
            dreaming.transform.position = Vector3.zero;
            dreamingBody.velocity = Vector2.zero;
            //bool onHardMode = PlayerPrefs.GetInt("HardMode", 0) != 0;
            overlayManager.UpdateLevelCount(++levelManager.level + 1/*, onHardMode*/);
            int lastLevel = /*onHardMode ? */5/* : 3*/;
            if (levelManager.level >= lastLevel)
            {
                canMove = false;
                StartCoroutine(overlayManager.FinalScoreAnimation(levelManager.strokes,
                                                                  levelManager.deaths,
                                                                  20,
                                                                  2500,
                                                                  150000,
                                                                  soundManager
                                                                 )
                                                                );
                return;
            }
            StartCoroutine(TransitionCard());
            levelManager.LoadLevel();
        }
    }

    private void MoveCamera()
    {
        Vector3 newCameraPosition = dreaming.transform.position;
        newCameraPosition.z = -10;
        mainCamera.transform.position = newCameraPosition;
    }

    private void ResetPlayer()
    {
        StartCoroutine(DelayedReset());
    }

    IEnumerator DelayedReset()
    {
        dreaming.transform.position = Vector3.zero;
        dreaming.color = Color.white;

        yield return new WaitForFixedUpdate();

        fadeOutTime = 0f;
        isDying = false;
    }

    IEnumerator TransitionCard()
    {
        string levelString = levelManager.levelStrings[levelManager.level];
        bool isLastLevel = levelManager.level >= 4;
        canMove = false;
        
        soundManager.PlayNextLevelSound();
        overlayManager.ShowTransitionCard(levelString, isLastLevel);    

        yield return new WaitForSeconds(2);

        overlayManager.HideTransitionCard();
        canMove = true;
    }
}
