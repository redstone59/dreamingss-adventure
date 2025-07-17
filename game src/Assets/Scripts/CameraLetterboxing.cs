using UnityEngine;
using System.Collections;

public class CameraLetterBoxing : MonoBehaviour
{
    public bool setBackground = true;

    void Start () 
    {
        // set the desired aspect ratio, I set it to fit every screen 
        float targetAspect = 1280 / 720f;
        
        // determine the game window's current aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;
        
        // current viewport height should be scaled by this amount
        float scaleHeight = windowAspect / targetAspect;
        
        // obtain camera component so we can modify its viewport
        Camera camera = GetComponent<Camera>();
        if (setBackground) camera.backgroundColor = Color.black;
        
        // if scaled height is less than current height, add letterbox
        if (scaleHeight < 1.0f)
        {  
            Rect rect = camera.rect;
            
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            
            camera.rect = rect;
        }
        else // add container box
        {
            float scaleWidth = 1.0f / scaleHeight;
            
            Rect rect = camera.rect;
            
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            
            camera.rect = rect;
        }
    }
}