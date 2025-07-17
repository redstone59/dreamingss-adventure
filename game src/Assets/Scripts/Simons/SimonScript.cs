using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonScript : MonoBehaviour
{
    public GameObject[] colourImages;

    public AudioSource audioSource;
    public AudioClip[] beepNoises;

    public AudioClip badNoise;
    public AudioClip victoryNoise;

    public bool playing = true;

    private bool sequenceShown = false;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject obj in colourImages) { obj.GetComponent<SpriteRenderer>().color = Color.clear; }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!sequenceShown) { return; }


    }

    public void StartRound(int[] colours)
    {

        StartCoroutine(ShowSequence(colours));
    }

    private IEnumerator ShowSequence(int[] colours)
    {
        while (playing) { yield return null; }
        playing = true;
        foreach (int colour in colours)
        {
            colourImages[colour].GetComponent<SpriteRenderer>().color = Color.white;
            audioSource.clip = beepNoises[colour];
            audioSource.Play();

            while (audioSource.isPlaying) { yield return null; }
            yield return new WaitForSeconds(0.1f);

            colourImages[colour].GetComponent<SpriteRenderer>().color = Color.clear;

            while (audioSource.isPlaying) { yield return null; }
            yield return new WaitForSeconds(0.14f);
        }
        playing = false;
    }

    public void Flash(int colour)
    {
        StartCoroutine(FlashButton(colour));
    }

    private IEnumerator FlashButton(int colour)
    {
        while (playing) { yield return null; }
        playing = true;
        colourImages[colour].GetComponent<SpriteRenderer>().color = Color.white;
        audioSource.clip = beepNoises[colour];
        audioSource.Play();

        while (audioSource.isPlaying) { yield return null; }
        yield return new WaitForSeconds(0.1f);

        colourImages[colour].GetComponent<SpriteRenderer>().color = Color.clear;
        playing = false;
    }

    public void Celebrate()
    {
        StartCoroutine(CelebrateAnimation());
    }

    private IEnumerator CelebrateAnimation()
    {
        while (playing) { yield return null; }
        playing = true;
        audioSource.clip = victoryNoise;
        audioSource.Play();

        while (!audioSource.isPlaying) yield return null;

        while (audioSource.isPlaying)
        {
            for (int j = 0; j < colourImages.Length; j++)
            {
                colourImages[j].GetComponent<SpriteRenderer>().color = Color.white;
            }
            yield return new WaitForSeconds(0.25f);
            for (int j = 0; j < colourImages.Length; j++)
            {
                colourImages[j].GetComponent<SpriteRenderer>().color = Color.clear;
            }
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(0.1f);

        playing = false;
    }

    public void IncorrectHit()
    {
        StartCoroutine(IncorrectAnimation());
    }

    private IEnumerator IncorrectAnimation()
    {
        while (playing) { yield return null; }
        playing = true;
        audioSource.clip = badNoise;
        audioSource.Play();
        for (int j = 0; j < colourImages.Length; j++)
        {
            colourImages[j].GetComponent<SpriteRenderer>().color = Color.white;
        }

        while (audioSource.isPlaying) { yield return null; }

        for (int j = 0; j < colourImages.Length; j++)
        {
            colourImages[j].GetComponent<SpriteRenderer>().color = Color.clear;
        }

        playing = false;
    }
}