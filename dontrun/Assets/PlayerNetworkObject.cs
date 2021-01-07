using System.Collections;
using System.Collections.Generic;
using Mirror;
using PlayerControls;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerNetworkObject : NetworkBehaviour 
{
    [SyncVar]
    public bool isReady = true;   
    [SyncVar]
    public bool levelGenExists = false;
    [SyncVar]
    public int roomsAmount = 0;
    [SyncVar]
    public int tilesAmount = 0;
    [SyncVar]
    public int propsAmount = 0;
    
    int weaponInArmsIndexNew = -1;
    [SyncVar]
    public int weaponInArmsIndex = -1;
    
    bool noseSkillNew = false;
    [SyncVar]
    public bool noseSkill = false;
    
    
    int toolInArmsIndexNew = -1;
    [SyncVar]
    public int toolInArmsIndex = -1;

    public PlayerNetworkDummyController connectedDummy;

    public PlayerNetworkDummyController playerNetworkDummyControllerPrefab;

    private Coroutine getInventoryCoroutine;

    
    public void GetPlayerInventory()
    {
        if (getInventoryCoroutine != null)
        {
            StopCoroutine(getInventoryCoroutine);
        }
        
        getInventoryCoroutine = StartCoroutine(GetPlayerInventoryCoroutine());
    }
    public IEnumerator GetPlayerInventoryCoroutine()
    {
        while (GLNetworkWrapper.instance.coopIsActive)
        {
            if (WeaponControls.instance != null)
            {
                if (WeaponControls.instance.activeWeapon == null)
                {
                    weaponInArmsIndexNew = -1;
                }
                else
                {
                    switch (WeaponControls.instance.activeWeapon.weapon)
                    {
                        case WeaponPickUp.Weapon.Axe:
                            weaponInArmsIndexNew = 0;
                            break;
                        case WeaponPickUp.Weapon.Pistol:
                            weaponInArmsIndexNew = 1;
                            break;
                        case WeaponPickUp.Weapon.Revolver:
                            weaponInArmsIndexNew = 2;
                            break;
                        case WeaponPickUp.Weapon.Shotgun:
                            weaponInArmsIndexNew = 3;
                            break;
                        case WeaponPickUp.Weapon.TommyGun:
                            weaponInArmsIndexNew = 4;
                            break;
                        case WeaponPickUp.Weapon.Pipe:
                            weaponInArmsIndexNew = 5;
                            break;
                        case WeaponPickUp.Weapon.Map:
                            weaponInArmsIndexNew = 6;
                            break;
                        case WeaponPickUp.Weapon.Knife:
                            weaponInArmsIndexNew = 7;
                            break;
                        case WeaponPickUp.Weapon.Torch:
                            weaponInArmsIndexNew = 8;
                            break;
                        case WeaponPickUp.Weapon.Polaroid:
                            weaponInArmsIndexNew = 9;
                            break;
                        case WeaponPickUp.Weapon.Shield:
                            weaponInArmsIndexNew = 10;
                            break;
                        case WeaponPickUp.Weapon.MeatSpear:
                            weaponInArmsIndexNew = 11;
                            break;
                        case WeaponPickUp.Weapon.OldPistol:
                            weaponInArmsIndexNew = 12;
                            break;
                        case WeaponPickUp.Weapon.VeinWhip:
                            weaponInArmsIndexNew = 13;
                            break;
                        default:
                            weaponInArmsIndexNew = -1;
                            break;
                    }
                }

                int index = WeaponControls.instance.currentToolIndex;
                if (ItemsList.instance.savedTools.Count > 0 && ItemsList.instance.savedTools[index].amount > 0)
                    toolInArmsIndexNew = index;
                else
                    toolInArmsIndexNew = -1;

                noseSkillNew = PlayerSkillsController.instance.noseWithTeeth;

                if (weaponInArmsIndexNew != weaponInArmsIndex || toolInArmsIndexNew != toolInArmsIndex || noseSkillNew != noseSkill)
                {
                    weaponInArmsIndex = weaponInArmsIndexNew;
                    toolInArmsIndex = toolInArmsIndexNew;
                    noseSkill = noseSkillNew;
                    CmdSetWeaponAndTool(weaponInArmsIndex, toolInArmsIndex, noseSkill);
                }
            }
            else
            {
                weaponInArmsIndex = -1;
                toolInArmsIndex = -1;
                noseSkill  = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    [Command]
    void CmdSetWeaponAndTool(int _weaponIndex, int _toolIndex, bool nose)
    {
        RpcSetWeaponAndTool(_weaponIndex, _toolIndex, nose);
    }

    [ClientRpc]
    void RpcSetWeaponAndTool(int _weaponIndex, int _toolIndex, bool nose)
    {
        weaponInArmsIndex = _weaponIndex;
        toolInArmsIndex = _toolIndex;
        
        if (connectedDummy != null)
            connectedDummy.dwc.SetWeaponAndTool(_weaponIndex, _toolIndex, nose);   
    }

    public void PlayMobSoundOnClient(int mobAuIndex, int action)
    {
        CmdPlayMobSoundOnClient(mobAuIndex, action);
    }
    [Command]
    void CmdPlayMobSoundOnClient(int mobAuIndex, int action)
    {
        RpcPlayMobSoundOnClient(mobAuIndex, action);
    }
    [ClientRpc]
    void RpcPlayMobSoundOnClient(int mobAuIndex, int action)
    {
        var _sc = SpawnController.instance;
        
        if (_sc.mobAudioManagers.Count > mobAuIndex && _sc.mobAudioManagers[mobAuIndex] != null)
            _sc.mobAudioManagers[mobAuIndex].PlaySoundOnClient(action);
    }

    public void StopInventoryCoroutine()
    {
        if (getInventoryCoroutine != null)
            StopCoroutine(getInventoryCoroutine);
    }

    public void NextLevelOnServer()
    {
        CmdNextLevelOnServer();
    }

    [Command]
    void CmdNextLevelOnServer()
    {
        ElevatorController.instance.elevatorButton.reciever.NextLevelOnServer();
    }

    public void Disconnect()
    {
        
    }
    
    public void StartCoopGame(int floor, int bossFloor, bool restart)
    {
        roomsAmount = 0;
        tilesAmount = 0;
        
        CmdStartCoopGame(floor, bossFloor, restart);
    }

    [Command]
    void CmdStartCoopGame(int floor, int bossFloor, bool restart)
    {
        RpcStartLoadingLevel(floor, bossFloor, restart);
    }

    [ClientRpc]
    void RpcStartLoadingLevel(int floor, int bossFloor, bool restart)
    {
        // set floor to client
        print("SET FLOOR " + floor + " TO CLIENT");
        GutProgressionManager.instance.SetFloorToClient(floor, bossFloor);
    
        print ("client got message to start loading a scene");
        StartCoroutine(GameManager.instance.LoadCoopGameScene(restart)); 
    }

    public void MeleeAttackSelf()
    {
        СmdAttack(true);
    }
    public void MeleeAttack()
    {
        СmdAttack(false);
    }
    public void RangeAttackSelf()
    {
        СmdAttack(true);
    }
    public void RangeAttack()
    {
        СmdAttack(false);
    }


    [Command]
    void СmdAttack(bool self)
    {
        RpcAttack(self);
    }

    [ClientRpc]
    void RpcAttack(bool self)
    {
        if (GLNetworkWrapper.instance.localPlayer != this && connectedDummy != null)
            connectedDummy.dwc.Attack(self);
    }

    public void EatWeapon()
    {
        CmdEatWeapon();
    }
    [Command]
    void CmdEatWeapon()
    {
        RpcEatWeapon();
    }
    [ClientRpc]
    void RpcEatWeapon()
    {
        if (GLNetworkWrapper.instance.localPlayer != this && connectedDummy != null)
            connectedDummy.dwc.EatWeapon();
    }
    
    public void ThrowTool()
    {
        CmdUseTool(true);
    }

    public void EatTool()
    {
        CmdUseTool(false);
    }

    [Command]
    void CmdUseTool(bool _throw)
    {
        RpcUseTool(_throw);
    }

    [ClientRpc]
    void RpcUseTool(bool _throw)
    {
        if (GLNetworkWrapper.instance.localPlayer != this && connectedDummy != null)
            connectedDummy.dwc.UseTool(_throw);
    }
    public void SpawnObjectOnServer(GameObject objToSpawn)
    {
        NetworkServer.Spawn(objToSpawn);
    }
    
    public void AddRoom()
    {
        roomsAmount = LevelGenerator.instance.roomsInGame.Count;
        CmdSetRoomsAmount(roomsAmount);
    }
    [Command]
    void CmdSetRoomsAmount(int amount)
    {
        roomsAmount = amount;
    }
    
    public void AddTile()
    {
        tilesAmount = LevelGenerator.instance.levelTilesInGame.Count;
        CmdSetTilesAmount(tilesAmount);
    }
    [Command]
    void CmdSetTilesAmount(int amount)
    {
        tilesAmount = amount;
    }
    
    public void AddProp()
    {
        propsAmount = LevelGenerator.instance.corridorPropsInGame.Count;
        CmdAddProp(propsAmount);
    }
    [Command]
    void CmdAddProp(int amount)
    {
        propsAmount = amount;
    }

    public void SendToMeatHole(GameObject go, GameObject meatHole)
    {
        CmdSendToMeatHole(go, meatHole);
    }
    [Command]
    void CmdSendToMeatHole(GameObject go, GameObject meatHole)
    {
        RpcSendToMeatHole(go, meatHole);
    }
    [ClientRpc]
    void RpcSendToMeatHole(GameObject go, GameObject meatHole) 
    {
        if (go == null) return;

        var interactable = go.GetComponent<Interactable>();
        if (interactable) interactable.SendToMeatHoleOnClient(meatHole);
    }
    
    
    public void InitCoopLevelOnClient()
    {
        RpcInitCoopLevelOnClient();
    }

    [ClientRpc]
    void RpcInitCoopLevelOnClient()
    {
        if (GLNetworkWrapper.instance.localPlayer == this)
            StartCoroutine(GameManager.instance.InitCoopLevelOnClient());
    }
    
    public void LoadCoopSceneOnServer(bool restart)
    {
        CmdLoadCoopSceneOnServer(restart);
    }
    [Command]
    void CmdLoadCoopSceneOnServer(bool restart)
    {
        if (GLNetworkWrapper.instance.localPlayer == this)
            StartCoroutine(LoadCoopScene(restart));
    }

    public void SwapProps()
    {
        CmdSwapProps();
    }

    [Command]
    void CmdSwapProps()
    {
        PlayerSkillsController.instance.SwapPropsOnServer();
    }
    
    IEnumerator LoadCoopScene(bool restart)
    {
        #region old way to load the scene
        /*
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

        yield return SceneManager.LoadSceneAsync(4, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        NetworkServer.SendToClientOfPlayer(GLNetworkWrapper.instance.playerNetworkObjects[1].netIdentity, new SceneMessage { sceneName = SceneManager.GetSceneByBuildIndex(4).name, sceneOperation = SceneOperation.LoadAdditive});
        //SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        
        StartCoroutine(GameManager.instance.GenerateCoopLevelOnServer(restart));
    }

    public void LevelCompleted()
    {
        CmdLevelCompleted();
    }

    [Command]
    void CmdLevelCompleted()
    {
        RpcLevelCompleted();
    }

    [ClientRpc]
    void RpcLevelCompleted()
    {
        levelGenExists = false;
        PlayerMovement.instance.hc.invincible = true;
        PlayerMovement.instance.goldenLightAnimator.SetBool("GoldenLight", true);
        PlayerMovement.instance.controller.enabled = false;
        
        GameManager.instance.SaveGame();
        CmdBothPlayersDied(false);

        GLNetworkWrapper.instance.StopGettingTheInventory();
    }
    
    public void BothPlayersDied(bool restart)
    {
        CmdBothPlayersDied(restart);
    }

    [Command]
    void CmdBothPlayersDied(bool restart)
    {
        print("Everyone is dead");
        //if (GLNetworkWrapper.instance.localPlayer == this)
            StartCoroutine(BothPlayersDiedCoroutine( restart));
    }

    IEnumerator BothPlayersDiedCoroutine(bool restart)
    {
        yield return new WaitForSeconds(2);
        print("unload the scene");
        if (restart)
        {
            SteamworksLobby lobby = SteamworksLobby.instance;
            int newFloor = 1; 
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
            
            GutProgressionManager.instance.SetLevel(newFloor,lobby.selectedCheckpoint);
            GutProgressionManager.instance.bossFloor = newFloor + 2;
        }

        print("[COOP] load scene???");
        NetworkServer.SendToClientOfPlayer(GLNetworkWrapper.instance.playerNetworkObjects[1].netIdentity, new SceneMessage { sceneName = SceneManager.GetSceneAt(1).name, sceneOperation = SceneOperation.UnloadAdditive});
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(4));
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
        
        GLNetworkWrapper.instance.StartCoopGame(restart);
    }

    public void CreateExplosion(Vector3 pos)
    {
        CmdCreateExplosion(pos);
    }
    [Command]
    void CmdCreateExplosion(Vector3 pos)
    {
        var explosion = Instantiate(ItemsList.instance.savedTools[0].toolController.grenadeExplosion, pos, Quaternion.identity);
        explosion.DestroyEffect(true);
        NetworkServer.Spawn(explosion.gameObject);
    }

    public void TogglePickUpBrain(GameObject brainGo, bool pickedUp)
    {
        CmdTogglePickUpBrain(brainGo, pickedUp);
    }

    [Command]
    void CmdTogglePickUpBrain(GameObject brainG, bool active)
    {
        RpcTogglePickUpBrain(brainG, active);
    }

    [ClientRpc]
    void RpcTogglePickUpBrain(GameObject brain, bool active)
    {
        if (brain == null) return;

        var b = brain.GetComponent<Interactable>();
        if (b) b.meatBrain.TogglePickUpBrainOnClient(active);
    }

    public void ElevatorShaftStopLoading()
    {
        CmdElevatorShaftStopLoading();
    }
    [Command]
    void CmdElevatorShaftStopLoading()
    {
        RpcElevatorShaftStopLoading();
    }
    [ClientRpc]
    void RpcElevatorShaftStopLoading()
    {
        ElevatorShaftController.instance.StopLoadingOnClient();
    }
    
    public void PlayerFinishedLevel()
    {
        CmdPlayerFinishedLevel();
    }
    [Command]
    void CmdPlayerFinishedLevel()
    {
        RpcPlayerFinishedLevel();
    }
    [ClientRpc]
    void RpcPlayerFinishedLevel()
    {
        GutProgressionManager.instance.PlayerFinishedLevelOnClient();
    }
    
    public void DropWeapon(int weaponEnumIndex, float ammoClip, float durability, List<string> generatedName,List<string> generatedDescriptions, int r1, int r2, int r3, int r4, int statusEffect, bool dead, bool npc, bool npcPaid,
        List<int> savedBarrelslvl_1, List<int> savedBarrelslvl_2, Vector3 eulerAngles, bool crazyGun, float clipSize, int barrelsCount, Vector3 pos)
    {
            CmdDropWeapon(weaponEnumIndex, ammoClip, durability, generatedName, generatedDescriptions, r1, r2, r3, r4,
                statusEffect, dead, npc, npcPaid, savedBarrelslvl_1, savedBarrelslvl_2, eulerAngles,
                crazyGun, clipSize, barrelsCount, pos);
    }
    [Command]
    public void CmdDropWeapon(int weaponEnumIndex, float ammoClip, float durability, List<string> generatedName,List<string> generatedDescriptions, int r1, int r2, int r3, int r4, int statusEffect, bool dead, bool npc, bool npcPaid,
        List<int> savedBarrelslvl_1, List<int> savedBarrelslvl_2, Vector3 eulerAngles, bool crazyGun, float clipSize, int barrelsCount, Vector3 pos)
    {
        RpcDropWeapon(weaponEnumIndex, ammoClip, durability, generatedName, generatedDescriptions, r1, r2, r3, r4,
            statusEffect, dead, npc, npcPaid, savedBarrelslvl_1, savedBarrelslvl_2, eulerAngles,
            crazyGun, clipSize, barrelsCount, pos);
    }
    [ClientRpc]
    public void RpcDropWeapon(int weaponEnumIndex, float ammoClip, float durability, List<string> generatedName,List<string> generatedDescriptions, int r1, int r2, int r3, int r4, int statusEffect, bool dead, bool npc, bool npcPaid,
        List<int> savedBarrelslvl_1, List<int> savedBarrelslvl_2, Vector3 eulerAngles, bool crazyGun, float clipSize, int barrelsCount, Vector3 pos)
    {
        WeaponControls.instance.DropWeaponOnClient(weaponEnumIndex, ammoClip, durability, generatedName, generatedDescriptions, r1, r2, r3, r4,
            statusEffect, dead, npc, npcPaid, savedBarrelslvl_1, savedBarrelslvl_2, eulerAngles,
            crazyGun, clipSize, barrelsCount, pos);
    }

    public void SetCultistOnClient(int unitIndex, int effect, bool leader)
    {
        CmdSetCultistOnClient(unitIndex, effect, leader);
    }

    [Command]
    void CmdSetCultistOnClient(int unitIndex, int effect, bool leader)
    {
        RpcSetCultistOnClient(unitIndex, effect, leader);
    }

    [ClientRpc]
    void RpcSetCultistOnClient(int unitIndex, int effect, bool leader)
    {
        CultGenerator.instance.SetCultistOnClient(unitIndex, effect, leader);
    }

    public void FoundKey(int playerId)
    {
        CmdFoundKey(playerId);
    }

    [Command]
    void CmdFoundKey(int playerId)
    {
        RpcFoundKey(playerId);
    }

    [ClientRpc]
    void RpcFoundKey(int playerId)
    {
        ItemsList.instance.FoundKeyOnClient(playerId);
    }

    public void AttractMonsters(Vector3 target, bool chase)
    {
        if (isServer)
            CmdAttractMonsters(target, chase);
    }

    [Command]
    void CmdAttractMonsters(Vector3 target, bool chase)
    {
        SpawnController.instance.AttractMonstersOnServer(target, chase);
    }

    public void TimeToLeaveFloorFeedbackOnClient()
    {
        CmdTimeToLeaveFloorFeedbackOnClient();
    }
    [Command]
    void CmdTimeToLeaveFloorFeedbackOnClient()
    {
        RpcTimeToLeaveFloorFeedbackOnClient();
    }
    [ClientRpc]
    void RpcTimeToLeaveFloorFeedbackOnClient()
    {
        UiManager.instance.TimeToLeaveFloorFeedbackOnClient();
    }
    
    public void SaveRandomObject(int index, int savedObjectIndex, bool activate)
    {
        CmdSaveRandomObject(index,  savedObjectIndex, activate);
    }

    [Command]
    void CmdSaveRandomObject(int index, int savedObjectIndex, bool activate)
    {
        RpcSaveRandomObject(index, savedObjectIndex, activate);
    }

    [ClientRpc]
    void RpcSaveRandomObject(int index, int savedObjectIndex, bool activate)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
        StartCoroutine(WaitUntilRandomObjectActivatorIsSpawned(index, savedObjectIndex, activate));
    }

    IEnumerator WaitUntilRandomObjectActivatorIsSpawned(int index, int savedObjectIndex, bool activate)
    {
        while (LevelGenerator.instance.randomObjectsActivators.Count <= index)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (LevelGenerator.instance.randomObjectsActivators[index] != null)
        {
            LevelGenerator.instance.randomObjectsActivators[index].SaveRandomObjectOnClient(savedObjectIndex);
        
            if (activate)
                LevelGenerator.instance.randomObjectsActivators[index].ActivateRandomObjectOnClient();   
        }
    }

    public void SetFlashbackSceneIndex(int index)
    {
        CmdSetFlashbackSceneIndex(index);
    }

    [Command]
    void CmdSetFlashbackSceneIndex(int index)
    {
        RpcSetFlashbackSceneIndex(index);
    }

    [ClientRpc]
    void RpcSetFlashbackSceneIndex(int index)
    {
        GameManager.instance.flashbackSceneIndex = index;
    }

    public void ActivateSavedRandomObjectsActivators()
    {
        CmdActivateSavedRandomObjectsActivators();
    }

    [Command]
    void CmdActivateSavedRandomObjectsActivators()
    {
        RpcActivateSavedRandomObjectsActivators();
    }

    [ClientRpc]
    void RpcActivateSavedRandomObjectsActivators()
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
            StartCoroutine(LevelGenerator.instance.ActivateSavedRandomObjectsActivatorsOnClient());
    }
    
    /*
    IEnumerator FindLevelGenLastResort()
    {
        yield return new WaitForSeconds(1f);
        if (GLNetworkWrapper.instance.localPlayer)
        {
            var lg = LevelGenerator.instance;
            while (lg == null)
            {
                yield return new WaitForSeconds(0.1f);
                lg = LevelGenerator.instance;
                print("LOCAL LEVEL GENERATOR IS " + lg);
            }

            print("LevelGenerator.instance is " + lg);
        }

        levelGenExists = true;
    }
    */

    public void DeathOnClient(GameObject o, bool affectReputation)
    {
        CmdDeathOnClient(o, affectReputation);
    }

    [Command]
    void CmdDeathOnClient(GameObject o, bool affectReputation)
    {
        RpcDeathOnClient(o, affectReputation);
    }

    [ClientRpc]
    void RpcDeathOnClient(GameObject o, bool affectReputation)
    {
        for (int i = GameManager.instance.units.Count - 1; i >= 0; i--)
        {
            var unit = GameManager.instance.units[i];
            if (unit.gameObject == o)
            {
                StartCoroutine(unit.DeathOnClient(null, affectReputation));
                break;
            }
        }
        //StartCoroutine(GameManager.instance.units[unitIndex].DeathOnClient(null));
    }
    
    public void MakeFloorTrap(int index)
    {
        CmdMakeFloorTrap(index);
    }
    [Command]
    void CmdMakeFloorTrap(int index)
    {
        RpcMakeFloorTrap(index);
    }
    [ClientRpc]
    void RpcMakeFloorTrap(int index)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
            LevelGenerator.instance.levelTilesInGame[index].MakeTrapOnClient();
    }
    
    public void SpawnCorridorProp(int tileIndex, int corridorPropIndex)
    {
        CmdSpawnCorridorProp(tileIndex,corridorPropIndex);
    }
    [Command]
    void CmdSpawnCorridorProp(int tileIndex,int corridorPropIndex)
    {
        RpcSpawnCorridorProp(tileIndex,corridorPropIndex);
    }
    [ClientRpc]
    void RpcSpawnCorridorProp(int tileIndex,int corridorPropIndex)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
            StartCoroutine(LevelGenerator.instance.levelTilesInGame[tileIndex].SpawnCorridorPropOnClient(corridorPropIndex));
    }

    public void RespawnDeadPlayer()
    {
        CmdRespawnDeadPlayer();
    }
    [Command]
    void CmdRespawnDeadPlayer()
    {
        print("FUCK YOU CMD");
        RpcRespawnDeadPlayer();
    }
    [ClientRpc]
    void RpcRespawnDeadPlayer()
    {
        print("FUCK YOU RPC");
        IronMaidenController.instance.ReleasePlayerOnClient();
    }

    
    public void SpawnRoomProp(int tileIndex, int RoomPropIndex)
    {
        CmdSpawnRoomProp(tileIndex, RoomPropIndex);
    }
    [Command]
    void CmdSpawnRoomProp(int tileIndex,int RoomPropIndex)
    {
        RpcSpawnRoomProp(tileIndex,RoomPropIndex);
    }
    [ClientRpc]
    void RpcSpawnRoomProp(int tileIndex,int RoomPropIndex)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
        StartCoroutine(LevelGenerator.instance.levelTilesInGame[tileIndex].SpawnRoomPropOnClient(RoomPropIndex));
    }

    public void SaveTool(int description, int effect)
    {
        CmdSaveTool(description, effect);
    }

    [Command]
    void CmdSaveTool(int desc, int effect)
    {
        RpcSaveTool(desc, effect);
    }
    [ClientRpc]
    void RpcSaveTool(int desc, int effect)
    {
        ToolsRandomizer.instance.SaveToolOnClient(desc, effect);
    }
    
    public void SetTileEffect(int tileIndex, int effectIndex, float lifeTime)
    {
        CmdSetTileEffect(tileIndex,effectIndex, lifeTime);
    }
    [Command]
    void CmdSetTileEffect(int tileIndex, int effectIndex, float lifeTime)
    {
        RpcSetTileEffect(tileIndex, effectIndex, lifeTime);
    }
    [ClientRpc]
    void RpcSetTileEffect(int tileIndex, int effectIndex, float lifeTime)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
            LevelGenerator.instance.levelTilesInGame[tileIndex].SetStatusEffectOnTileOnClient(effectIndex, lifeTime);
    }

    public void UnitHealthChanged(GameObject go, float healthOffset, int effect, bool badRep)
    {
        CmdUnitHealthChanged(go, healthOffset, effect, badRep);
    }
    [Command]
    void CmdUnitHealthChanged(GameObject go, float offset, int effect, bool badRep)
    {
        RpcUnitHealthChanged(go, offset, effect, badRep);
    }
    [ClientRpc]
    void RpcUnitHealthChanged(GameObject go, float offset, int effect, bool badRep)
    {
        if (go == null) return;
            
        var h = go.GetComponent<HealthController>();
        
        if (h) h.ChangeHealthOnClient(offset, effect, badRep);

        #region old
        /*
        var gm = GameManager.instance;

        if (gm.units.Count > index && gm.units[index].health > 0)
        {
            //print("DEAL DAMAGE TO UNIT. Index: " + index + ". Offset: " + offset);
            gm.units[index].ChangeHealthOnClient(offset, effect, badRep);   
        }
        else
        {
            print("CANT DEAL DAMAGE. Index: " + index + ". Offset: " + offset);
        }
        */
        #endregion
    }

    public void UseKeyOnLock(int lockIndex, int playerWhoUsedKey)
    {
        CmdUseKeyOnLock(lockIndex, playerWhoUsedKey);
    }
    [Command]
    void CmdUseKeyOnLock(int lockIndex, int playerWhoUsedKey)
    {
        RpcUseKeyOnLock(lockIndex, playerWhoUsedKey);
    }
    [ClientRpc]
    void RpcUseKeyOnLock(int lockIndex, int playerWhoUsedKey)
    {
        var l = ItemsList.instance.interactables[lockIndex];
        bool useKey = GLNetworkWrapper.instance.playerNetworkObjects[playerWhoUsedKey] == GLNetworkWrapper.instance.localPlayer ==
                      this;

        if (l && l.lockObject)
        {
            l.lockObject.UseKeyOnClient(useKey);
        }
    }

    public void ButtonPressed()
    {
        CmdButtonPressed();
    }

    [Command]
    void CmdButtonPressed()
    {
        RpcButtonPressed();
    }

    [ClientRpc]
    void RpcButtonPressed()
    {
        GameManager.instance.SaveGame();
        ElevatorController.instance.elevatorButton.reciever.ButtonPressedOnClient(true);
    }
    
    /*
    public void RemoveUnitOnClient(int unitIndex)
    {
        CmdRemoveUnitOnClient(unitIndex);
    }
    [Command]
    void CmdRemoveUnitOnClient(int index)
    {
        RpcRemoveUnitOnClient(index);
    }
    [ClientRpc]
    void RpcRemoveUnitOnClient(int index)
    {
        var gm = GameManager.instance;

        print("TRY TO REMOVE UNIT. Index: " + index + ". Units amount: " + gm.units.Count);
        if (gm.units.Count > index)
        {
            gm.units[index].RemoveUnitOnClient();   
        }
        else
        {
            print("CANT REMOVE UNIT. Index: " + index + ". Units amount: " + gm.units.Count);
        }
    }
    */

    public void SpawnMob()
    {
        CmdSpawnMob();
    }

    [Command]
    void CmdSpawnMob()
    {
        StartCoroutine(SpawnController.instance.SpawnMobOnClientCoroutine());
    }
    
    public void TurnPropOnTile(int tileIndex, float angle)
    {
        CmdTurnPropOnTile(tileIndex, angle);
    }
    [Command]
    void CmdTurnPropOnTile(int tileIndex, float angle)
    {
        RpcTurnPropOnTile(tileIndex, angle);
    }
    [ClientRpc]
    void RpcTurnPropOnTile(int tileIndex, float angle)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)    
        StartCoroutine(WaitUntillTileSpawnedTheProp(tileIndex, angle));
    }

    IEnumerator WaitUntillTileSpawnedTheProp(int tileIndex, float angle)
    {
        while (LevelGenerator.instance.levelTilesInGame.Count <= tileIndex || LevelGenerator.instance.levelTilesInGame[tileIndex].propOnTile == null)
        {
            print("WAIT UNTIL PROP SPAWNED ON TILE " + tileIndex);
            yield return new WaitForSeconds(0.1f);
        }
        LevelGenerator.instance.levelTilesInGame[tileIndex].TurnPropOnServer(angle);
    }

    public void StartLevel()
    {
        CmdStartLevel();
    }

    [Command]
    void CmdStartLevel()
    {
        RpcStartLevel();
    }
    [ClientRpc]
    void RpcStartLevel()
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
        LevelGenerator.instance.StartLevel();

        if (LevelGenerator.instance.levelgenOnHost)
        {
            var playerDummy = Instantiate(playerNetworkDummyControllerPrefab);
            playerDummy.player = 0;
            NetworkServer.Spawn(playerDummy.gameObject, GLNetworkWrapper.instance.playerNetworkObjects[0].gameObject);
            //GLNetworkWrapper.instance.playerNetworkObjects[0].connectedDummy = playerDummy;
            
            var partnerDummy = Instantiate(playerNetworkDummyControllerPrefab);
            partnerDummy.player = 1;
            NetworkServer.Spawn(partnerDummy.gameObject, GLNetworkWrapper.instance.playerNetworkObjects[1].gameObject);  
            //GLNetworkWrapper.instance.playerNetworkObjects[1].connectedDummy = partnerDummy;
        }

        
        GLNetworkWrapper.instance.localPlayer.GetPlayerInventory();
        
        /*
        if (isLocalPlayer)
        {
            StartCoroutine(GetPlayerInventory());
        }
        */
    }

    public void PlayerCrouch(int player, bool crouch)
    {
        CmdPlayerCrouch(player, crouch);
    }

    [Command]
    void CmdPlayerCrouch(int player, bool crouch)
    {
        RpcPlayerCrouch(player, crouch);
    }

    [ClientRpc]
    void RpcPlayerCrouch(int player, bool crouch)
    {
        if (GLNetworkWrapper.instance.playerNetworkObjects[player] && GLNetworkWrapper.instance.playerNetworkObjects[player].connectedDummy != null)
        {
            GLNetworkWrapper.instance.playerNetworkObjects[player].connectedDummy.hc.mobPartsController.SetSneak(crouch);
            GLNetworkWrapper.instance.playerNetworkObjects[player].connectedDummy.SetHiding(crouch);
        }
    }
    
    public void ClearTilesObjects(int tileIndex)
    {
        CmdClearTilesObjects(tileIndex);
    }

    [Command]
    void CmdClearTilesObjects(int tileIndex)
    {
        RpcClearTilesObjects(tileIndex);
    }
    [ClientRpc]
    void RpcClearTilesObjects(int tileIndex)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
        LevelGenerator.instance.levelTilesInGame[tileIndex].ClearTileObjectsOnClient();    
    }

    public void DummyHealthChanged(int index, float healthOffset, int effect)
    {
        print("DUMMY #" + index + " TRY TO CHANGE HIS HEALTH ON " + healthOffset);
        CmdDummyHealthChanged(index, healthOffset, effect);
    }

    [Command]
    void CmdDummyHealthChanged(int index, float healthOffset, int effect)
    {
        print("CMD DUMMY #" + index + " TRY TO CHANGE HIS HEALTH ON " + healthOffset);
        RpcDummyHealthChanged(index, healthOffset, effect);
    }

    [ClientRpc]
    void RpcDummyHealthChanged(int index, float healthOffset, int effect)
    {
        if (GLNetworkWrapper.instance.playerNetworkObjects[index].connectedDummy != null)
        {
            print("CHANGE DUMMY HEALTH");
            GLNetworkWrapper.instance.playerNetworkObjects[index].connectedDummy.ChangeHealthOnClient(healthOffset, effect);   
        }
    }

    public void RemoveProp(int tileIndex)
    {
        CmdRemoveProp(tileIndex);
    }
    [Command]
    void CmdRemoveProp(int index)
    {
        RpcRemoveProp(index);
    }
    [ClientRpc]
    void RpcRemoveProp(int index)
    {
        LevelGenerator.instance.levelTilesInGame[index].RemovePropOnClient(true);
    }
    
    public void MakeMap(int roomIndex)
    {
        CmdMakeMap(roomIndex);
    }
    [Command]
    void CmdMakeMap(int roomIndex)
    {
        RpcMakeMap(roomIndex);
    }
    [ClientRpc]
    void RpcMakeMap(int roomIndex)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
        print("TRY TO MAKE A MAP IN THE ROOM #" + roomIndex);
        StartCoroutine(WaitUntillRoomsSpawned(roomIndex));
    }

    IEnumerator WaitUntillRoomsSpawned(int index)
    {
        var lg = LevelGenerator.instance;
        while (lg.roomsInGame.Count <= index)
        {
            yield return new WaitForSeconds(0.1f);
        }
        LevelGenerator.instance.roomsInGame[index].CreateMapOnClient();
    }

    public void SetLevelGen(bool active)
    {
        if (GLNetworkWrapper.instance.localPlayer == null)
        {
            GLNetworkWrapper.instance.coopIsActive = false;
            return;
        }
        
        CmdSetLevelGen(active);
    }
    [Command]
    void CmdSetLevelGen(bool active)
    {
        levelGenExists = active;
        RpcSetLevelGen(active);
    }
    [ClientRpc]
    void RpcSetLevelGen(bool active)
    {
        levelGenExists = active;
    }

    public void StartChase()
    {
        CmdStartChase();
    }

    [Command]
    void CmdStartChase()
    {
        RpcStartChase();
    }

    [ClientRpc]
    void RpcStartChase()
    {
        SpawnController.instance.StartBadRepChase();
    }
    
    public void MakeCeilingTrap(int index, int r)
    {
        CmdMakeCeilingTrap(index, r);
    }
    [Command]
    void CmdMakeCeilingTrap(int index, int r)
    {
        RpcMakeCeilingTrap(index, r);
    }
    [ClientRpc]
    void RpcMakeCeilingTrap(int index, int r)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
            LevelGenerator.instance.levelTilesInGame[index].MakeCeilingTrapOnClient(r);
    }

    public void OpenDoor(int doorIndex, Vector3 openerPosition)
    {
        CmdOpenDoor(doorIndex, openerPosition);
    }

    [Command]
    void CmdOpenDoor(int index, Vector3 openerPosition)
    {
        RpcOpenDoor(index, openerPosition);
    }

    [ClientRpc]
    void RpcOpenDoor(int index, Vector3 openerPosition)
    {
        var il = ItemsList.instance;
        if (il.interactables.Count > index && il.interactables[index].door)
            il.interactables[index].door.OpenDoorOnClient(openerPosition);
    }

    public void ActivateMeatTrap(int index, int range)
    {
        CmdActivateMeatTrap(index, range);
    }

    [Command]
    void CmdActivateMeatTrap(int index, int range)
    {
        RpcActivateMeatTrap(index, range);
    }

    [ClientRpc]
    void RpcActivateMeatTrap(int index, int range)
    {
        var gm = GameManager.instance;
        if (gm.units.Count > index && gm.units[index] != null && gm.units[index].meatTrap)
            gm.units[index].meatTrap.ActivateOnClient(range);
    }
    
    public void DisarmMeatTrap(int index)
    {
        CmdDisarmMeatTrap(index);
    }

    [Command]
    void CmdDisarmMeatTrap(int index)
    {
        RpcDisarmMeatTrap(index);
    }

    [ClientRpc]
    void RpcDisarmMeatTrap(int index)
    {
        var gm = GameManager.instance;
        if (gm.units.Count > index && gm.units[index] != null && gm.units[index].meatTrap)
            gm.units[index].meatTrap.DisarmOnClient();
    }

    [Command]
    public void CmdDropTool(int toolIndex, Vector3 pos)
    {
        var newTool = Instantiate(ItemsList.instance.savedTools[toolIndex].toolController, pos,
            Quaternion.identity);
        
        NetworkServer.Spawn(newTool.gameObject);
    }


    public void ReturnToMenu()
    {
        CmdReturnToMenu();
    }

    [Command]
    void CmdReturnToMenu()
    {
        RpcReturnToMenu();
        NetworkManager.singleton.StopHost();
    }

    [ClientRpc]
    void RpcReturnToMenu()
    {
        NetworkManager.singleton.StopClient();
        GLNetworkWrapper.instance.isGameReady = false;
        
        GameManager.instance.ReturnToMenu(false);
    }

    public void CloseDoor(int doorIndex)
    {
        CmdCloseDoor(doorIndex);
    }

    [Command]
    void CmdCloseDoor(int index)
    {
        RpcCloseDoor(index);
    }

    [ClientRpc]
    void RpcCloseDoor(int index)
    {
        var il = ItemsList.instance;
        if (il.interactables.Count > index && il.interactables[index].door)
            il.interactables[index].door.CloseDoorOnClient();
    }
    
    
    public void CreateLockOnClient(int doorIndex)
    {
        CmdCreateLockOnClient(doorIndex);
    }

    [Command]
    void CmdCreateLockOnClient(int index)
    {
        RpcCreateLockOnClient(index);
    }

    [ClientRpc]
    void RpcCreateLockOnClient(int index)
    {
        var il = ItemsList.instance;
        if (il.interactables.Count > index && il.interactables[index].door)
            il.interactables[index].door.CreateLockOnClient();
    }
    
    public void BreakDoorPartOnClient(int doorIndex, int mobPartIndex)
    {
        CmdBreakDoorPartOnClient(doorIndex, mobPartIndex);
    }

    [Command]
    void CmdBreakDoorPartOnClient(int doorIndex, int mobPartIndex)
    {
        RpcBreakDoorPartOnClient(doorIndex, mobPartIndex);
    }
    [ClientRpc]
    void RpcBreakDoorPartOnClient(int doorIndex, int mobPartIndex)
    {
        ItemsList.instance.interactables[doorIndex].door.BreakDoorPartOnClient(mobPartIndex);
    }
    
    public void DestroyInteractable(int go)
    {
        CmdDestroyInteractable(go);
    }
    [Command]
    void CmdDestroyInteractable(int go)
    {
        RpcDestroyInteractable(go);
    }
    [ClientRpc]
    void RpcDestroyInteractable(int index)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
        {
            var il = ItemsList.instance;
            //print("Destroy interactable with index of " + index);
        
            if (il && il.interactables.Count > index && il.interactables[index] != null)
            {
                Destroy(il.interactables[index].gameObject);
                il.interactables.RemoveAt(index);
            }   
        }
    }

    public void PlayerHeard()
    {
        print("PlayerHeard");
        CmdPlayerHeard();
    }

    [Command]
    void CmdPlayerHeard()
    {
        print("CmdPlayerHeard");
        RpcPlayerHeard();
    }

    [ClientRpc]
    void RpcPlayerHeard()
    {
        print("RpcPlayerHeard");
        if (GLNetworkWrapper.instance.localPlayer == this)
            UiManager.instance.PlayerHeardOnClient();
    }
    
    public void MobHearNoiceOnClient(Vector3 noiseSource, float noiseDistance, int playerId)
    {
        CmdMobHearNoiceOnClient(noiseSource, noiseDistance, playerId);
    }
    [Command]
    void CmdMobHearNoiceOnClient(Vector3 noiseSource, float noiseDistance, int playerId)
    {
        SpawnController.instance.MobHearNoiceOnClient(noiseSource, noiseDistance, playerId);
        RpcMobHearNoiseOnClient(noiseSource, noiseDistance, playerId);
    }

    [ClientRpc]
    void RpcMobHearNoiseOnClient(Vector3 noiseSource, float noiseDistance, int playerId)
    {
        // dont run on server
        if (GLNetworkWrapper.instance.playerNetworkObjects[0] != this)
            SpawnController.instance.MobHearNoiceOnClient(noiseSource, noiseDistance, playerId);
    }
    
    public void SpawnEnvironmentTraps()
    {
        CmdSpawnEnvironmentTraps();
    }

    [Command]
    void CmdSpawnEnvironmentTraps()
    {
        RpcSpawnEnvironmentTraps();
    }

    [ClientRpc]
    void RpcSpawnEnvironmentTraps()
    {
        StartCoroutine(LevelGenerator.instance.SpawnEnvironmentTrapsOnClient());
    }

    /*
    public void SpawnRoom(int index, Vector3 pos)
    {
        // each player spawns room on his own machine
        
        //if (isLocalPlayer)
        CmdSpawnRoom(index, pos);   
    }

    [Command]
    void CmdSpawnRoom(int index, Vector3 pos)
    {
        //print("ASK SERVER TO SPAWN ROOM ON CLIENT");
        RpcSpawnRoom(index, pos);
    }
    
    [ClientRpc]
    void RpcSpawnRoom(int index, Vector3 pos)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
        {
            //print("TRY TO SPAWN ROOM ON CLIENT");
            StartCoroutine(LevelGenerator.instance.SpawnRoomOnClient(index, pos));
        }
    }
    */
    
    public void SpawnTile(Vector3 tilePosition, int tileIndex, RoomController masterRoom, StatusEffects.StatusEffect effect, 
        bool spawner, int coridorIndex, bool cultTile)
    {
        // each player spawns tile on his own machine
        
        //if (isLocalPlayer)
        int mr = LevelGenerator.instance.roomsInGame.IndexOf(masterRoom);
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
        CmdSpawnTile(tilePosition, tileIndex, mr, fxIndex, 
            spawner, coridorIndex, cultTile);   
    }

    public void SpawnInsideTileOnClient(int tileIndex, int prefabIndex, int side)
    {
        CmdSpawnInsideTileOnClient(tileIndex, prefabIndex, side);
    }

    [Command]
    void CmdSpawnInsideTileOnClient(int tileIndex, int prefabIndex, int side)
    {
        RpcSpawnInsideTileOnClient(tileIndex, prefabIndex, side);
    }

    [ClientRpc]
    void RpcSpawnInsideTileOnClient(int tileIndex, int prefabIndex, int side)
    {
        //print("SPAWN INSIDE TILE ON CLIENT. tileIndex, prefabIndex, side: " + tileIndex + ", " + prefabIndex + ", " + side);
        StartCoroutine(SpawnInsideTileOnClientCoroutine(tileIndex, prefabIndex, side));
    }

    IEnumerator SpawnInsideTileOnClientCoroutine(int tileIndex, int prefabIndex, int side)
    {
        var lg = LevelGenerator.instance;
        
        while (lg.levelTilesInGame.Count <= tileIndex)
        {
            print("Client waits for tile to spawn");
            yield return null;
        }
        //print("RPC SPAWN INSIDE TILE ON CLIENT. TILE INDEX IS " + tileIndex);
        LevelGenerator.instance.levelTilesInGame[tileIndex].SpawnInsideTileOnClient(prefabIndex, side, false);   
    }
    
    /*
    public void CreateWallsBetweenTiles(int index, float trapForwardR, float blockerForwardR, float doorForwardR,
        float trapRightR, float blockerRightR, float doorRightR,
        float trapBackR, float blockerBackR, float doorBackR,
        float trapLeftR, float blockerLeftR, float doorLeftR)
    {
        CmdCreateWallsBetweenTiles(index, trapForwardR, blockerForwardR, doorForwardR, trapRightR, blockerRightR, doorRightR, trapBackR, blockerBackR, doorBackR, trapLeftR, blockerLeftR, doorLeftR);
    }

    [Command]
    void CmdCreateWallsBetweenTiles(int index, float trapForwardR, float blockerForwardR, float doorForwardR,
        float trapRightR, float blockerRightR, float doorRightR,
        float trapBackR, float blockerBackR, float doorBackR,
        float trapLeftR, float blockerLeftR, float doorLeftR)
    {
        RpcCreateWallsBetweenTiles(index, trapForwardR, blockerForwardR, doorForwardR, trapRightR, blockerRightR, doorRightR, trapBackR, blockerBackR, doorBackR, trapLeftR, blockerLeftR, doorLeftR);
    }

    [ClientRpc]
    void RpcCreateWallsBetweenTiles(int index, float trapForwardR, float blockerForwardR, float doorForwardR,
        float trapRightR, float blockerRightR, float doorRightR,
        float trapBackR, float blockerBackR, float doorBackR,
        float trapLeftR, float blockerLeftR, float doorLeftR)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
        LevelGenerator.instance.CreateWallsBetweenTilesOnClient(index, trapForwardR, blockerForwardR, doorForwardR, trapRightR, blockerRightR, doorRightR, trapBackR, blockerBackR, doorBackR, trapLeftR, blockerLeftR, doorLeftR);
    }
    */
    
    public void TileRandomCeiling(int index, float r)
    {
        CmdTileRandomCeiling(index, r);
    }
    
    [Command]
    void CmdTileRandomCeiling(int tileIndex, float r)
    {
        RpcTileRandomCeiling(tileIndex, r);
    }

    [ClientRpc]
    void RpcTileRandomCeiling(int tileIndex, float r)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
            StartCoroutine(LevelGenerator.instance.TileRandomCeilingOnClient(tileIndex, r));
    }

    [Command]
    void CmdSpawnTile(Vector3 tilePosition, int tileIndex, int masterRoomIndex, int effectIndex, 
        bool spawner, int coridorIndex, bool cultTile)
    {
        RpcSpawnTile(tilePosition, tileIndex, masterRoomIndex, effectIndex, 
            spawner, coridorIndex,  cultTile);
    }
    
    [ClientRpc]
    void RpcSpawnTile(Vector3 tilePosition, int tileIndex, int masterRoomIndex, int effectIndex, 
        bool spawner, int coridorIndex, bool cultTile)
    {
        //if (GLNetworkWrapper.instance.localPlayer == this)
            LevelGenerator.instance.SpawnTileOnClient(tilePosition, tileIndex, masterRoomIndex, effectIndex, 
            spawner, coridorIndex,  cultTile);
    }
}
