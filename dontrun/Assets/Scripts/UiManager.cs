using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using PlayerControls;
using SCPE;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    public TextMeshProUGUI epigraphText;

    public TextMeshProUGUI goldAmountText;
    public Animator goldAmountTextAnim;
    public TextMeshProUGUI ammoAmountText;
    public TextMeshProUGUI activeWeaponName;
    public Animator pauseAnim;

    [Header("Feedback")]
    public List<Image> healthbars = new List<Image>();
    public List<Image> healthbarsFones = new List<Image>();
    public Animator hintAnim;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI pickedItemInfo;
    public Animator itemInfoAnim;
    public GameObject ikarusHands;
    public ParticleSystem mementoChooseParticles;
    ParticleSystem.EmissionModule mementoChooseParticlesEmission;
    
    public Animator reputationAnim;
    public TextMeshProUGUI reputationText;
    public AudioSource gutWhisperAu;
    public AudioClip gutLove;
    public AudioClip gutHate;

    public TextMeshProUGUI currentWeightFeedback;
    
    private string itemInfoUpdateString = "Update";
    public Animator eyePatchAnim;
    public TextMeshProUGUI foundKeysText;
    public Animator keysFoundHint;
    public Animator levelHint;
    public TextMeshProUGUI levelText;
    public Animator savedHint;
    public TextMeshProUGUI savedText;
    public List<Image> keys;
    public GameObject gameUi;
    public GameObject weaponsBackground;
    
    [Header("icons")]
    public Image activeWeaponIcon;
    public Image activeWeaponBrokenIcon;
    public Image secondWeaponIcon;
    public Image activeToolIcon;
    public Image toolBackground;
    public TextMeshProUGUI activeToolInfo;
    public TextMeshProUGUI floorNumber;
    public List<Sprite> toolsSprites;
    public TextMeshProUGUI switchWeaponButton;
    public List<Sprite> weaponSprites;
    public Button restart;
    public Button restartInLevelButton;
    public SpriteRenderer welcomeToMeatZone;
    public Sprite emptyInventorySlotIcon;

    public GameObject deathWindow;
    public GameObject questWindow;
    public GameObject questIcon;
    public List<TextMeshProUGUI> questsNames;
    public List<TextMeshProUGUI> questsDescriptions;
    public List<Image> questsCompleteMarks;
    
    [Header("Death screen")]
    public TextMeshProUGUI restartInHubButtonText;
    public TextMeshProUGUI restartInLevelButtonText;
    public TextMeshProUGUI imDead;
    public TextMeshProUGUI whoKilledMe;
    public TextMeshProUGUI goldOnDeath;
    public TextMeshProUGUI keysOnDeath;

    public List<Color> statuscolors = new List<Color>();
    
    public Animator damageUiAnim;
    private string damageBool = "Damage";
    private Coroutine damageFeedbackCoroutine;

    [Header("0 - poison, 1 - fire, 2 - bleed, 3 - rust, 4 - regeneration, 5 - gold hunger, 6- cold, 7- love")]
    public List<StatusEffectsUi> statusEffectsUi;
    public List<BaffBarUi> buffBarUi;

    [Header("Settings")] public GameObject settingsCanvas;
    
    [Header("Hints")]
    public bool playerInteracted = false;
    public bool playerSprinted = false;
    public bool playerSwitchedWeapon = false;
    public bool playerReloaded = false;
    public bool playerHealed = false;
    public bool playerDashed = false;
    public bool playerSwitchedTool = false;
    public bool playerAteItem = false;
    public bool playerThrewItem = false;
    public bool playerAteWeapon = false;
    public bool playerHitYourself = false;

    [Header("Settings")]
    public Slider volume;
    public Slider mouseSens;
    public Slider mouseSpeed;
    public Slider brightness;
    public Slider contrast;
    //public PostProcessVolume postProcess;
    public Volume renderVolume;
    public VolumeProfile postProcessVolume;
    UnityEngine.Rendering.Universal.ColorAdjustments _colorAdjustments;
    UnityEngine.Rendering.Universal.FilmGrain _filmGrain;
    UnityEngine.Rendering.Universal.Bloom _bloom;
    private Sharpen _sharpen;
    Dithering _dithering;
    Pixelize _pixels;
    DoubleVision _doubleVision;
    EdgeDetection _edgeDetection;

    [Header("0 - inventory, 1 - memento, 2 - notes, 3 - quests, 4 - quest items")]
    public List<InventoryViewerController> mvc;

    [Header("Dialogue")]
    public Animator dialogueAnim;
    public NpcController dialogueInteractor;
    public TextMeshProUGUI dialogueSpeakerName;
    public TextMeshProUGUI dialoguePhrase;
    public TextMeshProUGUI dialogueChoice;
    private bool secondMementoDisabled = false;
    
    GameManager gm;
    ElevatorController ec;
    private QuestManager qm;
    private PlayerAudioController pac;

    PlayerMovement pm;
    ItemsList il;
    [SerializeField]
    Animator ammoAnim;

    private Coroutine getSkillCoroutine;
    
    private Color tempColor;

    [Header("Boss HP")] 
    public GameObject bossHpParent;
    public Image bossHealthbar;
    public TextMeshProUGUI bossNameText;
    
    [Header ("Memento Choice Window")]
    public Animator mementoChoiceWindow;
    public Image mementoFirstImage;
    public TextMeshProUGUI mementoFirstText;
    public Image mementoSecondImage;
    public TextMeshProUGUI mementoSecondText;
    
    [Header("Button locals")]
    public List<ButtonLocals> locals;

    [Header("ui navigation")] 
    public GameObject firstSelectedInPause;
    public GameObject firstSelectedInSettings;
    public GameObject firstSelectedInJournal;
    public GameObject firstSelectedInInventory;
    public GameObject firstSelectedInMementos;
    public GameObject firstSelectedInQuestItems;
    private string volumeString = "MusicVolume";

    public List<GameObject> uiElementsToHideInFlashback;
    public List<GameObject> settingWindowsToScale;

    private bool canSendDamageMessage = true;

    private bool chaseFeedbackShowed = false;
    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ec = ElevatorController.instance;
        pac = PlayerAudioController.instance;
        gm = GameManager.instance;

        mementoChooseParticlesEmission = mementoChooseParticles.emission;

        mouseSens.value = gm.mouseSensitivity;
        mouseSpeed.value = gm.mouseLookSpeed;
        
        if (!gm.hub)
        {
            renderVolume.profile = gm.level.renderProfile;
        }
        welcomeToMeatZone.gameObject.SetActive(false);
        postProcessVolume = renderVolume.profile;
        postProcessVolume.TryGet(out _colorAdjustments);
        postProcessVolume.TryGet(out _filmGrain);
        postProcessVolume.TryGet(out _pixels);
        postProcessVolume.TryGet(out _doubleVision);
        postProcessVolume.TryGet(out _edgeDetection);
        postProcessVolume.TryGet(out _dithering);
        postProcessVolume.TryGet(out _sharpen);
        postProcessVolume.TryGet(out _bloom);

        for (var index = 0; index < statusEffectsUi.Count; index++)
        {
            var s = statusEffectsUi[index];
            s.background.enabled = false;
            s.name.enabled = false;
            //s.backgroundOutline.enabled = false;

            Color newColor = s.fill.color;
            newColor.a = 0;
            s.fill.color = newColor;
            s.fill.fillAmount = 0;
        }
        
        Settings();
        CloseSettings();
        SetButtonLocals();

        volume.value = gm.volumeSliderValue;

        /*
        SetBloom(gm.bloom);
        SetDithering(gm.dithering);
        SetPixels(gm.pixels);
        SetDoubleVision(gm.doubleVision);
        SetEdgeDetection(gm.edgeDetection);
        */
        
        UpdateBuff(0, 0, false);
        UpdateBuff(1, 0, false);
        UpdateBuff(2, 0, false);
        
        ToggleGameUi(false, false);
    }

    public void SetGrain(int active) // 0 - active, 1 - inactive
    {
        /*
        if (active == 0)
            _filmGrain.active = true;
        else
            _filmGrain.active = false;
            */
    }

    public void SetDithering(int active) // 0 - active, 1 - inactive
    {
        _dithering.active = true;
        return;

        if (active == 0)
            _dithering.active = true;
        else
            _dithering.active = false;
    }

    public void SetSharpen(bool active)
    {
        /*
        ClampedFloatParameter newParam = new ClampedFloatParameter(amount, 0.5f, 2f, true);
        _sharpen.radius = newParam;
        */
        
        //_sharpen.active = active;
    }
    
    public void SetPixels(int active) // 0 - active, 1 - inactive
    {
        _pixels.active = true;
        return;
        
        if (active == 0)
            _pixels.active = true;
        else
            _pixels.active = false;
    }
    public void SetDoubleVision(int active) // 0 - active, 1 - inactive
    {
        _doubleVision.active = true;
        return;
        
        if (active == 0)
            _doubleVision.active = true;
        else
            _doubleVision.active = false;
    }
    public void SetEdgeDetection(int active) // 0 - active, 1 - inactive
    {
        _edgeDetection.active = false;
        return;
        
        if (active == 0)
            _edgeDetection.active = true;
        else
            _edgeDetection.active = false;
    }

    public void SetBloom(int active) // 0 - active, 1 - inactive
    {
        _bloom.active = true;
        return;
        
        if (active == 0)
            _bloom.active = true;
        else
            _bloom.active = false;
    }


    public void UpdateHealthbar()
    {
        var hc = PlayerMovement.instance.hc;
        int hearts = ItemsList.instance.heartContainerAmount;

        if (hearts > 0)
        {
            float healthToProceed = hc.health;
            healthbars[0].fillAmount = hc.health / 1000;
            healthToProceed -= 1000;
            
            for (int i = 1; i <= hearts; i++)
            {
                healthbarsFones[i].gameObject.SetActive(true);
                healthbars[i].gameObject.SetActive(true);

                if (healthToProceed > 0)
                {
                    healthbars[i].fillAmount = healthToProceed / 100;
                    healthToProceed -= 100;   
                }
                else
                    healthbars[i].fillAmount = 0;
            }
        }
        else
        {
            for (int i = 0; i < healthbars.Count; i++)
            {
                if (i == 0)
                {
                    healthbars[i].fillAmount = pm.hc.health / pm.hc.healthMax;
                }
                else
                {
                    healthbars[i].gameObject.SetActive(false);
                    healthbarsFones[i].gameObject.SetActive(false);
                }
            }
        }
    }
    
    
    public void SetButtonLocals()
    {
        for (int i = 0; i < locals.Count; i++)
        {
            if (locals[i].locals.Count > gm.language)
            {
                if (locals[i].textToChange)
                    locals[i].textToChange.text = locals[i].locals[gm.language];
                else if (locals[i].textToChangeLegacy)
                    locals[i].textToChangeLegacy.text = locals[i].locals[gm.language];   
            }
        }
    }


    public void Init(bool flashback)
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        il = ItemsList.instance;
        qm = QuestManager.instance;
        
        // lg is generating in flashbacks
        if (flashback)
        {
            //hide ui
            ToggleGameUi(false, false);
        }
        else
        {
            ToggleGameUi(true, false);
            UpdateTools();
            UpdateAmmo();
            UpdateGold(il.gold);
            //UpdateLockpicks();
            pm.hc.UpdateHealthbarOnLoad();
            StartCoroutine(CheckHints());
            UpdateLevel();
        
            //saved settings
            LoadSavedSettings();
            ////////////////

            UpdateFloor();
        
            UpdateWeapons();
        
            UpdateWeaponSwitchButtonText();

        }
        if (il.lostAnEye == 1)
            LostAnEye();
        
        if (!gm.hub)
            UpdateKeys(il.keys, -1);

        StartCoroutine(InitKeysSettings());
    }

    void UpdateFloor()
    {
        if (gm.hub)
            floorNumber.text = "";
        else
        {
            int curLevel = GutProgressionManager.instance.playerFloor;
            if (gm.tutorialPassed == 1)
            {
                if (gm.language == 0)
                    floorNumber.text = "Floor: -" + curLevel;
                else if (gm.language == 1)
                    floorNumber.text = "Этаж: -" + curLevel;
                else if (gm.language == 2)
                    floorNumber.text = "Piso: -" + curLevel;
                else if (gm.language == 3)
                    floorNumber.text = "Etage: -" + curLevel;
                else if (gm.language == 4)
                    floorNumber.text = "Pavimento: -" + curLevel;
                else if (gm.language == 5)
                    floorNumber.text = "Chão: -" + curLevel;
            }
            else if (gm.tutorialPassed == 0)
            {
                if (gm.language == 0)
                    floorNumber.text = "Floor: 0";
                else if (gm.language == 1)
                    floorNumber.text = "Этаж: 0";
                else if (gm.language == 2)
                    floorNumber.text = "Piso: 0";
                else if (gm.language == 3)
                    floorNumber.text = "Etage: 0";
                else if (gm.language == 4)
                    floorNumber.text = "Pavimento: 0";
                else if (gm.language == 5)
                    floorNumber.text = "Chão: 0";
            }   
        }
    }

    public void ToggleGameUi(bool active, bool ignoreCrosshair)
    {
        print("toggle UI. Active is " + active);
        for (var index = 0; index < uiElementsToHideInFlashback.Count; index++)
        {
            if (ignoreCrosshair && (index == 0 || index == 1))
                continue;
            
            var ui = uiElementsToHideInFlashback[index];
            ui.SetActive(active);
        }
    }
    
    IEnumerator InitKeysSettings()
    {
        foreach (var s in settingWindowsToScale)
        {
            s.transform.localScale = Vector3.zero;   
        }
        Settings();
        yield return null;
        CloseSettings();
        foreach (var s in settingWindowsToScale)
        {
            s.transform.localScale = Vector3.one;   
        }
        UpdateWeaponSwitchButtonText();
        
        SetBloom(1);
        SetDithering(1);
        SetPixels(1);
        SetDoubleVision(1);
        //SetEdgeDetection(0);

    }

    public void UpdateWeaponSwitchButtonText()
    {
        switchWeaponButton.text = KeyBindingManager.GetKeyCode(KeyAction.SwitchWeapon).ToString();
    }

    public void DamageFeedback()
    {
        /*
        if (damageFeedbackCoroutine != null)
            StopCoroutine(damageFeedbackCoroutine);
            */
        
        if (damageFeedbackCoroutine == null)
            damageFeedbackCoroutine = StartCoroutine(DamageFeedbackOverTime());
    }

    IEnumerator DamageFeedbackOverTime()
    {
        if (damageUiAnim.GetBool(damageBool) == false)
            damageUiAnim.SetBool(damageBool, true);
        
        yield return new WaitForSeconds(0.75f);
        damageUiAnim.SetBool(damageBool, false);
        
        damageFeedbackCoroutine = null;
    }
    
    public void UpdateStatusEffects(int index)
    {
        if (pm == null)
            pm = PlayerMovement.instance;

        if (pm.hc.statusEffects[index].effectLevelCurrent > 0)
        {
            statusEffectsUi[index].fill.fillAmount = pm.hc.statusEffects[index].effectLevelCurrent / pm.hc.statusEffects[index].effectLevelMax;
            statusEffectsUi[index].background.enabled = true;
            statusEffectsUi[index].name.enabled = true;
            
            tempColor = statusEffectsUi[index].fill.color;
            
            if (pm.hc.statusEffects[index].effectActive)
            {
                tempColor.a = 1;
                //statusEffectsUi[index].backgroundOutline.enabled = true;
            }
            else
            {
                tempColor.a = 0.2f;
                //statusEffectsUi[index].backgroundOutline.enabled = false;
            }
            
            statusEffectsUi[index].fill.color = tempColor;
        }
        else
        {
            if (statusEffectsUi[index].background == null) return;
            
            statusEffectsUi[index].fill.fillAmount = 0;
            statusEffectsUi[index].background.enabled = false;
            statusEffectsUi[index].name.enabled = false;
        }
    }
    
    void UpdateLevel()
    {
        if (!gm.hub)
        {
            var gpm = GutProgressionManager.instance;
            if (gm.arena)
            {
                if (gm.language == 0)
                    levelText.text = "Survive as long as you can";
                else if (gm.language == 1)
                    levelText.text = "Проживи сколько сможешь";
                else if (gm.language == 2)
                    levelText.text = "Sobrevive tanto como puedas";
                else if (gm.language == 3)
                    levelText.text = "Überlebe so lange du kannst";
                else if (gm.language == 4)
                    levelText.text = "Sopravvivi il più a lungo possibile";
                else if (gm.language == 5)
                    levelText.text = "sobreviva o máximo que puder";
            }
            else if (gm.tutorialPassed == 1)
            {
                if (gm.language == 0)
                    levelText.text = Translator.TranslateText("Floor:")  + (gpm.playerFloor) * -1;
                else if (gm.language == 1)
                    levelText.text = Translator.TranslateText("Этаж:")  + (gpm.playerFloor) * -1;
                else if (gm.language == 2)
                    levelText.text = Translator.TranslateText("Piso:") + (gpm.playerFloor) * -1;
                else if (gm.language == 3)
                    levelText.text = Translator.TranslateText("Etage:") + (gpm.playerFloor) * -1;
                else if (gm.language == 4)
                    levelText.text = Translator.TranslateText("Pavimento:") + (gpm.playerFloor) * -1;
                else if (gm.language == 5)
                    levelText.text = Translator.TranslateText("Chão:") + (gpm.playerFloor) * -1;
            }
            else if (gm.tutorialPassed == 0)
            {
                if (gm.language == 0)
                    levelText.text = Translator.TranslateText("Floor: 0");
                else if (gm.language == 1)
                    levelText.text = Translator.TranslateText("Этаж: 0");
                else if (gm.language == 2)
                    levelText.text = Translator.TranslateText("Piso: 0");
                else if (gm.language == 3)
                    levelText.text = Translator.TranslateText("Etage: 0");
                else if (gm.language == 4)
                    levelText.text = Translator.TranslateText("Pavimento: 0");
                else if (gm.language == 5)
                    levelText.text = Translator.TranslateText("Chão: 0");
            }
            levelHint.SetBool("Active", true);   
        }
    }

    public void UpdateReputation(float addRep)
    {
        if (gm.language == 0)
            reputationText.text = "YOU HEAR A NEW WHISPER";
        else if (gm.language == 1)
            reputationText.text = "ТЫ СЛЫШИШЬ НОВЫЙ ШЕПОТ";
        else if (gm.language == 2)
            reputationText.text = "ESCUCHAS UN NUEVO SUSURRO";
        else if (gm.language == 3)
            reputationText.text = "Sie hören ein neues Flüstern";
        else if (gm.language == 4)
            reputationText.text = "SENTI UN NUOVO SUSSURO";
        else if (gm.language == 5)
            reputationText.text = "VOCÊ OUVE UM NOVO SUSURRO";
        
        reputationAnim.SetTrigger(itemInfoUpdateString);

        if (addRep > 0)
            gutWhisperAu.clip = gutHate;
        else
            gutWhisperAu.clip = gutLove;

        gutWhisperAu.pitch = Random.Range(0.75f, 1.25f);
        gutWhisperAu.Play();
    }
    
    IEnumerator CheckHints()
    {
        while (true)
        {
            if (gm.hub)
            {
                yield return new WaitForSeconds(5);
                
                if (gm.tutorialHints == 1) continue;
                
                if (pm.inTransport)
                {
                    if (gm.language == 0)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " - get off the bike";
                    else if (gm.language == 1)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " - спрыгнуть с велика";
                    else if (gm.language == 2)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " - bajarse de la bicicleta";
                    else if (gm.language == 3)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " - Steig vom Fahrrad";
                    else if (gm.language == 4)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " - scendere dalla bici";
                    else if (gm.language == 5)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " - desça da bicicleta";
   
                    hintAnim.SetTrigger("Active");   
                }
                
                continue;
            }
            
            yield return new WaitForSeconds(30f);
            
            if (gm.tutorialHints == 1) continue;
            
            if (playerInteracted == false)
            {
                if (gm.language == 0)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Interaction) + " - interact";
                else if (gm.language == 1)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Interaction) + " - взаимодействовать";
                else if (gm.language == 2)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Interaction) + " - Interactuar";
                else if (gm.language == 3)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Interaction) + " - Interagieren";
                else if (gm.language == 4)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Interaction) + " - Interagisci";
                else if (gm.language == 5)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Interaction) + " - Interagir";

                hintAnim.SetTrigger("Active");
            }
            else if (playerDashed == false)
            {
                if (gm.language == 0)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " - jump / dash";
                else if (gm.language == 1)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " - прыжок / уворот";
                else if (gm.language == 2)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " - esquivar / saltar";
                else if (gm.language == 3)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " -  ausweichen / springen";
                else if (gm.language == 4)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " -  schivare/ saltare";
                else if (gm.language == 5)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Dash) + " -  Esquivar/Pular";

                hintAnim.SetTrigger("Active");
            }
            else if (playerSprinted == false)
            {
                if (gm.language == 0)
                    hintText.text = "Shift - run";
                else if (gm.language == 1)
                    hintText.text = "Shift - бег";
                else if (gm.language == 2)
                    hintText.text = "Shift - correr";
                else if (gm.language == 3)
                    hintText.text = "Shift- Rennen";
                else if (gm.language == 4)
                    hintText.text = "Shift - corri";
                else if (gm.language == 5)
                    hintText.text = " 'Shift' - Correr";

                hintAnim.SetTrigger("Active");
            }
            else if (!playerSwitchedWeapon && pm.hc.wc.activeWeapon && pm.hc.wc.secondWeapon)
            {
                if (gm.language == 0)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SwitchWeapon) + " - change weapon";
                else if (gm.language == 1)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SwitchWeapon) + " - сменить оружие";
                else if (gm.language == 2)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SwitchWeapon) + " - Cambiar arma";
                else if (gm.language == 3)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SwitchWeapon) + " - Wechsel Waffe";
                else if (gm.language == 4)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SwitchWeapon) + " - cambia arma";
                else if (gm.language == 5)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SwitchWeapon) + " - mudar de arma";

                hintAnim.SetTrigger("Active");
            }
            else if (!playerReloaded && pm.hc.wc.activeWeapon && pm.hc.wc.activeWeapon.weaponType == WeaponController.Type.Range &&  pm.hc.wc.activeWeapon.ammoClip < pm.hc.wc.activeWeapon.ammoClipMax)
            {
                if (gm.language == 0)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Reload) + " - reload weapon";
                else if (gm.language == 1)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Reload) + " - перезарядка";
                else if (gm.language == 2)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Reload) + " - Recargar arma";
                else if (gm.language == 3)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Reload) + " - Lade Waffe nach";
                else if (gm.language == 4)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Reload) + " - ricarica arma";
                else if (gm.language == 5)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.Reload) + " - Recarregar arma";

                hintAnim.SetTrigger("Active");
            }
            else if (!playerAteWeapon && pm.hc.health < pm.hc.healthMax * 0.5f && (pm.hc.wc.activeWeapon || pm.hc.wc.secondWeapon))
            {
                if (gm.language == 0)
                    hintText.text = "Hold " + KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " to gobble up a weapon";
                else if (gm.language == 1)
                    hintText.text = "Зажми " + KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " чтобы сожрать оружие";
                else if (gm.language == 2)
                    hintText.text = "Mantén presionado " + KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " para engullir un arma";
                else if (gm.language == 3)
                    hintText.text = "Halten Sie " + KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " gedrückt, um eine Waffe zu verschlingen";
                else if (gm.language == 4)
                    hintText.text = "Tenera " + KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " divorare un'arma";
                else if (gm.language == 5)
                    hintText.text = "Segure " + KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " para comer arma";

                hintAnim.SetTrigger("Active");
            }
            else if (il.GetToolsCount() > 0)
            {
                if (!playerThrewItem)
                {
                    if (gm.language == 0)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.ThrowTool) + " - throw item in the left hand";
                    else if (gm.language == 1)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.ThrowTool) + " - швырнуть предмет в левой руке";
                    else if (gm.language == 2)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.ThrowTool) + " - Tirar ítem en tu mano izquierda";
                    else if (gm.language == 3)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.ThrowTool) + " - Werfe Gegenstand in linker Hand";
                    else if (gm.language == 4)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.ThrowTool) + " - butta l'oggetto nella mano sinistra";
                    else if (gm.language == 5)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.ThrowTool) + " - Jogue o item na sua mão esquerda";

                    hintAnim.SetTrigger("Active");
                }
                else if (!playerAteItem)
                {
                    if (gm.language == 0)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " - eat item in the left hand";
                    else if (gm.language == 1)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " - сожрать предмет в левой руке";
                    else if (gm.language == 2)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " - Comer ítem en tu mano izquierda";
                    else if (gm.language == 3)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " - Gegenstannd in linker Hand essen";
                    else if (gm.language == 4)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " - mangia l'oggetto nella mano sinistra";
                    else if (gm.language == 5)
                        hintText.text = KeyBindingManager.GetKeyCode(KeyAction.UseTool) + " - Comer item na sua mão esquerda";

                    hintAnim.SetTrigger("Active");
                }
                else if (!playerSwitchedTool)
                {
                    if (gm.language == 0)
                        hintText.text = "Mouse Wheel - change item";
                    else if (gm.language == 1)
                        hintText.text = "Колесо мыши - сменить предмет";
                    else if (gm.language == 2)
                        hintText.text = "Rueda del ratón Arriba/Abajo - Cambiar ítem ";
                    else if (gm.language == 3)
                        hintText.text = "Mausrad - Ändere Gegenstand";
                    else if (gm.language == 4)
                        hintText.text = "Rotella mouse - cambia oggetto";
                    else if (gm.language == 5)
                        hintText.text = " 'Roda do Mouse' - Alterar item";

                    hintAnim.SetTrigger("Active");
                }
            }
            else if (!playerHitYourself && pm.hc.wc.activeWeapon)
            {
                if (gm.language == 0)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SelfAttack) + " - attack yourself";
                else if (gm.language == 1)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SelfAttack) + " - атаковать себя";
                else if (gm.language == 2)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SelfAttack) + " - Atacarte a ti mismo";
                else if (gm.language == 3)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SelfAttack) + " - Selbst angreifen";
                else if (gm.language == 4)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SelfAttack) + " - attacca te stesso";
                else if (gm.language == 5)
                    hintText.text = KeyBindingManager.GetKeyCode(KeyAction.SelfAttack) + " - Ataque-se";

                hintAnim.SetTrigger("Active");
            }
        }
    }

    public void GotItem(Interactable item)
    {
        //if (item.pickUp || item.ammoPickUp|| item.weaponPickUp)
        {
            if (item.pickedByPlayerMessage.Count > 0)
                pickedItemInfo.text = item.pickedByPlayerMessage[gm.language];
            
            if (item.pickUp && item.pickUp.resourceType == ItemsList.ResourceType.Tool)
            {
                UpdateWeightString();
                pickedItemInfo.text += ". " + currentWeightFeedback.text;
            }
            itemInfoAnim.SetTrigger(itemInfoUpdateString);
        }
    }

    public void ChangeToolHint()
    {
        playerSwitchedTool = true;
    }
    public void UseTool(int index)
    {
        pickedItemInfo.text = Translator.TranslateText(il.savedTools[pm.hc.wc.currentToolIndex].useMessage[gm.language]);
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
        playerAteItem = true;
    }

    public void EatWeapon()
    {
        string ateString = "";
        string effectString = "";

        if (gm.language == 0)
        {
            ateString = "I ate ";
            if (pm.hc.wc.activeWeapon.dataRandomizer.statusEffect >= 0)
            {
                effectString = " who had " + pm.hc.wc.activeWeapon.dataRandomizer.effectInName[gm.language] + " effect";
            }
        }
        else if (gm.language == 1)
        {
            ateString = "Я сожрал оружие ";
            if (pm.hc.wc.activeWeapon.dataRandomizer.statusEffect >= 0)
            {
                effectString = " c эффектом " + pm.hc.wc.activeWeapon.dataRandomizer.effectInName[gm.language];
            }
        }
        else if (gm.language == 2)
        {
            ateString = "Me he comido una ";
            if (pm.hc.wc.activeWeapon.dataRandomizer.statusEffect >= 0)
            {
                effectString = " quien tuvo " + pm.hc.wc.activeWeapon.dataRandomizer.effectInName[gm.language] + ".";
            }
        }
        else if (gm.language == 3)
        {
            ateString = "Ich aß einen ";
            if (pm.hc.wc.activeWeapon.dataRandomizer.statusEffect >= 0)
            {
                effectString = " Wer hatte " + pm.hc.wc.activeWeapon.dataRandomizer.effectInName[gm.language] + " bewirken";
            }
        }
        else if (gm.language == 4)
        {
            ateString = "Ho mangiato una ";
            if (pm.hc.wc.activeWeapon.dataRandomizer.statusEffect >= 0)
            {
                effectString = " chi ha avuto " + pm.hc.wc.activeWeapon.dataRandomizer.effectInName[gm.language] + ".";
            }
        }
        else if (gm.language == 5)
        {
            ateString = "Eu comi uma ";
            if (pm.hc.wc.activeWeapon.dataRandomizer.statusEffect >= 0)
            {
                effectString = " quem tinha " + pm.hc.wc.activeWeapon.dataRandomizer.effectInName[gm.language] + ".";
            }
        }

        pickedItemInfo.text = Translator.TranslateText(ateString + pm.hc.wc.activeWeapon.dataRandomizer.generatedName[gm.language] + effectString);
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
        playerAteWeapon = true;
    }

    public void HitYourself()
    {
        playerHitYourself = true;
    }

    public void ThrowTool(int index)
    {
        pickedItemInfo.text = Translator.TranslateText(il.savedTools[pm.hc.wc.currentToolIndex].throwMessage[gm.language]);
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
        playerThrewItem = true;
    }
    
    public void UpdateAmmo()
    {
        ammoAmountText.text = pm.hc.wc.activeWeapon && pm.hc.wc.activeWeapon.weaponType != WeaponController.Type.Melee
            ? $"{pm.hc.wc.activeWeapon.ammoClip}|{ItemsList.instance.ammoDataStorage.GetAmmoCount(pm.hc.wc.activeWeapon.weapon)}"
            : string.Empty;
    }

    public void WeaponBroke(WeaponController w)
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I broke this stuff");
        else if (gm.language == 1)
            pickedItemInfo.text = Translator.TranslateText("Я сломал это барахло");
        else if (gm.language == 2)
            pickedItemInfo.text = Translator.TranslateText("He roto esta cosa");
        else if (gm.language == 3)
            pickedItemInfo.text = Translator.TranslateText("Ich hab das hier kaputt gemacht");
        else if (gm.language == 4)
            pickedItemInfo.text = Translator.TranslateText("Ho rotto questa cosa");
        else if (gm.language == 5)
            pickedItemInfo.text = Translator.TranslateText("Eu quebrei essas coisas");

        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }
    
    public void SomeoneHearPlayer(int playerId)
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            if (GLNetworkWrapper.instance.playerNetworkObjects[playerId] != GLNetworkWrapper.instance.localPlayer)
                GLNetworkWrapper.instance.playerNetworkObjects[playerId].PlayerHeard();
            else
                PlayerHeardOnClient();
        }
        else
        {
            PlayerHeardOnClient();
        }
    }

    public void PlayerHeardOnClient()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("Someone heard me...");
        else if (gm.language == 1)
            pickedItemInfo.text = Translator.TranslateText("Кто-то услышал меня...");
        else if (gm.language == 2)
            pickedItemInfo.text = Translator.TranslateText("Alguien me ha oido...");
        else if (gm.language == 3)
            pickedItemInfo.text = Translator.TranslateText("Jemand hat mich gehört...");
        else if (gm.language == 4)
            pickedItemInfo.text = Translator.TranslateText("Qualcuno mi ha sentito...");
        else if (gm.language == 5)
            pickedItemInfo.text = Translator.TranslateText("Jemand hat mich gehört...");

        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }

    public void NoAmmo()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("No more ammo");
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Нет патронов");
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("No tengo mas munición");
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("Keine Muntion mehr");
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Niente munizioni");
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Sem mais munição");
        }
        ammoAnim.SetTrigger("Blink");
        
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }

    public void UpdateTools()
    {
        if (il == null)
            il = ItemsList.instance;
        
        if (pm == null)
            pm = PlayerMovement.instance;
        
        if (gm == null)
            gm = GameManager.instance;

        if (il.savedTools.Count > pm.hc.wc.currentToolIndex && il.savedTools[pm.hc.wc.currentToolIndex].amount > 0)
        {
            activeToolIcon.enabled = true;
            toolBackground.enabled = true;
            activeToolIcon.sprite = toolsSprites[pm.hc.wc.currentToolIndex];
            activeToolInfo.enabled = true;
            if (il.savedTools[pm.hc.wc.currentToolIndex].known)
                activeToolInfo.text = il.savedTools[pm.hc.wc.currentToolIndex].info[gm.language] + " X" + il.savedTools[pm.hc.wc.currentToolIndex].amount;
            else
                activeToolInfo.text = il.savedTools[pm.hc.wc.currentToolIndex].unknownInfo[gm.language] + " X" + il.savedTools[pm.hc.wc.currentToolIndex].amount;
        }
        else
        {
            activeToolIcon.enabled = false;
            toolBackground.enabled = false;
            activeToolInfo.enabled = false;
        }
    }

    public void KeyUsed()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I opened a lock");
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Я открыл замок");
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("Abrí una cerradura");
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("Ich habe ein Schloss geöffnet");
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Ho aperto un lucchetto");
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Eu abri uma fechadura");
        }

        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }
    
    public void UpdateKeys(int keysAmount, int playerId)
    {
        bool updateFeedback = true;
        if (playerId >= 0 && GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            if (GLNetworkWrapper.instance.playerNetworkObjects[playerId] != GLNetworkWrapper.instance.localPlayer)
            {
                updateFeedback = false;
            }
        }

        print("UpdateKeys. updateFeedback is " + updateFeedback);
        
        ec = ElevatorController.instance;
        if (ec && gm.keysFound == ec.padlocksInGame.Count && ec.padlocksInGame.Count > 0 && !chaseFeedbackShowed)
        {
            TimeToLeaveFloor();
            chaseFeedbackShowed = true;
        }

        if (!updateFeedback) return;
        
        for (int i = 0; i < keys.Count; i++)
        {
            if (i < keysAmount)
                keys[i].gameObject.SetActive(true);
            else
                keys[i].gameObject.SetActive(false);
        }
    }

    public void TimeToLeaveFloorFeedbackOnClient()
    {
        if (GutProgressionManager.instance.GetChaseScene())
        {
            if (gm.language == 0)
                foundKeysText.text = "Time to leave this floor!";
            else if (gm.language == 1)
                foundKeysText.text = "Пора уходить с этого этажа!";
            else if (gm.language == 2)
                foundKeysText.text = "¡Es hora de abandonar este piso!";
            else if (gm.language == 3)
                foundKeysText.text = "Es ist Zeit, diese Etage zu verlassen!";   
            else if (gm.language == 4)
                foundKeysText.text = "È ora di lasciare questo piano!";  
            else if (gm.language == 5)
                foundKeysText.text = "Hora de sair deste andar!";  
            
            //chase
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive &&
                GLNetworkWrapper.instance.localPlayer.isServer == false)
            {
                // on client
                SpawnController.instance.ClientStartChase();
            }
        }
        else
        {
            if (gm.language == 0)
                foundKeysText.text = "Now I can open the elevator";
            else if (gm.language == 1)
                foundKeysText.text = "Теперь я могу открыть лифт";
            else if (gm.language == 2)
                foundKeysText.text = "Ahora puedo abrir el ascensor";
            else if (gm.language == 3)
                foundKeysText.text = "Jetzt kann ich den Aufzug öffnen";
            else if (gm.language == 4)
                foundKeysText.text = "Ora posso sbloccare quell'ascensore";
            else if (gm.language == 5)
                foundKeysText.text = "Agora eu posso destrancar aquele elevador";
        }

        itemInfoAnim.SetTrigger(itemInfoUpdateString);
        keysFoundHint.SetBool("Active", true);

        int level = GutProgressionManager.instance.playerFloor;
        gm = GameManager.instance;
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            switch (level)
            {
                case 3:
                    // carre
                    if (gm.coopBiomeCheckpoints < 1)
                    {
                        gm.coopBiomeCheckpoints = 1;
                        gm.SaveGame();
                    }
                    break;
                
                case 6:
                    // wallmeat
                    if (gm.coopBiomeCheckpoints < 2)
                    {
                        gm.coopBiomeCheckpoints = 2;
                        gm.SaveGame();
                    }
                    break;
                
                case 9:
                    //needles
                    if (gm.coopBiomeCheckpoints < 3)
                    {
                        gm.coopBiomeCheckpoints = 3;
                        gm.SaveGame();
                    }
                    break;
                
                case 12:
                    //gut divers
                    if (gm.coopBiomeCheckpoints < 4)
                    {
                        gm.coopBiomeCheckpoints = 4;
                        gm.SaveGame();
                    }
                    break;
                
                case 15:
                    //meneater
                    if (gm.coopBiomeCheckpoints < 5)
                    {
                        gm.coopBiomeCheckpoints = 5;
                        gm.SaveGame();
                    }
                    break;
                case 18:
                    //castle
                    break;
            }
        }

    }
    
    public void TimeToLeaveFloor()
    {
        if (GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.TimeToLeaveFloorFeedbackOnClient();
        }
        else
        {
            TimeToLeaveFloorFeedbackOnClient();
        }
        
        // called only on host
        SpawnController.instance.PlayerFoundAllKeys();
    }

    public void MoveToTitles()
    {
        StartCoroutine(MoveToTitlesOverTime());
    }

    IEnumerator MoveToTitlesOverTime()
    {
        yield return new WaitForSeconds(5);
        PlayerMovement.instance.goldenLightAnimator.SetBool("GoldenLight", true);
        print("here");
        yield return new WaitForSeconds(5);
        print("here");
        GameManager.instance.ReturnToMenu(true);
    }

    public void UpdateGold(float _gold)
    {
        int g = Mathf.RoundToInt(_gold);
        goldAmountText.text = g.ToString();
        if (goldAmountTextAnim)
            goldAmountTextAnim.SetTrigger("Update");
    }
    /*
    public void UpdateLockpicks()
    {
        lockpicksAmountText.text = il.lockpicks.ToString();
        lockpicksAmountTextAnim.SetTrigger("Update");
    }
    */

    public void MeatTrapFeedback(string message)
    {
        pickedItemInfo.text = message;
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }
    
    public void DamagedMessage(string damageMessage)
    {
        if (canSendDamageMessage)
        {
            pickedItemInfo.text = damageMessage;
            itemInfoAnim.SetTrigger(itemInfoUpdateString);
            canSendDamageMessage = false;
            Invoke("DamageMessageCooldown", 1);
        }
    }


    
    void DamageMessageCooldown()
    {
        canSendDamageMessage = true;
    }

    public void AskForHelp()
    {
        gm.AskForHelp();
    }
    
    public void  StatusEffect(int index)
    {
        if (index == 0)
        {
            if (gm.language == 0)
                pickedItemInfo.text = Translator.TranslateText("I feel bad...");
            else if (gm.language == 1)
            {
                pickedItemInfo.text = Translator.TranslateText("Мне нехорошо...");
            }
            else if (gm.language == 2)
            {
                pickedItemInfo.text = Translator.TranslateText("Me siento mal...");
            }
            else if (gm.language == 3)
            {
                pickedItemInfo.text = Translator.TranslateText("Ich fühle mich nicht gut");
            }
            else if (gm.language == 4)
            {
                pickedItemInfo.text = Translator.TranslateText("Mi sento male...");
            }
            else if (gm.language == 5)
            {
                pickedItemInfo.text = Translator.TranslateText("Eu me sinto mal...");
            }
            itemInfoAnim.SetTrigger(itemInfoUpdateString);
        }
        else if (index == 3)
        {
            if (gm.language == 0)
                pickedItemInfo.text = Translator.TranslateText("My stuff can break easily");
            else if (gm.language == 1)
            {
                pickedItemInfo.text = Translator.TranslateText("Моё барахло может легко сломаться");
            }
            else if (gm.language == 2)
            {
                pickedItemInfo.text = Translator.TranslateText("Mis cosas se pueden romper facilmente");
            }
            else if (gm.language == 3)
            {
                pickedItemInfo.text = Translator.TranslateText("Mein Zeug kann schnell kaputt gehen");
            }
            else if (gm.language == 4)
            {
                pickedItemInfo.text = Translator.TranslateText("La mia roba può rompersi facilmente");
            }
            else if (gm.language == 5)
            {
                pickedItemInfo.text = Translator.TranslateText("Minhas coisas podem quebrar facilmente");
            }
            itemInfoAnim.SetTrigger(itemInfoUpdateString);
        }
        else if (index == 4)
        {
            if (gm.language == 0)
                pickedItemInfo.text = Translator.TranslateText("I feel warmth in my chest");
            else if (gm.language == 1)
            {
                pickedItemInfo.text = Translator.TranslateText("Чувствую тепло в груди");
            }
            else if (gm.language == 2)
            {
                pickedItemInfo.text = Translator.TranslateText("Siento calor en mi pecho");
            }
            else if (gm.language == 3)
            {
                pickedItemInfo.text = Translator.TranslateText("Ich fühl Wärme in meinem Bauch");
            }
            else if (gm.language == 4)
            {
                pickedItemInfo.text = Translator.TranslateText("Sento caldo nel mio petto");
            }
            else if (gm.language == 5)
            {
                pickedItemInfo.text = Translator.TranslateText("Sinto calor no meu peito");
            }
            itemInfoAnim.SetTrigger(itemInfoUpdateString);
        }
    }

    public void HideDialogue(float t)
    {
        StartCoroutine(HideDialogueOverTime(t));
    }

    IEnumerator HideDialogueOverTime(float t)
    {
        yield return  new WaitForSeconds(t);
        
        dialogueAnim.SetTrigger("Inactive");
        dialogueInteractor = null;
        dialoguePhrase.text = "";
        dialogueChoice.text = "";
    }
    
    public void DoorOpen()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I opened a door");
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Я открыл дверь");
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("He abierto una puerta");
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("Ich öffnete eine Tür");
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Ho aperto una porta");
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Eu abri uma porta");
        }
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }
    
    public void DoorClose()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I shut the door");
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Я захлопнул дверь");
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("He cerrado una puerta");
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("Ich schloß eine Tür");
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Ho chiuso una porta");
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Eu fechei uma porta");
        }
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }
    public void DoorUnlock()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I used a lockpick");
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Я использовал отмычку");
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("I used a lockpick");
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("I used a lockpick");
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Ho usato un grimaldello");
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Eu usei uma gazua");
        }
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }
    
    public void EnterBike()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I got on a weird bike");
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Я сел на странный велик");
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("Me subí a una bicicleta rara");
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("Ich stieg auf ein seltsames Fahrad");
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Sono salito su una strana bicicletta");
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Eu peguei uma bicicleta estranha");
        }
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }
    
    public void ExitBike()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I got down on grass");
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Я спрыгнул на траву");
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("Me puse en la hierba");
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("Ich stieg ab auf das Gras");
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Mi sono sdraiato sull'erba");
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Eu vou até a grama");
        }
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }

    public void LostAnEye()
    {
        if (eyePatchAnim)
            eyePatchAnim.SetBool("Active", true);
    }

    public void GotEye()
    {
        if (eyePatchAnim)
            eyePatchAnim.SetBool("Active", false);
    }

    public void BreakDoor()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I broke a door");
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Я сломал дверь");
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("He roto una puerta");
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("Ich zerstörte eine Tür");
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Ho rotto una porta");
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Eu Quebrei uma porta");
        }
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }
    public void InLove()
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I'm in love, I don't want to fight");
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Я влюблен, не хочу драться");
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("Estoy enamorado, no quiero pelear");
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("Ich bin verliebt, ich will nicht kämpfen");
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Sono innamorato, non voglio combattere");
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Estou apaixonado, não quero brigar");
        }
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }

    public void DashAttack(string targetMessage)
    {
        if (gm.language == 0)
            pickedItemInfo.text = Translator.TranslateText("I crashed into " + targetMessage);
        else if (gm.language == 1)
        {
            pickedItemInfo.text = Translator.TranslateText("Я врезался в " + targetMessage);
        }
        else if (gm.language == 2)
        {
            pickedItemInfo.text = Translator.TranslateText("Choqué contra ello " + targetMessage);
        }
        else if (gm.language == 3)
        {
            pickedItemInfo.text = Translator.TranslateText("Ich stürtze hinein " + targetMessage);
        }
        else if (gm.language == 4)
        {
            pickedItemInfo.text = Translator.TranslateText("Mi sono schiantato contro " + targetMessage);
        }
        else if (gm.language == 5)
        {
            pickedItemInfo.text = Translator.TranslateText("Eu colidi com " + targetMessage);
        }
        itemInfoAnim.SetTrigger(itemInfoUpdateString);
    }
    
    public void UpdateWeapons()
    {
        if (pm.hc.wc.activeWeapon && il.activeWeapon != WeaponPickUp.Weapon.Null)
        {
            weaponsBackground.SetActive(true);
            if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[0].weapon)
                activeWeaponIcon.sprite = weaponSprites[0];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[1].weapon)
                activeWeaponIcon.sprite = weaponSprites[1];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[2].weapon)
                activeWeaponIcon.sprite = weaponSprites[2];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[3].weapon)
                activeWeaponIcon.sprite = weaponSprites[3];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[4].weapon)
                activeWeaponIcon.sprite = weaponSprites[4];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[5].weapon)
                activeWeaponIcon.sprite = weaponSprites[5];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[6].weapon)
                activeWeaponIcon.sprite = weaponSprites[6];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[7].weapon)
                activeWeaponIcon.sprite = weaponSprites[7];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[8].weapon)
                activeWeaponIcon.sprite = weaponSprites[8];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[9].weapon)
                activeWeaponIcon.sprite = weaponSprites[9];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[10].weapon)
                activeWeaponIcon.sprite = weaponSprites[10];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[11].weapon)
                activeWeaponIcon.sprite = weaponSprites[11];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[12].weapon)
                activeWeaponIcon.sprite = weaponSprites[12];
            else if (pm.hc.wc.activeWeapon.weapon == pm.hc.wc.weaponList[13].weapon)
                activeWeaponIcon.sprite = weaponSprites[13];
            
            activeWeaponIcon.enabled = true;
            activeWeaponIcon.color = Color.Lerp(Color.red, Color.white, pm.hc.wc.activeWeapon.durability / pm.hc.wc.activeWeapon.durabilityMax);
            if (pm.hc.wc.activeWeapon && pm.hc.wc.activeWeapon.durability <= 0)
                activeWeaponBrokenIcon.enabled = true;
            else
                activeWeaponBrokenIcon.enabled = false;
            if (pm.hc.wc.activeWeapon)
            {
                activeWeaponName.text = pm.hc.wc.activeWeapon.dataRandomizer.generatedName[gm.language] + ". " + pm.hc.wc.activeWeapon.dataRandomizer.effectInName[gm.language];
            
                Color newColor = Color.white;
                if (pm.hc.wc.activeWeapon.dataRandomizer.statusEffect < 0)
                    newColor = Color.grey;
                else
                {
                    newColor = statuscolors[pm.hc.wc.activeWeapon.dataRandomizer.statusEffect];
                }

                activeWeaponName.color = newColor;   
            }
        }
        else
        {
            activeWeaponIcon.enabled = false;
            activeWeaponName.text = "";
        }

        if (pm.hc.wc.secondWeapon && il.secondWeapon != WeaponPickUp.Weapon.Null)
        {
            weaponsBackground.SetActive(true);
            if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[0].weapon)
                secondWeaponIcon.sprite = weaponSprites[0];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[1].weapon)
                secondWeaponIcon.sprite = weaponSprites[1];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[2].weapon)
                secondWeaponIcon.sprite = weaponSprites[2];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[3].weapon)
                secondWeaponIcon.sprite = weaponSprites[3];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[4].weapon)
                secondWeaponIcon.sprite = weaponSprites[4];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[5].weapon)
                secondWeaponIcon.sprite = weaponSprites[5];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[6].weapon)
                secondWeaponIcon.sprite = weaponSprites[6];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[7].weapon)
                secondWeaponIcon.sprite = weaponSprites[7];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[8].weapon)
                secondWeaponIcon.sprite = weaponSprites[8];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[9].weapon)
                secondWeaponIcon.sprite = weaponSprites[9];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[10].weapon)
                secondWeaponIcon.sprite = weaponSprites[10];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[11].weapon)
                secondWeaponIcon.sprite = weaponSprites[11];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[12].weapon)
                secondWeaponIcon.sprite = weaponSprites[12];
            else if (pm.hc.wc.secondWeapon.weapon == pm.hc.wc.weaponList[13].weapon)
                secondWeaponIcon.sprite = weaponSprites[13];

            if (pm.hc.wc.secondWeapon)
            {
                secondWeaponIcon.enabled = true;
                secondWeaponIcon.color = Color.Lerp(Color.red, Color.white, pm.hc.wc.secondWeapon.durability / pm.hc.wc.secondWeapon.durabilityMax);   
            }
            if (pm.hc.wc.activeWeapon && pm.hc.wc.activeWeapon.durability <= 0)
                activeWeaponBrokenIcon.enabled = true;
            else
                activeWeaponBrokenIcon.enabled = false;
        }
        else
        {
            secondWeaponIcon.enabled = false;
        }

        if (!pm.hc.wc.activeWeapon && !pm.hc.wc.secondWeapon)
        {
            weaponsBackground.SetActive(false);
        }
    }

    public void DisableGameUI()
    {
        gameUi.SetActive(false);
    }

    // suicide button
    public void Restart()
    {
        //gm.Restart();
        
        gm.TogglePause();
        if (gm.language == 0)
            gm.player.Damage(gm.player.healthMax * 5, gm.player.transform.position, gm.player.transform.position, null, "I've killed myself", false, "myself", null, null, false);
        else if (gm.language == 1)
            gm.player.Damage(gm.player.healthMax * 5, gm.player.transform.position, gm.player.transform.position, null, "Я убил себя", false, "я сам", null, null, false);
        else if (gm.language == 2)
            gm.player.Damage(gm.player.healthMax * 5, gm.player.transform.position, gm.player.transform.position, null, "Me he suicidado", false, "myself", null, null, false);
        else if (gm.language == 3)
            gm.player.Damage(gm.player.healthMax * 5, gm.player.transform.position, gm.player.transform.position, null, "Ich habe mich umgebracht", false, "myself", null, null, false);
        else if (gm.language == 4)
            gm.player.Damage(gm.player.healthMax * 5, gm.player.transform.position, gm.player.transform.position, null, "Ho ucciso me stesso", false, "myself", null, null, false);
        else if (gm.language == 5)
            gm.player.Damage(gm.player.healthMax * 5, gm.player.transform.position, gm.player.transform.position, null, "Eu me matei", false, "myself", null, null, false);
    }

    public void ExitToMenu()
    {
        VolumeFromScript(gm.volumeSliderValue);
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.ReturnToMenu();
        }
        else
            StartCoroutine(gm.ReturnToMainMenu(false));
    }
    
     public void QuitTheGame()
    {
        gm.ExitTheGame();
    }

    public void TogglePause()
    {
        int levCur = GutProgressionManager.instance.playerFloor;
        if (gm.hub) 
            floorNumber.text = "";
        else if (gm.arena)
            floorNumber.text = "???";
        else if (gm.tutorialPassed == 0)
        {
            levCur = 0;
            if (gm.language == 0)
                floorNumber.text = "Floor: " + levCur;
            else if (gm.language == 1)
                floorNumber.text = "Этаж: " + levCur;
            else if (gm.language == 2)
                floorNumber.text = "Piso: " + levCur;
            else if (gm.language == 3)
                floorNumber.text = "Etage: " + levCur;
            else if (gm.language == 4)
                floorNumber.text = "Pavimento: " + levCur;
            else if (gm.language == 5)
                floorNumber.text = "Chão: " + levCur;
        }
        else if (gm.tutorialPassed == 1)
        {
            if (gm.language == 0)
                floorNumber.text = "Floor: -" + levCur;
            else if (gm.language == 1)
                floorNumber.text = "Этаж: -" + levCur;
            else if (gm.language == 2)
                floorNumber.text = "Piso: -" + levCur;
            else if (gm.language == 3)
                floorNumber.text = "Etage: -" + levCur;
            else if (gm.language == 4)
                floorNumber.text = "Pavimento: -" + levCur;
            else if (gm.language == 5)
                floorNumber.text = "Chão: -" + levCur;
        }
        pauseAnim.gameObject.SetActive(gm.paused);

        if (!gm.paused)
        {
            CloseSettings();
        }
        
        if (gm.hub)
            restart.gameObject.SetActive(false);
        else if (gm.tutorialPassed == 1)
            restart.gameObject.SetActive(true);
        
        UpdateWeaponSwitchButtonText();
    }

    public void SteamLink()
    {
        gm.OpenSteamPage();
    }

    public void ToggleMouseInvert()
    {
        if (gm.mouseInvert == 0)
            gm.mouseInvert = 1;
        else
            gm.mouseInvert = 0;
    }
    
    public void MouseSens()
    {
        gm.MouseSense(mouseSens.value);
    }
    
    public void MouseSpeed()
    {
        gm.MouseSpeed(mouseSpeed.value);
    }

    //. this is used by UI
    public void Volume(float newValue)
    {
        gm.SetVolume(newValue);
        gm.volumeSliderValue = newValue;
    }
    // this by methods
    public void VolumeFromScript(float newValue)
    {
        gm.SetVolume(newValue);
    }

    public void Brightness()
    {
        gm.Brightness(brightness.value);
        //colorGradingLayer.postExposure.value = brightness.value;
        if (_colorAdjustments)
        _colorAdjustments.postExposure.Override(brightness.value);
    }
    
    public void Contrast()
    {
        gm.Contrast(contrast.value);
        //colorGradingLayer.postExposure.value = brightness.value;
        if (_colorAdjustments)
        _colorAdjustments.contrast.Override(contrast.value);
    }

    void LoadSavedSettings()
    {
        /*
        contrast.value = gm.contrast;
        if (_colorAdjustments)
        _colorAdjustments.contrast.Override(contrast.value);

        brightness.value = gm.brightness;
        if (_colorAdjustments)
        _colorAdjustments.postExposure.Override(brightness.value);
        */
        
        mouseSens.value = gm.mouseSensitivity;
        mouseSpeed.value = gm.mouseLookSpeed;
    }

    public void GetSkill(string skillInfo)
    {
        if (getSkillCoroutine !=null)
            StopCoroutine(GetSkillCoroutine(skillInfo));
        
        getSkillCoroutine = StartCoroutine(GetSkillCoroutine(skillInfo));
    }
    
    public IEnumerator GetSkillCoroutine(string skillInfo)
    {
        Color newColor = Color.white;
        newColor.a = 0;
        epigraphText.color = newColor;
        epigraphText.text = skillInfo;
        
        newColor.a = 1;
        float t = 0;
        while (t < 3)
        {
            newColor.a += 0.1f;
            epigraphText.color = newColor;
            t += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        
        t = 0;
        while (t < 3)
        {
            newColor.a -= 0.033f;
            epigraphText.color = newColor;
            t += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        epigraphText.text = "";
    }

    public void Death(string damager)
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            ToggleGameUi(false, false);
            
            return;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        #region set text
        string _imdead = "";
        string killedBy = "Killed by: ";
        string gold = "Collected gold: ";
        string _keys = "Collected keys: ";
        string inHub = "Go back to the field.";
        string onLevel = "Restart the floor.";
        
        if (gm.language == 0 && gm.difficultyLevel != GameManager.GameMode.MeatZone)
            _imdead = "I died on floor ";
        
        if (gm.language == 1)
        {
            if (gm.difficultyLevel != GameManager.GameMode.MeatZone)
                _imdead = "Я умер на этаже ";
            killedBy = "Кто убил: ";
            gold = "Собрано золота: ";
            _keys = "Собрано ключей: ";
            onLevel = "Этаж заново";
            inHub = "Вернуться в поле";
        }
        else if (gm.language == 2)
        {
            if (gm.difficultyLevel != GameManager.GameMode.MeatZone)
                _imdead = "Muerto en el piso:  ";
            
            killedBy = "Asesinado por: ";
            gold = "Oro recogido: ";
            _keys = "Llaves recogidas: ";
            onLevel = "Reiniciar el piso.";
            inHub = "Volver al Campo.";
        }
        else if (gm.language == 3)
        {
            if (gm.difficultyLevel != GameManager.GameMode.MeatZone)
                _imdead = "Ich bin auf der Etage gestorben: ";
            
            killedBy = "Getötet von: ";
            gold = "Gesammeltes Gold: ";
            _keys = "Gesammelte Schlüssel: ";
            onLevel = "Starte die Etage erneut";
            inHub = "Gehe zurück zum Feld.";
        }
        else if (gm.language == 4)
        {
            if (gm.difficultyLevel != GameManager.GameMode.MeatZone)
                _imdead = "Sono morto al piano: ";
            
            killedBy = "Ucciso da: ";
            gold = "Oro collezionato: ";
            _keys = "Chiavi collezionate: ";
            onLevel = "Ricomincia il piano.";
            inHub = "Torna al Campo.";
        }
        else if (gm.language == 5)
        {
            if (gm.difficultyLevel != GameManager.GameMode.MeatZone)
               _imdead = "Eu morri no chão: ";
            
            killedBy = "Morto por: ";
            gold = "Ouro coletado: ";
            _keys = "Chaves coletadas: ";
            onLevel = "Reinicie o chão.";
            inHub = "Volte para o campo.";
        }

        #endregion
        int level = GutProgressionManager.instance.playerFloor;
        
        if (gm.tutorialPassed == 1 && gm.difficultyLevel != GameManager.GameMode.MeatZone)
            _imdead += level * -1;
        
        imDead.text = _imdead; 
        restartInHubButtonText.text = inHub;
        restartInLevelButtonText.text = onLevel;
        whoKilledMe.text = killedBy + damager;
        goldOnDeath.text = gold + Mathf.Round(il.gold);
        keysOnDeath.text = _keys + il.keys;
        restartInHubButtonText.text = inHub;
        restartInLevelButtonText.text = onLevel;

        if (gm.wasInHub == 0)
            restartInLevelButton.gameObject.SetActive(false);    
        
        deathWindow.SetActive(true);
    }

    // this method is called from the death window
    // 0 is return to hub, 1 is to return to level
    public void Restart(int gub) // 0 - hub, 1 - labirynth
    {
        gm.Restart(gub);
    }

    public void UpdateJournalIcon()
    {
        questIcon.SetActive(true);
    }

    public void ToggleQuestWindow()
    {
        UpdateFloor();
        pac.ToggleQuests();
        questIcon.SetActive(false);
        
        if (!questWindow.activeInHierarchy)
        {
            //ShowQuests();
            questWindow.SetActive(true);   
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedInJournal);
            
            UpdateWeightString();
            
            ToggleInventoryPage(3);
        }
        else
        {
            questWindow.SetActive(false);
        }
    }

    void UpdateWeightString()
    {
            switch (gm.language)
            {
                case 0:
                    if (il.currentWeight == 0)
                        currentWeightFeedback.text = "I have nothing";
                    else if (il.currentWeight == 1)
                        currentWeightFeedback.text = "I carry 1 item. My speed is normal";
                    else if (il.currentWeight == 2)
                        currentWeightFeedback.text = "I carry 2 items. My speed is normal";
                    else
                        currentWeightFeedback.text = "I carry " + il.currentWeight + " items. I'm " + Mathf.RoundToInt((1 - pm.weightSpeedScaler) * 100) + "% slower";
                    break;
                case 1:
                    if (il.currentWeight == 0)
                        currentWeightFeedback.text = "У меня ничего нет";
                    else if (il.currentWeight == 1)
                        currentWeightFeedback.text = "У меня есть 1 предмет. Скорость нормальная";
                    else if (il.currentWeight == 2)
                        currentWeightFeedback.text = "У меня есть 2 предмета. Скорость нормальная";
                    else
                        currentWeightFeedback.text = "Предметов при себе: " + il.currentWeight + ". Я медленнее на " + Mathf.RoundToInt((1 - pm.weightSpeedScaler) * 100) + "%";
                    break;
                case 2:
                    if (il.currentWeight == 0)
                        currentWeightFeedback.text = "No tengo nada";
                    else if (il.currentWeight == 1)
                        currentWeightFeedback.text = "Tengo 1 artículo Velocidad normal";
                    else if (il.currentWeight == 2)
                        currentWeightFeedback.text = "Tengo 2 artículos Velocidad normal";
                    else
                        currentWeightFeedback.text = "Artículos conmigo: " + il.currentWeight + "3. Soy un " + Mathf.RoundToInt((1 - pm.weightSpeedScaler) * 100) + "% más lento";
                    break;
                case 3:
                    if (il.currentWeight == 0)
                        currentWeightFeedback.text = "Ich habe nichts";
                    else if (il.currentWeight == 1)
                        currentWeightFeedback.text = "Ich habe 1 Artikel. Normale Geschwindigkeit";
                    else if (il.currentWeight == 2)
                        currentWeightFeedback.text = "Ich habe 2 Artikel. Normale Geschwindigkeit";
                    else
                        currentWeightFeedback.text = "Artikel bei mir: " + il.currentWeight + ". Ich bin " + Mathf.RoundToInt((1 - pm.weightSpeedScaler) * 100) + "% langsamer";
                    break;
                case 4:
                    if (il.currentWeight == 0)
                        currentWeightFeedback.text = "Non ho nulla";
                    else if (il.currentWeight == 1)
                        currentWeightFeedback.text = "Porto 1 oggetto. La mia velocità è normale";
                    else if (il.currentWeight == 2)
                        currentWeightFeedback.text = "Porto 2 oggetti. La mia velocità è normale";
                    else
                        currentWeightFeedback.text = "Porto " + il.currentWeight + ". oggetti. Sono lento al " + Mathf.RoundToInt((1 - pm.weightSpeedScaler) * 100) + "%";
                    break;
                case 5:
                    if (il.currentWeight == 0)
                        currentWeightFeedback.text = "eu não tenho nada";
                    else if (il.currentWeight == 1)
                        currentWeightFeedback.text = "Estou Carregando 1 item. Minha velocidade é normal";
                    else if (il.currentWeight == 2)
                        currentWeightFeedback.text = "Estou Carregando 2 itens. Minha velocidade é normal";
                    else
                        currentWeightFeedback.text = "Estou Carregando " + il.currentWeight + " Itens. Estou " + Mathf.RoundToInt((1 - pm.weightSpeedScaler) * 100) + "% mais lento";
                    break;

            }
    }

    public void ShowMementoChoice(SkillInfo firstMemento, SkillInfo secondMemento)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mementoChooseParticlesEmission.rateOverTime = 50;
        mementoChoiceWindow.gameObject.SetActive(true);
        VolumeFromScript(0.0001f);
        mementoChoiceWindow.SetBool("Active", true);
        mementoFirstImage.sprite = firstMemento.image;
        if (gm.language == 0)
            mementoFirstText.text = firstMemento.info;
        else if (gm.language == 1)
            mementoFirstText.text = firstMemento.infoRu;
        else if (gm.language == 2)
            mementoFirstText.text = firstMemento.infoESP;
        else if (gm.language == 3)
            mementoFirstText.text = firstMemento.infoGER;
        else if (gm.language == 3)
            mementoFirstText.text = firstMemento.infoIT;
        else if (gm.language == 3)
            mementoFirstText.text = firstMemento.infoSPBR;
        

        if (Random.value > 0.5f)
        {
            secondMementoDisabled = false;
            mementoSecondImage.color = Color.white;
            mementoSecondImage.sprite = secondMemento.image;
            if (gm.language == 0)
                mementoSecondText.text = secondMemento.info;
            else if (gm.language == 1)
                mementoSecondText.text = secondMemento.infoRu;
            else if (gm.language == 2)
                mementoSecondText.text = secondMemento.infoESP;
            else if (gm.language == 3)
                mementoSecondText.text = secondMemento.infoGER;
            else if (gm.language == 4)
                mementoSecondText.text = secondMemento.infoIT;
            else if (gm.language == 5)
                mementoSecondText.text = secondMemento.infoSPBR;

        }
        else
        {
            secondMementoDisabled = true;
            mementoSecondImage.color = Color.black;
            if (gm.language == 0)
                mementoSecondText.text = "You can not remember this...";
            else if (gm.language == 1)
                mementoSecondText.text = "Ты не можешь вспомнить это...";
            else if (gm.language == 2)
                mementoSecondText.text = "No puedes recordar esto...";
            else if (gm.language == 3)
                mementoSecondText.text = "Du kannst dich nicht daran erinnern...";
            else if (gm.language == 4)
                mementoSecondText.text = "Non puoi ricordare questo...";
            else if (gm.language == 5)
                mementoSecondText.text = "Você não consegue se lembrar disso ...";
        }
    }

    public void ChooseMemento(int side)
    {
        if (side == 0 || side == 1 && !secondMementoDisabled)
        {
            gm.ChooseMemento(side);
        
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        
            mementoChoiceWindow.SetBool("Active", false);
            StartCoroutine(CloseMementoWindow());   
            
            mementoChooseParticlesEmission.rateOverTime = 0;
        }
    }

    IEnumerator CloseMementoWindow()
    {
        VolumeFromScript(gm.volumeSliderValue);
        Time.timeScale = 1;
        yield return new WaitForSeconds(0.5f);
        
        mementoChoiceWindow.gameObject.SetActive(false);
    }
    
    public void ToggleInventoryPage(int index)
    {
        for (int i = 0; i < mvc.Count; i++)
        {
            if (i != index)
            {
                mvc[i].Close();
                mvc[i].gameObject.SetActive(false);
            }
            else
            {
                mvc[i].gameObject.SetActive(true);
                mvc[i].Init();
            }
        }

        switch (index)
        {
            case 0:
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelectedInInventory);
                break;
            case 1:
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelectedInMementos);
                break;
            case 4:
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelectedInQuestItems);
                break;
        }
    }

    public void OpenWiki()
    {
        GameManager.instance.OpenWikiPage();
        gm.OpenWikiPage();
    }
    
    public void ReportBug()
    {
        Application.OpenURL("https://discord.gg/hsPKzDT");
    }

    public void Settings()
    {
        pauseAnim.gameObject.SetActive(false);
        settingsCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedInSettings);
    }

    public void CloseSettings()
    {
        //gm.SaveVolume(volume.value);

        settingsCanvas.SetActive(false);
        VolumeFromScript(gm.volumeSliderValue);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedInPause);
        if (gm.paused)
            gm.TogglePause();
    }

    public void UpdateBuff(int index, float amount, bool active)
    {
        if (amount <= 0)
        {
            buffBarUi[index].fill.gameObject.SetActive(false);
            buffBarUi[index].background.gameObject.SetActive(false);
            buffBarUi[index].name.gameObject.SetActive(false);
        }
        else
        {
            buffBarUi[index].fill.gameObject.SetActive(true);
            buffBarUi[index].background.gameObject.SetActive(true);
            buffBarUi[index].name.gameObject.SetActive(true);
        }

        float fillAmount = amount / 100f;
        buffBarUi[index].fill.fillAmount = fillAmount;
        
        if (active)
        {
            buffBarUi[index].fill.color = Color.red;
            buffBarUi[index].fill.color = new Color(buffBarUi[index].fill.color.r, buffBarUi[index].fill.color.g, buffBarUi[index].fill.color.b, 1); 
        }
        else
        {
            buffBarUi[index].fill.color = new Color(buffBarUi[index].fill.color.r, buffBarUi[index].fill.color.g, buffBarUi[index].fill.color.b, 0.8f); 
        }
    }
}

[Serializable]
public class StatusEffectsUi
{
    public TextMeshProUGUI name;
    public Image fill;
    public Image background;
    //public Outline backgroundOutline;
}

 [Serializable]
public class BaffBarUi
{
    public TextMeshProUGUI name;
    public Image fill;
    public Image background;
    //public Outline backgroundOutline;
}