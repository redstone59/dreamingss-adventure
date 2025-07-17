using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Contestant
{
    public string name;
    public List<AudioClip> announcerLines;
    public GameObject contestantImage;
    
    // Speed, after question is heard
    public float agility;             // how fast the contestant can be
    public float variation;           // variation in the time taken to buzz

    // Speed, before question is heard
    public float chanceEarlyGuess;    // between 0 and 1, chance contestant will buzz early
    public float earlyAgility;        // between 0 and 1, percent of the question heard before buzzing
    public float earlyVariance;       // between 0 and 1, percentage variance in early buzz time
    
    // Intelligence
    public float chanceCorrectAnswer; // between 0 and 1, chance the answer generated is correct
    public float garbledness;         // how malformed the wrong answers are (makes it easier to tell the correct answer)
}