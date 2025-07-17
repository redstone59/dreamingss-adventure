using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnswerTextBox : MonoBehaviour
{
    public TMP_InputField textBox;
    public int characterLimit = 24;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LimitCharacterCount()
    {
        if (textBox.text.Length >= characterLimit) textBox.text = textBox.text[0..characterLimit];
    }
}
