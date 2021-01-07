using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUp : MonoBehaviour
{
    public enum Weapon {Null, Axe, Pistol, Revolver, Shotgun, TommyGun, Pipe, Map, Knife, Torch, OldPistol, Polaroid, Shield, MeatSpear, VeinWhip}
    public Weapon weapon = Weapon.Axe;
    public AudioClip pickUpClip;

    public WeaponDataRandomizer weaponDataRandomier;
    
    [HideInInspector]
    public bool useBaseWeaponDurability = true;
    public float durability = 1600;
    public float ammoClipMax = 0;
    
    [HideInInspector]
    public bool hasFullAmmo = true;
    public float ammo = 0;
    
    public float minimumSpawnDistance = 50;
    public float maximumSpawnDistance = 75;
    public Interactable interactable;
    public WeaponConnector weaponConnector;

    [Header("Interact with npc through this")]
    public NpcController npc;

    void Start()
    {
        if (weaponConnector)
            weaponConnector.weaponPickUp = this;
        
        weaponDataRandomier.pickUp = this;

        
        ItemsList.instance.weaponsOnLevel.Add(this);
    }
}
