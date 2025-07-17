using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AnswerManager : MonoBehaviour
{
    public TMP_InputField answerTextBox;
    
    public string[] validAnswers;
    public string[] invalidAnswers;
    public float animationTime;
    public bool isEnabled = false;
    private bool isActive = false;
    
    private float hiddenPositionY = -175f;
    private Vector3 adjustToCentre = new(640, 360, 0);
    // Start is called before the first frame update
    void Start()
    {
        Vector3 startPosition = Vector3.zero;
        startPosition.y = hiddenPositionY;
        answerTextBox.transform.position = startPosition + adjustToCentre;
        answerTextBox.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        adjustToCentre = new(Screen.width / 2, Screen.height / 2);
        hiddenPositionY = -175f * (Screen.height / 720f);
//      if (isEnabled) answerTextBox.Select();
    }

    private int LevenshteinDistance(string a, string b)
    {
        /* Using two matrix rows approach
         * See https://en.wikipedia.org/wiki/Levenshtein_distance#Iterative_with_two_matrix_rows
         */
        int m = a.Length;
        int n = b.Length;

        int[] previousRow = new int[n + 1];
        int[] currentRow = new int[n + 1];

        for (int i = 0; i <= n; i++)
            previousRow[i] = i;

        for (int i = 0; i < m; i++)
        {
            currentRow[0] = i + 1;

            for (int j = 0; j < n;  j++)
            {
                int deletionCost = previousRow[j + 1];
                int insertionCost = currentRow[j];
                int substitutionCost;
                
                if (a[i] == b[i])
                    substitutionCost = previousRow[j] - 1;
                else
                    substitutionCost = previousRow[j];
                
                currentRow[j + 1] = 1 + Mathf.Min(deletionCost, insertionCost, substitutionCost);
            }
            
            Array.Copy(currentRow, previousRow, n + 1);
        }

        return currentRow[n];
    }

    public bool ContainsAnswer(string inputString, string[] validAnswers, string[] invalidAnswers = null)
    {
        inputString = $"          {inputString}          "; // pad some shit to unfuck some stuff. i cant be bothered to explain it

        int forLoopCount = 0;
        int forLoopLimit = 10000;
        foreach (string answer in validAnswers)
        {
            int tolerance = answer.Length switch {
                <= 2 => 0,
                <= 5 => 1,
                <= 8 => 2,
                _ => 3
            };
            for (int i = 0; i < Mathf.Abs(inputString.Length - answer.Length); i++)
            {
                string slicedString = inputString.Substring(i, answer.Length);
                if (LevenshteinDistance(slicedString, answer) <= tolerance)
                {
                    if (invalidAnswers != null && ContainsAnswer(inputString, invalidAnswers)) continue;
                    return true;
                }
                if (forLoopCount++ >= forLoopLimit) break;
            }
            if (forLoopCount >= forLoopLimit) break;
        }
        return false;
    }

    public string CreateBadAnswer(string answer, int substitutions)
    {
        char[] badChars = " !@#$%^&*\"".ToCharArray();
        List<int> indices = Enumerable.Range(0, answer.Length).ToList();
        indices = Shuffle(indices);
        char[] badAnswer = answer.ToCharArray();
        for (int i = 0; i < substitutions; i++)
        {
            int answerIndex = indices[0];
            indices.RemoveAt(0);
            
            int badCharIndex = UnityEngine.Random.Range(0, badChars.Length);
            badAnswer[answerIndex] = badChars[badCharIndex];
        }
        return new string(badAnswer);
    }

    private List<T> Shuffle<T>(List<T> list)
    {
        List<T> shuffledList = new List<T>();
        List<T> remaining = list;
        while (remaining.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, remaining.Count);
            shuffledList.Add(remaining[index]);
            remaining.RemoveAt(index);
        }
        return shuffledList;
    }

    [ContextMenu("Enable Input Box")]
    public void EnableInputBox(bool enableTextBox = true)
    {
        if (isActive) return;
        StartCoroutine(InputBoxAppearAnimation(enableTextBox));
    }

    private IEnumerator InputBoxAppearAnimation(bool enableTextBox = true)
    {
        answerTextBox.text = "";
        float timeElapsed = 0;
        float animationCompletion = 0;
        while (animationCompletion < 1)
        {
            animationCompletion = timeElapsed / animationTime;
            Vector3 newPosition = Vector3.zero;
            newPosition.y = Mathf.Lerp(hiddenPositionY, 0, Mathf.Min(1, animationCompletion));
            answerTextBox.transform.position = newPosition + adjustToCentre;
            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime;
        }

        isActive = true;
        if (enableTextBox)
        {
            answerTextBox.ActivateInputField();
            answerTextBox.interactable = true;
            answerTextBox.Select();
            isEnabled = true;
        }
    }

    [ContextMenu("Disable Input Box")]
    public void DisableInputBox()
    {
        if (!isActive) return;
        StartCoroutine(InputBoxDisappearAnimation());
    }
    private IEnumerator InputBoxDisappearAnimation()
    {
        float timeElapsed = 0;
        float animationCompletion = 0;
        while (animationCompletion < 1)
        {
            animationCompletion = timeElapsed / animationTime;
            Vector3 newPosition = Vector3.zero;
            newPosition.y = Mathf.Lerp(0, hiddenPositionY, Mathf.Min(1, animationCompletion));
            answerTextBox.transform.position = newPosition + adjustToCentre;
            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime;
        }
        isEnabled = false;
        isActive = false;
        answerTextBox.DeactivateInputField();
        answerTextBox.interactable = false;
        answerTextBox.gameObject.SetActive(false);
    }
}