using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponController : MonoBehaviour
{
    public WeaponPickUp.Weapon weapon = WeaponPickUp.Weapon.Axe;
    public enum Type {Melee, Range, RangeAuto}
    public Type weaponType = Type.Range;

    public WeaponDataRandomizer dataRandomizer;
    public List<string> descriptions = new List<string>();

    public float noiseDistance = 50;
    
    public float damageMax = 100;
    public float damageMin = 50;

    public float durability = 1600;
    public float durabilityMax = 1600;

    public string cameraShotName = "Shot";
    public bool broken = false;
    [HideInInspector]
    public bool attackingMelee = false;

    public StatussEffectsOnAttack effectsOnAttack;

    [Header("Range Settings")]
    public float shotDelay = 0.1f;
    public float shotVfxDelay = 0.04f;
    public float attackCooldown = 0.5f;
    public float ammoClip = 7;
    public float ammoClipMax = 7;
    public float reloadTime = 1.5f;
    public float raycastAmount = 1;
    [Range(0, 10)]
    public float randomCast = 0;

    public Interactable ammoPackPrefab;

    public WeaponConnector weaponConnector;

    public List<GameObject> visibleBullets;

    public AudioClip shotClip;
    public AudioClip reloadClip;
    public AudioClip reloadClipEnd;

    [Header("Melee Settings")]
    public float meleeSwingTime = 0.5f;
    public float meleeAttackTime = 0.15f;
    public float meleeReturnTime = 0.5f;
    public float blockTime = 1f;
    public float cooldownAfterBlock = 0.7f;
    public Transform meleeImpactVfxHolder;
    public Transform blockImpactVfxHolder;
    public bool dangerous = false;
    public float meleeSwingStaminaCost = 20;
    //bool durabilityDamaged = false;

    List<MobBodyPart> damagedInOnHit;

    public AudioClip swingClip;
    public AudioClip attackClip;

    [Space]

    public float damagedTargetKnockback = 0;
    public float damagedTargetStunTime = 1;

    public bool canAct = true;
    public bool hidden = false;
    public PlayerMovement playerMovement;
    public Animator meshAnim;
    public Animator weaponMovementAnim;
    public Animator crosshair;
    public Transform crosshairVisual;

    public LayerMask layerMask;
    public Transform shotHolder;
    //public ParticleSystem shotVfx;
    //public float bulletSpeed = 15;

    public GameObject bulletImpact;
    [Range(0f, 1f)]
    public float stressAmount = 0.2f;

    public GameObject livingParticles;

    [HideInInspector]
    public bool reloading = false;

    public StaminaStats staminaStats;

    [Header("Locals")]
    public List<string> damageObjectMessage = new List<string>(); 
    
    GameManager gm;
    UiManager uiManager;
    ItemsList itemList;
    WeaponControls wc;
    PlayerAudioController pac;
    Coroutine slowMo;
    Coroutine reload;
    AiDirector aiDir;
    private PlayerSkillsController psc;

    public RandomizedPhrasesData randomPhrases;

    private void Start()
    {
        gm = GameManager.instance;
        uiManager = UiManager.instance;
        itemList = ItemsList.instance;
        pac = PlayerAudioController.instance;
        psc = PlayerSkillsController.instance;
        wc = WeaponControls.instance;
        aiDir = AiDirector.instance;

        if (weaponConnector) weaponConnector.weaponController = this;

        damagedInOnHit = new List<MobBodyPart>();
        NewStatusEffect();

        if (!gm.demo && weapon == WeaponPickUp.Weapon.Shotgun && weaponConnector && weaponConnector.barrelsCount > 1)
        {
            SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_30");
        }
    }

    void OnEnable()
    {
        if (weaponConnector && weaponConnector.crazyGunAnim)
        {
            if (dataRandomizer && dataRandomizer.dead)
            {
                weaponConnector.crazyGunAnim.speed = 0;
                print("crazy gun anim speed = " + weaponConnector.crazyGunAnim.speed);
            }
            else
            {
                weaponConnector.crazyGunAnim.speed = Random.Range(0.1f, 1.25f);
                print("crazy gun anim speed = " + weaponConnector.crazyGunAnim.speed);
            }
        }

        PlayerMovement.instance.DisableCameraAnimatorBools();
    }

    public void AttackSelf()
    {
        if (canAct)
        {
            if (weaponType == Type.Melee)
            {
                StartCoroutine(AttackSelfMelee());
            }
            else
            {
                if (ammoClip > 0)
                {
                    StartCoroutine(AttackSelfRange());
                }
                else
                {
                    uiManager.NoAmmo();
                    // play no ammo sfx
                }
            }
        }
    }

    void UseLadyShoe()
    {
        RaycastHit hit;
        Physics.Raycast(playerMovement.cameraAnimator.transform.position, WeaponControls.instance.crosshair.transform.position - playerMovement.cameraAnimator.transform.position, out hit, 1000, layerMask);
        if (hit.collider)
        {
            DynamicObstaclesManager.instance.CreatePlayersHumanProp(hit.point);
            //DynamicObstaclesManager.instance.CreatePlayersHumanMob(hit.point);
            Instantiate(bulletImpact, hit.point, Quaternion.identity);
        }
    }
    
    public void Attack()
    {    
        if (canAct)
        {
            if (weaponType == Type.Range)
            {
                if (ammoClip > 0 || (psc && psc.goldShooter && itemList && itemList.gold > 0))
                {
                    StartCoroutine(ShotAfterDelay());
                }
                else if (uiManager)
                {
                    uiManager.NoAmmo();
                    // play no ammo sfx
                }
            }
            else if (weaponType == Type.Melee) // melee
            {
                int random = Random.Range(1, 4);
                //StartCoroutine(MeleeAttack(random));
                StartCoroutine(MeleeAttack(1));
            }
            else // auto range
            {
                if (ammoClip > 0 || (psc && psc.goldShooter && itemList && itemList.gold > 0))
                {
                    StartCoroutine(ShotAfterDelay());
                }
                else if (uiManager)
                {
                    uiManager.NoAmmo();
                    // play no ammo sfx
                }
            }
        }
    }

    IEnumerator AttackSelfMelee()
    {
        canAct = false;
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.MeleeAttackSelf();
        }
        
        float staminaBuffScaler = 1f;
        if (playerMovement.hc.rustAndPoisonActive) staminaBuffScaler = 0.5f;
        staminaStats.CurrentValue -= meleeSwingStaminaCost * staminaBuffScaler;
        attackingMelee = true;
        pac.PlaySwingSound(swingClip);
        damagedInOnHit.Clear();
        
        weaponMovementAnim.SetTrigger("MeleeAttackSelf");
        playerMovement.cameraAnimator.SetBool("MeleeSwing", true);
        
        yield return new WaitForSeconds(meleeSwingTime);
        if (!gm.hub)
            aiDir.Reset();
        pac.PlayShotSound(attackClip, 0);

        dangerous = true;
        playerMovement.ShotJiggle(stressAmount);

        playerMovement.cameraAnimator.SetBool("MeleeSwing", false);
        playerMovement.cameraAnimator.SetBool("MeleeAttack", true);
        
        SpawnController.instance.MobHearNoise(transform.position, noiseDistance);
        
        LoseDurability();
        LoseDurability();
        LoseDurability();
        LoseDurability();
        LoseDurability();
        LoseDurability();
        LoseDurability();
        LoseDurability();
        string damageSelf = " myself";
        if (gm.language == 1) damageSelf = " себя";

        
        var hcToDamage = playerMovement.hc;
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            hcToDamage = GLNetworkWrapper.instance.localPlayer.connectedDummy.hc;
        }
        hcToDamage.Damage(100, playerMovement.cameraAnimator.transform.position, playerMovement.cameraAnimator.transform.position, 
            null, damageObjectMessage[gm.language] + damageSelf, true, null, playerMovement.hc, effectsOnAttack, false);
        
        //crosshair.SetTrigger("MeleeAttack");
        yield return new WaitForSeconds(meleeAttackTime);
        attackingMelee = false;

        dangerous = false;
        playerMovement.cameraAnimator.SetBool("MeleeAttack", false);
        playerMovement.cameraAnimator.SetBool("MeleeReturn", true);
        //crosshair.SetTrigger("MeleeReturn");
        yield return new WaitForSeconds(meleeReturnTime);
        playerMovement.cameraAnimator.SetBool("MeleeReturn", false);
        
        if (durability <= 0)
        {
            wc.WeaponBroke(this);
            Broke();
        }

        canAct = true;
    }

    private string meleeSwingString = "MeleeSwing";
    private string meleeAttackString = "MeleeAttack";
    private string meleeReturnString = "MeleeReturn";
    IEnumerator MeleeAttack(int attackVariation) // basic attack
    {
        canAct = false;
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.MeleeAttack();
        }

        float staminaBuffScaler = 1f;
        if (playerMovement.hc.rustAndPoisonActive) staminaBuffScaler = 0.5f;
        staminaStats.CurrentValue -= meleeSwingStaminaCost * staminaBuffScaler;
        attackingMelee = true;
        pac.PlaySwingSound(swingClip);
        damagedInOnHit.Clear();

        weaponMovementAnim.SetBool(meleeSwingString, true);
        playerMovement.cameraAnimator.SetBool(meleeSwingString, true);
        //durabilityDamaged = false;
        //crosshair.SetTrigger("MeleeSwing");
        yield return new WaitForSeconds(meleeSwingTime);
        if (!gm.hub)
            aiDir.Reset();
        pac.PlayShotSound(attackClip, 0);

        dangerous = true;
        playerMovement.ShotJiggle(stressAmount);

        weaponMovementAnim.SetBool(meleeSwingString, false);
        playerMovement.cameraAnimator.SetBool(meleeSwingString, false);
        weaponMovementAnim.SetBool(meleeAttackString, true);
        playerMovement.cameraAnimator.SetBool(meleeAttackString, true);
        //crosshair.SetTrigger("MeleeAttack");
        yield return new WaitForSeconds(meleeAttackTime);
        if (weapon == WeaponPickUp.Weapon.LadyShoe)
            UseLadyShoe();
        
        attackingMelee = false;

        if (SpawnController.instance)
            SpawnController.instance.MobHearNoise(transform.position, noiseDistance);
        
        dangerous = false;
        weaponMovementAnim.SetBool(meleeAttackString, false);
        playerMovement.cameraAnimator.SetBool(meleeAttackString, false);
        weaponMovementAnim.SetBool(meleeReturnString, true);
        playerMovement.cameraAnimator.SetBool(meleeReturnString, true);
        //crosshair.SetTrigger("MeleeReturn");
        yield return new WaitForSeconds(meleeReturnTime);
        weaponMovementAnim.SetBool(meleeReturnString, false);
        playerMovement.cameraAnimator.SetBool(meleeReturnString, false);
        canAct = true;
    }

    public void BlockVfx()
    {
        var newBlockImpact = Instantiate(bulletImpact, blockImpactVfxHolder.position, Quaternion.identity);
        newBlockImpact.transform.LookAt(blockImpactVfxHolder.position);
        Destroy(newBlockImpact, 2);
    }

    private void OnTriggerStay(Collider coll)
    {
        if (weapon == WeaponPickUp.Weapon.LadyShoe || !dangerous || (coll.gameObject.layer != 11 && coll.gameObject.layer != 10 && coll.gameObject.layer != 16) ||
            coll.gameObject == playerMovement.gameObject) return;
        
        
        bool alreadyHit = false;
        for (var index = 0; index < damagedInOnHit.Count; index++)
        {
            MobBodyPart part = damagedInOnHit[index];
            if (coll && part && part.gameObject == coll.gameObject)
                alreadyHit = true;
        }

        if (!alreadyHit)
        {
            if (FindDamageTarget(coll.gameObject, meleeImpactVfxHolder.position, playerMovement.cameraAnimator.transform.position))
            {
                if (psc.activeCult == PlayerSkillsController.Cult.bleedingCult)
                    playerMovement.hc.Heal((damageMax + damageMin) / 20);
                LoseDurability();
            }

            uiManager.UpdateWeapons();
            damagedInOnHit.Add(coll.gameObject.GetComponent<MobBodyPart>());
        }

        var newBulletImpact = Instantiate(bulletImpact, meleeImpactVfxHolder.position, Quaternion.identity);
        newBulletImpact.transform.LookAt(meleeImpactVfxHolder.position);
        Destroy(newBulletImpact, 2);
    }
    
    IEnumerator AttackSelfRange()
    {
        canAct = false;
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.RangeAttackSelf();
        }
        pac.PlayShotSound(shotClip, weaponConnector.activeBarrels.Count - 1);

        weaponMovementAnim.SetTrigger("ShootSelf");
        
        yield return new WaitForSeconds(shotDelay);
        
        if (!gm.hub)
            aiDir.Reset();

        LoseDurability();
        LoseDurability();
        LoseDurability();
        LoseDurability();

        uiManager.UpdateWeapons();

        playerMovement.playerAudio.heartbeatSource.volume += stressAmount;
        playerMovement.ShotJiggle(stressAmount);

        playerMovement.cameraAnimator.SetTrigger(cameraShotName);
        MouseLook.instance.Recoil();
        if (weaponType != Type.RangeAuto)
            crosshair.SetTrigger("Attack");
        else
            crosshair.SetTrigger("AutoAttack");
        
        if (SpawnController.instance)
            SpawnController.instance.MobHearNoise(transform.position, noiseDistance);

        for (int b = 0; b < weaponConnector.activeBarrels.Count; b++)
        {
            if (ammoClip > 0)
            {
                weaponConnector.activeBarrels[b].shotVfx.Play();
                
                ammoClip--;
                
                string damageSelf = " myself";
                if (gm.language == 1) damageSelf = " себя";
        
                var hcToDamage = playerMovement.hc;
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    hcToDamage = GLNetworkWrapper.instance.localPlayer.connectedDummy.hc;
                }
                
                hcToDamage.Damage(100, playerMovement.cameraAnimator.transform.position, playerMovement.cameraAnimator.transform.position, 
                    null, damageObjectMessage[gm.language] + damageSelf, true, null, playerMovement.hc, effectsOnAttack, false);

                yield return new WaitForSecondsRealtime(Random.Range(0.001f,0.01f));
            }
        }

        yield return new WaitForSeconds(shotVfxDelay);
        if (durability <= 0)
        {
            wc.WeaponBroke(this);
            Broke();
        }
        uiManager.UpdateAmmo();

        yield return new WaitForSeconds(attackCooldown);
        canAct = true;
    }
    
    IEnumerator ShotAfterDelay()
    {
        canAct = false;
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.RangeAttack();
        }
        pac.PlayShotSound(shotClip, weaponConnector.activeBarrels.Count - 1);
        if (meshAnim)
            meshAnim.SetTrigger("ReadyForShot");
        yield return new WaitForSeconds(shotDelay);
        if (!gm.hub)
            aiDir.Reset();

        LoseDurability();
        if (durability <= 0)
        {
            wc.WeaponBroke(this);
            Broke();
        }

        uiManager.UpdateWeapons();

        playerMovement.playerAudio.heartbeatSource.volume += stressAmount;
        playerMovement.ShotJiggle(stressAmount);

        if (meshAnim)
            meshAnim.SetTrigger("Shot");
        playerMovement.cameraAnimator.SetTrigger(cameraShotName);
        MouseLook.instance.Recoil();
        if (weaponType != Type.RangeAuto)
            crosshair.SetTrigger("Attack");
        else
            crosshair.SetTrigger("AutoAttack");

        if (SpawnController.instance)
            SpawnController.instance.MobHearNoise(transform.position, noiseDistance);
        
        for (int b = 0; b < weaponConnector.activeBarrels.Count; b++)
        {
            //if (ammoClip > 0)
            {
                weaponConnector.activeBarrels[b].shotVfx.Play();
                
                if (ammoClip > 0)
                    ammoClip--;
                else if (psc.goldShooter && itemList.gold > 0)
                {
                    int goldAmount = 1;
                    switch (weapon)
                    {
                        case WeaponPickUp.Weapon.Pistol:
                            goldAmount = 2;
                            break;
                        case WeaponPickUp.Weapon.Revolver:
                            goldAmount = 5;
                            break;
                        case WeaponPickUp.Weapon.Shotgun:
                            goldAmount = 10;
                            break;
                        case WeaponPickUp.Weapon.TommyGun:
                            goldAmount = 2;
                            break;
                        case WeaponPickUp.Weapon.OldPistol:
                            goldAmount = 3;
                            break;
                    }    
                    
                    itemList.LoseGold(goldAmount);
                    psc.PlayerLoseGold(goldAmount);
                }
                
                for (int i = 1; i <= raycastAmount; i++)
                {
                    RaycastHit hit;
                    Vector3 r = new Vector3(Random.Range(-randomCast, randomCast), Random.Range(-randomCast, randomCast), Random.Range(-randomCast, randomCast));
                    Physics.Raycast(playerMovement.cameraAnimator.transform.position, (wc.crosshair.transform.position - playerMovement.cameraAnimator.transform.position) + r, out hit, 1000, layerMask);
                    //Physics.Raycast(playerMovement.cameraAnimator.transform.position, (weaponConnector.activeBarrels[b].shotDirection.transform.position - playerMovement.cameraAnimator.transform.position) + r, out hit, 1000, layerMask);

                    if (hit.collider != null && (hit.collider.gameObject.layer == 11 || hit.collider.gameObject.layer == 10|| hit.collider.gameObject.layer == 16)) // if UNIT or wall
                    {
                        FindDamageTarget(hit.collider.gameObject, hit.point, transform.position);
                    }

                    var newBulletImpact = Instantiate(bulletImpact, hit.point, Quaternion.identity);
                    newBulletImpact.transform.LookAt(shotHolder.transform.position);
                    Destroy(newBulletImpact, 2);
                }      
                yield return new WaitForSecondsRealtime(Random.Range(0.001f,0.01f));
            }
        }
        //UpdateBulletsInGun();

        yield return new WaitForSeconds(shotVfxDelay);
        uiManager.UpdateAmmo();
        itemList.activeWeaponClip = ammoClip;

        yield return new WaitForSeconds(attackCooldown);
        canAct = true;
    }

    bool FindDamageTarget(GameObject go, Vector3 bloodPosition, Vector3 shotOrigin)// AND DEAL DAMAGE
    {
        if (go)
        {
            MobBodyPart part  = go.GetComponent<MobBodyPart>();

            if (part != null && part.hc != null)
            {
                if (slowMo != null)
                {
                    StopCoroutine(slowMo);
                }
                slowMo = StartCoroutine(SlowMo());
                float randomDamage = Random.Range(damageMin, damageMax);
                if (psc.vertigo)
                    randomDamage *= 1.5f;

                if (durability < durabilityMax / 20)
                {
                    randomDamage *= 2f;
                    durability = 0;
                }

                // if weapon is cursed
                if (dataRandomizer.npc)
                {
                    if (dataRandomizer.statusEffect == 0)
                    {
                        playerMovement.hc.AddPoison(15);
                    }
                    if (dataRandomizer.statusEffect == 1)
                    {
                        playerMovement.hc.AddFire(15);
                    }
                    if (dataRandomizer.statusEffect == 2) 
                    {
                        playerMovement.hc.AddBleed(15);
                    }
                    if (dataRandomizer.statusEffect == 3) 
                    {
                        playerMovement.hc.AddRust(15);
                    }
                    if (dataRandomizer.statusEffect == 4) 
                    {
                        playerMovement.hc.AddRegen(15);
                    }
                    if (dataRandomizer.statusEffect == 5) 
                    {
                        playerMovement.hc.AddGoldHunger(15);
                    }
                }

                if (dataRandomizer.dead) randomDamage *= 0.75f;
                randomDamage = ScaleDamageByMemento(randomDamage);
                
                if (playerMovement.hc.fireAndBloodActive) randomDamage *= 2;
                
                part.hc.Damage(randomDamage * part.damageModificator, 
                    bloodPosition, shotOrigin, part, damageObjectMessage[gm.language] + part.hc.damagedByPlayerMessage[gm.language], true, null, playerMovement.hc, effectsOnAttack, true);

                if (weapon == WeaponPickUp.Weapon.VeinWhip && Random.value >= 0.8f)
                    part.hc.InstantBleed();

                if (part.hc.health <= 0)
                {
                    KilledSomeone(part.hc);
                }
                else
                {
                    if (psc.fireAttack && Random.value > 0.8f)
                        part.hc.StartFire();
                    
                    // ADD HERE NEW INSTANT EFFECTS

                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    {
                        if (part.hc.playerNetworkDummyController && PlayerSkillsController.instance.traitor)
                        {
                            playerMovement.hc.Heal(playerMovement.hc.healthMax / 5);
                        }
                    }
                }

                if (durability <= 0)
                {
                    wc.WeaponBroke(this);
                    Broke();
                }
                
                return true;
            }
        }
        return false;
    }

    void KilledSomeone(HealthController hc)
    {
        print("Killed someone");
        itemList.AddToBadReputation(hc.addBadRepOnDeath);

        gm.SaveNewKill();
        
        if (SteamAchievements.instance)
        {
            if (hc.mobKilledAchievementID.Length > 0)
            {
                SteamAchievements.instance.UnlockSteamAchievement(hc.mobKilledAchievementID);
            }

            switch (weapon)
            {
                case WeaponPickUp.Weapon.Pipe:
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_0");
                    break;
                case WeaponPickUp.Weapon.Revolver:
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_1");
                    break;
                case WeaponPickUp.Weapon.Axe:
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_2");
                    break;
                case WeaponPickUp.Weapon.Pistol:
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_3");
                    break;
                case WeaponPickUp.Weapon.Shotgun:
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_4");
                    break;
                case WeaponPickUp.Weapon.TommyGun:
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_5");
                    break;
                case WeaponPickUp.Weapon.Map:
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_8");
                    break;
            }   
        }
    }

    float ScaleDamageByMemento(float dmg)
    {
        if (weapon == WeaponPickUp.Weapon.Axe && psc.axeLover ||
            weapon == WeaponPickUp.Weapon.Knife && psc.knifeLover ||
            weapon == WeaponPickUp.Weapon.Pipe && psc.pipeLover ||
            weapon == WeaponPickUp.Weapon.Pistol && psc.pistolLover ||
            weapon == WeaponPickUp.Weapon.Revolver && psc.revolverLover ||
            weapon == WeaponPickUp.Weapon.Shotgun && psc.shotgunLover ||
            weapon == WeaponPickUp.Weapon.TommyGun && psc.tommyLover)
        {
            if (!psc.dashOnAttack)
                return dmg * 1.5f;
            else
                return dmg * 2f;
        }
        
        return dmg;
    }

    public void NewStatusEffect()
    {
        effectsOnAttack = GetStatusEffects(); 
    }
    
    StatussEffectsOnAttack GetStatusEffects()
    {
        StatussEffectsOnAttack newEffects = new StatussEffectsOnAttack();
        if (dataRandomizer.statusEffect > -1)
        {
            newEffects.effects.Add(playerMovement.hc.statusEffects[dataRandomizer.statusEffect]);
            newEffects.effectsValues.Add(101);   
        }
        
        return newEffects;
    }

    public void LoseDurability()
    {
        return;
        
        float durabilityDamage = Random.Range(damageMin, damageMax);

        if (gm.tutorialPassed == 0)
            durabilityDamage *= 0.5f;
        else if (psc.horrorMode) durabilityDamage *= 2f;
        
        if (psc.strongWeapon) durabilityDamage *= 0.75f;
        
        if (gm.difficultyLevel == GameManager.GameMode.StickyMeat)
            durabilityDamage *= 0.7f;
        
        if (playerMovement.hc.statusEffects[3].effectActive) // if player is rusting
            durability -=  durabilityDamage * playerMovement.hc.statusEffects[3].effectValue;
        else
            durability -= durabilityDamage;
    }

    public void LoseDurabilityEverySecond()
    {
        return;

        if (durability > 0)
        {
            if (PlayerSkillsController.instance.strongWeapon)
                durability -= 1;
            else
                durability -= 1 * 3f;   
        }
        else
        {
            wc.WeaponBroke(this);
            Broke();
        }
        UiManager.instance.UpdateWeapons();
    }

    public void FixWeapon()
    {
        durability = durabilityMax;
        broken = false;
        weaponMovementAnim.SetBool("Broken", false);
        if (livingParticles)
            livingParticles.SetActive(true);
        UiManager.instance.UpdateWeapons();
    }

    public void Broke()
    {
        durability = 0;
        broken = true;
        weaponMovementAnim.SetBool("Broken", true);
        if (livingParticles)
            livingParticles.SetActive(false);
        UiManager.instance.UpdateWeapons();
    }
    
    public IEnumerator SlowMo()
    {
        Time.timeScale = 0.15f;
        yield return new WaitForSeconds(0.05f * Time.timeScale);
        Time.timeScale = 1f;
    }

    public IEnumerator UseTool(float t)
    {
        canAct = false;
        yield return new WaitForSeconds(t);
        canAct = true;
    }

    public void Reload()
    {
        if (reload == null)
            reload = StartCoroutine(Reloading());
    }
    
    public IEnumerator Reloading()
    {
        var availableSpace = ammoClipMax - ammoClip;
        var bulletsToAdd = Math.Min(itemList.ammoDataStorage.GetAmmoCount(weapon), availableSpace); 
        
        if (bulletsToAdd <= 0)
            yield break;

        PlayerAudioController.instance.PlayReload(reloadClip);

        if (playerMovement.mouseLook.activeWeaponHolderAnim)
            playerMovement.mouseLook.activeWeaponHolderAnim.SetBool("Reloading", true);
        reloading = true;
        canAct = false;
        
        if (itemList.savedQuestItems.Contains(6))
            yield return new WaitForSeconds(reloadTime / 2);
        else
            yield return new WaitForSeconds(reloadTime);
        
        itemList.ammoDataStorage.ReduceAmmo(weapon, bulletsToAdd);
        
        if (playerMovement.mouseLook.activeWeaponHolderAnim)
            playerMovement.mouseLook.activeWeaponHolderAnim.SetBool("Reloading", false);

        PlayerAudioController.instance.StopReload(reloadClipEnd);
        canAct = true;
        reloading = false;

        ammoClip += bulletsToAdd;
        //UpdateBulletsInGun();
        reload = null;
        itemList.activeWeaponClip = ammoClip;
        uiManager.UpdateAmmo();
    }

    public void StopReloading()
    {
        if (reload != null)
        {
            StopCoroutine(Reloading());   
            reload = null;
            
            reloading = false;
            playerMovement.mouseLook.activeWeaponHolderAnim.SetBool("Reloading", false);
            PlayerAudioController.instance.StopReload(reloadClipEnd);
        }
    }

    /*
    private void UpdateBulletsInGun()
    {
        for (var i = visibleBullets.Count; i > 0; i --)
            visibleBullets[i - 1].SetActive(i <= ammoClip);
    }
    */
}