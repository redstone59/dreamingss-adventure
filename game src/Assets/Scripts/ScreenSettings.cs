using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSettings : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetResolution()
    {
        SetResolution(720);
    }

    public void SetResolution(int height)
    {
        int width = height * 16 / 9;
        Screen.SetResolution(width, height, Screen.fullScreen);
    }
}
