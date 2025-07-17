using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct UserResponse
{
    public void ShowButtons(string firstButtonText, string secondButtonText)
    {
        textInput.gameObject.SetActive(false);
        
        yesButton.GetComponentInChildren<TextMeshProUGUI>().text = firstButtonText;
        yesButton.gameObject.SetActive(true);

        noButton.GetComponentInChildren<TextMeshProUGUI>().text = secondButtonText;
        noButton.gameObject.SetActive(true);
    }

    public void ShowTextInput()
    {
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        textInput.text = "";
        textInput.gameObject.SetActive(true);
    }

    public void HideAll()
    {
        textInput.gameObject.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }

    public Button yesButton;
    public Button noButton;
    public TMP_InputField textInput;
}

[Serializable]
public struct DialogBox
{
    public GameObject gameObject;
    public TextMeshProUGUI head;
    public TextMeshProUGUI body;
}

public class DeleteDataThing : MonoBehaviour
{
    public UserResponse inputs;
    public DialogBox dialogBox;
    public TextMeshProUGUI errorText;
    public GameObject[] mathImages;

    public int stage = 0;
    public string textToCompare;
    public bool dialogOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        inputs.HideAll();
        dialogBox.gameObject.SetActive(false);
        errorText.text = "";

        HideMathImages();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseStage()
    {
        if (dialogOpen) return;

        dialogBox.gameObject.SetActive(true);
        dialogOpen = true;
        switch (stage)
        {
            case 0:
                dialogBox.head.text = "Warning!";
                dialogBox.body.text = "Deleting your save data is <b>irreversible</b>\nAre you sure you want to delete your save data?";
                inputs.ShowButtons("Yes", "No");
                break;
            case 1:
                dialogBox.head.text = "Warning!";
                dialogBox.body.text = "I mean, you're that eager to throw it all away? You're <b>absolutely sure</b> you want to delete your save data?";
                inputs.ShowButtons("Yes", "No");
                break;
            case 2:
                dialogBox.head.text = "Just to be sure...";
                dialogBox.body.text = "Okay, you seem pretty confident.\nJust, for my own sake, type <i>\"I am certain that I want my save data deleted.\"</i>";
                inputs.ShowTextInput();
                textToCompare = "I am certain that I want my save data deleted.";
                break;
            case 3:
                dialogBox.head.text = "Are you a robot?";
                dialogBox.body.text = "We're unable to confirm that a human is trying to delete the save data.\nTo verify this, please type the name of your computer within 15 seconds.";
                inputs.ShowTextInput();
                textToCompare = Environment.MachineName;
                StartCoroutine(StartResetTimer(15));
                break;
            case 4:
                dialogBox.head.text = "Mental Capacity Test";
                dialogBox.body.text = "To ensure that this action is being done by a mentally competent person, a small mathematical test will be given. Please solve this to proceed with deleting your save data.";
                mathImages[UnityEngine.Random.Range(0, mathImages.Length - 1)].SetActive(true);
                inputs.ShowTextInput();
                textToCompare = "1";
                break;
            case 5:
                dialogBox.head.text = "Mental Capacity Test";
                dialogBox.body.text = "To test short-term memory, recall the statement you used to first state your desire to delete your save data within 10 seconds.";
                inputs.ShowTextInput();
                textToCompare = "I am certain that I want my save data deleted.";
                StartCoroutine(StartResetTimer(10));
                break;
            case 6:
                dialogBox.head.text = "Warning!";
                dialogBox.body.text = "Not deleting your save data is not considered irreversable. Are you sure that you don't not want to not keep your save data?";
                inputs.ShowButtons("Yes?", "No?");
                break;
            case 7:
                dialogBox.head.text = "Think fast!";
                dialogBox.body.text = "What was the name of your computer again?";
                inputs.ShowTextInput();
                textToCompare = Environment.MachineName;
                StartCoroutine(StartResetTimer(10));
                break;
            default:
                dialogBox.head.text = "Deleting saved game data";
                dialogBox.body.text = "In order to delete saved game data, press Ctrl+Delete on the title screen for 5 seconds.\n\nThis dialog will automatically close in 10 seconds.";
                StartCoroutine(StartResetTimer(10));
                break;
        }
    }

    public void ClickedYesButton()
    {
        inputs.HideAll();
        dialogBox.gameObject.SetActive(false);
        stage++;
        ShowErrorText();
    }

    public void OnEndEdit()
    {
        dialogBox.gameObject.SetActive(false);
        inputs.HideAll();
        HideMathImages();

        if (inputs.textInput.text != textToCompare)
        {
            CheckForAchievement();
            stage = 0;
            ShowErrorText("Error: Strings do not match. Save game data deletion cancelled.");
        }
        else
        {
            stage++;
            ShowErrorText();
        }
    }

    public void Reset()
    {
        CheckForAchievement();
        inputs.HideAll();
        dialogBox.gameObject.SetActive(false);
        stage = 0;
        ShowErrorText("Save game data deletion cancelled.");
    }

    public void CheckForAchievement()
    {
        if (stage == 4)
        {
            AchievementManager achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
            achievementManager.UnlockAchievement(AllAchievements.BadAtMath);
        }
    }

    public void ShowErrorText(string overrideString = "")
    {
        string[] strings = {
            "Error: how did you get to this text please read the code",
            "Error: Save data not deleted. Please try again.",
            "Error: Unable to read confidence. Please try again.",
            "Error: Identity not verified. Please try again.",
            "Error: Mental capacity unchecked. Please try again.",
            "Error: Automatic short-term memory test not passed. Please try again.",
            "Error: Save data confidence checker failure. Please try again.",
            "Error: Unknown error occured. Please try again.",
            "Error: Unknown error occured (but a different one this time). Please try again."
        };

        errorText.text = overrideString != "" ? overrideString : strings[stage];

        StartCoroutine(ErrorTextFadeOut());
    }

    private IEnumerator ErrorTextFadeOut()
    {
        float numberOfFixedUpdates = 1 / Time.fixedDeltaTime;
        for (int i = 0; i <= numberOfFixedUpdates; i++)
        {
            errorText.color = Color.Lerp(Color.black, Color.clear, i / numberOfFixedUpdates);
            yield return new WaitForFixedUpdate();
        }
        dialogOpen = false;
        yield return null;
    }

    private IEnumerator StartResetTimer(float timeUntilReset)
    {
        int numberOfFixedUpdates = (int)(timeUntilReset / Time.fixedDeltaTime);
        int elapsedFixedUpdates = 0;

        while (dialogOpen && elapsedFixedUpdates <= numberOfFixedUpdates)
        {
            yield return new WaitForFixedUpdate();
            elapsedFixedUpdates++;
        }

        if (!dialogOpen) yield break;

        inputs.HideAll();
        dialogBox.gameObject.SetActive(false);
        Reset();
        ShowErrorText("Error: No input within allocated time. Save game data deletion cancelled.");
    }

    private void HideMathImages()
    {
        foreach (GameObject gameObject in mathImages)
            gameObject.SetActive(false);
    }
}
