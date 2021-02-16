using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Chat;
using NUnit.Framework;
using PlayerControls;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class DynamicObstaclesManager : MonoBehaviour
{
    public List<DynamicObstaclesZone> zones;
    public DynamicObstaclesZone closestZone;
    public AssetReference humanPropreference;
    public List<AssetReference> dykReferences;
    
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

    public Interactable spawnedLeg;
    public Interactable spawnedRevolver;

    void Awake()
    {
        instance = this;
    }
    
    private string skyColorString = "_Tint";
    private Color skyTempColor;
    private Color mainLightTempColor;
    private Color fogTempColor;
    
    IEnumerator Start()
    {
        pm = PlayerMovement.instance;
        sc = SpawnController.instance;
        lg = LevelGenerator.instance;
        
        StartCoroutine(HandleDynamicProps());
        StartCoroutine(HandleDynamicMobs());
        StartCoroutine(HandleDyk());

        while (closestZone == null)
        {
            yield return null;
        }
        
        yield return null;
        
        RenderSettings.skybox.SetColor(skyColorString, closestZone.skyColor);  
        mainLight.color = closestZone.mainLightColor;
        RenderSettings.fogColor = closestZone.fogColor;
        
        // MANAGE WEATHER
        while (true)
        {
            yield return null;
            if (closestZone == null)
            {
                continue;
            }

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
    }

    private GameObject spawnedDyk;
    IEnumerator HandleDyk()
    {
        int dykeToSpawn = -1;
        while (true)
        {
            pm = PlayerMovement.instance;
            
            yield return new WaitForSeconds(10f);

            if (spawnedDyk != null)
            {
                if (Vector3.Distance(spawnedDyk.transform.position, pm.transform.position) > 15f)
                {
                    DestroyGameObjectAnimated(spawnedDyk.gameObject, spawnedDyk.transform.position + Vector3.up * 100f, 5f);
                    spawnedDyk = null;
                }
            }
            else
            {
                // get needed dyke
                dykeToSpawn = GetNeededDyke();
                if (dykeToSpawn >= 0)
                    SpawnDykAround(dykReferences[dykeToSpawn]);
            }
        }
    }

    public void SetNewDykToSpawned(GameObject newdyk)
    {
        print("set new dyk");
        spawnedDyk = newdyk;
    }

    public void SaveLastTalkedDyk(int index)
    {
        if (GameManager.instance.lastTalkedDyk < index)
        {
            GameManager.instance.lastTalkedDyk = index;
            GameManager.instance.SaveGame();
        }
    }
    
    int GetNeededDyke()
    {
        if (GameManager.instance.lastTalkedDyk == -1 && closestZone != zones[0])
        {
            return 0;
        }
        
        if (GameManager.instance.lastTalkedDyk == 0 && pm.hc.health < pm.hc.healthMax)
        {
            return 1;
        }
        
        if (GameManager.instance.lastTalkedDyk == 1 && WeaponControls.instance.activeWeapon && WeaponControls.instance.activeWeapon.weapon == WeaponPickUp.Weapon.Revolver)
        {
            return 2;
        }
        
        if (GameManager.instance.lastTalkedDyk == 2 && WeaponControls.instance.activeWeapon == null)
        {
            return 3;
        }
        
        if (GameManager.instance.lastTalkedDyk == 3 && WeaponControls.instance.activeWeapon && WeaponControls.instance.activeWeapon.weapon == WeaponPickUp.Weapon.LadyShoe)
        {
            return 4;
        }

        return -1;
    }
    public void SpawnDykAround(AssetReference dykeRef)
    {
        if (Random.value > 0.5f)
            spawnPosition = GetPositionAroundPoint(pm.transform.position, false);
        else
            spawnPosition = GetPositionAroundPoint(pm.transform.position + pm.movementTransform.forward * distanceToDestroy, true);
                
        if (Vector3.Distance(spawnPosition, pm.transform.position) > 5)
            AssetSpawner.instance.Spawn(dykeRef, spawnPosition, AssetSpawner.ObjectType.Dyk); 
    }

    public void PlayerDied()
    {
        StopCoroutine(HandleDynamicProps());
        StopCoroutine(HandleDynamicMobs());
        
        for (int i = lg.propsInGame.Count - 1; i >= 0; i--)
        {
            var prop = lg.propsInGame[i];
            if (prop == null)
            {
                lg.propsInGame.RemoveAt(i);
                continue;
            }

            lg.propsInGame.Remove(prop);
            StartCoroutine(DestroyGameObjectAnimated(prop.gameObject, prop.gameObject.transform.position, 1f));
        }
        
        for (int i = sc.mobsInGame.Count - 1; i >= 0; i--)
        {
            var mob = sc.mobsInGame[i];
            if (mob == null)
            {
                sc.mobsInGame.RemoveAt(i);
                continue;
            }

            sc.mobsInGame.Remove(mob);
            StartCoroutine(DestroyGameObjectAnimated(mob.gameObject, mob.gameObject.transform.position, 1f));
        }
        
        StartCoroutine(HandleDynamicProps());
        StartCoroutine(HandleDynamicMobs());
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
            if (lg.propsInGame.Count > 1)
            {
                for (int i = lg.propsInGame.Count - 1; i >= 0; i--)
                {
                    if (lg.propsInGame.Count <= i)
                        continue;
                    
                    var prop = lg.propsInGame[i];
                    if (prop == null)
                    {
                        lg.propsInGame.RemoveAt(i);
                        continue;
                    }

                    distanceToObject = Vector3.Distance(pm.transform.position, prop.transform.position); 
                    if (distanceToObject > distanceToDestroyFar || (distanceToObject > distanceToDestroy && MouseLook.instance.PositionIsVisibleToPlayer(prop.transform.position) == false && Random.value > 0.5f))
                    { 
                        /*
                        if (prop.spawnedObject != null)
                            Destroy(prop.spawnedObject.gameObject);
                            */
                    
                        lg.propsInGame.Remove(prop);
                        StartCoroutine(DestroyGameObjectAnimated(prop.gameObject, prop.gameObject.transform.position, Random.Range(1f,3f)));
                        //Destroy(prop.gameObject);
                    }
                    yield return null;
                }   
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

        if (Random.value > 0.1f)
            spawnPosition = GetPositionAroundPoint(pm.transform.position, false);
        else
            spawnPosition = GetPositionAroundPoint(pm.transform.position + pm.movementTransform.forward * distanceToDestroy, true);
                
        if (Vector3.Distance(spawnPosition, pm.transform.position) > 5)
            AssetSpawner.instance.Spawn(closestZone.propsReferences[Random.Range(0, closestZone.propsReferences.Count)], spawnPosition, AssetSpawner.ObjectType.Prop);  
    }

    public void PlaceSpawnedWeaponOnProp(PropController newProp)
    {
        return;
        
        if (spawnedLeg && spawnedLeg.gameObject.activeInHierarchy == false)
        {
            Destroy(spawnedLeg.gameObject);
            spawnedLeg = null;
        }
        if (spawnedRevolver && spawnedRevolver.gameObject.activeInHierarchy == false)
        {
            Destroy(spawnedRevolver.gameObject);
            spawnedRevolver = null;
        }
        
        var spawnerNew = newProp.spawners[Random.Range(0, newProp.spawners.Count)];
        if (spawnedLeg != null && Vector3.Distance(pm.transform.position, spawnedLeg.transform.position) > 30)
        {
            spawnedLeg.rb.useGravity = false;
            spawnedLeg.rb.constraints = RigidbodyConstraints.FreezeAll;
            spawnedLeg.transform.eulerAngles = new Vector3(Random.Range(0,360), Random.Range(0,360),Random.Range(0,360));
            spawnedLeg.transform.parent = spawnerNew.transform;
            spawnedLeg.transform.localPosition = Vector3.zero;
            spawnedLeg.insideTheProp = true;
            newProp.spawnedObject = spawnedLeg;
        }
        else if (spawnedRevolver != null && Vector3.Distance(pm.transform.position, spawnedRevolver.transform.position) > 30)
        {
            spawnedRevolver.rb.useGravity = false;
            spawnedRevolver.rb.constraints = RigidbodyConstraints.FreezeAll;
            spawnedRevolver.transform.eulerAngles = new Vector3(Random.Range(0,360), Random.Range(0,360),Random.Range(0,360));
            spawnedRevolver.transform.parent = spawnerNew.transform;
            spawnedRevolver.transform.localPosition = Vector3.zero;
            spawnedRevolver.insideTheProp = true;
            newProp.spawnedObject = spawnedRevolver;
        }
    }
    
    IEnumerator HandleDynamicMobs()
    {
        while (true)
        {
            GetClosestZone();
            
            yield return new WaitForSeconds(Random.Range(closestZone.handleEnemiesTimeMin, closestZone.handleEnemiesTimeMax));
            if (sc.mobsInGame.Count > 1)
            {
                for (int i = sc.mobsInGame.Count - 1; i >= 0; i--)
                {
                    if (sc.mobsInGame.Count <= i)
                        continue;

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
                        StartCoroutine(DestroyGameObjectAnimated(mob.gameObject, mob.gameObject.transform.position, Random.Range(1f,3f)));
                        //Destroy(mob.gameObject);
                    }
                    yield return null;
                }   
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
        
        if (Random.value > 0.5f)
            spawnPosition = GetPositionAroundPoint(pm.transform.position, false);
        else
            spawnPosition = GetPositionAroundPoint(pm.transform.position + pm.movementTransform.forward * distanceToDestroy, true);
                
        if (Vector3.Distance(spawnPosition, pm.transform.position) > 5)
            AssetSpawner.instance.Spawn(closestZone.enemiesReferences[Random.Range(0, closestZone.enemiesReferences.Count)], spawnPosition, AssetSpawner.ObjectType.Mob); 
    }

    public IEnumerator DestroyGameObjectAnimated(GameObject go, Vector3 targetPos, float tt)
    {
        float t = 0;
        Vector3 startScale = go.transform.localScale;
        Vector3 startPos = go.transform.position;
        while (t < tt)
        {
            if (go == null)
                yield break;
            
            go.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / tt);
            go.transform.position = Vector3.Lerp(startPos, targetPos, t / tt); 
            t += Time.deltaTime;
            yield return null;
        }

        
        Destroy(go);
    }
    
    public IEnumerator CreateGameObjectAnimated(GameObject go, Vector3 targetPos, Vector3 targetScale)
    {
        if (LevelGenerator.instance.propsInGame.Count > propsSpawnedMax)
            StartCoroutine(DestroyGameObjectAnimated(LevelGenerator.instance.propsInGame[0].gameObject, LevelGenerator.instance.propsInGame[0].gameObject.transform.position, Random.Range(1f,3f)));
        
        float t = 0;
        float tt = Random.Range(1, 3);
        go.transform.localScale = Vector3.zero;
        go.transform.position = targetPos;
        
        while (t < tt)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (go == null)
                yield break;
            
            go.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t / tt);
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
            var initialLocalScale = propTemp.transform.localScale;
            if (propTemp.humanPropBonesRandomizer && MouseLook.instance.PositionIsVisibleToPlayer(propTemp.transform.position) == false &&
                Vector3.Distance(PlayerMovement.instance.transform.position, propTemp.transform.position) > 5)
            {
                foundProp = true;
                propTemp.transform.localScale = Vector3.zero;
                propTemp.transform.position = pos + Vector3.up * 500;
                StartCoroutine(CreateGameObjectAnimated(propTemp.gameObject, pos, initialLocalScale));
                
                if (propTemp && propTemp.humanPropBonesRandomizer)
                    propTemp.humanPropBonesRandomizer.RandomizeBones();   
            }
        }
        
        if (!foundProp)
        {
            AssetSpawner.instance.Spawn(humanPropreference, pos, AssetSpawner.ObjectType.Prop);
        }
    }
    
    public void CreatePlayersHumanMob(Vector3 pos)
    {
        bool foundMob = false;
        if (sc.mobsInGame.Count > 1)
        {
            var mobTemp = sc.mobsInGame[Random.Range(0, sc.mobsInGame.Count)];
            var initialLocalScale = mobTemp.transform.localScale;
            if (MouseLook.instance.PositionIsVisibleToPlayer(mobTemp.transform.position) == false &&
                Vector3.Distance(PlayerMovement.instance.transform.position, mobTemp.transform.position) > 5)
            {
                foundMob = true;
                mobTemp.transform.localScale = Vector3.zero;
                mobTemp.transform.position = pos + Vector3.up * 500;
                StartCoroutine(CreateGameObjectAnimated(mobTemp.gameObject, pos, initialLocalScale));
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
        var newPos = point + Random.onUnitSphere * Random.Range(closestZone.sphereSpawnRadiusMin, closestZone.sphereSpawnRadiusMax);

        for (int i = 0; i < 5; i++)
        {
            if (NavMesh.SamplePosition(newPos, out hit, closestZone.sphereSpawnRadiusMax, NavMesh.AllAreas) && (ignoreVisibility || MouseLook.instance.PositionIsVisibleToPlayer(hit.position) == false))
            {
                return hit.position;
            }   
        }

        return pm.transform.position;
    }
}