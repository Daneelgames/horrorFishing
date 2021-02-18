using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PropController : MonoBehaviour
{
    public bool bigProp = false;
    public bool wallProp = false;
    public HealthController wallHc;
    public List<Spawner> spawners;
    public TileController usedTile;

    private LevelGenerator lg;
    private SpawnController sc;
    public AudioSource swapAu;
    public NpcController npc;

    public ParticleSystem swapParticles;
    public Interactable spawnedObject;
    private AsyncOperationHandle<GameObject> _asyncOperationHandle;
    public HumanPropBonesRandomizer humanPropBonesRandomizer;

    void Start()
    {
        lg = LevelGenerator.instance;
        sc = SpawnController.instance;
    }
    
    public void SwapRandomProp()
    {
        if (wallProp) return;

        print("SWAP RANDOM PROP");
        
        foreach (Spawner spawner in spawners)
        {
            sc.spawnersOnProps.Remove(spawner);
        }

        AssetReference newPropReference = lg.propsReferences[Random.Range(0, lg.propsReferences.Count)];
        PropController newProp = null;
        
        _asyncOperationHandle = newPropReference.LoadAssetAsync<GameObject>();
        _asyncOperationHandle.Completed += handle =>
        {
            var prefab = handle.Result;
            var newGameObject = Instantiate(prefab);
            newProp = newGameObject.GetComponent<PropController>();
            
            newProp.swapAu.pitch = Random.Range(0.2f, 0.75f);
            newProp.swapAu.Play();
            newProp.transform.position = transform.position;
            newProp.transform.rotation = transform.rotation;
            newProp.usedTile = usedTile;
        
            newProp.swapParticles.transform.parent = null;
            newProp.swapParticles.Play();
        
            lg.propsInGame.Add(newProp);
            usedTile.spawner.spawnedProp = newProp;
            usedTile.propOnTile = newProp;

            lg.propsInGame.Remove(this);
            if (npc)
            {
                ItemsList.instance.interactables.Remove(npc.interactable);
            }

            if (newProp.wallProp || newProp.bigProp)
            {
                Destroy(newProp.gameObject);
                if (Random.value < 0.25f)
                    sc.SpawnRandomMobInsteadOfProp(transform.position);
            }
            else
                Destroy(gameObject);
        };
    }

    void OnDestroy()
    {
        lg = LevelGenerator.instance;
        if (lg != null && lg.propsInGame.Contains(this))
            lg.propsInGame.Remove(this);

        if (spawnedObject)
        {
            if (spawnedObject.weaponPickUp)
            {
                print("RELEASE WEAPON");
                spawnedObject.ReleaseItemWithExplosion();   
                spawnedObject = null;   
            }
            else
            {
                print("DESTROY PICKUP NAMED " + spawnedObject.gameObject.name);
                Destroy(spawnedObject.gameObject);
            }   
        }
    }
    
    /*
    [ContextMenu("FindSpawners")]
    public void FindSpawners()
    {
        spawners.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var spawner = transform.GetChild(0).GetComponent<Spawner>();
            if (spawner)
                spawners.Add(spawner);
        }
    }*/
}