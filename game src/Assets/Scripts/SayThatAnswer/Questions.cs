using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Question
{
    public string text;
    public AudioClip audio;
    public List<string> validAnswers;
    public List<string> invalidAnswers;
}