using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RLADMenuNoise : MonoBehaviour
{
    public RawImage background;
    public Texture[] noiseImages;
    public int speed;
    private int lastChange = 0;
    private int currentImage = 0;

    // Start is called before the first frame update
    void Start()
    {
        background = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (lastChange++ < speed) return;
        lastChange = 0;

        currentImage = ++currentImage % noiseImages.Length;
        background.texture = noiseImages[currentImage];
    }
}
