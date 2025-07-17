using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FDAGT_AI : MonoBehaviour
{
    public FDAGT_Hitter hitter;
    public Rigidbody2D puck;
    public float puckCloseness;
    public float puckRadius;
    public float verticalTolerance;

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
        if (puck.position.x > 0)
        {
            Vector3 distanceFromPuck = puck.transform.position - transform.position;

            if (distanceFromPuck.magnitude > puckCloseness)
            {
                return;
            }

            hitter.active = true;
            if (transform.position.x < puck.position.x + puckRadius)
            {
                hitter.target = transform.position + new Vector3(1, 0, 0);
                if (Mathf.Abs(transform.position.y - puck.position.y) < verticalTolerance)
                {
                    hitter.target += new Vector3(0, transform.position.y > 0 ? -2 : 2);
                }
            }
            else
            {
                hitter.target = puck.transform.position;
            }
            
        }
        else if (transform.position.x < 0)
        {
            hitter.target = new(5.6f, 0, 0);
            hitter.active = true;
        }
        else
        {
            hitter.active = false;
        }
    }
}
