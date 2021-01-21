using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;

public class DynamicObstaclesManager : MonoBehaviour
{
    public float checkGameStateTimeMin = 1;
    public float checkGameStateTimeMax = 5;
    
    public int propsSpawnedMax = 10;
    public int enemiesSpawnedMax = 10;

    public float distanceToDestroy = 30;
    public float distanceToDestroyFar = 70;
    public float sphereSpawnRadius = 30;
    
    private PlayerMovement pm;
    private SpawnController sc;
    private LevelGenerator lg;
    
    void Start()
    {
        pm = PlayerMovement.instance;
        sc = SpawnController.instance;
        lg = LevelGenerator.instance;
        
        StartCoroutine(CheckGameState());
    }

    private Vector3 spawnPosition;
    private Vector3 spawnNormalDirection;
    private float distanceToObject = 0;
    
    
    IEnumerator CheckGameState()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(checkGameStateTimeMin,checkGameStateTimeMax));
            for (int i = lg.propsInGame.Count - 1; i >= 0; i--)
            {
                var prop = lg.propsInGame[i];
                if (prop == null)
                {
                    lg.propsInGame.RemoveAt(i);
                    continue;
                }

                distanceToObject = Vector3.Distance(pm.transform.position, prop.transform.position); 
                if (distanceToObject > distanceToDestroyFar || (distanceToObject > distanceToDestroy && MouseLook.instance.PositionIsVisibleToPlayer(prop.transform.position) == false && Random.value > 0.5f))
                {
                    Destroy(prop.gameObject);
                }
                yield return null;
            }

            if (lg.propsInGame.Count < propsSpawnedMax)
            {
                spawnPosition = GetPositionAroundPlayer();
                if (Vector3.Distance(spawnPosition, pm.transform.position) > 3)
                    AssetSpawner.instance.Spawn(lg.propsReferences[Random.Range(0, lg.propsReferences.Count)], spawnPosition, AssetSpawner.ObjectType.Prop);
            }
        }
    }

    Vector3 GetPositionAroundPlayer()
    {
        
        NavMeshHit hit;
        var newPos = pm.transform.position + Random.insideUnitSphere * sphereSpawnRadius;

        for (int i = 0; i < 5; i++)
        {
            if (NavMesh.SamplePosition(newPos, out hit, sphereSpawnRadius, NavMesh.AllAreas) && MouseLook.instance.PositionIsVisibleToPlayer(hit.position) == false)
            {
                return hit.position;
            }   
        }

        return pm.transform.position;
    }
}