using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FDAGT_Puck : MonoBehaviour
{
    public Rigidbody2D body;
    public FDAGT_ReactiveMusic sound;
    public string lastHitterCollision;
    public int collisionsWithPlayer = 0;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        sound = GameObject.Find("Reactive Music").GetComponent<FDAGT_ReactiveMusic>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Sent when a collider on another object stops touching this
    /// object's collider (2D physics only).
    /// </summary>
    /// <param name="other">The Collision2D data associated with this collision.</param>
    void OnCollisionExit2D(Collision2D other)
    {
        sound.PlayBonkNoise(body.velocity.magnitude);
        if (other.gameObject.name == "Player" || other.gameObject.name == "Opponent")
            lastHitterCollision = other.gameObject.name;

        if (other.gameObject.name == "Player")
            collisionsWithPlayer++;
    }
}
