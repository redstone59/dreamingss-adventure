using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MW_NotePrefab : MonoBehaviour
{
    public GameObject noteEnd;
    public GameObject noteTail;
    public TMP_Text character;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        noteTail.transform.position = transform.position + new Vector3(42.31f, 0, 0);
    }

    public void Initialise(Note note, float unitsPerTick)
    {
        character.text = note.character.ToString();
        if (note.sustainLength == null) return;

        Vector3 endPosition = noteEnd.transform.position;
        endPosition.x += (int)note.sustainLength * unitsPerTick;
        noteEnd.transform.position = endPosition;
    }
}
