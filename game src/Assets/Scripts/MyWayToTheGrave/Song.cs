using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventTypes
{
    BPM,
    TimeSignature,
    WordStart,
    WordEnd
}

[Serializable]
public struct Event
{
    public int tick;
    public EventTypes eventType;
    public float[] values;
}

[Serializable]
public struct Song
{
    public Song(bool filler = true)
    {
        notes = new();
        events = new();
    }

    public List<Note> notes;
    public List<Event> events;
}