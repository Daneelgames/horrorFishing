using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class HubItemsSpawner : MonoBehaviour
{
    public List<CitySpawner> itemSpawners = new List<CitySpawner>();
    public List<CitySpawner> npcSpawners = new List<CitySpawner>();
    public List<CitySpawner> fieldEventsSpawners = new List<CitySpawner>();
    public List<CitySpawner> noteSpawners = new List<CitySpawner>();
    public List<LevelBlockerController> levelBlockersPrefabs = new List<LevelBlockerController>();
    
    private ItemsList il;
    private SpawnController sc;

    public static HubItemsSpawner instance;

    private HealthController worldHolderSpawned;
    private HealthController ladyOnRoofSpawned_0;
    private HealthController gunnWorkingSpawned_1;
    private HealthController gunnOnBeachSpawned;
    private HealthController motherOnBeachSpawned;
    private HealthController gunnWalkable;
    private HealthController headDragonSpawned;
    private GameObject shoesOnBeachSpawned;
    
    public List<ChangeDialogueOnQuest> dialogueOnQuestsChangers = new List<ChangeDialogueOnQuest>();

    private WeaponControls wc;
    private QuestManager qm;
    void Awake()
    {
        instance = this;
    }

    NavMeshHit hit;
    private Vector3 positionNearSpawner;
    public Vector3 GetPositionNearSpawner(Vector3 spawnerPosition)
    {
        positionNearSpawner = spawnerPosition + Random.insideUnitSphere * 50;
                            
        if (NavMesh.SamplePosition(positionNearSpawner, out hit, 50.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
            
        return spawnerPosition;
    }

    public void NewNotePickedUp(int noteIndex)
    {
        if (GameManager.instance.notesPickedUp < noteIndex)
        {
            GameManager.instance.notesPickedUp = noteIndex;
            GameManager.instance.SaveGame();
        }
    }
    
    bool HaveRevolver()
    {
        if (wc.activeWeapon && wc.activeWeapon.weapon == WeaponPickUp.Weapon.Revolver)
            return true;
        if (wc.secondWeapon && wc.secondWeapon.weapon == WeaponPickUp.Weapon.Revolver)
            return true;

        return false;
    }
    bool HaveLeg()
    {
        if (wc.activeWeapon && wc.activeWeapon.weapon == WeaponPickUp.Weapon.LadyShoe)
            return true;
        if (wc.secondWeapon && wc.secondWeapon.weapon == WeaponPickUp.Weapon.LadyShoe)
            return true;

        return false;
    }
    
    public void UpdateHub()
    {
        // check what to spawn based on what quests are active
        qm = QuestManager.instance;
        sc = SpawnController.instance;

        if (worldHolderSpawned == null)
        {
            worldHolderSpawned = Instantiate(npcSpawners[0].npcsToSpawn[0], npcSpawners[0].transform.position,
                npcSpawners[0].transform.rotation);
        }

        // spawn lady
        if (ladyOnRoofSpawned_0 == null)
        {
            // nps
            ladyOnRoofSpawned_0 = Instantiate(npcSpawners[1].npcsToSpawn[0], npcSpawners[1].transform.position,
                npcSpawners[1].transform.rotation); 
            //human ladder
            Instantiate(npcSpawners[1].gameObjectToSpawn, npcSpawners[1].transform.position, npcSpawners[1].transform.rotation);

            wc = WeaponControls.instance;
            
            // revolver start cutscene
            if (GameManager.instance.revolverFound == false)
                Instantiate(fieldEventsSpawners[0].gameObjectToSpawn, fieldEventsSpawners[0].transform.position,
                    fieldEventsSpawners[0].transform.rotation);
            
            // spawn notes
            for (int i = 0; i < noteSpawners.Count; i++)
            {
                Instantiate(noteSpawners[i].itemToSpawn, noteSpawners[i].transform.position,
                    noteSpawners[i].transform.rotation);
            }
        }
        
        #region Quest 1. Shoes on beach

            if (qm.activeQuestsIndexes.Contains(1)) // shoe quest is active
            {
                // spawn shoes on beach if not spawned
                if (shoesOnBeachSpawned == null)
                {
                    var go = Instantiate(fieldEventsSpawners[1].gameObjectToSpawn, fieldEventsSpawners[1].transform.position,
                        fieldEventsSpawners[1].transform.rotation);

                    shoesOnBeachSpawned = go;
                     
                }
                
                // spawn lady
                if (ladyOnRoofSpawned_0 == null)
                {
                    var go = Instantiate(npcSpawners[1].npcsToSpawn[0], npcSpawners[1].transform.position,
                        npcSpawners[1].transform.rotation);

                    ladyOnRoofSpawned_0 = go;
                }
            }
        #endregion

        #region quest 2 Return the shoe

        if (qm.completedQuestsIndexes.Contains(2)) // return shoe quest is completed
        {
            //remove shoes
            if (shoesOnBeachSpawned != null)
            {
                Destroy(shoesOnBeachSpawned);
                shoesOnBeachSpawned = null;
            }
        }
        #endregion
        
        #region quest 3 Find the Gunn

        if (qm.activeQuestsIndexes.Contains(3))
        {
            //remove shoes
            if (gunnWorkingSpawned_1 == null)
            {
                
                gunnWorkingSpawned_1 = Instantiate(npcSpawners[2].npcsToSpawn[0], npcSpawners[2].transform.position,
                    npcSpawners[2].transform.rotation); 
            }
        }
        #endregion

        #region quest 4 find wood

        if (qm.activeQuestsIndexes.Contains(4) || (qm.completedQuestsIndexes.Contains(4) 
                                                   && !qm.activeQuestsIndexes.Contains(5) 
                                                   && !qm.completedQuestsIndexes.Contains(5)))
        {
            //remove shoes
            if (gunnOnBeachSpawned == null)
            {
                gunnOnBeachSpawned = Instantiate(npcSpawners[3].npcsToSpawn[0], npcSpawners[3].transform.position,
                    npcSpawners[3].transform.rotation); 
            }
        }
        #endregion

        #region quest 5 speak to mother

        if (qm.activeQuestsIndexes.Contains(5) || qm.activeQuestsIndexes.Contains(6))
        {
            //remove shoes
            if (motherOnBeachSpawned == null)
            {
                motherOnBeachSpawned = Instantiate(npcSpawners[4].npcsToSpawn[0], npcSpawners[4].transform.position,
                    npcSpawners[4].transform.rotation); 
            }
        }
        #endregion
        
        #region quest 6 talk back to gunn

        if (qm.activeQuestsIndexes.Contains(6) || qm.activeQuestsIndexes.Contains(7))
        {
            //remove shoes
            if (gunnWalkable == null)
            {
                gunnWalkable = Instantiate(npcSpawners[5].npcsToSpawn[0], npcSpawners[5].transform.position,
                    npcSpawners[5].transform.rotation); 
            }
        }
        #endregion
        
        #region quest 7 kill dragon

        if (qm.activeQuestsIndexes.Contains(7))
        {
            if (headDragonSpawned == null)
            {
                headDragonSpawned = Instantiate(npcSpawners[6].npcsToSpawn[0], npcSpawners[6].transform.position,
                    npcSpawners[6].transform.rotation);
            }
        }
        #endregion
        
        
        
        UpdateLevelBlockers();
        
        for (int i = 0; i < dialogueOnQuestsChangers.Count; i++)
        {
            dialogueOnQuestsChangers[i].UpdateDialogue();
        }
    }

    private LevelBlockerController blockersBeforeGoingToGunn;
    private LevelBlockerController blockersBeforeGettingShoeQuest;
    private LevelBlockerController blockersGoingToGunn;
    private LevelBlockerController blockersFindWood;
    private LevelBlockerController blockersSpeaktoMother;
    private LevelBlockerController blockersFinal;
    void UpdateLevelBlockers()
    {
        qm = QuestManager.instance;
        // before getting shoe quest
        if (!qm.activeQuestsIndexes.Contains(1) && !qm.completedQuestsIndexes.Contains(1))
        {
            if (blockersBeforeGettingShoeQuest == null)
                blockersBeforeGettingShoeQuest = Instantiate(levelBlockersPrefabs[0], Vector3.zero, Quaternion.identity);
        }
        else if (blockersBeforeGettingShoeQuest)
            blockersBeforeGettingShoeQuest.DestroyBlockers();
        
        // before going to gunn
        if (!qm.activeQuestsIndexes.Contains(3) && !qm.completedQuestsIndexes.Contains(3))
        {
            if (blockersBeforeGoingToGunn == null)
                blockersBeforeGoingToGunn = Instantiate(levelBlockersPrefabs[1], Vector3.zero, Quaternion.identity);
        }
        else if (blockersBeforeGoingToGunn)
            blockersBeforeGoingToGunn.DestroyBlockers();

        // Gunn quest blocker
        if (qm.activeQuestsIndexes.Contains(3))
        {
            if (blockersGoingToGunn == null)
                blockersGoingToGunn = Instantiate(levelBlockersPrefabs[2], Vector3.zero, Quaternion.identity);
        }
        else if (blockersGoingToGunn)
            blockersGoingToGunn.DestroyBlockers();
        
        // find wood questblocker
        if (qm.activeQuestsIndexes.Contains(4) || (qm.completedQuestsIndexes.Contains(4) 
                                                     && !qm.activeQuestsIndexes.Contains(5) 
                                                     && !qm.completedQuestsIndexes.Contains(5)))
        {
            if (blockersFindWood == null)
                blockersFindWood = Instantiate(levelBlockersPrefabs[3], Vector3.zero, Quaternion.identity);
        }
        else if (blockersFindWood)
            blockersFindWood.DestroyBlockers();
        
        // speak to mother
        if (qm.activeQuestsIndexes.Contains(5) || qm.activeQuestsIndexes.Contains(6))
        {
            if (blockersSpeaktoMother == null)
                blockersSpeaktoMother = Instantiate(levelBlockersPrefabs[4], Vector3.zero, Quaternion.identity);
        }
        else if (blockersSpeaktoMother)
            blockersSpeaktoMother.DestroyBlockers();
        
        if (qm.activeQuestsIndexes.Contains(7) || qm.completedQuestsIndexes.Contains(7))
        {
            if (blockersFinal == null)
                blockersFinal = Instantiate(levelBlockersPrefabs[5], Vector3.zero, Quaternion.identity);
        }
        else if (blockersFinal)
            blockersFinal.DestroyBlockers();
    }

    public IEnumerator RespawnPlayerAfterDeath()
    {
        DynamicObstaclesManager.instance.PlayerDied();
        PlayerMovement.instance.cameraAnimator.SetBool("Death", true);
        PlayerMovement.instance.hc.invincible = true;
        //PlayerMovement.instance.goldenLightAnimator.SetBool("GoldenLight", true);
        PlayerMovement.instance.controller.enabled = false; 
        
        wc = WeaponControls.instance;
        var sc = SpawnController.instance;
        var spawnersTemp = new List<Transform>(sc.spawners);
        var currentSpawner = spawnersTemp[Random.Range(0, spawnersTemp.Count)];
        bool spawnLeg = HaveLeg();
        bool spawnRevolver = HaveRevolver();
        
        wc.ResetInventory();
        StartCoroutine(AnimateDeathFov());
        yield return new WaitForSeconds(1f);
        // death anim is over

        StartCoroutine(GetUpPhraseCoroutine());
        yield return new WaitForSeconds(2.1f);
        PlayerSkillsController.instance.InstantTeleport(currentSpawner.position);
        PlayerMovement.instance.hc.RespawnPlayer(true);
        spawnersTemp.Remove(currentSpawner);
        if (spawnLeg)
        {
            currentSpawner = GetClosestSpawner(currentSpawner.position, spawnersTemp);
            DynamicObstaclesManager.instance.spawnedLeg = sc.InstantiateItem(itemSpawners[0].itemToSpawn, currentSpawner.position + Vector3.up * 0.5f, currentSpawner.rotation, false);
            spawnersTemp.Remove(currentSpawner);
        }
        if (spawnRevolver)
        {
            currentSpawner = GetClosestSpawner(currentSpawner.position, spawnersTemp);
            DynamicObstaclesManager.instance.spawnedRevolver = sc.InstantiateItem(itemSpawners[1].itemToSpawn, currentSpawner.position + Vector3.up * 0.5f, currentSpawner.rotation, false);
        }
    }
    public IEnumerator RespawnPlayerOnContinue(Vector3 newPlayerPos)
    {
        PlayerAudioController.instance.Death();
        DynamicObstaclesManager.instance.PlayerDied();
        PlayerMovement.instance.cameraAnimator.SetBool("Death", true);
        PlayerMovement.instance.hc.invincible = true;
        //PlayerMovement.instance.goldenLightAnimator.SetBool("GoldenLight", true);
        PlayerMovement.instance.controller.enabled = false; 
        
        wc = WeaponControls.instance;
        var sc = SpawnController.instance;
        var spawnersTemp = new List<Transform>(sc.spawners);
        print("spawners temp count = " + spawnersTemp.Count);
        var currentSpawner = spawnersTemp[Random.Range(0, spawnersTemp.Count)];
        bool spawnLeg = GameManager.instance.ladyshoeFound;
        bool spawnRevolver = GameManager.instance.revolverFound;
        
        wc.ResetInventory();
        StartCoroutine(AnimateDeathFov());
        yield return new WaitForSeconds(1f);
        // death anim is over

        StartCoroutine(GetUpPhraseCoroutine());
        yield return new WaitForSeconds(2.1f);
        PlayerSkillsController.instance.InstantTeleport(newPlayerPos);
        PlayerMovement.instance.hc.RespawnPlayer(false);
        if (spawnLeg)
        {
            currentSpawner = GetClosestSpawner(newPlayerPos, spawnersTemp);
            DynamicObstaclesManager.instance.spawnedLeg = sc.InstantiateItem(itemSpawners[0].itemToSpawn, currentSpawner.position + Vector3.up * 0.5f, currentSpawner.rotation, false);
            spawnersTemp.Remove(currentSpawner);
        }
        if (spawnRevolver)
        {
            currentSpawner = GetClosestSpawner(currentSpawner.position, spawnersTemp);
            DynamicObstaclesManager.instance.spawnedRevolver = sc.InstantiateItem(itemSpawners[1].itemToSpawn, currentSpawner.position + Vector3.up * 0.5f, currentSpawner.rotation, false);
        }
    }

    public void RespawnItems()
    {
        var spawnersTemp = new List<Transform>(sc.spawners);
        var currentSpawner = GetClosestSpawner(PlayerMovement.instance.transform.position, spawnersTemp);
        float distance = 1000f;
        float newDistance = 0;
        
        
        if (DynamicObstaclesManager.instance.spawnedLeg)
        {
            Destroy(DynamicObstaclesManager.instance.spawnedLeg.gameObject);
            DynamicObstaclesManager.instance.spawnedLeg = sc.InstantiateItem(itemSpawners[0].itemToSpawn, currentSpawner.position + Vector3.up * 0.5f, currentSpawner.rotation, false);
            currentSpawner = GetClosestSpawner(DynamicObstaclesManager.instance.spawnedLeg.transform.position, spawnersTemp);
        }
        
        if (DynamicObstaclesManager.instance.spawnedRevolver)
        {
            Destroy(DynamicObstaclesManager.instance.spawnedRevolver.gameObject);
            DynamicObstaclesManager.instance.spawnedRevolver = sc.InstantiateItem(itemSpawners[1].itemToSpawn, currentSpawner.position + Vector3.up * 0.5f, currentSpawner.rotation, false);
        }
    }

    IEnumerator AnimateDeathFov()
    {
        float t = 0;
        float tt = 3;
        var cam = MouseLook.instance.mainCamera;
        float startFov = cam.fieldOfView;

        while (t < tt)
        {
            t += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(startFov, 180, t / tt);
            yield return null;
        }

        t = 0;
        tt = 1;
        startFov = 180;
        while (t < tt)
        {
            t += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(startFov, MouseLook.instance.cameraFovIdle, t / tt);
            yield return null;
        }
        MouseLook.instance.SyncCameraFov();
    }
    IEnumerator GetUpPhraseCoroutine()
    {
        var ui = UiManager.instance;
        string newPhrase = "PICK YOURSELF UP AND GET BACK IN THE RACE";
        ui.dialogueSpeakerName.text = "Lady in Red";
        
        ui.dialogueAnim.SetTrigger("Active");
        
        foreach (char c in newPhrase)
        {
            ui.dialoguePhrase.text += c;
            yield return null;
        }
        
        yield return new WaitForSeconds(3f);
    
        ui.dialogueAnim.SetTrigger("Inactive");
        ui.dialoguePhrase.text = "";
        ui.dialogueChoice.text = "";
    }

    Transform GetClosestSpawner(Vector3 originPoint, List<Transform> spawnersTemp)
    {
        float distance = 1000;
        float newDist = 0;
        Transform closestSpawner = null;
        for (int i = 0; i < spawnersTemp.Count; i++)
        {
            newDist = Vector3.Distance(originPoint, spawnersTemp[i].position);

            if (newDist > 10 && newDist < distance)
            {
                distance = newDist;
                closestSpawner = spawnersTemp[i];
            }
        }

        return closestSpawner;
    }
}
