using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DynamicObstaclesZone : MonoBehaviour
{
    public List<AssetReference> propsReferences;
    public List<AssetReference> enemiesReferences;

    [Range(0, 1)] public float chanceOfSpawningInPlayerSight = 0;
    public int propsSpawnedMax = 20;
    public int enemiesSpawnedMax = 2;
    
    public bool removePropOnEveryStep = false;
    
    public float handlePropsTimeMin = 1;
    public float handlePropsTimeMax = 5;
    
    public float handleEnemiesTimeMin = 1;
    public float handleEnemiesTimeMax = 30;
    public float sphereSpawnRadiusMin = 10;
    public float sphereSpawnRadiusMax = 30;

    public float destroyTimeMin = 1;
    public float destroyTimeMax = 3;
    public Color skyColor;
    public Color fogColor;
    public Color mainLightColor;
}
