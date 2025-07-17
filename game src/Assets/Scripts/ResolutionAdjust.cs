using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionAdjust : MonoBehaviour
{
    public TextMeshProUGUI widthText;
    public ScreenSettings screenSettings;
    public TMP_InputField heightInput;
    public TMP_Dropdown fullscreenDropdown;

    public Toggle capFPS;
    public Toggle vsync;
    public TMP_InputField targetFPS;
    
    // Start is called before the first frame update
    void Start()
    {
        vsync.isOn = QualitySettings.vSyncCount != 0;
        capFPS.isOn = Application.targetFrameRate != -1;
        targetFPS.text = Application.targetFrameRate == -1 ? "" : $"{Application.targetFrameRate}";
    }

    // Update is called once per frame
    void Update()
    {
        vsync.interactable = !capFPS.isOn;
        capFPS.interactable = !vsync.isOn;
        targetFPS.gameObject.SetActive(capFPS.isOn);
    }

    public void UpdateResolutionText()
    {
        string text = heightInput.text.Trim();
        if (text.Length <= 1 || !text.All(char.IsDigit))
        {
            widthText.text = "err";
            return;
        }
        widthText.text = (int.Parse(text) * 16 / 9).ToString();
    }

    public void UpdateResolution()
    {
        if (heightInput.text.Length > 1 && heightInput.text.All(char.IsDigit))
            screenSettings.SetResolution(Mathf.Clamp(int.Parse(heightInput.text), 480, Screen.height));
        
        FullScreenMode[] fullScreenModes =
        { 
            FullScreenMode.Windowed, 
            FullScreenMode.MaximizedWindow, 
            FullScreenMode.FullScreenWindow, 
            FullScreenMode.ExclusiveFullScreen
        };

        Screen.fullScreenMode = fullScreenModes[fullscreenDropdown.value];
    }
    
    public void LoadResolutionSettings()
    {
        widthText.text = Screen.width.ToString();
        heightInput.text = Screen.height.ToString();

        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.Windowed:
                fullscreenDropdown.value = 0;
                break;
            case FullScreenMode.MaximizedWindow:
                fullscreenDropdown.value = 1;
                break;
            case FullScreenMode.FullScreenWindow:
                fullscreenDropdown.value = 2;
                break;
            case FullScreenMode.ExclusiveFullScreen:
                fullscreenDropdown.value = 3;
                break;
        }
    }

    public void ChangeFPS()
    {
        if (!capFPS)
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
        }
        else if (vsync.isOn)
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            string text = targetFPS.text.Trim();
            if (!text.All(char.IsDigit)) return;
            Application.targetFrameRate = int.Parse(text);
            QualitySettings.vSyncCount = 0;
        }
    }
}
