using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ExtendedItemSpawn;
using Mirror;
using PlayerControls;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class SpawnController : MonoBehaviour
{
    public static SpawnController instance;
    
    private static List<ItemsList.ResourceType> _resourcesToSpawnInRooms = new List<ItemsList.ResourceType>
    {
        ItemsList.ResourceType.Tool, ItemsList.ResourceType.Skill, ItemsList.ResourceType.Key
    };
    
    public List<AssetReference> enemiesReferences;
    public List<AssetReference> cultistsReferences;
    private AsyncOperationHandle<GameObject> _asyncOperationHandle;
    public HealthController eyeEaterPrefab;
    public HealthController bonefirePrefab;
    public List<HealthController> bossesPrefabs;
    public HealthController treeterPrefab;
    public HealthController floorBladePrefab;
    public HealthController meatTrap;
    public List<Transform> spawners = new List<Transform>();
    public List<Spawner> spawnersOnProps = new List<Spawner>();
    
    public Interactable notePrefab;
    public Interactable meatNotePrefab;
    public Interactable hintNotePrefab;
    private int trapDoorsAmount = 0;
    
    public List<Interactable> weaponPickUpPrefabs;

    public HealthController meatHolePrefab;
    public HealthController prisonerPrefab;
    public HealthController faceEaterPrefab;
    public HealthController ironMaidenPrefab;
    
    public Interactable lockPickPrefab;
    public Interactable profilePrefab;

    public Interactable gold;
    public Interactable key;
    public Interactable goldenKey;
    public Interactable tape;
    
    public List<HealthController> mobsInGame;
    public List<HealthController> mobsInGameStatic;

    public List<Interactable> keysOnLevel = new List<Interactable>();
    
    int lastRoom = 0;
    private int j = 0; // mob counter

    private LevelTree levelTree;
    GameManager gm;
    ItemsList il;
    WeaponControls wc;
    LevelGenerator lg;
    LevelData newLevelData = null;

    //todo: remove
    public GameObject subRoomMarker;
    public GameObject deadEndMarker;
    
    public float mobSpawnTimer = 0;

    public int maximumMobsAliveCurrentBonus = 0;
    public List<GameObject> doorsInGame = new List<GameObject>();

    public GameObject bike;

    private GutProgressionManager gpm;
    public List<DynamicMapMark> dynamicMapMarksPrefabs;

    public bool chase = false;
    
    public List<MobSpawnerInRoom> mobSpawnersInRooms = new List<MobSpawnerInRoom>();
    
    public List<MobAudioManager> mobAudioManagers = new List<MobAudioManager>();
    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gm = GameManager.instance;
        gpm = GutProgressionManager.instance;
        gm.sc = this;
        wc = WeaponControls.instance;
        lg = LevelGenerator.instance;
        
        mobSpawnersInRooms.Clear();
    }

    public void ClearData()
    {
        spawners.Clear();
    }

    public IEnumerator Init(LevelTree _levelTree)
    {
        levelTree = _levelTree;
        yield return new WaitForSeconds(0.1f);
        il = ItemsList.instance;
        
        if (gm.arena)
            newLevelData = gm.arenaLevel;
        else
        {
            if (gm.tutorialPassed == 1)
                newLevelData = gm.level;
            else if (gm.tutorialPassed == 0)
                newLevelData = gm.tutorialLevel;   
        }
        
        trapDoorsAmount = newLevelData.trapDoorsAmount;
        StartCoroutine(SpawnStartEnemies());
        SpawnItemsReworked();
        SpawnNpc(newLevelData);
        SpawnTrapDoors();
        if (gm.tutorialPassed == 1 && newLevelData.meatHoles > 0)
            SpawnMeatHoles();
        
        Invoke("RemoveExtraMrWindows", 1);
    }

    void RemoveExtraMrWindows()
    {
        int mrWindowAmount = 0;
        for (int i = gm.units.Count - 1; i >= 0; i--)
        {
            var unit = gm.units[i];
            if (unit && unit.mobPartsController && unit.mobPartsController.mobType == MobPartsController.Mob.MrWindow)
            {
                mrWindowAmount++;
                if (mrWindowAmount > gm.level.mrWindowMax)
                {
                    print("kill extra mr window");
                    unit.Damage(unit.healthMax, unit.transform.position, unit.transform.position, null, null,
                    false, null, null, null, true);
                }
            }
        }
    }

    private void SpawnItemsReworked()
    {
        //print("spawn items");
        var spawnProcessor = new ItemSpawnProcessor(gm);
        var data = newLevelData;
        var items = spawnProcessor.SpawnItems(data.spawnGroups.ToList(), data.ammoSpawn.ToList(), true);
        var spawnProvider = new ItemSpawnProvider(levelTree.subRooms, levelTree.deadEnds, lg.roomsInGame);
        
        // spawn drop
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            SpawnItemOptimized(item, spawnProvider, true);
        }

        for (int i = 0; i < newLevelData.keysToSpawn; i++)
        {
            SpawnItemOptimized(key, spawnProvider, true);
        }

        for (int i = 0; i < newLevelData.goldToSpawn; i++)
        {
            SpawnItemOptimized(gold, spawnProvider, true);
        }

        if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            if (!gm.demo && !gm.goldenKeysFoundOnFloors.Contains(GutProgressionManager.instance.playerFloor))
            {
                for (int i = 0; i < newLevelData.goldenKeysToSpawn; i++)
                {
                    SpawnItemOptimized(goldenKey, spawnProvider, false);
                }
            }

            if (!gm.tapesFoundOfFloors.Contains(GutProgressionManager.instance.playerFloor))
            {
                for (int i = 0; i < newLevelData.tapesToSpawn; i++)
                {
                    SpawnItemOptimized(tape, spawnProvider, false);
                }
            }
        }

        for (int i = 0; i < newLevelData.notesAmount; i++)
        {
            SpawnItemOptimized(notePrefab, spawnProvider, true);
        }
        
        if (meatNotePrefab)
            SpawnItemOptimized(meatNotePrefab, spawnProvider, true);

        if (hintNotePrefab)
        {
            for (int i = 0; i < newLevelData.noteHintsAmount; i++)
            {
                SpawnItemOptimized(hintNotePrefab, spawnProvider, true);
            }
        }

        if (gm.tutorialPassed == 1)
            SpawnLockpicks(spawnProvider);
        
        var elevator = ElevatorController.instance;
        if (elevator)
        {
            for (var i = 0; i < newLevelData.padlocksAmount; i++)
            {
                var newPadlock = elevator.CreatePadlocks(i);
                
                if (LevelGenerator.instance.levelgenOnHost)
                    NetworkServer.Spawn(newPadlock.gameObject);
            }
        }
        
        if (ElevatorShaftController.instance)
            ElevatorShaftController.instance.StopLoading();
    }

    void SpawnLockpicks(ItemSpawnProvider spawnProvider)
    {
        /*
            print("SpawnLockpicks");
            List<TileController> tempTiles = new List<TileController>();
            for (var index = 0; index < lg.levelTilesInGame.Count; index++)
            {
                var tile = lg.levelTilesInGame[index];
                if (tile.WallsAmount() > 2 && Vector3.Distance(tile.transform.position, Vector3.zero) > 50) // in dead ends
                {
                    tempTiles.Add(tile);
                }
            }
    
            /
            for (int i = 0; i < newLevelData.lockpicksAmount; i++)
            {
                if (tempTiles.Count > 0)
                {
                    int r = Random.Range(0, tempTiles.Count);
                    Vector3 spawnPoint = tempTiles[r].transform.position;
                    if (tempTiles[r].propOnTile)
                        spawnPoint = tempTiles[r].propOnTile.spawners[Random.Range(0, tempTiles[r].propOnTile.spawners.Count)].transform.position;
                    
                    tempTiles.RemoveAt(r);
                    Instantiate(lockPickPrefab, spawnPoint, Quaternion.identity);
                }
                else
                {
                    SpawnItemOptimized(lockPickPrefab, spawnProvider);
                }
            }
    
            if (tempTiles.Count > 0)
            {
                for (int i = tempTiles.Count - 1; i >= 0; i--)
                {
                    Vector3 spawnPoint = tempTiles[i].transform.position;
                    if (tempTiles[i].propOnTile)
                        spawnPoint = tempTiles[i].propOnTile.spawners[Random.Range(0, tempTiles[i].propOnTile.spawners.Count)].transform.position;
                    
                    tempTiles.RemoveAt(i);
                    if (Random.value < 0.2f)
                    {
                        // spawn lockpick
                        Instantiate(lockPickPrefab, spawnPoint, Quaternion.identity);   
                    }
                    else if (Random.value < 0.5f)
                    {
                        // spawn note
                        Instantiate(notePrefab, spawnPoint, Quaternion.identity);
                    }
                    else if (Random.value < 0.8f)
                    {
                        Instantiate(gold, spawnPoint, Quaternion.identity);
                    }
                    else if (Random.value < 0.9f)
                    {
                        // spawn ammo
                        if (Random.value > 0.5f)
                            Instantiate(newLevelData.ammoSpawn[Random.Range(0, newLevelData.ammoSpawn.Length)].value.bulletPack, spawnPoint, Quaternion.identity);
                        else
                        {
                            var r = Random.Range(0, newLevelData.ammoSpawn.Length);
                            if (newLevelData.ammoSpawn[r].value.bullets != null)
                                Instantiate(newLevelData.ammoSpawn[r].value.bullets, spawnPoint, Quaternion.identity);
                            else
                                Instantiate(newLevelData.ammoSpawn[r].value.bulletPack, spawnPoint, Quaternion.identity);
                        }
                    }
                    else
                    {
                        // spawn item
                        // spawn group[0] is always items
                        Instantiate(newLevelData.spawnGroups[0].simpleItems[Random.Range(0,newLevelData.spawnGroups[0].simpleItems.Length)].item.value, spawnPoint, Quaternion.identity);
                    }
                }
            }
        */
    
        var doors = new List<DoorController>();
        for (var index = 0; index < ItemsList.instance.interactables.Count; index++)
        {
            Interactable i = ItemsList.instance.interactables[index];
            if (i.door && i.door.padlock != null)
            {
                doors.Add(i.door);
            }
        }

        if (lg.levelgenOnHost) return;
        
        for (int i = 0; i < newLevelData.lockedDoorsAmount; i++)
        {
            if (doors.Count > 0)
            {
                int r = Random.Range(0, doors.Count);
                doors[r].CreateLockOnDoor();
                doors.RemoveAt(r);
            }
        }
    }

    // main spawn thread
    void SpawnNpc(LevelData newLevelData)
    {
        print("Spawn Npc and or Boss");
        List<HealthController> tempList = new List<HealthController>(newLevelData.npcsPool);

        for (int i = 0; i < newLevelData.npcAmount; i++)
        {
            int r = Random.Range(0, tempList.Count);

            HealthController newNpc = Instantiate(tempList[r], GetMobSpawner(true).position, Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));
            tempList.RemoveAt(r);
            if (tempList.Count <= 0)
            {
                tempList = new List<HealthController>(newLevelData.npcsPool);
            }
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }

        if (newLevelData.sheNpcPrefab)
        {
            for (int i = 0; i < newLevelData.sheAmount; i++)
            {
                HealthController newNpc = Instantiate(newLevelData.sheNpcPrefab, GetMobSpawner(true).position, Quaternion.identity);
                newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));   
                
                if (lg.levelgenOnHost)
                {
                    NetworkServer.Spawn(newNpc.gameObject);
                }
            }
        }

        for (int i = 0; i < newLevelData.keyNpcAmount; i++)
        {
            HealthController newNpc = Instantiate(newLevelData.keyNpcPrefabs[Random.Range(0,newLevelData.keyNpcPrefabs.Count)], GetMobSpawner(true).position, Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));
            mobsInGameStatic.Add(newNpc);
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }
        for (int i = 0; i < newLevelData.prisonersAmount; i++)
        {
            HealthController newNpc = Instantiate(prisonerPrefab, GetMobSpawner(true).position, Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));
            mobsInGameStatic.Add(newNpc);
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }
        
        for (int i = 0; i < newLevelData.faceEaterAmount; i++)
        {
            HealthController newNpc = Instantiate(faceEaterPrefab, GetMobSpawner(false).position, Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));
            mobsInGameStatic.Add(newNpc);
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }

        SpawnEyeTaker(newLevelData.eyeEatersAmount);

        
        if (newLevelData.currentBoss >= 0)
        {
            var newPos = GetMobSpawner(true).position;
            print(newPos);
            HealthController newNpc = Instantiate(bossesPrefabs[newLevelData.currentBoss], newPos, Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));
            mobsInGameStatic.Add(newNpc);
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }
        
        for (int i = 0; i < newLevelData.bonefiresAmount; i++)
        {
            HealthController newNpc = Instantiate(bonefirePrefab, GetMobSpecificTileSpawner(StatusEffects.StatusEffect.Cold), Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }
        
        
        for (int i = 0; i < newLevelData.treetersAmount; i++)
        {
            HealthController newNpc = Instantiate(treeterPrefab, GetMobSpecificTileSpawner(StatusEffects.StatusEffect.Cold), Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));
            
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }
        
        SpawnMeatTraps(newLevelData.meatTrapsAmount);
        
        for (int i = 0; i < newLevelData.itemsMimicsAmount; i++)
        {
            SpawnMobFromReference(enemiesReferences[10], GetMobSpawner(false).position);
        }

        for (int i = 0; i < mobSpawnersInRooms.Count; i++)
        {
            SpawnMobByType(mobSpawnersInRooms[i].mobType, mobSpawnersInRooms[i].transform);
        }
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            HealthController newNpc = Instantiate(ironMaidenPrefab, GetMobSpawner(true).position, Quaternion.identity);
            // IN ELEVATOR ROOM: HealthController newNpc = Instantiate(ironMaidenPrefab, GetFreeRoomTile(0).transform.position, Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));
            
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }
    }

    TileController GetFreeRoomTile(int roomIndex)
    {
        foreach (var t in lg.roomsInGame[roomIndex].tiles)
        {
            if (t.propOnTile == null && t.spawnerTile)
                return t;
        }
        return lg.roomsInGame[roomIndex].tiles[Random.Range(0, lg.roomsInGame[roomIndex].tiles.Count)];
    }
    
    public void SpawnEyeTaker(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            HealthController newNpc = Instantiate(eyeEaterPrefab, GetMobSpawner(true).position, Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0,360f));
            mobsInGameStatic.Add(newNpc);

            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }
    }

    public void SpawnMeatTraps(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            HealthController newNpc = Instantiate(meatTrap, GetMobSpawner(true).position, Quaternion.identity);
            newNpc.transform.Rotate(Vector3.up, Random.Range(0, 3)* 90f);
            
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newNpc.gameObject);
            }
        }
    }
    
    private void SpawnItemOptimized(Interactable item, ItemSpawnProvider spawnProvider, bool server)
    {
        var spawner = (item.pickUp && _resourcesToSpawnInRooms.Contains(item.pickUp.resourceType))
                      || item.weaponPickUp
            ? spawnProvider.Get()
            : GetResourceSpawner();

        Vector3 spawnPos = spawner.position + Vector3.up;
        InstantiateItem(item, spawnPos, Quaternion.Euler(Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)), server);
    }

    public void SpawnItemOnProp(PropController newProp)
    {
        il = ItemsList.instance;
        wc = WeaponControls.instance;

        if (Random.value >= 0.5f)
        {
            int weaponIndex = -1;
            if (wc.activeWeapon && wc.activeWeapon.weaponType != WeaponController.Type.Melee &&
                il.ammoDataStorage.GetAmmoCount(wc.activeWeapon.weapon) <= 0)
            {
                weaponIndex = 0;
            }
            else if (wc.secondWeapon &&
                wc.secondWeapon.weaponType != WeaponController.Type.Melee &&
                il.ammoDataStorage.GetAmmoCount(wc.secondWeapon.weapon) <= 0)
            {
                weaponIndex = 1;
            }

            if (weaponIndex == -1) return;
            
            SpawnAdditionalAmmoOnProp(weaponIndex, newProp);
        }
        else
        {
            DynamicObstaclesManager.instance.PlaceSpawnedWeaponOnProp(newProp);
        }
    }
    
    public Interactable InstantiateItem(Interactable item, Vector3 spawnPos, Quaternion rotation, bool serverSpawn)
    {
        var spawnedItem = Instantiate(item, spawnPos, rotation);

        if (spawnedItem.pickUp && spawnedItem.pickUp.resourceType == ItemsList.ResourceType.Key)
        {
            keysOnLevel.Add(spawnedItem);
        }
        
        if (spawnedItem.weaponPickUp)
        {
            if (spawnedItem.weaponPickUp.weaponConnector)
                spawnedItem.weaponPickUp.weaponConnector.GenerateOnSpawn();
            
            spawnedItem.weaponPickUp.weaponDataRandomier.GenerateOnSpawn(false, false);
        }

        return spawnedItem;
    }
    
    IEnumerator SpawnStartEnemies()
    {
        j = 0;
        int arenaCurrentPool = 0; 
        while(true)
        {
            if (gm.tutorialPassed == 0 && (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.localPlayer == false))
            {
                while (il.activeWeapon == WeaponPickUp.Weapon.Null && il.secondWeapon == WeaponPickUp.Weapon.Null)
                {
                    yield return new WaitForSeconds(1);
                }
            }
            mobSpawnTimer = 0;

            if (!gm.arena)
            {
                while (mobSpawnTimer < newLevelData.mobSpawnDelay)
                {
                    if (gm.player && gm.player.usedTile && (!gm.player.usedTile.masterRoom || gm.player.usedTile.masterRoom != lg.roomsInGame[0]))
                    {
                        if (gm.player.wc.activeWeapon && !PlayerSkillsController.instance.horrorMode)
                        {
                            // COUNT MOVEMENT
                            if (gm.player.pm.movementStats.isRunning)
                                mobSpawnTimer += 2f * il.badReputaion;
                            else
                                mobSpawnTimer += 1f * il.badReputaion;

                            // COUNT WEAPONS
                            // ONLY WHEN GUT DOESN'T     LIKE YOU
                            if (il.badReputaion >= 3 && (gm.player.wc.activeWeapon.durability > 0 ||
                                (gm.player.wc.secondWeapon && gm.player.wc.secondWeapon.durability > 0)))
                            {
                                WeaponController newWc = gm.player.wc.activeWeapon;
                                for (int i = 0; i <= 1; i++)
                                {
                                    bool canAdd = true;
                                
                                    if (newWc.weaponType != WeaponController.Type.Melee)
                                        canAdd = il.ammoDataStorage.GetAmmoCount(newWc.weapon) > newWc.ammoClipMax || newWc.ammoClip > newWc.ammoClipMax / 2;

                                    if (canAdd)
                                    {
                                        switch (newWc.weapon)
                                        {
                                            case WeaponPickUp.Weapon.Axe:
                                                mobSpawnTimer += 1.5f * il.badReputaion;
                                                break;
                                    
                                            case WeaponPickUp.Weapon.Pistol:
                                                mobSpawnTimer += 2f * il.badReputaion;
                                                break;
                                    
                                            case WeaponPickUp.Weapon.Revolver:
                                                mobSpawnTimer += 2.5f * il.badReputaion;
                                                break;
                                    
                                            case WeaponPickUp.Weapon.TommyGun:
                                                mobSpawnTimer += 3 * il.badReputaion;
                                                break;
                                    
                                            case WeaponPickUp.Weapon.Shotgun:
                                                mobSpawnTimer += 4f * il.badReputaion;
                                                break;
                                            
                                            default:
                                                mobSpawnTimer += 1f * il.badReputaion;
                                                break;
                                        }
                                    }

                                    if (gm.player.wc.secondWeapon)
                                    {
                                        newWc = gm.player.wc.secondWeapon;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }   
                            }
                        }
                        else //  delay spawn if no weapons or if in horror mode
                        {
                                if (gm.player.pm.movementStats.isRunning)
                                    mobSpawnTimer += 2f * il.badReputaion;
                                else
                                    mobSpawnTimer += 1f * il.badReputaion;   
                        }

                        for (var index = 0; index < il.savedTools.Count; index++)
                        {
                            var t = il.savedTools[index];
                            mobSpawnTimer += t.amount * il.badReputaion;
                        }

                        for (int j = 0; j < il.gold; j += 10)
                        {
                            mobSpawnTimer += 1f * il.badReputaion;
                        }

                    }
                    yield return new WaitForSeconds(1f);
                }
                MoveMobsCloser();
            }
            else
            {
                if (!GLNetworkWrapper.instance || !GLNetworkWrapper.instance.coopIsActive)
                {
                    // solo
                    StartBadRepChase();
                }
                else if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive &&
                         GLNetworkWrapper.instance.localPlayer.isServer)
                {
                    //ON HOST
                    GLNetworkWrapper.instance.StartChase();
                }
            }
        
            //j = SpawnMob(j);
            // spawn mobs
            while (mobsInGame.Count < gm.level.mobsAmount + maximumMobsAliveCurrentBonus)
            {
                if (gm.level.mobsPool.Count <= j)
                    j = 0;

                MobPartsController.Mob newMobType;
                if (!gm.arena)
                    newMobType = newLevelData.mobsPool[j];
                else
                    newMobType = newLevelData.arenaMonsterWaves[arenaCurrentPool].mobsPool[j];

                SpawnMobByType(newMobType, null);
                
                j++;
                if (j >= newLevelData.mobsPool.Count) j = 0;
                yield return _asyncOperationHandle;
                yield return new WaitForSeconds(Random.Range(0.5f, 5f));
            }
                
            if (gm.arena)
            {
                yield return new WaitForSeconds(10 + maximumMobsAliveCurrentBonus);
                arenaCurrentPool++;
                if (arenaCurrentPool >= newLevelData.arenaMonsterWaves.Count)
                    arenaCurrentPool = 0;
                
                maximumMobsAliveCurrentBonus++;
            }
            
            // помощь игроку, если у него нет патронов 
            if (Random.value >= 0.5f && gm.player.wc.activeWeapon && gm.player.wc.activeWeapon.weaponType != WeaponController.Type.Melee && il.ammoDataStorage.GetAmmoCount(gm.player.wc.activeWeapon.weapon) <= 0)
            {
                int weaponIndex = 0;
                if (gm.player.wc.secondWeapon &&
                    gm.player.wc.secondWeapon.weaponType != WeaponController.Type.Melee &&
                    il.ammoDataStorage.GetAmmoCount(gm.player.wc.activeWeapon.weapon) <= 0)
                {
                    if (Random.value > 0.5f) weaponIndex = 1;
                }
                SpawnAdditionalAmmo(weaponIndex);
            }
        }
    }

    public void SpawnMobByType(MobPartsController.Mob newMobType, Transform transformToSpawn)
    {
        Vector3 newPos;
        if (transformToSpawn != null) newPos = transformToSpawn.position;
        else
            newPos = GetMobSpawner(false).position;
        
        switch (newMobType)
        {
            case MobPartsController.Mob.Walker:
                SpawnMobFromReference(enemiesReferences[0], newPos);
                break;

            case MobPartsController.Mob.Witch:
                SpawnMobFromReference(enemiesReferences[1], newPos);
                break;

            case MobPartsController.Mob.Jumper:
                SpawnMobFromReference(enemiesReferences[2], newPos);
                break;
                
            case MobPartsController.Mob.Mimic:
                SpawnMobFromReference(enemiesReferences[3], newPos);
                break;

            case MobPartsController.Mob.MimicBig:
                SpawnMobFromReference(enemiesReferences[4], newPos);
                break;
            
            case MobPartsController.Mob.Castle:
                SpawnMobFromReference(enemiesReferences[5], newPos);
                break;
            
            case MobPartsController.Mob.BigTurret:
                SpawnMobFromReference(enemiesReferences[6], newPos);
                break;
                
            case MobPartsController.Mob.Moocher:
                SpawnMobFromReference(enemiesReferences[7], newPos);
                break;
            
            case MobPartsController.Mob.MimicForest:
                SpawnMobFromReference(enemiesReferences[8], newPos);
                break;
            
            case MobPartsController.Mob.MrPole:
                SpawnMobFromReference(enemiesReferences[9], newPos);
                break;
            
            case MobPartsController.Mob.ItemMimic:
                SpawnMobFromReference(enemiesReferences[10], newPos);
                break;
            
            case MobPartsController.Mob.MimicCity:
                SpawnMobFromReference(enemiesReferences[11], newPos);
                break;
            
            case MobPartsController.Mob.EyeTaker:
                SpawnMobFromReference(enemiesReferences[12], newPos);
                break;
            
            case MobPartsController.Mob.FactoryMimic:
                SpawnMobFromReference(enemiesReferences[13], newPos);
                break;
            
            case MobPartsController.Mob.FactoryMimicBig:
                SpawnMobFromReference(enemiesReferences[14], newPos);
                break;
            
            case MobPartsController.Mob.MrWindow:
                SpawnMobFromReference(enemiesReferences[15], newPos);
                break;
            
            default:
                SpawnMobFromReference(enemiesReferences[0], newPos);
                break;
        }
    }

    public void SpawnRandomMobInsteadOfProp(Vector3 pos)
    {
        var r = newLevelData.mobsPool[Random.Range(0, newLevelData.mobsPool.Count)];
        
        switch (r)
        {
            case MobPartsController.Mob.Walker:
                SpawnMobFromReference(enemiesReferences[0], pos);
                break;

            case MobPartsController.Mob.Witch:
                SpawnMobFromReference(enemiesReferences[1], pos);
                break;

            case MobPartsController.Mob.Jumper:
                SpawnMobFromReference(enemiesReferences[2], pos);
                break;
                
            case MobPartsController.Mob.Mimic:
                SpawnMobFromReference(enemiesReferences[3], pos);
                break;

            case MobPartsController.Mob.MimicBig:
                SpawnMobFromReference(enemiesReferences[4], pos);
                break;
            
            case MobPartsController.Mob.Castle:
                SpawnMobFromReference(enemiesReferences[5], pos);
                break;
            
            case MobPartsController.Mob.BigTurret:
                SpawnMobFromReference(enemiesReferences[6], pos);
                break;
                
            case MobPartsController.Mob.Moocher:
                SpawnMobFromReference(enemiesReferences[7], pos);
                break;
            
            case MobPartsController.Mob.MimicForest:
                SpawnMobFromReference(enemiesReferences[8], pos);
                break;
            
            case MobPartsController.Mob.MrPole:
                SpawnMobFromReference(enemiesReferences[9], pos);
                break;
            
            case MobPartsController.Mob.ItemMimic:
                SpawnMobFromReference(enemiesReferences[10], pos);
                break;
            
            case MobPartsController.Mob.MimicCity:
                SpawnMobFromReference(enemiesReferences[11], pos);
                break;
            
            case MobPartsController.Mob.EyeTaker:
                SpawnMobFromReference(enemiesReferences[12], pos);
                break;
            
            case MobPartsController.Mob.FactoryMimic:
                SpawnMobFromReference(enemiesReferences[13], pos);
                break;
            
            case MobPartsController.Mob.FactoryMimicBig:
                SpawnMobFromReference(enemiesReferences[14], pos);
                break;
            
            case MobPartsController.Mob.MrWindow:
                SpawnMobFromReference(enemiesReferences[15], pos);
                break;

            default:
                SpawnMobFromReference(enemiesReferences[0], pos);
                break;
        }
    }


    public void SpawnMobByIndex(int i)
    {
        SpawnMobFromReference(enemiesReferences[i], GetMobSpawner(false).position);
    }
    
    public void SpawnMobFromReference(AssetReference mob, Vector3 transformPosition)
    {
        AssetSpawner.instance.Spawn(mob, transformPosition, AssetSpawner.ObjectType.Mob);
    }

    public void SpawnCultistMob(int index, Vector3 pos)
    {
        if (index == 0)
            AssetSpawner.instance.Spawn(cultistsReferences[0], pos, AssetSpawner.ObjectType.Mob);
        else
            AssetSpawner.instance.Spawn(cultistsReferences[1], pos, AssetSpawner.ObjectType.Mob);
    }

    public void ProceedMob(GameObject mob)
    {
        HealthController newMob = mob.GetComponent<HealthController>();

        bool rotate = true;
        
        if (newMob.cultist)
            CultGenerator.instance.AddCultist(newMob);
        else if (newMob.mobPartsController && newMob.mobPartsController.mobType == MobPartsController.Mob.MrWindow)
        {
            // rotate mr window
            rotate = false;

            float distance = 1000;
            Transform closestTransform = null;
            for (int i = 0; i < mobSpawnersInRooms.Count; i++)
            {
                float newDistance = Vector3.Distance(newMob.transform.position, mobSpawnersInRooms[i].transform.position);
                if (newDistance <= distance)
                {
                    distance = newDistance;
                    closestTransform = mobSpawnersInRooms[i].transform;
                }
            }

            if (closestTransform != null)
            {
                newMob.transform.position = closestTransform.position;
                newMob.transform.rotation = closestTransform.rotation;   
            }
        }
        else
            mobsInGame.Add(newMob);
        
        newMob.mobPartsController.level = gm.level.mobsLevel;
        
        if (rotate)
            newMob.transform.Rotate(Vector3.up, Random.Range(0f, 360f));
        
        if (lg.levelgenOnHost)
            NetworkServer.Spawn(newMob.gameObject);
    }
    

    void SpawnTrapDoors()
    {
        if (trapDoorsAmount > 0)
        {
            List<TileController> tilesTemp = new List<TileController>();
            for (int i = 0; i < lg.levelTilesInGame.Count; i++)
            {
                if (!lg.levelTilesInGame[i].propTile && lg.levelTilesInGame[i].wallsAmount > 0 && (lg.levelTilesInGame[i].map == null || !lg.levelTilesInGame[i].map.gameObject.activeInHierarchy)&&
                    lg.levelTilesInGame[i].door == null && lg.levelTilesInGame[i].tileStatusEffect == StatusEffects.StatusEffect.Null)
                {
                    tilesTemp.Add(lg.levelTilesInGame[i]);
                }
            }

            var gmp = GutProgressionManager.instance;
            
            for (int i = 0; i < trapDoorsAmount; i++)
            {
                if (tilesTemp.Count > 0)
                {
                    int r = Random.Range(0, tilesTemp.Count);

                    HealthController newTrapDoor = Instantiate(gmp.GetTrapDoopPrefab(tilesTemp[r]), tilesTemp[r].transform.position,
                        Quaternion.identity);
                    
                    if (tilesTemp[r].wallRight)
                        newTrapDoor.transform.Rotate(Vector3.up, 90);
                    else if (tilesTemp[r].wallBack)
                        newTrapDoor.transform.Rotate(Vector3.up, 180);
                    else if (tilesTemp[r].wallLeft)
                        newTrapDoor.transform.Rotate(Vector3.up, 270);
                    
                    if (lg.levelgenOnHost)
                        NetworkServer.Spawn(newTrapDoor.gameObject);
                    tilesTemp.RemoveAt(r);
                }   
            }
        }
    }

    public void MobHearNoise(Vector3 noiseSource, float noiseDistance)
    {
        
        if (LevelGenerator.instance.generating) return;
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.MobHearNoiceOnClient(noiseSource, noiseDistance);
        }
        else
        {
            MobHearNoiceOnClient(noiseSource, noiseDistance, -1);
        }
    }

    public void MobHearNoiceOnClient(Vector3 noiseSource, float noiseDistance, int playerId)
    {
        bool heard = false;
        int m = 0;
        gm = GameManager.instance;

        int maxMobAttacking = 100000;
        if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            if (gm.arena == false)
                maxMobAttacking = gm.level.maximumMobsAttackingPlayer;
            else
                maxMobAttacking = gm.arenaLevel.maximumMobsAttackingPlayer;
        }
        
        for (int i = mobsInGame.Count - 1; i >= 0 && m < maxMobAttacking; i--)
        {
            var mob = mobsInGame[i];

            if (mob != null && (mob.canHear || PlayerSkillsController.instance.horrorMode) && (!mob.eyeEater || ItemsList.instance.lostAnEye == 0))
            {
                if (Vector3.Distance(noiseSource, mob.transform.position) <= noiseDistance)
                {
                    if (mob.mobGroundMovement && mob.mobGroundMovement.monsterState == MobGroundMovement.State.Idle)
                    {
                        heard = true;
                        
                        // BREAK HERE IF NOT HOST
                        if (playerId > 0) break;
                        
                        if (gm.player.health < gm.player.healthMax * 0.3f && Random.value > 0.5f)
                            mob.mobGroundMovement.MoveCloser(noiseSource, false);
                        else
                            mob.mobGroundMovement.MoveCloser(noiseSource, true);
                        m++;
                    }
                }   
            }
        }

        m = 0;
        for (int i = mobsInGameStatic.Count - 1; i >= 0 && m < maxMobAttacking; i--)
        {
            var mob = mobsInGameStatic[i];

            if (mob != null && (mob.canHear || PlayerSkillsController.instance.horrorMode) && (!mob.eyeEater || il.lostAnEye == 0))
            {
                if (Vector3.Distance(noiseSource, mob.transform.position) <= noiseDistance)
                {
                    if (mob.mobGroundMovement && mob.mobGroundMovement.monsterState == MobGroundMovement.State.Idle)
                    {
                        heard = true;
                        
                        // BREAK HERE IF NOT HOST
                        if (playerId > 0) break;
                        
                        if (gm.player.health < gm.player.healthMax * 0.33f && Random.value > 0.33f)
                            mob.mobGroundMovement.MoveCloser(noiseSource, false);
                        else
                            mob.mobGroundMovement.MoveCloser(noiseSource, true);
                        m++;
                    }
                }   
            }
        }
        
        /*
        print("MOB HEAR NOISE: " + playerId + "; " + heard);
        if (heard)
            UiManager.instance.SomeoneHearPlayer(playerId);  
            */
    }
    
    public void MoveMobsCloser()
    {
        if (gm.tutorialPassed == 0) return;

        int m = 0;
        gm = GameManager.instance;

        var closestPlayer = PlayerMovement.instance.hc;

        // move mobs closer
        bool monsterSent = false;
        //for (int i = 0; i < mobsInGame.Count && m < newLevelData.maximumMobsAttackingPlayer; i++)
        for (int i = 0; i < mobsInGame.Count; i++)
        {
            if (mobsInGame[i] == null)
            {
                mobsInGame.RemoveAt(i);
                continue;
            }
            
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                closestPlayer = GLNetworkWrapper.instance.GetClosestPlayer(mobsInGame[i].transform.position);
            }
            
            float newDist = 0;
            if (lg.generating)
                newDist = Vector3.Distance(mobsInGame[i].transform.position, lg.playerSpawner.transform.position);
            else
                newDist = Vector3.Distance(mobsInGame[i].transform.position, closestPlayer.transform.position);
            
            if (mobsInGame[i].boss == false && newDist > 40)
            {
                if (mobsInGame[i].mobGroundMovement)
                {
                    if (!monsterSent && Random.value > 0.9f)
                    {
                        /*
                        if (closestPlayer.health < closestPlayer.healthMax * 0.3f)
                            mobsInGame[i].mobGroundMovement.MoveCloser(closestPlayer.transform.position, false);
                        else
                            mobsInGame[i].mobGroundMovement.MoveCloser(closestPlayer.transform.position, true);
                            */
                        
                        if (closestPlayer.health < closestPlayer.healthMax * 0.3f && Random.value > 0.5f)
                            mobsInGame[i].mobGroundMovement.MoveCloser(GetMobSpawner(false).position, false);
                        else
                            mobsInGame[i].mobGroundMovement.MoveCloser(GetMobSpawner(false).position, true);
                        m++;
                        monsterSent = true;
                    }
                    else
                    {
                        if (closestPlayer.health < closestPlayer.healthMax * 0.3f && Random.value > 0.5f)
                            mobsInGame[i].mobGroundMovement.MoveCloser(GetMobSpawner(false).position, false);
                        else
                            mobsInGame[i].mobGroundMovement.MoveCloser(GetMobSpawner(false).position, true);
                        m++;
                    }
                }
                else if (mobsInGame[i].mobHideInCorners)
                {
                    m++;
                    mobsInGame[i].mobHideInCorners.MoveCloser(GetMobSpawner(false).position);
                }
                else if (mobsInGame[i].mobJumperMovement)
                {
                    m++;
                    mobsInGame[i].mobJumperMovement.JumpOnTeleport();
                    mobsInGame[i].transform.position = GetMobSpawner(false).position;
                }
            }
        }
        
        // move static mobs closer
        for (int i = 0; i < mobsInGameStatic.Count; i++)
        {
            if (mobsInGameStatic[i] == null)
            {
                mobsInGameStatic.RemoveAt(i);
                continue;
            }

            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                closestPlayer = GLNetworkWrapper.instance.GetClosestPlayer(mobsInGameStatic[i].transform.position);
            }
            
            float newDist = 0;
            if (lg.generating)
                newDist = Vector3.Distance(mobsInGameStatic[i].transform.position, lg.playerSpawner.transform.position);
            else
                newDist = Vector3.Distance(mobsInGameStatic[i].transform.position, closestPlayer.transform.position);
            
            if (mobsInGameStatic[i].boss == false && Random.value > 0.33f && newDist > 30)
            {
                if (mobsInGameStatic[i].mobGroundMovement && !mobsInGameStatic[i].eyeEater)
                {
                    mobsInGameStatic[i].mobGroundMovement.MoveCloser(GetMobSpawner(false).position, true);
                }
                else if (mobsInGameStatic[i].mobHideInCorners)
                {
                    mobsInGameStatic[i].mobHideInCorners.MoveCloser(GetMobSpawner(false).position);
                }
                else if (mobsInGameStatic[i].mobJumperMovement)
                {
                    mobsInGameStatic[i].mobJumperMovement.JumpOnTeleport();
                    mobsInGameStatic[i].transform.position = GetMobSpawner(false).position;
                }
            }
        }
    }

    public void SpawnMob()
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.SpawnMob();
        }
        else
        {
            StartCoroutine(SpawnMobOnClientCoroutine());
        }
    }
    public IEnumerator SpawnMobOnClientCoroutine()
    {
        // spawn mobs
        if (mobsInGame.Count < gm.level.mobsAmount + maximumMobsAliveCurrentBonus)
        {
            MobPartsController.Mob newMobType;
            if (!gm.arena)
                newMobType = gm.tutorialPassed == 1 ? gm.level.mobsPool[j] : gm.tutorialLevel.mobsPool[0];
            else
                newMobType = gm.arenaLevel.arenaMonsterWaves[Mathf.Clamp(maximumMobsAliveCurrentBonus, 0, gm.arenaLevel.arenaMonsterWaves.Count)].mobsPool[j];

            SpawnMobByType(newMobType, null);
            
            if (gm.tutorialPassed == 1)
            {
                j++;
                if (j >= gm.level.mobsPool.Count) j = 0;   
            }
            yield return _asyncOperationHandle;
        }
    }
    public IEnumerator SpawnMobsToChase()
    {
        var closestPlayer = PlayerMovement.instance.hc;
        j = 0;
        int maxMobs = gm.level.mobsAmount + maximumMobsAliveCurrentBonus;
        if (gm.tutorialPassed == 0)
            maxMobs = 1;
        
        while (true)
        {
           // spawn mobs
            if (mobsInGame.Count < maxMobs)
            {
                if (Random.value > 0.8f && GutProgressionManager.instance.playerFloor > 6)
                    SpawnMobByType(MobPartsController.Mob.EyeTaker, null);
                else
                {
                    MobPartsController.Mob newMobType = gm.tutorialPassed == 1 ? gm.level.mobsPool[j] : gm.tutorialLevel.mobsPool[0];
                    SpawnMobByType(newMobType, null);
                }

                if (gm.tutorialPassed == 1)
                {
                    j++;
                    if (j >= gm.level.mobsPool.Count) j = 0;   
                }
                yield return _asyncOperationHandle;
            }
            
            if (gm.tutorialPassed == 0)
            {
                yield return new WaitForSeconds(5);
            }
            else
            {
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    yield return new WaitForSeconds(1);
                }
                else
                {
                    if (closestPlayer.health > closestPlayer.healthMax * 0.75f)
                        yield return new WaitForSeconds(1);
                    else if (closestPlayer.health > closestPlayer.healthMax * 0.5f)
                        yield return new WaitForSeconds(3);
                    else 
                        yield return new WaitForSeconds(6);   
                }   
            }
        }
    }
    
    private void SpawnAdditionalAmmo(int weaponIndex) // 0 - activeWeapon, 1 - secondWeapon
    {
        Interactable item = null;
        if (weaponIndex == 0)
            item = gm.player.wc.activeWeapon.ammoPackPrefab;
        else if (weaponIndex == 1)
            item = gm.player.wc.secondWeapon.ammoPackPrefab;

        var newAmmo = Instantiate(item, GetResourceSpawner().position, Quaternion.identity);
        
        if (lg.levelgenOnHost)
        {
            NetworkServer.Spawn(newAmmo.gameObject);
        }
    }
    private void SpawnAdditionalAmmoOnProp(int weaponIndex, PropController newProp) // 0 - activeWeapon, 1 - secondWeapon
    {
        Interactable item = null;
        if (weaponIndex == 0)
            item = gm.player.wc.activeWeapon.ammoPackPrefab;
        else if (weaponIndex == 1)
            item = gm.player.wc.secondWeapon.ammoPackPrefab;

        var spawnerNew = newProp.spawners[Random.Range(0, newProp.spawners.Count)];
        var newAmmo = Instantiate(item, spawnerNew.transform.position, Quaternion.identity);
        newAmmo.transform.eulerAngles = new Vector3(Random.Range(0,360), Random.Range(0,360),Random.Range(0,360));
        newAmmo.transform.parent = spawnerNew.transform;
        newAmmo.transform.localPosition = Vector3.zero;
        newAmmo.insideTheProp = true;
        newProp.spawnedObject = newAmmo;
    }

    public void AttractMonstersOnServer(Vector3 newTarget, bool _chase)
    {
        int m = 0;
        //for (var index = 0; index < mobsInGame.Count && m < newLevelData.maximumMobsAttackingPlayer; index++)
        for (var index = 0; index < mobsInGame.Count; index++)
        {
            HealthController hc = mobsInGame[index];
            
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                newTarget = GLNetworkWrapper.instance.GetClosestPlayer(hc.transform.position).transform.position;
            }
            
            if (hc.mobGroundMovement)
            {
                hc.mobGroundMovement.MoveCloser(newTarget, false);
            }
            else if (hc.mobHideInCorners)
            {
                hc.mobHideInCorners.MoveCloser(newTarget);
            }
            else if (hc.mobJumperMovement && !hc.boss)
            {
                hc.mobJumperMovement.JumpOnTeleport();
                hc.transform.position = GetMobSpawner(false).position;
            }
            
            if (!_chase) m++;
        }
    }
    
    public void AttractMonsters(Vector3 newTarget, bool _chase)
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.AttractMonsters(newTarget, _chase);
        }
        else
        {
            AttractMonstersOnServer(newTarget, _chase);    
        }
    }

    public void DifficultyUp()
    {
        if (gm.tutorialPassed == 1)
        {
            maximumMobsAliveCurrentBonus++;
            mobSpawnTimer += gm.level.mobSpawnDelay / 5;
            MoveMobsCloser();
        }
    }
    public void DifficultyDown()
    {
        if (gm.tutorialPassed == 1)
        {
            if (maximumMobsAliveCurrentBonus > 0)
                maximumMobsAliveCurrentBonus--;
        }
    }
    

    Transform GetResourceSpawner() // in corridors
    {
        Spawner newSpawner = null;
        List<Spawner> tempSpawners = new List<Spawner>(spawnersOnProps);

        for (int i = tempSpawners.Count - 1; i >= 0; i--)
        {
            if (tempSpawners[i] != null)
            {
                float distanceToPlayer = 0;
                if (lg.generating)
                    distanceToPlayer = Vector3.Distance(tempSpawners[i].transform.position, lg.playerSpawner.transform.position);
                else
                    distanceToPlayer = Vector3.Distance(tempSpawners[i].transform.position, gm.player.transform.position);


                if (tempSpawners.Count > 1)
                {
                    if (distanceToPlayer < 30)
                    {
                        tempSpawners.RemoveAt(i);
                    }
                }   
            }
            else
            {
                tempSpawners.RemoveAt(i);
            }
        }

        if (tempSpawners.Count <= 1)
        {
            for (var index = 0; index < lg.levelTilesInGame.Count; index++)
            {
                var t = lg.levelTilesInGame[index];
                
                float distanceToPlayer = 0;
                if (lg.generating)
                    distanceToPlayer = Vector3.Distance(t.transform.position, lg.playerSpawner.transform.position);
                else
                    distanceToPlayer = Vector3.Distance(t.transform.position, gm.player.transform.position);

                if (distanceToPlayer > 50)
                    return t.spawner.transform;
            }
        }
        else
        {
            return tempSpawners[Random.Range(0, tempSpawners.Count)].transform;   
        }
        
        return lg.levelTilesInGame[Random.Range(0, lg.levelTilesInGame.Count)].spawner.transform;
    }

    Transform GetMobSpawner(bool npc) // everywhere except spawnerTiles
    {
        TileController newTile = null;
        List<TileController> tempTiles = new List<TileController>();
        for (var index = 0; index < lg.levelTilesInGame.Count; index++)
        {
            var t = lg.levelTilesInGame[index];
            if ((!npc || t.tileStatusEffect == StatusEffects.StatusEffect.Null) && t.spawnerTile 
                /*&& !t.propTile */ 
                && !t.trapTile && t.propOnTile == null)
            {
                float newDist = 0;
                if (lg.generating)
                    newDist = Vector3.Distance(t.transform.position, lg.playerSpawner.transform.position);
                else
                {
                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    {
                        // find closest player point
                        newDist = Vector3.Distance(t.transform.position, GLNetworkWrapper.instance.GetClosestPlayer(t.transform.position).transform.position);
                    }
                    else
                    {
                        // solo
                        newDist = Vector3.Distance(t.transform.position, gm.player.transform.position);
                    }
                }
                
                if (newDist > 50)
                {
                    bool canAdd = true;
                    for (int i = gm.units.Count - 1; i >= 0; i--)
                    {
                        if (gm.units[i] != null)
                        {
                            if (gm.units[i].wallMasterTile || gm.units[i].wallBlockerController)
                                continue;
                            
                            if (Vector3.Distance(gm.units[i].transform.position, t.transform.position) < 6.5f)
                            {
                                canAdd = false;
                                break;
                            }
                        }
                        else
                        {
                            gm.units.RemoveAt(i);
                        }
                    }

                    if (canAdd)
                        tempTiles.Add(t);
                }
            }
        }

        if (tempTiles.Count > 0)
        {
            int r = Random.Range(0, tempTiles.Count);
            newTile = tempTiles[r];   
        }
        else
        {
            tempTiles.Clear();
            for (int i = 0; i < lg.levelTilesInGame.Count; i++)
            {
                float newDist = 0;
                
                if (lg.generating)
                    newDist = Vector3.Distance(lg.levelTilesInGame[i].transform.position, lg.playerSpawner.transform.position);
                else
                    newDist = Vector3.Distance(lg.levelTilesInGame[i].transform.position, gm.player.transform.position);
                        
                if (newDist > 50)
                {
                    tempTiles.Add(lg.levelTilesInGame[i]);
                }
            }
        }

        if (tempTiles.Count > 0)
        {
            newTile = tempTiles[Random.Range(0, tempTiles.Count)];
        }
        else
        {
            newTile = lg.levelTilesInGame[Random.Range(lg.levelTilesInGame.Count / 2, lg.levelTilesInGame.Count)];
            print(newTile.transform.position);
        }

        newTile.RemoveProp();
        
        return newTile.transform;
    }

    Vector3 GetMobSpecificTileSpawner(StatusEffects.StatusEffect statusEffect)
    {
        TileController newTile = null;
        
        List<TileController> tempTiles = new List<TileController>();
        for (var index = 0; index < lg.levelTilesInGame.Count; index++)
        {
            var t = lg.levelTilesInGame[index];
            if (t.tileStatusEffect == statusEffect && !t.trapTile)
            {
                bool canAdd = true;
                for (int i = gm.units.Count - 1; i >= 0; i--)
                {
                    if (gm.units[i] != null)
                    {
                        if (gm.units[i].wallMasterTile || gm.units[i].wallBlockerController)
                            continue;
                        
                        if (Vector3.Distance(gm.units[i].transform.position, t.transform.position) < 6.5f)
                        {
                            canAdd = false;
                            break;
                        }
                    }
                    else
                    {
                        gm.units.RemoveAt(i);
                    }
                }

                if (canAdd)
                    tempTiles.Add(t);
            }
        }

        if (tempTiles.Count > 0)
        {
            int r = Random.Range(0, tempTiles.Count);
            newTile = tempTiles[r];

            if (newTile.propOnTile != null)
            {
                newTile.RemoveProp();
            }
        }
        else if (lg.levelTilesInGame.Count > 0)
        {
            newTile = lg.levelTilesInGame[Random.Range(lg.levelTilesInGame.Count / 2, lg.levelTilesInGame.Count)];
        }

        if (newTile)
        {
            newTile.RemoveProp();
            return newTile.transform.position;   
        }
        
        return Vector3.zero;
    }
    
    private sealed class ItemSpawnProvider
    {
        private readonly List<SubRoom> _subRooms;
        private readonly List<TileNode> _deadEnds;
        private readonly List<RoomController> _rooms;
        private readonly List<object> _processedSpawnSource = new List<object>();
        private bool _firstIteration = true;
        
        internal ItemSpawnProvider(List<SubRoom> subRooms, List<TileNode> deadEnds, List<RoomController> rooms)
        {
            _subRooms = subRooms;
            _deadEnds = deadEnds;
            _rooms = new List<RoomController>(rooms);
            
            // DONT SPAWN ANYTHING IN ROOM 0 (elevator)
            _rooms.RemoveAt(0);
            // REMOVE ELEVATOR ROOM
        }

        private static Transform Get(TileNode tile)
        {
            if (tile.value.propOnTile != null)
                return (tile.value.propOnTile.spawners[Random.Range(0, tile.value.propOnTile.spawners.Count)].transform);   
               
            return (tile.value.transform);
        }
        
        private static Transform GetTransformFromTile(TileController tile)
        {
            if (tile.propOnTile)
                return tile.propOnTile.spawners[Random.Range(0, tile.propOnTile.spawners.Count)].transform;
                
            return tile.transform;
        }
        
        internal Transform Get()
        {
            var deadEnds = _deadEnds //Spawn on dead ends first
                .Where(d => _firstIteration && !_processedSpawnSource.Contains(d) && d.value.spawnerTile)
                .ToList();
            if (deadEnds.Any())
            {
                TileNode spawnSource = null;
                
                for (int i = 0; i < deadEnds.Count; i++)
                {
                    float newDist = 0;
                    if (LevelGenerator.instance.generating)
                        newDist = Vector3.Distance(deadEnds[i].value.transform.position, LevelGenerator.instance.playerSpawner.transform.position);
                    else
                        newDist = Vector3.Distance(deadEnds[i].value.transform.position, GameManager.instance.player.transform.position);
                            
                    if (newDist > 75)
                    {
                        spawnSource = deadEnds[i];
                        break;
                    }   
                }

                if (spawnSource == null)
                    spawnSource = deadEnds[Random.Range(0, deadEnds.Count)];
                
                _processedSpawnSource.Add(spawnSource);

                return Get(spawnSource);
            }
            
            
            //No dead ends left, spawn in usual rooms
            var rooms = _rooms
            .Where(d => !_processedSpawnSource.Contains(d) 
                        && d.tiles
                            .Any(t => t.propOnTile != null && t.propOnTile.spawners.Count > 0))
            .ToList();
            if (rooms.Any())
            {
                var room = rooms[Random.Range(0, rooms.Count)];
                _processedSpawnSource.Add(room);

                var spawnSources = room.tiles
                    .Where(t => t.propOnTile != null && t.propOnTile.spawners.Count > 0)
                    .ToList();
                   
                return GetTransformFromTile(spawnSources[Random.Range(0, spawnSources.Count())]);
            }
            
            //No usual rooms left also, spawn in auto generated rooms.
            var subRooms = _subRooms
                .Where(d => _firstIteration && !_processedSpawnSource.Contains(d)
                            && d.nodes.Any(t => t.value.propOnTile != null
                                                && t.value.propOnTile.spawners.Count > 0)).ToList();
            if (subRooms.Any())
            {
                var subRoom = subRooms[Random.Range(0, subRooms.Count)];
                _processedSpawnSource.Add(subRoom);

                var spawnSources = subRoom.nodes
                    .Where(t => t.value.propOnTile != null && t.value.propOnTile.spawners.Count > 0)
                    .ToList();
                   
                return Get(spawnSources[Random.Range(0, spawnSources.Count())]);
            }

            //All spawn places was used once.
            _firstIteration = false;
            _processedSpawnSource.Clear();
            var lg = LevelGenerator.instance;
            return GetTransformFromTile(lg.levelTilesInGame[Random.Range(0, lg.levelTilesInGame.Count)]);
            ///
        }
    }

    public void MobKilled()
    {
        //SpawnMob(Random.Range(0, gm.levels[gm.currentLevel].mobsPool.Count));
        mobSpawnTimer = 0;
        if (mobsInGame.Count == 0)
            gm.DifficultyDown();
    }

    public void ClientStartChase()
    {
        // called ONLY on 2nd player
        
        chase = true;
        PlayerMovement.instance.StartAdrenaline();
        PlayerAudioController.instance.StartChase(false);
    }

    public void PlayerFoundAllKeys()
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && GLNetworkWrapper.instance.localPlayer.isServer == false)
            return;
        
        var pm = PlayerMovement.instance;
        if (gpm.GetChaseScene()) // chase
        {
            chase = true;
            pm.StartAdrenaline();
            StartCoroutine(SpawnMobsToChase());
            StartCoroutine(FinalChase());  
            PlayerAudioController.instance.StartChase(false);   
        }
        else
        {
            //StartCoroutine(sc.SpawnMobOnClientCoroutine());   
            SpawnMob();
            AttractMonsters(pm.transform.position, true);
        }
    }
    
    public void StartBadRepChase()
    {
        return;
        if (!chase)
        {
            if (gm.hub)
            {
                EndingController.instance.InfiniteDarkness();
            }
            chase = true;
            PlayerMovement.instance.StartAdrenaline();
            if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
            {
                // SOLO
                StartCoroutine(SpawnMobsToChase());
                StartCoroutine(FinalChase());   
            }  
            else if (GLNetworkWrapper.instance.localPlayer.isServer)
            {
                //ON HOST
                StartCoroutine(SpawnMobsToChase());
                StartCoroutine(FinalChase());
            }
            
            PlayerAudioController.instance.StartChase(true);   
        }  
    }

    IEnumerator FinalChase()
    {
        var pm = PlayerMovement.instance;
        while (true)
        {
            AttractMonsters(gm.player.transform.position, true);

            if (gm.tutorialPassed == 1)
            {
                yield return new WaitForSeconds(1);
                
                /*
                if (pm.hc.health > pm.hc.healthMax * 0.5f)
                    yield return new WaitForSeconds(1);
                else
                    yield return new WaitForSeconds(2);   */
            }
            else
            {
                yield return new WaitForSeconds(2);
            }
        }
    }

    public void StopSpawningOnLevel()
    {
        StopCoroutine(SpawnStartEnemies());
        for (int i = gm.units.Count - 1; i >= 0 ; i --)
        {
            if (gm.units[i].mobGroundMovement || gm.units[i].mobMeleeAttack || gm.units[i].mobJumperMovement || gm.units[i].mobHideInCorners)
            {
                gm.units[i].Damage(1000000, gm.units[i].transform.position + Vector3.up, gm.units[i].transform.position, null,
                    null,false,null,null,null, true);
            }
        }
    }

    public HealthController GetMeatHole(Vector3 closestPoint)
    {
        HealthController newHc = null;
        TileController closestTile = null;
        
        // find closestTile
        float distance = 100;
        float newDistance = 0;
        ElevatorController ec = ElevatorController.instance;
        
        for (int i = lg.levelTilesInGame.Count - 1; i >= 0; i--)
        {
            if (ec && Vector3.Distance(lg.levelTilesInGame[i].transform.position, ec.transform.position) <= 16)
                continue;
            
            newDistance = Vector3.Distance(closestPoint, lg.levelTilesInGame[i].transform.position);
            if (newDistance <= distance)
            {
                distance = newDistance;
                closestTile = lg.levelTilesInGame[i];
            }
        }

        if (closestTile != null)
        {
            newHc = Instantiate(meatHolePrefab, closestTile.transform.position + Vector3.up * Random.Range(1, 9),
                Quaternion.identity);

            if (closestTile.wallFront)
            {
                newHc.transform.position += Vector3.forward * (lg.tileSize / 2);
                newHc.transform.Rotate(Vector3.up, 180);
            }
            else if (closestTile.wallRight)
            {
                newHc.transform.position += Vector3.right * (lg.tileSize / 2);
                newHc.transform.Rotate(Vector3.up, 270);
            }
            else if (closestTile.wallBack)
            {
                newHc.transform.position += Vector3.back * (lg.tileSize / 2);
            }
            else if (closestTile.wallLeft)
            {
                newHc.transform.position += Vector3.left * (lg.tileSize / 2);
                newHc.transform.Rotate(Vector3.up, 90);
            }
            else
            {
                newHc.transform.position = new Vector3(newHc.transform.position.x, -0.5f, newHc.transform.position.z);
                newHc.transform.Rotate(Vector3.right, 270);
            }
            
            if (lg.levelgenOnHost)
            {
                NetworkServer.Spawn(newHc.gameObject);
            }
        }
        
        return newHc;
    }
    
    public void SpawnFloorBlade(TileController tile)
    {
        Vector3 newPos = Vector3.zero;
        if (tile == null)
            newPos = GetMobSpawner(false).position;
        else
        {
            newPos = tile.transform.position;
        }
        
        var newHc = Instantiate(floorBladePrefab, newPos, Quaternion.identity);
        if (lg.levelgenOnHost)
        {
            NetworkServer.Spawn(newHc.gameObject);
        }
            
    }

    public void SpawnMeatHoles()
    {
        for (int i = 0; i < newLevelData.meatHoles - newLevelData.mainItemsInMeatHoles; i++)
        {
            GetMeatHole(RandomPositionOnLevel());
        }
        
        int holes = newLevelData.mainItemsInMeatHoles;
        var roomsTemp = new List<RoomController>(lg.roomsInGame);
        roomsTemp.RemoveAt(0);
        int r = Random.Range(0, roomsTemp.Count);
        var tilesTemp = new List<TileController>(roomsTemp[r].tiles);
        
        for (int j = il.interactables.Count - 1; j >= 0 && holes > 0; j--)
        {
            
            var interactable = il.interactables[j];
            if (Vector3.Distance(Vector3.zero, interactable.transform.position) > 50)
                continue;
            
            if (interactable.pickUp && (interactable.pickUp.skill ||
                                         interactable.pickUp.tool ||
                                         interactable.pickUp.resourceType ==
                                         ItemsList.ResourceType.Key))
            {
                int index = MoveInteractableOutsideTheLevel(interactable, tilesTemp);
                if (index > -1)
                {
                    holes--;
                    tilesTemp.RemoveAt(index);
                }
            }
        }
        
        tilesTemp.Clear();
        for (int i = 0; i < keysOnLevel.Count; i++)
        {
            for (int t = 0; t < lg.levelTilesInGame.Count; t++)
            {
                if (!gm.arena)
                {
                    if (Vector3.Distance(lg.levelTilesInGame[t].transform.position, Vector3.zero) > 50)
                    {
                        tilesTemp.Add(lg.levelTilesInGame[t]);
                    }   
                }
                else
                {
                    tilesTemp.Add(lg.levelTilesInGame[t]);
                }
            }
            
            if (Vector3.Distance(keysOnLevel[i].transform.position, Vector3.zero) < 30)
            {
                MoveInteractableOutsideTheLevel(keysOnLevel[i], tilesTemp);
            }
        }
    }

    public void SpawnRandomWeapon()
    {
        if (newLevelData.spawnGroups[2].weapons.Length > 0)
            Instantiate(
                newLevelData.spawnGroups[2].weapons[Random.Range(0, newLevelData.spawnGroups[2].weapons.Length)].weapon.value,
                gm.player.transform.position, Quaternion.identity);
    }

    int MoveInteractableOutsideTheLevel(Interactable interactable, List<TileController> tilesTemp)
    {
        bool found = false;
        for (int t = tilesTemp.Count - 1; t >= 0; t--)
        {
            if (tilesTemp[t].wallsAmount > 0)
            {
                var tile = tilesTemp[t];
                if (tile.wallFront)
                {
                    interactable.transform.position = tile.transform.position + Vector3.forward * lg.tileSize;
                    found = true;
                    interactable.rb.isKinematic = true;
                    interactable.rb.constraints = RigidbodyConstraints.FreezeAll;
                }   
                else if (tile.wallRight)
                {
                    interactable.transform.position = tile.transform.position + Vector3.right * lg.tileSize;
                    found = true;
                    interactable.rb.isKinematic = true;
                    interactable.rb.constraints = RigidbodyConstraints.FreezeAll;
                }   
                else if (tile.wallBack)
                {
                    interactable.transform.position = tile.transform.position + Vector3.back * lg.tileSize;
                    found = true;
                    interactable.rb.isKinematic = true;
                    interactable.rb.constraints = RigidbodyConstraints.FreezeAll;
                }   
                else if (tile.wallLeft)
                {
                    interactable.transform.position = tile.transform.position + Vector3.left * lg.tileSize;
                    interactable.rb.isKinematic = true;
                    interactable.rb.constraints = RigidbodyConstraints.FreezeAll;
                    found = true;
                }   
            }

            if (found)
            {
                return t;
            }
        }
        return -1;
    }


    Vector3 RandomPositionOnLevel()
    {
        var newPos = Vector3.zero;

        newPos = new Vector3(Random.Range(-lg.width * lg.tileSize, lg.width * lg.tileSize), 1,
            Random.Range(-lg.length * lg.tileSize, lg.length * lg.tileSize));
        
        return newPos;
    }

    public void FinishLevel()
    {
        StopAllCoroutines();
    }

}
