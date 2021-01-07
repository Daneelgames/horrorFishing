using System.Collections;
using System.Collections.Generic;
using Mirror;
using PlayerControls;
using Steamworks;
using UnityEngine;

public class GLNetworkWrapper : MonoBehaviour
{
    public static GLNetworkWrapper instance;
    
    public bool coopIsActive = false;
    public PlayerNetworkObject localPlayer;
    public List<PlayerNetworkObject> playerNetworkObjects = new List<PlayerNetworkObject>();
    public bool isGameReady = false;

    public static CSteamID userId;


    void Awake() => instance = this;

    void Update()
    {
        if (NetworkManager.singleton.isNetworkActive)
        {
            GameReadyCheck();

            if (localPlayer == null)
            {
                FindLocalPlayer();
            }
            else if (playerNetworkObjects.Count == 2)
            {
                if (coopIsActive == false)
                {
                    coopIsActive = true;
                    if (localPlayer.isServer)
                    {
                        SteamworksLobby.instance.ToggleConnectionFeedback(false);
                        GameManager.instance.StartCoopGame();   
                    }
                }
            }
            
            /*
            if (playerNetworkObjects.Count > 1 && playerNetworkObjects[1] == null)
                playerNetworkObjects.RemoveAt(1);
                */
            for (int i = 0; i < playerNetworkObjects.Count; i++)
            {
                if (playerNetworkObjects[i] == null)
                {
                    NetworkManager.singleton.StopHost();
                    NetworkManager.singleton.StopClient();
                    GameManager.instance.ReturnToMenu(false);
                    break;
                }
            }
        }
        else
        {
            if (NetworkManager.singleton && (NetworkManager.singleton.networkAddress == "" || NetworkManager.singleton.networkAddress == "localhost"))
            {
                userId = SteamUser.GetSteamID();
                if (userId != CSteamID.Nil)
                {
                    NetworkManager.singleton.networkAddress = "Your ID: " + userId.m_SteamID;   
                }
            }

            if (coopIsActive)
            {
                coopIsActive = false;
                NetworkManager.singleton.StopHost();
                NetworkManager.singleton.StopClient();
                GameManager.instance.ReturnToMenu(false);
            }

            //Cleanup state once network goes offline
            localPlayer = null;
            playerNetworkObjects.Clear();
        }
    }

    public void NextLevelOnServer()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].NextLevelOnServer();
        }
    }

    public void StopGettingTheInventory()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].StopInventoryCoroutine();
        }
    }

    public void ToggleSpectatorCameraFromLocalPlayer(bool active)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            if (playerNetworkObjects[i] != localPlayer)
            {
                playerNetworkObjects[i].connectedDummy.spectatorCam.gameObject.SetActive(active);
                break;
            }
        }
    }

    public void PlayMobSoundOnClient(int mobAudioManagerIndex, int actionIndex)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].PlayMobSoundOnClient(mobAudioManagerIndex, actionIndex);
        }
    }
    
    void GameReadyCheck()
    {
        if (!isGameReady)
        {
            //Look for connections that are not in the player list
            foreach (KeyValuePair<uint, NetworkIdentity> kvp in NetworkIdentity.spawned)
            {
                PlayerNetworkObject comp = kvp.Value.GetComponent<PlayerNetworkObject>();

                //Add if new
                if (comp != null && !playerNetworkObjects.Contains(comp))
                {
                    playerNetworkObjects.Add(comp);
                }
            }

            //If minimum connections has been check if they are all ready
            if (playerNetworkObjects.Count == 2)
            {
                bool AllReady = true;
                foreach (PlayerNetworkObject pno in playerNetworkObjects)
                {
                    if (!pno.isReady)
                    {
                        AllReady = false;
                    }
                }
                if (AllReady)
                {
                    isGameReady = true;
                    // start game
                }
            }
        }
    }
    
    void FindLocalPlayer()
    {
        //Check to see if the player is loaded in yet
        if (ClientScene.localPlayer == null)
            return;

        localPlayer = ClientScene.localPlayer.GetComponent<PlayerNetworkObject>();
    }

    /*
    public void SaveRandomSeed(int seed)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            print("[[[SEED: " + seed + "]]]");
            playerNetworkObjects[i].SaveRandomSeed(seed);
        }
    }
    */

    public void StartCoopGame(bool restart)
    {
        if (localPlayer.isServer)
        {
            int floor = GutProgressionManager.instance.playerFloor;
            int bossFloor = GutProgressionManager.instance.bossFloor;

            for (int i = 0; i < playerNetworkObjects.Count; i++)
            {
                playerNetworkObjects[i].StartCoopGame(floor, bossFloor, restart);
            }
        }
    }

    public HealthController GetClosestPlayer(Vector3 origin)
    {
        HealthController newHc = GameManager.instance.player;

        float distance = 10000;
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            if (playerNetworkObjects[i] == null || playerNetworkObjects[i].connectedDummy == null || playerNetworkObjects[i].connectedDummy.currentHp <= 0|| playerNetworkObjects[i].connectedDummy.hc.health <= 0)
                continue;

            float newDist = Vector3.Distance(origin, playerNetworkObjects[i].connectedDummy.transform.position);
            if (newDist < distance)
            {
                distance = newDist;
                newHc = playerNetworkObjects[i].connectedDummy.hc;
            }
        }
        return newHc;
    }

    public void FoundKey(int playerId)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].FoundKey(playerId);
        }
    }

    public void SaveRandomObject(int index, int savedObjectIndex, bool activate)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SaveRandomObject(index, savedObjectIndex, activate);
        }
    }

    public void TimeToLeaveFloorFeedbackOnClient()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].TimeToLeaveFloorFeedbackOnClient();
        }
    }

    public void AttractMonsters(Vector3 target, bool _chase)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].AttractMonsters(target, _chase);
        }
    }

    public void SetCultistOnClient(int unitIndex, int effectIndex, bool leader)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SetCultistOnClient(unitIndex, effectIndex, leader);
        }
    }
    
    public void DropWeapon(int weaponEnumIndex, float ammoClip, float durability, List<string> generatedName,List<string> generatedDescriptions, int r1, int r2, int r3, int r4, int statusEffect, bool dead, bool npc, bool npcPaid,
        List<int> savedBarrelslvl_1, List<int> savedBarrelslvl_2, Vector3 eulerAngles, bool crazyGun, float clipSize, int barrelsCount, Vector3 pos)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].DropWeapon(weaponEnumIndex, ammoClip, durability, generatedName, generatedDescriptions, r1, r2, r3, r4,
                statusEffect, dead, npc, npcPaid, savedBarrelslvl_1, savedBarrelslvl_2, eulerAngles,
                crazyGun, clipSize, barrelsCount, pos);
        }
    }

    public void CreateExplosion(Vector3 position)
    {
        localPlayer.CreateExplosion(position);
    }

    public void DropTool(int toolIndex, Vector3 dropPos)
    {
        /*
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].CmdDropTool(toolIndex, dropPos);
        }
        */
        localPlayer.CmdDropTool(toolIndex, dropPos);
    }
    
    public void StartLevel()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].StartLevel();
        }
    }

    public void SpawnEnvironmentTraps()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SpawnEnvironmentTraps();
        }
    }
    
    public void MobHearNoiceOnClient(Vector3 noiseSource, float noiseDistance)
    {
        int playerId = playerNetworkObjects.IndexOf(localPlayer);
        
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].MobHearNoiceOnClient(noiseSource, noiseDistance, playerId);
        }
    }
    
    /*
    public void CreateWallsBetweenTiles(int index, float trapForwardR, float blockerForwardR, float doorForwardR,
        float trapRightR, float blockerRightR, float doorRightR,
        float trapBackR, float blockerBackR, float doorBackR,
        float trapLeftR, float blockerLeftR, float doorLeftR)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].CreateWallsBetweenTiles(index, trapForwardR, blockerForwardR, doorForwardR, trapRightR, blockerRightR, doorRightR, trapBackR, blockerBackR, doorBackR, trapLeftR, blockerLeftR, doorLeftR);
        }
    }*/

    public void MakeFloorTrap(int index)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].MakeFloorTrap(index);
        }
    }

    public void DeathOnClient(GameObject o, string damager, bool affectReputation)
    {
        // dont send damager name
        
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].DeathOnClient(o, affectReputation);
        }
    }
    
    public void MakeCeilingTrap(int index, int r)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].MakeCeilingTrap(index, r);
        }
    }
    
    public void CreateMap(int roomIndex)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].MakeMap(roomIndex);
        }
    }

    public void RemoveProp(int tileIndex)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].RemoveProp(tileIndex);
        }
    }

    public void SpawnCorridorProp(int tileIndex, int corridorPropReference)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SpawnCorridorProp(tileIndex, corridorPropReference);
        }
    }
    
    public void BothPlayersDied(bool restart)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].BothPlayersDied(restart);
        }
    }
    
    public void LevelCompleted()
    {
        localPlayer.LevelCompleted();
        /*
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].LevelCompleted();
        }*/
    }
    public void LoadCoopSceneOnServer(bool restart)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].LoadCoopSceneOnServer(restart);
        }
    }
    public void InitCoopLevelOnClient()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].InitCoopLevelOnClient();
        }
    }

    public void SendToMeatHole(GameObject go, GameObject meatHole)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SendToMeatHole(go, meatHole);
        }
    }

    public void TogglePickUpBrain(GameObject brainGo, bool pickedUp)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].TogglePickUpBrain(brainGo, pickedUp);
        }
    }
    
    public void SpawnRoomProp(int tileIndex, int RoomPropReference)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SpawnRoomProp(tileIndex, RoomPropReference);
        }
    }
    public void RespawnDeadPlayer()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].RespawnDeadPlayer();
        }
    }
    public void ElevatorShaftStopLoading()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].ElevatorShaftStopLoading();
        }
    }

    public GameObject GetAlivePlayerNetworkObject()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            if (playerNetworkObjects[i].connectedDummy && playerNetworkObjects[i].connectedDummy.hc.health > 0)
                return playerNetworkObjects[i].gameObject;
        }

        return null;
    }

    public void TurnPropOnTile(int tileIndex, float angle)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].TurnPropOnTile(tileIndex, angle);
        }
    }

    public void SpawnMob()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SpawnMob();
        }
    }
    
    public void SpawnDropFromMob(int tileIndex, float angle)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].TurnPropOnTile(tileIndex, angle);
        }
    }

    public void UnitHealthChanged(GameObject gameObject, float healthOffset, int effect, bool badRep)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].UnitHealthChanged(gameObject, healthOffset, effect, badRep);
        }
    }
    
    /*
    public void RemoveUnitOnClient(int unitIndex)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].RemoveUnitOnClient(unitIndex);
        }
    }
    */

    public void ButtonPressed()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].ButtonPressed();
        }
    }

    public void UseKeyOnLock(int index)
    {
        int playerIndex = playerNetworkObjects.IndexOf(localPlayer);

        print("Use Key On Lock. Lock interactable index is " + index + "; player index is " + playerIndex);
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].UseKeyOnLock(index, playerIndex);
        }

    }
    
    public void SpawnTile(Vector3 tilePosition, int tileIndex, RoomController masterRoom, StatusEffects.StatusEffect effect, 
        bool spawner, int coridorIndex, bool cultTile)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SpawnTile(tilePosition, tileIndex, masterRoom, effect, 
            spawner, coridorIndex,  cultTile);
        }
    }

    public void DummyHealthChanged(int index, float healthOffset, int effect)
    {
        print("PLAYER #" + index + " IS GONNA CHANGE HIS HEALTH ON " + healthOffset);
        //playerNetworkObjects[index].DummyHealthChanged(healthOffset);
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].DummyHealthChanged(index, healthOffset, effect);
        }
    }
    public void RandomCeiling(int tileIndex, float r)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].TileRandomCeiling(tileIndex, r);
        }
    }

    public void OpenDoor(int doorIndex, Vector3 openerPosition)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].OpenDoor(doorIndex, openerPosition);   
        }
    }
    

    public void ActivateMeatTrap(int index, int range)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].ActivateMeatTrap(index, range);   
        }   
    }
    public void DisarmMeatTrap(int index)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].DisarmMeatTrap(index);   
        }   
    }
    
    
    public void CloseDoor(int doorIndex)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].CloseDoor(doorIndex);   
        }
    }
    
    public void CreateLockOnClient(int doorIndex)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].CreateLockOnClient(doorIndex);   
        }
    }
    public void BreakDoorPartOnClient(int doorInteractableIndex, int mobPartIndex)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].BreakDoorPartOnClient(doorInteractableIndex, mobPartIndex);   
        }
    }
    
    public void DestroyInteractable(int index)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].DestroyInteractable(index);   
        }
    }
    
    public void PlayerCrouch(bool crouch)
    {
        if (LevelGenerator.instance.levelgenOnHost)
        {
            for (int i = 0; i < playerNetworkObjects.Count; i++)
            {
                playerNetworkObjects[i].PlayerCrouch(0, crouch);   
            }   
        }
        else
        {
            for (int i = 0; i < playerNetworkObjects.Count; i++)
            {
                playerNetworkObjects[i].PlayerCrouch(1, crouch);   
            }
        }
    }

    public void ReturnToMenu()
    {
        localPlayer.ReturnToMenu();
        
        /*
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].ReturnToMenu();
        }*/
    }

    public void SpawnInsideTileOnClient(int tileIndex, int prefabIndex, int side)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SpawnInsideTileOnClient(tileIndex,  prefabIndex,  side);
        }
    }

    public void StartChase()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].StartChase();
        }
    }

    public void SaveTool(int description, int effect)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SaveTool(description, effect);
        }
    }

    public void PlayerFinishedLevel()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].PlayerFinishedLevel();
        }
    }
    
    public void SetFlashbackSceneIndex(int flashbackSceneIndex)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SetFlashbackSceneIndex(flashbackSceneIndex);
        }
    }

    public void ActivateSavedRandomObjectsActivators()
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].ActivateSavedRandomObjectsActivators();
        }
    }
    
    /*
    public void SpawnRoom(int index, Vector3 pos)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SpawnRoom(index, pos);
        }
    }
    */
    
    public void SetTileEffect(int tileIndex, int effectIndex, float lifeTime)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SetTileEffect(tileIndex, effectIndex, lifeTime);
        }
    }
    public void ClearTilesObjects(int tileIndex)
    {
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].ClearTilesObjects(tileIndex);
        }
    }

    public bool ClientsGotLevelgen()
    {
        bool levelgens = true;
        
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            if (playerNetworkObjects[i].levelGenExists == false)
            {
                levelgens = false;
                break;
            }
        }

        return levelgens;
    }

    public void SwapProps()
    {
        localPlayer.SwapProps();
        
        /*
        for (int i = 0; i < playerNetworkObjects.Count; i++)
        {
            playerNetworkObjects[i].SwapProps();
        }*/
    }
}
