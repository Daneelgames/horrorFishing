using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HubProgressionManager : MonoBehaviour
{
    public static HubProgressionManager instance;
    
    public List<GameObject> questEventsPrefabs = new List<GameObject>();
    
    public List<Interactable> permanentPickups = new List<Interactable>();
    
    [HideInInspector]
    public List<LockObject> hubDoors = new List<LockObject>();
    
    [HideInInspector]
    public List<Interactable> goldenKeys = new List<Interactable>();
    
    [HideInInspector]
    public List<HealthController> roseNpcs = new List<HealthController>();
    
    public  List<CutSceneTrigger> cutScenesInHub = new List<CutSceneTrigger>();

    private QuestManager qm;
    private GameManager gm;
    private GutProgressionManager gpm;
    
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gm = GameManager.instance;
        qm = QuestManager.instance;
        gpm = GutProgressionManager.instance;
    }

    public void UpdateHub()
    {
        // check what to spawn based on what quests are active
        qm = QuestManager.instance;
        
        if (qm.activeQuestsIndexes.Contains(0)) // shoe quest is active
        {
            // spawn shoes on beach if not spawned
            // store them as questEvent
        }
        else if (qm.completedQuestsIndexes.Contains(0)) // shoe quest is completed
        {
            // remove lady on the roof
        }
        else // quest didnt even started
        {
            // spawn start pile of gold
        }
    }
    
    public void FieldAfterDeath(bool newVisit)
    {
        return;
        if (newVisit)
            gm.hubVisits++;
        
        qm.StartQuest(1);

        if (gm.snoutFound == 0)
        {
            questEventsPrefabs[0].SetActive(true); // intro bike
        }
        else if (gm.snoutFound == 1)
        {
            if (ItemsList.instance.herPiecesApplied.Count > 0)
            {
                if (qm.activeQuestsIndexes.Contains(10))
                {
                    questEventsPrefabs[13].SetActive(true); // bike after heart
                }
                else
                {
                    questEventsPrefabs[9].SetActive(true); // bike after single part applied   
                }
            }
            else
            {
                if (qm.activeQuestsIndexes.Contains(2))
                {
                    questEventsPrefabs[5].SetActive(true); // finish first world bike
                }
                else
                {
                    questEventsPrefabs[1].SetActive(true); // default bike
                }
            }
        }


        if (qm.activeQuestsIndexes.Contains(7))
        {
            questEventsPrefabs[7].SetActive(false); // hide carl if he already taken
        }

        if (qm.activeQuestsIndexes.Contains(8))
        {
            //hide carl and show glutton
            questEventsPrefabs[7].SetActive(false); 
            questEventsPrefabs[8].SetActive(true); // glutton
        }
        
        if (qm.activeQuestsIndexes.Contains(9) || qm.completedQuestsIndexes.Contains(9))
        {
            questEventsPrefabs[10].SetActive(false); // royal is killed
            questEventsPrefabs[11].SetActive(true); // valentine is free
        }
        else
        {
            questEventsPrefabs[10].SetActive(true); // royal is alive
            questEventsPrefabs[11].SetActive(false); // valentine is playing
        }

        if (gm.hubVisits == 2 && !qm.activeQuestsIndexes.Contains(11))
        {
            questEventsPrefabs[2].SetActive(true); // intro son
        }
        else if (gm.hubVisits > 2 || qm.activeQuestsIndexes.Contains(11))
            questEventsPrefabs[3].SetActive(true); // default son

        if (ItemsList.instance.savedQuestItems.Contains(8))
        {
            questEventsPrefabs[14].SetActive(false); // camera
        }

        for (int i = 0; i < gm.permanentPickupsTakenIndexesInHub.Count; i++)
        {
            permanentPickups[gm.permanentPickupsTakenIndexesInHub[i]].gameObject.SetActive(false);
        }

        if (gm.demo)
        {
            for (int i = 0; i < goldenKeys.Count; i++)
            {
                goldenKeys[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < gm.goldenKeysFoundInHub.Count; i++)
            {
                goldenKeys[gm.goldenKeysFoundInHub[i]].gameObject.SetActive(false);
            }   
        }

        if (!gm.demo)
        {
            for (int i = 0; i < gm.doorsOpenedIndexesInHub.Count; i++)
            {
                hubDoors[gm.doorsOpenedIndexesInHub[i]].gameObject.SetActive(false);
            }   
        }

        SpawnRoseNpc();

        SpawnCutScenesInHub();
    }

    void SpawnRoseNpc()
    {
        if (!gm.roseNpcsInteractedInHub.Contains(0) && (qm.activeQuestsIndexes.Contains(2) || ItemsList.instance.herPiecesApplied.Count > 0))
        {
            ElevatorController.instance.Deactivate();
            roseNpcs[0].gameObject.SetActive(true); // rose elevator
        }
        else if (!gm.roseNpcsInteractedInHub.Contains(1) && (qm.activeQuestsIndexes.Contains(4) || ItemsList.instance.herPiecesApplied.Count > 1))
        {
            roseNpcs[1].gameObject.SetActive(true); // rose factory
        }
        else if (!gm.roseNpcsInteractedInHub.Contains(3) && (gm.roseNpcsInteractedInHub.Count >= 2))
        {
            roseNpcs[3].gameObject.SetActive(true); // cabins
        }
        else if (!gm.roseNpcsInteractedInHub.Contains(4) && (gm.roseNpcsInteractedInHub.Count >= 2))
        {
            roseNpcs[4].gameObject.SetActive(true); // city
        }
        else if (!gm.roseNpcsInteractedInHub.Contains(5) && (gm.roseNpcsInteractedInHub.Count >= 2))
        {
            roseNpcs[5].gameObject.SetActive(true); // bar
        }
        
        
        if (!gm.roseNpcsInteractedInHub.Contains(2) && (gm.roseNpcsInteractedInHub.Count >= 2))
        {
            roseNpcs[2].gameObject.SetActive(true); // dead on field
        }
        else if (!gm.roseNpcsInteractedInHub.Contains(6) && (gm.roseNpcsInteractedInHub.Count >= 2))
        {
            roseNpcs[6].gameObject.SetActive(true); // disabled cage
        }
    }

    void SpawnCutScenesInHub()
    {
        if (!gm.cutscenesInteractedInHub.Contains(0) && ItemsList.instance.foundHerPiecesOnFloors.Count > 0)
        {
            cutScenesInHub[0].gameObject.SetActive(true); // first biome beaten cutScene
        }
    }

    public void CutSceneActivated(CutSceneTrigger cutScene)
    {
        for (int i = 0; i < cutScenesInHub.Count; i++)
        {
            if (cutScene.gameObject == cutScenesInHub[i].gameObject)
            {
                gm = GameManager.instance;
                if (!gm.cutscenesInteractedInHub.Contains(i))
                    gm.cutscenesInteractedInHub.Add(i);
                break;
            }
        }
    }
    
    public void PermanentPickupPickedUp(ResourcePickUp pickUp)
    {
        for (int i = 0; i < permanentPickups.Count; i++)
        {
            if (pickUp.gameObject == permanentPickups[i].gameObject)
            {
                if (!gm.permanentPickupsTakenIndexesInHub.Contains(i))
                    gm.permanentPickupsTakenIndexesInHub.Add(i);
                break;
            }
        }
    }
    public void GoldenKeyPickUpInHub(ResourcePickUp pickUp)
    {
        for (int i = 0; i < goldenKeys.Count; i++)
        {
            if (pickUp.gameObject == goldenKeys[i].gameObject)
            {
                if (!gm.goldenKeysFoundInHub.Contains(i))
                    gm.goldenKeysFoundInHub.Add(i);
                break;
            }
        }
    }

    public void HubDoorOpened(LockObject lockObject)
    {
        for (int i = 0; i < hubDoors.Count; i++)
        {
            if (lockObject.gameObject == hubDoors[i].gameObject)
            {
                if (!gm.doorsOpenedIndexesInHub.Contains(i))
                    gm.doorsOpenedIndexesInHub.Add(i);
                break;
            }
        }
    }
}