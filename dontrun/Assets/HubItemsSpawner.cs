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

    private HealthController ladyOnRoofSpawned_0;
    private HealthController gunnWorkingSpawned_1;
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

        // spawn lady
        if (ladyOnRoofSpawned_0 == null)
        {
            // nps
            ladyOnRoofSpawned_0 = Instantiate(npcSpawners[0].npcsToSpawn[0], npcSpawners[0].transform.position,
                npcSpawners[0].transform.rotation); 
            //human ladder
            Instantiate(npcSpawners[0].gameObjectToSpawn, npcSpawners[0].transform.position, npcSpawners[0].transform.rotation);

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
                    var go = Instantiate(npcSpawners[0].npcsToSpawn[0], npcSpawners[0].transform.position,
                        npcSpawners[0].transform.rotation);

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
                
                gunnWorkingSpawned_1 = Instantiate(npcSpawners[1].npcsToSpawn[0], npcSpawners[1].transform.position,
                    npcSpawners[1].transform.rotation); 
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
        if (qm.activeQuestsIndexes.Contains(4))
        {
            if (blockersFindWood == null)
                blockersFindWood = Instantiate(levelBlockersPrefabs[3], Vector3.zero, Quaternion.identity);
        }
        else if (blockersFindWood)
            blockersFindWood.DestroyBlockers();
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
