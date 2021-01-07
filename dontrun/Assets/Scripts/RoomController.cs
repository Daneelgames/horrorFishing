using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RoomController : MonoBehaviour
{
    public List<TileController> tiles;
    public List<PropController> roomProps = new List<PropController>();
    public bool proceduralMap = true;
    /*
    public HealthController mapPrefab;
    public Transform mapTransform;
    */

    LevelGenerator lg;
    public List<RoomFiller> fillers;
    public List<int> connectedCoridorIndexes = new List<int>();
    public CultGenerator cultGenerator;

    private void Start()
    {
        lg = LevelGenerator.instance;
        lg.AddRoom(this);

        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.AddRoom();
        }
    }

    public void CreateMap()
    {
        CreateMapOnClient();
        
        /*
        if (LevelGenerator.instance.levelgenOnHost)
        {
            GLNetworkWrapper.instance.CreateMap(LevelGenerator.instance.roomsInGame.IndexOf(this));
        }
        else
        {
            CreateMapOnClient();
        }
        */
    }

    public void CreateMapOnClient()
    {
        if (proceduralMap)
        {
            int map = -1;
            for (var index = 0; index < tiles.Count; index++)
            {
                TileController t = tiles[index];
                map = t.CreateMap();

                if (map >= 0)
                {
                    HealthController newHc = null;
                    
                    if (lg.levelgenOnHost)
                    {
                        newHc = Instantiate(t.mapPrefab, t.transform.position, Quaternion.identity);

                        newHc.transform.parent = t.map.transform;
                        newHc.transform.localPosition = t.mapObjectLocalPos;
                        newHc.transform.localRotation = t.mapObjectLocalRot;
                        newHc.transform.parent = null;

                        GLNetworkWrapper.instance.localPlayer.SpawnObjectOnServer(newHc.gameObject);
                    }
                    else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                    {
                        newHc = Instantiate(t.mapPrefab, t.transform.position, Quaternion.identity);
   
                        newHc.transform.parent = t.map.transform;
                        newHc.transform.localPosition = t.mapObjectLocalPos;
                        newHc.transform.localRotation = t.mapObjectLocalRot;
                        newHc.transform.parent = null;
                    }
                    break;
                }
            }
        }
        /*
        else if (mapPrefab != null)
        {
            if (lg.levelgenOnHost)
            {
                var newHc = Instantiate(mapPrefab, mapTransform.position, mapTransform.rotation);
                GLNetworkWrapper.instance.localPlayer.SpawnObjectOnServer(newHc.gameObject);
            }
            else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                Instantiate(mapPrefab, mapTransform.position, mapTransform.rotation);
        }   */
    }
}