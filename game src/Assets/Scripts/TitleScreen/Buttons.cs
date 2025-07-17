using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public AchievementManager achievementManager;
    // Start is called before the first frame update
    void Start()
    {
        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
