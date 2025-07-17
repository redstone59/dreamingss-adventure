using System;
using UnityEngine;

[Serializable]
public struct Note
{
    public int position;
    public float pitch;
    public char character;
    public int? sustainLength;

    public bool drawn;
    public GameObject gameObject;
}

[Serializable]
public struct NoteHit
{
    public NoteHit(bool head, float accuracy, int sustain = 0)
    {
        headHit = head;
        ticksSustained = sustain;
        pitchAccuracy = accuracy;
        sustainAccuracy = 0;
        wasMissed = false;
    }

    public bool headHit;
    public int ticksSustained;

    public float pitchAccuracy;
    public float sustainAccuracy;

    public bool wasMissed;
}