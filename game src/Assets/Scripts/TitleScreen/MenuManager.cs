using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Keys;

[Serializable]
public struct Menu
{
    public GameObject menuObject;
    public AudioClip musicClip;
    public float musicVolume;
}

public class MenuManager : MonoBehaviour
{
    public Menu[] menus;
    public int savedLevel = 0;
    public AudioSource music;
    public AchievementManager achievementManager;
    public float deleteKeybindHeldLength = 0;
    public AchievementMenu achievementMenu;

    public AudioMixerAdjust mixer;

    public bool focused = true;
    private AsyncOperation asyncLevelLoad;
    public Func<bool> deleteKeybindPressed;
    
    // Start is called before the first frame update
    void Start()
    {
        mixer.LoadAllVolumes();
        SaveSystem.CheckForLegacySaveData();
        SaveSystem.LoadSaveFile();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (PlayerPrefs.GetInt("DontSaveProgress", 0) != 0)
            PlayerPrefs.SetInt("DontSaveProgress", 0);
        
        if (PlayerPrefs.HasKey("WasOnHardMode"))
        {
            PlayerPrefs.SetInt(PlayerPrefKeys.HardMode, PlayerPrefs.GetInt("WasOnHardMode"));
            PlayerPrefs.DeleteKey("WasOnHardMode");
        }

        PlayerPrefs.DeleteKey("LeftSTA");

        music = GetComponent<AudioSource>();
        savedLevel = SaveSystem.GameData.savedLevel + 1; // Index 0 is the menu for no save game.
        
        StartCoroutine(PreloadSavedLevel());
        SetMenu(savedLevel);

        deleteKeybindPressed = () => (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Delete);
        deleteKeybindHeldLength = 0;

        achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
        if (PlayerPrefs.GetInt(PlayerPrefKeys.KickedOutLastOpen, 0) != 0)
        {
            achievementManager.UnlockAchievement(AllAchievements.AfraidQuestionMark);
            PlayerPrefs.DeleteKey(PlayerPrefKeys.KickedOutLastOpen);
        }
    }

    private void SetMenu(int index)
    {
        foreach (Menu menu in menus) menu.menuObject.SetActive(false);
        Menu selectedMenu;
        try
        {
            selectedMenu = menus[index];
        }
        catch (Exception)
        {
            selectedMenu = menus[0];
        }

        selectedMenu.menuObject.SetActive(true);
        music.clip = selectedMenu.musicClip;
        music.volume = selectedMenu.musicVolume;
        music.Play();
    }

    private void Jukebox()
    {
        if (PlayerPrefs.GetInt(PlayerPrefKeys.NumberOfVictories, 0) == 0) return;

        float songTime = music.time;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetMenu(0);      // normal
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetMenu(1); // fdagt
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SetMenu(2); // worlds hardest golf
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SetMenu(3); // rogue like at dreamings
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SetMenu(4); // simon
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SetMenu(5); // my way to the grave
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SetMenu(6); // say that answer
        else if (Input.GetKeyDown(KeyCode.Alpha8)) SetMenu(7); // muffin credits / victory

        music.time = songTime;
    }

    // Update is called once per frame
    void Update()
    {
        Jukebox();
        if (!music.isPlaying && focused)
        {
            PlayerPrefs.SetInt(PlayerPrefKeys.KickedOutLastOpen, 1);
            Debug.Log("quit");
            Application.Quit();
        }

        if (deleteKeybindPressed())
        {
            deleteKeybindHeldLength += Time.deltaTime;
            if (deleteKeybindHeldLength >= 5)
            {
                deleteKeybindHeldLength = 0;
                if (Input.GetKey(KeyCode.S))
                {
                    Debug.Log("Wiped splits!");
                    SaveSystem.ResetSplitsAndTimes();
                }
                else
                {
                    DeleteAllSaveData();
                    Start();
                }
            }
        }
        else deleteKeybindHeldLength = 0;
    }

    [ContextMenu("Delete All Save Data")]
    public void DeleteAllSaveData()
    {
        /*
         * Now, you may be asking, why would you do this instead of PlayerPrefs.DeleteAll()?
         * The answer is that I wanted to have a one-time event at the start of the game.
         * The only issue now, is that I no longer remember what I wanted to keep between saves.
         * So, I have to manually delete everything that I add.
         *
         * The pains of not actually writing notes.
         *
         * okay so 20 minutes later i realised its the fucking settings i dont want to delete the settings
         * why did i not just save the settings to local variables then use DeleteAll() then set the settings back
         * 
         * post release update! i redid the whole save system!
         */

        PlayerPrefs.DeleteKey(PlayerPrefKeys.KickedOutLastOpen); // Avoids bug where you can get the Afraid? achievement through the game resetting itself
        PlayerPrefs.DeleteKey(PlayerPrefKeys.HasHitNewGameOrContinue);

        // Resetting Non-essential Settings
        PlayerPrefs.SetInt(PlayerPrefKeys.JumpscareSoundSubstituted, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.HardModeNextGame, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.SpeedrunMode, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.DontSaveProgress, 0);
        PlayerPrefs.SetInt(PlayerPrefKeys.SpeedrunMode, 0);

        SaveSystem.ResetSaveData();

        Debug.Log("Deleted all save game data.");
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus) StartCoroutine(UpdateFocus());
        else focused = false;
    }

    private IEnumerator UpdateFocus()
    {
        yield return new WaitForSeconds(1);
        focused = true;
    }

    private IEnumerator PreloadSavedLevel()
    {
        asyncLevelLoad = SceneManager.LoadSceneAsync(LevelOrder.GetLevelAtIndex(Mathf.Clamp(savedLevel - 1, 0, LevelOrder.sceneNames.Length - 1)));
        asyncLevelLoad.allowSceneActivation = false;

        yield return null;
    }

    public void NewGame()
    {
        SaveSystem.GameData.totalScore = 0;
        SaveSystem.GameData.savedLevel = 0;

        bool hardModeEnabled = PlayerPrefs.GetInt(PlayerPrefKeys.HardModeNextGame, 0) != 0;
        PlayerPrefs.SetInt(PlayerPrefKeys.HardMode, hardModeEnabled ? 1 : 0);

        achievementManager.UnlockAchievement(AllAchievements.Adventurer);
        SceneManager.LoadScene(LevelOrder.GetLevelAtIndex(0), LoadSceneMode.Single);
    }

    public void Continue()
    {
        asyncLevelLoad.allowSceneActivation = true;
    }

    public void Exit()
    {
        SaveSystem.WriteSaveFile();
        Application.Quit();
    }

    public void AchievementMenu()
    {
        achievementMenu.gameObject.SetActive(true);
        achievementMenu.Initialise();
    }

    public void ToggleOptionsMenu()
    {
        GameObject.Find("Option Menu stupid parent")
                  .transform
                  .Find("Options Menu")
                  .gameObject
                  .SetActive(true);

        GameObject.Find("Option Menu stupid parent")
                  .GetComponent<OptionsMenu>()
                  .Reload();

        GameObject.Find("Option Menu stupid parent")
                  .GetComponent<OptionsMenu>()
                  .SetLevelSelectButtons();

        GameObject.Find("Option Menu stupid parent")
                  .GetComponent<OptionsMenu>()
                  .ResetUnlockAllButton();
    }
}
