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
    public List<GameObject> levelBlockersPrefabs = new List<GameObject>();
    
    private ItemsList il;
    private SpawnController sc;

    public static HubItemsSpawner instance;

    private HealthController strangerWomanSpawned;
    private HealthController ladyOnRoofSpawned;
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
        if (ladyOnRoofSpawned == null)
        {
            // nps
            ladyOnRoofSpawned = Instantiate(npcSpawners[0].npcsToSpawn[0], npcSpawners[0].transform.position,
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
        
        #region Quest 0. Shoes on beach

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
                if (ladyOnRoofSpawned == null)
                {
                    var go = Instantiate(npcSpawners[0].npcsToSpawn[0], npcSpawners[0].transform.position,
                        npcSpawners[0].transform.rotation);

                    ladyOnRoofSpawned = go;
                }
            }
        #endregion

        #region quest 3 Return the shoe

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

        UpdateLevelBlockers();
        
        for (int i = 0; i < dialogueOnQuestsChangers.Count; i++)
        {
            dialogueOnQuestsChangers[i].UpdateDialogue();
        }
    }

    private GameObject blockersBeforeGoingToGunn;
    private GameObject blockersBeforeGettingShoeQuest;
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
            StartCoroutine(DynamicObstaclesManager.instance.DestroyGameObjectAnimated(blockersBeforeGettingShoeQuest, Vector3.up * 200, 30f));
        
        // before going to gunn
        if (!qm.activeQuestsIndexes.Contains(3) && !qm.completedQuestsIndexes.Contains(3))
        {
            if (blockersBeforeGoingToGunn == null)
                blockersBeforeGoingToGunn = Instantiate(levelBlockersPrefabs[1], Vector3.zero, Quaternion.identity);
        }
        else if (blockersBeforeGoingToGunn)
            StartCoroutine(DynamicObstaclesManager.instance.DestroyGameObjectAnimated(blockersBeforeGoingToGunn, Vector3.up * 200, 30f));
    }
}
