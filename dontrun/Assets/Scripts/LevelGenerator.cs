using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using ExtendedItemSpawn;
using Mirror;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator instance;

    [HideInInspector]
    public List<RoomController> roomsPrefabs;

    public List<AssetReference> propsReferences;
    public List<PropController> propsInGame;
    
    [HideInInspector]
    public TileController meatTile;
    [HideInInspector]
    public List<TileController> levelTiles;
    [HideInInspector]
    public List<TileController> levelTilesInGame;
    [HideInInspector]
    public List<AssetReference> roomsPropsReferences;
    int coridorIndexCurrent = 0;

    public int currentMainTile = 0;
    public int currentRoomTile = 0;

    public List<Vector3> usedPositions = new List<Vector3>();
    public List<Vector3> blockersPositions = new List<Vector3>();
    public List<Vector3> availablePositions = new List<Vector3>();

    public int steps = 150;
    public float tileSize = 5;

    public int trapsAmount = 10;
    public int ceilingTrapsAmount = 0;
    public int wallTrapsMax = 0;
    public int wallBlockersMax = 0;
    public List<GameObject> corridorPrefabsFound = new List<GameObject>(); 
    [Range(0, 1)] public float propsRate = 1;
    [Range(0, 1)] public float roomPropsRate = 1;

    public int roomsAmount = 3;

    // ALL IN TILES
    /// SQUARE IT BY tileSize
    /// ///////////////////////
    [HideInInspector] public int maximumHorizontalRoomPos = 150;

    [HideInInspector] public int maximumVerticalRoomPos = 150;
    [HideInInspector] public int width = 300;
    [HideInInspector] public int length = 300;

    public List<RoomExit> roomExits;
    public List<RoomFiller> roomFillers;

    public List<TileController> connectedTiles = new List<TileController>();
    public List<TileController> disconnectedTiles = new List<TileController>();
    public List<RoomController> roomsInGame = new List<RoomController>();
    public List<TileController> tilesInCurrentIsland = new List<TileController>();

    List<TileController> island_0 = new List<TileController>();
    List<TileController> island_1 = new List<TileController>();
    List<TileController> island_2 = new List<TileController>();
    List<TileController> island_3 = new List<TileController>();
    List<TileController> island_4 = new List<TileController>();
    List<TileController> island_5 = new List<TileController>();
    List<TileController> island_6 = new List<TileController>();
    List<TileController> island_7 = new List<TileController>();
    List<TileController> island_8 = new List<TileController>();
    List<TileController> island_9 = new List<TileController>();
    List<TileController> island_10 = new List<TileController>();
    List<TileController> island_11 = new List<TileController>();
    List<TileController> island_12 = new List<TileController>();
    List<TileController> island_13 = new List<TileController>();
    List<TileController> island_14 = new List<TileController>();
    List<TileController> island_15 = new List<TileController>();
    List<TileController> island_16 = new List<TileController>();
    List<TileController> island_17 = new List<TileController>();
    List<TileController> island_18 = new List<TileController>();
    List<TileController> island_19 = new List<TileController>();
    List<TileController> island_20 = new List<TileController>();

    public GameObject subRoomMarker;
    public GameObject deadEndMarker;

    public bool generating = false;

    GameManager gm;
    PlayerMovement pm;
    SpawnController sc;
    GameObject tilesFolder;

    public LevelTree levelTree = new LevelTree();

    private List<int> tilesPositionsUsedX = new List<int>();
    private List<int> tilesPositionsUsedZ = new List<int>();
    List<RoomTilesPositionsManager> rtps = new List<RoomTilesPositionsManager>(); // temp list from gm.loadedRtps
    List<RoomTilesPositionsManager> roomsToSpawn = new List<RoomTilesPositionsManager>(); //temp list of the RTPs to spawn rooms

    public Transform playerSpawner;
    
    LevelData newLevelData = null;
    
    public List<ActivateRandomObject> randomObjectsActivators = new List<ActivateRandomObject>();

    public bool levelgenOnHost = false;

    private int roomsToSpawnAmount = 0;
    
    private void Awake()
    {
        instance = this;
        gm = GameManager.instance;
        gm.lg = this;

        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            StartCoroutine(SetActiveScene());
        }
    }

    IEnumerator SetActiveScene()
    {
        while (SceneManager.GetSceneByBuildIndex(4).isLoaded == false)
        {
            yield return null;
        }
        
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
    }
    

    private void Start()
    {
        GameManager.instance.loadingAnim.gameObject.SetActive(false);
        
        pm = PlayerMovement.instance;
        tilesFolder = new GameObject();
        tilesFolder.transform.parent = transform;
        tilesFolder.name = "TilesFolder";
        sc = SpawnController.instance;
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            print("[COOP] SET LEVEL GEN");
            GLNetworkWrapper.instance.localPlayer.SetLevelGen(true);

            if (!levelgenOnHost)
            {
                generating = true;
                if (!PlayerSkillsController.instance.horrorMode)
                    RenderSettings.fogEndDistance = gm.level.fogDistance;
                
                ItemsList.instance.Init();
            }
        }
    }

    void OnDestroy()
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.SetLevelGen(false);
        }
    }
    
    public void ClearData()
    {
        for (var i = levelTilesInGame.Count - 1; i >= 0; i--)
        {
            Destroy(levelTilesInGame[i]);
        }

        if (roomsInGame.Count > 0)
        {
            for (var i = roomsInGame.Count - 1; i >= 0; i--)
            {
                Destroy(roomsInGame[i].gameObject);
            }
        }

        roomsInGame.Clear();
        levelTilesInGame.Clear();
        island_0.Clear();
        island_1.Clear();
        island_2.Clear();
        island_3.Clear();
        island_4.Clear();
        island_5.Clear();
        island_6.Clear();
        island_7.Clear();
        island_8.Clear();
        island_9.Clear();
        island_10.Clear();
        island_11.Clear();
        island_12.Clear();
        connectedTiles.Clear();
        disconnectedTiles.Clear();
        tilesInCurrentIsland.Clear();
        roomExits.Clear();
        roomFillers.Clear();
        usedPositions.Clear();
        availablePositions.Clear();
        blockersPositions.Clear();
        tilesPositionsUsedX.Clear();
        tilesPositionsUsedZ.Clear();
        corridorPrefabsFound.Clear();
        rtps.Clear();
    }

    public void UpdateCorridorPropsReferences()
    {
        propsReferences = new List<AssetReference>(gm.level.propsAdressables);
    }
    
    public void UpdateRoomPropsReferences()
    {
        roomsPropsReferences = new List<AssetReference>(gm.level.roomsPropsReferences);
    }
    
    public void Init()
    {
        print ("init level gen");
        rtps.Clear();

        if (gm.arena)
        {
            newLevelData = gm.arenaLevel;
        }
        else
        {
            if (gm.tutorialPassed == 1)
                newLevelData = gm.level;
            else if (gm.tutorialPassed == 0)
            {
                newLevelData = gm.tutorialLevel;
            }   
        }

        roomsAmount = newLevelData.roomsAmount;
        maximumHorizontalRoomPos = newLevelData.maximumHorizontalRoomPos;
        maximumVerticalRoomPos = newLevelData.maximumVerticalRoomPos;
        width = newLevelData.width;
        length = newLevelData.lenght;
        steps = newLevelData.steps;
        currentMainTile = newLevelData.mainTileIndex;
        currentRoomTile = newLevelData.roomTileIndex;
        trapsAmount = newLevelData.trapsAmount;
        ceilingTrapsAmount = newLevelData.ceilingTrapsMax;
        roomPropsRate = newLevelData.roomPropsRate;
        wallTrapsMax = newLevelData.wallTrapsMax;
        wallBlockersMax = newLevelData.wallBlockersMax;
        propsRate = newLevelData.propsRate;

        propsReferences = new List<AssetReference>(newLevelData.propsAdressables);
        roomsPropsReferences = new List<AssetReference>(newLevelData.roomsPropsReferences);

        if (!PlayerSkillsController.instance.horrorMode)
            RenderSettings.fogEndDistance = newLevelData.fogDistance;

        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            if (GLNetworkWrapper.instance.localPlayer)
            {
                //GLNetworkWrapper.instance.localPlayer.SetLevelGen();
                if (GLNetworkWrapper.instance.localPlayer.isServer)
                {
                    levelgenOnHost = true;
                    StartCoroutine(SpawnRooms());   
                }
            }
            else
            {
                rtps.Clear();
                rtps = new List<RoomTilesPositionsManager>(gm.loadedRtps);
            }
        }
        else
            StartCoroutine(SpawnRooms());
    }

    void StoreTilesPositions(RoomTilesPositionsManager rtp)
    {
        for (var index = 0; index < rtp.tilesLocalPositions.Count; index++)
        {
            var v = rtp.tilesLocalPositions[index];
            var newIntX = Mathf.RoundToInt(v.x + rtp.transform.position.x);
            var newIntZ = Mathf.RoundToInt(v.z + rtp.transform.position.z);
            tilesPositionsUsedX.Add(newIntX);
            tilesPositionsUsedZ.Add(newIntZ);
        }
    }

    bool GetPositionForRoom(RoomTilesPositionsManager rtp)
    {
        for (var index = 0; index < rtp.tilesLocalPositions.Count; index++)
        {
            var v = rtp.tilesLocalPositions[index];
            // check the same pos
            if (tilesPositionsUsedX.Contains(Mathf.RoundToInt(v.x + rtp.transform.position.x)) &&
                tilesPositionsUsedZ.Contains(Mathf.RoundToInt(v.z + rtp.transform.position.z)))
                return false;

            // check diagonal pos
            if (tilesPositionsUsedX.Contains(Mathf.RoundToInt(v.x + rtp.transform.position.x - tileSize)) &&
                tilesPositionsUsedZ.Contains(Mathf.RoundToInt(v.z + rtp.transform.position.z + tileSize)))
                return false;
            if (tilesPositionsUsedX.Contains(Mathf.RoundToInt(v.x + rtp.transform.position.x + tileSize)) &&
                tilesPositionsUsedZ.Contains(Mathf.RoundToInt(v.z + rtp.transform.position.z + tileSize)))
                return false;
            if (tilesPositionsUsedX.Contains(Mathf.RoundToInt(v.x + rtp.transform.position.x + tileSize)) &&
                tilesPositionsUsedZ.Contains(Mathf.RoundToInt(v.z + rtp.transform.position.z - tileSize)))
                return false;
            if (tilesPositionsUsedX.Contains(Mathf.RoundToInt(v.x + rtp.transform.position.x - tileSize)) &&
                tilesPositionsUsedZ.Contains(Mathf.RoundToInt(v.z + rtp.transform.position.z - tileSize)))
                return false;
        }

        return true;
    }

    public void AddRoom(RoomController room)
    {
        roomsInGame.Add(room);
    }
    
    void SpawnStartRoom(AssetReference roomAdressable)
    {
        // spawn room on clients
        AssetSpawner.instance.Spawn(roomAdressable, Vector3.zero,AssetSpawner.ObjectType.Room);
        
        /*
        if (levelgenOnHost)
        {
            GLNetworkWrapper.instance.SpawnRoom(0, Vector3.zero);            
        }
        else
        {
            AssetSpawner.instance.Spawn(roomAdressable, Vector3.zero, AssetSpawner.ObjectType.Room);
        }
        */
    }

    // THIS CALLS BY LEVELGEN
    void SpawnRoom(int index, Vector3 pos)
    {
        //spawn room on clients
        AssetSpawner.instance.Spawn(gm.loadedRtps[index].roomReference, pos,AssetSpawner.ObjectType.Room);    
        
        /*
        if (levelgenOnHost)
        {
            GLNetworkWrapper.instance.SpawnRoom(index, pos);
        }
        else
        {
            print(index);
            //AssetSpawner.instance.Spawn(rtps[index].roomReference, pos, AssetSpawner.ObjectType.Room);    
            AssetSpawner.instance.Spawn(gm.loadedRtps[index].roomReference, pos, AssetSpawner.ObjectType.Room);    
        }
        */
    }

    /*
    // this called on clients by PLAYER NETWORK OBJECT
    public IEnumerator SpawnRoomOnClient(int index, Vector3 pos)
    {
        while (GLNetworkWrapper.instance.localPlayer.roomsAmount < roomsToSpawnAmount)
        {
            print("Wait until all rooms are spawned. Rooms: " + GLNetworkWrapper.instance.localPlayer.roomsAmount +
                  ". Need: " + roomsToSpawnAmount);
            yield return new WaitForSeconds(0.1f);
        }
     
        roomsToSpawnAmount++;

        print("Try to load RTPs with index " + index);
        //AssetSpawner.instance.Spawn(rtps[index].roomReference, pos, AssetSpawner.ObjectType.Room);    
        AssetSpawner.instance.Spawn(gm.loadedRtps[index].roomReference, pos, AssetSpawner.ObjectType.Room);       

        //rtps.RemoveAt(index);

        if (rtps.Count < 1)
        {
            rtps.Clear();

            rtps = new List<RoomTilesPositionsManager>(gm.loadedRtps);
            rtps.RemoveAt(0);
        }
    }
    */
    
    // this called on clients by PLAYER NETWORK OBJECTS
    public IEnumerator TileRandomCeilingOnClient(int tileIndex, float r)
    {
        while (levelTilesInGame.Count <= tileIndex)
        {
            yield return new WaitForSeconds(0.1f);
        }
        levelTilesInGame[tileIndex].RandomCelling(r);
    }

    // this called on host
    IEnumerator SpawnRooms()
    {
        while (levelgenOnHost && GLNetworkWrapper.instance.ClientsGotLevelgen() == false)
        {
            print("WAIT UNTIL PLAYERS FIND THEIR LEVELGENS");
            yield return new WaitForSeconds(0.1f);
        }
        
        yield return new WaitForSeconds(1);
        
        print ("spawn rooms level gen. levelgenOnHost is " + levelgenOnHost);
        rtps.Clear();
        rtps = new List<RoomTilesPositionsManager>(gm.loadedRtps);
        
        // store tile positions in matrix
        StoreTilesPositions(rtps[0]);
        SpawnStartRoom(rtps[0].roomReference);
        rtps.RemoveAt(0);
        
        yield return null;
        
        roomsToSpawn = new List<RoomTilesPositionsManager>();
        
        int roomSide = 0;
        // 0 - top left, 1 - top right, 2 - bottom right, 3 - bottom lefy
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            for (int i = rtps.Count - 1; i >= 0; i--)
            {
                if (rtps[i].soloOnly)
                    rtps.RemoveAt(i);
            }   
        }

        for (int i = 1; i <= roomsAmount; i++)
        {
            // берем здесь рандомный RTP
            
            var randomRtp = rtps[Random.Range(0, rtps.Count)];
            
            // CHANGE RANDOM POS IN ONE OF FOUR AREAS AROUND ELEVATOR
            Vector3 newPos = Vector3.zero;
            bool foundPlace = false;
            int c = 0;
            while (!foundPlace && c < 500)
            {
                switch (roomSide)
                {
                    case 0:
                        newPos = new Vector3(Random.Range(-maximumHorizontalRoomPos, -3) * tileSize, 0,
                            Random.Range(3, maximumVerticalRoomPos) * tileSize);
                        break;

                    case 1:
                        newPos = new Vector3(Random.Range(3, maximumHorizontalRoomPos) * tileSize, 0,
                            Random.Range(3, maximumVerticalRoomPos) * tileSize);
                        break;

                    case 2:
                        newPos = new Vector3(Random.Range(3, maximumHorizontalRoomPos) * tileSize, 0,
                            Random.Range(-maximumVerticalRoomPos, -3) * tileSize);
                        break;

                    case 3:
                        newPos = new Vector3(Random.Range(-maximumHorizontalRoomPos, -3) * tileSize, 0,
                            Random.Range(-maximumVerticalRoomPos, -4) * tileSize);
                        break;
                }

                randomRtp.transform.position = newPos;
                foundPlace = GetPositionForRoom(randomRtp);
                if (foundPlace)
                {
                    roomsToSpawn.Add(randomRtp);
                    StoreTilesPositions(randomRtp);
                }

                c++;

                roomSide++;
                if (roomSide >= 3)
                    roomSide = 0;
                yield return new WaitForEndOfFrame();
            }
            rtps.Remove(randomRtp);

            if (rtps.Count < 1)
            {
                rtps.Clear();

                rtps = new List<RoomTilesPositionsManager>(gm.loadedRtps);
                rtps.RemoveAt(0);
            }

            yield return new WaitForEndOfFrame();
        }
        
        // SPAWN ROOMS

        for (int i = 0; i < roomsToSpawn.Count; i++)
        {
            Vector3 newPos = roomsToSpawn[i].transform.position;
            //int rtpsIndex = rtps.IndexOf(roomsToSpawn[i]);
            int rtpsIndex = gm.loadedRtps.IndexOf(roomsToSpawn[i]);
            print(roomsToSpawn[i].name + " is trying to spawn by RTPS index of " + rtpsIndex + ". i is " + i);
            SpawnRoom(rtpsIndex, newPos);
        }
        
        while (roomsInGame.Count - 1 < roomsToSpawn.Count) // exclude elevator room
        {
            print("wait for all rooms to spawn");
            yield return null;
        }

        print("activate all rooms");
        for (var index = 0; index < roomsInGame.Count; index++)
        {
            var r = roomsInGame[index];
            
            r.gameObject.SetActive(true);
            //if (gm.tutorialPassed == 1)
                yield return new WaitForEndOfFrame();
        }

        for (int i = roomsPrefabs.Count - 1; i >= 0; i--)
        {
            if (!roomsPrefabs[i].gameObject.activeInHierarchy)
            {
                print("destroy room prefab??");
                Destroy(roomsPrefabs[i].gameObject);
                roomsPrefabs.RemoveAt(i);
            }
        }
        for (int i = rtps.Count - 1; i >= 0; i--)
        {
            print("destroy room rtp");
            Destroy(rtps[i].gameObject);
            rtps.RemoveAt(i);   
        }
        rtps.Clear();
        //gm.loadedRtps.Clear();
        roomsPrefabs.Clear();
        StartCoroutine(GenerateFloor());
    }


    IEnumerator GenerateFloor()
    {
        if (levelgenOnHost)
        {
            while (GLNetworkWrapper.instance.playerNetworkObjects[1].roomsAmount < GLNetworkWrapper.instance.playerNetworkObjects[0].roomsAmount)
            {
                print("[COOP] Second player have missing rooms");
                yield return new WaitForSeconds(0.1f);
                
            }    
        }
        
        print("generate floor");
        // fill rooms
        for (var index = 0; index < roomFillers.Count; index++)
        {
            RoomFiller filler = roomFillers[index];

            if (gm.tutorialPassed == 1)
            {
                if (filler.masterRoom == roomsInGame[0])
                    filler.tileIndex = currentMainTile;
                else
                {
                    filler.tileIndex = currentRoomTile;
                    if (!filler.cultTile)
                    {
                        if (filler.tileIndex == 1)
                            filler.statusEffect = StatusEffects.StatusEffect.Rust;
                        else if (filler.tileIndex == 2)
                            filler.statusEffect = StatusEffects.StatusEffect.Poison;
                        else if (filler.tileIndex == 3)
                            filler.statusEffect = StatusEffects.StatusEffect.Cold;
                        else if (filler.tileIndex == 5)
                            filler.statusEffect = StatusEffects.StatusEffect.Bleed;   
                    }
                }   
            }

            Vector3 tilePosition = filler.transform.position;
            for (int i = 0; i < filler.steps; i++)
            {
                SpawnTile(tilePosition, filler.tileIndex, filler.masterRoom, filler.statusEffect, filler.spawner, 0, true, filler.cultTile);

                //lineCurrentLenght++;
                Vector3 newPos = NewStepPosition(tilePosition);
                if (newPos.x > -9000f)
                    tilePosition = newPos;
                else
                    break;
                
                //if (gm.tutorialPassed == 1)
                    yield return new WaitForEndOfFrame();
            }

            //if (gm.tutorialPassed == 1)
                yield return new WaitForEndOfFrame();
                //yield return new WaitForSeconds(0.1f);
            
        }

        yield return new WaitForEndOfFrame();
        
        //lineCurrentLenght = 0;
        // fill coridors
        for (var index = 0; index < roomExits.Count; index++)
        {
            RoomExit exit = roomExits[index];
            Vector3 tilePosition = exit.transform.position;
            for (int i = 0; i < steps; i++)
            {
                //print("spawnTile from room exits");
                SpawnTile(tilePosition, currentMainTile, null, StatusEffects.StatusEffect.Null, true, coridorIndexCurrent, false, false);
                //lineCurrentLenght++;
                Vector3 newPos = NewStepPosition(tilePosition);
                if (newPos.x > -9000f)
                    tilePosition = newPos;
                else
                {
                    coridorIndexCurrent++;
                    break;
                }
                
                //if (gm.tutorialPassed == 1)
                    yield return new WaitForEndOfFrame();
                    //yield return new WaitForSeconds(0.1f);
            }

            //if (gm.tutorialPassed == 1)
                yield return new WaitForEndOfFrame();
                //yield return new WaitForSeconds(0.1f);

            //yield return new WaitForEndOfFrame();
            coridorIndexCurrent++;
        }

        print("Floor generation is over.");
        StartCoroutine(ConnectTiles());
    }

    void RandomCelling()
    {
        print("random ceiling");
        for (var index = 0; index < levelTilesInGame.Count; index++)
        {
            var tile = levelTilesInGame[index];
            float r = Random.value;
            if (levelgenOnHost)
            {
                // multiplayer
                GLNetworkWrapper.instance.RandomCeiling(index, r);
                
            }
            else
                tile.RandomCelling(r);
        }
        StartCoroutine(SpawnWalls());
    }
    
    public void SpawnTile(Vector3 tilePosition, int tileIndex, RoomController masterRoom, StatusEffects.StatusEffect effect, 
        bool spawner, int coridorIndex, bool ignoreBoundaries, bool cultTile)
    {
        bool usedTile = false;

        if (masterRoom == null || !ignoreBoundaries)
        {
            usedTile = tilePosition.x > width * tileSize || tilePosition.x < width * tileSize * -1 ||
                      tilePosition.z > length * tileSize || tilePosition.z < length * tileSize * -1;
        }

        if (!usedTile || tileIndex == -1)
        {
            bool alreadySpawnedHere = false;
            for (var index = 0; index < levelTilesInGame.Count; index++)
            {
                var t = levelTilesInGame[index];
                if (Vector3.Distance(t.transform.position, tilePosition) < 1)
                {
                    alreadySpawnedHere = true;
                    break;
                }
            }

            if (!alreadySpawnedHere)
            {
                if (levelgenOnHost)
                {
                    GLNetworkWrapper.instance.SpawnTile(tilePosition, tileIndex, masterRoom, effect, 
                        spawner, coridorIndex, cultTile);
                }
                else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false) // spawn tile for solo campaign
                {
                    int fxIndex = -1;
                    switch (effect)
                    {
                        case StatusEffects.StatusEffect.Poison:
                            fxIndex = 0;
                            break;
                        case StatusEffects.StatusEffect.Fire:
                            fxIndex = 1;
                            break;
                        case StatusEffects.StatusEffect.Bleed:
                            fxIndex = 2;
                            break;
                        case StatusEffects.StatusEffect.Rust:
                            fxIndex = 3;
                            break;
                        case StatusEffects.StatusEffect.HealthRegen:
                            fxIndex = 4;
                            break;
                        case StatusEffects.StatusEffect.GoldHunger:
                            fxIndex = 5;
                            break;
                        case StatusEffects.StatusEffect.Cold:
                            fxIndex = 6;
                            break;
                        case StatusEffects.StatusEffect.InLove:
                            fxIndex = 7;
                            break;
                    }

                    int masterRoomIndex = -1;
                    if (masterRoom != null)
                        masterRoomIndex = roomsInGame.IndexOf(masterRoom);
                    
                    SpawnTileOnClient(tilePosition, tileIndex, masterRoomIndex, fxIndex, 
                        spawner, coridorIndex, cultTile);
                }
            }
        }
    }

    public void SpawnTileOnClient(Vector3 tilePosition, int tileIndex, int masterRoomIndex, int effectIndex, 
        bool spawner, int coridorIndex, bool cultTile)
    {
        // HOST OR SOLO
        
        TileController newTile;
            
        if (tileIndex >= 0) newTile = Instantiate(levelTiles[tileIndex], tilePosition, Quaternion.identity);
        else newTile = Instantiate(meatTile, tilePosition, Quaternion.identity);
        
        if (masterRoomIndex >= 0 && masterRoomIndex < roomsInGame.Count && roomsInGame[masterRoomIndex] != null)
        {
            roomsInGame[masterRoomIndex].tiles.Add(newTile);
            newTile.transform.parent = roomsInGame[masterRoomIndex].transform;
            newTile.spawnerTile = spawner;
            newTile.canSpawnProp = spawner;
            newTile.canSpawnTrap = spawner;
        }
        else
        {
            newTile.transform.parent = tilesFolder.transform;
            newTile.spawnerTile = spawner;
            newTile.canSpawnProp = spawner;
            newTile.canSpawnTrap = spawner;
            newTile.corridorIndex = coridorIndex;
            newTile.CorridorTile();   
        }

        var effect = StatusEffects.StatusEffect.Null;
        switch (effectIndex)
        {
            case 0:
                effect = StatusEffects.StatusEffect.Poison;
                break;
            
            case 1:
                effect = StatusEffects.StatusEffect.Fire;
                break;
            case 2:
                effect = StatusEffects.StatusEffect.Bleed;
                break;
            case 3:
                effect = StatusEffects.StatusEffect.Rust;
                break;
            case 4:
                effect = StatusEffects.StatusEffect.HealthRegen;
                break;
            case 5:
                effect = StatusEffects.StatusEffect.GoldHunger;
                break;
            case 6:
                effect = StatusEffects.StatusEffect.Cold;
                break;
            case 7:
                effect = StatusEffects.StatusEffect.InLove;
                break;
        }
        
        if (tileIndex == 0 || tileIndex == 6 || tileIndex == 4)
            newTile.tileStatusEffect = StatusEffects.StatusEffect.Null;
        else if (effect != StatusEffects.StatusEffect.Null)
            newTile.tileStatusEffect = effect;

        RoomController roomInGame = null;
        if (masterRoomIndex >= 0 && masterRoomIndex < roomsInGame.Count)
            roomInGame = roomsInGame[masterRoomIndex];
        
        newTile.Init(roomInGame);
        if (cultTile && roomInGame != null && roomInGame.cultGenerator) roomInGame.cultGenerator.AddTileCult(newTile);
        
        List<Vector3> availablePositionsTemp = AvailablePositions(tilePosition);
        for (int i = availablePositionsTemp.Count - 1; i >= 0; i--)
        {
            bool canAdd = true;

            for (var index = 0; index < availablePositions.Count; index++)
            {
                Vector3 v = availablePositions[index];
                if (Vector3.Distance(v, availablePositionsTemp[i]) <= 0.5f)
                {
                    canAdd = false;
                    break;
                }
            }

            if (canAdd)
                availablePositions.Add(availablePositionsTemp[i]);
        }

        for (int i = availablePositions.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(tilePosition, availablePositions[i]) <= 0.5f)
            {
                availablePositions.RemoveAt(i);
            }
        }

        if (tileIndex == -1) // meat tile
        {
            newTile.GetNeighbours();
            newTile.CreateWallsBetweenTiles(0,0,0,0,0,0,0,0,0,0,0,0);
            
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && levelgenOnHost)
                AiDirector.instance.NewMeatTile(newTile);
            else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                AiDirector.instance.NewMeatTile(newTile);

        }
    }
    

    Vector3 NewStepPosition(Vector3 lastPos)
    {
        List<Vector3> availablePositionsTemp = AvailablePositions(lastPos);
        if (availablePositionsTemp.Count > 0)
        {
            Vector3 newSectionPosition =
                availablePositionsTemp[Random.Range(0, availablePositionsTemp.Count)];
            
            return newSectionPosition;
        }
        else
        {
            return new Vector3(-10000, -10000, -10000);
        }
    }

    public List<Vector3> AvailablePositions(Vector3 currentPosition)
    {
        List<Vector3> availablePositionsTemp = new List<Vector3>();

        availablePositionsTemp.Add(currentPosition + Vector3.forward * tileSize);
        availablePositionsTemp.Add(currentPosition + Vector3.right * tileSize);
        availablePositionsTemp.Add(currentPosition + Vector3.back * tileSize);
        availablePositionsTemp.Add(currentPosition + Vector3.left * tileSize);

        for (int i = 3; i >= 0; i--)
        {
            bool remove = false;
            
            if (availablePositionsTemp[i].x > width * tileSize ||
                availablePositionsTemp[i].x < width * tileSize * -1 ||
                availablePositionsTemp[i].z > length * tileSize ||
                availablePositionsTemp[i].z < length * tileSize * -1) // if out of bounds
            {
                remove = true;
                break;
            }

            for (var index = 0; index < usedPositions.Count; index++)
            {
                Vector3 usedPos = usedPositions[index];
                if (Vector3.Distance(usedPos, availablePositionsTemp[i]) < 0.5f)
                {
                    remove = true;
                    break;
                }
            }

            for (var index = 0; index < blockersPositions.Count; index++)
            {
                Vector3 blocker = blockersPositions[index];
                if (Vector3.Distance(blocker, availablePositionsTemp[i]) < 0.5f)
                {
                    remove = true;
                    break;
                }
            }

            for (var index = 0; index < roomFillers.Count; index++)
            {
                RoomFiller filler = roomFillers[index];
                if (Vector3.Distance(filler.transform.position, availablePositionsTemp[i]) < 1f)
                {
                    remove = true;
                    break;
                }
            }

            if (remove)
                availablePositionsTemp.RemoveAt(i);
        }

        return availablePositionsTemp;
    }

    IEnumerator ConnectTiles()
    {
        print("find isolateTiles");
        disconnectedTiles = new List<TileController>(levelTilesInGame);

        while (disconnectedTiles.Count > 0)
        {
            for (int i = 0; i < levelTilesInGame.Count; i++)
            {
                if (disconnectedTiles.Count <= 0)
                {
                    break;
                }

                int randomTile = UnityEngine.Random.Range(0, disconnectedTiles.Count);
                tilesInCurrentIsland.Clear();
                disconnectedTiles[randomTile].MarkIsland(i);

                int tilesInIslandLastFrame = 0;
                int frame = 0;

                // этот луп занимает очень много времени на больших локах, но без него сейчас все вешается
                while (tilesInCurrentIsland.Count < levelTilesInGame.Count)
                {
                    tilesInIslandLastFrame = tilesInCurrentIsland.Count;

                    //yield return new WaitForEndOfFrame();
                    if (tilesInIslandLastFrame == tilesInCurrentIsland.Count)
                    {
                        if (frame < 5)
                            frame++;
                        else
                        {
                            frame = 0;
                            break;
                        }
                    }
                    else
                        frame = 0;
                }
                
                //if (gm.tutorialPassed == 1)
                    yield return new WaitForEndOfFrame();
            }

            //yield return new WaitForEndOfFrame();
        }


        Debug.Log("check if tiles can be added to islands list");
        //fill islands lists
        for (var i = 0; i < levelTilesInGame.Count; i++)
        {
            TileController tc = levelTilesInGame[i];
            bool canAdd = true;
            switch (tc.island)
            {
                case 0:
                    island_0.Add(tc);
                    break;
                case 1:
                    canAdd = true;
                    for (var index = 0; index < island_0.Count; index++)
                    {
                        TileController t = island_0[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    //if (canAdd)
                    island_1.Add(tc);
                    break;
                case 2:
                    canAdd = true;
                    for (var index = 0; index < island_1.Count; index++)
                    {
                        TileController t = island_1[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    //if (canAdd)
                    island_2.Add(tc);
                    break;
                case 3:
                    canAdd = true;
                    for (var index = 0; index < island_2.Count; index++)
                    {
                        TileController t = island_2[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_3.Add(tc);
                    break;
                case 4:
                    canAdd = true;
                    for (var index = 0; index < island_3.Count; index++)
                    {
                        TileController t = island_3[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_4.Add(tc);
                    break;
                case 5:
                    canAdd = true;
                    for (var index = 0; index < island_4.Count; index++)
                    {
                        TileController t = island_4[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_5.Add(tc);
                    break;
                case 6:
                    canAdd = true;
                    for (var index = 0; index < island_5.Count; index++)
                    {
                        TileController t = island_5[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_6.Add(tc);
                    break;
                case 7:
                    canAdd = true;
                    for (var index = 0; index < island_6.Count; index++)
                    {
                        TileController t = island_6[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_7.Add(tc);
                    break;
                case 8:
                    canAdd = true;
                    for (var index = 0; index < island_7.Count; index++)
                    {
                        TileController t = island_7[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_8.Add(tc);
                    break;
                case 9:
                    canAdd = true;
                    for (var index = 0; index < island_8.Count; index++)
                    {
                        TileController t = island_8[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_9.Add(tc);
                    break;
                case 10:
                    canAdd = true;
                    for (var index = 0; index < island_9.Count; index++)
                    {
                        TileController t = island_9[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_10.Add(tc);
                    break;
                case 11:
                    canAdd = true;
                    for (var index = 0; index < island_10.Count; index++)
                    {
                        TileController t = island_10[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_11.Add(tc);
                    break;
                case 12:
                    canAdd = true;
                    for (var index = 0; index < island_11.Count; index++)
                    {
                        TileController t = island_11[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_12.Add(tc);
                    break;
                case 13:
                    canAdd = true;
                    for (var index = 0; index < island_12.Count; index++)
                    {
                        TileController t = island_12[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_13.Add(tc);
                    break;
                case 14:
                    canAdd = true;
                    for (var index = 0; index < island_13.Count; index++)
                    {
                        TileController t = island_13[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_14.Add(tc);
                    break;
                case 15:
                    canAdd = true;
                    for (var index = 0; index < island_14.Count; index++)
                    {
                        TileController t = island_14[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_15.Add(tc);
                    break;
                case 16:
                    canAdd = true;
                    for (var index = 0; index < island_15.Count; index++)
                    {
                        TileController t = island_15[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_16.Add(tc);
                    break;
                case 17:
                    canAdd = true;
                    for (var index = 0; index < island_16.Count; index++)
                    {
                        TileController t = island_16[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_17.Add(tc);
                    break;
                case 18:
                    canAdd = true;
                    for (var index = 0; index < island_17.Count; index++)
                    {
                        TileController t = island_17[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_18.Add(tc);
                    break;
                case 19:
                    canAdd = true;
                    for (var index = 0; index < island_18.Count; index++)
                    {
                        TileController t = island_18[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_19.Add(tc);
                    break;
                case 20:
                    canAdd = true;
                    for (var index = 0; index < island_19.Count; index++)
                    {
                        TileController t = island_19[index];
                        if (t == tc)
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                        island_20.Add(tc);
                    break;
            }
            
            //if (gm.tutorialPassed == 1)
                yield return new WaitForEndOfFrame();
        }

        Debug.Log("Start connecting islands");

        // connect islands
        if (island_1.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_1[UnityEngine.Random.Range(0, island_1.Count)];
            TileController tileToConnectPrevious = island_0[UnityEngine.Random.Range(0, island_0.Count)];

            for (var i = 0; i < island_1.Count; i++)
            {
                TileController tCur = island_1[i];
                for (var index = 0; index < island_0.Count; index++)
                {
                    TileController tPrev = island_0[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance <= distance) // found closest tiles
                    {
                        distance = newDistance;
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_2.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_2[UnityEngine.Random.Range(0, island_2.Count)];
            TileController tileToConnectPrevious = island_1[UnityEngine.Random.Range(0, island_1.Count)];

            for (var i = 0; i < island_2.Count; i++)
            {
                TileController tCur = island_2[i];
                for (var index = 0; index < island_1.Count; index++)
                {
                    TileController tPrev = island_1[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));

        }

        if (island_3.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_3[UnityEngine.Random.Range(0, island_3.Count)];
            TileController tileToConnectPrevious = island_2[UnityEngine.Random.Range(0, island_2.Count)];

            for (var i = 0; i < island_3.Count; i++)
            {
                TileController tCur = island_3[i];
                for (var index = 0; index < island_2.Count; index++)
                {
                    TileController tPrev = island_2[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_4.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_4[UnityEngine.Random.Range(0, island_4.Count)];
            TileController tileToConnectPrevious = island_3[UnityEngine.Random.Range(0, island_3.Count)];

            for (var i = 0; i < island_4.Count; i++)
            {
                TileController tCur = island_4[i];
                for (var index = 0; index < island_3.Count; index++)
                {
                    TileController tPrev = island_3[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_5.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_5[UnityEngine.Random.Range(0, island_5.Count)];
            TileController tileToConnectPrevious = island_4[UnityEngine.Random.Range(0, island_4.Count)];

            for (var i = 0; i < island_5.Count; i++)
            {
                TileController tCur = island_5[i];
                for (var index = 0; index < island_4.Count; index++)
                {
                    TileController tPrev = island_4[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_6.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_6[UnityEngine.Random.Range(0, island_6.Count)];
            TileController tileToConnectPrevious = island_5[UnityEngine.Random.Range(0, island_5.Count)];

            for (var i = 0; i < island_6.Count; i++)
            {
                TileController tCur = island_6[i];
                for (var index = 0; index < island_5.Count; index++)
                {
                    TileController tPrev = island_5[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_7.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_7[UnityEngine.Random.Range(0, island_7.Count)];
            TileController tileToConnectPrevious = island_6[UnityEngine.Random.Range(0, island_6.Count)];

            for (var i = 0; i < island_7.Count; i++)
            {
                TileController tCur = island_7[i];
                for (var index = 0; index < island_6.Count; index++)
                {
                    TileController tPrev = island_6[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_8.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_8[UnityEngine.Random.Range(0, island_8.Count)];
            TileController tileToConnectPrevious = island_7[UnityEngine.Random.Range(0, island_7.Count)];

            for (var i = 0; i < island_8.Count; i++)
            {
                TileController tCur = island_8[i];
                for (var index = 0; index < island_7.Count; index++)
                {
                    TileController tPrev = island_7[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_9.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_9[UnityEngine.Random.Range(0, island_9.Count)];
            TileController tileToConnectPrevious = island_8[UnityEngine.Random.Range(0, island_8.Count)];

            for (var i = 0; i < island_9.Count; i++)
            {
                TileController tCur = island_9[i];
                for (var index = 0; index < island_8.Count; index++)
                {
                    TileController tPrev = island_8[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_10.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_10[UnityEngine.Random.Range(0, island_10.Count)];
            TileController tileToConnectPrevious = island_9[UnityEngine.Random.Range(0, island_9.Count)];

            for (var i = 0; i < island_10.Count; i++)
            {
                TileController tCur = island_10[i];
                for (var index = 0; index < island_9.Count; index++)
                {
                    TileController tPrev = island_9[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_11.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_11[UnityEngine.Random.Range(0, island_11.Count)];
            TileController tileToConnectPrevious = island_10[UnityEngine.Random.Range(0, island_10.Count)];

            for (var i = 0; i < island_11.Count; i++)
            {
                TileController tCur = island_11[i];
                for (var index = 0; index < island_10.Count; index++)
                {
                    TileController tPrev = island_10[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        if (island_12.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_12[UnityEngine.Random.Range(0, island_12.Count)];
            TileController tileToConnectPrevious = island_11[UnityEngine.Random.Range(0, island_11.Count)];

            for (var i = 0; i < island_12.Count; i++)
            {
                TileController tCur = island_12[i];
                for (var index = 0; index < island_11.Count; index++)
                {
                    TileController tPrev = island_11[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }
        if (island_13.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_13[UnityEngine.Random.Range(0, island_13.Count)];
            TileController tileToConnectPrevious = island_12[UnityEngine.Random.Range(0, island_12.Count)];

            for (var i = 0; i < island_13.Count; i++)
            {
                TileController tCur = island_13[i];
                for (var index = 0; index < island_12.Count; index++)
                {
                    TileController tPrev = island_12[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }
        if (island_14.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_14[UnityEngine.Random.Range(0, island_14.Count)];
            TileController tileToConnectPrevious = island_13[UnityEngine.Random.Range(0, island_13.Count)];

            for (var i = 0; i < island_14.Count; i++)
            {
                TileController tCur = island_14[i];
                for (var index = 0; index < island_13.Count; index++)
                {
                    TileController tPrev = island_13[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }
        if (island_15.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_15[UnityEngine.Random.Range(0, island_15.Count)];
            TileController tileToConnectPrevious = island_14[UnityEngine.Random.Range(0, island_14.Count)];

            for (var i = 0; i < island_15.Count; i++)
            {
                TileController tCur = island_15[i];
                for (var index = 0; index < island_14.Count; index++)
                {
                    TileController tPrev = island_14[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }
        if (island_16.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_16[UnityEngine.Random.Range(0, island_16.Count)];
            TileController tileToConnectPrevious = island_15[UnityEngine.Random.Range(0, island_15.Count)];

            for (var i = 0; i < island_16.Count; i++)
            {
                TileController tCur = island_16[i];
                for (var index = 0; index < island_15.Count; index++)
                {
                    TileController tPrev = island_15[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }
        if (island_17.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_17[UnityEngine.Random.Range(0, island_17.Count)];
            TileController tileToConnectPrevious = island_16[UnityEngine.Random.Range(0, island_16.Count)];

            for (var i = 0; i < island_17.Count; i++)
            {
                TileController tCur = island_17[i];
                for (var index = 0; index < island_16.Count; index++)
                {
                    TileController tPrev = island_16[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }
        if (island_18.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_18[UnityEngine.Random.Range(0, island_18.Count)];
            TileController tileToConnectPrevious = island_17[UnityEngine.Random.Range(0, island_17.Count)];

            for (var i = 0; i < island_18.Count; i++)
            {
                TileController tCur = island_18[i];
                for (var index = 0; index < island_17.Count; index++)
                {
                    TileController tPrev = island_17[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }
        if (island_19.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_19[UnityEngine.Random.Range(0, island_19.Count)];
            TileController tileToConnectPrevious = island_18[UnityEngine.Random.Range(0, island_18.Count)];

            for (var i = 0; i < island_19.Count; i++)
            {
                TileController tCur = island_19[i];
                for (var index = 0; index < island_18.Count; index++)
                {
                    TileController tPrev = island_18[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }
        if (island_20.Count > 0)
        {
            coridorIndexCurrent++;
            steps = 0;
            float distance = 1000;
            TileController tileToConnectCurrent = island_20[UnityEngine.Random.Range(0, island_20.Count)];
            TileController tileToConnectPrevious = island_19[UnityEngine.Random.Range(0, island_19.Count)];

            for (var i = 0; i < island_20.Count; i++)
            {
                TileController tCur = island_20[i];
                for (var index = 0; index < island_19.Count; index++)
                {
                    TileController tPrev = island_19[index];
                    float newDistance = Vector3.Distance(tCur.transform.position, tPrev.transform.position);
                    if (newDistance < distance) // found closest tiles
                    {
                        tileToConnectCurrent = tCur;
                        tileToConnectPrevious = tPrev;
                    }
                }
            }

            yield return StartCoroutine(ConnectTwoTiles(tileToConnectCurrent, tileToConnectPrevious));
        }

        while (disconnectedTiles.Count > 0)
        {
            yield return null;
        }
        print("All islands connected.");

        StartCoroutine(ConnectDeadEnds());
        //StartCoroutine(SpawnWalls());
    }

    IEnumerator ConnectTwoTiles(TileController firstTile, TileController secondTile)
    {
        bool connected = false;
        Vector3 lastTilePos = firstTile.transform.position;
        while (!connected)
        {
            if (lastTilePos.z < secondTile.transform.position.z)
            {
                // move forward
                lastTilePos += Vector3.forward * tileSize;
                SpawnTile(lastTilePos, currentMainTile, null, StatusEffects.StatusEffect.Null, true, coridorIndexCurrent, true, false);
            }
            else if (lastTilePos.z > secondTile.transform.position.z)
            {
                // move back
                lastTilePos += Vector3.back * tileSize;
                SpawnTile(lastTilePos, currentMainTile, null, StatusEffects.StatusEffect.Null, true, coridorIndexCurrent, true, false);
            }
            else if (lastTilePos.x < secondTile.transform.position.x)
            {
                // move right
                lastTilePos += Vector3.right * tileSize;
                SpawnTile(lastTilePos, currentMainTile, null, StatusEffects.StatusEffect.Null, true, coridorIndexCurrent, true, false);
            }
            else if (lastTilePos.x > secondTile.transform.position.x)
            {
                // move left
                lastTilePos += Vector3.left * tileSize;
                SpawnTile(lastTilePos, currentMainTile, null, StatusEffects.StatusEffect.Null, true, coridorIndexCurrent, true, false);
            }
            else connected = true;

            //if (gm.tutorialPassed == 1)
                yield return new WaitForEndOfFrame();
            
        }
        yield return null;
    }

    IEnumerator ConnectDeadEnds()
    {
        print("connect dead ends");
        // create dead ends
        levelTree.InitializeTree(levelTilesInGame, roomsInGame, tileSize);

        List<TileController> deadEndsTemp = new List<TileController>();

        for (var index = 0; index < levelTree.deadEnds.Count; index++)
        {
            var deadEnd = levelTree.deadEnds[index];
            deadEndsTemp.Add(deadEnd.value);
        }

        for (int i = deadEndsTemp.Count - 1; i > 0; i--)
        {
            yield return StartCoroutine(ConnectTwoTiles(deadEndsTemp[i], deadEndsTemp[0]));
        }


        //StartCoroutine(ClearDeadEnds());
        
        RandomCelling();
    }

    /*
    IEnumerator ClearDeadEnds()
    {
        print("Clear dead ends.");
        for (int i = levelTilesInGame.Count - 1; i >= 0; i--)
        {
            levelTilesInGame[i].ClearDeadEnd();
        }

        yield return new WaitForEndOfFrame();

        for (int i = levelTilesInGame.Count - 1; i >= 0; i--)
        {
            levelTilesInGame[i].ClearDeadEnd();
        }

        yield return new WaitForEndOfFrame();

        for (int i = levelTilesInGame.Count - 1; i >= 0; i--)
        {
            levelTilesInGame[i].ClearDeadEnd();
        }

        yield return new WaitForEndOfFrame();

        for (int i = levelTilesInGame.Count - 1; i >= 0; i--)
        {
            levelTilesInGame[i].ClearDeadEnd();
        }

        yield return new WaitForEndOfFrame();

        for (int i = levelTilesInGame.Count - 1; i >= 0; i--)
        {
            levelTilesInGame[i].ClearDeadEnd();
        }

        yield return new WaitForEndOfFrame();

        StartCoroutine(SpawnWalls());
    }
    */
    
    IEnumerator SpawnWalls()
    {
        for (int i = 0; i < levelTilesInGame.Count; i++)
        {
            levelTilesInGame[i].GetNeighbours();
            //if (gm.tutorialPassed == 1)
                yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < levelTilesInGame.Count; i++)
        {
            float trapForwardR = Random.value;
            float blockerForwardR = Random.value;
            float doorForwardR = Random.value;
            float trapRightR  = Random.value;
            float blockerRightR  = Random.value;
            float doorRightR  = Random.value;
            float trapBackR = Random.value;
            float blockerBackR = Random.value;
            float doorBackR = Random.value;
            float trapLeftR = Random.value;
            float blockerLeftR = Random.value; 
            float doorLeftR = Random.value;

            
            levelTilesInGame[i].CreateWallsBetweenTiles(trapForwardR, blockerForwardR, doorForwardR, trapRightR, blockerRightR, doorRightR, trapBackR, blockerBackR, doorBackR, trapLeftR, blockerLeftR, doorLeftR);   
            
            /*
            if (levelgenOnHost)
            {
                // COOP
                GLNetworkWrapper.instance.CreateWallsBetweenTiles(i, trapForwardR, blockerForwardR, doorForwardR, trapRightR, blockerRightR, doorRightR, trapBackR, blockerBackR, doorBackR, trapLeftR, blockerLeftR, doorLeftR);
            }
            else if (GLNetworkWrapper.instance == null || !GLNetworkWrapper.instance.coopIsActive)
            {
                levelTilesInGame[i].CreateWallsBetweenTiles(trapForwardR, blockerForwardR, doorForwardR, trapRightR, blockerRightR, doorRightR, trapBackR, blockerBackR, doorBackR, trapLeftR, blockerLeftR, doorLeftR);   
            }
            */
            yield return new WaitForEndOfFrame();
        }
        
        yield return new WaitForEndOfFrame();

        StartCoroutine(SpawnDoors());
    }

    /*
    // this called on client by Player Network Object
    public void CreateWallsBetweenTilesOnClient(int index, float trapForwardR, float blockerForwardR, float doorForwardR,
        float trapRightR, float blockerRightR, float doorRightR,
        float trapBackR, float blockerBackR, float doorBackR,
        float trapLeftR, float blockerLeftR, float doorLeftR)
    {
        levelTilesInGame[index].GetNeighbours();
        levelTilesInGame[index].CreateWallsBetweenTiles(trapForwardR, blockerForwardR, doorForwardR, trapRightR, blockerRightR, doorRightR, trapBackR, blockerBackR, doorBackR, trapLeftR, blockerLeftR, doorLeftR);
        levelTilesInGame[index].CreateDoor();
        levelTilesInGame[index].CreateColumns();
    }
    */

    IEnumerator SpawnDoors()
    {
        /*
        if (levelgenOnHost)
        {
            // spawning doors should be already done here
            yield return new WaitForEndOfFrame();
            GLNetworkWrapper.instance.SpawnEnvironmentTraps();
        }
        else
        {
        }
        */
        
        print("SpawnDoors");
        for (int i = 0; i < levelTilesInGame.Count; i++)
        {
            levelTilesInGame[i].CreateDoor();
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(SpawnColumns());
    }
    
    IEnumerator SpawnColumns()
    {
        // need to send RPC to clients for this
        // make it later
        /*
        print("SpawnColumns");
        for (int i = 0; i < levelTilesInGame.Count; i++)
        {
            levelTilesInGame[i].CreateColumns();
        }
        */

        yield return new WaitForEndOfFrame();

        StartCoroutine(SpawnEnvironmentTraps());
    }

    public IEnumerator SpawnEnvironmentTraps()
    {
        yield return null;
        
        if (levelgenOnHost)
        {
            GLNetworkWrapper.instance.SpawnEnvironmentTraps();
        }
        else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            yield return StartCoroutine(SpawnEnvironmentTrapsOnClient());
        }
        
    }

    public IEnumerator SpawnEnvironmentTrapsOnClient()
    {
        int difficulty = GutProgressionManager.instance.currentLevelDifficulty;

        while (roomsInGame.Count == 0)
        {
            // WAIT FOR ROOMS
            yield return new WaitForSeconds(0.1f);
        }
        
        for (int i = roomsInGame.Count - 1; i >= 1; i--)
        {
            bool hazardRoom = false;
            for (int j = roomsInGame[i].tiles.Count - 1; j > 0; j--)
            {
                if (roomsInGame[i].tiles[j].tileStatusEffect != StatusEffects.StatusEffect.Null && roomsInGame[i].tiles[j].tileStatusEffect != StatusEffects.StatusEffect.HealthRegen)
                {
                    hazardRoom = true;
                    if (difficulty > 0)
                        break;
                    else
                        roomsInGame[i].tiles[j].tileStatusEffect = StatusEffects.StatusEffect.Null;
                }
            }

            if (hazardRoom && difficulty > 0)
            {
                difficulty--;
            }
        }

        for (var index = 0; index < roomsInGame[0].tiles.Count; index++)
        {
            var t = roomsInGame[0].tiles[index];
            t.tileStatusEffect = StatusEffects.StatusEffect.Null;
        }
        
        
        if (levelgenOnHost)
        {
            SpawnTraps();
        }
        else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            SpawnTraps();
        }
    }
    
    void SpawnTraps()
    {
        print("SpawnTraps");
        List<TileController> tempTiles = new List<TileController>();
        List<TileController> tempTilesForCeilingTraps = new List<TileController>();

        for (int i = levelTilesInGame.Count - 1; i >= 0; i--)
        {
            if (levelTilesInGame[i].canSpawnTrap)
            {
                if (Vector3.Distance(Vector3.zero, levelTilesInGame[i].transform.position) > 40f)
                {
                    if (!levelTilesInGame[i].propTile && levelTilesInGame[i].floorTrapPrefab != null)
                        tempTiles.Add(levelTilesInGame[i]);

                    if (levelTilesInGame[i].ceilingTraps.Count > 0 &&
                        levelTilesInGame[i].tileStatusEffect == StatusEffects.StatusEffect.Null)
                        tempTilesForCeilingTraps.Add(levelTilesInGame[i]);
                }   
            }
        }

        for (int i = 0; i < trapsAmount; i++)
        {
            if (tempTiles.Count > 0)
            {
                int r = Random.Range(0, tempTiles.Count);
                tempTiles[r].MakeTrap();
                tempTiles.RemoveAt(r);
            }
            else
                break;
        }

        // ceiling traps
        for (int i = 0; i < ceilingTrapsAmount; i++)
        {
            if (tempTilesForCeilingTraps.Count > 0)
            {
                int r = Random.Range(0, tempTilesForCeilingTraps.Count);
                tempTilesForCeilingTraps[r].MakeCeilingTrap();

                tempTilesForCeilingTraps.RemoveAt(r);
            }
            else
                break;
        }

        CreateMaps();
    }
    
    

    void CreateMaps()
    {
        print("CreateMaps");
        for (var index = 0; index < roomsInGame.Count; index++)
        {
            RoomController r = roomsInGame[index];
            r.CreateMap();
        }

        CreateSpawners();
    }

    void CreateSpawners()
    {
        StartCoroutine(CreateSpawnersOnClient());
    }

    public IEnumerator CreateSpawnersOnClient()
    {
        print("CreateSpawnersOnClient");
        for (var index = 0; index < levelTilesInGame.Count; index++)
        {
            TileController t = levelTilesInGame[index];
            if (t.spawnerTile)
            {
                t.CreateSpawner();
            }
        }

        yield return null;
        StartCoroutine(CreateProps());
    }
    
    IEnumerator CreateProps()
    {
        if (levelgenOnHost)
        {
            while (GLNetworkWrapper.instance.playerNetworkObjects[1].tilesAmount < GLNetworkWrapper.instance.playerNetworkObjects[0].tilesAmount)
            {
                print("[COOP] Second player have missing tiles");
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        print("CreateProps In Corridors");
        for (var index = 0; index < levelTilesInGame.Count; index++)
        {
            TileController t = levelTilesInGame[index];
            if (t.corridorTile)
            {
                if (!t.trapTile)
                {
                    if (t.canSpawnProp)
                    {
                        t.SpawnProp(propsReferences);
                        sc.spawners.Remove(t.spawner);
                    }
                }
            }

            yield return null;
        }

        print("CreateProps In Rooms");
        for (int i = 0; i < roomsInGame.Count; i++)
        {
            for (var index = 0; index < roomsInGame[i].tiles.Count; index++)
            {
                var t = roomsInGame[i].tiles[index];
                if (t.canSpawnProp)
                {
                    if (i != 0)
                        t.SpawnRoomProp(roomsPropsReferences);
                    else
                        t.SpawnProp(propsReferences);

                    sc.spawners.Remove(t.spawner);
                }

                yield return null;
            }

            yield return null;
        }

        /*
        // create regen tiles
        for (int i = 0; i < newLevelData.regenTilesAmount; i++)
        {
            for (int j = levelTilesInGame.Count - 1; j >= 0; j--)
            {
                if (levelTilesInGame[j].corridorTile && levelTilesInGame[j].propOnTile == null &&
                    !levelTilesInGame[j].trapTile &&
                    levelTilesInGame[j].tileStatusEffect == StatusEffects.StatusEffect.Null)
                {
                    levelTilesInGame[j].StartStatusEffectOnTile(4, -1);
                    break;
                }
                yield return null;
            }
        }
        */

        // create vending machines
        for (int i = 0; i < newLevelData.vendingMachinesAmount; i++)
        {
            for (int j = levelTilesInGame.Count - 1; j >= 0; j--)
            {
                var t = levelTilesInGame[j]; 
                if (t.propOnTile == null &&
                    !t.trapTile &&
                    t.tileStatusEffect == StatusEffects.StatusEffect.Null &&
                    Vector3.Distance(t.transform.position, playerSpawner.transform.position) >= 50)
                {
                    if (gm.qm.activeQuestsIndexes.Contains(7))
                    {
                        var glutton = Instantiate(newLevelData.vendingMachinePrefabForCarl, t.transform.position,
                            Quaternion.identity);
                        
                        if (levelgenOnHost)
                            NetworkServer.Spawn(glutton.gameObject);
                    }
                    else
                    {
                        var glutton = Instantiate(newLevelData.vendingMachinePrefab, t.transform.position,
                            Quaternion.identity);   
                        if (levelgenOnHost)
                            NetworkServer.Spawn(glutton.gameObject);
                    }
                    break;
                }

                yield return null;
            }
        }

        yield return new WaitForEndOfFrame();
        for (var index = 0; index < levelTilesInGame.Count; index++)
        {
            var t = levelTilesInGame[index];
            t.ClearTileObjects(); // remove all unused objects in tile
            yield return new WaitForEndOfFrame();
        }
        
        if (CultGenerator.instance != null)
            CultGenerator.instance.Init();

        if (levelgenOnHost)
        {
            /*
            while (GLNetworkWrapper.instance.playerNetworkObjects[0].propsAmount < GLNetworkWrapper.instance.playerNetworkObjects[1].propsAmount)
            {
                print("[COOP] First player have missing props");
                yield return new WaitForSeconds(0.1f);
            }
            */
            GLNetworkWrapper.instance.StartLevel();
        }
        else
        {
            StartLevel();   
        }   
    }

    public TileController GetClosestTile(Vector3 pos)
    {
        TileController closestTile = null;
        float distance = 1000;
        
        for (int i = 0; i < levelTilesInGame.Count; i++)
        {
            if (levelTilesInGame[i] == null)
                continue;
            
            float newDistance = Vector3.Distance(pos, levelTilesInGame[i].transform.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                closestTile = levelTilesInGame[i];
            }
        }

        return closestTile;
    }
    
    public void StartLevel()
    {
        // IF HOST
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && levelgenOnHost)
            StartCoroutine(sc.Init(levelTree));
        else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            // IF SOLO
            StartCoroutine(sc.Init(levelTree));
        }
        
        if (levelgenOnHost)
        {
            GLNetworkWrapper.instance.ActivateSavedRandomObjectsActivators();
        }
        
        var fc = FlashbackController.instance;
        if (fc)
        {
            fc.ActivateTransitionEventBeforeStartTheLevel();
        }
        
        generating = false;
        AiDirector.instance.Init();
    }

    public IEnumerator ActivateSavedRandomObjectsActivatorsOnClient()
    {
        for (int i = randomObjectsActivators.Count - 1; i >= 0; i--)
        {
            if (randomObjectsActivators[i] != null)
            {
                while (randomObjectsActivators[i].savedObject == null)
                {
                    print("WAIT UNTIL OBJECT GOT THE SAVED OBJECT");
                    yield return new WaitForSeconds(0.1f);
                }
                randomObjectsActivators[i].ActivateRandomObjectOnClient();   
            }
        }
    }
}

[Serializable]
public class Materials
{
    public List<Material> walls;
    public List<Material> wallsBroken;
    public List<Material> columns;
    public List<Material> doorways;
}
