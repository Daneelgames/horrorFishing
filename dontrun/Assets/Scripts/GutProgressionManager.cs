using System;
using System.Collections;
using System.Collections.Generic;
using ExtendedItemSpawn;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class GutProgressionManager : MonoBehaviour
{
    public static GutProgressionManager instance;
    public LevelData levelData;
    public int playerFloor = 0;
    public int bossFloor = 3;
    public int currentLevelDifficulty = 0;
    public int checkpoints = 0;

    [Header("Content")] 
    public RoomTilesPositionsManager bossElevatorRoom;
    public RoomTilesPositionsManager barElevatorRoom;
    public List<RoomTilesPositionsManager> elevatorRooms;
    [Tooltip("For each tile index")] public List<PropsPool> coridorPropsPools;
    public List<RoomsPool> roomsPools;
    public List<UnitsPrefabsPool> keyNpcPool;
    public List<UnitsPrefabsPool> trapDoorsPool;
    public List<MobsPool> mobsPools;
    public List<int> bossLevels = new List<int>();
    //public List<int> bossesKilled = new List<int>();
    public List<ItemsOnlevel> itemsOnlevels;

    private GameManager gm;

    [Header("Loading progression UI")] public GameObject playerUi;
    public GameObject bossUi;
    public Image playerIcon;
    public List<Sprite> playerSprites;
    public TextMeshProUGUI levelNumber;
    public List<Image> levelDots;

    public int levelsWithoutShow = 0;
    
     public List<int> checkpointsOnFloors = new List<int>();

    void Awake()
    {
        instance = this;
        levelsWithoutShow = 0;
    }

    public void Init()
    {
        // both coop and host
        //generate next floor
        gm = GameManager.instance;
        UpdateProgression();
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        // main tile in level
        var ambientIndex = currentLevelDifficulty + 1;
        
        levelData.goldToSpawn = 10 + 1 * currentLevelDifficulty;
        levelData.goldToSpawn = Mathf.RoundToInt(levelData.goldToSpawn * ItemsList.instance.badReputaion);
        // aesthetics
        levelData.ambientIndex = ambientIndex;
        levelData.prisonersAmount = 0;

        var playerHandicap = bossFloor - playerFloor;

        // CHOOSE TILES
        ChooseTiles();

        // LOAD PROPS
        levelData.propsAdressables.Clear();
        levelData.propsAdressables =
            new List<AssetReference>(coridorPropsPools[levelData.mainTileIndex].propsReferences);
        levelData.roomsPropsReferences =
            new List<AssetReference>(coridorPropsPools[levelData.roomTileIndex].propsReferences);
        

        levelData.roomTilesPositionsManagers.Clear();
        List<RoomTilesPositionsManager> roomsTemp;

        if (bossFloor - playerFloor >= 2)
            roomsTemp = new List<RoomTilesPositionsManager>(roomsPools[levelData.mainTileIndex].easyRooms);
        else if (bossFloor - playerFloor == 1)
            roomsTemp = new List<RoomTilesPositionsManager>(roomsPools[levelData.mainTileIndex].mediumRooms);
        else
            roomsTemp = new List<RoomTilesPositionsManager>(roomsPools[levelData.mainTileIndex].hardRooms);

        levelData.roomTilesPositionsManagers = new List<RoomTilesPositionsManager>(roomsTemp);

        levelData.lightRate = 0.1f;

        // SPAWN
        if (levelData.currentBoss >= 0)
        {
            levelData.startRoomTilesPositionsManager = bossElevatorRoom;  
        }
        else
        {
            if (levelsWithoutShow < 4 || playerFloor <= 4)
            {
                levelData.startRoomTilesPositionsManager = elevatorRooms[levelData.mainTileIndex];
            }
            else if (Random.value > 0.5f)
            {
                levelData.startRoomTilesPositionsManager = barElevatorRoom;
                levelsWithoutShow = 0;
            }
            else
            {
                levelData.startRoomTilesPositionsManager = elevatorRooms[levelData.mainTileIndex];   
            }   
        }
        
        if (playerFloor > bossFloor) // player is above
        {
            bossFloor = playerFloor + 2;
            levelData.startRoomTilesPositionsManager = elevatorRooms[levelData.mainTileIndex];   
            levelData.currentBoss = -1;
        }

        if (gm.tutorialPassed == 1)
            levelData.lockedDoorsAmount =
                Mathf.Clamp(Random.Range(1, 4) + currentLevelDifficulty - playerHandicap, 0, 10);
        else
            levelData.lockedDoorsAmount = 0;
        
        //levelData.lockpicksAmount = Random.Range(0, playerHandicap + 1);
        //levelData.regenTilesAmount = 1;
        levelData.vendingMachinesAmount = 1;
        if (levelData.mainTileIndex == 3 || levelData.roomTileIndex == 3) levelData.bonefiresAmount = playerHandicap;
        else levelData.bonefiresAmount = 0;

        levelData.notesAmount = 3;
        levelData.noteHintsAmount = 3;


        levelData.meatTrapsAmount = 1 + currentLevelDifficulty;



        // mobs
        levelData.mobsLevel = currentLevelDifficulty;
        levelData.ceilingTrapsMax = Mathf.Clamp(currentLevelDifficulty * 3 - playerHandicap, 0, 15);



        if (playerFloor == bossFloor)
        {
            levelData.npcAmount = levelData.npcsPool.Count - 1;
        }


        // spawn ammo and weapon???
        levelData.spawnGroups = itemsOnlevels[levelData.mainTileIndex].spawnGroups;
        levelData.ammoSpawn = itemsOnlevels[levelData.mainTileIndex].ammoSpawn;
    }

    public void SetFloorToClient(int floor, int _bossFloor)
    {
        print("SET PLAYER FLOOR " + floor + ". Choose rooms rtps");
        gm.tutorialPassed = 1;
        playerFloor = floor;
        bossFloor = _bossFloor;

        switch (playerFloor)
        {
            case 1:
                currentLevelDifficulty = 0;
                break;
            case 4:
                currentLevelDifficulty = 1;
                break;
            case 7:
                currentLevelDifficulty = 2;
                break;
            case 10:
                currentLevelDifficulty = 3;
                break;
            case 13:
                currentLevelDifficulty = 4;
                break;
            case 16:
                currentLevelDifficulty = 5;
                break;
        }
        
        List<RoomTilesPositionsManager> roomsTemp;
        if (bossFloor - playerFloor >= 2)
        {
            roomsTemp = new List<RoomTilesPositionsManager>(roomsPools[levelData.mainTileIndex].easyRooms);   
        }
        else if (bossFloor - playerFloor == 1)
        {
            roomsTemp = new List<RoomTilesPositionsManager>(roomsPools[levelData.mainTileIndex].mediumRooms);   
        }
        else
        {
            roomsTemp = new List<RoomTilesPositionsManager>(roomsPools[levelData.mainTileIndex].hardRooms);   
        }
        
        levelData.roomTilesPositionsManagers = new List<RoomTilesPositionsManager>(roomsTemp);
        ChooseTiles();
    }

    // this method is actualy the main setup for the levels
    void ChooseTiles()
    {
        print("Choose tiles, level " + playerFloor);
        
        switch (playerFloor)
        {
            // tutorial
            case 0:
                levelData.fogDistance = 40;
                levelData.mainTileIndex = 0;
                levelData.roomTileIndex = 0;
                
                //mobs
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].easy;
                levelData.eyeEatersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 0;
                levelData.wallBlockersMax = 0;
                levelData.prisonersAmount = 0;
                levelData.wallTrapsMax = 0;
                levelData.faceEaterAmount = 0;
            
                //guns
                levelData.deadWeaponRate = 0.9f;
                levelData.npcWeaponRate = 0.1f;

                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 0;
                levelData.padlocksAmount = 2;
                levelData.keysToSpawn = 2;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                levelData.sheAmount = 0;

            
                levelData.npcAmount = 0;
                levelData.meatHoles = 0;
                levelData.mainItemsInMeatHoles = 0;
                
                levelData.propsRate = Random.Range(0.7f, 1);
                levelData.roomPropsRate = Random.Range(0.7f, 1);
                levelData.mainLightColor = Random.Range(0,6);

                levelData.maximumHorizontalRoomPos = 8;
                levelData.maximumVerticalRoomPos = 8;
                levelData.steps = 5;
                levelData.width = 10;
                levelData.lenght = 10;
                
                levelData.trapDoorsAmount = 0;
                levelData.maximumMobsAttackingPlayer = 1;
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 180;
                
                levelData.roomsAmount = 3;
                
                levelData.itemsMimicsAmount = 0;
                // choose boss
                levelData.currentBoss = -1;
                break;

            // office
            case 1:
                levelData.mainTileIndex = 1;
                levelData.roomTileIndex = 0;
                
                levelData.fogDistance = 35;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].easy;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 1;
                levelData.wallBlockersMax = 0;
                levelData.prisonersAmount = 0;
                levelData.wallTrapsMax = 0;
                levelData.faceEaterAmount = 0;
                levelData.npcAmount = 1;
            
                //guns
                levelData.deadWeaponRate = 0.9f;
                levelData.npcWeaponRate = 0.1f;
                
                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 2;
                levelData.keysToSpawn = 2;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 0;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 0;
                levelData.mainItemsInMeatHoles = 0;
                
                levelData.propsRate = 1;
                levelData.roomPropsRate = 1;
                levelData.mainLightColor = 1;
                
                levelData.maximumHorizontalRoomPos = 7;
                levelData.maximumVerticalRoomPos = 7;
                levelData.steps = 5;
                levelData.width = 10;
                levelData.lenght = 10;
                
                levelData.trapDoorsAmount = 0;

                levelData.maximumMobsAttackingPlayer = 2;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 150;
                
                levelData.roomsAmount = 3;
                
                levelData.itemsMimicsAmount = 0;
                // choose boss
                levelData.currentBoss = -1;
                break;
            
            case 2:
                levelData.mainTileIndex = 0;
                levelData.roomTileIndex = 1;
                
                levelData.fogDistance = 30;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 5;
                levelData.wallBlockersMax = 1;
                levelData.prisonersAmount = 0;
                levelData.wallTrapsMax = 0;
                levelData.faceEaterAmount = 0;
            
                levelData.npcAmount = 1;
                
                //guns
                levelData.deadWeaponRate = 0.75f;
                levelData.npcWeaponRate = 0.2f;
                
                // keys
                levelData.tapesToSpawn = 1;
                levelData.goldenKeysToSpawn = 1;
                levelData.padlocksAmount = 3;
                levelData.keysToSpawn = 3;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 1;
                levelData.mainItemsInMeatHoles = 1;
                
                levelData.propsRate = Random.Range(0.7f, 1);
                levelData.roomPropsRate = Random.Range(0.7f, 1);
                levelData.mainLightColor = 0;
                
                levelData.maximumHorizontalRoomPos = 10;
                levelData.maximumVerticalRoomPos = 7;
                levelData.steps = 70;
                levelData.width = 12;
                levelData.lenght = 9;
                
                levelData.trapDoorsAmount = 2;
                levelData.maximumMobsAttackingPlayer = 3;
                
                levelData.mobsAmount = 2;
                levelData.mobSpawnDelay = 160;
                
                levelData.roomsAmount = 4;
                
                levelData.itemsMimicsAmount = 1;
                // choose boss
                levelData.currentBoss = -1;
                break;
            
            case 3: // carre boss
                levelData.mainTileIndex = 1;
                levelData.roomTileIndex = 1;
                
                levelData.fogDistance = 30;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 10;
                levelData.wallBlockersMax = 2;
                levelData.prisonersAmount = 0;
                levelData.wallTrapsMax = 0;
                levelData.faceEaterAmount = 0;
            
                //guns
                levelData.deadWeaponRate = 0.7f;
                levelData.npcWeaponRate = 0.3f;
                
                // keys
                levelData.tapesToSpawn = 1;
                levelData.goldenKeysToSpawn = 1;
                levelData.padlocksAmount = 0;
                levelData.keysToSpawn = 0;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 0;
                levelData.mainItemsInMeatHoles = 0;
                
                levelData.propsRate = Random.Range(0.7f, 1);
                levelData.roomPropsRate = Random.Range(0.7f, 1);
                levelData.mainLightColor = 2;
                
                levelData.maximumHorizontalRoomPos = 5;
                levelData.maximumVerticalRoomPos = 5;
                levelData.steps = 5;
                levelData.width = 8;
                levelData.lenght = 8;
                
                levelData.trapDoorsAmount = 1;
                levelData.maximumMobsAttackingPlayer = 1;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 180;
                
                levelData.roomsAmount = 2;
                
                levelData.itemsMimicsAmount = 0;
                // choose boss
                levelData.currentBoss = levelData.mainTileIndex;
                break;

            // factory
            case 4:
                levelData.mainTileIndex = 9;
                levelData.roomTileIndex = 10;
                
                levelData.ambientIndex = 9;
                
                levelData.fogDistance = 30;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].easy;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 3;
                levelData.wallBlockersMax = 3;
                levelData.prisonersAmount = 2;
                levelData.goldToSpawn = 0;
                levelData.wallTrapsMax = 0;
                levelData.faceEaterAmount = 0;
                levelData.npcAmount = 1;
            
                //guns
                levelData.deadWeaponRate = 0.9f;
                levelData.npcWeaponRate = 0.1f;
                
                // keys
                levelData.tapesToSpawn = 1;
                levelData.goldenKeysToSpawn = 0;
                levelData.padlocksAmount = 2;
                levelData.keysToSpawn = 1;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 1;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 1;
                levelData.mainItemsInMeatHoles = 0;
                
                levelData.propsRate = 1;
                levelData.roomPropsRate = 1;
                levelData.mainLightColor = 1;
                
                levelData.maximumHorizontalRoomPos = 5;
                levelData.maximumVerticalRoomPos = 3;
                levelData.steps = 15;
                levelData.width = 10;
                levelData.lenght = 5;
                
                levelData.trapDoorsAmount = 0;

                levelData.maximumMobsAttackingPlayer = 2;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 240;
                
                levelData.roomsAmount = 4;
                
                levelData.itemsMimicsAmount = 0;
                // choose boss
                levelData.currentBoss = -1;
                break;
            
            case 5:
                levelData.mainTileIndex = 10;
                levelData.roomTileIndex = 9;
                levelData.ambientIndex = 9;
                
                levelData.fogDistance = 25;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 5;
                levelData.wallBlockersMax = 3;
                levelData.prisonersAmount = 4;
                levelData.goldToSpawn = 0;
                levelData.wallTrapsMax = 3;
                levelData.faceEaterAmount = 1;
                levelData.npcAmount = 1;
            
                //guns
                levelData.deadWeaponRate = 0.75f;
                levelData.npcWeaponRate = 0.2f;
                
                // keys
                levelData.goldenKeysToSpawn = 1;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 3;
                levelData.keysToSpawn = 2;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 1;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 1;
                levelData.mainItemsInMeatHoles = 1;
                
                levelData.propsRate = Random.Range(0.7f, 1);
                levelData.roomPropsRate = Random.Range(0.7f, 1);
                levelData.mainLightColor = 0;
                
                levelData.maximumHorizontalRoomPos = 7;
                levelData.maximumVerticalRoomPos = 7;
                levelData.steps = 10;
                levelData.width = 11;
                levelData.lenght = 10;
                
                levelData.trapDoorsAmount = 0;
                levelData.maximumMobsAttackingPlayer = 3;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 200;
                
                levelData.roomsAmount = 5;
                
                levelData.itemsMimicsAmount = 1;
                // choose boss
                levelData.currentBoss = -1;
                break;
            
            case 6: //  boss
                levelData.mainTileIndex = 9;
                levelData.roomTileIndex = 9;
                levelData.ambientIndex = 9;
                
                levelData.fogDistance = 17.5f;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 10;
                levelData.wallBlockersMax = 6;
                levelData.prisonersAmount = 9;
                levelData.goldToSpawn = 0;
                levelData.wallTrapsMax = 0;
                levelData.faceEaterAmount = 0;
            
                //guns
                levelData.deadWeaponRate = 0.7f;
                levelData.npcWeaponRate = 0.3f;
                
                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 0;
                levelData.keysToSpawn = 0;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 0;
                levelData.mainItemsInMeatHoles = 0;
                
                levelData.propsRate = Random.Range(0.7f, 1);
                levelData.roomPropsRate = Random.Range(0.7f, 1);
                levelData.mainLightColor = 2;
                
                levelData.maximumHorizontalRoomPos = 10;
                levelData.maximumVerticalRoomPos = 8;
                levelData.steps = 15;
                levelData.width = 12;
                levelData.lenght = 10;
                
                levelData.trapDoorsAmount = 0;
                levelData.maximumMobsAttackingPlayer = 1;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 360;
                
                levelData.roomsAmount = 3;
                
                levelData.itemsMimicsAmount = 0;
                // choose boss
                levelData.currentBoss = levelData.mainTileIndex;
                break;

            case 7: // cages
                levelData.mainTileIndex = 2;
                levelData.roomTileIndex = 0;
                levelData.fogDistance = 45;
                
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].easy;
                levelData.eyeEatersAmount = 1;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 5;
                levelData.wallBlockersMax = 3;
                levelData.wallTrapsMax = 3;
                levelData.faceEaterAmount = 1;
                levelData.npcAmount = 1;
            
                //guns
                levelData.deadWeaponRate = 0.6f;
                levelData.npcWeaponRate = 0.3f;
                
                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 3;
                levelData.keysToSpawn = 3;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 2;
                levelData.mainItemsInMeatHoles = 1;
                
                levelData.propsRate = Random.Range(0.7f, 1);
                levelData.roomPropsRate = Random.Range(0.7f, 1);
                levelData.mainLightColor = Random.Range(0,6);
                
                levelData.maximumHorizontalRoomPos = 12;
                levelData.maximumVerticalRoomPos = 7;
                levelData.steps = 5;
                levelData.width = 14;
                levelData.lenght = 10;
                
                levelData.trapDoorsAmount = 0;
                levelData.maximumMobsAttackingPlayer = 2;
                
                levelData.mobsAmount = 2;
                levelData.mobSpawnDelay = 220;
                
                levelData.roomsAmount = 4;
                
                levelData.itemsMimicsAmount = 0;
                // choose boss
                levelData.currentBoss = -1;
                break;
            
            case 8:
                levelData.mainTileIndex = 2;
                levelData.roomTileIndex = 1;
                levelData.fogDistance = 35;
                
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].hard;
                levelData.eyeEatersAmount = 2;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 8;
                levelData.wallBlockersMax = 6;
                levelData.wallTrapsMax = 4;
                levelData.faceEaterAmount = 0;
                levelData.npcAmount = 1;
            
                //guns
                levelData.deadWeaponRate = 0.6f;
                levelData.npcWeaponRate = 0.3f;
                
                // keys
                levelData.goldenKeysToSpawn = 1;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 4;
                levelData.keysToSpawn = 2;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 2;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabEye;
                
                levelData.meatHoles = 2;
                levelData.mainItemsInMeatHoles = 1;
                
                levelData.propsRate = Random.Range(0.7f, 1);
                levelData.roomPropsRate = Random.Range(0.7f, 1);
                levelData.mainLightColor = 2;
                
                levelData.maximumHorizontalRoomPos = 13;
                levelData.maximumVerticalRoomPos = 10;
                levelData.steps = 10;
                levelData.width = 15;
                levelData.lenght = 13;
                
                levelData.trapDoorsAmount = 2;
                levelData.maximumMobsAttackingPlayer = 3;
                
                levelData.mobsAmount = 2;
                levelData.mobSpawnDelay = 220;
                
                levelData.roomsAmount = 5;
                
                levelData.itemsMimicsAmount = 1;
                // choose boss
                levelData.currentBoss = -1;
                break;
            
            case 9: // needles boss
                levelData.mainTileIndex = 2;
                levelData.roomTileIndex = 2;
                levelData.fogDistance = 27.5f;
                
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.trapsAmount = 15;
                levelData.mrWindowMax = 0;
                levelData.wallBlockersMax = 6;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                levelData.wallTrapsMax = 6;
                levelData.faceEaterAmount = 0;
            
                //guns
                levelData.deadWeaponRate = 0.1f;
                levelData.npcWeaponRate = 0.4f;
                
                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 0;
                levelData.keysToSpawn = 0;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                
                levelData.meatHoles = 0;
                levelData.mainItemsInMeatHoles = 0;
                
                levelData.propsRate = Random.Range(0.7f, 1);
                levelData.roomPropsRate = Random.Range(0.7f, 1);
                levelData.mainLightColor = Random.Range(0,6);
                
                levelData.maximumHorizontalRoomPos = 10;
                levelData.maximumVerticalRoomPos = 6;
                levelData.steps = 5;
                levelData.width = 13;
                levelData.lenght = 9;
                
                levelData.trapDoorsAmount = 4;
                levelData.maximumMobsAttackingPlayer = 1;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 200;
                
                levelData.roomsAmount = 1;
                
                levelData.itemsMimicsAmount = 1;
                // choose boss
                levelData.currentBoss = levelData.mainTileIndex;
                break;
            
            case 10: // city 
                levelData.mainTileIndex = 7;
                levelData.roomTileIndex = 7;
                levelData.fogDistance = 40;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 200;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].easy;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 1;
                levelData.trapsAmount = 2;
                levelData.wallBlockersMax = 6;
                levelData.wallTrapsMax = 0;
                levelData.faceEaterAmount = 0;
                levelData.npcAmount = 1;
                
                //guns
                levelData.deadWeaponRate = 0.1f;
                levelData.npcWeaponRate = 0.33f;
                
                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 3;
                levelData.keysToSpawn = 3;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 2;
                levelData.mainItemsInMeatHoles = 1;
                
                levelData.propsRate = 0.75f;
                levelData.roomPropsRate = 0.5f;
                levelData.mainLightColor = Random.Range(0, 3);
                
                levelData.maximumHorizontalRoomPos = 10;
                levelData.maximumVerticalRoomPos = 10;
                levelData.steps = 20;
                levelData.width = 15;
                levelData.lenght = 13;
                
                levelData.trapDoorsAmount = 0;
                levelData.maximumMobsAttackingPlayer = 1;
                
                levelData.roomsAmount = 3;
                
                levelData.itemsMimicsAmount = 1;
                // choose boss
                levelData.currentBoss = -1;
                break;

            case 11:
                levelData.mainTileIndex = 8;
                levelData.roomTileIndex = 8;
                levelData.fogDistance = 35;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 200;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 3;
                levelData.wallBlockersMax = 9;
                levelData.trapsAmount = 4;
                levelData.wallTrapsMax = 2;
                levelData.faceEaterAmount = 1;
                levelData.npcAmount = 2;
                
                //guns
                levelData.deadWeaponRate = 0.5f;
                levelData.npcWeaponRate = 1f;
                
                // keys
                levelData.goldenKeysToSpawn = 1;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 4;
                levelData.keysToSpawn = 3;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;
                levelData.keyNpcAmount = 1;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 4;
                levelData.mainItemsInMeatHoles = 1;
                
                levelData.propsRate = 0.5f;
                levelData.roomPropsRate = 0.75f;
                levelData.mainLightColor = Random.Range(3, 6);
                
                levelData.maximumHorizontalRoomPos = 15;
                levelData.maximumVerticalRoomPos = 12;
                levelData.steps = 20;
                levelData.width = 15;
                levelData.lenght = 15;

                levelData.trapDoorsAmount = 0;
                levelData.maximumMobsAttackingPlayer = 1;
                
                levelData.roomsAmount = 5;
                
                levelData.itemsMimicsAmount = 2;
                // choose boss
                levelData.currentBoss = -1;
                break;

            case 12: // divers boss
                levelData.mainTileIndex = 7;
                levelData.roomTileIndex = 8;
                levelData.fogDistance = 35;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 250;
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].hard;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 3;
                levelData.wallBlockersMax = 3;
                levelData.trapsAmount = 5;
                levelData.wallTrapsMax = 2;
                levelData.faceEaterAmount = 0;
                
                //guns
                levelData.deadWeaponRate = 0.3f;
                levelData.npcWeaponRate = 1f;
                
                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 0;
                levelData.keysToSpawn = 0;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 4;
                levelData.mainItemsInMeatHoles = 1;
                
                levelData.propsRate = 0.75f;
                levelData.roomPropsRate = 0.75f;
                levelData.mainLightColor = 2;
                
                levelData.maximumHorizontalRoomPos = 8;
                levelData.maximumVerticalRoomPos = 5;
                levelData.steps = 30;
                levelData.width = 15;
                levelData.lenght = 15;
                
                levelData.trapDoorsAmount = 0;
                levelData.maximumMobsAttackingPlayer = 1;
                
                levelData.roomsAmount = 2;
                
                levelData.itemsMimicsAmount = 1;
                // choose boss
                levelData.currentBoss = levelData.mainTileIndex;
                break;
            
            case 13:  // snows
                levelData.mainTileIndex = 6;
                levelData.roomTileIndex = 3;
                levelData.fogDistance = 45;
                
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].easy;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 2;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 5;
                levelData.wallBlockersMax = 9;
                levelData.wallTrapsMax = 0;
                levelData.faceEaterAmount = 0;
                levelData.npcAmount = 2;
                
                //guns
                levelData.deadWeaponRate = 0.75f;
                levelData.npcWeaponRate = 0.5f;
                
                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 5;
                levelData.keysToSpawn = 5;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 3;
                levelData.mainItemsInMeatHoles = 1;
                
                levelData.propsRate = 0.75f;
                levelData.roomPropsRate = 1;
                levelData.mainLightColor = 5;
                
                levelData.maximumHorizontalRoomPos = 8;
                levelData.maximumVerticalRoomPos = 8;
                levelData.steps = 20;
                levelData.width = 15;
                levelData.lenght = 13;
                
                levelData.trapDoorsAmount = 1;
                levelData.maximumMobsAttackingPlayer = 2;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 180;
                
                levelData.roomsAmount = 6;
                
                levelData.itemsMimicsAmount = 1;
                // choose boss
                levelData.currentBoss = -1;
                break;
            
            case 14:  
                levelData.mainTileIndex = 3;
                levelData.roomTileIndex = 6;
                levelData.fogDistance = 40;
                
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;
                levelData.eyeEatersAmount = 1;
                levelData.treetersAmount = 3;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 7;
                levelData.wallBlockersMax = 6;
                levelData.wallTrapsMax = 6;
                levelData.faceEaterAmount = 1;
                levelData.npcAmount = 2;
            
                //guns
                levelData.deadWeaponRate = 0.75f;
                levelData.npcWeaponRate = 0.5f;
                
                // keys
                levelData.goldenKeysToSpawn = 1;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 5;
                levelData.keysToSpawn = 4;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].hard;
                levelData.keyNpcAmount = 1;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 4;
                levelData.mainItemsInMeatHoles = 2;
                
                levelData.propsRate = 1f;
                levelData.roomPropsRate = 0.75f;
                levelData.mainLightColor = 4;
                
                levelData.maximumHorizontalRoomPos = 15;
                levelData.maximumVerticalRoomPos = 10;
                levelData.steps = 50;
                levelData.width = 18;
                levelData.lenght = 13;
                
                levelData.trapDoorsAmount = 3;
                levelData.maximumMobsAttackingPlayer = 2;
                
                levelData.mobsAmount = 2;
                levelData.mobSpawnDelay = 150;
                
                levelData.roomsAmount = 5;
                
                levelData.itemsMimicsAmount = 1;
                // choose boss
                levelData.currentBoss = -1;
                break;
            
            case 15: // maneater boss
                levelData.mainTileIndex = 6;
                levelData.roomTileIndex = 3;
                levelData.fogDistance = 35;
                
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;
                levelData.eyeEatersAmount = 1;
                levelData.treetersAmount = 4;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 7;
                levelData.wallBlockersMax = 15;
                levelData.wallTrapsMax = 6;
                levelData.faceEaterAmount = 0;
            
                //guns
                levelData.deadWeaponRate = 0.75f;
                levelData.npcWeaponRate = 1f;
                
                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 0;
                levelData.keysToSpawn = 0;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabEye;
                
                levelData.meatHoles = 3;
                levelData.mainItemsInMeatHoles = 2;
                
                levelData.propsRate = 1f;
                levelData.roomPropsRate = 0.75f;
                levelData.mainLightColor = 3;
                
                levelData.maximumHorizontalRoomPos = 10;
                levelData.maximumVerticalRoomPos = 10;
                levelData.steps = 10;
                levelData.width = 13;
                levelData.lenght = 13;
                
                levelData.trapDoorsAmount = 6;
                levelData.maximumMobsAttackingPlayer = 1;
                
                levelData.mobsAmount = 2;
                levelData.mobSpawnDelay = 200;
                
                levelData.roomsAmount = 4;
                
                levelData.itemsMimicsAmount = 1;
                // choose boss
                levelData.currentBoss = levelData.mainTileIndex;
                break;
            
            case 16: // bar
                levelData.mainTileIndex = 4;
                levelData.roomTileIndex = 2;
                levelData.fogDistance = 45;
                
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].easy;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 5;
                levelData.wallBlockersMax = 6;
                levelData.wallTrapsMax = 6;
                levelData.faceEaterAmount = 0;
                levelData.npcAmount = 2;
            
                //guns
                levelData.deadWeaponRate = 0.6f;
                levelData.npcWeaponRate = 0.6f;
                
                // keys
                levelData.goldenKeysToSpawn = 0;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 6;
                levelData.keysToSpawn = 6;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 4;
                levelData.mainItemsInMeatHoles = 2;
                
                levelData.propsRate = 0.75f;
                levelData.roomPropsRate = 0.75f;
                levelData.mainLightColor = Random.Range(0,6);
                
                levelData.maximumHorizontalRoomPos = 15;
                levelData.maximumVerticalRoomPos = 5;
                levelData.steps = 5;
                levelData.width = 18;
                levelData.lenght = 7;
                
                levelData.trapDoorsAmount = 0;
                levelData.maximumMobsAttackingPlayer = 2;
                
                levelData.mobsAmount = 2;
                levelData.mobSpawnDelay = 150;
                
                levelData.roomsAmount = 5;
                
                levelData.itemsMimicsAmount = 3;
                levelData.currentBoss = -1;
                break;
            
            case 17: 
                levelData.mainTileIndex = 4;
                levelData.roomTileIndex = Random.Range(1, 9);
                levelData.fogDistance = 40;
                
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].hard;
                levelData.eyeEatersAmount = 2;
                if (levelData.roomTileIndex == 3)
                    levelData.treetersAmount = Random.Range(3,6);
                else
                    levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 10;
                levelData.wallBlockersMax = 15;
                levelData.wallTrapsMax = 6;
                levelData.faceEaterAmount = 1;
                levelData.npcAmount = 2;
            
                //guns
                levelData.deadWeaponRate = 0.6f;
                levelData.npcWeaponRate = 0.6f;
                
                // keys
                levelData.goldenKeysToSpawn = 1;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 6;
                levelData.keysToSpawn = 3;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].medium;
                levelData.keyNpcAmount = 3;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabEye;
                
                levelData.meatHoles = 4;
                levelData.mainItemsInMeatHoles = 2;
                
                levelData.propsRate = 0.75f;
                levelData.roomPropsRate = 0.75f;
                levelData.mainLightColor = Random.Range(0,6);
                
                levelData.maximumHorizontalRoomPos = 17;
                levelData.maximumVerticalRoomPos = 7;
                levelData.steps = 20;
                levelData.width = 20;
                levelData.lenght = 10;
                
                levelData.trapDoorsAmount = 2;
                levelData.maximumMobsAttackingPlayer = 3;
                
                levelData.mobsAmount = 3;
                levelData.mobSpawnDelay = 130;
                
                levelData.roomsAmount = 6;
                
                levelData.itemsMimicsAmount = 4;
                
                levelData.currentBoss = -1;
                break;
            
            case 18: // boss castle
                levelData.mainTileIndex = 4;
                levelData.roomTileIndex = 5;
                levelData.fogDistance = 35;
                
                levelData.mobsPool = mobsPools[levelData.mainTileIndex].easy;
                levelData.eyeEatersAmount = 0;
                levelData.treetersAmount = 0;
                levelData.mrWindowMax = 0;
                levelData.trapsAmount = 15;
                levelData.wallBlockersMax = 20;
                levelData.wallTrapsMax = 6;
                levelData.faceEaterAmount = 0;
            
                //guns
                levelData.deadWeaponRate = 0.3f;
                levelData.npcWeaponRate = 1f;
                
                // keys
                levelData.goldenKeysToSpawn = 1;
                levelData.tapesToSpawn = 1;
                levelData.padlocksAmount = 0;
                levelData.keysToSpawn = 0;
                levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                levelData.keyNpcAmount = 0;
                levelData.sheAmount = 1;
                levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;
                
                levelData.meatHoles = 4;
                levelData.mainItemsInMeatHoles = 1;
                
                levelData.propsRate = 0.75f;
                levelData.roomPropsRate = 0.5f;
                levelData.mainLightColor = 2;
                
                levelData.maximumHorizontalRoomPos = 20;
                levelData.maximumVerticalRoomPos = 10;
                levelData.steps = 30;
                levelData.width = 25;
                levelData.lenght = 15;
                
                levelData.trapDoorsAmount = 3;
                levelData.maximumMobsAttackingPlayer = 1;
                
                levelData.mobsAmount = 1;
                levelData.mobSpawnDelay = 180;
                
                levelData.roomsAmount = 5;
                
                levelData.itemsMimicsAmount = 5;
                // boss
                levelData.currentBoss = levelData.mainTileIndex;
                break;
            
            default: // random levels
                levelData.mainTileIndex = Random.Range(0, elevatorRooms.Count);
                levelData.roomTileIndex =  Random.Range(0, elevatorRooms.Count);
                levelData.npcAmount = 2;
                //mobs
                if (Random.value < 0.33f)
                {
                    levelData.mobsPool = mobsPools[levelData.mainTileIndex].easy;   
                    levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].easy;
                }
                else if (Random.value < 0.66f)
                {
                    levelData.mobsPool = mobsPools[levelData.mainTileIndex].medium;   
                    levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].medium;
                }
                else
                {
                    levelData.keyNpcPrefabs = keyNpcPool[levelData.mainTileIndex].hard;
                    levelData.mobsPool = mobsPools[levelData.mainTileIndex].hard;   
                }
                
                levelData.faceEaterAmount = Random.Range(0,3);
                
                if (Random.value > 0.5f)
                    levelData.prisonersAmount = 0;
                else
                {
                    if (Random.value <= 0.33)
                        levelData.prisonersAmount = 3;
                    else if (Random.value <= 0.66)
                        levelData.prisonersAmount = 6;
                    else
                        levelData.prisonersAmount = 9;
                }

                levelData.mrWindowMax = Random.Range(3, 10);
                levelData.tapesToSpawn = 0;
                
                // boss 
                if (Random.value > 0.33f)
                    levelData.currentBoss = -1;
                else
                    levelData.currentBoss = levelData.mainTileIndex;
                
                levelData.eyeEatersAmount = Mathf.Clamp(playerFloor / 20, 3, 50);
                levelData.trapsAmount = Random.Range(0,20);
                
                if (levelData.eyeEatersAmount >= 2 && Random.value > 0.25f)
                    levelData.sheNpcPrefab = levelData.sheNpcPrefabEye;
                else
                    levelData.sheNpcPrefab = levelData.sheNpcPrefabFontain;

                if (levelData.roomTileIndex == 3 || levelData.mainTileIndex == 3)
                    levelData.treetersAmount = Random.Range(3, 9);
                else
                    levelData.treetersAmount = 0;
            
                //guns
                levelData.deadWeaponRate = 0.5f;
                levelData.npcWeaponRate = 0.5f;

                // keys
                levelData.goldenKeysToSpawn = 0;
                if (levelData.currentBoss == -1)
                {
                    levelData.padlocksAmount = Random.Range(2,20);
                    levelData.keyNpcAmount = Random.Range(0, levelData.padlocksAmount);
                    levelData.keysToSpawn = levelData.padlocksAmount - levelData.keyNpcAmount;   
                }
                else
                {
                    levelData.padlocksAmount = 0;
                    levelData.keyNpcAmount = 0;
                    levelData.keysToSpawn = 0;
                }
                levelData.sheAmount = 1;

                levelData.meatHoles = Random.Range(0,10);
                levelData.mainItemsInMeatHoles = Random.Range(0,10);
                levelData.wallTrapsMax = Random.Range(0,10);
                levelData.wallBlockersMax = Random.Range(0,10);
                
                levelData.propsRate = Random.Range(0.7f, 1);
                levelData.roomPropsRate = Random.Range(0.7f, 1);
                levelData.mainLightColor = Random.Range(0,6);

                levelData.maximumHorizontalRoomPos = 20;
                levelData.maximumVerticalRoomPos = 10;
                levelData.steps = 30;
                levelData.width = 25;
                levelData.lenght = 15;
                
                levelData.trapDoorsAmount = Random.Range(0, 10);
                levelData.maximumMobsAttackingPlayer = playerFloor;
                
                levelData.mobsAmount = Mathf.Clamp(playerFloor, 1, 100);
                levelData.mobSpawnDelay = Mathf.RoundToInt(1750 / playerFloor);
                
                levelData.roomsAmount = Random.Range(2, 10);
                break;
        }

        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            levelData.sheAmount = 1;
            levelData.sheNpcPrefab = levelData.sheNpcPrefabCoop;
            levelData.faceEaterAmount = 0;

            return;
            
            if (playerFloor > 1)
            {
                levelData.maximumHorizontalRoomPos = Mathf.RoundToInt(levelData.maximumHorizontalRoomPos * 1.25f);
                levelData.maximumVerticalRoomPos = Mathf.RoundToInt(levelData.maximumVerticalRoomPos * 1.25f);
                levelData.steps = Mathf.RoundToInt(levelData.steps * 1.25f);
                levelData.width = Mathf.RoundToInt(levelData.width * 1.25f);
                levelData.lenght = Mathf.RoundToInt(levelData.lenght * 1.25f);
                levelData.roomsAmount = Mathf.RoundToInt(levelData.roomsAmount * 1.5f);
            }
            
            levelData.padlocksAmount = Mathf.RoundToInt(levelData.padlocksAmount * 1.5f);;
            levelData.keyNpcAmount = Mathf.RoundToInt(levelData.keyNpcAmount * 1.5f);
            levelData.keysToSpawn = levelData.padlocksAmount - levelData.keyNpcAmount;

            levelData.mobsAmount = Mathf.CeilToInt(levelData.mobsAmount * 1.75f);

            if (Random.value > 0.75f)
                levelData.faceEaterAmount *= 3;
            else
                levelData.faceEaterAmount = Mathf.CeilToInt(levelData.faceEaterAmount * 1.5f);
            
            if (Random.value > 0.75f)
                levelData.mrWindowMax *= 2;
            else
                levelData.mrWindowMax = Mathf.CeilToInt(levelData.mrWindowMax * 1.5f);
            
            if (Random.value > 0.75f)
                levelData.trapDoorsAmount *= 3;
            else
                levelData.trapDoorsAmount = Mathf.CeilToInt(levelData.trapDoorsAmount * 1.5f);
            
            levelData.eyeEatersAmount = Mathf.Clamp(levelData.eyeEatersAmount * 2, 0, 50);
            
            if (Random.value > 0.5f)
                levelData.trapsAmount = Mathf.RoundToInt(levelData.trapsAmount * Random.Range(1.1f, 2));
            
            levelData.treetersAmount *= 2;
        }
    }
    
    public HealthController GetTrapDoopPrefab(TileController tile)
    {
        if (currentLevelDifficulty == 0)
            return trapDoorsPool[tile.tileIndex].easy[Random.Range(0, trapDoorsPool[tile.tileIndex].easy.Count)];
        else if (currentLevelDifficulty == 1)
            return trapDoorsPool[tile.tileIndex].medium[Random.Range(0, trapDoorsPool[tile.tileIndex].medium.Count)];
        else
            return trapDoorsPool[tile.tileIndex].hard[Random.Range(0, trapDoorsPool[tile.tileIndex].hard.Count)];
    }

    public void PlayerFinishLevel()
    {
        if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            // SOLO
            PlayerFinishedLevelOnClient();
        }
        else
        {
            // COOP
            GLNetworkWrapper.instance.PlayerFinishedLevel();
        }
    }

    public void PlayerFinishedLevelOnClient()
    {
        if (!gm.demo || playerFloor < 3)
            playerFloor++;
        else
            playerFloor = 1;
        
        levelsWithoutShow++;
        if (bossFloor < playerFloor)
        {
            //bossFloor = playerFloor + 3;
            if (!gm.demo)
            {
                currentLevelDifficulty++;
                bossFloor = bossLevels[currentLevelDifficulty];
                
                // CREATE NEW CHECKPOINT
                if (gm.difficultyLevel != GameManager.GameMode.MeatZone)
                {
                    ItemsList.instance.AddToBadReputation(-5);
                    NewCheckpoint();   
                }
                else
                {
                    ItemsList.instance.AddToBadReputation(-2f);
                }
            }
        }
        else
        {
            if (gm.difficultyLevel == GameManager.GameMode.StickyMeat)
            {
                ItemsList.instance.AddToBadReputation(-2);
            }
            else if (gm.difficultyLevel == GameManager.GameMode.Ikarus)
            {
                ItemsList.instance.AddToBadReputation(-2);
            }
            else if (gm.difficultyLevel == GameManager.GameMode.MeatZone)
            {
                ItemsList.instance.AddToBadReputation(-2);
            }
        }

        UpdateProgression();
    }

    void NewCheckpoint()
    {
        checkpoints++;
    }

    void UpdateProgression()
    {
        gm = GameManager.instance;
        if (playerFloor > 0)
            levelNumber.text = "-" + playerFloor;
        else
            levelNumber.text = "0";
    }


    public void PlayerDie()
    {
        var il = ItemsList.instance;
        
        switch (gm.difficultyLevel)
        {
            case GameManager.GameMode.StickyMeat:
                il.AddToBadReputation(-1);
                break;
            
            case GameManager.GameMode.Ikarus:
                playerFloor = bossFloor - 2;
                il.AddToBadReputation(-1);
                break;
            
            case GameManager.GameMode.MeatZone:
                playerFloor = 1;
                currentLevelDifficulty = 0;
                il.AddToBadReputation(-5);
                break;
        }

        if (gm.difficultyLevel != GameManager.GameMode.MeatZone)
        {
            il.LoseCult();
            il.LoseGold(il.gold / 5);
            UpdateProgression();   
        }
        else
        {
            gm.DeleteLocalSave(true);
        }
        
        gm.SaveGame();
    }
    
    

    public void SetLevel(int goToFloor, int difficulty)
    {
        playerFloor = goToFloor;
        currentLevelDifficulty = difficulty;
    }
    
    public void LoadData(SavingData data)
    {
        playerFloor = data.playerFloor;
        bossFloor = data.bossFloor;
        checkpoints = data.checkpoints;
        //bossesKilled = new List<int>(data.bossesKilled);
        currentLevelDifficulty = data.currentLevelDifficulty;
        
        if (data.checkpointsOnFloors != null)
            checkpointsOnFloors = new List<int>(data.checkpointsOnFloors);
        
        UpdateProgression();
    }

    public void SaveNewCheckpoint()
    {
        if (!checkpointsOnFloors.Contains(playerFloor))
            checkpointsOnFloors.Add(playerFloor);
    }
    
    public void ClearSave()
    {
        playerFloor = 0;
        bossFloor = 3;
        checkpoints = 0;
        checkpointsOnFloors.Clear();
        //bossesKilled.Clear();
        currentLevelDifficulty = playerFloor;
        UpdateProgression();
    }

    public bool GetChaseScene()
    {
        if (playerFloor == 0)
            return false;
        else if (playerFloor <= 20)
        {
            switch (playerFloor)
            {
                case 2:
                    return true;
            
                case 5:
                    return true;
            
                case 8:
                    return true;
            
                case 11:
                    return true;
            
                case 14:
                    return true;
            
                case 17:
                    return true;
            
                case 20:
                    return true;
            
                default:
                    return false;
            }   
        }
        else
            return true;
    }
}

[Serializable]
public class MobPool
{
    public List<HealthController> mobs;
}

[Serializable]
public class PropsPool
{
    public List<AssetReference> propsReferences = new List<AssetReference>();
}

[Serializable]
public class RoomsPool
{
    public List<RoomTilesPositionsManager> easyRooms = new List<RoomTilesPositionsManager>();
    public List<RoomTilesPositionsManager> mediumRooms = new List<RoomTilesPositionsManager>();
    public List<RoomTilesPositionsManager> hardRooms = new List<RoomTilesPositionsManager>();
}

[Serializable]
public class UnitsPrefabsPool
{
    public List<HealthController> easy = new List<HealthController>();
    public List<HealthController> medium = new List<HealthController>();
    public List<HealthController> hard = new List<HealthController>();
}
[Serializable]
public class MobsPool
{
    public List<MobPartsController.Mob> easy = new List<MobPartsController.Mob>();
    public List<MobPartsController.Mob> medium = new List<MobPartsController.Mob>();
    public List<MobPartsController.Mob> hard = new List<MobPartsController.Mob>();
}

[Serializable]
public class ItemsOnlevel
{
    public SpawnGroup[] spawnGroups; 
    public AmmoSpawnInfo[] ammoSpawn;
}