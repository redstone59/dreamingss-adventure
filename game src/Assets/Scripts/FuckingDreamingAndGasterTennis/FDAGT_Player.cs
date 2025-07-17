using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FDAGT_Player : MonoBehaviour
{
    public FDAGT_Hitter hitter;
    public Camera mainCamera;
    public Collider2D screenBoundaries;
    // Start is called before the first frame update
    void Start()
    {
        hitter = GetComponent<FDAGT_Hitter>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if (transform.position.x > 0)
        {
            hitter.target = new(-5.91f, 0, 0);
            hitter.active = true;
            return;
        }
        
        hitter.target = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (!screenBoundaries.OverlapPoint(hitter.target))
            hitter.target = screenBoundaries.ClosestPoint(hitter.target);

        hitter.active = Input.GetMouseButton(0);
    }
}
