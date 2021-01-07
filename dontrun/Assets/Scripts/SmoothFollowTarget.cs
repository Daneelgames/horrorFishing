using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class SmoothFollowTarget : MonoBehaviour
{
    public int randomOffset = 0;
    public Transform target;
    public bool lookAtTarget = true;
    public bool aggressiveOnPlayer = false;

    private Vector3 targetVector;
    private PlayerMovement pm;
    
    public float speed = 3;
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
        
        // dont run on client
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && !LevelGenerator.instance.levelgenOnHost)
            return;
        
        pm = PlayerMovement.instance;
        StartCoroutine(FollowTarget());
        if (randomOffset != 0)
            StartCoroutine(GetRandomOffset());
    }

    IEnumerator GetRandomOffset()
    {
        HealthController targetPlayer = PlayerMovement.instance.hc;
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            targetPlayer = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
        }
        
        while (true)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.transform.position);
            
            if (dist > 8 || targetPlayer.health <= targetPlayer.healthMax * 0.2f || !aggressiveOnPlayer)
                targetVector = target.position + new Vector3(Random.Range(-randomOffset * 1f, randomOffset * 1f),
                               Random.Range(-randomOffset * 1f, randomOffset * 1f), Random.Range(-randomOffset * 1f, randomOffset * 1f));
            else if (aggressiveOnPlayer && dist <= 8)
            {
                targetVector = targetPlayer.transform.position + new Vector3(0, Random.Range(randomOffset * 1f, randomOffset * 2f), 0);
            }
            yield return new WaitForSeconds(1);   
        }
    }
    
    IEnumerator FollowTarget()
    {
        while (target != null)
        {
            if (randomOffset != 0)
                transform.position = Vector3.Lerp(transform.position, targetVector, speed * Time.deltaTime);
            else
                transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);
            
            if (lookAtTarget)
                transform.LookAt(target);
            yield return null;
        }
    }
}
