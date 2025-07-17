using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyWayToTheGrave.SongLoading;

public class MyWayManager : MonoBehaviour
{
    public AudioSource songAudio;
    public AudioSource vocalAAudio;
    public AudioSource vocalBAudio;
    public AudioSource deathAudio;
    public AudioSource warmupAudio;

    public AudioClip[] vocalACoughs;
    public AudioClip[] vocalBCoughs;
    public AudioSource vocalACoughSource;
    public AudioSource vocalBCoughSource;

    private float _previousSeekTime;
    private float _currentSeekTime;

    public float startingVolume = 0.7f;

    public bool dreamingModeVocals = false;
    public bool dreamingModeWarmupDeath = false;
    public bool dreamingModeCoughs = false;

    // Start is called before the first frame update
    void Start()
    {
        volume = startingVolume;
        _previousSeekTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _previousSeekTime = _currentSeekTime;
        _currentSeekTime = timeElapsed;
    }

    public void SetAudio(SongAudio audio)
    {
        songAudio.clip = audio.song;
        vocalAAudio.clip = audio.vocalA;
        vocalBAudio.clip = audio.vocalB;
        deathAudio.clip = audio.death;
        warmupAudio.clip = audio.warmup;
    }

    public void LoadAllAudio()
    {
        songAudio.clip.LoadAudioData();
        vocalAAudio.clip?.LoadAudioData();
        vocalBAudio.clip?.LoadAudioData();
        deathAudio.clip?.LoadAudioData();
        warmupAudio.clip?.LoadAudioData();

        vocalAAudio.mute = dreamingModeVocals;
        vocalACoughSource.mute = dreamingModeVocals;
        warmupAudio.mute = dreamingModeWarmupDeath;
        deathAudio.mute = dreamingModeWarmupDeath;
    }

    [ContextMenu("Warmup")]
    public void PlayWarmup()
    {
        warmupAudio.Play();
    }

    [ContextMenu("Die")]
    public void Die()
    {
        if (vocalAAudio.clip != null)
            vocalAAudio.Stop();
        if (vocalBAudio.clip != null)
            StartCoroutine(DelayedVocalBStop());
        if (vocalAAudio.clip == null && vocalBAudio.clip == null)
            songAudio.Stop();
        deathAudio.Play();
    }

    private IEnumerator DelayedVocalBStop()
    {
        yield return new WaitForSeconds(1.868f);
        vocalBAudio.Stop();
    }

    [ContextMenu("Play")]
    public void Play()
    {
        deathAudio.Stop();
        warmupAudio.Stop();
        songAudio.Play();
        vocalAAudio.Play();
        vocalBAudio.Play();
    }

    public void Cough()
    {
        if (vocalACoughSource.isPlaying && vocalBCoughSource.isPlaying) return;

        bool vocalAToCough = dreamingModeCoughs ? false : Random.Range(0, 1f) >= 0.5f;
        if (vocalAToCough && vocalACoughSource.isPlaying)
            vocalAToCough = false;
        else if (!vocalAToCough && vocalBCoughSource.isPlaying)
            vocalAToCough = true;
        
        StartCoroutine(CoughNoise(vocalAToCough));
    }

    public IEnumerator CoughNoise(bool vocalAToCough)
    {
        if (vocalAToCough)
        {
            float previousVolume = vocalAAudio.volume;
            vocalAAudio.volume = 0;
            AudioClip cough = vocalACoughs[Random.Range(0, vocalACoughs.Length)];
            vocalACoughSource.clip = cough;
            vocalACoughSource.Play();
            yield return new WaitForEndOfFrame();
            while (vocalACoughSource.isPlaying) yield return null;
            vocalAAudio.volume = previousVolume;
        }
        else
        {
            float previousVolume = vocalBAudio.volume;
            vocalBAudio.volume = 0;
            AudioClip cough = vocalBCoughs[Random.Range(0, vocalBCoughs.Length)];
            vocalBCoughSource.clip = cough;
            vocalBCoughSource.Play();
            yield return new WaitForEndOfFrame();
            while (vocalBCoughSource.isPlaying) yield return null;
            vocalBAudio.volume = previousVolume;
        }
        yield return null;
    }

    public bool isPlaying
    {
        get
        {
            return songAudio.isPlaying   || 
                   vocalAAudio.isPlaying || 
                   vocalBAudio.isPlaying || 
                   deathAudio.isPlaying  || 
                   warmupAudio.isPlaying  ;
        }
    }

    public bool SongIsFinished
    {
        get
        {
            return songAudio.time >= songAudio.clip.length;
        }
    }

    public float timeElapsed
    {
        get
        {
            return Mathf.Max(
                songAudio.time,
                vocalAAudio.time,
                vocalBAudio.time
            );
        }
    }

    public float volume
    {
        get { return songAudio.volume; }
        set
        {
            songAudio.volume = value;
            vocalAAudio.volume = value;
            vocalBAudio.volume = value;
            deathAudio.volume = value;
            warmupAudio.volume = value;
            vocalACoughSource.volume = value;
            vocalBCoughSource.volume = value;
        }
    }

    public float deltaTime
    {
        get
        {
            return _currentSeekTime - _previousSeekTime;
        }
    }
}
