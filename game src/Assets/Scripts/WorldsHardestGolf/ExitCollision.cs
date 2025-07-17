using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ExitCollision : MonoBehaviour
{
    public SpriteRenderer dreaming;
    public BoxCollider2D dreamingBox;
    public TilemapCollider2D exitBox;
    // Start is called before the first frame update
    void Start()
    {
        exitBox = GetComponent<TilemapCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (exitBox.IsTouching(dreamingBox))
        {
            dreaming.BroadcastMessage("CollidedWithExit");
        }
    }
}
