using System;
using System.Collections;
using System.Collections.Generic;
using ExtendedItemSpawn;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "LevelSpawnData/NewLevel", menuName = "Level Data")]
public class LevelData : ScriptableObject
{
    public bool dynamicDifficulty = false;

    [Header("Level generator")] 
    public RoomTilesPositionsManager startRoomTilesPositionsManager;

    public float fogDistance = 30f;

    public int ambientIndex = 0;

    public VolumeProfile renderProfile;
    
    public List<RoomTilesPositionsManager> roomTilesPositionsManagers;
    public int roomsAmount = 3;
    public int maximumHorizontalRoomPos = 50;
    public int maximumVerticalRoomPos = 30;
    public int width = 70;
    public int lenght = 50;
    public int steps = 50;
    public int mainTileIndex = 0;
    public int roomTileIndex = 0;
    public float lightRate = 0.1f;
    public int mainLightColor = 0;

    [Header("Spawn")] 
    public int padlocksAmount = 1;
    public int keysToSpawn = 1;
    public int goldenKeysToSpawn = 0;
    public int tapesToSpawn = 0;
    public int lockedDoorsAmount = 1;

    public int meatHoles = 0;
    public int mainItemsInMeatHoles = 0;
    [Range(0, 1)] public float propsRate = 0.5f;
    [Header("These props are used in corridors")]
    public List<AssetReference> propsAdressables;
    public List<AssetReference> roomsPropsReferences;
    [Range(0, 1)] public float roomPropsRate = 0.66f;
    
    public int goldToSpawn = 10;
    //public int regenTilesAmount = 1;

    [Header("NPC")]
    public int vendingMachinesAmount = 1;
    public int prisonersAmount = 1;
    public int bonefiresAmount = 1;
    public HealthController vendingMachinePrefab;
    public HealthController vendingMachinePrefabForCarl;
    public HealthController sheNpcPrefab;
    public HealthController sheNpcPrefabFontain;
    public HealthController sheNpcPrefabEye;
    public HealthController sheNpcPrefabCoop;
    public int sheAmount = 1;
    public List<HealthController> keyNpcPrefabs;
    public int keyNpcAmount = 0;
    public int notesAmount = 3;
    public int noteHintsAmount = 3;

    [Header("Guns")] 
    public float deadWeaponRate = 0.25f;
    public float npcWeaponRate = 0.5f;

    [Header("Monsters")] 
    public int maximumMobsAttackingPlayer = 1;
    public int mobsLevel = 0;
    public int trapsAmount = 10;
    public int trapDoorsAmount = 1;
    public int wallTrapsMax = 10;
    public int faceEaterAmount = 0;
    public int ceilingTrapsMax = 0;
    public int wallBlockersMax = 10;
    public int eyeEatersAmount = 0;
    public int currentBoss = -1;
    public int treetersAmount = 0;
    public int meatTrapsAmount = 0;
    public int itemsMimicsAmount = 0;
    public int mrWindowMax = 0;
    
    [Header("These mobs are spawning in play time")]
    public int mobsAmount = 1;
    public float mobSpawnDelay = 30;
    public List<MobPartsController.Mob> mobsPool;
    public List<ArenaMonsterWave> arenaMonsterWaves;

    [Header("Npc")]
    public int npcAmount = 0;
    public List<HealthController> npcsPool;

    [Header("Loot")]
    //[Range(1, 20)]
    public AmmoSpawnInfo[] ammoSpawn;
    public SpawnGroup[] spawnGroups;
}

[Serializable]
public class ArenaMonsterWave
{
    public List<MobPartsController.Mob> mobsPool;
}
