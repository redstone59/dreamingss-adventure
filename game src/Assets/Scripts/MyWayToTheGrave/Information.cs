using System;
using System.Collections;
using System.Collections.Generic;
using MyWayToTheGrave.Scoring;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct InformationText
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI artist;
    public TextMeshProUGUI coverers;

    public Color color
    {
        set
        {
            title.color = value;
            artist.color = value;
            coverers.color = value;
        }
    }
}

public class Information : MonoBehaviour
{
    public InformationText text;
    public RawImage albumImage;
    public RawImage fadeBackground;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI gradeText;
    public AudioSource thud;

    public List<string> possibleGrades;
    public List<Color> gradeGlows;
    public Material gradeMaterial;

    private Color clearButWhite = new(1, 1, 1, 0);

    private bool _dreamingMode = false;
    public bool DreamingMode
    {
        get { return _dreamingMode; }
        set { _dreamingMode = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        text.color = clearButWhite;
        albumImage.color = clearButWhite;
        fadeBackground.color = Color.black;
        finalScoreText.text = "";
        gradeText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Show Information")]
    public void ShowInformation()
    {
        string ScrubEvidence(string evidence)
        {
            evidence = evidence.Replace("redstone59 and", "");
            evidence = evidence.Replace("redstone59 &", "");
            evidence = evidence.Replace("redstone59, ", "");
            evidence = evidence.Replace("redstone59,", "");
            evidence = evidence.Replace("redstone59", "");
            evidence = evidence.Replace("  ", " ");

            return evidence;
        }

        if (_dreamingMode)
        {
            text.artist.text = ScrubEvidence(text.artist.text);
            text.coverers.text = ScrubEvidence(text.coverers.text);
            text.title.text = ScrubEvidence(text.title.text);
        }
        StartCoroutine(InformationAnimation());
    }

    private IEnumerator InformationAnimation()
    {
        // Fade in
        float timeElapsed = 0;
        while (timeElapsed <= 2)
        {
            Color lerpColour = Color.Lerp(clearButWhite, Color.white, timeElapsed / 2);
            text.color = lerpColour;
            albumImage.color = lerpColour;
            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime;
        }

        yield return new WaitForSeconds(4f);

        timeElapsed = 0;

        while (timeElapsed <= 3)
        {
            Color lerpColour = Color.Lerp(Color.white, clearButWhite, timeElapsed / 3);
            text.color = lerpColour;
            albumImage.color = lerpColour;
            fadeBackground.color = Color.Lerp(Color.black, Color.clear, timeElapsed / 3);
            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime;
        }
    }

    public void ScoreAnimation(SongStats stats, int score, string grade)
    {
        StartCoroutine(EndAnimation(stats, score, grade));
    }

    private IEnumerator EndAnimation(SongStats stats, int score, string grade)
    {
        float timeElapsed = 0;
        while (timeElapsed <= 1)
        {
            Color lerpColour = Color.Lerp(clearButWhite, Color.white, timeElapsed);
            text.color = lerpColour;
            albumImage.color = lerpColour;
            fadeBackground.color = Color.Lerp(Color.clear, Color.black, timeElapsed);
            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime;
        }

        yield return new WaitForSeconds(1f);

        finalScoreText.text += $"Notes hit: {stats.noteHeadsHit:N0} / {stats.totalNoteHeads:N0} ({100 * stats.noteHeadsHit / (float)stats.totalNoteHeads:N2}%)\n";
        thud.Play();
        yield return new WaitForSeconds(0.5f);
        finalScoreText.text += $"Average pitch accuracy: {100 * stats.summedPitchAccuracy / (float)stats.totalNoteHeads:N2}%\n";
        thud.Play();
        yield return new WaitForSeconds(0.5f);
        finalScoreText.text += $"Overtaps: {stats.overtaps:N0}\n";
        thud.Play();
        yield return new WaitForSeconds(0.5f);
        finalScoreText.text += $"\nTotal Score: {score:N0}";
        thud.Play();
        yield return new WaitForSeconds(1);

        Color glowColour = possibleGrades.Contains(grade.ToUpper()) ? gradeGlows[possibleGrades.IndexOf(grade.ToUpper())] : Color.clear;
        gradeMaterial.SetColor(Shader.PropertyToID("_GlowColor"), glowColour);
        gradeText.text = grade.ToUpper();

        timeElapsed = 0;
        while (timeElapsed <= 1)
        {
            gradeText.transform.localScale = Vector3.one * Mathf.Lerp(2.5f, 1, timeElapsed);
            gradeText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, timeElapsed));
            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime;
        }
        gradeText.transform.localScale = Vector3.one;
        gradeText.color = Color.white;

        yield return new WaitForSeconds(2f);
    }
}
