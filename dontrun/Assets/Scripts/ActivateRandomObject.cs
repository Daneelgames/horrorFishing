using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using PlayerControls;

public class ActivateRandomObject : MonoBehaviour
{
    public bool disableIfPlayerIsArmed = false;
    public GameObject savedObject;
    public List<GameObject> randomObjects;

    public GameObject staticObject;
    //public bool canDestroyUnusedObjects = true;

    private ItemsList il;

    private void Awake()
    {
        if (disableIfPlayerIsArmed && GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            for (int i = randomObjects.Count - 1; i >= 0; i--)
            {
                Destroy(randomObjects[i]);
                randomObjects.RemoveAt(i);
            }   
            Destroy(gameObject);
            return;
        }
        
        // saved object should be synced
        int savedObjectIndex = Random.Range(0, randomObjects.Count);
        savedObject = randomObjects[savedObjectIndex];
        
        if (gameObject.scene.buildIndex == 4) // this GO located in a game scene
        {
            if (LevelGenerator.instance && !LevelGenerator.instance.randomObjectsActivators.Contains(this))
            {
                LevelGenerator.instance.randomObjectsActivators.Add(this);

                /*
                print("ACTIVATOR IS ADDED. ITS INDEX IS " +
                      LevelGenerator.instance.randomObjectsActivators.IndexOf(this));
                      */
                
                if (LevelGenerator.instance.levelgenOnHost)
                {
                    if (!LevelGenerator.instance.generating)
                    {
                        GLNetworkWrapper.instance.SaveRandomObject(LevelGenerator.instance.randomObjectsActivators.IndexOf(this), savedObjectIndex, true);
                    }
                    else
                    {
                        GLNetworkWrapper.instance.SaveRandomObject(LevelGenerator.instance.randomObjectsActivators.IndexOf(this), savedObjectIndex, false);
                    }
                }
                else if (GLNetworkWrapper.instance == null || !GLNetworkWrapper.instance.coopIsActive)
                {
                    SaveRandomObjectOnClient(savedObjectIndex);
                    ActivateRandomObjectOnClient();
                }
            }   
        }
        else
        {
            //this GO isn't tied to levelgen so activate it in any time
            SaveRandomObjectOnClient(savedObjectIndex);
            ActivateRandomObjectOnClient();
        }
    }

    public void SaveRandomObjectOnClient(int index)
    {
        if (disableIfPlayerIsArmed && ItemsList.instance.PlayerIsArmed() && 
            ((GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false) ||
             (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && LevelGenerator.instance.levelgenOnHost == false)))
        {
            Destroy(gameObject);
        }
        else
        {
            // TEMPORAL FIX
            //////////////
            if (index >= randomObjects.Count) index = randomObjects.Count - 1;
            /// BETTER DELETE THIS SHIT LATER
            /// /////////////////////////
            
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