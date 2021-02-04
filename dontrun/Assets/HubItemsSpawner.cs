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
            bool canSpawnRevolver = !(wc.activeWeapon && wc.activeWeapon.weapon == WeaponPickUp.Weapon.Revolver);

            if (wc.secondWeapon && wc.secondWeapon.weapon == WeaponPickUp.Weapon.Revolver)
                canSpawnRevolver = false;
            
            // revolver start cutscene
            if (canSpawnRevolver)
                Instantiate(fieldEventsSpawners[0].gameObjectToSpawn, fieldEventsSpawners[0].transform.position,
                    fieldEventsSpawners[0].transform.rotation);
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
                    
                    // ladyshoe
                    sc.InstantiateItem(itemSpawners[0].itemToSpawn, itemSpawners[0].transform.position, itemSpawners[0].transform.rotation, false); 
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
}
