using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public const int NOTE_BUFFER_CAP = 1000;
    public NoteBuffer noteBuffer = new(NOTE_BUFFER_CAP);
    public float bottomOfSlider;
    public float topOfSlider;
    public float strikelinePositionX;

    public GameObject notePrefab;
    public GameObject beatMarker;
    public float unitsPerTick;
    public int beatsPerBar;

    public float leftNoteBound;
    public float rightNoteBound;

    public List<Note> notesOnScreen;
    public List<GameObject> beatMarkers;

    // Start is called before the first frame update
    void Start()
    {
        notesOnScreen = new();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawNotes(int position)
    {
        int drawnNotes = 0;
        notesOnScreen.Clear();

        // For some reason the `foreach` implementation throws an InvalidCastException so whatever i'll do it myself
        noteBuffer.Reset();

        //foreach (object potentialNote in noteBuffer)
        while (drawnNotes < noteBuffer.Count)
        {
            if (!noteBuffer.MoveNext()) break;

            Note? potentialNote = noteBuffer.Current;

            if (potentialNote == null) continue;
            Note note = (Note)potentialNote;

            int deltaPosition = note.position - position;
            int deltaEndPosition;            
            float noteXPosition = strikelinePositionX + deltaPosition * unitsPerTick;

            bool noteOnScreen;
            if (note.sustainLength != null)
            {
                deltaEndPosition = note.position + (int)note.sustainLength - position;
                float noteendXPosition = strikelinePositionX + deltaEndPosition * unitsPerTick;
                bool noteendAheadOfLeftBound = noteendXPosition > leftNoteBound;
                bool noteheadPastRightBound = noteXPosition < rightNoteBound;
                noteOnScreen = noteendAheadOfLeftBound && noteheadPastRightBound;
            }
            else
            {
                noteOnScreen = leftNoteBound < noteXPosition && noteXPosition < rightNoteBound;
            }
            
            
            // Cull notes not on screen
            if (!noteOnScreen)
            {
                if (note.gameObject) Destroy(note.gameObject);
                continue;
            }

            notesOnScreen.Add(note);

            GameObject noteObject;
            noteObject = GetNoteObject();
            
            float pitchPosition = Mathf.Lerp(bottomOfSlider, topOfSlider, note.pitch);

            Vector3 notePosition = new(noteXPosition, pitchPosition, 0);
            noteObject.transform.position = notePosition;

            //if (++drawnNotes >= noteBuffer.Count) break;
        }
    }

    private void DrawBars(int position)
    {

    }

    private GameObject GetNoteObject()
    {
        Note note = (Note)noteBuffer.Current;
        GameObject noteObject;
        if (!note.drawn || note.gameObject == null)
        {
            noteObject = Instantiate(notePrefab);
            noteObject.name = "Rendered Note";
            note.gameObject = noteObject;
            note.drawn = true;
            
            MW_NotePrefab prefab = noteObject.GetComponent<MW_NotePrefab>();
            prefab.Initialise(note, unitsPerTick);
        }
        else
            noteObject = note.gameObject;


        noteBuffer.Current = note;

        return noteObject;
    }

    [ContextMenu("Test Drawing Notes")]
    public void TestDraw()
    {
        int drawnNotes = 0;
        int position = 0;
        while (drawnNotes < 1000)
        {
            Note testNote = new()
            {
                position = position,
                pitch = Random.Range(0f, 1f),
                character = "asdf".ToCharArray()[Random.Range(0, 4)],
                sustainLength = Random.Range(0f, 1f) > 0.5 ? null : Random.Range(1, 3) * 60,
                drawn = false,
                gameObject = null
            };
            noteBuffer.Add(testNote);
            position += Random.Range(2, 6) * 60;
            drawnNotes++;
        }
    }
}
