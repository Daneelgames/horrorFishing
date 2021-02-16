using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    public TextMeshProUGUI languageText;
    public Button continueButton;
    public List<string> startButton = new List<string>();
    public TextMeshProUGUI startText;
    public List<string> continueButtonL = new List<string>();
    public TextMeshProUGUI continueButtonText;
    public List<string> quitButton = new List<string>();
    public TextMeshProUGUI quitText;

    public List<ButtonLocals> buttonLocals;

    public GameObject settingsWindow;
    public GameObject menuWindow;
    public GameObject menuVisuals;

    GameManager gm;

    [Header("First selected objects")] 
    public GameObject firstSelectedInMenu;
    public GameObject firstSelectedInOptions;
    public GameObject firstSelectedInDifficultyWindow;

    public AudioSource menuMusicSource;
    public AudioClip rearWindowClip;
    public AudioClip christmasClip;
    
    void Start()
    {
        Init();
    }

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        gm = GameManager.instance;

        SavingData data = SavingSystem.LoadGame();

        if (data != null)
        {
            // show continue tab
            continueButton.gameObject.SetActive(true);

        }
        else
        {
            // hide
            continueButton.gameObject.SetActive(false);
        }
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedInMenu);

        print("Current month is " + DateTime.Now.Month);
        
        if (DateTime.Now.Month == 12 || DateTime.Now.Month == 1)
        {
            menuMusicSource.Stop();
            menuMusicSource.clip = christmasClip;
            menuMusicSource.Play();
        }
        else if (gm != null && gm.rareWindowShown == 1)
        {
            menuMusicSource.Stop();
            menuMusicSource.clip = rearWindowClip;
            menuMusicSource.Play();   
        }
    }


    public void ShowDifficultySettings()
    {
        if (SteamworksLobby.instance)
            SteamworksLobby.instance.ToggleButtons(false);
        
        if (gm.newGamePlus)
        {
            // clear event data selected obj
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedInDifficultyWindow);   
        }
        else
        {
            GameManager.instance.NewGame(0);
        }
    }


    public void ChangeLanguage()
    {
        if (GameManager.instance.language == 0)
        {
            GameManager.instance.language = 1;
        }
        else if (GameManager.instance.language == 1)
        {
            GameManager.instance.language = 2;
        }
        else if (GameManager.instance.language == 2)
        {
            GameManager.instance.language = 3;
        }
        else if (GameManager.instance.language == 3)
        {
            GameManager.instance.language = 4;
        }
        else if (GameManager.instance.language == 4)
        {
            GameManager.instance.language = 5;
        }
        else if (GameManager.instance.language == 5)
        {
            GameManager.instance.language = 0;
        }
        

        startText.text = startButton[gm.language];
         continueButtonText.text = continueButtonL[gm.language];
         quitText.text = quitButton[gm.language];

         SetButtonLocals();
    }

    public void SetButtonLocals()
    {
        for (int i = 0; i < buttonLocals.Count; i++)
        {
            if (buttonLocals[i].textToChange)
                buttonLocals[i].textToChange.text = buttonLocals[i].locals[gm.language];
            else if (buttonLocals[i].textToChangeLegacy)
                buttonLocals[i].textToChangeLegacy.text = buttonLocals[i].locals[gm.language];
        }
    }

    public void ToggleSettingsWindow(bool exitToMenu)
    {
        if (!exitToMenu && UiManager.instance)
        {
            menuWindow.SetActive(false);
            menuVisuals.SetActive(false);
            if (!settingsWindow.activeInHierarchy)
            {
                print("SETTINGS MAKE ACTIVE");
                settingsWindow.SetActive(true);   
            }
            else
            {
                print("SETTINGS MAKE INACTIVE");
                settingsWindow.SetActive(false);
                if (gm.paused)
                    gm.TogglePause(false);
            }
            return;
        }
        
        if (!settingsWindow.activeInHierarchy)
        {
            menuWindow.SetActive(false);
            settingsWindow.SetActive(true);
            
            // clear event data selected obj
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedInOptions);
        }
        else
        {
            menuWindow.SetActive(true);
            settingsWindow.SetActive(false);
            
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedInMenu);
        }
        menuVisuals.SetActive(true);
    }
}


[Serializable]
public class ButtonLocals
{
    public TextMeshProUGUI textToChange;
    public Text textToChangeLegacy;
    [Header("0 eng, 1 rus, 2 spa, 3 ger")]
    public List<string> locals = new List<string>();
}