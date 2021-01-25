using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;

public class DynamicObstaclesManager : MonoBehaviour
{
    public float handlePropsTimeMin = 1;
    public float handlePropsTimeMax = 5;
    
    public float handleEnemiesTimeMin = 1;
    public float handleEnemiesTimeMax = 30;
    
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
        
        StartCoroutine(HandleDynamicProps());
        StartCoroutine(HandleDynamicMobs());
    }

    private Vector3 spawnPosition;
    private Vector3 spawnNormalDirection;
    private float distanceToObject = 0;
    
    
    IEnumerator HandleDynamicProps()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(handlePropsTimeMin, handlePropsTimeMax));
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
                    if (prop.spawnedObject != null)
                        Destroy(prop.spawnedObject.gameObject);
                    
                    Destroy(prop.gameObject);
                }
                yield return null;
            }

            if (lg.propsInGame.Count < propsSpawnedMax)
            {
                int propsAmountToSpawn = Random.Range(1, 10);
                
                for (int i = 0; i < propsAmountToSpawn; i++)
                {
                    if (Random.value > 0.001f)
                        spawnPosition = GetPositionAroundPoint(pm.transform.position, false);
                    else
                        spawnPosition = GetPositionAroundPoint(pm.transform.position + pm.movementTransform.forward * distanceToDestroy, true);
                
                    if (Vector3.Distance(spawnPosition, pm.transform.position) > 3)
                        AssetSpawner.instance.Spawn(lg.propsReferences[Random.Range(0, lg.propsReferences.Count)], spawnPosition, AssetSpawner.ObjectType.Prop);  
                    
                    yield return null;
                }
            }
        }
    }
    
    IEnumerator HandleDynamicMobs()
    {
        while (true)
        {
            while (QuestManager.instance.activeQuestsIndexes.Count <= 2)
            {
                yield return new WaitForSeconds(1);
            }
            
            yield return new WaitForSeconds(Random.Range(handleEnemiesTimeMin, handleEnemiesTimeMax));
            for (int i = sc.mobsInGame.Count - 1; i >= 0; i--)
            {
                var mob = sc.mobsInGame[i];
                if (mob == null)
                {
                    sc.mobsInGame.RemoveAt(i);
                    continue;
                }

                distanceToObject = Vector3.Distance(pm.transform.position, mob.transform.position); 
                if (distanceToObject > distanceToDestroyFar || (distanceToObject > distanceToDestroy && MouseLook.instance.PositionIsVisibleToPlayer(mob.transform.position) == false && Random.value > 0.5f))
                {
                    Destroy(mob.gameObject);
                }
                yield return null;
            }

            if (sc.mobsInGame.Count < enemiesSpawnedMax)
            {
                if (Random.value > 0.001f)
                    spawnPosition = GetPositionAroundPoint(pm.transform.position, false);
                else
                    spawnPosition = GetPositionAroundPoint(pm.transform.position + pm.movementTransform.forward * distanceToDestroy, true);
                if (Vector3.Distance(spawnPosition, pm.transform.position) > 3)
                    AssetSpawner.instance.Spawn(sc.enemiesReferences[Random.Range(0, sc.enemiesReferences.Count)], spawnPosition, AssetSpawner.ObjectType.Mob);
            }
        }
    }

    Vector3 GetPositionAroundPoint(Vector3 point, bool ignoreVisibility)
    {
        NavMeshHit hit;
        var newPos = point + Random.insideUnitSphere * sphereSpawnRadius;

        for (int i = 0; i < 5; i++)
        {
            if (NavMesh.SamplePosition(newPos, out hit, sphereSpawnRadius, NavMesh.AllAreas) && (ignoreVisibility || MouseLook.instance.PositionIsVisibleToPlayer(hit.position) == false))
            {
                return hit.position;
            }   
        }

        return pm.transform.position;
    }
}