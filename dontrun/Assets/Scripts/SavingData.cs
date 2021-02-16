using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class SavingData
{
    public int heartContainerAmount;
    
    public List<int> goldenKeysFoundOnFloors = new List<int>();
    public List<int> goldenKeysFoundInHub = new List<int>();
    public int goldenKeysAmount;
    public int playersGold;
    public float badReputaion;
    public int invertMouse = 0;
    public int tutorialPassed = 0;
    public int language = 0;
    public float volumeSliderValue = 0.5f;
    public float brightness = 0.25f;
    public float contrast = 30f;
    public float mouseSense = 2.5f;
    public float camSpeed = 8f;
    public int wasInHub = 0;
    public int checkpoints = 0;
    public int hubVisits = 0;
    public int resolution = 0;
    public int difficultyLevel = 0;
    public int goldSpentOnSon = 0;
    public int bloodMist;
    public int lastTalkedDyk = -1;
    public bool cagesCompleted = false;
    public bool ladyshoeFound = false;
    public bool revolverFound = false;
    public bool newGamePlus = false;
    
    public int grain;
    public int bloom;
    public int pixels;
    public int edgeDetection;
    public int doubleVision;
    public int dithering;
    
    
    public int lowPitchDamage;
    public List<int> activeQuests = new List<int>();
    public List<int> completedQuests = new List<int>();
    public List<int> savedQuestItems = new List<int>();
    //public List<int> bossesKilled = new List<int>();
    public int savedOnFloor = 0; // 0 is false
    public int playerFloor = 0;
    public int bossFloor = 3;
    public int currentLevelDifficulty = 0;
    public List<int> appliedHerPieces;
    public List<int> foundHerPiecesOnFloors;
    public List<int> checkpointsOnFloors = new List<int>();
    public List<int> heartControllersTakenIndexesInHub = new List<int>();
    public List<int> roseNpcsInteractedInHub = new List<int>();
    public List<int> cutscenesInteractedInHub = new List<int>();
    public List<int> doorsOpenedIndexesInHub = new List<int>();
    public List<int> unlockedTracks = new List<int>();
    public List<int> tapesFoundOfFloors = new List<int>();
    
    public int coopBiomeCheckpoints = 0;
    public int notesPickedUp = 0;
    
    public int savedWeaponActive = -1;
    public int savedWeaponSecond = -1;
    public List<int> savedSkills = new List<int>();
    public List<int> toolsAmount = new List<int>();
    
    public int rareWindowShown = 0;
    // 1 is false
    public int tutorialHints;

    // 0 is false
    public int darkness;
    
    // active NPCs
    public int currentActiveNpcGoalFirst = 0;
    public int currentActiveNpcGoalSecond = 0;
    public int currentActiveNpcName1 = 0;
    public int currentActiveNpcName2 = 0;
    public int currentActiveNpcName3 = 0;
    
    // stats
    public int levelsOnRun = 0;
    public int goldOnRun = 0;
    public int overallKills = 0;
    public int overallDeaths = 0;

    public int snoutFound;
    
    public int hubActiveCheckpointIndex;
    
    // player inventory
    public List<int> activeWeaponSavedBarrels_1;
    public List<int> activeWeaponSavedBarrels_2;
    public float activeWeaponClipSize = 0;
    public bool activeWeaponNpc = false;
    public bool activeWeaponCrazyGun = false;
    public float activeWeaponDurability = 0;
    public List<int> activeWeaponInfo;
    
    public List<int> secondWeaponSavedBarrels_1;
    public List<int> secondWeaponSavedBarrels_2;
    public bool secondWeaponCrazyGun = false;
    public float secondWeaponClipSize = 0;
    public bool secondWeaponNpc = false;
    public List<int> secondWeaponInfo;
    
    public float secondWeaponDurability = 0;
    public float activeWeaponClip = 0;
    public float secondWeaponClip = 0;

    public int pistolAmmo = 0;
    public int revolverAmmo = 0;
    public int shotgunAmmo = 0;
    public int tommyAmmo = 0;
    public int blunderbussAmmo = 0;

    public SavingData (GameManager gm, ItemsList il)
    {
        SavePlayerInventory();
        
        heartContainerAmount = il.heartContainerAmount;
        language = gm.language;
        brightness = gm.brightness;
        contrast = gm.contrast;
        camSpeed = gm.mouseLookSpeed;
        bloodMist = gm.bloodMist;
        mouseSense = gm.mouseSensitivity;
        cagesCompleted = gm.cagesCompleted;
        revolverFound = gm.revolverFound;
        ladyshoeFound = gm.ladyshoeFound;
        lastTalkedDyk = gm.lastTalkedDyk;
        playersGold = Mathf.RoundToInt(il.gold);
        invertMouse = gm.mouseInvert;
        badReputaion = il.badReputaion;
        goldSpentOnSon = il.goldSpentOnSon;
        volumeSliderValue = gm.volumeSliderValue;
        unlockedTracks = il.unlockedTracks;
        goldenKeysAmount = il.goldenKeysAmount;
        savedOnFloor = gm.savedOnFloor;
        goldenKeysFoundOnFloors = gm.goldenKeysFoundOnFloors;
        goldenKeysFoundInHub = gm.goldenKeysFoundInHub;
        tapesFoundOfFloors = gm.tapesFoundOfFloors;
        doorsOpenedIndexesInHub = gm.doorsOpenedIndexesInHub;
        rareWindowShown = gm.rareWindowShown;
        tutorialPassed = gm.tutorialPassed;
        tutorialHints = gm.tutorialHints;
        darkness = gm.darkness;
        coopBiomeCheckpoints = gm.coopBiomeCheckpoints;
        notesPickedUp = gm.notesPickedUp;
        
        grain = gm.grain;
        bloom = gm.bloom;
        doubleVision = gm.doubleVision;
        dithering = gm.dithering;
        pixels = gm.pixels;
        edgeDetection = gm.edgeDetection;

        newGamePlus = gm.newGamePlus;
        
        wasInHub = gm.wasInHub;
        hubVisits = gm.hubVisits;
        resolution = gm.resolution;
        snoutFound = gm.snoutFound;
        hubActiveCheckpointIndex = GameManager.instance.hubActiveCheckpointIndex;
        activeQuests = new List<int>(QuestManager.instance.activeQuestsIndexes);
        completedQuests = new List<int>(QuestManager.instance.completedQuestsIndexes);
        savedQuestItems = new List<int>(ItemsList.instance.savedQuestItems);
        heartControllersTakenIndexesInHub = new List<int>(GameManager.instance.permanentPickupsTakenIndexesInHub);
        roseNpcsInteractedInHub = new List<int>(GameManager.instance.roseNpcsInteractedInHub);
        cutscenesInteractedInHub = new List<int>(GameManager.instance.cutscenesInteractedInHub);

        levelsOnRun = gm.levelsOnRun;
        goldOnRun = gm.goldOnRun;
        overallDeaths = gm.overallDeaths;
        overallKills = gm.overallKills;

        if (gm.difficultyLevel == GameManager.GameMode.StickyMeat)
            difficultyLevel = 0;
        else if (gm.difficultyLevel == GameManager.GameMode.Ikarus)
            difficultyLevel = 1;
        else if (gm.difficultyLevel == GameManager.GameMode.MeatZone)
            difficultyLevel = 2;
        
        var gpm = GutProgressionManager.instance;
        //bossesKilled = new List<int>(gpm.bossesKilled);
        checkpoints = gpm.checkpoints;
        checkpointsOnFloors = new List<int>(gpm.checkpointsOnFloors);
        playerFloor = gpm.playerFloor;
        bossFloor = gpm.bossFloor;
        currentLevelDifficulty = gpm.currentLevelDifficulty;
        if (il.foundHerPiecesOnFloors.Count > 0)
            foundHerPiecesOnFloors = new List<int>(il.foundHerPiecesOnFloors);
        else
            foundHerPiecesOnFloors = new List<int>();
        
        if (il.herPiecesApplied.Count > 0)
            appliedHerPieces = new List<int>(il.herPiecesApplied);
        else
            appliedHerPieces = new List<int>();

        var anm = ActiveNpcManager.instance;
        currentActiveNpcGoalFirst = (int)anm.currentNpc.npcGoal1st;
        currentActiveNpcGoalSecond = (int)anm.currentNpc.npcGoal2nd;
        currentActiveNpcName1 = anm.currentNpc.name1;
        currentActiveNpcName2 = anm.currentNpc.name2;
        currentActiveNpcName3 = anm.currentNpc.name3;

        // save ammo
        for (int i = 0; i < il.ammoDataStorage.allAmmo.Length; i++)
        {
            if (il.ammoDataStorage.allAmmo[i] == null) continue;

            switch (il.ammoDataStorage.allAmmo[i].weaponType)
            {
                case WeaponPickUp.Weapon.Pistol:
                    pistolAmmo = Mathf.RoundToInt(il.ammoDataStorage.allAmmo[i].count + il.GetAmmoInGun(WeaponPickUp.Weapon.Pistol));
                    break;
                case WeaponPickUp.Weapon.Revolver:
                    revolverAmmo = Mathf.RoundToInt(il.ammoDataStorage.allAmmo[i].count + il.GetAmmoInGun(WeaponPickUp.Weapon.Revolver));
                    break;
                case WeaponPickUp.Weapon.Shotgun:
                    shotgunAmmo = Mathf.RoundToInt(il.ammoDataStorage.allAmmo[i].count + il.GetAmmoInGun(WeaponPickUp.Weapon.Shotgun));
                    break;
                case WeaponPickUp.Weapon.TommyGun:
                    tommyAmmo = Mathf.RoundToInt(il.ammoDataStorage.allAmmo[i].count + il.GetAmmoInGun(WeaponPickUp.Weapon.TommyGun));
                    break;
                case WeaponPickUp.Weapon.OldPistol:
                    blunderbussAmmo = Mathf.RoundToInt(il.ammoDataStorage.allAmmo[i].count + il.GetAmmoInGun(WeaponPickUp.Weapon.OldPistol));
                    break;
            }
        }
    }

    void SavePlayerInventory()
    {
        savedSkills.Clear();
        toolsAmount.Clear();
        
        var il = ItemsList.instance;
        
        #region SaveWeapons
        for (int i = 0; i < 2; i++)
        {
            var weapon = WeaponPickUp.Weapon.Null;
            int index = -1;

            if (i == 0) weapon = il.activeWeapon;
            else weapon = il.secondWeapon;

            switch (weapon)
            {
                case WeaponPickUp.Weapon.Axe:
                    index = 0;
                    break;
                case WeaponPickUp.Weapon.Pistol:
                    index = 1;
                    break;
                case WeaponPickUp.Weapon.Revolver:
                    index = 2;
                    break;
                case WeaponPickUp.Weapon.Shotgun:
                    index = 3;
                    break;
                case WeaponPickUp.Weapon.TommyGun:
                    index = 4;
                    break;
                case WeaponPickUp.Weapon.Pipe:
                    index = 5;
                    break;
                case WeaponPickUp.Weapon.Map:
                    index = 6;
                    break;
                case WeaponPickUp.Weapon.Knife:
                    index = 7;
                    break;
                case WeaponPickUp.Weapon.Torch:
                    index = 8;
                    break;
                case WeaponPickUp.Weapon.OldPistol:
                    index = 9;
                    break;
                case WeaponPickUp.Weapon.Polaroid:
                    index = 10;
                    break;
                case WeaponPickUp.Weapon.Shield:
                    index = 11;
                    break;
                case WeaponPickUp.Weapon.MeatSpear:
                    index = 12;
                    break;
                case WeaponPickUp.Weapon.VeinWhip:
                    index = 13;
                    break;
                
                default:
                    index = -1;
                    break;
            }

            if (i == 0)
                savedWeaponActive = index;
            else
                savedWeaponSecond = index;
        }
        activeWeaponSavedBarrels_1 = il.activeWeaponSavedBarrels_1;
        activeWeaponSavedBarrels_2 = il.activeWeaponSavedBarrels_2;
        activeWeaponClipSize = il.activeWeaponClipSize;
        activeWeaponNpc = il.activeWeaponNpc;
        activeWeaponInfo = il.activeWeaponInfo;
        activeWeaponCrazyGun = il.activeWeaponCrazyGun;
        activeWeaponDurability = il.activeWeaponDurability;
        
        secondWeaponSavedBarrels_1 = il.secondWeaponSavedBarrels_1;
        secondWeaponSavedBarrels_2 = il.secondWeaponSavedBarrels_2;
        secondWeaponClipSize = il.secondWeaponClipSize;
        secondWeaponNpc = il.secondWeaponNpc;
        secondWeaponInfo = il.secondWeaponInfo;
        secondWeaponCrazyGun = il.secondWeaponCrazyGun;
        secondWeaponDurability = il.secondWeaponDurability;
        #endregion SaveWeapons
        
        #region SaveSkills
            // iterate through saved skills and get their indexes
            for (int i = 0; i < il.savedSkills.Count; i++)
            {
                if (!savedSkills.Contains(il.savedSkills[i].skillIndex))
                    savedSkills.Add(il.savedSkills[i].skillIndex);
            }
            #endregion
            
        #region SaveTools
            for (int i = 0; i < il.savedTools.Count; i++)
            {
                toolsAmount.Add(il.savedTools[i].amount);
            }
        #endregion
    }
}