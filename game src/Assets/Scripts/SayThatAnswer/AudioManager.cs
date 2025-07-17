using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] correctLines;
    public AudioClip[] wrongLines;
    public AudioClip[] changeLines;
    public AudioClip[] introLines;
    public AudioClip[] transitionLines;
    public AudioClip[] firstQuestionLines;
    public AudioClip[] slowLines;

    public AudioClip[] backgroundMusic;

    private AudioSource audioSource;
    public AudioSource musicSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private T RandomSelection<T>(List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public void PlayRandomLine(AudioClip[] lines)
    {
        clip = RandomSelection(lines.ToList());
        Play();
    }

    public void Play() { audioSource.Play(); }
    public void Stop() { audioSource.Stop(); }

    public void PlayBackgroundMusic(int intensity = 0)
    {
        musicSource.clip = backgroundMusic[intensity];
        musicSource.Play();
    }

    public AudioClip clip { get {  return audioSource.clip; } set { audioSource.clip = value; } }
    public bool isPlaying { get { return audioSource.isPlaying; } }
}
