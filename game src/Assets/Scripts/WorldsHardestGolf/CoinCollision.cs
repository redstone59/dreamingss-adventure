using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CoinCollision : MonoBehaviour
{
    public SpriteRenderer dreaming;
    public BoxCollider2D dreamingBox;
    public Grid grid;
    public TilemapCollider2D coinBox;
    public Tilemap coinTiles;
    public TileBase coin;
    private List<Vector3Int> removedCoins;

    // Start is called before the first frame update
    void Start()
    {
        grid = GetComponentInParent<Grid>();
        coinBox = GetComponent<TilemapCollider2D>();
        coinTiles = GetComponent<Tilemap>();

        removedCoins = new();
    }

    // Update is called once per frame
    void Update()
    {
        if (coinBox.IsTouching(dreamingBox))
        {
            Vector3Int dreamingPosition = grid.WorldToCell(dreaming.transform.position);
            Vector3Int[] neighbourCells = 
            { 
                new(-1, 1, 0),        
                new(0, 1, 0),
                new(1, 1, 0),
                new(-1, 0, 0),
                new(0, 0, 0), // Not technically a neighbour, but whatever.
                new(1, 0, 0),
                new(-1, -1, 0),
                new(0, -1, 0),
                new(1, -1, 0)
            };

            dreaming.BroadcastMessage("CollidedWithCoin");

            foreach (Vector3Int neighbourCell in neighbourCells)
            {
                if (coinTiles.GetTile(dreamingPosition + neighbourCell) == null)
                    continue;
                
                removedCoins.Add(dreamingPosition + neighbourCell);
                coinTiles.SetTile(dreamingPosition + neighbourCell, null);
            }
        }
    }

    public void ReplaceCoins()
    {
        foreach (Vector3Int position in removedCoins)
        {
            coinTiles.SetTile(position, coin);
        }

        removedCoins.Clear();
    }
}
