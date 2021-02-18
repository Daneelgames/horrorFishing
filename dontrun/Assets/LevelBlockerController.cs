using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelBlockerController : MonoBehaviour
{
    public GameObject deathParticle;
    public List<GameObject> blockers = new List<GameObject>();
    public List<Transform> spawners = new List<Transform>();

    public GameObject eyePrefab;

    /*
    [ContextMenu("SpawnEye")]
    public void SpawnEye()
    {
        foreach (var b in blockers)
        {
            var newObject = PrefabUtility.InstantiatePrefab(eyePrefab);
            var newPrefab = newObject as GameObject;
            newPrefab.transform.position = b.transform.position;
            newPrefab.transform.rotation = b.transform.rotation;
            newPrefab.transform.parent = b.transform;
            newPrefab.transform.localScale = Vector3.one;
            newPrefab.transform.parent = transform;
        }
    }
    [ContextMenu("RotateEyesRandomly")]
    public void RotateEyesRandomly()
    {
        for (var index = 0; index < blockers.Count; index++)
        {
            blockers[index].transform.GetChild(0).Rotate(Random.Range(0f,360f),Random.Range(0f,360f),Random.Range(0f,360f));
        }
    }*/
    
    void Awake()
    {
        var sc = SpawnController.instance;
        
        for (int i = 0; i < spawners.Count; i++)
        {
            if (!sc.spawners.Contains(spawners[i]))
                sc.spawners.Add(spawners[i]);
        }
    }
    
    public void DestroyBlockers()
    {
        for (int i = 0; i < blockers.Count; i++)
        {
            StartCoroutine(DestroyBlocker(i));
        }
        StartCoroutine(WaitUntilBlockersDestroyed());
    }

    IEnumerator DestroyBlocker(int i)
    {
        var blocker = blockers[i];
        
        float t = 0;
        float tt = Random.Range(1f,5f);
        
        yield return new WaitForSeconds(tt);
        
        if (!blocker)
            yield break;
        
        var startScale = blocker.transform.localScale; 
        while (t < tt)
        {
            if (!blocker)
                yield break;
            
            blocker.transform.localScale = startScale + new Vector3(Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
            t += Time.deltaTime;
            yield return null;
        }

        Instantiate(deathParticle, blocker.transform.position, quaternion.identity);
        Destroy(blocker);
    }
    
    IEnumerator WaitUntilBlockersDestroyed()
    {
        while (blockers.Count > 0)
        {
            yield return null;
            for (int i = blockers.Count - 1; i >= 0; i--)
            {
                if (blockers[i] == null)
                    blockers.RemoveAt(i);
                yield return null;
            }
        }

        var sc = SpawnController.instance;
        for (int i = spawners.Count - 1; i >= 0; i--)
        {
            if (spawners[i] != null)
            {
                sc.spawners.Remove(spawners[i]);
                Destroy(spawners[i].gameObject);   
            }
        }

        HubItemsSpawner.instance.RespawnItems();
        Destroy(gameObject);
    }
}
