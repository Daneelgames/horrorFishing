using System;
using ExtendedItemSpawn;
using UnityEngine;
[Serializable]
public class AmmoSpawnInfo
{
    public AmmoSpawnContainer value;
    
    public bool reduceByPlayerOwnedAmmo;
    public int minCount;
    public int maxCount;
    
    [Header("Spawn conditions (at least one of checked must be true)")]
    public bool weaponSpawned;
    public bool playerHaveWeapon;
}

[Serializable]
public class WeaponSpawnInfo 
{
    public InteractableContainer weapon;

    public int minCount;
    public int maxCount;
    
    [Header("Spawn conditions")]
    public WeaponInHandsRule playerHaveThis;
    public int spawnWeight;
}

public enum WeaponInHandsRule
{
    NotCheckThis,
    Yes,
    No,
}

[Serializable]
public class SimpleItemSpawnInfo 
{
    public InteractableContainer item;

    public int minCount = 1;
    public int maxCount = 1;
    public int spawnWeight = 1;
}