using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Chat;
using NUnit.Framework;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;

public class DynamicObstaclesManager : MonoBehaviour
{
    public List<DynamicObstaclesZone> zones;
    public DynamicObstaclesZone closestZone;
    
    [Header("Global Settings")]
    public int propsSpawnedMax = 10;
    public int enemiesSpawnedMax = 10;
    
    public float distanceToDestroy = 30;
    public float distanceToDestroyFar = 70;
    
    
    [Header("Zone Settings")]
    public float handlePropsTimeMin = 1;
    public float handlePropsTimeMax = 5;
    
    public float handleEnemiesTimeMin = 1;
    public float handleEnemiesTimeMax = 30;
    public float sphereSpawnRadius = 30;


    [Header("Colors")] 
    public Light mainLight;
    
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

    private string skyColorString = "_Tint";
    private Color skyTempColor;
    private Color mainLightTempColor;
    private Color fogTempColor;
    void Update()
    {
        if (closestZone == null) return;

        if (RenderSettings.skybox)
        {
            skyTempColor = Color.Lerp(RenderSettings.skybox.GetColor(skyColorString), closestZone.skyColor, Time.deltaTime / 10);
            RenderSettings.skybox.SetColor(skyColorString, skyTempColor);   
        }
        
        mainLightTempColor = Color.Lerp(mainLight.color, closestZone.mainLightColor, Time.deltaTime / 10);
        mainLight.color = mainLightTempColor;
        
        fogTempColor = Color.Lerp(RenderSettings.fogColor, closestZone.fogColor, Time.deltaTime / 10);
        RenderSettings.fogColor = fogTempColor;
    }

    private Vector3 spawnPosition;
    private Vector3 spawnNormalDirection;
    private float distanceToObject = 0;


    void GetClosestZone()
    {
        float distance = 1000;
        float newDist = 0;
        for (int i = 0; i < zones.Count; i++)
        {
            newDist = Vector3.Distance(PlayerMovement.instance.transform.position, zones[i].transform.position);
            if (newDist < distance)
            {
                distance = newDist;
                closestZone = zones[i];
            }
        }
    }
    
    IEnumerator HandleDynamicProps()
    {
        while (true)
        {
            GetClosestZone();
            yield return new WaitForSeconds(Random.Range(closestZone.handlePropsTimeMin, closestZone.handlePropsTimeMax));
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
                int propsAmountToSpawn = Random.Range(1, 4);
                
                for (int i = 0; i < propsAmountToSpawn; i++)
                {
                    SpawnPropAround();
                    yield return null;
                }
            }
        }
    }

    public void SpawnPropAround()
    {
        if (closestZone.propsReferences.Count == 0)
            return;

        if (Random.value > 0.3f)
            spawnPosition = GetPositionAroundPoint(pm.transform.position, false);
        else
            spawnPosition = GetPositionAroundPoint(pm.transform.position + pm.movementTransform.forward * distanceToDestroy, true);
                
        if (Vector3.Distance(spawnPosition, pm.transform.position) > 5)
            AssetSpawner.instance.Spawn(closestZone.propsReferences[Random.Range(0, closestZone.propsReferences.Count)], spawnPosition, AssetSpawner.ObjectType.Prop);  
    }
    
    IEnumerator HandleDynamicMobs()
    {
        while (true)
        {
            GetClosestZone();
            
            yield return new WaitForSeconds(Random.Range(closestZone.handleEnemiesTimeMin, closestZone.handleEnemiesTimeMax));
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
                SpawnMobAround();
            }
        }
    }

    public void SpawnMobAround()
    {
        if (closestZone.enemiesReferences.Count == 0)
            return;
        
        if (Random.value > 0.25f)
            spawnPosition = GetPositionAroundPoint(pm.transform.position, false);
        else
            spawnPosition = GetPositionAroundPoint(pm.transform.position + pm.movementTransform.forward * distanceToDestroy, true);
                
        if (Vector3.Distance(spawnPosition, pm.transform.position) > 5)
            AssetSpawner.instance.Spawn(closestZone.enemiesReferences[Random.Range(0, closestZone.enemiesReferences.Count)], spawnPosition, AssetSpawner.ObjectType.Mob); 
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
    
    public IEnumerator CreateGameObjectAnimated(GameObject go, Vector3 targetPos)
    {
        if (LevelGenerator.instance.propsInGame.Count > propsSpawnedMax)
            StartCoroutine(DestroyGameObjectAnimated(LevelGenerator.instance.propsInGame[0].gameObject));
        
        float t = 0;
        float tt = Random.Range(1, 3);
        go.transform.localScale = Vector3.zero;
        go.transform.position = targetPos;
        
        while (t < tt)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
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
                propTemp.transform.position = pos + Vector3.up * 500;
                StartCoroutine(CreateGameObjectAnimated(propTemp.gameObject, pos));
                propTemp.humanPropBonesRandomizer.RandomizeBones();   
            }
        }
        
        if (!foundProp)
        {
            AssetSpawner.instance.Spawn(lg.propsReferences[Random.Range(0, lg.propsReferences.Count)], pos, AssetSpawner.ObjectType.Prop);
        }
    }
    
    public void CreatePlayersHumanMob(Vector3 pos)
    {
        bool foundMob = false;
        if (sc.mobsInGame.Count > 1)
        {
            var mobTemp = sc.mobsInGame[Random.Range(0, sc.mobsInGame.Count)];
            if (MouseLook.instance.PositionIsVisibleToPlayer(mobTemp.transform.position) == false &&
                Vector3.Distance(PlayerMovement.instance.transform.position, mobTemp.transform.position) > 5)
            {
                foundMob = true;
                mobTemp.transform.localScale = Vector3.zero;
                mobTemp.transform.position = pos + Vector3.up * 500;
                StartCoroutine(CreateGameObjectAnimated(mobTemp.gameObject, pos));
            }
        }
        
        if (!foundMob)
        {
            AssetSpawner.instance.Spawn(sc.enemiesReferences[Random.Range(0, sc.enemiesReferences.Count)], pos, AssetSpawner.ObjectType.Mob);
        }
    }
    
    Vector3 GetPositionAroundPoint(Vector3 point, bool ignoreVisibility)
    {
        NavMeshHit hit;
        var newPos = point + Random.insideUnitSphere * closestZone.sphereSpawnRadius;

        for (int i = 0; i < 5; i++)
        {
            if (NavMesh.SamplePosition(newPos, out hit, closestZone.sphereSpawnRadius, NavMesh.AllAreas) && (ignoreVisibility || MouseLook.instance.PositionIsVisibleToPlayer(hit.position) == false))
            {
                return hit.position;
            }   
        }

        return pm.transform.position;
    }
}