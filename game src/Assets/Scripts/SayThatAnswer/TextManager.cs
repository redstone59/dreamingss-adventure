using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public bool stopText = false;
    // Start is called before the first frame update
    void Start()
    {
        questionText = GetComponent<TextMeshProUGUI>();
        questionText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowText(string text, float time)
    {
        questionText.text = "";
        text = text.Replace('\u2018', '\'')
                   .Replace('\u2019', '\'')
                   .Replace('\u201c', '\"')
                   .Replace('\u201d', '\"');
        StartCoroutine(TextAnimation(text, time / text.Length));
    }

    private IEnumerator TextAnimation(string text, float lengthPerChar)
    {
        stopText = false;
        foreach (char c in text)
        {
            if (stopText) break;
            questionText.text += c;
            yield return new WaitForSeconds(lengthPerChar);
        }
        stopText = false;
        yield return null;
    }
}
