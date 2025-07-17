using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using TMPro;

using MyWayToTheGrave.Parsing;
using MyWayToTheGrave.SongLoading;
using MyWayToTheGrave.DefaultSong;
using MyWayToTheGrave.Scoring;
using MyWayToTheGrave.Menus;
using Keys;
using UnityEngine.Assertions;
using Speedrun;

public struct BPMChangeRecord
{
    public float bpm;
    public double time;
    public int tick;
}

public class RhythmGame : MonoBehaviour
{
    public AchievementManager achievementManager;

    public Camera mainCamera;
    public SpriteRenderer lips;
    public SpriteRenderer background;
    public TMP_Text scoreText;

    public MyWayManager audioManager;
    public Information information;
    public NoteManager noteManager;
    public MW_ScoreManager scoreManager;
    public MW_CrowdManager crowdManager;
    public SongStats stats;

    public List<Event> songEventList;
    public List<BPMChangeRecord> bpmChanges;
    public Dictionary<Note, NoteHit> noteHits;
    public Dictionary<string, bool> buttonPressed;
    public float lastPitch;

    public Song chart;
    public float bpm;
    public int currentTick = int.MinValue;
    public double timeOfLastBPMChange;
    public int lastSustainCheck;
    
    public double hitTolerance;
    public float pitchTolerance;
    private float pitchCoefficient;

    public float unitsPerSecond;

    public bool loadingCustomSong;
    public DefaultSong defaultSong;
    private int lastNoteHeadPosition = 0;

    public TextMeshProUGUI loadingText;
    public SongMenu customSongMenu;

    public bool noFail;
    public float lagCompensationMinimum = 1f;

    private bool _dreamingMode = false;
    public bool DreamingMode
    {
        get { return _dreamingMode; }
        set { _dreamingMode = value; }
    }

    public SpeedrunTimer speedrunTimer;

    public void SetNoFail(bool value)
    {
        noFail = value;
    }

    public void SetLoadingCustomSong(bool value)
    {
        loadingCustomSong = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        stats = new();
        bpmChanges = new();
        buttonPressed = new();
        noteHits = new();
        pitchCoefficient = 1 / (2 * pitchTolerance);
        noFail = false;
        currentTick = int.MinValue;

        loadingCustomSong = false;
        customSongMenu.gameObject.SetActive(false);

        SongFolder nonCustom = defaultSong.GetDefaultSong(PlayerPrefs.GetInt(PlayerPrefKeys.HardMode, 0) != 0);
        string customsPath = Application.isEditor
                               ? Path.Combine(Application.dataPath, "Customs")
                               : Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "Bonus Content", "Customs");

        if (Directory.Exists(customsPath) && PlayerPrefs.GetInt(PlayerPrefKeys.DontSaveProgress, 0) != 0)
        {
            customSongMenu.currentSong = nonCustom;
            customSongMenu.gameObject.SetActive(true);
            customSongMenu.ScanDirectory(customsPath);
        }
        else
            OnChartLoaded(nonCustom);
        
        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
        speedrunTimer = GameObject.Find("Speedrun Timer").GetComponent<SpeedrunTimer>();
        speedrunTimer.Initialise("Any%", "Death");
    }

    // Update is called once per frame
    void Update()
    {
        if (noFail || !crowdManager.Failed) noteManager.unitsPerTick = unitsPerSecond / (2 * bpm);

        noteManager.DrawNotes(currentTick);
        if (InputManager.Initialised && (noFail || !crowdManager.Failed))
        {
            CheckInputQueue();
            CheckNonNoteEvents();
            UpdateScore();
        }
    }

    public void OnChartLoaded(SongFolder song)
    {
        loadingText.gameObject.SetActive(false);
        audioManager.SetAudio(song.audio);
        audioManager.dreamingModeVocals = _dreamingMode && song.metadata.dreamingable_vocals;
        audioManager.dreamingModeWarmupDeath = _dreamingMode && song.metadata.dreamingable_warmupdeath;
        audioManager.dreamingModeCoughs = _dreamingMode;

        information.text.title.text = '"' + song.metadata.name + '"';
        information.text.artist.text = "by " + song.metadata.composer;
        information.albumImage.texture = song.metadata.album;
        if (song.metadata.coverers != null)
            information.text.coverers.text = "as covered by " + song.metadata.coverers;
        else
            information.text.coverers.text = "";

        background.sprite = song.metadata.background;
        if (!loadingCustomSong && _dreamingMode)
        {
            background.color = Color.black;
        }
        ResizeBackgroundImage();

        chart = ChartParser.ParseChart(song.chart);
        stats.totalNoteHeads = chart.notes.Count;
        songEventList = chart.events;
        List<Note?> workingBuffer = new();

        foreach (Note note in chart.notes)
        {
            lastNoteHeadPosition = Math.Max(lastNoteHeadPosition, note.position);
            workingBuffer.Add((Note?)note);
        }
        
        while (workingBuffer.Count < 1000)
            workingBuffer.Add(null);

        noteManager.noteBuffer.Set(workingBuffer.ToArray());

        StartCoroutine(LoadingSongAndAllat());
        StartCoroutine(TickIncrement());
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void ResizeBackgroundImage()
    {
        float width, height;
        (width, height) = (background.sprite.bounds.size.x, background.sprite.bounds.size.y);

        float worldScreenHeight = Camera.main.orthographicSize * 2;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        Vector3 scale = Vector3.one;
        scale.x = worldScreenWidth / width;
        scale.y = worldScreenHeight / height;
        background.transform.localScale = scale;
    }

    private void UpdateScore()
    {
        if (currentTick - lastSustainCheck >= 10)
            CheckSustains(currentTick);
        PunishMissedNotes();
        if (!noFail && crowdManager.Failed)
            Fail();
        scoreText.text = $"{scoreManager.Score:N0}";
    }

    private void PunishMissedNotes()
    {
        foreach (Note note in noteManager.notesOnScreen)
        {
            bool notePassedStrikeline = currentTick > note.position + InputTimeToTickValue(hitTolerance);
            if (!noteHits.ContainsKey(note))
            {
                if (!notePassedStrikeline) continue;

                NoteHit filler = new()
                {
                    wasMissed = true
                };
                noteHits[note] = filler;
                crowdManager.AdjustCrowdScore(false);
            }

            NoteHit hit = noteHits[note];

            if (notePassedStrikeline && !hit.headHit && !hit.wasMissed)
            {
                hit.wasMissed = true;
                crowdManager.AdjustCrowdScore(false);
                noteHits[note] = hit;
            }
        }
    }

    private IEnumerator TickIncrement()
    {
        while (information.gameObject.activeSelf) yield return null;
        while (audioManager.isPlaying && (noFail || !crowdManager.Failed))
        {
            float tickRate = 1 / (2 * bpm);
            currentTick = Mathf.FloorToInt(audioManager.timeElapsed / tickRate);

            yield return new WaitForSecondsRealtime(tickRate);
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        InputManager.Destroy();
        if (!loadingCustomSong) speedrunTimer.Split();
        if (!crowdManager.Failed)
        {
            if (!loadingCustomSong && !noFail) LevelOrder.AddToSavedScore(scoreManager.Score);
            string nextLevel = LevelOrder.GetNextLevel("MyWayToTheGrave");
            LevelOrder.IncrementSavedLevel();
            string grade = GetGrade();
            information.gameObject.SetActive(true);
            information.ScoreAnimation(stats, scoreManager.Score, grade);
            yield return new WaitForSecondsRealtime(10);
            SceneManager.LoadScene(nextLevel);
        }
    }

    private void CheckNonNoteEvents()
    {
        List<Event> toRemove = new();
        foreach (Event ev in songEventList)
        {
            if (!(currentTick >= ev.tick)) continue;

            Debug.Log($"New event of type {ev.eventType} with values {string.Join(", ", ev.values)}");

            switch (ev.eventType)
            {
                case EventTypes.BPM:
                    ChangeBPM(ev.values[0]);
                    break;
            }
            toRemove.Add(ev);
        }

        foreach (Event ev in toRemove)
            songEventList.Remove(ev);
    }

    private void CheckInputQueue()
    {
        InputEvent[] inputQueue = InputManager.InputQueue;

        if (inputQueue.Length != 0)
        {
            Debug.Log($"{inputQueue.Length}: {string.Join(", ", inputQueue)}");
        }

        foreach (InputEvent input in inputQueue)
        {
            double elapsedTime = input.time - InputManager.InitaliseTime;
            int inputTick = InputTimeToTickValue(elapsedTime);
            if (elapsedTime >= audioManager.timeElapsed + lagCompensationMinimum)
            {
                currentTick = inputTick;
            }

            if (input.isKeyEvent)
            {
                KeyControl[] keyControls = (KeyControl[])input.keyControls;
                UpdatePressedKeys(keyControls);
            }
            else
            {
                CheckSustains(inputTick);
                lastPitch = mainCamera.ScreenToWorldPoint((Vector2)input.mouseYPosition).y;
                lastPitch = Mathf.Clamp(lastPitch, -3.33f, 3.33f);
                lastPitch += 3.33f;
                lastPitch /= 6.66f;
            }
            CheckNoteHeadHit(input);
        }
    }

    public void UpdatePressedKeys(KeyControl[] keys)
    {
        foreach (KeyControl key in keys)
        {
            string dictKey = key.displayName;
            buttonPressed[dictKey] = key.isPressed;
        }
    }

    private void CheckNoteHeadHit(InputEvent input)
    {
        if (!input.isKeyEvent) return;
        bool hasHitAnyNotes = false;
        List<Note> hitNotes = new();
        List<NoteHit> correspondingHits = new();

        foreach (Note note in noteManager.notesOnScreen)
        {
            if (noteHits.ContainsKey(note) && noteHits[note].headHit) continue;

            double elapsedTime = input.time - InputManager.InitaliseTime;
            int leftTolerance = InputTimeToTickValue(elapsedTime - hitTolerance);
            int rightTolerance = InputTimeToTickValue(elapsedTime + hitTolerance);
            bool hitWithinTolerance = leftTolerance <= note.position && note.position <= rightTolerance;

            bool correctKey = false;
            foreach (KeyControl key in input.keyControls)
            {
                bool correctCharacter = key.displayName == note.character.ToString().ToUpper();
                correctKey = correctCharacter && key.wasPressedThisFrame;
                if (correctKey) break;
            }

            float pitchAccuracy = GetPitchAccuracy(note);
            bool noteheadWasHit = hitWithinTolerance && correctKey && pitchAccuracy != 0;
            hasHitAnyNotes |= noteheadWasHit;

            if (noteheadWasHit)
            {
                hitNotes.Add(note);
                correspondingHits.Add(new(noteheadWasHit, pitchAccuracy));
            }
        }

        if (!hasHitAnyNotes)
        {
            bool wasKeyUpEvent = false;
            foreach (KeyControl key in input.keyControls)
            {
                wasKeyUpEvent |= key.wasReleasedThisFrame;
                if (wasKeyUpEvent) break;
            }
            if (!wasKeyUpEvent)
            {
                crowdManager.Oversung();
                audioManager.Cough();
                stats.overtaps++;
            }
        }
        else
        {
            Assert.AreEqual(hitNotes.Count, correspondingHits.Count);
            Note earliestNote = new() { position = int.MaxValue };
            NoteHit earliestHit = new();
            for (int i = 0; i < hitNotes.Count; i++)
            {
                if (hitNotes[i].position < earliestNote.position)
                {
                    earliestNote = hitNotes[i];
                    earliestHit = correspondingHits[i];
                }
            }

            if (earliestNote.position != int.MaxValue)
            {
                noteHits[earliestNote] = earliestHit;
            }

            Note note = earliestNote;
            if (noteHits[note].headHit)
            {
                scoreManager.AddNoteHead(noteHits[note]);
                crowdManager.AdjustCrowdScore(true);
                stats.noteHeadsHit++;
                stats.summedPitchAccuracy += noteHits[note].pitchAccuracy;
                if (note.sustainLength == null) note.gameObject.SetActive(false);
                else 
                {
                    note.gameObject.transform.Find("notehead").gameObject.SetActive(false); // one hell of a line
                    note.gameObject.transform.Find("character").gameObject.SetActive(false); // ooughhh
                }
            }
        }
    }

    private void CheckSustains(int tickOfPitchChange)
    {
        foreach (Note note in noteManager.notesOnScreen)
        {
            string dictKey = note.character.ToString().ToUpper();
            bool correctKey = buttonPressed.ContainsKey(dictKey) && buttonPressed[dictKey];
            if (note.sustainLength == null || !correctKey || !noteHits.ContainsKey(note))
                continue;
            
            NoteHit currentHit = noteHits[note];
            
            int ticksInSustain = (int)GetTicksSustained(note, tickOfPitchChange);
            int scorableSustain = (int)GetSustainLength(note);
            
            if (ticksInSustain <= 0)
            {
                continue;
            }

            currentHit.ticksSustained += ticksInSustain;
            currentHit.ticksSustained = Math.Clamp(currentHit.ticksSustained, 0, scorableSustain);

            float pitchAccuracy = GetPitchAccuracy(note);
            float currentAccuracy = pitchAccuracy * ticksInSustain / scorableSustain;
            currentHit.sustainAccuracy += currentAccuracy;
            noteHits[note] = currentHit;
            
            scoreManager.AddPartialSustain(currentAccuracy, ticksInSustain);
            crowdManager.SustainBonus(ticksInSustain, pitchAccuracy);
            lastSustainCheck = tickOfPitchChange;
        }
    }

    private void Fail()
    {
        if (achievementManager != null)
            achievementManager.UnlockAchievement(currentTick > lastNoteHeadPosition ? AllAchievements.AllThatWork : AllAchievements.Shot);
        audioManager.Die();
        StartCoroutine(FailAnimation());
    }

    private IEnumerator FailAnimation()
    {
        while (bpm > 10f)
        {
            currentTick++;
            yield return new WaitForSeconds(1 / (2 * bpm));
            bpm *= 0.98f;
        }
        bpm = 0;
        InputManager.Destroy();
        SceneManager.LoadScene("TitleScreen");
        yield return null;
    }

    private int? GetTicksSustained(Note note, int tickOfPitchChange)
    {
        if (note.sustainLength == null) return null;

        int sustainStart = note.position;
        int sustainEnd = note.position + (int)note.sustainLength - InputTimeToTickValue(hitTolerance);

        if (!(sustainStart <= tickOfPitchChange && tickOfPitchChange <= sustainEnd))
            return 0;
        
        return Math.Min(tickOfPitchChange, sustainEnd) - Math.Max(lastSustainCheck, sustainStart);
    }

    private int? GetSustainLength(Note note)
    {
        if (note.sustainLength == null) return null;
        return (int)note.sustainLength- InputTimeToTickValue(hitTolerance);
    }

    private float GetPitchAccuracy(Note note)
    {
        float pitchDifference = Mathf.Abs(note.pitch - lastPitch);
        return Mathf.Min(1, Mathf.Max(0, 1 - pitchCoefficient * pitchDifference));
    }

    private int InputTimeToTickValue(double inputTime)
    {
        int tickValue = 0;
        double remainingTime = inputTime;
        
        foreach (BPMChangeRecord bpmChange in bpmChanges)
        {
            if (bpmChange.time >= inputTime)
            {
                tickValue += bpmChange.tick;
                remainingTime -= bpmChange.time;
                continue;
            }
            break;
        }

        tickValue += (int)(remainingTime * 2 * bpm);

        return tickValue;
    }

    private IEnumerator LoadingSongAndAllat()
    {
        audioManager.PlayWarmup();
        information.ShowInformation();
        audioManager.LoadAllAudio();

        yield return new WaitForSeconds(9f);
        while (audioManager.isPlaying)
        {
            float timeUntilWarmupEnd = audioManager.warmupAudio.clip.length - audioManager.warmupAudio.time;
            currentTick = -Mathf.FloorToInt(timeUntilWarmupEnd * 2 * bpm);
            yield return null;
        }
        currentTick = 0;

        audioManager.Play();
        information.gameObject.SetActive(false);
        if (InputManager.Initialised) InputManager.Destroy();
        InputManager.Initialise();
        _ = InputManager.InputQueue; // clear InputQueue to prevent dying when using "Restart Minigame"
        if (!loadingCustomSong) speedrunTimer.BeginTimer();
        timeOfLastBPMChange = Time.timeAsDouble;
    }

    private void ChangeBPM(float newBPM)
    {
        bpmChanges.Add(
            new()
            {
                bpm = bpm,
                time = Time.timeAsDouble - timeOfLastBPMChange,
                tick = currentTick
            }
        );
        bpm = newBPM;
        timeOfLastBPMChange = Time.timeAsDouble;
    }

    private string GetGrade()
    {
        if (achievementManager != null && stats.noteHeadsHit == stats.totalNoteHeads)
        {
            achievementManager.UnlockAchievement(stats.overtaps == 0 ? AllAchievements.FullCombo : AllAchievements.Overcough);
        }

        if (crowdManager.HasEnteredBonus && !crowdManager.HasLeftBonus)
        {
            float notePercent = stats.noteHeadsHit / (float)stats.totalNoteHeads;

            if (notePercent > 0.98f)
            {
                if (achievementManager != null)
                    achievementManager.UnlockAchievement(AllAchievements.SuperSinging);
                return "SS";
            }
            else if (notePercent > 0.95f)
            {
                if (achievementManager != null)
                    achievementManager.UnlockAchievement(AllAchievements.FamiliarName);
                return "S";
            }
            
            if (achievementManager != null)
                achievementManager.UnlockAchievement(AllAchievements.Amazing);
            return "A";
        }

        if (!crowdManager.wentBelowHalf)
        {
            if (achievementManager != null)
                achievementManager.UnlockAchievement(AllAchievements.BetterThanAverage);
            return "B";
        }
        if (!crowdManager.wentIntoRed)
        {
            if (achievementManager != null)
                achievementManager.UnlockAchievement(AllAchievements.Competent);
            return "C";
        }

        if (achievementManager != null)
            achievementManager.UnlockAchievement(AllAchievements.DidWell);
        return "D";
    }
}
