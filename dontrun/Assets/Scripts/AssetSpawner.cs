using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetSpawner : MonoBehaviour
{
    public enum ObjectType
    {
        Room, Prop, Mob
    }

    public static AssetSpawner instance;
    
    private readonly Dictionary<AssetReference, List<GameObject>> spawnedAssets = 
        new Dictionary<AssetReference, List<GameObject>>();

    private readonly Dictionary<AssetReference, Queue<Vector3>> queuedSpawnRequests = 
        new Dictionary<AssetReference, Queue<Vector3>>(); 
    private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> asyncOperationHandles = 
        new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

    void Awake()
    {
        instance = this;
    }
    
    public void Spawn(AssetReference assetReference, Vector3 newPos, ObjectType objType)
    {
        if (assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.Log("Invalid Key " + assetReference.RuntimeKey.ToString());
        }
        if (asyncOperationHandles.ContainsKey(assetReference)) // if exists
        {
            if (asyncOperationHandles[assetReference].IsDone) // if exists and loaded
            {
                SpawnFromLoadedReference(assetReference, newPos, objType);
            }
            else // if exists and not loaded
                EnqueueSpawnForAfterInitialization(assetReference, newPos, objType);
                
            return;
        }
        
        // if not exists
         LoadAndSpawn(assetReference, newPos, objType);
    }

    void SpawnFromLoadedReference(AssetReference assetReference, Vector3 newPos, ObjectType objectType)
    {
        assetReference.InstantiateAsync(newPos, Quaternion.identity).Completed 
            += (asyncOperationHandle) =>
        {
            if (spawnedAssets.ContainsKey(assetReference) == false)
            {
                spawnedAssets[assetReference] = new List<GameObject>();
            }
            
            spawnedAssets[assetReference].Add(asyncOperationHandle.Result);

            if (objectType == ObjectType.Prop)
            {
                ProceedProp(asyncOperationHandle.Result);
            }
            else if (objectType == ObjectType.Mob)
            {
                ProceedMob(asyncOperationHandle.Result);
            }
            else if (objectType == ObjectType.Room)
            {
                if (GLNetworkWrapper.instance.coopIsActive)
                {
                    // spawns only on host or solo
                    NetworkServer.Spawn(asyncOperationHandle.Result);
                    
                    print("[COOP] Room spawned on server");
                }
            }
            
            var notify = asyncOperationHandle.Result.AddComponent<NotifyOnDestroy>();
            notify.Destroyed += Remove;
            notify.AssetReference = assetReference;
        };
    }

    void ProceedProp(GameObject go)
    {
        Vector3 finalPosition = go.transform.position;
        go.transform.position += Vector3.up * 50;
        
        var hits = Physics.RaycastAll(finalPosition + Vector3.up, Vector3.down, 500);
        for (var index = 0; index < hits.Length; index++)
        {
            var hit = hits[index];

            if (hit.collider.gameObject.layer != 23 && hit.collider.gameObject.layer != 28 &&
                hit.collider.gameObject.layer != 29 && hit.collider.gameObject.layer != 30 &&
                hit.collider.gameObject.layer != 25) // floor
                continue;

            print("Raycasted to GO " + hit.collider.gameObject.name + ". Layer " + hit.collider.gameObject.layer + ". Hit normal: " + hit.normal);

            var normalRotation = Quaternion.LookRotation(hit.normal);
            go.transform.rotation = normalRotation;
            go.transform.Rotate(90, 0, 0);
            go.transform.Rotate(0, Random.Range(1, 359f), 0);
            break;
        }

        go.transform.position = finalPosition;

        /*
        float angle = Random.Range(0, 360);
        go.transform.Rotate(Vector3.up, angle, Space.Self);
        */

        PropController newProp = go.GetComponent<PropController>();
        LevelGenerator.instance.propsInGame.Add(newProp);

        return;
        var lg = LevelGenerator.instance;

        for (var index = 0; index < lg.levelTilesInGame.Count; index++)
        {
            var tile = lg.levelTilesInGame[index];
            if (Vector3.Distance(tile.transform.position, go.transform.position) < lg.tileSize)
            {
                tile.SetPropToTile(go);
                break;
            }
        }
    }

    void ProceedMob(GameObject go)
    {
        var sc = SpawnController.instance;
        sc.ProceedMob(go);
    }

    void EnqueueSpawnForAfterInitialization(AssetReference assetReference, Vector3 newPos, ObjectType objectType)
    {
        if (queuedSpawnRequests.ContainsKey(assetReference) == false)
            queuedSpawnRequests[assetReference] = new Queue<Vector3>();
        queuedSpawnRequests[assetReference].Enqueue(newPos);
    }

    void LoadAndSpawn(AssetReference assetReference, Vector3 newPos, ObjectType objectType)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
        asyncOperationHandles[assetReference] = op;
        op.Completed += (operation) =>
        {
            SpawnFromLoadedReference(assetReference, newPos, objectType);
            if (queuedSpawnRequests.ContainsKey(assetReference))
            {
                while (queuedSpawnRequests[assetReference]?.Any() == true)
                {
                    var position = queuedSpawnRequests[assetReference].Dequeue();
                    SpawnFromLoadedReference(assetReference, position, objectType);
                }
            }
        };
    }

    void Remove(AssetReference assetReference, NotifyOnDestroy obj)
    {
        Addressables.ReleaseInstance(obj.gameObject);

        spawnedAssets[assetReference].Remove(obj.gameObject);
        if (spawnedAssets[assetReference].Count == 0)
        {
           // Debug.Log($"Removed all{assetReference.RuntimeKey.ToString()}");
            
            if (asyncOperationHandles.Count > 0 && asyncOperationHandles[assetReference].IsValid())
                Addressables.Release(asyncOperationHandles[assetReference]);

            asyncOperationHandles.Remove(assetReference);
        }
    }
}