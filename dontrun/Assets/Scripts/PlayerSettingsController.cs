using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

public class PlayerSettingsController : MonoBehaviour
{
    [Header("0 video; 1 keyboard; 2 mouse; 3 gamepad; 4 audio")]
    public List<GameObject> settingsWindows = new List<GameObject>();

    private string audioString = "MasterVolume";

    private GameManager gm;
    private string volumeString = "MusicVolume";
    public Slider volume;
    public GameObject invertMouseFeedback;

    private void OnEnable()
    {
        gm = GameManager.instance;
        volume.value = gm.volumeSliderValue;
        
        if (gm.mouseInvert == 1)
        {
            invertMouseFeedback.SetActive(true);
        }
        else
        {
            invertMouseFeedback.SetActive(false);
        }
    }
    
    public void OpenWindow(int index)
    {
        for (int i = 0; i < settingsWindows.Count; i++)
        {
            if (index == i)
            {
                settingsWindows[i].SetActive(true);
            }
            else
                settingsWindows[i].SetActive(false);
        }
    }

    // VIDEO
    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void SetResolution(int scale)
    {
        ChangeScreenResolution(scale);

        GameManager.instance.resolution = scale;
        GameManager.instance.SaveGame();
    }

    void ChangeScreenResolution(int scale)
    {
        switch (scale)
        {
            case 0:
                Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow);
                break;
            case 1:
                Screen.SetResolution(2560, 1440, FullScreenMode.FullScreenWindow);
                break;
            case 2:
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
                break;
            case 3:
                Screen.SetResolution(1600, 900, FullScreenMode.FullScreenWindow);
                break;
            case 4:
                Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow);
                break;
            case 5:
                Screen.SetResolution(960, 540, FullScreenMode.FullScreenWindow);
                break;
            case 6:
                Screen.SetResolution(640, 360, FullScreenMode.FullScreenWindow);
                break;
            case 7:
                Screen.SetResolution(480, 270, FullScreenMode.FullScreenWindow);
                break;
        }
    }

    public void SetVolume(float volume)
    {
        GameManager.instance.SetVolume(volume);
        GameManager.instance.volumeSliderValue = volume;
    }

    public void ToggleInvertMouse()
    {
        if (gm.mouseInvert == 0)
        {
            invertMouseFeedback.SetActive(true);
            gm.mouseInvert = 1;
        }
        else
        {
            invertMouseFeedback.SetActive(false);
            gm.mouseInvert = 0;   
        }
    }
}
