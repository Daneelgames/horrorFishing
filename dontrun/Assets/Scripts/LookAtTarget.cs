using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    private Transform target;
    public bool lookAtPlayer = true;
 
    void Start()
    {
        if (lookAtPlayer)
            target = PlayerMovement.instance.cameraAnimator.transform; 
        StartCoroutine(Look());
    }

    IEnumerator Look()
    {
        while (true)
        {
            if (target)
                transform.LookAt(target);
            yield return new WaitForSeconds(1);
        }
    }
}