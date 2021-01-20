using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class KillHcTrigger : MonoBehaviour
{
    public HealthController hcToKill;
    
    void OnTriggerStay(Collider coll)
    {
        if (hcToKill != null && coll.gameObject == PlayerMovement.instance.gameObject)
        {
            hcToKill.Kill();
            hcToKill = null;
            
        }
    }
}
