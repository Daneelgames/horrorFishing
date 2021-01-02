using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawnerInRoom : MonoBehaviour
{
    public  MobPartsController.Mob mobType = MobPartsController.Mob.MrWindow;

    void Awake()
    {
        SpawnController.instance.mobSpawnersInRooms.Add(this);
    }
}