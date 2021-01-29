using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExtendedItemSpawn;
using PlayerControls;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemsList : MonoBehaviour
{
    public static ItemsList instance;

    public enum ResourceType {Tool, Key, Gold, Skill, QuestItem, Lockpick, HeartContainer, GoldenKey};
    public List<Interactable> interactables;
    public List<WeaponPickUp> weaponsOnLevel;
    public WeaponPickUp.Weapon activeWeapon = WeaponPickUp.Weapon.Null;
    public List<string> activeWeaponDescriptions = new List<string>();
    public List<string> secondWeaponDescriptions = new List<string>();
    public WeaponPickUp.Weapon secondWeapon = WeaponPickUp.Weapon.Null;

    public float playerCurrentHealth = 1000;

    [Header("Snout Music")]
    public PoemData cassetteNames;
    public List<int> unlockedTracks = new List<int>();
    public List<int> tracksOverall = new List<int>();
    
    public List<int> activeWeaponInfo;
    public List<int> secondWeaponInfo;

    public int goldenKeysAmount = 0;
    public int currentWeight = 0;
    
    public List<int> activeWeaponSavedBarrels_1;
    public List<int> activeWeaponSavedBarrels_2;
    public Vector3 activeWeaponRotation;
    public float activeWeaponClipSize = 0;
    public bool activeWeaponNpc = false;
    public bool activeWeaponNpcPaid = false;
    public List<int> secondWeaponSavedBarrels_1;
    public List<int> secondWeaponSavedBarrels_2;
    public Vector3 secondWeaponRotation;
    public bool activeWeaponCrazyGun = false;
    public bool secondWeaponCrazyGun = false;
    public float secondWeaponClipSize = 0;
    public bool secondWeaponNpc = false;
    public bool secondWeaponNpcPaid = false;
    
    public float activeWeaponDurability = 0;
    public float secondWeaponDurability = 0;
    public float activeWeaponClip = 0;
    public float secondWeaponClip = 0;
    
    public List<SkillInfo> savedSkills;
    public List<int> savedQuestItems = new List<int>();

    public AmmoDataStorage ammoDataStorage;
    public SkillsData skillsData;
    public List<SkillInfo> skillsPool;
    
    public List<QuestItem> questItemsList = new List<QuestItem>();

    [Header("0 - painkillers; 1 - antidote")]
    public List<Tool> savedTools;

    public PlayerSkillsController.Cult activeCult = PlayerSkillsController.Cult.none;

    public int heartContainerAmount = 0;
    public float badReputaion = 1; // 1; 5
    public int keys = 0;
    public int lockpicks = 0;
    public float gold = 0;
    public int goldSpentOnSon = 0;
    public int lostAnEye = 0;

    [HideInInspector]
    public Transform itemCanvasesParent;

    UiManager ui;
    GameManager gm;
    SpawnController sc;
    WeaponControls wc;
    PlayerMovement pm;
    private MouseLook ml;
    private PlayerSkillsController psc;

    private int levelsOnRunCurrent = 0;
    private int goldOnRunCurrent = 0;

    public List<int> foundHerPiecesOnFloors = new List<int>();
    public List<int> herPiecesApplied = new List<int>();
    
    public List<Interactable> herPartsPrefabs = new List<Interactable>();

    private void Awake()
    {
        instance = this;
        skillsPool = new List<SkillInfo>(skillsData.skills);
    }

    public void ClearDataFromLevel()
    {
        keys = 0;
        interactables.Clear();
        weaponsOnLevel.Clear();
    }

    public void LoadInventory(SavingData data)
    {
        for (int i = 0; i < 2; i++)
        {
            int weaponIndex = -1;
            if (i == 0)
                weaponIndex = data.savedWeaponActive;
            else
                weaponIndex = data.savedWeaponSecond;

            var weapon = WeaponPickUp.Weapon.Null;
            
            switch (weaponIndex)
            {
                case 0:
                    weapon = WeaponPickUp.Weapon.Axe;
                    break;
                case 1:
                    weapon = WeaponPickUp.Weapon.Pistol;
                    break;
                case 2:
                    weapon = WeaponPickUp.Weapon.Revolver;
                    break;
                case 3:
                    weapon = WeaponPickUp.Weapon.Shotgun;
                    break;
                case 4:
                    weapon = WeaponPickUp.Weapon.TommyGun;
                    break;
                case 5:
                    weapon = WeaponPickUp.Weapon.Pipe;
                    break;
                case 6:
                    weapon = WeaponPickUp.Weapon.Map;
                    break;
                case 7:
                    weapon = WeaponPickUp.Weapon.Knife;
                    break;
                case 8:
                    weapon = WeaponPickUp.Weapon.Torch;
                    break;
                case 9:
                    weapon = WeaponPickUp.Weapon.OldPistol;
                    break;
                case 10:
                    weapon = WeaponPickUp.Weapon.Polaroid;
                    break;
                case 11:
                    weapon = WeaponPickUp.Weapon.Shield;
                    break;
                case 12:
                    weapon = WeaponPickUp.Weapon.MeatSpear;
                    break;
                case 13:
                    weapon = WeaponPickUp.Weapon.VeinWhip;
                    break;
                
                default: weapon = WeaponPickUp.Weapon.Null;
                    break;
            }

            if (i == 0)
                activeWeapon = weapon;
            else
                secondWeapon = weapon;
        }
        
        if (data.activeWeaponSavedBarrels_1 != null)
            activeWeaponSavedBarrels_1 = data.activeWeaponSavedBarrels_1;
        if (data.activeWeaponSavedBarrels_2 != null)
            activeWeaponSavedBarrels_2 = data.activeWeaponSavedBarrels_2;
        if (data.activeWeaponInfo != null)
            activeWeaponInfo = data.activeWeaponInfo;

        
        activeWeaponClip = data.activeWeaponClip;
        activeWeaponClipSize = data.activeWeaponClipSize;
        activeWeaponNpc = data.activeWeaponNpc;
        activeWeaponCrazyGun = data.activeWeaponCrazyGun;
        activeWeaponDurability = data.activeWeaponDurability;

        if (data.secondWeaponSavedBarrels_1 != null)
            secondWeaponSavedBarrels_1 = data.secondWeaponSavedBarrels_1;
        if (data.secondWeaponSavedBarrels_2 != null)
            secondWeaponSavedBarrels_2 = data.secondWeaponSavedBarrels_2;
        if (data.secondWeaponInfo != null)
            secondWeaponInfo = data.secondWeaponInfo;
        
        secondWeaponClip = data.secondWeaponClip;
        secondWeaponClipSize = data.secondWeaponClipSize;
        secondWeaponNpc = data.secondWeaponNpc;
        secondWeaponCrazyGun = data.secondWeaponCrazyGun;
        secondWeaponDurability = data.secondWeaponDurability;
        
                
        // load skills
        if (data.savedSkills != null)
        {
            for (int i = 0; i < skillsData.skills.Count; i++)
            {
                if (data.savedSkills.Contains(skillsData.skills[i].skillIndex))
                    savedSkills.Add(skillsData.skills[i]);
            }   
        }
        
        // load tools
        if (data.toolsAmount != null)
        {
            for (int i = 0; i < savedTools.Count; i++)
            {
                savedTools[i].amount = data.toolsAmount[i];
            }   
        }
        
        // load ammo
        ammoDataStorage.ResetAllAmmo();
        if (data.pistolAmmo > 0)
            ammoDataStorage.AddAmmo(WeaponPickUp.Weapon.Pistol, data.pistolAmmo);
        if (data.revolverAmmo > 0)
            ammoDataStorage.AddAmmo(WeaponPickUp.Weapon.Revolver, data.revolverAmmo);
        if (data.shotgunAmmo > 0)
            ammoDataStorage.AddAmmo(WeaponPickUp.Weapon.Shotgun, data.shotgunAmmo);
        if (data.tommyAmmo > 0)
            ammoDataStorage.AddAmmo(WeaponPickUp.Weapon.TommyGun, data.tommyAmmo);
        if (data.blunderbussAmmo > 0)
            ammoDataStorage.AddAmmo(WeaponPickUp.Weapon.OldPistol, data.blunderbussAmmo);
    }

    public int GetAmmoInGun(WeaponPickUp.Weapon weaponType)
    {
        int amount = 0;
        if (activeWeapon == weaponType)
            amount += Mathf.RoundToInt(activeWeaponClip);
        
        if (secondWeapon == weaponType)
            amount += Mathf.RoundToInt(secondWeaponClip);
        
        return amount;
    }

    private void Start()
    {
        gm = GameManager.instance;
        ui = UiManager.instance;
        sc = SpawnController.instance;

        itemCanvasesParent = new GameObject().transform;
        itemCanvasesParent.gameObject.name = "Item Canvases Parent";
        
        InvokeRepeating("UpdateCurrentWeight", 1, 1);
        
    }

    public int GetToolsCount()
    {
        int amount = 0;
        for (int i = 0; i < savedTools.Count; i++)
        {
            amount += savedTools[i].amount;
        }

        return amount;
    }
    
    public void UpdateCurrentWeight()
    {
        if (pm)
        {
            currentWeight = 0;

            if (activeWeapon != WeaponPickUp.Weapon.Null)
                currentWeight++;
        
            if (secondWeapon != WeaponPickUp.Weapon.Null)
                currentWeight++;
        
            for (int i = 0; i < savedTools.Count; i++)
            {
                currentWeight += savedTools[i].amount;
            }

            pm.UpdateWeightSpeedScaler(currentWeight);   
        }
    }
    
    void RestoreLinks()
    {
        gm = GameManager.instance;
        ui = UiManager.instance;
        sc = SpawnController.instance;
        wc = WeaponControls.instance;
        pm = PlayerMovement.instance;
        ml = MouseLook.instance;
        psc = PlayerSkillsController.instance;
    }

    public void ResetPlayerInventory()
    {
        levelsOnRunCurrent = 0;
        goldOnRunCurrent = 0;
        activeCult = PlayerSkillsController.Cult.none;
        ammoDataStorage.ResetAllAmmo();
        
        for (var index = 0; index < savedTools.Count; index++)
        {
            var t = savedTools[index];
            t.amount = 0;
        }

        lockpicks = 0;
        
        savedSkills.Clear();
        
        wc = WeaponControls.instance;
        wc.ResetInventory();
        activeWeapon = WeaponPickUp.Weapon.Null;
        secondWeapon = WeaponPickUp.Weapon.Null;

        
        skillsPool.Clear();
        skillsPool = new List<SkillInfo>(skillsData.skills);
        
        lostAnEye = 0;

        playerCurrentHealth = 1000 + 100 * heartContainerAmount;
    }
    
    public void Init()
    {
        instance = this;
        wc = WeaponControls.instance;
        pm = PlayerMovement.instance;
        ml = MouseLook.instance;
        psc = PlayerSkillsController.instance;
        sc = SpawnController.instance;


        
        if (playerCurrentHealth < 350)
            playerCurrentHealth = 350;

        pm.hc.health = playerCurrentHealth;
        
        UpdateWeaponOnLoad();
        UpdateSkillsOnLoad();

        if (gm.hub) 
            GenerateAllWeaponsOnFloor();
        
        UpdateCurrentWeight();
    }

    public void PlayerFinishedLevel()
    {
        levelsOnRunCurrent++;
        gm.SaveNewLevelsOnRun(levelsOnRunCurrent);
    }

    public void GenerateAllWeaponsOnFloor()
    {
        for (var index = 0; index < interactables.Count; index++)
        {
            var i = interactables[index];
            if (i.weaponPickUp)
            {
                i.weaponPickUp.weaponDataRandomier.GenerateOnSpawn(false, false);

                if (i.weaponPickUp.weaponConnector)
                    i.weaponPickUp.weaponConnector.GenerateOnSpawn();
            }
        }
    }

    public void MakeAllWeaponsEvil()
    {
        for (var index = 0; index < interactables.Count; index++)
        {
            var i = interactables[index];
            if (i.weaponPickUp)
            {
                i.weaponPickUp.weaponDataRandomier.GenerateOnSpawn(false, true);

                if (i.weaponPickUp.weaponConnector)
                    i.weaponPickUp.weaponConnector.GenerateOnSpawn();
            }
        }
    }

    public void AddToBadReputation(float add)
    {
        return;
        
        if (!Mathf.Approximately(add, 0))
        {
            ui = UiManager.instance;
            var prevRep = badReputaion;
            prevRep = Mathf.FloorToInt(prevRep);
            badReputaion += add;
            badReputaion = Mathf.Clamp(badReputaion, 1, 5);
            
            var newRep = badReputaion;
            newRep = Mathf.FloorToInt(newRep);
            print("ADD BAD REP OF " + add);

            if (badReputaion >= 5 && SpawnController.instance)
            {
                SpawnController.instance.StartBadRepChase();
            }
            
            if (ui == null)
                return;
            
            if (prevRep < newRep)
            {
                //feedback on repo up
                ui.UpdateReputation(add);
            }
            else if (prevRep > newRep)
            {
                //feedback on repo down
                ui.UpdateReputation(add);
            }
        }
    }

    public void GetRandomTool()
    {
        savedTools[Random.Range(0, savedTools.Count)].amount++;

        pm.hc.wc.FindNewTool();
        if (!ui)
            ui = UiManager.instance;
        ui.UpdateTools();
    }
    
    public void PickAmmo(AmmoPickUp pickUp)
    {
        RestoreLinks();

        if (psc.usefulAmmo) // change ammo to weapons ammo
        {
            if (wc.activeWeapon && wc.activeWeapon.weaponType != WeaponController.Type.Melee)
            {
                pickUp.weaponType = activeWeapon;
                if (pickUp.amount > 1)
                    pickUp.amount = wc.activeWeapon.ammoClipMax;
            }
            else if (wc.secondWeapon && wc.secondWeapon.weaponType != WeaponController.Type.Melee)
            {
                pickUp.weaponType = secondWeapon;
                if (pickUp.amount > 1)
                    pickUp.amount = wc.secondWeapon.ammoClipMax;
            }
        }

        
        switch (pickUp.weaponType) // change name
        {
            case WeaponPickUp.Weapon.Pistol:
                if (gm.language == 0)
                {
                    if (pickUp.amount == 1)
                        pickUp.interactable.itemName = "Pistol bullet";
                    else
                        pickUp.interactable.itemName = "Pistol bullets"; 
                }
                else if (gm.language == 1)   
                {
                    if (pickUp.amount == 1)
                        pickUp.interactable.itemName = "Пистолетная пуля";
                    else
                        pickUp.interactable.itemName = "Пистолетные пули"; 
                }
                break;
            
            case WeaponPickUp.Weapon.Revolver:
                if (gm.language == 0)
                {
                    if (pickUp.amount == 1)
                        pickUp.interactable.itemName = "Revolver bullet";
                    else
                        pickUp.interactable.itemName = "Revolver bullets"; 
                }
                else if (gm.language == 1)   
                {
                    if (pickUp.amount == 1)
                        pickUp.interactable.itemName = "Револьверная пуля";
                    else
                        pickUp.interactable.itemName = "Револьверные пули"; 
                }                
                break;

            case WeaponPickUp.Weapon.Shotgun:
                if (gm.language == 0)
                {
                    if (pickUp.amount == 1)
                        pickUp.interactable.itemName = "Shotgun slug";
                    else
                        pickUp.interactable.itemName = "Shotgun ammo"; 
                }
                else if (gm.language == 1)   
                {
                    if (pickUp.amount == 1)
                        pickUp.interactable.itemName = "Ружейный патрон";
                    else
                        pickUp.interactable.itemName = "Ружейные патроны"; 
                }                                
                break;
            
            case WeaponPickUp.Weapon.TommyGun:
                if (gm.language == 0)
                {
                    if (pickUp.amount == 1)
                        pickUp.interactable.itemName = "Thompson bullet";
                    else
                        pickUp.interactable.itemName = "Thompson ammo";
                }
                else if (gm.language == 1)   
                {
                    if (pickUp.amount == 1)
                        pickUp.interactable.itemName = "Пуля для Томми";
                    else
                        pickUp.interactable.itemName = "Пули для Томми"; 
                }                
                break;
        }
        
        ammoDataStorage.AddAmmo(pickUp.weaponType, pickUp.amount);
        ui.GotItem(pickUp.interactable);
        ui.UpdateAmmo();
        sc.mobSpawnTimer += 3;
    }

    public void FoundKeyOnClient(int playerId)
    {
        if (!gm.hub && gm.tutorialPassed == 1)
        {
            gm.DifficultyUp();
        }
        GameManager.instance.keysFound++;
        UiManager.instance.UpdateKeys(keys, playerId);
    }
    
    public void PickResource(ResourcePickUp pickUp)
    {
        RestoreLinks();
        if (HubProgressionManager.instance)
            HubProgressionManager.instance.PermanentPickupPickedUp(pickUp);

        switch (pickUp.resourceType)
        {
            case ResourceType.Tool:
                AddTool(pickUp.tool);
                sc.mobSpawnTimer += 10;
                break;

            case ResourceType.Key:
                keys++;
                
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    GLNetworkWrapper.instance.FoundKey(GLNetworkWrapper.instance.playerNetworkObjects.IndexOf(GLNetworkWrapper.instance.localPlayer));
                }
                else
                {
                    FoundKeyOnClient(-1);
                }
                
                break;

            case ResourceType.GoldenKey:
                goldenKeysAmount++;
                if (gm.hub)
                    HubProgressionManager.instance.GoldenKeyPickUpInHub(pickUp);
                else
                    gm.goldenKeysFoundOnFloors.Add(GutProgressionManager.instance.playerFloor);
                break;
            
            case ResourceType.Lockpick:
                lockpicks++;
                //ui.UpdateLockpicks();
                sc.mobSpawnTimer ++;
                break;
            
            case ResourceType.Gold:
                GetGold(1);
                sc.mobSpawnTimer ++;

                if (!gm.demo && gold >= 100)
                {
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_17");
                    if (gold >= 200)
                    {
                        SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_18");
                        if (gold >= 500)
                        {
                            SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_19");
                            if (gold >= 1000)
                            {
                                SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_20");
                            }
                        }
                    }
                }
                break;
            
            case  ResourceType.Skill:
                // show memento choice window
                gm.ShowMementoChoiceWindow();
                break; 
            case  ResourceType.HeartContainer:
                heartContainerAmount++;
                pm.hc.ApplyHeartContainer();
                break; 
            case ResourceType.QuestItem:
                if (pickUp.questItemIndex >= 0 && !savedQuestItems.Contains(pickUp.questItemIndex))
                {
                    savedQuestItems.Add(pickUp.questItemIndex);
                    ui.UpdateJournalIcon();   
                }
                break;
        }
        
    }

    void AddTool(ToolController tool)
    {
        RestoreLinks();
        savedTools[tool.toolIndex].amount++;
        wc.currentToolIndex = tool.toolIndex;
        if (tool.forceKnown)
            savedTools[tool.toolIndex].known = true;
        
        for (int i = 0; i < wc.allTools.Count; i++)
        {
            wc.allTools[i].gameObject.SetActive(i == wc.currentToolIndex);
        }
        
        UpdateCurrentWeight();
        ui.UpdateTools();
    }
    
     private void UpdateWeaponOnLoad() 
    {
        wc = WeaponControls.instance;
        pm = PlayerMovement.instance;

        for (var index = 0; index < wc.weaponList.Count; index++)
        {
            wc.weaponList[index].gameObject.SetActive(false);
        }

        var active = wc.weaponList.SingleOrDefault(w => w.weapon == activeWeapon);
        
        if (active != null)
        {
            var baseTransform = active.transform;
            wc.activeWeapon = Instantiate(active, baseTransform.position, baseTransform.rotation);
            wc.activeWeapon.transform.parent = active.transform.parent;
            wc.activeWeapon.gameObject.SetActive(true);
            wc.activeWeapon.ammoClip = activeWeaponClip;
            wc.activeWeapon.ammoClipMax = activeWeaponClipSize;
            wc.activeWeapon.durability = activeWeaponDurability;
            wc.activeWeapon.descriptions = new List<string>(activeWeaponDescriptions);
            pm.mouseLook.activeWeaponHolderAnim = wc.activeWeapon.weaponMovementAnim;
            
            wc.activeWeapon.dataRandomizer.ReturnInfo(false);
            
            if (wc.activeWeapon.weaponConnector)
            {
                wc.activeWeapon.weaponConnector.ConfigureBarrels(activeWeaponSavedBarrels_1, activeWeaponSavedBarrels_2, activeWeaponRotation, activeWeaponCrazyGun);
            }
        }
        else
            activeWeaponClip = 0;

        var second = wc.weaponList.SingleOrDefault(w => w.weapon == secondWeapon);
        if (second != null)
        {
            var baseTransform = second.transform;
            wc.secondWeapon = Instantiate(second, baseTransform.position, baseTransform.rotation);
            wc.secondWeapon.transform.parent = second.transform.parent;
            wc.secondWeapon.gameObject.SetActive(false);
            wc.secondWeapon.ammoClip = secondWeaponClip;
            wc.secondWeapon.ammoClipMax = secondWeaponClipSize;
            wc.secondWeapon.durability = secondWeaponDurability;
            wc.secondWeapon.descriptions = new List<string>(secondWeaponDescriptions);
            
            wc.secondWeapon.dataRandomizer.ReturnInfo(true);
            
            if (wc.secondWeapon.weaponConnector)
            {
                wc.secondWeapon.weaponConnector.ConfigureBarrels(secondWeaponSavedBarrels_1, secondWeaponSavedBarrels_2, secondWeaponRotation, secondWeaponCrazyGun);
            }
        }
        else
            secondWeaponClip = 0;
    }

     
    void UpdateSkillsOnLoad()
    {
        psc.playerSkills = new List<SkillInfo>(savedSkills);
        psc.activeCult = activeCult;
        psc.StartSkillsAction();
    }

    public void PickWeapon(WeaponPickUp weaponPickUp)
    {
        RestoreLinks();
        
        wc.PickUpWeapon(weaponPickUp);
        weaponsOnLevel.Remove(weaponPickUp);
        ui.UpdateAmmo();
        ui.UpdateJournalIcon();
    }

    public void SaveWeapons()
    {
        wc = WeaponControls.instance;
        
        if (wc.activeWeapon)
        {
            activeWeapon = wc.activeWeapon.weapon;
            activeWeaponClip = wc.activeWeapon.ammoClip;
            activeWeaponDurability = wc.activeWeapon.durability;
            activeWeaponDescriptions = new List<string>(wc.activeWeapon.descriptions);

            wc.activeWeapon.dataRandomizer.SendInfo(false);
            //activeWeaponInfo = new Vector4(wc.activeWeapon.dataRandomizer.r1, wc.activeWeapon.dataRandomizer.r2,wc.activeWeapon.dataRandomizer.r3,wc.activeWeapon.dataRandomizer.r4);
            
            if (wc.activeWeapon.weaponConnector)
            {
                activeWeaponClipSize = wc.activeWeapon.ammoClipMax;
                activeWeaponSavedBarrels_1 = new List<int>(wc.activeWeapon.weaponConnector.savedBarrelslvl_1);
                activeWeaponSavedBarrels_2 = new List<int>(wc.activeWeapon.weaponConnector.savedBarrelslvl_2);
                activeWeaponRotation = wc.activeWeapon.weaponConnector.transform.localRotation.eulerAngles;
                activeWeaponCrazyGun = wc.activeWeapon.weaponConnector.crazyGun;
            }
            else
            {
                activeWeaponClipSize = 0;
                activeWeaponSavedBarrels_1.Clear();
                activeWeaponSavedBarrels_2.Clear();
            }
        }
        else
            activeWeapon = WeaponPickUp.Weapon.Null;

        if (wc.secondWeapon)
        {
            secondWeapon = wc.secondWeapon.weapon;
            secondWeaponClip = wc.secondWeapon.ammoClip;
            secondWeaponDurability = wc.secondWeapon.durability;
            secondWeaponDescriptions = new List<string>(wc.secondWeapon.descriptions);
            
            wc.secondWeapon.dataRandomizer.SendInfo(true);
            //secondWeaponInfo = new Vector4(wc.secondWeapon.dataRandomizer.r1, wc.secondWeapon.dataRandomizer.r2,wc.secondWeapon.dataRandomizer.r3,wc.secondWeapon.dataRandomizer.r4);
            
            if (wc.secondWeapon.weaponConnector)
            {
                secondWeaponClipSize = wc.secondWeapon.ammoClipMax;
                secondWeaponSavedBarrels_1 = new List<int>(wc.secondWeapon.weaponConnector.savedBarrelslvl_1);
                secondWeaponSavedBarrels_2 = new List<int>(wc.secondWeapon.weaponConnector.savedBarrelslvl_2);
                secondWeaponRotation = wc.secondWeapon.weaponConnector.transform.localRotation.eulerAngles;
                secondWeaponCrazyGun = wc.secondWeapon.weaponConnector.crazyGun;
            }
            else
            {
                secondWeaponClipSize = 0;
                secondWeaponSavedBarrels_1.Clear();
                secondWeaponSavedBarrels_2.Clear();
            }
        }
        else
            secondWeapon = WeaponPickUp.Weapon.Null;
        
        UpdateCurrentWeight();
    }

    public void SomethingStolen()
    {
        // choose item to steal
        float r = Random.value;
        // item
        if (r <= 0.25f)
        {
            for (int i = 0; i < savedTools.Count; i++)
            {
                if (savedTools[i].amount > 0)
                {
                    savedTools[i].amount--;
                    wc.FindNewTool();
                    break;
                }
            }
        }
        else if (r <= 0.5f)
        {
            if (wc.activeWeapon != null)
                wc.RemoveWeapon(0);
            else if (wc.secondWeapon != null)
                wc.RemoveWeapon(1);
        }
        else if (r <= 0.75f)
        {
            if (savedSkills.Count > 0)
            {
                var randomSkill = Random.Range(0, savedSkills.Count);
                psc.RemoveSkill(savedSkills[randomSkill]);
                //savedSkills.RemoveAt(randomSkill);
            }
        }
        else
        {
            if (gold > 0)
            {
                LoseGold(Random.Range(1, 10));
            }
        }
    }

    public void UpdateSkills()
    {
        savedSkills = new List<SkillInfo>(psc.playerSkills);
    }
    
    public void LostAnEye()
    {
        lostAnEye = 1;
    }

    public void LoseCult()
    {
        activeCult = PlayerSkillsController.Cult.none;
    }

    public void LoseGold(float lose)
    {
        gold -= lose;
        if (gold < 0) gold = 0;

        if(!ui)
            ui = UiManager.instance;
        
        ui.UpdateGold(gold);
    }
    public void GetGold(float get)
    {
        gold += get;
        if (gold < 0) gold = 0;
        
        goldOnRunCurrent += Mathf.RoundToInt(get);
        //gm.goldOnRun = goldOnRunCurrent;
        
        gold = Mathf.RoundToInt(gold);

        if(!ui)
            ui = UiManager.instance;
        
        pm.hc.PlayerGetGold();
        ui.UpdateGold(gold);
    }

    public void UnlockNewCassette(NoteTextGenerator cassetteTextGenerator)
    {
        var cassettesListTemp = new List<int>(tracksOverall);
        
        for (int i = 0; i < unlockedTracks.Count; i++)
        {
            cassettesListTemp.Remove(unlockedTracks[i]);
        }

        string cassetteName = "void";
        
        if (cassettesListTemp.Count > 0)
        {
            int r = Random.Range(0, cassettesListTemp.Count);
            
            unlockedTracks.Add(r);

            gm = GameManager.instance;
            
            if (gm.language == 0)
                cassetteName = cassetteNames.lines[r].engLines[0];
            else if (gm.language == 1)
                cassetteName = cassetteNames.lines[r].rusLines[0];
            else if (gm.language == 2)
                cassetteName = cassetteNames.lines[r].espLines[0];
            else if (gm.language == 3)
                cassetteName = cassetteNames.lines[r].gerLines[0];
            else if (gm.language == 4)
                cassetteName = cassetteNames.lines[r].itaLines[0];
            else if (gm.language == 5)
                cassetteName = cassetteNames.lines[r].spbrLines[0];
        }
        
        if (cassetteTextGenerator)
            cassetteTextGenerator.SetCassetteTapeName(cassetteName);
        
        gm.SaveGame();
    }
    
    public bool PlayerIsArmed()
    {
        bool hasWeapon = false;
        WeaponControls wc = WeaponControls.instance;

        if (activeWeapon != WeaponPickUp.Weapon.Null && activeWeaponDurability > 500)
            hasWeapon = true;
        
        if (secondWeapon != WeaponPickUp.Weapon.Null && secondWeaponDurability > 500)
            hasWeapon = true;

        
        return hasWeapon;
    }

    public void PlayerDie()
    {
        // after player die
        gm.SaveNewLevelsOnRun(levelsOnRunCurrent);
        gm.SaveNewGoldOnRun(goldOnRunCurrent);
        
        levelsOnRunCurrent = 0;
        goldOnRunCurrent = 0;
    }
}

[Serializable]
public class Tool
{
    public ToolController toolController;
    public ToolController.ToolType type = ToolController.ToolType.Heal;
    public int amount = 0;
    public int maxAmount = 2;
    
    public bool known = false;
    [Header("0 - eng; 1-  rus")]
    public List<string> info = new List<string>();
    public List<string> unknownInfo = new List<string>();
    
    public List<string> useMessage = new List<string>();
    public List<string> throwMessage = new List<string>();
    public List<string> effectHint = new List<string>();

}

[Serializable]
public class QuestItem
{
    public List<string> descriptions = new List<string>();
    public Sprite sprite;
}
