using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool propSpawner = false;
    public bool corridor = false;
    public PropController spawnedProp;
    void Start()
    {
        if (!propSpawner)
            SpawnController.instance.spawners.Add(this);
        else
            SpawnController.instance.spawnersOnProps.Add(this);
    }
}