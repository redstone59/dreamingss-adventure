using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonSpriteButton : MonoBehaviour
{
    public SimonManager manager;
    public int colour;
    public Collider2D boxCollider;
    public Camera mainCamera;
    private bool hitMouseThisFrame = false;
    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponentInParent<SimonScript>().GetComponentInParent<SimonManager>(); // ew.
        boxCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.currentSimon == null || manager.currentSimon.playing) { return; }

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (boxCollider.OverlapPoint(mouseWorldPosition) && !hitMouseThisFrame && Input.GetMouseButtonDown(0))
        {
            manager.HitButton(colour);
            hitMouseThisFrame = true;
        }
        else
        {
            hitMouseThisFrame = false;
        }
    }
}
