using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class DarknessColliderController : MonoBehaviour
{
    public float effectDistance = 150;
    private bool playerInside = false;

    private bool started = false;
    
    void OnTriggerEnter(Collider coll)
    {
        if (!started && !EndingController.instance.darknessActive && coll.gameObject == PlayerMovement.instance.gameObject)
        {
            started = true;
            EndingController.instance.StartDarknessBeforeEnding();
            StartCoroutine(GetPlayerDistance());
        }
    }

    IEnumerator GetPlayerDistance()
    {
        while (EndingController.instance.darknessActive)
        {
            if (Vector3.Distance(transform.position, PlayerMovement.instance.transform.position) > effectDistance)
                EndingController.instance.StopDarknessInstantly();
                
            yield return new WaitForSeconds(1);
        }
    }

    /*
    void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject == PlayerMovement.instance.gameObject)
        {
            EndingController.instance.StopDarknessInstantly();
        }
    }*/
}
