using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Timeline;

public class EnemyCollisions : MonoBehaviour
{
    public SpriteRenderer dreaming;
    public BoxCollider2D dreamingBox;
    public TilemapCollider2D enemyBox;
    // Start is called before the first frame update
    void Start()
    {
        enemyBox = GetComponent<TilemapCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyBox.IsTouching(dreamingBox))
        {
            dreaming.BroadcastMessage("CollidedWithEnemy");
        }
    }
}
