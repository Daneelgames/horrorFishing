using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class AiDirector : MonoBehaviour
{
    public static AiDirector instance;

    PlayerMovement pm;

    public List<LightController> lights;

    public List<AudioSource> soundSources;
    public float soundSourceMinDistance = 20;
    public float soundSourceMaxDistance = 50;
    LevelGenerator lg;
    private GameManager gm;
    ItemsList il;
    Coroutine createEventCoroutine;
    Coroutine createEventAfterDelayCoroutine;
    List <AudioSource> activeSources = new List<AudioSource>();
    private bool wallBreaked = false;

    private void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        gm = GameManager.instance;
        il = ItemsList.instance;
        lg = LevelGenerator.instance;
        pm = PlayerMovement.instance;
        if (!GLNetworkWrapper.instance || GLNetworkWrapper.instance.coopIsActive == false)
        {
            // SOLO
            createEventAfterDelayCoroutine = StartCoroutine(CreateEventAterDelay());
            StartCoroutine(CheckDistances());   
        }
        else if (LevelGenerator.instance.levelgenOnHost)
        {
            // RUN ON HOST
            createEventAfterDelayCoroutine = StartCoroutine(CreateEventAterDelay());
            StartCoroutine(CheckDistances());   
        }
    }

    IEnumerator CheckDistances()
    {
        while (true)
        {
            if (activeSources.Count > 0)
            {
                for (int i = activeSources.Count - 1; i >= 0; i--)
                {
                    if (activeSources[i] != null)
                    {
                        if (Vector3.Distance(pm.transform.position, activeSources[i].transform.position) < 7)
                        {
                            StartCoroutine(StopActiveAudioSource(activeSources[i]));
                        }   
                    }
                    else
                    {
                        activeSources.RemoveAt(i);
                    }
                }   
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator StopActiveAudioSource(AudioSource source)
    {
        float t = 0;
        float tt = 1;
        float initVolume = source.volume;
        
        if (!wallBreaked && Random.value <= 0.1f)
        {
            wallBreaked = true;
            BreakClosestWall(false, 30);
        }
        
        while (t < tt)
        {
            yield return null;
            if (source == null) break;
            
            source.volume = Mathf.Lerp(initVolume, 0, t / tt);
            t += Time.deltaTime;
        }

        if (source != null)
        {
            Destroy(source.gameObject);
            activeSources.Remove(source);   
        }
    }
    

    IEnumerator CreateEventAterDelay()
    {
        yield return new WaitForSeconds(Random.Range(15f, 90f));

        createEventCoroutine = StartCoroutine(CreateEvent());
    }

    public void Reset()
    {
        if (createEventCoroutine != null)
            StopCoroutine(createEventCoroutine);

        if (createEventAfterDelayCoroutine != null)
            StopCoroutine(createEventAfterDelayCoroutine);

        createEventCoroutine = StartCoroutine(CreateEvent());
    }

    IEnumerator CreateEvent()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(20f, 100));

            // hazards
            float r = Random.value;
            if (r > 0.75f)
            {
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && LevelGenerator.instance.levelgenOnHost == false) 
                {
                    
                }
                else
                {
                    if (il.badReputaion >= 2)
                        SpawnController.instance.SpawnFloorBlade(null);   
                }
            }
            else if (!MobSoundSource()) // sound
            {
                if (!SpawnSoundSource())
                {
                    if (r <= 0.33f)
                    {
                        StartCoroutine(RotateClosestProp());
                    }
                    else if (r <= 0.66f)
                        BreakLight(0.25f);
                }
            }
        }
    }

    public void RotatePropFromShortCut()
    {
        StartCoroutine(RotateClosestProp());
    }
    
    IEnumerator RotateClosestProp()
    {
        float distance = 1000;
        PropController prop = null;
        for (int i = LevelGenerator.instance.propsInGame.Count - 1; i >= 0; i--)
        {
            var p = LevelGenerator.instance.propsInGame[i];
            
            if(p == null) continue;

            float newDistance = Vector3.Distance(pm.transform.position, p.transform.position);
            if (newDistance < distance && newDistance > 4)
            {
                distance = newDistance;
                prop = p;
            }
        }

        if (prop != null && !prop.wallProp && !prop.bigProp)
        {
            float t = 0;
            float tt = Random.Range(1, 5);
            float speed = Random.Range(1, 360);
            while (t < tt)
            {
                prop.transform.Rotate(Vector3.up, Time.deltaTime * speed);
                t += Time.deltaTime;
                yield return null;
            }
        }
        
        // maybe add here some gameplay stuff too?
    }

    public void BreakClosestWall(bool explode, float maxDistance)
    {
        gm = GameManager.instance;
        if (gm.hub) return;
        
        // find closest wall
        HealthController closestWall = null;
        float distance = maxDistance;
        for (int i = gm.units.Count - 1; i >= 0; i--)
        {
            var unit = gm.units[i];
            if (unit && unit.wallMasterTile != null && MouseLook.instance.PositionIsVisibleToPlayer(unit.transform.position + Vector3.up))
            {
                float newDist = Vector3.Distance(unit.transform.position, PlayerMovement.instance.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closestWall = unit;   
                }
            }
        }

        if (closestWall != null)
        {
            if (explode)
            {
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    GLNetworkWrapper.instance.CreateExplosion(closestWall.transform.position);
                else
                {
                    var newGrenade = Instantiate(ItemsList.instance.savedTools[0].toolController.grenadeExplosion, closestWall.transform.position, Quaternion.identity);
                    newGrenade.DestroyEffect(true);
                }
            }
            else
                closestWall.Kill();
        }
    }

    // this is called from CreateEvent ienumerator
    public void NewMeatTile(TileController tile)
    {
        if (Random.value > 0.75f)
        {
            CreateSound(tile.transform);
        }
        if (gm.player.health > gm.player.healthMax * 0.5f)
        {
            // if health is high
            if (Random.value >= 0.5f)
                SpawnPunishment(tile);
            
            if (Random.value > 0.9f)
                SpawnReward(tile);
        }
        else
        {
            // if health is low
            if (Random.value >= 0.5f)
                SpawnReward(tile);
            
            if (Random.value > 0.9f)
                SpawnPunishment(tile);
        }
    }

    void SpawnPunishment(TileController tile)
    {
        float r = Random.value;
        if (r < 0.25f)
        {
            SpawnController.instance.SpawnFloorBlade(tile);      
        }
        else if (r < 0.5f)
        {
            int rrrr = Random.Range(0, gm.level.mobsPool.Count);
            
            SpawnController.instance.SpawnMobByType(gm.level.mobsPool[rrrr], tile.transform);
        }
        else if (r < 0.75f)
        {
            tile.SpreadToolEffect(ToolController.ToolType.Fire, 6, Random.Range(3, 8), Random.Range(5, 20), null);
        }
        else
        {
            tile.SpreadToolEffect(ToolController.ToolType.GoldHunger, 6, Random.Range(3, 8), Random.Range(5, 20), null);
            /*
            var explosion = Instantiate(gm.player.wc.allTools[0].grenadeExplosion, tile.transform.position + Vector3.up * 3, Quaternion.identity);
            explosion.DestroyEffect(true);
            */
        }
    }

    void SpawnReward(TileController tile)
    {
        float r = Random.value;
        if (r <= 0.25)
            tile.SpreadToolEffect(ToolController.ToolType.Regen, 3, Random.Range(3, 10), Random.Range(1, 30), null);
        else if (r <= 0.33f) // WEAPONS
        {
            int weaponSpawnGroup = 2;
            int randomWeapon = Random.Range(0, gm.level.spawnGroups[weaponSpawnGroup].weapons.Length);
            SpawnController.instance.InstantiateItem(gm.level.spawnGroups[weaponSpawnGroup].weapons[randomWeapon].weapon.value, tile.transform.position + Vector3.up, Quaternion.Euler(Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)),true);
        }
        else if (r <= 0.66f)
        {
            // ITEMS
            for (int i = 0; i < Random.Range(1, 3); i++)
            {
                int randomSpawnGroup = 0;
                int randomItem = Random.Range(0, gm.level.spawnGroups[randomSpawnGroup].simpleItems.Length);
                SpawnController.instance.InstantiateItem(gm.level.spawnGroups[randomSpawnGroup].simpleItems[randomItem].item.value, tile.transform.position + Vector3.up, Quaternion.Euler(Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)),true);
            }
        }
        else // AMMO
        {
            for (int i = 0; i < Random.Range(3, 6); i++)
            {
                int random = Random.Range(0, gm.level.ammoSpawn.Length);

                var item = gm.level.ammoSpawn[random].value.bulletPack;
                if (item == null) item = gm.level.ammoSpawn[random].value.bullets;
                SpawnController.instance.InstantiateItem(item, tile.transform.position + Vector3.up, Quaternion.Euler(Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)),true);   
            }
        }
    }

    public void BreakLight(float randomTreshold)
    {
        if (lights.Count > 0)
        {
            float distance = 1000;
            LightController newLightToBreak = null;

            bool destroyMultipleLights = Random.value >= randomTreshold;

            for (int i = lights.Count - 1; i >= 0; i--)
            {
                if (lights[i].gameObject.activeInHierarchy && lights[i].working)
                {
                    float newDist = Vector3.Distance(pm.transform.position, lights[i].transform.position);
                    if (destroyMultipleLights && newDist < 50)
                    {
                        newLightToBreak = lights[i];
                        if (newLightToBreak != null)
                            newLightToBreak.LightOff();

                        lights.Remove(newLightToBreak);
                    }
                    else if (newDist <= distance)
                    {
                        distance = newDist;
                        newLightToBreak = lights[i];
                    }
                }
            }

            if (!destroyMultipleLights)
            {
                if (newLightToBreak != null)
                    newLightToBreak.LightOff();

                lights.Remove(newLightToBreak);   
            }
        }
    }

    bool SpawnSoundSource()
    {
        int r = Random.Range(0, soundSources.Count);
        Transform newTransform = null;
        bool found = false;

        foreach (Interactable i in il.interactables)
        {
            if (i.weaponPickUp || i.pickUp)
            {
                float newDist = Vector3.Distance(i.transform.position, pm.transform.position);

                if (newDist <= soundSourceMaxDistance && newDist >= soundSourceMinDistance)
                {
                    //found place
                    newTransform = i.transform;
                    found = true;
                    break;
                }
            }
        }
        if (found)
        {
            CreateSound(newTransform);
        }

        return found;
    }

    bool MobSoundSource()
    {
        Transform newTransform = null;
        bool found = false;

        foreach (HealthController i in GameManager.instance.units)
        {
            if (i.mobGroundMovement || i.mobHideInCorners || i.mobJumperMovement)
            {
                float newDist = Vector3.Distance(i.transform.position, pm.transform.position);

                if (newDist <= soundSourceMaxDistance && newDist >= soundSourceMinDistance)
                {
                    //found place
                    newTransform = i.transform;
                    found = true;
                    break;
                }
            }
        }
        if (found)
        {
            CreateSound(newTransform);
        }
        return found;
    }

    void CreateSound(Transform newTransform)
    {
        List<AudioSource> tempList = new List<AudioSource>(soundSources);
        for (int i = 0; i <= UnityEngine.Random.Range(0, 3); i++)
        {
            int r = Random.Range(0, tempList.Count);
            AudioSource newSoundSource = Instantiate(tempList[r], newTransform.position + Vector3.up, Quaternion.identity);
            newSoundSource.pitch = Random.Range(0.5f, 1.5f);
            newSoundSource.transform.parent = newTransform;
            if (Random.value > 0.5f)
                newSoundSource.loop = false;

            newSoundSource.Play();
            Destroy(newSoundSource.gameObject, Random.Range(10f, 30f));
            activeSources.Add(newSoundSource);
            tempList.RemoveAt(r);
        }
    }
}
