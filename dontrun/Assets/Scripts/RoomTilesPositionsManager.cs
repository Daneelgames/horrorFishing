using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RoomTilesPositionsManager : MonoBehaviour
{
    public bool soloOnly = false;
    public RoomController roomToGetReferences;
    public AssetReference roomReference;
    
    public List<Vector3> tilesLocalPositions;

    [ContextMenu("GetPositions")]
    void GetPositions()
    {
        tilesLocalPositions.Clear();
        
        if (!roomToGetReferences) return;

        foreach (var f in roomToGetReferences.fillers)
        {
            tilesLocalPositions.Add(f.transform.localPosition);
        }
        roomToGetReferences = null;
    }
}
