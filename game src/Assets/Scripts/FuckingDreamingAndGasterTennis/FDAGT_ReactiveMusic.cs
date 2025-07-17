using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FDAGT_ReactiveMusic : MonoBehaviour
{
    public float loudestSpeed;
    public float puckMaxVolume;

    public float musicMasterVolume;

    public AudioSource bonkNoise;
    public AudioSource pointJingle;
    public AudioSource victoryJingle;

    public AudioSource mainLoop;
    public AudioSource overtones;
    public AudioSource intensityBass;
    public AudioSource warningSiren;
    public AudioSource fastPuck;

    public float fastPuckThreshold;
    public float fastPuckMaximum;
    public bool fastPuckFading = false;
    public Rigidbody2D puck;

    // Start is called before the first frame update
    void Start()
    {
        mainLoop.volume = musicMasterVolume;
        pointJingle.volume = musicMasterVolume * 0.9f;
        victoryJingle.volume = musicMasterVolume * 0.9f;
        overtones.volume = 0;
        intensityBass.volume = 0;
        warningSiren.volume = 0;
        fastPuck.volume = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if (puck.velocity.magnitude > fastPuckThreshold)
        {
            float difference = Mathf.Min(puck.velocity.magnitude, fastPuckMaximum) - fastPuckThreshold;
            float newVolume = musicMasterVolume * Mathf.Clamp01(difference / (fastPuckMaximum - fastPuckThreshold));
            if (newVolume > fastPuck.volume)
            {
                fastPuckFading = false;
                fastPuck.volume = newVolume;
            }
        }
        else if (!fastPuckFading)
        {
            StartCoroutine(FastPuckFadeout());
        }
    }

    public void PlayBonkNoise(float speed)
    {
        bonkNoise.volume = Mathf.Clamp01(speed / loudestSpeed) * puckMaxVolume;
        bonkNoise.Play();
    }

    public void StopMusic()
    {
        mainLoop.Stop();
        overtones.Stop();
        intensityBass.Stop();
        warningSiren.Stop();
        fastPuck.Stop();
        bonkNoise.Stop();
    }

    public void PlayPointStinger()
    {
        StopMusic();
        pointJingle.Play();
    }

    public void PlayVictoryStinger()
    {
        StopMusic();
        victoryJingle.Play();
    }

    public void PlayMusic()
    {
        mainLoop.Play();
        overtones.Play();
        intensityBass.Play();
        warningSiren.Play();
        fastPuck.Play();
    }

    public IEnumerator FastPuckFadeout()
    {
        fastPuckFading = true;
        while (puck.velocity.magnitude <= fastPuckThreshold)
        {
            fastPuck.volume *= 0.95f;
            if (fastPuck.volume <= 1e-7)
            {
                fastPuck.volume = 0;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        fastPuckFading = false;
        yield return null;
    }
}
