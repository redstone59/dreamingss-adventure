using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource soundEffects;
    public AudioClip[] hitNoises;
    public AudioClip deadNoise;
    public AudioClip coinNoise;
    public AudioClip nextLevelSound;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PlayBallHitSoundEffect()
    {
        int index = UnityEngine.Random.Range(0, hitNoises.Length);
        soundEffects.clip = hitNoises[index];
        soundEffects.Play();
    }

    public void PlayCoinNoise()
    {
        if (soundEffects.isPlaying)
        {
            if (soundEffects.clip == coinNoise)
                return;
            soundEffects.Stop();
        }
        soundEffects.clip = coinNoise;
        soundEffects.Play();
    }

    public void PlayDeadNoise()
    {
        soundEffects.Stop();
        soundEffects.clip = deadNoise;
        soundEffects.Play();
    }

    public void PlayNextLevelSound()
    {
        soundEffects.Stop();
        soundEffects.clip = nextLevelSound;
        soundEffects.Play();
    }
}
