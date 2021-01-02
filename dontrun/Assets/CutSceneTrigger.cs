using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneTrigger : MonoBehaviour
{
    public bool activateByRaycast;
    public GameObject objectToActivate;
    public Collider coll;
    public void PlayerLookedOnTrigger()
    {
        if (activateByRaycast)
        {
            // called when player is looking on trigger at distance of (30 at hub and 6.5 on floor)
            // this uses raycast
            coll.enabled = false;
            objectToActivate.SetActive(true);
            HubProgressionManager.instance.CutSceneActivated(this);
        }
    }

    void PlayerEnteredCollider()
    {
        if (!activateByRaycast)
        {
            coll.enabled = false;
            objectToActivate.SetActive(true);
        }
    }
}
