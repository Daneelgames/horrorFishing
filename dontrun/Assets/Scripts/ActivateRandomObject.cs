using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using PlayerControls;

public class ActivateRandomObject : MonoBehaviour
{
    public GameObject savedObject;
    public List<GameObject> randomObjects;

    public GameObject staticObject;
    //public bool canDestroyUnusedObjects = true;

    private ItemsList il;

    private void Start()
    {
        
        // saved object should be synced
        int savedObjectIndex = Random.Range(0, randomObjects.Count);
        savedObject = randomObjects[savedObjectIndex];
        
        SaveRandomObjectOnClient(savedObjectIndex);
        ActivateRandomObjectOnClient();
    }

    public void SaveRandomObjectOnClient(int index)
    {
        if (index >= randomObjects.Count) index = randomObjects.Count - 1;
        
        for (int i = randomObjects.Count - 1; i >= 0; i--)
        {
            randomObjects[i].SetActive(false);  
        }

        savedObject = randomObjects[index];
    
        for (int i = randomObjects.Count - 1; i >= 0; i--)
        {
            if (randomObjects[i].gameObject != savedObject)
            {
                Destroy(randomObjects[i]);
                randomObjects.RemoveAt(i);   
            }
        }
    }

    public void ActivateRandomObjectOnClient()
    {
        il = ItemsList.instance;
        
        savedObject.SetActive(true);
        
        if (staticObject)
            staticObject.SetActive(true);
        Interactable newDrop = savedObject.GetComponent<Interactable>();
        
        if (newDrop && newDrop.weaponPickUp)
        {
            if (newDrop.weaponPickUp.weaponConnector)
                newDrop.weaponPickUp.weaponConnector.GenerateOnSpawn();

            bool deadWeapon = Random.value <= GameManager.instance.level.deadWeaponRate;
            bool npcWeapon = Random.value <= GameManager.instance.level.npcWeaponRate;
            newDrop.weaponPickUp.weaponDataRandomier.GenerateOnSpawn(deadWeapon, npcWeapon); // spawn dead weapon
        }
    }

    [ContextMenu("RandomizeObjectsInEditor")]
    public void RandomizeObjectsInEditor()
    {
        for (int i = randomObjects.Count - 1; i >= 0; i--)
        {
            randomObjects[i].SetActive(false);  
        }
        
        savedObject = randomObjects[Random.Range(0, randomObjects.Count)]; 
        
        for (int i = randomObjects.Count - 1; i >= 0; i--)
        {
            if (randomObjects[i].gameObject != savedObject)
            {
                DestroyImmediate(randomObjects[i]);
                randomObjects.RemoveAt(i);   
            }
        }
        savedObject.SetActive(true);
        DestroyImmediate(this);
    }

    void OnDestroy()
    {
        /*
        if (gameObject.scene.buildIndex == 4 && LevelGenerator.instance && LevelGenerator.instance.randomObjectsActivators.Contains(this))
            LevelGenerator.instance.randomObjectsActivators.Remove(this);
            */
    }
}