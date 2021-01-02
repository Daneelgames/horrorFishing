using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubItemsSpawner : MonoBehaviour
{
    public List<Transform> itemSpawners = new List<Transform>();
    private ItemsList il;
    private SpawnController sc;

    public static HubItemsSpawner instance;
    
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void SpawnItems()
    {
        il = ItemsList.instance;
        // spawn weapons, items, ammo and skills
        int itemsAmount = itemSpawners.Count;

        for (int i = 0; i < itemsAmount; i++)
        {
            if (itemSpawners.Count <= 1)
                break;
            
            if (Random.value > 0.1f)
            {
                int r = Random.Range(0, itemSpawners.Count);
                SpawnRandomItem(itemSpawners[r].transform.position);
                itemSpawners.RemoveAt(r);
                //itemSpawners.RemoveAt(r);   
            }
        }
    }

    void SpawnRandomItem(Vector3 spawnPos)
    {
        sc = SpawnController.instance;
        var gm = GameManager.instance;

        /*
        RaycastHit hit;
        if (Physics.Raycast(spawnPos, Vector3.down, out hit, 500))
        {
            if (hit.collider.gameObject.layer == 10 || hit.collider.gameObject.layer == 16 ||
                hit.collider.gameObject.layer == 20 || 
                hit.collider.gameObject.layer == 13 )
            {
                spawnPos = hit.point + Vector3.up;
            }
        }*/
        spawnPos += Vector3.up * 1.5f;
        
        float r = Random.value;
        if (r < 0.1f) // weapon
        {
            int weaponIndex = 0;
            weaponIndex = Random.Range(0, sc.weaponPickUpPrefabs.Count);
            sc.InstantiateItem(sc.weaponPickUpPrefabs[weaponIndex], spawnPos, false); 
        }
        else if (r < 0.2f) // item
        {
            var items = gm.arenaLevel.spawnGroups[0].simpleItems;
            sc.InstantiateItem(items[Random.Range(0, items.Length)].item.value, spawnPos, false);
        }
        else if (r < 0.9f) // ammo
        {
            var ammo = gm.arenaLevel.ammoSpawn;
            sc.InstantiateItem(ammo[Random.Range(0, ammo.Length)].value.bulletPack, spawnPos, false);
        }
        else // skill
        {
            sc.InstantiateItem(gm.arenaLevel.spawnGroups[2].simpleItems[0].item.value, spawnPos, false);
        }
    }
}
