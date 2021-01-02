using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEditor;
using UnityEngine;

public class HubStreamer : MonoBehaviour
{
    public List<Transform> zones = new List<Transform>();
    public List<GameObject> objectsToPackToPrefabs = new List<GameObject>();
    public List<GameObject> prefabSources = new List<GameObject>();
    
    [Header("ZonesControllers")]
    public List<HubStreamZone> zoneControllers = new List<HubStreamZone>();
    [Header("Prefabs")]
    public List<GameObject> zonesStreamingPrefabs = new List<GameObject>();

    public HubStreamZone activeZone;

    private PlayerMovement pm;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        pm = PlayerMovement.instance;
        StartCoroutine(StartStreaming());
    }

    IEnumerator StartStreaming()
    {
        while (gameObject.activeInHierarchy)
        {
            float distance = 1000;
            HubStreamZone tempClosestZone = null; 
            for (int i = 0; i < zoneControllers.Count; i++)
            {
                float newDist = Vector3.Distance(pm.transform.position, zoneControllers[i].transform.position);
                if (newDist <= distance)
                {
                    distance = newDist;
                    tempClosestZone = zoneControllers[i];
                }
            }

            if (tempClosestZone != null && tempClosestZone != activeZone)
            {
                StartCoroutine(tempClosestZone.UpdateNeighbours(activeZone));
                activeZone = tempClosestZone;
            }
            yield return new WaitForSeconds(1);
        }
    }

    /*
    #region Editor
    [ContextMenu("MoveStreamingObjectsUnderZones")]
    void MoveStreamingObjectsUnderZones()
    {
        for (int i = 0; i < objectsToPackToPrefabs.Count; i++)
        {
            objectsToPackToPrefabs[i].transform.parent = null;
            float distance = 1000;
            Transform closestZone = null;

            for (int j = 0; j < zones.Count; j++)
            {
                float newDistance =
                    Vector3.Distance(objectsToPackToPrefabs[i].transform.position, zones[j].transform.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closestZone = zones[j];
                }
            }

            objectsToPackToPrefabs[i].transform.parent = closestZone;
        }
        
        // all streaming objects proceeded
        for (int i = 0; i < zones.Count; i++)
        {
            GameObject zonePrefabSource = new GameObject();
            zonePrefabSource.transform.parent = zones[i];
            zonePrefabSource.transform.localPosition = Vector3.zero;
            zonePrefabSource.transform.localRotation = Quaternion.identity;

            zonePrefabSource.name = zones[i].name;
            prefabSources.Add(zonePrefabSource);
        }
    }

    [ContextMenu("MoveObjectsToPrefabsParents")]
    void MoveObjectsToPrefabsParents()
    {
        for (int i = 0; i < zoneControllers.Count; i++)
        {
            Transform parentPrefab = zoneControllers[i].transform.GetChild(zoneControllers[i].transform.childCount - 1);
            foreach (Transform child in zoneControllers[i].transform)
            {
                if (child != parentPrefab)
                    child.transform.parent = parentPrefab;
            }
        }
    }

    [ContextMenu("GetPrefabsSources")]
    void GetPrefabsSources()
    {
        prefabSources.Clear();
        
        for (int i = 0; i < zones.Count; i++)
        {
            prefabSources.Add(zones[i].GetChild(0).gameObject);
        }
    }

    [ContextMenu("RandomizeShrooms")]
    void RandomizeShrooms()
    {
        for (int i = 0; i < prefabSources.Count; i++)
        {
            foreach (Transform child in prefabSources[i].transform)
            {
                child.GetChild(0).GetComponent<ActivateRandomObject>().RandomizeObjectsInEditor();
            }
        }
    }
    
    [ContextMenu("SavePrefabs")]
    void SavePrefabs()
    {
        zonesStreamingPrefabs.Clear();
        
        for (int i = 0; i < prefabSources.Count; i++)
        {
            prefabSources[i].name.Replace("(Clone)", "");
            string localPath = "Assets/GLPrefabs/Temporary/" + prefabSources[i].name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prefabSources[i], localPath);
            zoneControllers[i].streamingPrefab = prefab;
        }
    }

    [ContextMenu("GetPrefabsFromZones")]
    void GetPrefabsFromZones()
    {
        zonesStreamingPrefabs.Clear();
        for (int i = 0; i < zoneControllers.Count; i++)
        {
            zonesStreamingPrefabs.Add(zoneControllers[i].streamingPrefab);
        }
    }

    [ContextMenu("DeleteAllPrefabSources")]
    void DeleteAllPrefabSources()
    {
        if (prefabSources.Count > 0)
        {
            for (int i = prefabSources.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(prefabSources[i].gameObject);
            }   
        }
        
        prefabSources.Clear();
    }
    
    [ContextMenu("InstantiateAllPrefabs")]
    void InstantiateAllPrefabs()
    {
        for (int i = 0; i < zoneControllers.Count; i++)
        {
         //   var newPrefab = Instantiate(zoneControllers[i].streamingPrefab, zoneControllers[i].transform.position, zoneControllers[i].transform.rotation);
            var newObject = PrefabUtility.InstantiatePrefab(zoneControllers[i].streamingPrefab);
            var newPrefab = newObject as GameObject;
            newPrefab.transform.position = zoneControllers[i].transform.position;
            newPrefab.transform.rotation = zoneControllers[i].transform.rotation;
            newPrefab.transform.parent = zoneControllers[i].transform;
            prefabSources.Add(newPrefab);
        }
    }
    
    [ContextMenu("GetZonesControllers")]
    void GetZonesControllers()
    {
        zoneControllers.Clear();

        for (int i = 0; i < zones.Count; i++)
        {
            zoneControllers.Add(zones[i].GetComponent<HubStreamZone>());
            zoneControllers[i].streamingPrefab = zonesStreamingPrefabs[i];
        }
    }
    [ContextMenu("GetZonesNeighbours")]
    void GetZonesNeighbours()
    {
        for (int i = 0; i < zoneControllers.Count; i++)
        {
            zoneControllers[i].GetNeighbours(this);
        }
    }
    #endregion
    */
}