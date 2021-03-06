using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class MoveGunnToCenterOfShere : MonoBehaviour
{
    private bool activated = false;
    public GameObject blocker;
    void OnTriggerEnter(Collider coll)
    {
        if (!activated && coll.gameObject == PlayerMovement.instance.gameObject)
        {
            activated = true;
            blocker.SetActive(true);
            var gunn = HubItemsSpawner.instance.gunnWalkableWithHeads;
            
            if (gunn)
            {
                gunn.mobGroundMovement.enabled = false;   
                gunn.inLove = false;   
                gunn.mobPartsController.agent.SetDestination(transform.position);   
            }
        }
    }
}
