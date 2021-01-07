using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CitySpawner : MonoBehaviour
{
    public List<HealthController> monstersToSpawn = new List<HealthController>();
    public List<HealthController> npcsToSpawn = new List<HealthController>();
    public GameObject gameObjectToSpawn;
    public Interactable itemToSpawn;
}
