using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FirstGearGames.Mirrors.Assets.FlexNetworkAnimators;
using Mirror;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class TileController : MonoBehaviour
{
    [Header("0 - office, 1 - noCeiling, 2 - cage, 3 - frost cage, 4 - bar, 5 - meat bar, 6 - cabin. 7 - city bricks, 8 - city panels, 9 - city wire")]
    public RoomController masterRoom;
    public int tileIndex = 0;
    public bool corridorTile = false;
    public int corridorIndex = 0;
    public bool trapTile = false;
    public bool spawnerTile = false;
    public bool propTile = false;
    public bool canSpawnProp = true;
    public bool canSpawnTrap = true;

    
    public List<GameObject> cellingTiles;
    public List<GameObject> floorTiles;
    public List<GameObject> ceilingTraps;
    public List<GameObject> columns; // 0 - FL, 1 - FR, 2 - BR, 3 - BL
    
    /*
    public List<GameObject> walls; // 0 - forward; 1 - right; 2 - back; 3 - left;
    public List<GameObject> wallsBroken; // 0 - forward; 1 - right; 2 - back; 3 - left;
    public List<GameObject> wallTraps; // 0 - forward; 1 - right; 2 - back; 3 - left;
    public List<GameObject> wallBlockers; // 0 - forward; 1 - right; 2 - back; 3 - left;
    */
    
    public DoorController door;// 0 - forward; 1 - right; 2 - back; 3 - left;
    public List<SetWallsColor> changeColors;
    
    [Header("Map")]
    public GameObject map;

    public HealthController mapPrefab;
    public Vector3 mapObjectLocalPos;
    public Quaternion mapObjectLocalRot;

    [ContextMenu("GetMapTransform")]
    void GetMapTransform()
    {
        var _map = map.transform.GetChild(0);
        mapObjectLocalPos = _map.localPosition;
        mapObjectLocalRot = _map.localRotation;
    }
    
    private bool mapIsActive = false;
    public PropController propOnTile;
    GameObject doorParent;
    public Spawner spawner;

    [Header("Prefabs")] 
    public GameObject wallPrefab;
    public GameObject brokenWallPrefab;
    public GameObject wallBlockerPrefab;
    public GameObject wallTrapPrefab;
    public GameObject floorTrapPrefab;

    [Header("Tools")]
    public int wallsAmount = 0;
    public int island = 100;
    public Transform wallsParent;
    public LayerMask tileLayer;

    [Header("Add used by wall poisiton:")]
    public bool forwardUsed = false;
    public bool rightUsed = false;
    public bool backUsed = false;
    public bool leftUsed = false;

    [Header("Place door")]
    public bool forwardDoor = false;
    public bool rightDoor = false;
    public bool backDoor = false;
    public bool leftDoor = false;
    public LayerMask doorMask;
    private bool canHaveDoor = true;

    [Header("Neighbour tiles")] 
    public bool neighboursChecked = false;
    public TileController neighbourForward;
    public TileController neighbourRight;
    public TileController neighbourBack;
    public TileController neighbourLeft;

    [Header("HealthController walls")] 
    public HealthController wallForwardHc;
    public HealthController wallRightHc;
    public HealthController wallBackHc;
    public HealthController wallLeftHc;
    
    [Header("Gameplay")] 
    public StatusEffects.StatusEffect tileStatusEffect = StatusEffects.StatusEffect.Null;

    public List<EffectVisualController> statusEffectsVisuals;

    [Header("0 - wood, 1 - metal, 2 - tiles, 3 - snow, 4 - squeeze wood")]
    public int floorType = 0;
    
    [Header("Read only. Actual physical walls")]
    public bool wallFront = false;
    public bool wallRight = false;
    public bool wallBack = false;
    public bool wallLeft = false;

    private Coroutine endEffectOverTime;
    private Coroutine checkEffectsCoroutine;
    
    private AsyncOperationHandle<GameObject> _asyncOperationHandle;
    LevelGenerator lg;

    void Awake()
    {
        lg = LevelGenerator.instance;

        // ADD TO DATABASE
        lg.levelTilesInGame.Add(this);
        doorParent = door.transform.parent.parent.gameObject;

        StartCoroutine(CheckEffect());

        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.AddTile();
        }
    }

    
    IEnumerator CheckEffect()
    {
        while (gameObject.activeInHierarchy)
        {
            if (tileStatusEffect == StatusEffects.StatusEffect.Null)
            {
                for (int i = 0; i < statusEffectsVisuals.Count; i++)
                {
                    if (statusEffectsVisuals[i].gameObject.activeInHierarchy)
                        statusEffectsVisuals[i].DestroyEffect(false);
                    
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                switch (tileStatusEffect)
                {
                    case StatusEffects.StatusEffect.Poison:
                        statusEffectsVisuals[0].gameObject.SetActive(true);
                        statusEffectsVisuals[0].StartEffect();
                        break;
            
                    case StatusEffects.StatusEffect.Fire:
                        statusEffectsVisuals[1].gameObject.SetActive(true);
                        statusEffectsVisuals[1].StartEffect();
                        break;
            
                    case StatusEffects.StatusEffect.Bleed:
                        statusEffectsVisuals[2].gameObject.SetActive(true);
                        statusEffectsVisuals[2].StartEffect();
                        break;
            
                    case StatusEffects.StatusEffect.Rust:
                        statusEffectsVisuals[3].gameObject.SetActive(true);
                        statusEffectsVisuals[3].StartEffect();
                        break;
            
                    case StatusEffects.StatusEffect.HealthRegen:
                        statusEffectsVisuals[4].gameObject.SetActive(true);
                        statusEffectsVisuals[4].StartEffect();
                        break;
            
                    case StatusEffects.StatusEffect.GoldHunger:
                        statusEffectsVisuals[5].gameObject.SetActive(true);
                        statusEffectsVisuals[5].StartEffect();
                        break;
            
                    case StatusEffects.StatusEffect.Cold:
                        statusEffectsVisuals[6].gameObject.SetActive(true);
                        statusEffectsVisuals[6].StartEffect();
                        break;
                    
                    case StatusEffects.StatusEffect.InLove:
                        statusEffectsVisuals[7].gameObject.SetActive(true);
                        statusEffectsVisuals[7].StartEffect();
                        break;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void Init(RoomController _masterRoom)
    {
        if (!lg)
            lg = LevelGenerator.instance;
        lg.usedPositions.Add(transform.position);
        lg.blockersPositions.Add(transform.position);
        RemoveAvailable(transform.position);

        if (_masterRoom != null) masterRoom = _masterRoom;
        
        // RANDOM TILE
        int r = UnityEngine.Random.Range(0, floorTiles.Count);

        for (int i = 0; i < floorTiles.Count; i++)
        {
            if (i == r)
                floorTiles[i].SetActive(true);
            else
                floorTiles[i].SetActive(false);
        }

        if (masterRoom)
            AddUsedPositions();

        // reset tile rotation
        wallsParent.transform.parent = null;
        wallsParent.rotation = Quaternion.identity;
        wallsParent.transform.parent = transform;
        
        //print( gameObject.name+ " " + tileStatusEffect);
        switch (tileStatusEffect)
        {
            case StatusEffects.StatusEffect.Poison:
                statusEffectsVisuals[0].gameObject.SetActive(true);
                statusEffectsVisuals[0].StartEffect();
                break;
            
            case StatusEffects.StatusEffect.Fire:
                statusEffectsVisuals[1].gameObject.SetActive(true);
                statusEffectsVisuals[1].StartEffect();
                break;
            
            case StatusEffects.StatusEffect.Bleed:
                statusEffectsVisuals[2].gameObject.SetActive(true);
                statusEffectsVisuals[2].StartEffect();
                break;
            
            case StatusEffects.StatusEffect.Rust:
                statusEffectsVisuals[3].gameObject.SetActive(true);
                statusEffectsVisuals[3].StartEffect();
                break;
            
            case StatusEffects.StatusEffect.HealthRegen:
                statusEffectsVisuals[4].gameObject.SetActive(true);
                statusEffectsVisuals[4].StartEffect();
                break;
            
            case StatusEffects.StatusEffect.GoldHunger:
                statusEffectsVisuals[5].gameObject.SetActive(true);
                statusEffectsVisuals[5].StartEffect();
                break;
            
            case StatusEffects.StatusEffect.Cold:
                statusEffectsVisuals[6].gameObject.SetActive(true);
                statusEffectsVisuals[6].StartEffect();
                break;
            
            case StatusEffects.StatusEffect.InLove:
                statusEffectsVisuals[7].gameObject.SetActive(true);
                statusEffectsVisuals[7].StartEffect();
                break;
        }

        if (tileIndex == -1 && lg.levelgenOnHost) // meatTile
            StartCoroutine(CheckForMissedWalls());
    }

    IEnumerator CheckForMissedWalls()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            RestoreWalls();
        }
    }

    void RestoreWalls()
    {
        if (neighbourForward == null && wallForwardHc == null)
        {
            SpawnInsideTile(0,0);
        }
        if (neighbourRight == null && wallRightHc == null)
        {
            SpawnInsideTile(0,1);
        }
        if (neighbourBack == null && wallBackHc == null)
        {
            SpawnInsideTile(0,2);
        }
        if (neighbourLeft == null && wallLeftHc == null)
        {
            SpawnInsideTile(0,3);
        }
    }

    public void RandomCelling(float r)
    {
        //Random celling
        if (cellingTiles.Count > 1)
        {
            if (r <= GameManager.instance.level.lightRate)
            {
                cellingTiles[0].SetActive(false);
                // LIGHT
                cellingTiles[1].SetActive(true); // activate lamp   
            }
            else
            {
                cellingTiles[0].SetActive(true);
            }
        }
        else if (cellingTiles.Count == 1)
            cellingTiles[0].SetActive(true);
    }

    void RemoveAvailable(Vector3 tilePosition)
    {
        for (int i = lg.availablePositions.Count - 1; i >= 0; i--)
        {
            if (lg.availablePositions[i] == tilePosition)
            {
                lg.availablePositions.RemoveAt(i);
                break;
            }
        }
    }

    void AddUsedPositions()
    {
        // get fillers from master room and see if there should be walls

        bool f = true;
        bool r = true;
        bool b = true;
        bool l = true;
        
        for (int i = 0; i < masterRoom.fillers.Count; i++)
        {
            if (Vector3.Distance(transform.position, masterRoom.fillers[i].transform.position) < 7) // this filler is neighbour
            {
                if (masterRoom.fillers[i].transform.position.z > transform.position.z + 0.5f)
                    f = false;
                if (masterRoom.fillers[i].transform.position.x > transform.position.x + 0.5f)
                    r = false;
                if (masterRoom.fillers[i].transform.position.z < transform.position.z - 0.5f)
                    b = false;
                if (masterRoom.fillers[i].transform.position.x < transform.position.x - 0.5f)
                    l = false;
            }
        }

        forwardUsed = f;
        rightUsed = r;
        backUsed = b;
        leftUsed = l;
        
        if (forwardUsed)
        {
            //Vector3 newPos = transform.position + transform.parent.parent.transform.forward * lg.tileSize;
            Vector3 newPos = transform.position + transform.forward * lg.tileSize;
            lg.blockersPositions.Add(newPos);
            lg.usedPositions.Add(newPos);
            RemoveAvailable(newPos);
        }
        if (rightUsed)
        {
            //Vector3 newPos = transform.position + transform.parent.parent.transform.right * lg.tileSize;
            Vector3 newPos = transform.position + transform.right * lg.tileSize;
            lg.blockersPositions.Add(newPos);
            lg.usedPositions.Add(newPos);
            RemoveAvailable(newPos);
        }
        if (backUsed)
        {
            //Vector3 newPos = transform.position + transform.parent.parent.transform.forward * -1 * lg.tileSize;
            Vector3 newPos = transform.position + transform.forward * -1 * lg.tileSize;
            lg.blockersPositions.Add(newPos);
            lg.usedPositions.Add(newPos);
            RemoveAvailable(newPos);
        }
        if (leftUsed)
        {
            //Vector3 newPos = transform.position + transform.parent.parent.transform.right * -1 * lg.tileSize;
            Vector3 newPos = transform.position + transform.right * -1 * lg.tileSize;
            lg.blockersPositions.Add(newPos);
            lg.usedPositions.Add(newPos);
            RemoveAvailable(newPos);
        }
    }

    public void CorridorTile()
    {
        corridorTile = true;
    }

    public void CreateSpawner()
    {
        if (spawnerTile && !propTile)
        {
            spawner.gameObject.SetActive(true);

            if (corridorTile)
                spawner.corridor = true;
        }
    }

    /*
    public void ClearDeadEnd()
    {
        if (corridorTile)
        {
            wallsAmount = 0;
            Vector3 raycastOrigin = transform.position + Vector3.up * 20 + Vector3.forward * lg.tileSize;
            for (int i = 0; i < 4; i++)
            {
                bool empty = true;
                if (Physics.Raycast(raycastOrigin, Vector3.down * 20, tileLayer))
                {
                    empty = false;
                }

                switch (i)
                {
                    case 0:
                        if (empty)
                        {
                            wallFront = true;
                            wallsAmount++;
                        }
                        raycastOrigin = transform.position + Vector3.up * 20 + Vector3.right * lg.tileSize; // new origin
                        break;
                    case 1:
                        if (empty)
                        {
                            wallRight = true;
                            wallsAmount++;
                        }
                        raycastOrigin = transform.position + Vector3.up * 20 + Vector3.back * lg.tileSize;
                        break;
                    case 2:
                        if (empty)
                        {
                            wallBack = true;
                            wallsAmount++;
                        }
                        raycastOrigin = transform.position + Vector3.up * 20 + Vector3.left * lg.tileSize;
                        break;
                    case 3:
                        if (empty)
                        {
                            wallLeft = true;
                            wallsAmount++;
                        }
                        break;
                }
            }
            if (wallsAmount > 2)
            {
                lg.levelTilesInGame.Remove(this);
                Destroy(gameObject);
            }
        }
    }
    */

    public void GetNeighbours()
    {
        Vector3 raycastOrigin = transform.position + Vector3.forward * lg.tileSize; // new origin
        var sc = SpawnController.instance;
        for (int i = sc.doorsInGame.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(transform.position, sc.doorsInGame[i].transform.position) <= lg.tileSize + 1)
            {
                canHaveDoor = false;
                break;
            }   
        }
        
        for (int i = 0; i < 4; i++)
        {
            bool empty = true;
            TileController newNeighbour = null;
            
            for (int j = lg.levelTilesInGame.Count - 1; j >= 0; j--)
            {
                if (Vector3.Distance(raycastOrigin, lg.levelTilesInGame[j].transform.position) <= 1)
                {
                    empty = false;
                    newNeighbour = lg.levelTilesInGame[j];
                    break;
                }
            }

            switch (i)
            {
                case 0:
                    if (empty)
                    {
                        if (wallFront == false)
                        {
                            wallsAmount++;
                            SpawnInsideTile(0, 0);
                        }

                        wallFront = true;
                        //walls[0].SetActive(true);
                    }
                    else
                    {
                        neighbourForward = newNeighbour;
                        if (GetUsed(neighbourForward))
                            forwardUsed = true;
                        else 
                            forwardUsed = false;
                    }

                    raycastOrigin = transform.position + Vector3.right * lg.tileSize; // new origin
                break;

                case 1:
                    if (empty)
                    {
                        if (wallRight == false)
                        {
                            wallsAmount++;
                            SpawnInsideTile(0, 1);
                        }

                        wallRight = true;
                        //walls[1].SetActive(true);
                    }
                    else
                    {
                        neighbourRight = newNeighbour;
                        if (GetUsed(neighbourRight))
                            rightUsed = true;
                        else
                            rightUsed = false;
                    }
                    
                    raycastOrigin = transform.position + Vector3.back * lg.tileSize;
                break;

                case 2:
                    if (empty)
                    {
                        if (wallBack == false)
                        {
                            wallsAmount++;
                            SpawnInsideTile(0, 2);
                        }
                        
                        wallBack = true;
                    }
                    else
                    {
                        neighbourBack = newNeighbour;
                        if (GetUsed(neighbourBack))
                            backUsed = true;
                        else
                            backUsed = false;
                    }
                    
                    raycastOrigin = transform.position + Vector3.left * lg.tileSize;
                break;

                case 3:
                    if (empty)
                    {
                        if (wallLeft == false)
                        {
                            wallsAmount++;
                            SpawnInsideTile(0, 3);
                        }
                        wallLeft = true;
                    }
                    else
                    {
                        neighbourLeft = newNeighbour;
                        if (GetUsed(neighbourLeft))
                            leftUsed = true;
                        else
                            leftUsed = false;
                    }
                    break;
            }
        }
    }

    void SpawnInsideTile(int prefabIndex, int side)
    {
        if (lg.levelgenOnHost)
        {
            if (prefabIndex == 1 || prefabIndex == 4) // broken wall. This one has no network ID. No need to sync them after spawn
            {
                int _tileIndex = lg.levelTilesInGame.IndexOf(this);
                GLNetworkWrapper.instance.SpawnInsideTileOnClient(_tileIndex, prefabIndex, side);   
            }
            else
            {
                // SPAWN ON SERVER
                SpawnInsideTileOnClient(prefabIndex, side, true);
            }
        }
        else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            SpawnInsideTileOnClient(prefabIndex, side, false);
        }   
    }

    public void SpawnInsideTileOnClient(int prefabIndex, int side, bool spawnOnServer)
    {
        GameObject newPrefab;
        if (prefabIndex != 4) // if not door
        {
            GameObject prefab;
            switch (prefabIndex)
            {
                case 0:
                    prefab = wallPrefab;
                    break;
                case 1:
                    prefab = brokenWallPrefab;
                    break;
                case 2:
                    prefab = wallTrapPrefab;
                    break;
                case 3:
                    prefab = wallBlockerPrefab;
                    break;

                default:
                    prefab = brokenWallPrefab;
                    break;
            }
        
            newPrefab = Instantiate(prefab, transform.position, Quaternion.identity);
            newPrefab.transform.parent = wallsParent;
            // двери перемещать не нужно, у них пивот в центре тайла
            switch (side)
            {
                case 0:
                    newPrefab.transform.localPosition = new Vector3(0, 0, lg.tileSize / 2);
                    if (prefabIndex == 0)
                    {
                        wallForwardHc = newPrefab.GetComponent<HealthController>();
                        wallForwardHc.wallMasterTile = this;
                    }
                    break;
            
                case 1:
                    newPrefab.transform.localPosition = new Vector3(lg.tileSize / 2, 0, 0);
                    if (prefabIndex == 0)
                    {
                        wallRightHc = newPrefab.GetComponent<HealthController>();  
                        wallRightHc.wallMasterTile = this; 
                    }
                    break;
            
                case 2:
                    newPrefab.transform.localPosition = new Vector3(0, 0, lg.tileSize / 2 * -1);
                    if (prefabIndex == 0)
                    {
                        wallBackHc = newPrefab.GetComponent<HealthController>();   
                        wallBackHc.wallMasterTile = this; 
                    }
                    break;
            
                case 3:
                    newPrefab.transform.localPosition = new Vector3(lg.tileSize / 2 * -1, 0, 0);
                    if (prefabIndex == 0)
                    {
                        wallLeftHc = newPrefab.GetComponent<HealthController>();   
                        wallLeftHc.wallMasterTile = this; 
                    }
                    break;
            }
        }
        else
        {
            newPrefab = doorParent;
            doorParent.gameObject.SetActive(true);
            SpawnController.instance.doorsInGame.Add(doorParent);
        }
        
        newPrefab.transform.Rotate(Vector3.up, 90 * side);
        if (spawnOnServer)
        {
            print("[COOP] NetworkServer.Spawn(newPrefab.gameObject);");
            NetworkServer.Spawn(newPrefab.gameObject);   
        }
    }

    public void CreateTileBehindBrokenWall(HealthController wallHc)
    {
        // this called ON HOST or SOLO
        // spawn tile
        print("Tile Controller / CreateTileBehindBrokenWall " + wallHc.gameObject);
        
        GetNeighbours();
        
        if (wallHc == wallForwardHc && neighbourForward == null)
        {
            wallFront = false;
            forwardUsed = true;
            /*
            var newTile = lg.SpawnTileOnClient(transform.position + Vector3.forward * lg.tileSize, -1, -1, -1, false, -1, false);
            newTile.GetNeighbours();
            AiDirector.instance.NewMeatTile(newTile);
            newTile.CreateWallsBetweenTiles(0,0,0,0,0,0,0,0,0,0,0,0);
            */
            lg.SpawnTile(transform.position + Vector3.forward * lg.tileSize, -1, null, StatusEffects.StatusEffect.Null, false, -1, true, false);
            
        }
        else if (wallHc == wallRightHc && neighbourRight == null)
        {
            wallRight = false;
            rightUsed = true;
            /*
            var newTile = lg.SpawnTileOnClient(transform.position + Vector3.right * lg.tileSize, -1, -1, -1, false, -1, false);
            newTile.GetNeighbours();
            AiDirector.instance.NewMeatTile(newTile);
            newTile.CreateWallsBetweenTiles(0,0,0,0,0,0,0,0,0,0,0,0);
            */
            lg.SpawnTile(transform.position + Vector3.right * lg.tileSize, -1, null, StatusEffects.StatusEffect.Null, false, -1, true, false);
        }
        else if (wallHc == wallBackHc && neighbourBack == null)
        {
            wallBack = false;
            backUsed = true;
            /*
            var newTile = lg.SpawnTileOnClient(transform.position + Vector3.forward * -lg.tileSize, -1, -1, -1, false, -1, false);
            newTile.GetNeighbours();
            AiDirector.instance.NewMeatTile(newTile);
            newTile.CreateWallsBetweenTiles(0,0,0,0,0,0,0,0,0,0,0,0);
            */
            lg.SpawnTile(transform.position + Vector3.back * lg.tileSize, -1, null, StatusEffects.StatusEffect.Null, false, -1, true, false);
        }
        else if (wallHc == wallLeftHc && neighbourLeft == null)
        {
            wallLeft = false;
            leftUsed = true;
            /*
            var newTile = lg.SpawnTileOnClient(transform.position + Vector3.right * - lg.tileSize, -1, -1, -1, false, -1, false);
            newTile.GetNeighbours();
            AiDirector.instance.NewMeatTile(newTile);
            newTile.CreateWallsBetweenTiles(0,0,0,0,0,0,0,0,0,0,0,0);
            */
            lg.SpawnTile(transform.position + Vector3.left * lg.tileSize, -1, null, StatusEffects.StatusEffect.Null, false, -1, true, false);
        }

        // remove prop if its wall is broken
        if (propOnTile != null && propOnTile.wallHc == wallHc)
        {
            RemoveProp();
        }
    }
    
    public void CreateWallsBetweenTiles(float trapForwardR, float blockerForwardR, float doorForwardR,
                                        float trapRightR, float blockerRightR, float doorRightR,
                                        float trapBackR, float blockerBackR, float doorBackR,
                                        float trapLeftR, float blockerLeftR, float doorLeftR)
    {
        if (!wallFront && forwardUsed && !corridorTile && neighbourForward)
        {
            if (neighbourForward.backDoor)
            {
                // nothing
            }
            else if (neighbourForward.wallBackHc != null)
            {
                neighbourForward.wallBackHc.Damage(10000, neighbourForward.wallBackHc.transform.position, neighbourForward.wallBackHc.transform.position,
                    null, null, false, null, null, null, true);
            }
            else if (lg.wallTrapsMax > 0 && trapForwardR > 0.5f)
            {
                SpawnInsideTile(1, 0);
                SpawnInsideTile(2, 0);
               
                lg.wallTrapsMax--;
            }
            else if (lg.wallBlockersMax > 0 && blockerForwardR > 0.5f)
            {
                SpawnInsideTile(1, 0);
                SpawnInsideTile(3, 0);

                lg.wallBlockersMax--;
                canSpawnTrap = false;
            }
            else
            {
                // if no doors 
                if (canHaveDoor && !forwardDoor && !rightDoor && !backDoor && !leftDoor && doorForwardR >= 0.66f)
                {
                    forwardDoor = true;
                }
                else
                {
                    SpawnInsideTile(1, 0);
                }
            }
        }
                
        if (!wallRight && rightUsed && !corridorTile && neighbourRight)
        {
            if (neighbourRight.leftDoor)
            {
                // nothing
            }
            else if (lg.wallTrapsMax > 0 && trapRightR > 0.5f)
            {
                SpawnInsideTile(1, 1);
                SpawnInsideTile(2, 1);
                
                lg.wallTrapsMax--;
            }
            else if (lg.wallBlockersMax > 0 && blockerRightR > 0.5f)
            {
                SpawnInsideTile(1, 1);
                SpawnInsideTile(3, 1);
                
                
                lg.wallBlockersMax--;
                canSpawnTrap = false;
            }
            else
            {
                // if no doors 
                if (canHaveDoor && !forwardDoor && !rightDoor && !backDoor && !leftDoor && doorRightR >= 0.66f)
                {
                    rightDoor = true;
                }
                else
                {
                    SpawnInsideTile(1, 1);
                }  
            }
        }
                
        if (!wallBack && backUsed && !corridorTile && neighbourBack)
        {
            if (neighbourBack.forwardDoor)
            {
                // nothing
            }
            else if (lg.wallTrapsMax > 0 && trapBackR > 0.5f)
            {                    
                SpawnInsideTile(1, 2);
                SpawnInsideTile(2, 2);

                lg.wallTrapsMax--;
            }
            else if (lg.wallBlockersMax > 0 && blockerBackR > 0.5f)
            {            
                SpawnInsideTile(1, 2);
                SpawnInsideTile(3, 2);

                lg.wallBlockersMax--;
                canSpawnTrap = false;
            }
            else
            {
                // if no doors 
                if (canHaveDoor && !forwardDoor && !rightDoor && !backDoor && !leftDoor && doorBackR >= 0.66f)
                {
                    backDoor = true;
                }
                else
                {
                    SpawnInsideTile(1, 2);
                }
            }
        }
        if (!wallLeft && leftUsed && !corridorTile && neighbourLeft)
        {
            if (neighbourLeft.rightDoor)
            {
                // nothing
            }
            else if (lg.wallTrapsMax > 0 && trapLeftR > 0.5f)
            {
                SpawnInsideTile(1, 3);
                SpawnInsideTile(2, 3);
                
                lg.wallTrapsMax--;
            }
            else if (lg.wallBlockersMax > 0 && blockerLeftR > 0.5f)
            {
                SpawnInsideTile(1, 3);
                SpawnInsideTile(3, 3);

                lg.wallBlockersMax--;
                canSpawnTrap = false;
            }
            // if no doors 
            else
            {
                if (canHaveDoor && !forwardDoor && !rightDoor && !backDoor && !leftDoor && doorLeftR >= 0.66f)
                {
                    leftDoor = true;
                }
                else
                {
                    SpawnInsideTile(1, 3);
                }
            }
        }
    }

    bool GetUsed(TileController tile)
    {
        if (tile.tileIndex != tileIndex || tile.corridorTile != corridorTile)
            return true;
        else
            return false;
    }

    public void CreateDoor()
    {
        var sc = SpawnController.instance;

        for (int i = sc.doorsInGame.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(transform.position, sc.doorsInGame[i].transform.position) <= lg.tileSize + 1)
            {
                canHaveDoor = false;
                break;
            }   
        }
        
        doorParent.transform.localRotation = Quaternion.identity;
        if (forwardDoor)
        {
            if (!wallFront && canHaveDoor)
            {
                SpawnInsideTile(4, 0);
            }
            else
            {
                SpawnInsideTile(1, 0);
                    //wallsBroken[0].SetActive(true);
            }
        }
        else if (rightDoor)
        {
            if (!wallRight && canHaveDoor)
            {
                // door
                SpawnInsideTile(4, 1);
            }
            else
            {
                // broken wall
                SpawnInsideTile(1, 1);
                //wallsBroken[1].SetActive(true);
            }
        }
        else if (backDoor)
        {
            if (!wallBack && canHaveDoor)
            {
                // door
                SpawnInsideTile(4, 2);
            }
            else
            {
                SpawnInsideTile(1, 2);
                //wallsBroken[2].SetActive(true);
            }
        }
        else if (leftDoor)
        {
            if (!wallLeft && canHaveDoor)
            {
                // door
                SpawnInsideTile(4, 3);
            }
            else
            {
                SpawnInsideTile(1, 3);
                //wallsBroken[3].SetActive(true);
            }
        }
    }

    public int CreateMap()
    {
        int mapIndex = -1;

        if (mapPrefab)
        {
            if (wallFront)
            {
                //map.SetActive(true);
                mapIndex = 0;
            }
            else if (wallRight)
            {
                //map.SetActive(true);
                map.transform.Rotate(Vector3.up, 90f);
                mapIndex = 1;
            }
            else if (wallBack)
            {
                //map.SetActive(true);
                map.transform.Rotate(Vector3.up, 180f);
                mapIndex = 2;
            }
            else if (wallLeft)
            {
                //map.SetActive(true);
                map.transform.Rotate(Vector3.up, 270f);
                mapIndex = 3;
            }
        }
        
        if (mapIndex >= 0)
            mapIsActive = true;
        else mapIsActive = false;
        
        return mapIndex;
    }

    public void CreateColumns()
    {
        if (columns.Count > 0)
        {
            if (wallLeft && wallFront)
                columns[0].SetActive(true);
            if (wallFront && wallRight)
                columns[1].SetActive(true);
            if (wallRight && wallBack)
                columns[2].SetActive(true);
            if (wallBack && wallLeft)
                columns[3].SetActive(true);   
        }
    }

    public int WallsAmount()
    {
        int amount = 0;

        if (wallBack) amount++;
        if (wallRight) amount++;
        if (wallLeft) amount++;
        if (wallFront) amount++;

        return amount;
    }

    public void MakeTrap()
    {
        if (LevelGenerator.instance.levelgenOnHost)
        {
            GLNetworkWrapper.instance.MakeFloorTrap(LevelGenerator.instance.levelTilesInGame.IndexOf(this));
        }
        else
        {
            MakeTrapOnClient();
        }
    }

    public void MakeTrapOnClient()
    {
        // floor trap
        for (int i = floorTiles.Count - 1; i >= 0; i--)
        {
            floorTiles[i].SetActive(false);   
        }
        
        trapTile = true;

        var trap = Instantiate(floorTrapPrefab);
        trap.transform.parent = transform;
        trap.transform.localPosition = Vector3.zero;
        trap.transform.localRotation = Quaternion.identity;
    }

    public void MakeCeilingTrap()
    {
        int r = Random.Range(0, ceilingTraps.Count);
        if (LevelGenerator.instance.levelgenOnHost)
        {
            GLNetworkWrapper.instance.MakeCeilingTrap(LevelGenerator.instance.levelTilesInGame.IndexOf(this), r);
        }
        else
        {
            MakeCeilingTrapOnClient(r);
        }
    }

    public void MakeCeilingTrapOnClient(int r)
    {
        ceilingTraps[r].SetActive(true);
    }

    public void ClearTileObjects()
    {
        if (LevelGenerator.instance.levelgenOnHost)
        {
            GLNetworkWrapper.instance.ClearTilesObjects(lg.levelTilesInGame.IndexOf(this));
        }
        else
        {
            ClearTileObjectsOnClient();
        }
        
    }
    public void ClearTileObjectsOnClient()
    {
        if (ceilingTraps.Count > 0)
        {
            for (int i = ceilingTraps.Count - 1; i >= 0; i--)
            {
                if (ceilingTraps[i] != null && ceilingTraps[i].activeInHierarchy == false)
                {
                    Destroy(ceilingTraps[i].gameObject);
                    ceilingTraps.RemoveAt(i);
                }
            }   
        }
        
        for (int i = floorTiles.Count - 1; i >= 0; i--)
        {
            if (floorTiles[i] != null && floorTiles[i].activeInHierarchy == false)
            {
                Destroy(floorTiles[i]);
                floorTiles.RemoveAt(i);
            }
        }
        
        for (int i = columns.Count - 1; i >= 0; i--)
        {
            if (columns[i] != null && columns[i].activeInHierarchy == false)
            {
                Destroy(columns[i]);
                columns.RemoveAt(i);
            }
        }
        
        for (int i = cellingTiles.Count - 1; i >= 0; i--)
        {
            if (cellingTiles[i] != null && cellingTiles[i].activeInHierarchy == false)
            {
                Destroy(cellingTiles[i]);
                cellingTiles.RemoveAt(i);
            }
        }

        if (doorParent != null && doorParent.gameObject.activeInHierarchy == false)
        {
            Destroy(doorParent);
            door = null;
        }

        if (map != null && !map.activeInHierarchy)
        {
            Destroy(map);
            map = null;
        }
    }
    
    public bool NearDoor()
    {
        var il = ItemsList.instance;
        
        for (int i = il.interactables.Count; i >= 0; i--)
        {
            if (il.interactables.Count > i && il.interactables[i].door)
            {
                if (Vector3.Distance(transform.position, il.interactables[i].transform.position) < lg.tileSize)
                    return true;
            }
        }

        return false;
    }

    public void SpawnRoomProp(List<AssetReference> roomPropsReferences)
    {
        if ((map && map.gameObject.activeInHierarchy)  || Random.value > lg.roomPropsRate)
            return;

        if (!NearDoor())
        {
            int r = Random.Range(0, roomPropsReferences.Count);
            
            /*
            if (LevelGenerator.instance.levelgenOnHost)
            {
                GLNetworkWrapper.instance.SpawnRoomProp(LevelGenerator.instance.levelTilesInGame.IndexOf(this), r);
            }
            else
            {
                StartCoroutine(SpawnRoomPropOnClient(r));
            }
            */
            
            StartCoroutine(SpawnRoomPropOnClient(r));
        }
    }

    public void SpawnProp(List<AssetReference> corridorPropsReferences)
    {
        if ((map && map.gameObject.activeInHierarchy)  || Random.value > lg.propsRate)
        {
            return;
        }
        
        if (!NearDoor())
        {
            int r = Random.Range(0, corridorPropsReferences.Count);
            /*
            if (LevelGenerator.instance.levelgenOnHost)
            {
                GLNetworkWrapper.instance.SpawnCorridorProp(LevelGenerator.instance.levelTilesInGame.IndexOf(this), r);
            }
            else
            {
                StartCoroutine(SpawnCorridorPropOnClient(r));   
            }
            */
            
            StartCoroutine(SpawnCorridorPropOnClient(r));   
        }
    }

    public IEnumerator SpawnCorridorPropOnClient(int corridorPropReferenceIndex)
    {
        while (LevelGenerator.instance.propsReferences.Count <= corridorPropReferenceIndex)
        {
            LevelGenerator.instance.UpdateCorridorPropsReferences();
            yield return new WaitForSeconds(0.1f);
        }
        SpawnPropFromPrefab(LevelGenerator.instance.propsReferences[corridorPropReferenceIndex]);
    }
    public IEnumerator SpawnRoomPropOnClient(int roomPropReferenceIndex)
    {
        while (LevelGenerator.instance.roomsPropsReferences.Count <= roomPropReferenceIndex)
        {
            LevelGenerator.instance.UpdateRoomPropsReferences();
            yield return new WaitForSeconds(0.1f);
        }
        //print("@@@ SPAWN FUCKING ROOM PROP FOR GOD'S SAKE. Index is " + roomPropReferenceIndex);
        SpawnPropFromPrefab(LevelGenerator.instance.roomsPropsReferences[roomPropReferenceIndex]);
    }
    
    void SpawnPropFromPrefab(AssetReference propRef)
    {
        //print("SPAWN SOME FUCKING SHIT ON TILE HERE");
        AssetSpawner.instance.Spawn(propRef, transform.position, AssetSpawner.ObjectType.Prop);
    }

    public void SetPropToTile(GameObject newGameObject)
    {
        PropController newProp = newGameObject.GetComponent<PropController>();
        lg.propsInGame.Add(newProp);
        //print("SET SOME FUCKING PROP TO TILE BITCH. NewProp is " + newProp);
        if (newProp)
        {
            propTile = true;
            newProp.usedTile = this;
            lg.propsInGame.Add(newProp);
            spawner.spawnedProp = newProp;
            propOnTile = newProp;

            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                GLNetworkWrapper.instance.localPlayer.AddProp();
            }

            RotateProp(newProp);   
        }
    }

    void RotateProp(PropController pc)
    {
        float angle = 0;
        if (pc.bigProp || pc.wallProp)
        {
            if (map != null && map.gameObject.activeInHierarchy)
            {
                RemoveProp();
                return;
            }
            if (wallForwardHc)
            {
                pc.wallHc = wallForwardHc;
            }
            else if (wallRightHc)
            {
                angle = 90;
                pc.wallHc = wallRightHc;
            }
            else if (wallBackHc)
            {
                angle = 180;
                pc.wallHc = wallBackHc;
            }
            else if (wallLeftHc)
            {
                angle = 270;
                pc.wallHc = wallLeftHc;
            }
            else // no walls
            {
                if (!pc.wallProp)
                {
                    angle = 10 * Random.Range(0, 36);
                }
                else // remove wall prop
                {
                    RemoveProp();
                    return;
                }
            }
        }
        else
        {
            angle =  10 * Random.Range(0, 36);
        }

        /*
        if (LevelGenerator.instance.levelgenOnHost)
        {
            GLNetworkWrapper.instance.TurnPropOnTile(lg.levelTilesInGame.IndexOf(this), angle);
        }
        else
        {
            TurnPropOnClient(angle);   
        }   
        */
        
        TurnPropOnServer(angle);   
    }

    public void TurnPropOnServer(float angle)
    {    
        propOnTile.transform.rotation = Quaternion.identity;
        propOnTile.transform.Rotate(Vector3.up, angle);
        if (lg.levelgenOnHost)
            NetworkServer.Spawn(propOnTile.gameObject);
    }

    public void RemoveProp()
    {
        RemovePropOnClient(true);
        
        /*
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive &&
            lg.levelgenOnHost)
        {
            GLNetworkWrapper.instance.RemoveProp(lg.levelTilesInGame.IndexOf(this));
        }
        else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            RemovePropOnClient(true);
        }
        */
    }

    public void RemovePropOnClient(bool destroy)
    {
        if (propOnTile != null)
        {
            lg.propsInGame.Remove(propOnTile);
            spawner.spawnedProp = null;
            
            if (destroy)
            {
                Destroy(propOnTile.gameObject);   
            }
            propOnTile = null;
            propTile = false;   
        }
    }
    
    public void MarkIsland(int isl) // get islands
    {
        island = isl;
        lg.tilesInCurrentIsland.Add(this);
        lg.disconnectedTiles.Remove(this);

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.forward * lg.tileSize + Vector3.up * 50f, Vector3.down, out hit, 100f, tileLayer))
        {
            for (int i = lg.disconnectedTiles.Count - 1; i >= 0; i--)
            {
                if (lg.disconnectedTiles[i] && hit.collider.gameObject == lg.disconnectedTiles[i].gameObject)
                {
                    neighbourForward = lg.disconnectedTiles[i];
                    lg.disconnectedTiles[i].MarkIsland(island);
                    break;
                }   
            }
        }
        if (Physics.Raycast(transform.position + Vector3.right * lg.tileSize + Vector3.up * 50f, Vector3.down, out hit, 100f, tileLayer))
        {
            for (int i = lg.disconnectedTiles.Count - 1; i >= 0; i--)
            {
                if (lg.disconnectedTiles[i] && hit.collider.gameObject == lg.disconnectedTiles[i].gameObject)
                {
                    neighbourRight = lg.disconnectedTiles[i];
                    lg.disconnectedTiles[i].MarkIsland(island);
                    break;
                }   
            }
        }
        if (Physics.Raycast(transform.position + Vector3.back * lg.tileSize + Vector3.up * 50f, Vector3.down, out hit, 100f, tileLayer))
        {
            for (int i = lg.disconnectedTiles.Count - 1; i >= 0; i--)
            {
                if (lg.disconnectedTiles[i] && hit.collider.gameObject == lg.disconnectedTiles[i].gameObject)
                {
                    neighbourBack = lg.disconnectedTiles[i];
                    lg.disconnectedTiles[i].MarkIsland(island);
                    break;
                }   
            }
        }
        if (Physics.Raycast(transform.position + Vector3.left * lg.tileSize + Vector3.up * 50f, Vector3.down, out hit, 100f, tileLayer))
        {
            for (int i = lg.disconnectedTiles.Count - 1; i >= 0; i--)
            {
                if (lg.disconnectedTiles[i] && hit.collider.gameObject == lg.disconnectedTiles[i].gameObject)
                {
                    neighbourLeft = lg.disconnectedTiles[i];
                    lg.disconnectedTiles[i].MarkIsland(island);
                    break;
                }   
            }
        }
    }


    public void StartStatusEffectOnTile(int effectIndex, float lifeTime)
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.SetTileEffect(lg.levelTilesInGame.IndexOf(this), effectIndex, lifeTime);
        }
        else
        {
            SetStatusEffectOnTileOnClient(effectIndex, lifeTime);            
        }
    }

    public void  SetStatusEffectOnTileOnClient(int effectIndex, float lifeTime)
    {
        for (int i = 0; i < statusEffectsVisuals.Count; i++)
        {
            if (i == effectIndex)
            { 
                switch (effectIndex)
                {
                    case 0:
                        tileStatusEffect = StatusEffects.StatusEffect.Poison;
                        break;
                    case 1:
                        tileStatusEffect = StatusEffects.StatusEffect.Fire;
                        break;
                    case 2:
                        tileStatusEffect = StatusEffects.StatusEffect.Bleed;
                        break;
                    case 3:
                        tileStatusEffect = StatusEffects.StatusEffect.Rust;
                        break;
                    case 4:
                        tileStatusEffect = StatusEffects.StatusEffect.HealthRegen;
                        break;
                    case 5:
                        tileStatusEffect = StatusEffects.StatusEffect.GoldHunger;
                        break;
                    case 6:
                        tileStatusEffect = StatusEffects.StatusEffect.Cold;
                        break;
                    case 7:
                        tileStatusEffect = StatusEffects.StatusEffect.InLove;
                        break;
                    
                    case -1:
                        tileStatusEffect = StatusEffects.StatusEffect.Null;
                        break;
                }
                statusEffectsVisuals[effectIndex].gameObject.SetActive(true);
                statusEffectsVisuals[effectIndex].StartEffect(); 
            }

            if (effectIndex == -1)
            {
                statusEffectsVisuals[i].DestroyEffect(false);   
            } 
        }

        if (lifeTime > 0)
        {
            if (endEffectOverTime != null)
                StopCoroutine(endEffectOverTime);
            endEffectOverTime = StartCoroutine(EndEffectOverTime(lifeTime));   
        }
    }

    public void SpreadStatusEffect(StatusEffects.StatusEffect effect, int spreadSize, float spreadDelay, float lifeTime,
        TileController previousTile)
    {
        for (int i = statusEffectsVisuals.Count - 1; i >= 0; i--)
        {
            if (statusEffectsVisuals[i].gameObject.activeInHierarchy)
            {
                statusEffectsVisuals[i].timeToFinish = lifeTime;
                statusEffectsVisuals[i].timeToDestroy = 5;
                statusEffectsVisuals[i].DestroyEffect(false);   
            }
        }
        
        switch (effect)
        {
            case StatusEffects.StatusEffect.Poison:
                StartStatusEffectOnTile(0, lifeTime);
                /*
                tileStatusEffect = StatusEffects.StatusEffect.Poison;
                statusEffectsVisuals[0].gameObject.SetActive(true);
                statusEffectsVisuals[0].StartEffect();
                */
                break;
            
            case StatusEffects.StatusEffect.Fire:
                StartStatusEffectOnTile(1, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.Fire;
                statusEffectsVisuals[1].gameObject.SetActive(true);
                statusEffectsVisuals[1].StartEffect();
                */
                
                break;
            
            case StatusEffects.StatusEffect.Bleed:
                StartStatusEffectOnTile(2, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.Bleed;
                statusEffectsVisuals[2].gameObject.SetActive(true);
                statusEffectsVisuals[2].StartEffect();
                */
                break;
            
            case StatusEffects.StatusEffect.Rust:
                StartStatusEffectOnTile(3, lifeTime);
                
                /*tileStatusEffect = StatusEffects.StatusEffect.Rust;
                statusEffectsVisuals[3].gameObject.SetActive(true);
                statusEffectsVisuals[3].StartEffect();*/
                break;
            
            case StatusEffects.StatusEffect.HealthRegen:
                StartStatusEffectOnTile(4, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.HealthRegen;
                statusEffectsVisuals[4].gameObject.SetActive(true);
                statusEffectsVisuals[4].StartEffect();*/
                break;
            
            case StatusEffects.StatusEffect.GoldHunger:
                StartStatusEffectOnTile(5, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.GoldHunger;
                statusEffectsVisuals[5].gameObject.SetActive(true);
                statusEffectsVisuals[5].StartEffect();*/
                break;
            
            case StatusEffects.StatusEffect.Cold:
                StartStatusEffectOnTile(6, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.Cold;
                statusEffectsVisuals[6].gameObject.SetActive(true);
                statusEffectsVisuals[6].StartEffect();*/
                break;
            case StatusEffects.StatusEffect.InLove:
                StartStatusEffectOnTile(7, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.InLove;
                statusEffectsVisuals[7].gameObject.SetActive(true);
                statusEffectsVisuals[7].StartEffect();*/
                break;
            
            case StatusEffects.StatusEffect.Null:
                
                StartStatusEffectOnTile(-1, lifeTime);
                //tileStatusEffect = StatusEffects.StatusEffect.Null;
                break;
        }
        
        if (spreadSize > 0)
        {
            StartCoroutine(SpreadStatusEffectToNeighbours(effect, spreadSize, spreadDelay, lifeTime, previousTile));
        }
        
        /*
        if (endEffectOverTime != null)
            StopCoroutine(endEffectOverTime);
        endEffectOverTime = StartCoroutine(EndEffectOverTime(lifeTime));*/
    }
    
    public void SpreadToolEffect(ToolController.ToolType type, int spreadSize, float spreadDelay, float lifeTime, TileController previousTile)
    {
        ToolController.ToolType spreadStatusType = type;

        for (int i = statusEffectsVisuals.Count - 1; i >= 0; i--)
        {
            if (statusEffectsVisuals[i].gameObject.activeInHierarchy)
            {
                statusEffectsVisuals[i].timeToFinish = lifeTime;
                statusEffectsVisuals[i].timeToDestroy = 5;
                statusEffectsVisuals[i].DestroyEffect(false);
            }
        }
        //print(spreadStatusType);
        
        switch (type)
        {
            case ToolController.ToolType.Poison:
                StartStatusEffectOnTile(0, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.Poison;
                statusEffectsVisuals[0].gameObject.SetActive(true);
                statusEffectsVisuals[0].StartEffect();*/
                break;
            
            case ToolController.ToolType.Fire:
                StartStatusEffectOnTile(1, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.Fire;
                statusEffectsVisuals[1].gameObject.SetActive(true);
                statusEffectsVisuals[1].StartEffect();*/
                break;
            
            case ToolController.ToolType.Bleed:
                StartStatusEffectOnTile(2, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.Bleed;
                statusEffectsVisuals[2].gameObject.SetActive(true);
                statusEffectsVisuals[2].StartEffect();*/
                break;
            
            case ToolController.ToolType.Rust:
                StartStatusEffectOnTile(3, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.Rust;
                statusEffectsVisuals[3].gameObject.SetActive(true);
                statusEffectsVisuals[3].StartEffect();*/
                break;
            
            case ToolController.ToolType.Regen:
                StartStatusEffectOnTile(4, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.HealthRegen;
                statusEffectsVisuals[4].gameObject.SetActive(true);
                statusEffectsVisuals[4].StartEffect();*/
                break;
            
            case ToolController.ToolType.Antidote:
                StartStatusEffectOnTile(-1, lifeTime);
                //tileStatusEffect = StatusEffects.StatusEffect.Null;
                break;
            
            case ToolController.ToolType.Antirust:
                StartStatusEffectOnTile(-1, lifeTime);
                //tileStatusEffect = StatusEffects.StatusEffect.Null;
                break;
            
            case ToolController.ToolType.Heal:
                StartStatusEffectOnTile(-1, lifeTime);
                //tileStatusEffect = StatusEffects.StatusEffect.Null;
                break;
            
            case ToolController.ToolType.AttrackMonsters:
                //tileStatusEffect = StatusEffects.StatusEffect.Null;
                GameManager.instance.sc.AttractMonsters(transform.position, true);
                break;
            
            case ToolController.ToolType.GoldHunger:
                StartStatusEffectOnTile(5, lifeTime);
                /*
                tileStatusEffect = StatusEffects.StatusEffect.GoldHunger;
                statusEffectsVisuals[5].gameObject.SetActive(true);
                statusEffectsVisuals[5].StartEffect();*/
                break;
                
            case ToolController.ToolType.BoneShiver:
                StartStatusEffectOnTile(6, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.Cold;
                statusEffectsVisuals[6].gameObject.SetActive(true);
                statusEffectsVisuals[6].StartEffect();*/
                break;
            
            case ToolController.ToolType.Love:
                StartStatusEffectOnTile(7, lifeTime);
                
                /*
                tileStatusEffect = StatusEffects.StatusEffect.InLove;
                statusEffectsVisuals[7].gameObject.SetActive(true);
                statusEffectsVisuals[7].StartEffect();*/
                break;
        }

        if (spreadSize > 0)
        {
            StartCoroutine(SpreadToolEffectToNeighbours(type, spreadSize, spreadDelay, lifeTime, previousTile));
        }
        
        /*
        if (endEffectOverTime != null)
            StopCoroutine(endEffectOverTime);
        endEffectOverTime = StartCoroutine(EndEffectOverTime(lifeTime));
        */
        
    }

    IEnumerator EndEffectOverTime(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        
        for (int i = statusEffectsVisuals.Count - 1; i >= 0; i--)
        {
            if (statusEffectsVisuals[i].gameObject.activeInHierarchy)
                statusEffectsVisuals[i].DestroyEffect(false);
        }
        tileStatusEffect = StatusEffects.StatusEffect.Null;

        endEffectOverTime = null;
    }

    IEnumerator SpreadToolEffectToNeighbours(ToolController.ToolType type, int size, float delay, float lifeTime, TileController previousTile)
    {
        yield return new WaitForSeconds(delay);

        if (!neighboursChecked)
        {
            neighboursChecked = true;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.forward * lg.tileSize + Vector3.up * 50f, Vector3.down,
                out hit, 100f, tileLayer))
            {
                neighbourForward = hit.collider.gameObject.GetComponent<TileController>();
            }
            if (Physics.Raycast(transform.position + Vector3.right * lg.tileSize + Vector3.up * 50f, Vector3.down,
                out hit, 100f, tileLayer))
            {
                neighbourRight = hit.collider.gameObject.GetComponent<TileController>();
            }
            if (Physics.Raycast(transform.position + Vector3.back * lg.tileSize + Vector3.up * 50f, Vector3.down,
                out hit, 100f, tileLayer))
            {
                neighbourBack = hit.collider.gameObject.GetComponent<TileController>();
            }

            if (Physics.Raycast(transform.position + Vector3.left * lg.tileSize + Vector3.up * 50f, Vector3.down,
                out hit, 100f, tileLayer))
            {
                neighbourLeft = hit.collider.gameObject.GetComponent<TileController>();
            }
        }

        if (neighbourForward && neighbourForward != previousTile)
        {
            neighbourForward.SpreadToolEffect(type, size - 1, delay, lifeTime, this);
        }
        if (neighbourRight && neighbourRight != previousTile)
        {
            neighbourRight.SpreadToolEffect(type, size - 1, delay, lifeTime, this);
        }
        if (neighbourBack && neighbourBack != previousTile)
        {
            neighbourBack.SpreadToolEffect(type, size - 1, delay, lifeTime, this);
        }
        if (neighbourLeft && neighbourLeft != previousTile)
        {
            neighbourLeft.SpreadToolEffect(type, size - 1, delay, lifeTime, this);
        }
    }
    
    IEnumerator SpreadStatusEffectToNeighbours(StatusEffects.StatusEffect effect, int size, float delay, float lifeTime, TileController previousTile)
    {
        yield return new WaitForSeconds(delay);

        if (!neighboursChecked)
        {
            neighboursChecked = true;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.forward * lg.tileSize + Vector3.up * 50f, Vector3.down,
                out hit, 100f, tileLayer))
            {
                neighbourForward = hit.collider.gameObject.GetComponent<TileController>();
            }
            if (Physics.Raycast(transform.position + Vector3.right * lg.tileSize + Vector3.up * 50f, Vector3.down,
                out hit, 100f, tileLayer))
            {
                neighbourRight = hit.collider.gameObject.GetComponent<TileController>();
            }
            if (Physics.Raycast(transform.position + Vector3.back * lg.tileSize + Vector3.up * 50f, Vector3.down,
                out hit, 100f, tileLayer))
            {
                neighbourBack = hit.collider.gameObject.GetComponent<TileController>();
            }

            if (Physics.Raycast(transform.position + Vector3.left * lg.tileSize + Vector3.up * 50f, Vector3.down,
                out hit, 100f, tileLayer))
            {
                neighbourLeft = hit.collider.gameObject.GetComponent<TileController>();
            }
        }

        if (neighbourForward && neighbourForward != previousTile)
        {
            neighbourForward.SpreadStatusEffect(effect, size - 1, delay, lifeTime, this);
        }
        if (neighbourRight && neighbourRight != previousTile)
        {
            neighbourRight.SpreadStatusEffect(effect, size - 1, delay, lifeTime, this);
        }
        if (neighbourBack && neighbourBack != previousTile)
        {
            neighbourBack.SpreadStatusEffect(effect, size - 1, delay, lifeTime, this);
        }
        if (neighbourLeft && neighbourLeft != previousTile)
        {
            neighbourLeft.SpreadStatusEffect(effect, size - 1, delay, lifeTime, this);
        }
    }
}