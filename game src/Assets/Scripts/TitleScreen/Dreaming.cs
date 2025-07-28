using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dreaming : MonoBehaviour
{
    public SpriteRenderer dreamingSprite;
    public GameObject partyHat;
//  public AudioSource minuet;

    // Start is called before the first frame update
    void Start()
    {
        dreamingSprite = GetComponent<SpriteRenderer>();
        bool hasVictory = SaveSystem.GameData.numberofVictories != 0;
        DateTime currentDay = DateTime.Now;
        bool isDreamingsBirthday = currentDay.DayOfYear == DateTime.Parse($"April 24 {DateTime.Now.Year}").DayOfYear;
        partyHat.SetActive(hasVictory || isDreamingsBirthday);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rotation = new(0, 35f * Time.deltaTime, 0);
        dreamingSprite.transform.Rotate(rotation);

//      if (!minuet.isPlaying)
//          Application.Quit();
    }
}
