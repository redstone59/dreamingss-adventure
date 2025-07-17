using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[Serializable]
public struct VolumeThing
{
    public string name;
    public TextMeshProUGUI valueText;
    public Slider slider;
}

public class AudioMixerAdjust : MonoBehaviour
{
    public AudioMixer mixer;
    public VolumeThing masterVol;
    public VolumeThing musicVol;
    public VolumeThing sfxVol;
    public VolumeThing fmvVol;
    public VolumeThing dialogueVol;
    public VolumeThing scaryVol;

    public List<VolumeThing> allThings;

    void Start()
    {
        allThings = new List<VolumeThing> { masterVol, musicVol, sfxVol, fmvVol, dialogueVol, scaryVol };
    }

    private void SetSoundOf(float soundLevel, VolumeThing exposedName)
    {
        if (soundLevel <= 1e-3f)
        {
            mixer.SetFloat(exposedName.name, -80);
            exposedName.valueText.text = "0.00";
            return;
        }

        mixer.SetFloat(exposedName.name, Mathf.Log10(soundLevel) * 20);
        exposedName.valueText.text = $"{soundLevel:F2}";
    }

    public void SetMasterVol(float soundLevel)
    {
        SetSoundOf(soundLevel, masterVol);
    }

    public void SetMusicVol(float soundLevel)
    {
        SetSoundOf(soundLevel, musicVol);
    }

    public void SetSFXVol(float soundLevel)
    {
        SetSoundOf(soundLevel, sfxVol);
    }

    public void SetFMVVol(float soundLevel)
    {
        SetSoundOf(soundLevel, fmvVol);
    }

    public void SetDialogueVol(float soundLevel)
    {
        SetSoundOf(soundLevel, dialogueVol);
    }

    public void SetScaryVol(float soundLevel)
    {
        SetSoundOf(soundLevel, scaryVol);
    }

    public void SaveAllVolumes()
    {
        foreach (VolumeThing thing in allThings)
        {
            mixer.GetFloat(thing.name, out float volume);
            PlayerPrefs.SetFloat($"AudioMixer {thing.name}", volume);
        }
    }

    public void LoadAllVolumes()
    {
        foreach (VolumeThing thing in allThings)
        {
            float volume = PlayerPrefs.GetFloat($"AudioMixer {thing.name}", 0);
            volume = Mathf.Pow(10, volume / 20);
            SetSoundOf(volume, thing);
            thing.slider.value = volume;
        }
    }
}
