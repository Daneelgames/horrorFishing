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
        //GameManager.instance.SaveGame();
    }

    public IEnumerator Init()
    {
        yield return new WaitForSeconds(1);
        
        if (GameManager.instance.tutorialPassed == 1)
        {
            activeCheckpointIndex = GameManager.instance.hubActiveCheckpointIndex;
            var p = PlayerMovement.instance;
            UiManager.instance.Init(false);
            p.teleport = true;
        
            RaycastHit hit;
            if (Physics.Raycast(checkpoints[activeCheckpointIndex].transform.position, Vector3.down, out hit, 500, layerMask))
            {
                p.transform.position = hit.point + Vector3.up;
                print("PLAYER POS AT CHECKPOINT ON TERRAIN: " + p.transform.position);
            }
            else
            {
                p.transform.position = checkpoints[activeCheckpointIndex].transform.position;
                print("PLAYER POS AT CHECKPOINT: " + p.transform.position);   
            }
            p.playerHead.position = p.transform.position;
            print("PLAYER POS: " + p.transform.position);
            //PlayerAudioController.instance.GetNewFootSteps();
            p.teleport = false;
            print("PLAYER POS: " + p.transform.position);   
        }
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
