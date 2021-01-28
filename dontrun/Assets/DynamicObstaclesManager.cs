using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Chat;
using NUnit.Framework;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;

public class DynamicObstaclesManager : MonoBehaviour
{
    public int maximumPropsInGame = 30;
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

    public static DynamicObstaclesManager instance;


    void Awake()
    {
        instance = this;
    }
    
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
                    
                    lg.propsInGame.Remove(prop);
                    StartCoroutine(DestroyGameObjectAnimated(prop.gameObject));
                    //Destroy(prop.gameObject);
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
                
                    if (Vector3.Distance(spawnPosition, pm.transform.position) > 5)
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
            while (WeaponControls.instance.activeWeapon == null)
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
                    sc.mobsInGame.Remove(mob);
                    StartCoroutine(DestroyGameObjectAnimated(mob.gameObject));
                    //Destroy(mob.gameObject);
                }
                yield return null;
            }

            if (sc.mobsInGame.Count < enemiesSpawnedMax)
            {
                if (Random.value > 0.25f)
                    spawnPosition = GetPositionAroundPoint(pm.transform.position, false);
                else
                    spawnPosition = GetPositionAroundPoint(pm.transform.position + pm.movementTransform.forward * distanceToDestroy, true);
                
                if (Vector3.Distance(spawnPosition, pm.transform.position) > 5)
                    AssetSpawner.instance.Spawn(sc.enemiesReferences[Random.Range(0, sc.enemiesReferences.Count)], spawnPosition, AssetSpawner.ObjectType.Mob);
            }
        }
    }

    IEnumerator DestroyGameObjectAnimated(GameObject go)
    {
        float t = 0;
        float tt = Random.Range(1, 3);
        while (t < tt)
        {
            if (go == null)
                yield break;
            
            go.transform.localScale = Vector3.Lerp(go.transform.localScale, Vector3.zero, t / tt);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(go);
    }
    
    public IEnumerator CreateGameObjectAnimated(GameObject go)
    {
        if (LevelGenerator.instance.propsInGame.Count > maximumPropsInGame)
            StartCoroutine(DestroyGameObjectAnimated(LevelGenerator.instance.propsInGame[0].gameObject));
        
        float t = 0;
        float tt = Random.Range(1, 3);
        
        while (t < tt)
        {
            if (go == null)
                yield break;
            
            go.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / tt);
            t += Time.deltaTime;
            yield return null;
        }
    }

    public void CreatePlayersHumanProp(Vector3 pos)
    {
        bool foundProp = false;
        if (lg.propsInGame.Count > 1)
        {
            var propTemp = lg.propsInGame[Random.Range(0, lg.propsInGame.Count)];
            if (MouseLook.instance.PositionIsVisibleToPlayer(propTemp.transform.position) == false &&
                Vector3.Distance(PlayerMovement.instance.transform.position, propTemp.transform.position) > 5)
            {
                foundProp = true;
                propTemp.transform.localScale = Vector3.zero;
                propTemp.transform.position = pos;
                StartCoroutine(CreateGameObjectAnimated(propTemp.gameObject));
                propTemp.humanPropBonesRandomizer.RandomizeBones();   
            }
        }
        
        if (!foundProp)
        {
            AssetSpawner.instance.Spawn(lg.propsReferences[Random.Range(0, lg.propsReferences.Count)], pos, AssetSpawner.ObjectType.Prop);
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