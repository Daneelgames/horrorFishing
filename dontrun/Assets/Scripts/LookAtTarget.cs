using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    private Transform target;
    public bool lookAtPlayer = true;

    public float lookTime = 1;
    public float lookDistance = 25;
    public float lookStep = 1;
 
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
            yield return new WaitForSeconds(lookStep);
            if (Vector3.Distance(transform.position, target.transform.position) > lookDistance)
            {   
                yield return new WaitForSeconds(lookStep);
            }
            else
            {
                StartCoroutine(SmoothLookAtPlayer(target.transform.position));
            }
        }
    }
    
    private float t = 0;
    private float tt = 1;
    Quaternion startQuaternion = Quaternion.identity;
    Quaternion endQuaternion = Quaternion.identity;
    IEnumerator SmoothLookAtPlayer(Vector3 playerPos)
    {
        t = 0;
        tt = lookTime;
        startQuaternion = transform.rotation;
        endQuaternion.SetLookRotation(transform.position - playerPos, Vector3.up);
        endQuaternion.eulerAngles = new Vector3(0, endQuaternion.eulerAngles.y, 0);
        while (t < tt)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startQuaternion, endQuaternion, t/tt);
            yield return null;
        }    
    }
}