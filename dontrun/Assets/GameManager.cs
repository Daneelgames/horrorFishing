﻿using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Mirror;
using Mirror.FizzySteam;
using PlayerControls;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public enum GameMode
    {
        StickyMeat,
        Ikarus,
        MeatZone
    }

    public GameMode difficultyLevel = GameMode.StickyMeat;

    public static GameManager instance;
    public bool demo = false;
    public bool cheats = true;
    public List<AssetReference> scenesReferences;
    public LevelData tutorialLevel;
    public List<int> goldenKeysFoundOnFloors = new List<int>(); 
    public List<int> goldenKeysFoundInHub = new List<int>(); 
    public List<int> tapesFoundOfFloors = new List<int>(); 
    //public List<LevelData> levels;
    public LevelData level;
    public bool arena;
    public LevelData arenaLevel;

    public List<HealthController> units = new List<HealthController>();
    public HealthController player;
    public float mouseSensitivity = 100;
    public float mouseLookSpeed = 30;
    public float mouseLookSpeedCurrent = 30;

    public int mouseInvert = 0;
    public float brightness = 0.25f;
    public float contrast = 30;

    public int keysFound = 0;

    public ItemsList itemList;

    Vector3 playerStartPos;
    Quaternion playerStartRot;

    public LevelStreamer ls;
    public LevelGenerator lg;
    public UiManager ui;
    public ToolsRandomizer tr;
    public SpawnController sc;
    public MenuController menuController;
    public GameObject menuSettings;
    public EpigraphsData epigraphsData;
    private bool loading = false;

    public Animator loadingAnim;
    public Camera loadingCam;

    public bool paused = false;
    public bool questWindow = false;
    public bool mementoWindow = false;

    public bool hub = false;
    public int wasInHub = 0; // 0 - was, 1 - wasnt
    public int hubVisits = 0;

    // 0 - eng, 1 - rus, 2 - esl, 3 - ger, 3 - Ita, 3 - Sp-BR
    public int language = 0;
    public int tutorialPassed = 0;
    public int resolution = 2;

    public bool readyToStartLevel = false;

    private LoadingHintsController lhc;

    [HideInInspector] public QuestManager qm;

    public bool creditsActive = false;
    public List<GameObject> credits = new List<GameObject>();
    public GameObject creditsParent;
    
    
    SkillInfo firstMementoToChoose;
    SkillInfo secondMementoToChoose;

    AsyncOperation AOfield;
    AsyncOperation AOlevel;

    public List<RoomTilesPositionsManager> loadedRtps = new List<RoomTilesPositionsManager>();

    private AsyncOperationHandle<SceneInstance> activeSceneOp;
    
    public Material procMapMaterialPrefab;
    public Material procMapMaterialInstance;
    public RenderTexture renderTexturePrefab;
    public RenderTexture renderTextureInstance;
    public AudioMixer mixer;

    private string questsString = "Quests";
    private string crouchString = "Crouch";
    private string dashString = "Dash";
    private string cancelString = "Cancel";
    
    private string volumeString = "MusicVolume";

    public AudioSource finishLevelJingleSource;

    public float volumeSliderValue = 0.5f;

    // 0 is false
    public int darkness = 0;

    public int savedOnFloor = 0; // 0 is false

    // 0 is true
    public int tutorialHints = 0;
    public int grain = 0;
    public int bloom = 0;
    public int dithering = 0;
    public int doubleVision = 0;
    public int pixels = 0;
    public int edgeDetection = 0;
    
    public int lowPitchDamage = 0;
    
    //[Header("Meat Game")] public MeatGameController meatGame;

    public int bloodMist = 1;
    
    //stats
    public int rareWindowShown = 0;
    public int levelsOnRun = 0;
    public int goldOnRun = 0;
    public int overallDeaths = 0;
    public int overallKills = 0;
    
    public int snoutFound = 0;

    public int hubActiveCheckpointIndex = 0;
    public List<int> permanentPickupsTakenIndexesInHub = new List<int>();
    public List<int> roseNpcsInteractedInHub = new List<int>();
    public List<int> cutscenesInteractedInHub = new List<int>();
    public List<int> doorsOpenedIndexesInHub = new List<int>();

    public Camera mapCamera;
    public GameObject mapBackgroundFx;

    public int coopBiomeCheckpoints = 0;
    private int randomSeed = 0;
    public int notesPickedUp = -1;
    public int flashbackSceneIndex = 0;
    public bool newGamePlus = false;
    public bool cagesCompleted = false;
    public int lastTalkedDyk = -1;

    public bool revolverFound = false;
    public bool ladyshoeFound = false;
    
    void Awake()
    {
        randomSeed = (int)System.DateTime.Now.Ticks;
        Random.InitState(randomSeed);
        
        if (instance)
            Destroy(gameObject);
        
        instance = this;
        
    }

    void Start()
    {
        if (procMapMaterialPrefab && renderTexturePrefab)
        {
            procMapMaterialInstance = Instantiate(procMapMaterialPrefab);
            renderTextureInstance = Instantiate(renderTexturePrefab);
            procMapMaterialInstance.SetTexture(0, renderTextureInstance);   
        }
        
        qm = QuestManager.instance;
        LoadGame();
        menuController.SetButtonLocals();
        
        SetVolume(volumeSliderValue);
        
        var lobby = SteamworksLobby.instance;
        if (lobby) lobby.SetCheckpoints(coopBiomeCheckpoints);
        
        savedOnFloor = 0;
        
        switch (resolution)
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

    public void PreloadNextLevelRooms()
    {
        if (loadedRtps.Count > 0)
        {
            for (int i = loadedRtps.Count - 1; i >= 0; i--)
            {
                if (loadedRtps[i] != null)
                    Destroy(loadedRtps[i].gameObject);
                
                loadedRtps.RemoveAt(i);
            }
        }
        loadedRtps.Clear();
        
        if (tutorialPassed == 0)
            StartCoroutine(PreloadRooms(tutorialLevel));
        else if (arena)
            StartCoroutine(PreloadRooms(arenaLevel));
        else
            StartCoroutine(PreloadRooms(level));
    }

    IEnumerator PreloadRooms(LevelData ld)
    {
        if (loadedRtps.Count > 0)
        {
            for (int i = loadedRtps.Count - 1; i >= 0; i--)
            {
                if (loadedRtps[i] != null)
                    Destroy(loadedRtps[i].gameObject);
                
                loadedRtps.RemoveAt(i);
            }
        }
        loadedRtps.Clear();
        
        loadedRtps.Add(Instantiate(ld.startRoomTilesPositionsManager, Vector3.zero, Quaternion.identity));

        for (var index = 0; index < ld.roomTilesPositionsManagers.Count; index++)
        {
            var rtp = ld.roomTilesPositionsManagers[index];
            var newRtp = Instantiate(rtp, Vector3.zero, Quaternion.identity);
            loadedRtps.Add(newRtp);
        }

        yield return null;
    }

    public void CoopButton()
    {
        var s = SteamworksLobby.instance;
        if (s && !s.hosting)
            s.ToggleButtons(true);
    }

    public void StopHostingButton()
    {
        var s = SteamworksLobby.instance;
        
        if (s)
        {
            s.ToggleButtons(false);
            s.ToggleConnectionFeedback(false);
            if (s.hosting)
            {
                s.hosting = false;
                NetworkManager.singleton.StopHost();   
            }
        }
    }

    public void StartCoopGame()
    {
        if (GLNetworkWrapper.instance.coopIsActive && GLNetworkWrapper.instance.localPlayer.isServer && Cursor.visible && !loading)
        {
            var newFloor = 1;
            //var newFloor = 2;
            SteamworksLobby lobby = SteamworksLobby.instance;

            lobby.ToggleButtons(false);
            switch (lobby.selectedCheckpoint)
            {
                case 1:
                    newFloor = 4;
                    break;
                case 2:
                    newFloor = 7;
                    break;
                case 3:
                    newFloor = 10;
                    break;
                case 4:
                    newFloor = 13;
                    break;
                case 5:
                    newFloor = 16;
                    break;
            }
            
            GutProgressionManager.instance.SetLevel(newFloor, lobby.selectedCheckpoint);
            //GutProgressionManager.instance.SetLevel(8, 2);
            
            GutProgressionManager.instance.bossFloor = newFloor + 2;
            
            var network = GLNetworkWrapper.instance;
            
            /*
            if (network != null && network.coopIsActive)
            {
                network.SaveRandomSeed(randomSeed);
            }
            */
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (TwitchManager.instance)
            {
                TwitchManager.instance.ToggleCanvasAnim(false);   
                TwitchManager.instance.ToggleMeatchImages(false);
            }
            
            //menuController.gameObject.SetActive(false);
            
            player = null;
            units.Clear();
            ui = null;

            GLNetworkWrapper.instance.StartCoopGame(true);
        }   
    }

    public void ContinueGameButton()
    {
        if (Cursor.visible && !loading)
        {
            if (GLNetworkWrapper.instance) GLNetworkWrapper.instance.coopIsActive = false;

            if (SteamworksLobby.instance)
            {
                SteamworksLobby.instance.ToggleButtons(false);
                SteamworksLobby.instance.ToggleConnectionFeedback(false);   
            }
            Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = false;
            lhc = LoadingHintsController.instance;
            if (savedOnFloor == 0) // load hub
                StartCoroutine(StartGame());
            else // load floor
            {
                if (TwitchManager.instance)
                {
                    TwitchManager.instance.ToggleCanvasAnim(false);   
                    TwitchManager.instance.ToggleMeatchImages(false);
                }
                player = null;
                units.Clear();
                ui = null;
                
                StartCoroutine(LoadGameScene(true, 1, true));
            }
        }
    }

    public void NewGame(int diff)
    {
        if (Cursor.visible && !loading)
        {
            if (GLNetworkWrapper.instance) GLNetworkWrapper.instance.coopIsActive = false;
            if (SteamworksLobby.instance)
                SteamworksLobby.instance.ToggleConnectionFeedback(false);
            
            DeleteLocalSave(false);
            if (diff == 0)
                difficultyLevel = GameMode.StickyMeat;
            else if (diff == 1)
                difficultyLevel = GameMode.Ikarus;
            else if (diff == 2)
                difficultyLevel = GameMode.MeatZone;

            Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = false;
            lhc = LoadingHintsController.instance;
            SaveGame();
            StartCoroutine(StartGame());
        }
    }

    public void AddUnit(HealthController h)
    {
        if (h.player && h.wc && h.playerMovement && h.playerNetworkDummyController == null)
        {
            player = h;
            playerStartPos = player.transform.position;
            playerStartRot = player.transform.rotation;
        }

        units.Add(h);
    }

    public void ExitTheGame()
    {
        if (!hub)
            SaveGame();
        
        Application.Quit();
    }

    private void Update()
    {
        if (PlayerMovement.instance)
           player = PlayerMovement.instance.hc;
        
        if (ui == null) ui = UiManager.instance;
        
        if (player && player.health > 0)
        {
            if (player.playerMovement && player.playerMovement.inTransport && (Input.GetButtonDown(dashString) || KeyBindingManager.GetKeyDown(KeyAction.Dash)))
            {
                player.playerMovement.inTransport.ExitTransport();   
            }
        }
        
        lg = LevelGenerator.instance;
        
        if (player && player.health > 0 && (player.playerMovement.controller.enabled || (!player.playerMovement.controller.enabled && player.playerMovement.inTransport)) && (!lg || !lg.generating))
        {
            if (Input.GetKeyDown("u"))
                UiManager.instance.ToggleGameUi(false, true);
            
            if (Input.GetKeyDown("i"))
                UiManager.instance.ToggleGameUi(true, true);

            if (cheats && Input.GetKey("g") && Input.GetKey("z"))
            {
                // cheats
                if (Input.GetKeyDown("6"))
                    ReturnToHub(true, false);
                
                if (Input.GetKeyDown("l"))
                    PlayerSkillsController.instance.AddSkill(itemList.skillsData.skills[33]);

                if (Input.GetKeyDown("f"))
                    ItemsList.instance.AddToBadReputation(5);

                /*
                if (Input.GetKeyDown("n"))
                {
                    if (!hub) 
                        GutProgressionManager.instance.PlayerFinishLevel();
                    NextLevel();   
                }
                */
                
                if (Input.GetKeyDown("m"))
                {
                    // milk
                    PlayerSkillsController.instance.SwapProps();
                }
                
                if (Input.GetKeyDown("t"))
                    AllTools();   
                
                if (Input.GetKeyDown("x"))
                    GiveRandomStatus();   
                
                if (Input.GetKeyDown("w"))
                    AiDirector.instance.BreakClosestWall(true, 1000);   
                
                if (Input.GetKeyDown("r"))
                    PlayerSkillsController.instance.InstantTeleport(PlayerMovement.instance.transform.position);

                if (Input.GetKeyDown("p"))
                    AiDirector.instance.RotatePropFromShortCut();   
                
                if (Input.GetKeyDown("s"))
                    GetSkill();

                if (Input.GetKeyDown("k"))
                {
                    print("K");
                    PlayerMovement.instance.hc.Kill();
                    /*
                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.localPlayer != null)
                        GLNetworkWrapper.instance.localPlayer.connectedDummy.hc.Kill();*/
                }

                if (Input.GetKeyDown("b"))
                {
                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.localPlayer != null)
                    {
                        var newEffect = new StatussEffectsOnAttack();
                        var statusEffect = new StatusEffects();
                        statusEffect.depletionSpeed = 1;
                        statusEffect.effectActive = true;
                        statusEffect.effectType = StatusEffects.StatusEffect.Bleed;
                        statusEffect.effectValue = 100;

                        var effectValues = new List<float>();
                        effectValues.Add(100);
                        
                        newEffect.effects.Add(statusEffect);
                        newEffect.effectsValues = new List<float>(effectValues);
                        
                        GLNetworkWrapper.instance.localPlayer.connectedDummy.hc.Damage(100, Vector3.zero, Vector3.zero, null, null, 
                            false, null, null, newEffect, true);   
                    }
                    
                    //itemList.GenerateAllWeaponsOnFloor();  
                }
                
                if (Input.GetKeyDown("t"))
                    itemList.GetRandomTool();

                if (Input.GetKeyDown("c"))
                    player.invincible = !player.invincible;
                
                if (Input.GetKeyDown("d"))
                    StartCoroutine(KillAll());      
                
                if (Input.GetKeyDown("h") && hub)
                    EndingController.instance.StartDarknessBeforeEnding();
            }
            

            if (UiManager.instance && !questWindow && !mementoWindow)
            {
                if (Input.GetButtonDown(cancelString))
                {
                    TogglePause(true);
                }
                
                if (!Application.isFocused)
                {
                    if (!paused)
                        TogglePause(true);
                }
            }
            
            if (mementoWindow)
            {
                if (Input.GetButtonDown(dashString) || KeyBindingManager.GetKeyDown(KeyAction.Dash))
                {
                    ui.ChooseMemento(0);
                }
                else if (Input.GetButtonDown(crouchString) || KeyBindingManager.GetKeyDown(KeyAction.Crouch))
                {
                    ui.ChooseMemento(1);
                }
            }

            if (!ui || paused || mementoWindow) return;
            if(Input.GetButtonDown(questsString) || KeyBindingManager.GetKeyDown(KeyAction.Quests) || questWindow && Input.GetButtonDown(cancelString))
                ToggleQuests();
        }
        
        /*
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {/*
                if (Cursor.lockState != CursorLockMode.None)
                    Cursor.lockState = CursorLockMode.None;
                else
                    Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = !Cursor.visible;
                #1#

                if (MouseLook.instance)
                {
                    MouseLook.instance.debugMap.SetActive(!MouseLook.instance.debugMap.activeInHierarchy);
                }
            }
        }*/
    }

    IEnumerator KillAll()
    {
        for (int i = units.Count - 1; i >= 0; i--)
        {
            if (units[i].player == false)
            {
                units[i].Damage(10000, units[i].transform.position, units[i].transform.position ,null, null, false, null, null, null, false);
                yield return null;
            }
        }
    }

    void AllTools()
    {
        for (var index = 0; index < itemList.savedTools.Count; index++)
        {
            var t = itemList.savedTools[index];
            t.amount++;
        }
    }

    void GetSkill()
    {
        ResourcePickUp newSkill = new ResourcePickUp();
        newSkill.resourceType = ItemsList.ResourceType.Skill;
        itemList.PickResource(newSkill);
    }
    
    public void TogglePause(bool togglePlayerUiPause) // if not pause than quest window
    {
        if (!paused)
        {
            paused = true;
            if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                Time.timeScale = 0;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (TwitchManager.instance)
                TwitchManager.instance.ToggleMeatchImages(true);
            if (ui)
                ui.VolumeFromScript(0.0001f);
        }
        else
        {
            paused = false;
            if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                Time.timeScale = 1;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (TwitchManager.instance)
            {
                TwitchManager.instance.ToggleMeatchImages(false);
                TwitchManager.instance.ToggleCanvasAnim(false);
            }
            if (ui)
            {
                ui.VolumeFromScript(volumeSliderValue);
            }
        }
        if (togglePlayerUiPause && ui)
            ui.TogglePause();
    }

    public void ToggleQuests()
    {
        if (questWindow)
        {
            questWindow = false;
            /*
            if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;*/
        }
        else
        {
            itemList.SaveWeapons();
            questWindow = true;
            
            /*
            if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;*/
        }
        ui.ToggleQuestWindow();
    }
    
    public void Restart(int _hub) // 0 - returnToHub, 1 - returnToLevel
    {
        if (!loading)
        {
            paused = false;
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ui.TogglePause();
            
            itemList.ResetPlayerInventory();
            SaveGame();
            
            StartCoroutine(LoadGameScene(true, _hub, false));
        }
    }

    public void NextLevel()
    {
        if (!loading)
        {
            if (player && GutProgressionManager.instance.playerFloor > 0 && !hub && tutorialPassed == 1)
            {
                // no damage achievement
                if (player.damagedOnLevel == false && !demo)
                {
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_2_6");   
                }   
            }
            
            //next dungeon level is loading

            
            StartCoroutine(LoadGameScene(false, 1, true));
        }
    }

    public void ReturnToHub(bool forceRareWindow, bool savePlayerStuff)
    {
        print(loading);
        if (!loading)
        {
            if (TwitchManager.instance)TwitchManager.instance.StopVoting();
            if (forceRareWindow || GutProgressionManager.instance.playerFloor > 10 && hubVisits > 10 && rareWindowShown == 0 && Random.value < 0.1f)
            {
                rareWindowShown = 1;
                SaveGame();
                SceneManager.LoadScene(16);
            }
            else
            {
                hub = true;
                StartCoroutine(LoadGameScene(true, 0, savePlayerStuff));   
            }
        }
    }

    public void ReturnToMenu(bool credits)
    {
        StartCoroutine(ReturnToMainMenu(credits));
    }
    
    public IEnumerator ReturnToMainMenu(bool showCredits)
    {
        SaveGame();
        MoveLoadingAnimToParent(transform);
        StopHostingButton();
        ItemsList.instance.savedTools.Clear();
        paused = false;
        Time.timeScale = 1;
        ui.TogglePause();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        itemList.keys = 0;
        
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        
        print("wait until scene unloads");
        
        yield return new WaitForEndOfFrame();
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));

        print("menu active");
        
        if (showCredits)
        {
            ToggleCredits();
        }
        
        //menuController.ToggleSettingsWindow( true);
        menuController.menuWindow.SetActive(true);
        menuController.menuVisuals.SetActive(true);
        menuController.settingsWindow.SetActive(false);
    }

    public void ToggleCredits()
    {
        creditsActive = !creditsActive;
        
        creditsParent.SetActive(creditsActive);
        // show credits
        for (var index = 0; index < credits.Count; index++)
        {
            var c = credits[index];
            c.SetActive(false);
        }

        if (creditsActive)
            credits[language].SetActive(true);
    }

    
    public void IntroLevelFinish()
    {
        if (!loading)
            StartCoroutine(LoadGameScene(false, 1, false));
    }

    public void LoadArena()
    {
        StartCoroutine(LoadGameScene(false, 2, true));   
    }

    void MoveLoadingAnimToParent(Transform parent)
    {
        loadingAnim.gameObject.transform.parent = parent;
        loadingAnim.gameObject.transform.localPosition = Vector3.zero;
        loadingAnim.gameObject.transform.localRotation = Quaternion.identity;
    }
    
    public IEnumerator StartGame()
    {
        mouseLookSpeedCurrent = mouseLookSpeed;
        if (TwitchManager.instance)
        {
            TwitchManager.instance.ToggleCanvasAnim(false);   
            TwitchManager.instance.ToggleMeatchImages(false);
        }
        loading = true;
        
        MoveLoadingAnimToParent(transform);
        loadingAnim.SetBool("Active", true);
        loadingCam.gameObject.SetActive(true);
        lhc.StartHints();
        yield return  new WaitForSeconds(1f);
        
        player = null;
        units.Clear();
        ui = null;
        
        hub = true;
        
        yield return new WaitForEndOfFrame();
        
        AOfield = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive); // load hub   
        AOfield.allowSceneActivation = false;
        
        while (!AOfield.isDone)
        {
            yield return null;
            if (AOfield.progress >= 0.9f)
            {
                AOfield.allowSceneActivation = true;
            }
        }

        AOfield.allowSceneActivation = true;
        AOfield = null;
        
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));

        yield return null;

        /*
        if (tutorialPassed == 0)
            DeleteLocalSave(false);
            */

        player = PlayerMovement.instance.hc;
        player.transform.position = playerStartPos;
        player.transform.rotation = playerStartRot;

        ui = UiManager.instance;
        WeaponGenerationManager.instance.Init();

        if (lg && !hub)
            lg.Init();
        
        tr = ToolsRandomizer.instance;
        tr.Init();

        if (tutorialPassed == 0)
            itemList.ResetPlayerInventory();
        
        itemList.Init();
        itemList.keys = 0;
        if (tutorialPassed == 0)
            ui.Init(false);
        else 
            ui.Init(false);
        
        MoveLoadingAnimToParent(MouseLook.instance.mainCamera.transform);
        loadingAnim.SetBool("Active", false);
        loadingCam.gameObject.SetActive(false);
        
        yield return  new WaitForSeconds(1f);
        loading = false;
        lhc.StopHints();
        
        if (ls)
            ls.Init();
        print("FIELD LOADED");
        player.playerMovement.StartLevel();
        
        /*
        if (tutorialPassed == 0)
        {
            qm.StartQuest(0);
        }*/
        
        if (QuestManager.instance)
            QuestManager.instance.Init();
            
        if (HubProgressionManager.instance)
            HubProgressionManager.instance.FieldAfterDeath(true);
            
        if (HubItemsSpawner.instance)
            HubItemsSpawner.instance.UpdateHub();
            
        if (PlayerCheckpointsController.instance)
            StartCoroutine(PlayerCheckpointsController.instance.Init());
        
        player.pac.Init();
        if (paused) paused = false;
        
        yield return  new WaitForSeconds(0.1f);
        //PlayerCheckpointsController.instance.Init();
        
        yield return  new WaitForSeconds(1f);
    }

    // on player death or when going on deeper level
    public IEnumerator LoadGameScene(bool restart, int returnTo, bool savePlayerStuff) // restart: has player died?;  returnTo: 0 - hub, 1 - level, 2 - arena
    {
        loading = true;
        if (returnTo == 1)
            yield return new WaitForSeconds(3);
        if (sc)
            sc.FinishLevel();
        
        readyToStartLevel = false;
        GutQuestsController.instance.ClearQuests();
        
        bool bikeInLevel = !restart && player && player.playerMovement.inTransport && returnTo == 1;
        
        GutProgressionManager.instance.Init();

        if (restart && returnTo == 1 && savePlayerStuff)
        {
            
        }
        else
        {
            itemList.SaveWeapons();   
        }
        
        SaveGame();
        mouseLookSpeedCurrent = mouseLookSpeed;
        
        yield return null;
        
        keysFound = 0;
        itemList.ClearDataFromLevel();
        units.Clear();
        
        if (loadedRtps.Count > 0)
        {
            for (int i = loadedRtps.Count - 1; i >= 0; i--)
            {
                if (loadedRtps[i] != null)
                    Destroy(loadedRtps[i].gameObject);
                
                loadedRtps.RemoveAt(i);
            }
        }
        loadedRtps.Clear();
        
        if (sc)
            sc.ClearData();
        if (lg)
            lg.ClearData();
        if (ls)
            ls.ClearData();
        
        if (restart)
        {
            arena = false;
            
            if (returnTo == 0)
            {
                hub = true;
                wasInHub = 1;
            }
            else if (returnTo == 1)
                hub = false;
            
            mouseLookSpeedCurrent = mouseLookSpeed;
        }
        else
        {
            if (!hub)
            {
                if (tutorialPassed == 0)
                    tutorialPassed = 1;
            }
        }
        
        yield return null;
        
        // UNLOAD SCENE
        AsyncOperation AOU;
        AOU = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        yield return AOU;
        
        /// LOAD SCENE
        if (tutorialPassed == 1)
        {
            print(returnTo);
            if (returnTo == 0)
            {
                print("load hub");
                AOlevel = SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive); // load hub
            }
            else
            {
                AOlevel = SceneManager.LoadSceneAsync(4, LoadSceneMode.Additive); // load game
            }
            
            if (returnTo == 2)
                arena = true;
            
            AOlevel.allowSceneActivation = false;
        }
        else if (tutorialPassed == 0)
        {
            print(returnTo);
            
            if (returnTo == 0)
            {
                print("load hub");
                AOlevel = SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive); // load hub   
            }
            else
            {
                AOlevel = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive); // load tutorial   
            }
            
            AOlevel.allowSceneActivation = false;
        }

        
        while (!AOlevel.isDone)
        {
            if (AOlevel.progress >= 0.9f)
            {
                AOlevel.allowSceneActivation = true;
            }

            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        
        PreloadNextLevelRooms();

        ui = UiManager.instance;

        player = PlayerMovement.instance.hc;
        
        if (restart) // restart in hub or in level
        {
            //player.health = player.healthMax;
            itemList.Init();
        }
        else if (hub) // go from hub to level
        {
            player.health = player.healthMax;
            hub = false;
            itemList.Init();
        }
        else // go on a new floor
        {
            itemList.Init();
        }

        if (!hub)
        {
            // LOAD FLASHBACK SCENE
            flashbackSceneIndex = GetCurrentFlashBackSceneIndex();
            AOlevel = SceneManager.LoadSceneAsync(flashbackSceneIndex, LoadSceneMode.Additive); // load flashback   
            AOlevel.allowSceneActivation = false;
            while (!AOlevel.isDone)
            {
                if (AOlevel.progress >= 0.9f)
                {
                    AOlevel.allowSceneActivation = true;
                }

                yield return null;
            }
            /////
        }
        
        // GENERATE LEVEL
        if (lg && !hub)
        {
            lg.Init();
            lg.generating = true;
        }

        while (lg.generating)
        {
            yield return null;
        }
        /////////

        if (restart)
        {
            tr = ToolsRandomizer.instance;
            tr.Init();
            if (hub)
            {
                HubProgressionManager.instance.FieldAfterDeath(true);
            }
        }
        
        itemList.keys = 0;

        readyToStartLevel = true;
        
        MoveLoadingAnimToParent(MouseLook.instance.mainCamera.transform);
        loadingAnim.SetBool("Active", false);
        loadingCam.gameObject.SetActive(false);
        
        yield return  new WaitForSeconds(1f);
        
        loading = false;

        if (bikeInLevel && sc.bike) sc.bike.SetActive(true);
        
        player.playerMovement.StartLevel();
        
        player.wc.FindNewTool();
        if (ls)
            ls.Init();
        
        SaveGame();
        
        if (hub)
            ui.savedHint.SetBool("Active", true);
        else
        {
            itemList.GenerateAllWeaponsOnFloor();
        }
        
        player.pac.Init();
        if (paused) paused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        QuestManager.instance.Init();

        ui.Init(false);
        HubItemsSpawner.instance.UpdateHub();
        StartCoroutine(PlayerCheckpointsController.instance.Init());
    }

    
    public IEnumerator LoadCoopGameScene(bool restart)
    {
        // called everywhere
        
        if (restart)
        {
            itemList.savedTools.Clear();
            itemList.AddToBadReputation(-5);
        }
        
        SteamworksLobby.instance.ToggleButtons(false);
        SteamworksLobby.instance.ToggleConnectionFeedback(false);
        
        if (TwitchManager.instance)
        {
            TwitchManager.instance.ToggleCanvasAnim(false);   
            TwitchManager.instance.ToggleMeatchImages(false);
        }
        
        MoveLoadingAnimToParent(transform);
        loadingAnim.SetBool("Active", true);
        loadingCam.gameObject.SetActive(true);
        yield return  new WaitForSeconds(1f);
        
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        loading = true;
        readyToStartLevel = false;
        GutQuestsController.instance.ClearQuests();
        
        SaveGame();
        mouseLookSpeedCurrent = mouseLookSpeed;
        
        yield return null;

        keysFound = 0;
        itemList.ClearDataFromLevel();
        units.Clear();
        
        if (loadedRtps.Count > 0)
        {
            for (int i = loadedRtps.Count - 1; i >= 0; i--)
            {
                if (loadedRtps[i] != null)
                    Destroy(loadedRtps[i].gameObject);
                
                loadedRtps.RemoveAt(i);
            }
        }
        loadedRtps.Clear();
        
        if (sc)
            sc.ClearData();
        if (lg)
            lg.ClearData();
        if (ls)
            ls.ClearData();
        
        yield return null;
        
        GutProgressionManager.instance.Init();
        
        PreloadNextLevelRooms();

        print("Is local player a server? " + GLNetworkWrapper.instance.localPlayer.isServer);
        if (GLNetworkWrapper.instance.localPlayer.isServer)
        {
            GLNetworkWrapper.instance.LoadCoopSceneOnServer(restart);
        }
    }

    public IEnumerator GenerateCoopLevelOnServer(bool restart)
    {
        #region old way to load scene
        /*
        // LOAD COOP GAME SCENE
        AOlevel = SceneManager.LoadSceneAsync(4, LoadSceneMode.Additive);
        AOlevel.allowSceneActivation = false;
        
        while (!AOlevel.isDone)
        {
            if (AOlevel.progress >= 0.9f)
            {
                AOlevel.allowSceneActivation = true;
            }
            yield return null;
        }
        
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        */
        #endregion
        
        ui = UiManager.instance;
        player = PlayerMovement.instance.hc;
        itemList.Init();
        
        // GENERATE LEVEL
        lg.Init();
        lg.generating = true;

        while (lg.generating)
        {
            yield return null;
        }
        /////////

        if (restart)
        {
            tr = ToolsRandomizer.instance;
            tr.Init();   
        }
        
        // Scenes are loaded, now send stuff to RPCs

        print("Scenes are loaded, now send stuff to RPCs");
        ui.Init(true);
        GLNetworkWrapper.instance.InitCoopLevelOnClient();
    }
    
    public IEnumerator InitCoopLevelOnClient()
    {
        // RUN THIS ON CLIENTS WHEN SCENE IS LOADED   
        itemList.keys = 0;
        readyToStartLevel = true;
        
        MoveLoadingAnimToParent(MouseLook.instance.mainCamera.transform);
        loadingAnim.SetBool("Active", false);
        loadingCam.gameObject.SetActive(false);
        
        yield return  new WaitForSeconds(1f);
        
        loading = false;
        
        player.playerMovement.StartLevel();
        player.wc.FindNewTool();
        
        if (ls)
            ls.Init();
        
        SaveGame();
        
        itemList.GenerateAllWeaponsOnFloor();
        
        player.pac.Init();
        
        if (paused) paused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        QuestManager.instance.Init();
        
        UiManager.instance.Init(false);

        LevelGenerator.instance.generating = false;

        /*
        var tm = TwitchManager.instance;
        var ti = TwitchIrc.Instance;
        if (tm && ti != null && ti.stream != null)
        {
            yield return new WaitForSeconds(tm.voteTime);
            tm.StartNewVoting();   
        }
        */
    }

    int GetCurrentFlashBackSceneIndex()
    {
        var gpm = GutProgressionManager.instance;
        
        //use this for testing flashback rooms
        //return Random.Range(5,16); // random 

        if (tutorialPassed == 0)
            return 5; // loner flat
        
        if (gpm.playerFloor == 1)
            return 6; // phone call
        
        if (gpm.playerFloor == 2) 
            return 7; // street flat
         
        if (gpm.playerFloor == 3) // carre fight
            return 8; // date
         
        if (gpm.playerFloor == 4) // factory
            return 9; // she in gold
        
        if (gpm.playerFloor == 5) // factory
            return Random.Range(5,15); // random 
        
        if (gpm.playerFloor == 6) // factory boss
            return 19; // birthday 
        
        if (gpm.playerFloor == 7) // cages
            return 10; // he
        
        if (gpm.playerFloor == 8) // cages
            return Random.Range(10,11); // he / tvs
        
        if (gpm.playerFloor == 9) // cages boss
            return Random.Range(10,16); // random 
        
        if (gpm.playerFloor == 10) // backstreets 
            return 12; // burn
        
        if (gpm.playerFloor == 11) // backstreets 
            return Random.Range(10,16); // random 
        
        
        return Random.Range(5,15); // random 
    }
    

    public void ShowMementoChoiceWindow()
    {
        if (itemList.skillsPool.Count > 1)
        {
            paused = true;
            mementoWindow = true;
            
            /*
            if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                Time.timeScale = 0;
            */
            
            Time.timeScale = 1;
            
            firstMementoToChoose = null;
            secondMementoToChoose = null;
            
            if (tutorialPassed == 1)
            {
                var skillsTemp = new List<SkillInfo>(itemList.skillsPool);

                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    // remove nonCoop mementos
                    for (int i = skillsTemp.Count - 1; i >= 0; i--)
                    {
                        if (skillsTemp[i].soloOnly) skillsTemp.RemoveAt(i);   
                    }
                }
                else
                {
                    // remove coop mementos
                    for (int i = skillsTemp.Count - 1; i >= 0; i--)
                    {
                        if (skillsTemp[i].coopOnly) skillsTemp.RemoveAt(i);
                    }
                }
                
                int r = Random.Range(0, skillsTemp.Count);
                firstMementoToChoose = skillsTemp[r];
                skillsTemp.RemoveAt(r);
                secondMementoToChoose = skillsTemp[Random.Range(0, skillsTemp.Count)];   
            }
            else
            {
                firstMementoToChoose = itemList.skillsPool[0];
                secondMementoToChoose = itemList.skillsPool[19];
            }   

            UiManager.instance.ShowMementoChoice(firstMementoToChoose, secondMementoToChoose);
        }
    }

    public void ChooseMemento(int side) // 0 - left, 1 - right
    {
        if (side == 0)
            PlayerSkillsController.instance.AddSkill(firstMementoToChoose);
        else if (side == 1)
            PlayerSkillsController.instance.AddSkill(secondMementoToChoose);

        /*
        if (!hub)
            DifficultyUp();
        */
        
        paused = false;
        mementoWindow = false;
        firstMementoToChoose = null;
        secondMementoToChoose = null;
    }

    public void DifficultyUp()
    {
        if (tutorialPassed == 1 && level.dynamicDifficulty)
        {
            SpawnController.instance.DifficultyUp();   
        }
    }

    public void MouseSense(float v)
    {
        mouseSensitivity = v;
    }

    public void MouseSpeed(float v)
    {
        mouseLookSpeed = 30;
        mouseLookSpeedCurrent = mouseLookSpeed;
    }
    
    public void Brightness(float v)
    {
        brightness = v;
    }
    
    public void Contrast(float v)
    {
        contrast = v;
    }
    
    public void DifficultyDown()
    {
        if (tutorialPassed == 1 && level.dynamicDifficulty)
        {
            // dont up if next diff level isnt dynamic
            if (GutProgressionManager.instance.currentLevelDifficulty > 0)
            {
                //currentLevelDifficulty--;
                if (sc)
                    sc.DifficultyDown();   
            }
        }
    }

    public void OpenSteamPage()
    {
        Application.OpenURL("https://store.steampowered.com/app/1245430/Golden_Light/");
    }
    
    public void OpenDiscordPage()
    {
        Application.OpenURL("https://discord.gg/hsPKzDT");
    }
    
    public void OpenWikiPage()
    {
        Application.OpenURL("https://golden-light.fandom.com/wiki/Golden_Light_Wiki");
    }

    public void AskForHelp()
    {
        Application.OpenURL("https://discord.gg/Wsr7CHj");
    }

    public void Reddit()
    {
        Application.OpenURL("https://www.reddit.com/r/GoldenLightGame/");
    }

    public void MenuSettings()
    {
        menuSettings.SetActive(!menuSettings.activeInHierarchy);
    }

    public void SaveGame()
    {
        savedOnFloor = !hub ? 1 : 0;

        // COMMENTED FOR TESTING
            SavingSystem.SaveGame(this, itemList);
    }

    public void SaveNewKill()
    {
        overallKills++;
        if (LeaderboardWrapper.instance)
            LeaderboardWrapper.instance.UploadScoreToLeaderboard("OverallKills", overallKills);
    }

    public void SaveNewDeath()
    {
        overallDeaths++;
        if (LeaderboardWrapper.instance)
            LeaderboardWrapper.instance.UploadScoreToLeaderboard("OverallDeaths", overallDeaths);
    }

    public void SaveNewLevelsOnRun(int amount)
    {
        if (amount > levelsOnRun)
        {
            levelsOnRun = amount;
            if (LeaderboardWrapper.instance)
                LeaderboardWrapper.instance.UploadScoreToLeaderboard("LevelsCompletedInOneLife", levelsOnRun);   
        }
    }

    public void SaveNewGoldOnRun(int amount)
    {
        if (amount > goldOnRun)
        {
            goldOnRun = amount;
            if (LeaderboardWrapper.instance)
                LeaderboardWrapper.instance.UploadScoreToLeaderboard("GoldFoundInOneLIfe", goldOnRun);   
        }
    }

    public void LoadGame()
    {
        SavingData data = SavingSystem.LoadGame();

        if (data != null)
        {
            cagesCompleted = data.cagesCompleted;
            revolverFound = data.revolverFound;
            ladyshoeFound = data.ladyshoeFound;
            itemList.heartContainerAmount = data.heartContainerAmount;
            itemList.gold = data.playersGold;
            mouseInvert = data.invertMouse;
            /*
            mouseSensitivity = data.mouseSense;
            mouseLookSpeed = data.camSpeed;
            */
            language = data.language;
            contrast = data.contrast;
            brightness = data.brightness;
            wasInHub = data.wasInHub;
            hubActiveCheckpointIndex = data.hubActiveCheckpointIndex;

            newGamePlus = data.newGamePlus;
            
            levelsOnRun = data.levelsOnRun;
            goldOnRun = data.goldOnRun;
            overallDeaths = data.overallDeaths;
            overallKills = data.overallKills;
            rareWindowShown = data.rareWindowShown;
            savedOnFloor = data.savedOnFloor;
            
            volumeSliderValue = data.volumeSliderValue;
            tutorialPassed = data.tutorialPassed;
            snoutFound = data.snoutFound;
            itemList.badReputaion = data.badReputaion;
            bloodMist = data.bloodMist;
            lastTalkedDyk = data.lastTalkedDyk;
            lowPitchDamage = data.lowPitchDamage;
            if (data.roseNpcsInteractedInHub != null)
                roseNpcsInteractedInHub = new List<int>(data.roseNpcsInteractedInHub);
            if (data.cutscenesInteractedInHub != null)
                cutscenesInteractedInHub = new List<int>(data.cutscenesInteractedInHub);
            if (data.heartControllersTakenIndexesInHub != null)
                permanentPickupsTakenIndexesInHub = new List<int>(data.heartControllersTakenIndexesInHub);
            if (data.doorsOpenedIndexesInHub != null)
                doorsOpenedIndexesInHub = new List<int>(data.doorsOpenedIndexesInHub);
            darkness = data.darkness;
            
            /*
            grain = data.grain;
            bloom = data.bloom;
            dithering = data.dithering;
            doubleVision = data.doubleVision;
            pixels = data.pixels;
            edgeDetection = data.edgeDetection;
            */
            
            itemList.goldSpentOnSon = data.goldSpentOnSon;
            if (data.goldenKeysFoundInHub != null)
                goldenKeysFoundInHub = new List<int>(data.goldenKeysFoundInHub);
            if (data.goldenKeysFoundOnFloors != null)
                goldenKeysFoundOnFloors = new List<int>(data.goldenKeysFoundOnFloors);
            if (data.tapesFoundOfFloors != null)
                tapesFoundOfFloors = new List<int>(data.tapesFoundOfFloors);
            itemList.goldenKeysAmount = data.goldenKeysAmount;
            
            if (data.unlockedTracks != null)
                itemList.unlockedTracks = new List<int>(data.unlockedTracks);
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    itemList.UnlockNewCassette(null);   
                }    
            }
            
            if (data.activeQuests != null)
                qm.activeQuestsIndexes = new List<int>(data.activeQuests);
            if (data.completedQuests != null)
                qm.completedQuestsIndexes = new List<int>(data.completedQuests);
            if (data.savedQuestItems != null)
                itemList.savedQuestItems = new List<int>(data.savedQuestItems);
            itemList.herPiecesApplied = data.appliedHerPieces;
            itemList.foundHerPiecesOnFloors = data.foundHerPiecesOnFloors;
            itemList.LoadInventory(data);
            tutorialHints = data.tutorialHints;
            hubVisits = data.hubVisits;
            resolution = data.resolution;

            if (data.difficultyLevel == 0)
                difficultyLevel = GameMode.StickyMeat;
            else if (data.difficultyLevel == 1)
                difficultyLevel = GameMode.Ikarus;
            else if (data.difficultyLevel == 2)
                difficultyLevel = GameMode.MeatZone;
            
            GutProgressionManager.instance.LoadData(data);
            if (ui)
                ui.UpdateGold(itemList.gold);
            
            ActiveNpcManager.instance.LoadCurrentActiveNpc(data);

            coopBiomeCheckpoints = data.coopBiomeCheckpoints;
            notesPickedUp = data.notesPickedUp;
            
            if (SteamworksLobby.instance)
                SteamworksLobby.instance.SetCheckpoints(coopBiomeCheckpoints);
        }
    }

    public void DeleteLocalSave(bool deathOnMeatzone)
    {
        // restart levels, remove gold
        if (!deathOnMeatzone)
        {
            bloodMist = 1;
            grain = 0;
            wasInHub = 0;
            hubVisits = 0;
            snoutFound = 0;
            lowPitchDamage = 0;
            //rareWindowShown = 0;
            tutorialPassed = 0;
            resolution = 2;
            lastTalkedDyk = -1;
            tutorialHints = 0;
            tapesFoundOfFloors.Clear();
            itemList.unlockedTracks.Clear();
        }
        
        itemList.heartContainerAmount = 0;
        savedOnFloor = 0;
        itemList.gold = 0;
        cagesCompleted = false;
        ladyshoeFound = false;
        revolverFound = false;
        
        darkness = 0;
        goldenKeysFoundOnFloors.Clear();
        goldenKeysFoundInHub.Clear();
        permanentPickupsTakenIndexesInHub.Clear();
        roseNpcsInteractedInHub.Clear();
        cutscenesInteractedInHub.Clear();
        doorsOpenedIndexesInHub.Clear();
        hubActiveCheckpointIndex = 0;
        if (ui)
            ui.UpdateGold(0);
        GutProgressionManager.instance.ClearSave();
        qm.activeQuestsIndexes.Clear();
        qm.completedQuestsIndexes.Clear();
        itemList.savedQuestItems.Clear();
        itemList.goldenKeysAmount = 0;
        itemList.goldSpentOnSon = 0;
        itemList.foundHerPiecesOnFloors.Clear();

        for (int i = 0; i < 3; i++)
        {
            itemList.UnlockNewCassette(null);   
        }
        
        
        itemList.herPiecesApplied.Clear();
        itemList.badReputaion = 1;
        SaveGame();
    }

    public void SetVolume(float newVolume)
    {
        //mixer.SetFloat("MasterVolume",  Mathf.Log10(newVolume) * 20);
        AudioListener.volume = newVolume;
    }
    
    void OnDestroy()
    {
        Destroy(procMapMaterialInstance);
        Destroy(renderTextureInstance);
    }

    public HealthController GetClosestUnit(Vector3 pos, bool includePlayer, HealthController whoAsking)
    {
        HealthController newHc = null;
        float distance = 1000;
        float newDist = 0;
        
        for (int i = units.Count - 1; i >= 0; i--)
        {
            if (units[i] == whoAsking || (!includePlayer && units[i].player) || units[i].health <= 0)
                continue;

            newDist = Vector3.Distance(pos, units[i].transform.position);
            if (newDist <= distance)
            {
                distance = newDist;
                newHc = units[i];
            }
        }

        return newHc;
    }

    public void ToggleBloodMist()
    {
        if (bloodMist == 0)
            bloodMist = 1;
        else
            bloodMist = 0;
    }

    public void SnoutFound()
    {
        if (snoutFound == 0)
        {
            snoutFound = 1;
            SaveGame();
        }
    }

    public void GiveRandomStatus()
    {
        int r = Random.Range(0, 8);

        if (r != 1)
        {
            player.statusEffects[r].effectLevelCurrent = player.statusEffects[r].effectLevelMax;
            player.statusEffects[r].effectActive = true;
        }
        else
        {
            player.StartFire();
        }
    }
}
