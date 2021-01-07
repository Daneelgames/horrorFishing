using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubStreamZone : MonoBehaviour
{
    public GameObject streamingPrefab;

    public List<HubStreamZone> neighbourZones = new List<HubStreamZone>();
    public GameObject instantiatedPrefab;

    public IEnumerator UpdateNeighbours(HubStreamZone previousZone)
    {
        // this zone is now active
        InstantiatePrefab();
        
        yield return null;
        
        for (int i = 0; i < neighbourZones.Count; i++)
        {
            neighbourZones[i].InstantiatePrefab();
            yield return null;
        }
        
        // deactivate prev zone neighbours if they're not neighbours of this one
        if (previousZone != null)
        {
            for (int i = previousZone.neighbourZones.Count - 1; i >= 0; i--)
            {
                if (previousZone.neighbourZones[i] != this && !neighbourZones.Contains(previousZone.neighbourZones[i]))
                    previousZone.neighbourZones[i].DestroyInstantiatedPrefab();
                yield return null;
            }
        }
    }

    public void InstantiatePrefab()
    {
        if (instantiatedPrefab == null)
        {
            instantiatedPrefab = Instantiate(streamingPrefab,transform.position, transform.rotation);
            instantiatedPrefab.transform.parent = transform;
            /*
            instantiatedPrefab.transform.localPosition = Vector3.zero;
            instantiatedPrefab.transform.localRotation = Quaternion.identity;   
            */
        }
    }

    public void DestroyInstantiatedPrefab()
    {
        if (instantiatedPrefab != null)
            Destroy(instantiatedPrefab);
    }
    

    public void GetNeighbours(HubStreamer hubStreamer)
    {
        // get 3-8 neighbors
        
        // target distance is 275
        neighbourZones.Clear();

        for (int i = 0; i < hubStreamer.zoneControllers.Count; i++)
        {
            if (hubStreamer.zoneControllers[i] == this) continue;
            
            if (Vector3.Distance(transform.position, hubStreamer.zoneControllers[i].transform.position) <= 275)
            {
                neighbourZones.Add(hubStreamer.zoneControllers[i]);
            }
        }
    }


    #region Editor
    [ContextMenu("ToggleNeighboursActive")]
    void ToggleNeighboursActive()
    {
        for (int i = 0; i < neighbourZones.Count; i++)
        {
            neighbourZones[i].gameObject.SetActive(!neighbourZones[i].gameObject.activeInHierarchy);
        }
    }
    #endregion
    
}