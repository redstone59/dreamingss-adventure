using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public QuestionManager questionManager;
    public AudioManager audioSource;
    public VideoPlayer openerVideo;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = questionManager.audioSource;
        StartCoroutine(PlayOpenerVideo());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator PlayOpenerVideo()
    {
        openerVideo.Play();
        yield return new WaitForSeconds(1f);
        while (openerVideo.isPlaying) yield return null;
        
        audioSource.PlayRandomLine(questionManager.contestant.announcerLines.ToArray());
        yield return new WaitForSeconds(0.1f);
        
        openerVideo.gameObject.SetActive(false);
        while (audioSource.isPlaying) yield return null;

        audioSource.PlayRandomLine(audioSource.firstQuestionLines);
        yield return new WaitForSeconds(0.1f);
        while (audioSource.isPlaying) yield return null;

        questionManager.PlayRandomQuestion();
    }
}
