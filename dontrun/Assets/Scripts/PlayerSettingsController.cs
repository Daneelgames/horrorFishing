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

    public AudioMixer mixer;
    private string audioString = "MasterVolume";

    private GameManager gm;
    private string volumeString = "MusicVolume";
    public Slider volume;
    public GameObject tutorialHintsFeedback;
    public GameObject lowPitchNoiseFeedback;
    public GameObject bloodMistFeedback;
    
    public GameObject bloomFeedback;
    public GameObject pixelsFeedback;
    public GameObject ditheringFeedback;
    public GameObject edgeDetectionFeedback;
    public GameObject doubleVisionFeedback;

    private void OnEnable()
    {
        gm = GameManager.instance;
        volume.value = gm.volumeSliderValue;
        
        if (gm.tutorialHints == 0)
            tutorialHintsFeedback.SetActive(true);
        else
            tutorialHintsFeedback.SetActive(false);
        
        if (gm.bloodMist == 1)
            bloodMistFeedback.SetActive(true);
        else
            bloodMistFeedback.SetActive(false);

        if (gm.lowPitchDamage == 1)
            lowPitchNoiseFeedback.SetActive(true);
        else
            lowPitchNoiseFeedback.SetActive(false);
        
        /*
        if (gm.grain == 0)
        {
            grainFeedback.SetActive(false);
        }
        else
        {
            grainFeedback.SetActive(true);
        }
        */
        
        if (gm.bloom == 1)
        {
            bloomFeedback.SetActive(false);
        }
        else
        {
            bloomFeedback.SetActive(true);
        }
        
        if (gm.pixels == 1)
        {
            pixelsFeedback.SetActive(false);
        }
        else
        {
            pixelsFeedback.SetActive(true);
        }
        
        if (gm.doubleVision == 1)
        {
            doubleVisionFeedback.SetActive(false);
        }
        else
        {
            doubleVisionFeedback.SetActive(true);
        }
        
        if (gm.edgeDetection == 1)
        {
            edgeDetectionFeedback.SetActive(false);
        }
        else
        {
            edgeDetectionFeedback.SetActive(true);
        }
        
        if (gm.dithering == 1)
        {
            ditheringFeedback.SetActive(false);
        }
        else
        {
            ditheringFeedback.SetActive(true);
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

    public void ToggleBloodMist()
    {
        gm.ToggleBloodMist();
        if (gm.bloodMist == 1)
            bloodMistFeedback.SetActive(true);
        else
            bloodMistFeedback.SetActive(false);
    }
    
    public void SetVolume(float volume)
    {
        GameManager.instance.SetVolume(volume);
        GameManager.instance.volumeSliderValue = volume;
    }

    public void ToggleTutorialHints()
    {
        if (gm.tutorialHints == 0)
        {
            tutorialHintsFeedback.SetActive(false);
            gm.tutorialHints = 1;
        }
        else
        {
            tutorialHintsFeedback.SetActive(true);
            gm.tutorialHints = 0;   
        }
    }
    
    public void ToggleLowPitchNoise()
    {
        if (gm.lowPitchDamage == 0)
        {
            lowPitchNoiseFeedback.SetActive(true);
            gm.lowPitchDamage = 1;
        }
        else
        {
            lowPitchNoiseFeedback.SetActive(false);
            gm.lowPitchDamage = 0;   
        }
    }
    
    public void ToggleBloom()
    {
        if (gm.bloom == 0)
        {
            bloomFeedback.SetActive(false);
            gm.bloom = 1;
        }
        else
        {
            bloomFeedback.SetActive(true);
            gm.bloom = 0;   
        }

        var ui = UiManager.instance;
        if (ui)
        {
            ui.SetBloom(gm.bloom);
        }
    }
    public void TogglePixels()
    {
        if (gm.pixels == 0)
        {
            pixelsFeedback.SetActive(false);
            gm.pixels = 1;
        }
        else
        {
            pixelsFeedback.SetActive(true);
            gm.pixels = 0;   
        }

        var ui = UiManager.instance;
        if (ui)
        {
            ui.SetPixels(gm.pixels);
        }
    }
    public void ToggleDoubleVision()
    {
        if (gm.doubleVision == 0)
        {
            doubleVisionFeedback.SetActive(false);
            gm.doubleVision = 1;
        }
        else
        {
            doubleVisionFeedback.SetActive(true);
            gm.doubleVision = 0;   
        }

        var ui = UiManager.instance;
        if (ui)
        {
            ui.SetDoubleVision(gm.doubleVision);
        }
    }
    public void ToggleDithering()
    {
        if (gm.dithering == 0)
        {
            ditheringFeedback.SetActive(false);
            gm.dithering = 1;
        }
        else
        {
            ditheringFeedback.SetActive(true);
            gm.dithering = 0;   
        }

        var ui = UiManager.instance;
        if (ui)
        {
            ui.SetDithering(gm.dithering);
        }
    }
    public void ToggleEdgeDetection()
    {
        if (gm.edgeDetection == 0)
        {
            edgeDetectionFeedback.SetActive(false);
            gm.edgeDetection = 1;
        }
        else
        {
            edgeDetectionFeedback.SetActive(true);
            gm.edgeDetection = 0;   
        }

        var ui = UiManager.instance;
        if (ui)
        {
            ui.SetEdgeDetection(gm.edgeDetection);
        }
    }
}
