using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.Serialization;

public class LerpMoveToPlayer : MonoBehaviour
{
    public float timeStep = 0.1f;
    public bool aggressive = true;
    private float t = 0;
    public float loopTime = 10;
    private PlayerMovement pm;

    void Start()
    {
        pm = PlayerMovement.instance;
        StartCoroutine(MainLoop());
    }

    IEnumerator MainLoop()
    {
        while (true)
        {
            if (aggressive)
                transform.position = Vector3.Lerp(transform.position, pm.transform.position + Random.insideUnitSphere * Random.Range(3f,15f), t / loopTime);

            t += Time.deltaTime;
            
            yield return new WaitForSeconds(timeStep);
        }
    }

    public void ResetTime()
    {
        t = 0;
    }
}
