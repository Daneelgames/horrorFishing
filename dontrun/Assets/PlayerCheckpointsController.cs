using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class PlayerCheckpointsController : MonoBehaviour
{
    public static PlayerCheckpointsController instance;
    
    public List<HubCheckpoint> checkpoints = new List<HubCheckpoint>();
    public LayerMask layerMask;
    public int activeCheckpointIndex = 0;

    void Awake()
    {
        instance = this;
    }

    public void SetActiveCheckpoint(HubCheckpoint c)
    {
        activeCheckpointIndex = checkpoints.IndexOf(c);
        GameManager.instance.hubActiveCheckpointIndex = activeCheckpointIndex;
        GameManager.instance.SaveGame();
    }

    public IEnumerator Init()
    {
        if (GameManager.instance.revolverFound == false)
            yield break;
        
        activeCheckpointIndex = GameManager.instance.hubActiveCheckpointIndex;
        UiManager.instance.Init(false);

        Vector3 newPos = PlayerMovement.instance.transform.position;
    
        RaycastHit hit;
        if (Physics.Raycast(checkpoints[activeCheckpointIndex].transform.position, Vector3.down, out hit, 500, layerMask))
        {
            newPos = hit.point + Vector3.up;
        }
        else
        {
            newPos = checkpoints[activeCheckpointIndex].transform.position;
        }
        
        StartCoroutine(HubItemsSpawner.instance.RespawnPlayerOnContinue(newPos));
    }

    public IEnumerator TaxiToNeededCheckpoint(GameObject bike)
    {
        // get better position 
        Vector3 newPos = checkpoints[0].transform.position;;
        var il = ItemsList.instance;
        if (il.foundHerPiecesOnFloors.Count <= 1)
            newPos = checkpoints[11].transform.position;
        else if (il.foundHerPiecesOnFloors.Count == 2)
            newPos = checkpoints[7].transform.position;
        else if (il.foundHerPiecesOnFloors.Count == 3)
            newPos = checkpoints[1].transform.position;
        else if (il.foundHerPiecesOnFloors.Count == 4)
            newPos = checkpoints[3].transform.position;
        else
        {
            newPos = checkpoints[Random.Range(0, checkpoints.Count)].transform.position;
        }
        
        RaycastHit hit;
        if (Physics.Raycast(newPos, Vector3.down, out hit, 1000, layerMask))
        {
            newPos = hit.point + Vector3.up;
        }
        else
        {
            newPos = checkpoints[activeCheckpointIndex].transform.position;
        }
        
        var pm = PlayerMovement.instance;
        var gm = GameManager.instance;

        
        if (pm.inTransport != null)
        {
            pm.inTransport.ExitTransport();
        }
        
        pm.Teleport(true);
                
        pm.transform.position = newPos;
        gm.ls.PlayerTeleported();
        pm.playerHead.position = pm.transform.position;
        while (gm.paused)
        {
            yield return new WaitForEndOfFrame();   
        }
            
        pm.transform.position = newPos;
        pm.playerHead.position = pm.transform.position;
        pm.Teleport(false);
        bike.SetActive(false);
        bike.transform.position = newPos;
        bike.SetActive(true);


    }
}
