using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PregameTitleCard : MonoBehaviour
{
    public GameObject game;
    // Start is called before the first frame update
    void Start()
    {
        game.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            game.SetActive(true);
            game.transform.Find("dreaming").GetComponent<MuffinPlayer>().SendMessage("StartTimer");
            gameObject.SetActive(false);
        }
    }
}
