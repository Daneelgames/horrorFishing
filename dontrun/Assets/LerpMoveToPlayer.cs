using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.Serialization;

public class LerpMoveToPlayer : MonoBehaviour
{
    public float timeStep = 0.1f;
    public bool aggressive = false;
    public float moveSpeed = 10;
    private PlayerMovement pm;
    private Vector3 targetPos;
    public float minWanderingTime = 5f;
    public float maxWanderingTime = 90f;

    public static LerpMoveToPlayer instance;
    public HealthController hc;

    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        pm = PlayerMovement.instance;
        StartCoroutine(Wandering());
        StartCoroutine(MainLoop());
    }

    public void SetAggressive()
    {
        StopAllCoroutines();
        hc.invincible = false;
        aggressive = true;
        ResetTime();
        StartCoroutine(MainLoop());
    }

    public void RespawnBossOnPlayerDeath()
    {
        print("boss respawned?");
        HubItemsSpawner.instance.RespawnBoss();
        hc.invincible = false;
        hc.Kill();
    }

    IEnumerator Wandering()
    {
        while (!aggressive)
        {
            List<HubCheckpoint> checkpointsTemp = new List<HubCheckpoint>(PlayerCheckpointsController.instance.checkpoints);

            var closestCheckpoint = GetClosestCheckpoint(transform.position, checkpointsTemp);
            checkpointsTemp.Remove(closestCheckpoint);
            
            targetPos = GetClosestCheckpoint(PlayerMovement.instance.transform.position, checkpointsTemp).transform.position;
            
            yield return new WaitForSeconds(Random.Range(minWanderingTime, maxWanderingTime));
        }
    }

    HubCheckpoint GetClosestCheckpoint(Vector3 originPoint, List<HubCheckpoint> checkpointsTemp)
    {
        float distance = 1000f;
        float newDistance = 0;
            
        HubCheckpoint closestCheckpoint = null;

        for (int i = 0; i < checkpointsTemp.Count; i++)
        {
            newDistance = Vector3.Distance(originPoint, checkpointsTemp[i].transform.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                closestCheckpoint = checkpointsTemp[i];
            }
        }

        return closestCheckpoint;
    }
    
    IEnumerator MainLoop()
    {
        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
            
            yield return null;
        }
    }

    public void ResetTime()
    {
        if (aggressive)
            targetPos = pm.transform.position;
        
        //targetPos = pm.transform.position + Random.insideUnitSphere * Random.Range(3f,15f);
    }
}
