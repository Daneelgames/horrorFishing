using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class HubItemsSpawner : MonoBehaviour
{
    public List<CitySpawner> itemSpawners = new List<CitySpawner>();
    public List<CitySpawner> monstersSpawners = new List<CitySpawner>();
    public List<CitySpawner> npcSpawners = new List<CitySpawner>();
    public List<CitySpawner> fieldEventsSpawners = new List<CitySpawner>();
    
    private ItemsList il;
    private SpawnController sc;

    public static HubItemsSpawner instance;

    private GameObject mrSunSpawned;
    private GameObject startGoldSpawned;
    private HealthController ladyOnRoofSpawned;
    private GameObject shoesOnBeachSpawned;
    private List<HealthController> shoeMimicsSpawned = new List<HealthController>();
    
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void UpdateHub()
    {
        // check what to spawn based on what quests are active
        var qm = QuestManager.instance;
        sc = SpawnController.instance;
        if (mrSunSpawned == null)
        {
            mrSunSpawned = Instantiate(fieldEventsSpawners[2].gameObjectToSpawn, fieldEventsSpawners[2].transform.position,
                fieldEventsSpawners[2].transform.rotation);
        }
        
        #region Quest 0. Shoes on beach

            if (qm.activeQuestsIndexes.Contains(0)) // shoe quest is active
            {
                // spawn shoes on beach if not spawned
                if (shoesOnBeachSpawned == null)
                {
                    var go = Instantiate(fieldEventsSpawners[1].gameObjectToSpawn, fieldEventsSpawners[1].transform.position,
                        fieldEventsSpawners[1].transform.rotation);

                    shoesOnBeachSpawned = go;
                    
                    sc.InstantiateItem(itemSpawners[0].itemToSpawn, itemSpawners[0].transform.position, false); 
                }
                
                // spawn mob
                if (shoeMimicsSpawned.Count == 0)
                {
                    for (int i = 0; i < monstersSpawners.Count; i++)
                    {
                        var go = Instantiate(monstersSpawners[0].monstersToSpawn[0], monstersSpawners[i].transform.position,
                            monstersSpawners[i].transform.rotation);

                        shoeMimicsSpawned.Add(go);   
                    }
                }

                // spawn lady
                if (ladyOnRoofSpawned == null)
                {
                    var go = Instantiate(npcSpawners[0].npcsToSpawn[0], npcSpawners[0].transform.position,
                        npcSpawners[0].transform.rotation);

                    ladyOnRoofSpawned = go;
                }
                
                //remove start gold
                if (startGoldSpawned != null)
                {
                    Destroy(startGoldSpawned);
                    startGoldSpawned = null;
                }
            }
            else if (qm.completedQuestsIndexes.Contains(0)) // shoe quest is completed
            {
                // remove lady on the roof
                if (ladyOnRoofSpawned != null)
                    ladyOnRoofSpawned.Kill();
                
                //remove shoes
                if (shoesOnBeachSpawned != null)
                {
                    Destroy(shoesOnBeachSpawned);
                    shoesOnBeachSpawned = null;
                }
            }
            else // quest didnt even started
            {
                // spawn start pile of gold
                if (startGoldSpawned == null)
                {
                    var go = Instantiate(fieldEventsSpawners[0].gameObjectToSpawn, fieldEventsSpawners[0].transform.position,
                        fieldEventsSpawners[0].transform.rotation);

                    startGoldSpawned = go;
                }
                
                if (ladyOnRoofSpawned == null)
                {
                    var go = Instantiate(npcSpawners[0].npcsToSpawn[0], npcSpawners[0].transform.position,
                        npcSpawners[0].transform.rotation);

                    ladyOnRoofSpawned = go;
                }
            }
        #endregion
        
    }
    
    
    void SpawnRandomItem(Vector3 spawnPos)
    {
        sc = SpawnController.instance;
        var gm = GameManager.instance;

        spawnPos += Vector3.up * 0.5f;
        
        if (Random.value < 0.2f) // item
        {
            var items = gm.arenaLevel.spawnGroups[0].simpleItems;
            sc.InstantiateItem(items[Random.Range(0, items.Length)].item.value, spawnPos, false);
        }
        else 
        {
            var ammo = gm.arenaLevel.ammoSpawn;
            sc.InstantiateItem(ammo[Random.Range(0, ammo.Length)].value.bulletPack, spawnPos, false);
        }
    }

    void SpawnWeapons()
    {
        sc = SpawnController.instance;
        
        List<int> availableWeapons = new List<int>();
        for (int i = 0; i < SpawnController.instance.weaponPickUpPrefabs.Count; i++)
        {
            availableWeapons.Add(i); 
        }
        
        List<CitySpawner> tempSpawners = new List<CitySpawner>(itemSpawners);

        float distance = 2000;
        CitySpawner closestToPlayerSpawner = null;
        for (int j = 0; j < tempSpawners.Count; j++)
        {
            float newDist = Vector3.Distance(tempSpawners[j].transform.position, PlayerMovement.instance.transform.position);
            if (newDist < distance)
            {
                distance = newDist;
                closestToPlayerSpawner = tempSpawners[j];
            }
        }

        if (closestToPlayerSpawner != null)
        {
            tempSpawners.Remove(closestToPlayerSpawner);
            
            int weaponIndex = Random.Range(0, availableWeapons.Count);
            
            sc.InstantiateItem(sc.weaponPickUpPrefabs[availableWeapons[weaponIndex]], closestToPlayerSpawner.transform.position, false); 
            availableWeapons.RemoveAt(weaponIndex);
        }
        else
        {
            Debug.LogError("No weapon spawner found");
        }
        
        
        for (int i = availableWeapons.Count - 1; i >= 0; i++)
        {
            if (availableWeapons.Count <= 0)
                break;
            
            int weaponIndex = Random.Range(0, availableWeapons.Count);
            int spawnerIndex = Random.Range(0, tempSpawners.Count);
            
            sc.InstantiateItem(sc.weaponPickUpPrefabs[availableWeapons[weaponIndex]], tempSpawners[spawnerIndex].transform.position, false);
            
            availableWeapons.RemoveAt(weaponIndex);
        }
    }
}
