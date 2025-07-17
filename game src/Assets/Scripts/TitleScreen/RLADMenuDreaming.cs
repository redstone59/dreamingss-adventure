using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RLADMenuDreaming : MonoBehaviour
{
    public RawImage dreaming;
    public Texture normalTexture;
    public Texture[] scaryTextures;

    public float chance;
    public int speed;
    public float shakeDistance;

    private int lastChange = 0;
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        dreaming = GetComponent<RawImage>();
        startPosition = dreaming.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rotation = new(0, 17.5f * Time.deltaTime, 0);
        dreaming.transform.Rotate(rotation);
    }

    private void FixedUpdate()
    {
        if (lastChange++ < speed) return;
        lastChange = 0;

        if (Random.Range(0f, 1f) > chance)
        {
            dreaming.texture = normalTexture;
            dreaming.transform.position = startPosition;
            return;
        }

        int scaryIndex = Random.Range(0, scaryTextures.Length);
        dreaming.texture = scaryTextures[scaryIndex];

        Vector3 shakeOffset = Vector3.zero;
        shakeOffset.x = Random.Range(-shakeDistance, shakeDistance);
        shakeOffset.y = Random.Range(-shakeDistance, shakeDistance);
        dreaming.transform.position += shakeOffset;
    }
}
