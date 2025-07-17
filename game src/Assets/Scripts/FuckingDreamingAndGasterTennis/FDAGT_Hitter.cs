using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FDAGT_Hitter : MonoBehaviour
{
    public Rigidbody2D body;
    public float forceMultiplier;
    public float maxMagnitude;
    public Vector3 target;
    public bool active;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        target = Vector3.zero;
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
        if (!active) return;

        Vector3 difference = target - transform.position;
        difference *= forceMultiplier;

        if (difference.magnitude > maxMagnitude)
            difference = difference.normalized * maxMagnitude;

        body.AddForce(difference * Time.fixedDeltaTime);
    }

}
