using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using PlayerControls;
using Steamworks;
using UnityEngine;

public class MobDamageArea : MonoBehaviour
{
    public MobMeleeAttack mobAttack;
    public float distanceToBreakDoor = 4;
    bool dangerous = false;
    GameManager gm;
    public float distanceToplayerToBeAbleToAttack = 20;
    public string attackAnimString = "Attack";
    private bool closeToPlayer = false;
    Coroutine atackCoroutine;
    Quaternion newRot;
    public Collider coll;

    public Transform customTransformToFollow;
    
    private void Start()
    {
        coll = GetComponent<Collider>();
        gm = GameManager.instance;
        transform.parent = null;
        dangerous = true;
    }

    void OnEnable()
    {
        gameObject.layer = 12;
        gm = GameManager.instance;
        StartCoroutine(GetDistance());
    }

    private void OnDisable()
    {
        StopCoroutine(GetDistance());
    }
    
    IEnumerator GetDistance()
    {
        while (true)
        {
            if (mobAttack != null && mobAttack.gameObject.activeInHierarchy)
            {
                var distance = 1000f;

                if (gm.player)
                {
                    var playerPos = gm.player.transform.position;
                    
                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    {
                        playerPos = GLNetworkWrapper.instance.GetClosestPlayer(transform.position).transform.position;
                    }
                 
                    distance = Vector3.Distance(transform.position, playerPos);

                }
                
                if (distance <= distanceToplayerToBeAbleToAttack)
                {
                    closeToPlayer = true;   
                }
                else
                {
                    closeToPlayer = false;
                }   
            }
            else
                closeToPlayer = false;
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Update()
    {
        if (mobAttack == null)
            Destroy(gameObject);
        else if(!mobAttack.gameObject.activeInHierarchy)
            return;
        
        if (customTransformToFollow != null)
        {
            transform.position = customTransformToFollow.position;
            transform.rotation = customTransformToFollow.rotation;
        }
        else if (mobAttack != null)
        {
            transform.position = mobAttack.transform.position;
            newRot = mobAttack.transform.rotation;
            newRot.eulerAngles = new Vector3(0, mobAttack.transform.rotation.eulerAngles.y, 0);
            //transform.rotation = mobAttack.transform.rotation;
            transform.rotation = newRot;   
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (closeToPlayer == false || !other.gameObject.activeInHierarchy) return;
        if (!mobAttack || !mobAttack.hc || mobAttack.hc.peaceful) return;
        if (!mobAttack.gameObject.activeInHierarchy) return;
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && (other.gameObject == PlayerMovement.instance.gameObject || LevelGenerator.instance.levelgenOnHost == false)) return;


        switch (other.gameObject.layer)
        {
            // if hitting door
            case 18:
            {
                if (Vector3.Distance(transform.position, other.gameObject.transform.position) <= distanceToBreakDoor)
                {
                    if (other != null)
                    {
                        DoorController door = other.gameObject.GetComponent<DoorController>();
                        if (door)
                            door.DoorDestroyed();   
                    }
                }

                break;
            }
            // if hitting other monster (not his part)
            case 11 when mobAttack && other.gameObject != mobAttack.gameObject:
            {
                
                HealthController hc = other.gameObject.GetComponent<HealthController>();
                
                //print(hc);
                if (hc == null || hc == mobAttack.hc || (hc.player && mobAttack.hc.inLove) || (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && hc.pm))
                    return;
                
                Vector3 bloodSpawnPosition = hc.transform.position + Vector3.up;
                
                if (hc.mobPartsController && hc.mobPartsController.dropPosition)
                    bloodSpawnPosition = hc.mobPartsController.dropPosition.position;
                
                if (hc.damageCooldown <= 0 && hc.health > 0)
                {
                    if (mobAttack.mobGroundMovement || mobAttack.mobCharacterControllerMovement)
                    {
                        if (dangerous)
                        {
                            mobAttack.StopAttackAnimation();
                            if (mobAttack.hc.statusEffects[3].effectActive)// rust
                                hc.Damage(mobAttack.levels[Mathf.Clamp(mobAttack.mobParts.level, 0, mobAttack.levels.Count-1)].damage / 2, bloodSpawnPosition, transform.position + Vector3.up, null, mobAttack.hc.playerDamagedMessage[gm.language], false, mobAttack.hc.names[gm.language], mobAttack.hc, mobAttack.effectsOnAttack, false);
                            else
                                hc.Damage(mobAttack.levels[Mathf.Clamp(mobAttack.mobParts.level, 0, mobAttack.levels.Count-1)].damage, bloodSpawnPosition, transform.position + Vector3.up, null, mobAttack.hc.playerDamagedMessage[gm.language], false, mobAttack.hc.names[gm.language], mobAttack.hc, mobAttack.effectsOnAttack, false);

                            if (mobAttack.thiefBehaviour)
                            {
                                if (hc.player)
                                {
                                    ItemsList.instance.SomethingStolen();
                                }

                                var wc = WeaponControls.instance;
                                
                                if (wc && wc.activeWeapon == null && wc.secondWeapon == null && mobAttack.mobCharacterControllerMovement)
                                    mobAttack.mobCharacterControllerMovement.RunAway(15);
                                
                            }
                            
                            ToggleDangerous(false);
                            atackCoroutine = null;
                        }
                        else if (mobAttack.levels[Mathf.Clamp(mobAttack.mobParts.level, 0, mobAttack.levels.Count-1)].attackCooldown <= 0)
                        {
                            if (atackCoroutine == null)
                            {
                                if (hc.player && mobAttack.mobParts.hc.mobGroundMovement)
                                    mobAttack.mobParts.hc.mobGroundMovement.Damaged(hc);
                                
                                atackCoroutine = StartCoroutine(StartAttack());
                            }
                        }
                    }
                    else
                    {
                        if (mobAttack.levels[Mathf.Clamp(mobAttack.mobParts.level, 0, mobAttack.levels.Count-1)].attackCooldown <= 0)
                        {
                            if (mobAttack.hc.mobJumperMovement)
                                mobAttack.hc.mobJumperMovement.Damaged(mobAttack.hc);
                    
                            
                            if (mobAttack.hc.statusEffects[3].effectActive)// rust
                                hc.Damage(mobAttack.levels[Mathf.Clamp(mobAttack.mobParts.level, 0, mobAttack.levels.Count-1)].damage / 2, bloodSpawnPosition, transform.position + Vector3.up, null, mobAttack.hc.playerDamagedMessage[gm.language], false, mobAttack.hc.names[gm.language], mobAttack.hc, mobAttack.effectsOnAttack, false);
                            else
                                hc.Damage(mobAttack.levels[Mathf.Clamp(mobAttack.mobParts.level, 0, mobAttack.levels.Count-1)].damage, bloodSpawnPosition, transform.position + Vector3.up, null, mobAttack.hc.playerDamagedMessage[gm.language], false, mobAttack.hc.names[gm.language], mobAttack.hc, mobAttack.effectsOnAttack, false);
                        }
                    }   
                }
                break;
            }
        }
    }

    IEnumerator StartAttack()
    {
        yield return new WaitForSeconds(0.5f);
        mobAttack.Attack();
        mobAttack.Cooldown();
        atackCoroutine = null;
    }

    public void ToggleDangerous(bool danger)
    {
        return;
        
        if (!mobAttack.gameObject.activeInHierarchy)
            return;
        dangerous = danger;
    }
}