using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Mirror;
using PlayerControls;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore;
using Random = UnityEngine.Random;

public class MobPartsController : MonoBehaviour
{
    public int level = 0;
    public enum Mob {Walker, Jumper, Witch, Mimic, MimicBig, Castle, BigTurret, Moocher, MimicForest, MrPole, ItemMimic, MimicCity, MrWindow, EyeTaker, FactoryMimic, FactoryMimicBig, FaceEater} 
    public Mob mobType = Mob.Walker;

    public NavMeshAgent agent;

    [Tooltip("Sets Chase anim only when object is moving")]
    public IkMonsterAnimator ikMonsterAnimator;
    public bool simpleWalker = false;
    public string simpleWalkerString = "Chase";
    public float distanceThreshold = 0.1f;

    private Vector3 prevPos;
    private Vector3 newPos;
    
    public Animator anim;
    public List<MobBodyPart> bodyParts = new List<MobBodyPart>();
    public int bodyPartMax = 0;
    Vector3 bodyPartLastPosition;
    public HealthController hc;
    public GameObject bloodFx;
    public List<Drop> drop;
    public Transform dropPosition;
    public bool forceDropOnTile = false; 

    private bool dead = false;
    
    GameManager gm;
    private ItemsList il;

    public bool animDamageByTrigger = false;
    private string peacefulBool = "Peaceful";
    private void Start()
    {
        il = ItemsList.instance;
        
        if (hc.peaceful && anim && anim.GetBool(peacefulBool) == false)
            anim.SetBool(peacefulBool, true);
        
        bodyPartMax = bodyParts.Count;

        if (hc.mobGroundMovement)
            hc.mobGroundMovement.mobParts = this;

        gm = GameManager.instance;

        for (var index = 0; index < bodyParts.Count; index++)
        {
            if (bodyParts[index] != null)
                bodyParts[index].hc = hc;
        }

        if (simpleWalker)
            StartCoroutine(SimpleWalkerAnimationsDetector());
    }

    IEnumerator SimpleWalkerAnimationsDetector()
    {
        while (hc.health > 0)
        {
            prevPos = transform.position;
            yield return new WaitForSeconds(0.1f);
            newPos = transform.position;

            if (Vector3.Distance(prevPos, newPos) > distanceThreshold)
            {
                if (ikMonsterAnimator && ikMonsterAnimator.animate == false)
                {
                    ikMonsterAnimator.SetAnimate(true);
                    //ikMonsterAnimator.ToggleAggressiveMeshes(true);
                }
                else if (anim)
                    anim.SetBool(simpleWalkerString, true);
            }
            else
            {
                if (ikMonsterAnimator && ikMonsterAnimator.animate)
                {
                    ikMonsterAnimator.SetAnimate(false);
                    //ikMonsterAnimator.ToggleAggressiveMeshes(false);
                }
                else if (anim)
                    anim.SetBool(simpleWalkerString, false);
            }
        }
    }

    public void Death()
    {
        print("FUCK");
        if (!dead)
        {
            dead = true;
            
            if (anim)
                anim.gameObject.SetActive(false);
            for (var index = bodyParts.Count - 1; index >= 0 ; index--)
            {
                MobBodyPart part = bodyParts[index];
                if (part)
                    part.gameObject.SetActive(false);
                else
                {
                    bodyParts.Remove(part);
                }
            }

            print("FUCK111");
            if (dropPosition)
            {
                print("FUCK222");
                DropLoot();
            }
        }
    }

    void DropLoot()
    {
        // spawn only on server if online
        gm = GameManager.instance;
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && !gm.lg.levelgenOnHost)
            return;

        print("FOR THE LOVE OF MOTHERFUCK CAN YOU PRINT HERE OR NOT BITCH");
        
        if (forceDropOnTile)
            dropPosition.transform.position = hc.usedTile.transform.position + Vector3.up * 2;

        if (!hc.boss)
        {
            // check what player needs in arena
            if (gm.arena && Random.value >= 0.75f)
            {
                // change drop to weapon drop
                Drop newDrop = new Drop();
                bool foundNewDrop = false;
                var tempSpawnGroup = gm.arenaLevel.spawnGroups[Random.Range(0, gm.arenaLevel.spawnGroups.Length)];
                if (tempSpawnGroup.weapons.Length > 0)
                {
                    newDrop.item = tempSpawnGroup.weapons[Random.Range(0, tempSpawnGroup.weapons.Length)].weapon.value;
                    foundNewDrop = true;
                }
                else if (tempSpawnGroup.simpleItems.Length > 0)
                {
                    newDrop.item = tempSpawnGroup.simpleItems[Random.Range(0, tempSpawnGroup.weapons.Length)].item.value;   
                    foundNewDrop = true;
                }

                if (foundNewDrop)
                {
                    drop.Clear();
                    drop = new List<Drop>();
                    newDrop.amountMax = 1;
                    newDrop.amountMin = 1;
                    drop.Add(newDrop);   
                }
            }
        }
        else
        {
            // boss died
            // change his drop to Her part
            drop.Clear();

            print("BOSS TRIES TO FUCK YOU");
            if ((GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false) && !il.foundHerPiecesOnFloors.Contains(GutProgressionManager.instance.playerFloor))
            {
                print("C# TRIES TO FUCK YOU");
                // if player still didnt find body part on this level
                // find good quest item for him

                var qm = QuestManager.instance;
                Drop newDrop = new Drop();

                if (!qm.activeQuestsIndexes.Contains(2))
                {
                    // give head
                    newDrop.item = il.herPartsPrefabs[0];
                    newDrop.amountMin = 1;
                    newDrop.amountMax = 1;
                    drop.Add(newDrop);
                }
                else if (!qm.activeQuestsIndexes.Contains(4))
                {
                    // give palm   
                    newDrop.item = il.herPartsPrefabs[1];
                    newDrop.amountMin = 1;
                    newDrop.amountMax = 1;
                    drop.Add(newDrop);
                }
                else if (!qm.activeQuestsIndexes.Contains(5))
                {
                    // give leg   
                    newDrop.item = il.herPartsPrefabs[2];
                    newDrop.amountMin = 1;
                    newDrop.amountMax = 1;
                    drop.Add(newDrop);
                }
                else if (!qm.activeQuestsIndexes.Contains(6))
                {
                    // give arm   
                    newDrop.item = il.herPartsPrefabs[3];
                    newDrop.amountMin = 1;
                    newDrop.amountMax = 1;
                    drop.Add(newDrop);
                }
                else if (!qm.activeQuestsIndexes.Contains(3))
                {
                    // give body
                    newDrop.item = il.herPartsPrefabs[4];
                    newDrop.amountMin = 1;
                    newDrop.amountMax = 1;
                    drop.Add(newDrop);   
                }
                else if (!qm.activeQuestsIndexes.Contains(10))
                {
                    // give heart   
                    newDrop.item = il.herPartsPrefabs[5];
                    newDrop.amountMin = 1;
                    newDrop.amountMax = 1;
                    drop.Add(newDrop);
                }

                print("DROP COUNT " + drop.Count);
            }
            else
            {
                //d dont drop quest item if player already has it
                // activate the elevator instead
                        
                var elevator = ElevatorController.instance; 
                if (elevator)
                    elevator.Activate();
                        
                UiManager.instance.TimeToLeaveFloor();
            }
        }
        
        foreach (Drop d in drop)
        {
            if (d.item.pickUp && d.item.pickUp.tool)
            {
                bool maximumAmount = false;
                foreach (var tool in gm.itemList.savedTools)
                {
                    if (tool.toolController.type == d.item.pickUp.tool.type && tool.amount >= tool.maxAmount)
                    {
                        maximumAmount = true;
                        break;
                    }
                }

                if (!maximumAmount)
                {
                    int random = Random.Range(d.amountMin, d.amountMax);

                    for (int i = 0; i < random; i++)
                    {
                        MobSpawnDrop(d);
                    }
                }
            }
            else if (d.item.ammoPickUp)
            {
                if (Random.value > 0.75f)
                    continue;
                
                int ammoAmount = il.ammoDataStorage.GetAmmoCount(d.item.ammoPickUp.weaponType);
                bool canSpawn = true;
                switch (d.item.ammoPickUp.weaponType)
                {
                    case WeaponPickUp.Weapon.Pistol:
                        if (gm.player.wc.weaponList[1])
                        {
                            if (ammoAmount > gm.player.wc.weaponList[1].ammoClipMax)
                                canSpawn = false;
                        }

                        break;

                    case WeaponPickUp.Weapon.Revolver:
                        if (gm.player.wc.weaponList[2])
                        {
                            if (ammoAmount > gm.player.wc.weaponList[2].ammoClipMax)
                                canSpawn = false;
                        }

                        break;

                    case WeaponPickUp.Weapon.Shotgun:
                        if (gm.player.wc.weaponList[3])
                        {
                            if (ammoAmount > gm.player.wc.weaponList[3].ammoClipMax)
                                canSpawn = false;
                        }

                        break;

                    case WeaponPickUp.Weapon.TommyGun:
                        if (gm.player.wc.weaponList[4])
                        {
                            if (ammoAmount > gm.player.wc.weaponList[4].ammoClipMax)
                                canSpawn = false;
                        }

                        break;
                }

                if (canSpawn)
                {
                    int random = Random.Range(d.amountMin, d.amountMax);

                    for (int i = 0; i < random; i++)
                    {
                        MobSpawnDrop(d);
                    }
                }
            }
            else
            {
                if (d.item.pickUp && d.item.pickUp.resourceType == ItemsList.ResourceType.Gold)
                {
                    if (il.gold > 500 && Random.value > 0.25f)
                        continue;
                    
                    if (il.gold > 250 && Random.value > 0.5f)
                        continue;
                    
                    if (il.gold > 100 && Random.value > 0.66f)
                        continue;
                    
                    if (il.gold > 50 && Random.value > 0.75f)
                        continue;
                }
                
                int random = Random.Range(d.amountMin, d.amountMax);

                for (int i = 0; i < random; i++)
                {
                    if (d.item.pickUp && d.item.pickUp.resourceType == ItemsList.ResourceType.Key &&
                        hc.npcInteractor &&
                        hc.npcInteractor.keySold)
                    {
                        // dont drop key if player already bought one
                    }
                    else
                    {
                        MobSpawnDrop(d);
                    }
                }
            }
        }
    }

    public void MobSpawnDrop(Drop d)
    {
        print("TRY TO SPAWN STUFF");
        if ((GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false) // SOLO
            || (LevelGenerator.instance && LevelGenerator.instance.levelgenOnHost)) // OR HOST
        {
            Interactable newDrop = Instantiate(d.item, dropPosition.position, Quaternion.identity);
            
            print(newDrop + " IS SPAWNED!");
            
            if (newDrop.meatBrain)
            {
                var gnw = GLNetworkWrapper.instance;
                for (int i = 0; i < gnw.playerNetworkObjects.Count; i++)
                {
                    if (gnw.playerNetworkObjects[i].connectedDummy.hc != hc)
                        continue;
                    
                    newDrop.meatBrain.playerIndex = i;
                }
            }
            
            if (newDrop.weaponPickUp)
            {
                if (newDrop.weaponPickUp.weaponConnector)
                    newDrop.weaponPickUp.weaponConnector.GenerateOnSpawn();

                bool deadWeapon = Random.value <= 0.5f;
                bool npcWeapon = Random.value <= 0.5f;
                newDrop.weaponPickUp.weaponDataRandomier.GenerateOnSpawn(deadWeapon, npcWeapon); // spawn dead weapon
            }

            
            newDrop.rb.AddExplosionForce(300,
                dropPosition.position + Vector3.right * UnityEngine.Random.Range(-1, 1) +
                Vector3.forward * Random.Range(-1, 1), 100);
        
            if (LevelGenerator.instance && LevelGenerator.instance.levelgenOnHost)
            {
                var alivePlayer = GLNetworkWrapper.instance.GetAlivePlayerNetworkObject();
                
                if (alivePlayer && newDrop.meatBrain)
                    NetworkServer.Spawn(newDrop.gameObject, alivePlayer);
                else
                    NetworkServer.Spawn(newDrop.gameObject);
            }

            /*
            if (hc.playerNetworkDummyController && hc.playerNetworkDummyController.targetPlayer != null)
            {
                hc.playerNetworkDummyController.targetPlayer.transform.parent = newDrop.transform;
                hc.playerNetworkDummyController.targetPlayer.transform.localPosition = Vector3.zero;
                hc.playerNetworkDummyController.targetPlayer.pm.ToggleCrouch(true);
            }*/
        }
    }

    public void Resurrect()
    {
        dead = false;
            
        if (anim)
            anim.gameObject.SetActive(true);
        
        hc.monsterTrapTrigger.enabled = true;
        
        for (var index = bodyParts.Count - 1; index >= 0 ; index--)
        {
            MobBodyPart part = bodyParts[index];
            if (part)
                part.gameObject.SetActive(true);
            else
            {
                bodyParts.Remove(part);
            }
        }
    }

    public void Damaged(Vector3 bloodPosition, Vector3 damageOrigin)
    {
        if (bloodFx)
            SpawnBlood(bloodPosition, damageOrigin);

        if (anim)
        {
            anim.SetBool("Peaceful", false);
            if (!simpleWalker || simpleWalkerString != "Chase")
                anim.SetBool("Chase", true);   
            
            if (animDamageByTrigger)
                anim.SetTrigger("Damaged");
        }
    }

    public void SetSneak(bool sneak)
    {
        anim.SetBool("Chase", !sneak);
        if (sneak)
            hc.mobAudio.IdleAmbient();
        else
            hc.mobAudio.ChaseAmbient();
    }

    void SpawnBlood(Vector3 bloodPosition, Vector3 damageOrigin)
    {
        GameObject newBlood = Instantiate(bloodFx, bloodPosition, Quaternion.identity);
        /*
        if (GameManager.instance.bloodMist == 0)
            Destroy(newBlood.transform.GetChild(0).gameObject);*/
        newBlood.transform.LookAt(damageOrigin);
    }
    
    public void KillBodyPart(MobBodyPart part)
    {
        if (part.ikTargets.Count == 0)
            return;
        
        SpawnBlood(part.transform.position, PlayerMovement.instance.transform.position);
        
        part.transform.parent.localScale = Vector3.zero;
        ikMonsterAnimator.RemoveIkTarget(part.ikTargets);
        StartCoroutine(RestoreKilledBodyPart(part));
    }

    IEnumerator RestoreKilledBodyPart(MobBodyPart part)
    {
        yield return new WaitForSeconds(Random.Range(10f, 60f));
        
        if (!gameObject.activeInHierarchy || !gameObject)
            yield break;
        
        float t = 0;
        float tt = Random.Range(1, 3);
        
        while (t < tt)
        {
            part.transform.parent.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / tt);
            t += Time.deltaTime;
            yield return null;
        }
        
        ikMonsterAnimator.RestoreIkTarget(part.ikTargets);
    }
    
    [ContextMenu("Get body parts in children")]
    void GetMobParts()
    {
        bodyParts.Clear();
        var newParts =  GetComponentsInChildren<MobBodyPart>();
        if (newParts.Length > 0)
        {
            foreach (var p in newParts)
            {
                bodyParts.Add(p);
            }
        }
    }
    [ContextMenu("Set Melee attack to bodyParts")]
    void SetMeleeAttackToBodyParts()
    {
        if (!hc || !hc.mobMeleeAttack)
            return;
        
        foreach (var part in bodyParts)
        {
            part.mobAttack = hc.mobMeleeAttack;
        }
    }
    [ContextMenu("ToggleBodyPartsAsDangerous")]
    void ToggleBodyPartsAsDangerous()
    {
        foreach (var part in bodyParts)
        {
            part.usedForAttack = !part.usedForAttack;
        }
    }
    
}

[Serializable]
public class Drop
{
    public Interactable item;
    public int amountMax = 1;
    public int amountMin = 0;
}