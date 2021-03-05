using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class MoveGunnToCenterOfShere : MonoBehaviour
{
    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject == PlayerMovement.instance.gameObject)
        {
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
