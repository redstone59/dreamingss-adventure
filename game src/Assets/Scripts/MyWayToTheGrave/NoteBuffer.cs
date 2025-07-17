using System;
using System.Collections;
using UnityEngine;

public class NoteBuffer
{
    private Note?[] buffer;
    private int count;
    private int firstOpenSlot;
    private int position;

    public NoteBuffer(int size)
    {
        buffer = new Note?[size];
        firstOpenSlot = 0;
    }

    public void Add(Note note)
    {
        buffer[firstOpenSlot] = note;
        count++;
        firstOpenSlot = Array.IndexOf(buffer, null);
    }

    public void Set(Note?[] notes)
    {
        buffer = notes;
        count = notes.Length;
        firstOpenSlot = Array.IndexOf(buffer, null);
    }

    public void Remove(int index)
    {
        buffer[index] = null;
        firstOpenSlot = Mathf.Min(firstOpenSlot, index);
        count--;
    }

    public void Remove(Note note)
    {
        int index = Array.IndexOf(buffer, note);
        Remove(index);
    }

    public Note? this[int index]
    {
        get
        {
            return buffer[index];
        }
    }

    public int Count { get { return count; } }

    // `foreach` implementation
    // https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/make-class-foreach-statement

    public IEnumerator GetEnumerator()
    {
        return (IEnumerator)this;
    }

    public bool MoveNext()
    {
        position++;
        return position < buffer.Length;
    }

    public void Reset()
    {
        position = -1;
    }

    public Note? Current
    {
        get { return buffer[position]; }
        set { buffer[position] = value; }
    }
}