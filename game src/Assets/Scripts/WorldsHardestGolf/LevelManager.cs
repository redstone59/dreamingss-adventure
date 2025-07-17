using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int level;
    public int coins;
    public int strokes;
    public int deaths;
    public int[] requiredCoins;
    public GameObject[] levelObjects;
    public string[] levelStrings;
    public Camera mainCamera;
    public Color[] backgroundColours;

    public CoinCollision coinTilemap;
    
    // Start is called before the first frame update
    void Start()
    {
        level = 0;
        strokes = 0;
        LoadLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ChangeSkyboxColour()
    {
        switch (level)
        {
            case 0:
            case 1:
                mainCamera.backgroundColor = backgroundColours[0];
                break;
            case 2:
            case 3:
                mainCamera.backgroundColor = backgroundColours[1];
                break;
            case 4:
                mainCamera.backgroundColor = backgroundColours[2];
                break;
        }
    }

    public bool AllCoinsCollected()
    {
        return coins >= requiredCoins[level]; // Just in case.
    }

    public void LoadLevel()
    {
        foreach (GameObject obj in levelObjects)
        { 
            obj.SetActive(false);
        }
        levelObjects[level].SetActive(true);
        coinTilemap = levelObjects[level].GetComponentInChildren<CoinCollision>();
        ChangeSkyboxColour();
        coins = 0;
    }

    public void ReloadSavedCoins()
    {
        coinTilemap.ReplaceCoins();
        return;
    }
}
