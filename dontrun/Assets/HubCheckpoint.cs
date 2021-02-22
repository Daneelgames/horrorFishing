using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class HubCheckpoint : MonoBehaviour
{
    private bool active = false;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(10);
        active = true; 
    }

    void OnTriggerStay(Collider coll)
    {
        if (!active)
            return;

        if (coll.gameObject == PlayerMovement.instance.gameObject && PlayerCheckpointsController.instance.activeCheckpointIndex != PlayerCheckpointsController.instance.checkpoints.IndexOf(this))
        {
            PlayerCheckpointsController.instance.SetActiveCheckpoint(this);
        }
            
    }
}
