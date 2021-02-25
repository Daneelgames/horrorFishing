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
    public bool offset180 = false;
    public Transform childToSetEuler;
    public Vector3 childNewEuler;
    public bool randomizeChildScale = false;
 
    void Awake()
    {
        StopAllCoroutines();
        
        if (lookAtPlayer && PlayerMovement.instance)
            target = PlayerMovement.instance.cameraAnimator.transform;

        if (childToSetEuler)
        {
            transform.localRotation = Quaternion.identity;
            var newQuaternion = childToSetEuler.localRotation;
            newQuaternion.eulerAngles = childNewEuler;
            childToSetEuler.localRotation = newQuaternion;
        }

        StartCoroutine(Look());

        if (randomizeChildScale)
            StartCoroutine(RandomizeChildScale());
    }

    IEnumerator RandomizeChildScale()
    {
        var child = transform.GetChild(0);
        if (child == null) yield break;
        var initScale = child.transform.localScale;
        while (true)
        {
            child.transform.localScale = new Vector3(initScale.x + Random.Range(-initScale.x / 5, initScale.x / 5),
                                                     initScale.y + Random.Range(-initScale.y / 5, initScale.y / 5),
                                                     initScale.z + Random.Range(-initScale.z / 5, initScale.z / 5)); 
            yield return null;
        }
    }
    
    IEnumerator Look()
    {
        if (target == null)
            yield break;
        
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
        if (offset180)
            endQuaternion.SetLookRotation(playerPos - transform.position, Vector3.up);
        else
            endQuaternion.SetLookRotation(transform.position - playerPos, Vector3.up);
        //endQuaternion.eulerAngles = new Vector3(0, endQuaternion.eulerAngles.y, 0);
        while (t < tt)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startQuaternion, endQuaternion, t/tt);
            yield return null;
        }    
    }
}