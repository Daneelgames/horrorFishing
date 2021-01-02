using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class HubCheckpoint : MonoBehaviour
{
    void OnTriggerStay(Collider coll)
    {
        if (coll.gameObject == PlayerMovement.instance.gameObject && PlayerCheckpointsController.instance.activeCheckpointIndex != PlayerCheckpointsController.instance.checkpoints.IndexOf(this))
        {
            PlayerCheckpointsController.instance.SetActiveCheckpoint(this);
        }
            
    }
}
