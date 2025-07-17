using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lips : MonoBehaviour
{
    public Camera mainCamera;
    public SpriteRenderer lipSprite;

    public float lowerBound;
    public float upperBound;

    // Start is called before the first frame update
    void Start()
    {
        lipSprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float mouseYPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition).y;

        mouseYPosition = Mathf.Clamp(mouseYPosition, lowerBound, upperBound);
        
        Vector3 newLipPosition = lipSprite.transform.position;
        newLipPosition.y = mouseYPosition;
        lipSprite.transform.position = newLipPosition;
    }

    public float Position { get { return lipSprite.transform.position.y / (upperBound - lowerBound); } }
}
