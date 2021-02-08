using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using CerealDevelopment.TimeManagement;
using Mirror;
using PlayerControls;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HealthController : MonoBehaviour, IUpdatable
{
    public bool spawnedWithRoom = false;
    public List<string> names = new List<string>();
    
    public bool player = false;
    public float health = 1000;
    [SerializeField]
    public float healthMax = 1000;
    
    [Header("0 - poison; 1 - Fire; 2 - bleed; 3 - rust; 4 - regen; 5 - healthHunger")]
    public List<StatusEffects> statusEffects = new List<StatusEffects>();

    private float fireAndBloodAmount = 0; 
    public bool fireAndBloodActive = false;
    private Coroutine fireAndBloodCoroutine;
    
    private float rustAndPoisonAmount = 0; 
    public bool rustAndPoisonActive = false;
    private Coroutine rustAndPoisonCoroutine;
    
    private float loveAndGoldAmount = 0; 
    public bool loveAndGoldActive = false;
    private Coroutine loveAndGoldCoroutine;

    [Header("StatusEffectsVisuals")]
    public List<EffectVisualController> statusEffectsVisuals;
    [Header("Fire")]
    public ParticleSystem fireParticles;
    ParticleSystem.EmissionModule firePartilcesEmission;
    Coroutine burningCoroutine;
    public AudioSource fireAu;
    float fireAuVolume = 1;

    [Header("Player")]
    public PlayerMovement pm;
    public WeaponControls wc;
    public float damageCooldown = 0;
    float damageCooldownMax = 1;
    public PlayerAudioController pac;
    public bool invincible = false;
    public bool damagedOnLevel = false;
    public float faceHuggedCooldown = 30;
    public PlayerNetworkDummyController playerNetworkDummyController;

    public bool inLove = false;

    [Header("Monster")] public bool boss = false;
    public BossStateChanger bossStateChanger;
    public PropController propController;
    public string mobKilledAchievementID = "";
    public bool cultist = false;
    public MobGroundMovement mobGroundMovement;
    public MobWallsJumperMovement mobJumperMovement;
    public FaceEaterBehaviour faceEaterBehaviour;
    public MobNeedleMovement mobNeedleMovement;
    public MobHideInCorners mobHideInCorners;
    public MobPartsController mobPartsController;
    public MobBombBehaviour bombBehaviour;
    public MobMeleeAttack mobMeleeAttack;
    public MobAudioManager mobAudio;
    public MobDeerActivator deerActivator;
    public MobProjectileShooter mps;
    public MeatTrap meatTrap;
    public WallTrapController wallTrap;
    public WallBlockerController wallBlockerController;
    public TrapController trapController;
    public SetEffectToTileOnDamage setEffectToTileOnDamage;
    public Collider monsterTrapTrigger;
    public IkarusMovement ikarusMovement;
    public bool peaceful = false;
    public bool damagedByPlayer = false;
    [Header("peaceful if player has one eye")]
    public bool eyeEater = false;

    public DynamicMapMark dynamicMapMark;

    public float addBadRepOnDeath = 0;

    [Header("Prop")]
    public DoorController door;

    [Header("NPC")]
    public NpcController npcInteractor;
    public WeaponPickUp weaponPickUp;
    public int startQuestOnDeath = -1;

    [Range(0,1)]
    public float armor = 0;

    [Header("Misc")] 
    public TileController wallMasterTile;
    public TileController usedTile;
    public ParticleSystem deathParticles;
    public List<ParticleSystem> deathParticlesMulti;
    private AudioSource deathAudioSource;

    private bool lastChance = true;
    private SpawnController sc;
    GameManager gm;
    private UiManager ui;
    
    [Header("Locals")]
    public List<string> playerDamagedMessage = new List<string>();
    public List<string> damagedByPlayerMessage = new List<string>();

    public bool canHear = true;
    
    string healthBool = "Health";
    ItemsList il;
    private PlayerSkillsController psc;
    Vector3 npcInteractorOffset;

    public ActiveNpcController activeNpc;
    private FaceEaterBehaviour currentFaceEater;

    public bool wall = false;
    private bool dead = false;

    void Awake()
    {
        gm = GameManager.instance;
        sc = SpawnController.instance;

        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            if (boss)
            {
                health *= 2;
                healthMax *= 2;
            }
            if (spawnedWithRoom)
            {
                if (LevelGenerator.instance.levelgenOnHost)
                    NetworkServer.Spawn(gameObject);
                else
                {
                    Destroy(gameObject);
                }   
            }
        }
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && pm)
        {
            // dont send player to unit list in coop
        }
        else if (gm)
        {
            gm.AddUnit(this);   
        }

        il = ItemsList.instance;

        healthMax = health;
        damageCooldownMax = damageCooldown;

        if (pm)
        {
            pm.hc = this;
        }

        StartCoroutine(DamageCooldown());

        if (mobGroundMovement)
            mobGroundMovement.hc = this;
        if (mobJumperMovement)
            mobJumperMovement.hc = this;
        if (mobMeleeAttack)
        {
            mobMeleeAttack.hc = this;
        }
        if (mobPartsController)
            mobPartsController.hc = this;
        if (mobHideInCorners)
            mobHideInCorners.hc = this;
        if (mps)
            mps.hc = this;
        if (fireParticles)
        {
            firePartilcesEmission = fireParticles.emission;
            firePartilcesEmission.rateOverTime = 0;
            fireAuVolume = fireAu.volume;
            fireAu.volume = 0;
            fireAu.Stop();
        }
        if (npcInteractor && !eyeEater)
        {
            peaceful = true;
        }
    }

    public void ApplyHeartContainer()
    {
        healthMax += 100;
        UiManager.instance.UpdateHealthbar();
    }
    
    void Start()
    {
        ui = UiManager.instance;

        if (wallTrap)
            wallTrap.hc = this;
        psc = PlayerSkillsController.instance;
        ResetStatusEffects();
        
        if (npcInteractor) npcInteractor.hc = this;
        
        if (deathParticles)
            deathAudioSource = deathParticles.gameObject.GetComponent<AudioSource>();

        if (boss)
        {
            ui.bossHpParent.SetActive(true);
            ui.bossNameText.text = names[gm.language];
        }

        if (trapController) trapController.ownHc = this;
        if (wallBlockerController) wallBlockerController.hc = this;

        if (pm)
        {
            for (int i = 0; i < il.heartContainerAmount; i++)
            {
                ApplyHeartContainer();
            }

            if (!gm.hub && GutProgressionManager.instance.playerFloor >= 0)
            {
                GutProgressionManager.instance.SaveNewCheckpoint();
            }

            faceHuggedCooldown = 60;
        }
        else
        {
            if (sc && canHear && propController == null)
                sc.mobsInGame.Add(this);
        }
    }

    void ResetStatusEffects()
    {
        for (var index = 0; index < statusEffects.Count; index++)
        {
            var eff = statusEffects[index];
            eff.effectActive = false;
            eff.effectLevelCurrent = 0;
        }
    }
    
    private void OnEnable()
    {
        this.EnableUpdates();

        if (pm && il.savedQuestItems.Contains(8)) // if player have camera
            StartCoroutine(CameraRegen());
    }

    private void OnDisable()
    {
        this.DisableUpdates();
        StopAllCoroutines();
        
        GutQuestsController.instance.UnitKilled(this);
    }

    public void ResetFaceHugCooldown(FaceEaterBehaviour faceEater)
    {
        currentFaceEater = faceEater;
        faceHuggedCooldown = 20;
    }

    IEnumerator CameraRegen()
    {
        while (gameObject.activeInHierarchy)
        {
            if (health < healthMax * 0.1f)
            {
                health += Time.deltaTime * 10f;
            }

            yield return null;
        }
    }

    void IUpdatable.OnUpdate()
    {
        if (pm && pm.controller.enabled == false && pm.inTransport == null)
            return;
        
        if (pm)
        {
            if (faceHuggedCooldown > 0)
                faceHuggedCooldown -= Time.deltaTime;
            
            if (pm.controller.enabled && psc.dickheadStomach && health > healthMax * 0.1f)
            {
                // damage over time
                DamageOverTime(4 * Time.deltaTime, null, null);
            }
        }
        else if (eyeEater)
        {
            if (il.lostAnEye == 1)
            {
                if (!peaceful && !damagedByPlayer)
                {
                    peaceful = true;
                    if (!npcInteractor.gameObject.activeInHierarchy)
                        npcInteractor.gameObject.SetActive(true);
                }
            }
            else if (peaceful)
                peaceful = false;
        }
        if (npcInteractor)
            npcInteractor.transform.position = transform.position + npcInteractorOffset;

        if (health > 0)
        {
            GetTileEffect();   
        }

        if (boss)
        {
            ui.bossHealthbar.fillAmount = health / healthMax;
        }
    }

    public void PlayerAteWeapon(float percentage, StatussEffectsOnAttack effect)
    {
        float dickheadStomachScaler = 1;
        if (psc.dickheadStomach)
            dickheadStomachScaler = 2;
        
        if (percentage < 0.1f)
            Heal(100f * dickheadStomachScaler);
        else 
            Heal(Mathf.Clamp(healthMax * percentage, 200, 400) * dickheadStomachScaler);

        if (effect != null && effect.effects.Count > 0)
        {
            switch (effect.effects[0].effectType)
            {
                case StatusEffects.StatusEffect.Poison:
                    InstantPoison();
                    break;
                case StatusEffects.StatusEffect.Fire:
                    StartFire();
                    break;
                case StatusEffects.StatusEffect.Bleed:
                    InstantBleed();
                    break;
                case StatusEffects.StatusEffect.Rust:
                    InstantRust();
                    break;
                case StatusEffects.StatusEffect.HealthRegen:
                    InstantRegen();
                    break;
                case StatusEffects.StatusEffect.GoldHunger:
                    InstantGoldHunger();
                    break;
                case StatusEffects.StatusEffect.Cold:
                    InstantCold();
                    break;
                case StatusEffects.StatusEffect.InLove:
                    InstantLove();
                    break;
            }   
        }
    }

    void GetTileEffect()
    {
        // get tile effect here
        if (usedTile && statusEffects.Count > 0 && usedTile.tileStatusEffect == StatusEffects.StatusEffect.Poison)
        {
            if (!statusEffects[0].effectImmune)
                FillEffect(0);
        }
        else
        {
            DepleteEffect(0);
        }
        
        if (usedTile && statusEffects.Count > 1 && usedTile.tileStatusEffect == StatusEffects.StatusEffect.Fire)
        {
            if (!statusEffects[1].effectImmune)
                FillEffect(1);
        }
        else
        {
            DepleteEffect(1);
        }

        if (usedTile && statusEffects.Count > 2 && usedTile.tileStatusEffect == StatusEffects.StatusEffect.Bleed)
        {
            if (!statusEffects[2].effectImmune)
                FillEffect(2);
        }
        else
        {
            DepleteEffect(2);
        }
        if (usedTile && statusEffects.Count > 3 && usedTile.tileStatusEffect == StatusEffects.StatusEffect.Rust)
        {
            if (!statusEffects[3].effectImmune)
                FillEffect(3);
        }
        else
        {
            DepleteEffect(3);
        }
        
        if (usedTile && statusEffects.Count > 4 && usedTile.tileStatusEffect == StatusEffects.StatusEffect.HealthRegen)
        {
            if (!statusEffects[4].effectImmune)
                FillEffect(4);
        }
        else
        {
            DepleteEffect(4);
        }
        
        if (usedTile && statusEffects.Count > 5 && usedTile.tileStatusEffect == StatusEffects.StatusEffect.GoldHunger)
        {
            if (!statusEffects[5].effectImmune)
                FillEffect(5);
        }
        else
        {
            DepleteEffect(5);
        }
        
        if (usedTile && statusEffects.Count > 6 && usedTile.tileStatusEffect == StatusEffects.StatusEffect.Cold && !statusEffects[1].effectActive)
        {
            if (!statusEffects[6].effectImmune)
                FillEffect(6);
        }
        else
        {
            DepleteEffect(6);
        }
        
        if (usedTile && statusEffects.Count > 7 && usedTile.tileStatusEffect == StatusEffects.StatusEffect.InLove)
        {
            if (!statusEffects[7].effectImmune)
                FillEffect(7);
        }
        else
        {
            DepleteEffect(7);
        }

        if (usedTile && usedTile.tileStatusEffect == StatusEffects.StatusEffect.Null)
        {
            for (var index = 0; index < usedTile.statusEffectsVisuals.Count; index++)
            {
                var eff = usedTile.statusEffectsVisuals[index];
                if (eff.gameObject.activeInHierarchy)
                {
                    eff.DestroyEffect(false);
                }
            }
        }

        ApplyEffects();
    }

    void ApplyEffects()
    {
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].effectActive && statusEffects[i].effectLevelCurrent > 0)
            {
                if (i == 0 && !statusEffects[i].effectImmune) // poison
                {
                    if (health > healthMax / 20)
                    {
                        string damager = "Poison";
                        if (gm.language == 1)
                            damager = "Яд";
                        
                        DamageOverTime(statusEffects[i].effectValue * Time.deltaTime , damager, null);
                    }
                    else
                    {
                        Antidote();
                    }
                }
                else if (i == 1 && !statusEffects[i].effectImmune) // fire
                {
                    string damager = "Fire";
                    if (gm.language == 1)
                        damager = "Огонь";
                        
                    if (!fireAu.isPlaying)
                        StartFire();
                    
                    DamageOverTime(statusEffects[i].effectValue * Time.deltaTime , damager, null);
                }
                else if (i == 2 && !statusEffects[i].effectImmune) // bleed
                {
                    string damager = "Bleeding";
                    if (gm.language == 1)
                        damager = "Кровотечение";

                    DamageOverTime(statusEffects[i].effectValue * Time.deltaTime , damager, null);
                }
                else if (i == 3 && !statusEffects[i].effectImmune) // rust
                {
                    
                }
                else if (i == 4 && !statusEffects[i].effectImmune) //regen
                {
                    HealOverTime(Time.deltaTime * statusEffects[i].effectValue);
                }
                else if (i == 5 && !statusEffects[i].effectImmune) // gold hunger
                {
                    if (pm)
                    {
                        if (il.gold > 0)
                        {
                            il.LoseGold(statusEffects[i].effectValue * Time.deltaTime);
                            psc.PlayerLoseGold(Time.deltaTime * statusEffects[i].effectValue);
                        }
                        else
                        {
                            string damager = "Gold Hunger";
                            if (gm.language == 1)
                                damager = "Голод по Золоту";
                            DamageOverTime(statusEffects[i].effectValue * 10 * Time.deltaTime, damager, null);
                        }   
                    }
                    else
                    {
                        string damager = "Gold Hunger";
                        if (gm.language == 1)
                            damager = "Голод по Золоту";
                        DamageOverTime(statusEffects[i].effectValue * Time.deltaTime, damager, null);
                    }  
                }
                else if (i == 6 && !statusEffects[i].effectImmune) // cold
                {
                    if (pm)
                    {
                        pm.coldScaler = Mathf.Lerp(pm.coldScaler, 0.75f, Time.deltaTime);
                        if (!pm.movementStats.isRunning)
                        {
                            string damager = "Bone Shiver";
                            if (gm.language == 1) damager = "Костная Дрожь";
                            
                            DamageOverTime(statusEffects[i].effectValue * Time.deltaTime, damager, null);   
                        }                            
                    }
                    else
                    {
                        if (mobGroundMovement)
                            mobGroundMovement.coldModifier = Mathf.Lerp(mobGroundMovement.coldModifier, 0.5f, Time.deltaTime);
                        
                        if(mobJumperMovement)
                            mobJumperMovement.coldModifier = Mathf.Lerp(mobJumperMovement.coldModifier, 0.5f, Time.deltaTime);
                    }  
                }
                else if (i == 7 && !statusEffects[i].effectImmune) // love
                {
                    inLove = true;
                }
            }
            if (pm)
            {
                ui.UpdateStatusEffects(i);
            }
        }
    }
    
    void FillEffect(int index)
    {
        float modificator = 1;
        if (index == 6 && pm) // if cold and player has a torch, slow cold
        {
            if (wc.activeWeapon && wc.activeWeapon.weapon == WeaponPickUp.Weapon.Torch && wc.activeWeapon.durability > 0)
                modificator = 0.33f;
            if (pm.movementStats.isRunning)
                modificator *= 0.5f;
        }
        if (statusEffects[index].effectLevelCurrent < statusEffects[index].effectLevelMax)
        {
            statusEffects[index].effectLevelCurrent += Time.deltaTime * statusEffects[index].fillSpeed * modificator;
        }
        else
        {
            if (index == 4 && usedTile && usedTile.tileStatusEffect == StatusEffects.StatusEffect.HealthRegen)
            {
                usedTile.tileStatusEffect = StatusEffects.StatusEffect.Null;
                usedTile.statusEffectsVisuals[4].DestroyEffect(false);
            }
            
            if (!statusEffects[index].effectActive)
            {
                statusEffects[index].effectActive = true;
                
                if (index == 0)
                    GutQuestsController.instance.UnitPoisoned(this);
                else if (index == 1)
                    GutQuestsController.instance.UnitOnFire(this);
                else if (index == 2)
                    GutQuestsController.instance.UnitBleeding(this);
                
                if (index == 0)
                {
                    if (pac)
                        pac.PlayPoisonEffect();
                    else
                    {
                        if (statusEffectsVisuals.Count > 0)
                        {
                            statusEffectsVisuals[0].gameObject.SetActive(true);
                            statusEffectsVisuals[0].StartEffect();
                        }
                    }
                }
                else if (index == 1)
                {
                    StartFire();
                }
                else if (index == 2)
                {
                    if (pac)
                        pac.PlayPoisonEffect();
                    else
                    {
                        if (statusEffectsVisuals.Count > 2)
                        {
                            statusEffectsVisuals[2].gameObject.SetActive(true);
                            statusEffectsVisuals[2].StartEffect();
                        }
                    }
                }
                else if (index == 3)
                {
                    if (pac)
                        pac.PlayPoisonEffect();
                    else
                    {
                        if (statusEffectsVisuals.Count > 3)
                        {
                            statusEffectsVisuals[3].gameObject.SetActive(true);
                            statusEffectsVisuals[3].StartEffect();
                        }
                    }
                }
                else if (index == 4)
                {
                }
                else if (index == 5)
                {
                    if (pac)
                        pac.PlayPoisonEffect();
                    else
                    {
                        if (statusEffectsVisuals.Count > 5)
                        {
                            statusEffectsVisuals[5].gameObject.SetActive(true);
                            statusEffectsVisuals[5].StartEffect();
                        }
                    }
                }
                else if (index == 6)
                {
                    if (pac)
                        pac.PlayPoisonEffect();
                    else
                    {
                        if (statusEffectsVisuals.Count > 6)
                        {
                            statusEffectsVisuals[6].gameObject.SetActive(true);
                            statusEffectsVisuals[6].StartEffect();
                        }
                    }
                }
                else if (index == 7)
                {
                    if (pac)
                        pac.PlayPoisonEffect();
                    else
                    {
                        if (statusEffectsVisuals.Count > 7)
                        {
                            statusEffectsVisuals[7].gameObject.SetActive(true);
                            statusEffectsVisuals[7].StartEffect();
                        }
                    }
                }
                
                if (mobGroundMovement && mobGroundMovement.monsterState == MobGroundMovement.State.Idle) mobGroundMovement.Hide(false);
                if (mobHideInCorners && mobHideInCorners.mobState == MobHideInCorners.State.Hiding) mobHideInCorners.FindNewCorner();
            }
        }
    }

    public void InstantPoison()
    {
        if (statusEffects.Count > 0)
        {
            statusEffects[0].effectLevelCurrent = statusEffects[0].effectLevelMax;
            statusEffects[0].effectActive = true;
            if (statusEffectsVisuals.Count > 0)
            {
                statusEffectsVisuals[0].gameObject.SetActive(true);
                statusEffectsVisuals[0].StartEffect();   
            }      
            if (pm) 
                ui.UpdateStatusEffects(0);
        }
    }
    
    public void InstantBleed()
    {
        if (statusEffects.Count > 2)
        {
            statusEffects[2].effectLevelCurrent = statusEffects[2].effectLevelMax;
            statusEffects[2].effectActive = true;
            if (statusEffectsVisuals.Count > 2)
            {
                statusEffectsVisuals[2].gameObject.SetActive(true);
                statusEffectsVisuals[2].StartEffect();   
            }      
            if (pm) 
                ui.UpdateStatusEffects(2);
        }
    }
    
    public void StartFire()
    {
        Anticold();
        if (fireParticles)
        {
            fireParticles.gameObject.SetActive(false);
            fireParticles.gameObject.SetActive(true);
            
            firePartilcesEmission.rateOverTime = 65;
            fireAu.volume = fireAuVolume;
            fireAu.Play();   
            
            statusEffects[1].effectLevelCurrent = statusEffects[1].effectLevelMax;
            statusEffects[1].effectActive = true; 
        }
        if (pm) 
            ui.UpdateStatusEffects(1);
    }
    public void InstantRegen()
    {
        if (statusEffects.Count > 4)
        {
            statusEffects[4].effectLevelCurrent = statusEffects[4].effectLevelMax;
            statusEffects[4].effectActive = true;
            if (statusEffectsVisuals.Count > 4)
            {
                statusEffectsVisuals[4].gameObject.SetActive(true);
                statusEffectsVisuals[4].StartEffect();
            }   
            if (pm) 
                ui.UpdateStatusEffects(4);
        }
    }
    public void InstantGoldHunger()
    {
        if (statusEffects.Count > 5)
        {
            statusEffects[5].effectLevelCurrent = statusEffects[5].effectLevelMax;
            statusEffects[5].effectActive = true;
            if (statusEffectsVisuals.Count > 5)
            {
                statusEffectsVisuals[5].gameObject.SetActive(true);
                statusEffectsVisuals[5].StartEffect();
            }   
            if (pm) 
                ui.UpdateStatusEffects(5);
        }
    }
    public void InstantCold()
    {
        if (statusEffects.Count > 6)
        {
            statusEffects[6].effectLevelCurrent = statusEffects[6].effectLevelMax;
            statusEffects[6].effectActive = true;
            if (statusEffectsVisuals.Count > 6)
            {
                statusEffectsVisuals[6].gameObject.SetActive(true);
                statusEffectsVisuals[6].StartEffect();
            }
            if (pm) 
                ui.UpdateStatusEffects(6);
        }
    }
    public void InstantLove()
    {
        if (statusEffects.Count > 7)
        {
            statusEffects[7].effectLevelCurrent = statusEffects[7].effectLevelMax;
            statusEffects[7].effectActive = true;
            inLove = true;
            if (statusEffectsVisuals.Count > 7)
            {
                statusEffectsVisuals[7].gameObject.SetActive(true);
                statusEffectsVisuals[7].StartEffect();
            }   
            
            if (pm) 
                ui.UpdateStatusEffects(7);
        }
    }

    public void InstantRust()
    {
        if (statusEffects.Count > 3)
        {
            statusEffects[3].effectLevelCurrent = statusEffects[3].effectLevelMax;
            statusEffects[3].effectActive = true;
            if (statusEffectsVisuals.Count > 3)
            {
                statusEffectsVisuals[3].gameObject.SetActive(true);
                statusEffectsVisuals[3].StartEffect();
            }   
            if (pm) 
                ui.UpdateStatusEffects(3);
        }
    }
    
    void DepleteEffect(int i)
    {
        if (statusEffects.Count > i && health > 0)
        {
            if (statusEffects[i].effectLevelCurrent > 0)
            {
                if (i == 6) // cold
                {
                    if (pm)
                        pm.coldScaler = Mathf.Lerp(pm.coldScaler, 1, Time.deltaTime * 0.1f);
                    
                    statusEffects[i].effectLevelCurrent -= Time.deltaTime * statusEffects[i].depletionSpeed;
                }
                else if (i == 7) // love
                {
                    // THIS IS FOR MOBS ONLY
                    if (!player && inLove && psc.handsome)
                        statusEffects[i].effectLevelCurrent -= Time.deltaTime * statusEffects[i].depletionSpeed / 3;
                    else
                        statusEffects[i].effectLevelCurrent -= Time.deltaTime * statusEffects[i].depletionSpeed;
                }
                else
                    statusEffects[i].effectLevelCurrent -= Time.deltaTime * statusEffects[i].depletionSpeed;

                if (statusEffects[i].effectActive)
                {
                    switch (i)
                    {
                        case 0: // poison
                            if (statusEffects.Count > 3 && statusEffects[3].effectActive)
                            {
                                //  FILL RUST AND POISON
                                FillBuff(1);
                            }
                            break;
                        
                        case 1: // fire
                            if (statusEffects.Count > 2 && statusEffects[2].effectActive)
                            {
                                //  FILL FIRE AND BLOOD BUFF
                                FillBuff(0);
                            }
                            break;
                        
                        case 2: // bleed
                            
                            break;
                        
                        case 3: // rust
                            break;
                        
                        case 4: // regen
                            
                            break;
                        
                        case 5: // gold hunger
                            if (statusEffects.Count > 7 && statusEffects[7].effectActive)
                            {
                                //  FILL LOVE AND GOLD
                                FillBuff(2);
                            }
                            break;
                        
                        case 6: // bone shiver
                            
                            break;
                        
                        case 7: // love
                            
                            break;
                    }
                }
            }
            else if (statusEffects[i].effectActive)
            {
                statusEffects[i].effectActive = false;

                if (i == 1 && fireParticles)
                {
                    firePartilcesEmission.rateOverTime = 0;
                    fireAu.volume = 0;
                    fireAu.Stop();   
                }
                if (i == 6) // cold
                {
                    if (pm)
                        pm.coldScaler = 1;
                }
                if (i == 7) // love
                {
                    inLove = false;
                }
                
                if (statusEffectsVisuals.Count > i)
                    statusEffectsVisuals[i].DestroyEffect(false); 
            }   
        }
    }

    void FillBuff(int buffIndex)
    {
        switch (buffIndex)
        {
            case 0:
                // FIRE AND BLOOD
                if (fireAndBloodAmount < 100)
                {
                    fireAndBloodAmount += Time.deltaTime * 10;
                    ui.UpdateBuff(0, fireAndBloodAmount, fireAndBloodActive);
                }
                else // buff is full
                {
                    if (!fireAndBloodActive)
                    {
                        statusEffects[1].effectActive = false;
                        statusEffects[1].effectLevelCurrent = 0;
                        statusEffects[2].effectActive = false;
                        statusEffects[2].effectLevelCurrent = 0;

                        if (pm)
                        {
                            firePartilcesEmission.rateOverTime = 0;
                            fireAu.volume = 0;
                            fireAu.Stop();   
                        } 

                        inLove = false;
                        fireAndBloodActive = true;
                        if (fireAndBloodCoroutine == null)
                            fireAndBloodCoroutine = StartCoroutine(FireAndBlood());
                    }
                }
                break;
            
            case 1:
                // RUST AND POISON
                if (rustAndPoisonAmount < 100)
                {
                    rustAndPoisonAmount += Time.deltaTime * 10;
                    ui.UpdateBuff(1, rustAndPoisonAmount, rustAndPoisonActive);
                }
                else // buff is full
                {
                    if (!rustAndPoisonActive)
                    {
                        statusEffects[0].effectActive = false;
                        statusEffects[0].effectLevelCurrent = 0;
                        statusEffects[3].effectActive = false;
                        statusEffects[3].effectLevelCurrent = 0;

                        rustAndPoisonActive = true;
                        if (rustAndPoisonCoroutine == null)
                            rustAndPoisonCoroutine = StartCoroutine(RustAndPoison());
                    }
                }
                break;
            
            case 2:
                //  LOVE AND GOLD
                if (loveAndGoldAmount < 100)
                {
                    loveAndGoldAmount += Time.deltaTime * 10;
                    ui.UpdateBuff(2, loveAndGoldAmount, loveAndGoldActive);
                }
                else // buff is full
                {
                    if (!loveAndGoldActive)
                    {
                        statusEffects[5].effectActive = false;
                        statusEffects[5].effectLevelCurrent = 0;
                        statusEffects[7].effectActive = false;
                        statusEffects[7].effectLevelCurrent = 0;
                        inLove = false;
                        
                        loveAndGoldActive = true;
                        if (loveAndGoldCoroutine == null)
                            loveAndGoldCoroutine = StartCoroutine(LoveAndGold());
                    }
                }
                break;
        }
    }

    IEnumerator FireAndBlood()
    {
        while (fireAndBloodAmount > 0)
        {
            fireAndBloodAmount -= Time.deltaTime * 3.3f;
            ui.UpdateBuff(0, fireAndBloodAmount, fireAndBloodActive);
            yield return null;
        }

        fireAndBloodActive = false;
        fireAndBloodAmount = 0;
        ui.UpdateBuff(0, fireAndBloodAmount, fireAndBloodActive);
        fireAndBloodCoroutine = null;
    }
    IEnumerator RustAndPoison()
    {
        while (rustAndPoisonAmount > 0)
        {
            rustAndPoisonAmount -= Time.deltaTime * 3.3f;
            ui.UpdateBuff(1, rustAndPoisonAmount, rustAndPoisonActive);
            yield return null;
        }

        rustAndPoisonActive = false;
        rustAndPoisonAmount = 0;
        ui.UpdateBuff(0, rustAndPoisonAmount, rustAndPoisonActive);
        rustAndPoisonCoroutine = null;
    }

    IEnumerator LoveAndGold()
    {
        while (loveAndGoldAmount > 0)
        {
            loveAndGoldAmount -= Time.deltaTime * 3.3f;
            ui.UpdateBuff(2, loveAndGoldAmount, loveAndGoldActive);
            yield return null;
        }

        loveAndGoldActive = false;
        loveAndGoldAmount = 0;
        ui.UpdateBuff(0, loveAndGoldAmount, loveAndGoldActive);
        loveAndGoldCoroutine = null;
    }

    public void DamageOverTime(float dmg, string damager, string damageMessage)
    {
        if (invincible || gm.player == null || 
            (mobPartsController && (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                                && Vector3.Distance(transform.position, gm.player.transform.position) > 50))
            return;
        
        if (gameObject.activeInHierarchy)
        {
            if (pm || player)
            {
                damagedOnLevel = true;
                
                /*
                if (loveAndGoldActive)
                    dmg *= 0.5f;
                */
                
                if (gm.tutorialPassed == 0)
                    dmg *= 0.66f;
                
                if (health <= healthMax * 0.33f)
                    health -= (dmg - dmg * armor) * 0.5f;
                else if (health <= healthMax * 0.7f)
                    health -= (dmg - dmg * armor);
                else
                    health -= (dmg  - dmg * armor) * 1.5f;
            }
            else
                health -= dmg - dmg  * armor;
            
            if (mobAudio)
            {
                mobAudio.Damage();
            }
            if (deerActivator) deerActivator.Activate();
            
            if (pm)
            {
                if (damageMessage != null)
                    ui.DamagedMessage(damageMessage);
                il.playerCurrentHealth = health;
                if (health < healthMax * 0.33f)
                    ui.DamageFeedback();
                //PlayerSkillsController.instance.PlayerDamaged(dmg, null);
                ui.UpdateHealthbar();
            }
            
            if (bossStateChanger)
                bossStateChanger.BossDamaged();

            if (health <= 0)
            {
                if (mobAudio)
                {
                    mobAudio.Death();
                }
                
                if (wallBlockerController) wallBlockerController.Death();
                
                if (door)
                {
                    mobPartsController.Death();
                    door.DoorDestroyed();   
                }
                else if (pm)
                {
                    // first death achievement
                    if (!gm.demo && SteamAchievements.instance)
                        SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_15");
                    if (!gm.hub)
                    {
                        print("Player died!");
                        
                        if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                            pm.cameraAnimator.SetBool("Death", true);
                        else
                        {
                            print("Kill player dummy");
                            GLNetworkWrapper.instance.localPlayer.connectedDummy.DummyHealthChanged(-1000000000000, -1);   
                        }
                        
                        if (pac) pac.Damaged();   
                    }
                    else
                    {
                        health = 10;
                        return;
                    }
                }
                else
                {
                    // kill someone with a fire achievement
                    if (!gm.demo && statusEffects[1].effectActive && SteamAchievements.instance)
                        SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_12");
                }
                
                if (gameObject.activeInHierarchy)
                    Death(damager, false);
                //StartCoroutine(DeathOnClient(damager));
            }
        }
    }

    // called only on clients when units health is changed
    public void ChangeHealthOnClient(float offset, int effect, bool affectReputation)
    {
        if (offset < 0 && affectReputation)
            peaceful = false;
        
        health += offset;

        switch (effect)
            {
                case 0:
                    if (statusEffects.Count > 0)
                    {
                        statusEffects[0].effectLevelCurrent = statusEffects[0].effectLevelMax;
                        statusEffects[0].effectActive = true;
                            
                        GutQuestsController.instance.UnitPoisoned(this);
                        if (statusEffectsVisuals.Count > 1)
                        {
                            statusEffectsVisuals[0].gameObject.SetActive(true);
                            statusEffectsVisuals[0].StartEffect();
                        }
                    }

                    if (playerNetworkDummyController && playerNetworkDummyController.targetPlayer)
                        playerNetworkDummyController.targetPlayer.InstantPoison();

                    if (pm)
                        ui.UpdateStatusEffects(0);
                    break;
                
                case 1:
                    if (statusEffects.Count > 1)
                    {
                        statusEffects[1].effectLevelCurrent = statusEffects[1].effectLevelMax;
                        statusEffects[1].effectActive = true;
                            
                        GutQuestsController.instance.UnitOnFire(this);
                        if (statusEffectsVisuals.Count > 1)
                        {
                            statusEffectsVisuals[1].gameObject.SetActive(true);
                            statusEffectsVisuals[1].StartEffect();
                        }

                        StartFire();
                    }

                    if (playerNetworkDummyController && playerNetworkDummyController.targetPlayer)
                        playerNetworkDummyController.targetPlayer.StartFire();
                    if (pm)
                        ui.UpdateStatusEffects(1);
                    break;
                
                case 2:
                    if (statusEffects.Count > 2)
                    {
                        statusEffects[2].effectLevelCurrent = statusEffects[2].effectLevelMax;
                        statusEffects[2].effectActive = true;
                            
                        GutQuestsController.instance.UnitBleeding(this);
                        if (statusEffectsVisuals.Count > 2)
                        {
                            statusEffectsVisuals[2].gameObject.SetActive(true);
                            statusEffectsVisuals[2].StartEffect();
                        }
                    }
        
                    if (playerNetworkDummyController && playerNetworkDummyController.targetPlayer)
                        playerNetworkDummyController.targetPlayer.InstantBleed();
                    
                    if (pm)
                        ui.UpdateStatusEffects(2);
                    break;
                
                case 3:
                    if (statusEffects.Count > 3)
                    {
                        statusEffects[3].effectLevelCurrent = statusEffects[3].effectLevelMax;
                        statusEffects[3].effectActive = true;
                        if (statusEffectsVisuals.Count > 3)
                        {
                            statusEffectsVisuals[3].gameObject.SetActive(true);
                            statusEffectsVisuals[3].StartEffect();
                        }
                    }
        
                    if (playerNetworkDummyController && playerNetworkDummyController.targetPlayer)
                        playerNetworkDummyController.targetPlayer.InstantRust();

                    if (pm)
                        ui.UpdateStatusEffects(3);
                    break;
                
                case 4:
                    
                    if (statusEffects.Count > 4)
                    {
                        statusEffects[4].effectLevelCurrent = statusEffects[4].effectLevelMax;
                        statusEffects[4].effectActive = true;
                    }
        
                    if (playerNetworkDummyController && playerNetworkDummyController.targetPlayer)
                        playerNetworkDummyController.targetPlayer.InstantRegen();
                    if (pm)
                        ui.UpdateStatusEffects(4);
                    break;
                
                case 5:
                    if (statusEffects.Count > 5)
                    {
                        statusEffects[5].effectLevelCurrent = statusEffects[5].effectLevelMax;
                        statusEffects[5].effectActive = true;
                        if (statusEffectsVisuals.Count > 5)
                        {
                            statusEffectsVisuals[5].gameObject.SetActive(true);
                            statusEffectsVisuals[5].StartEffect();
                        }
                    }
        
                    if (playerNetworkDummyController && playerNetworkDummyController.targetPlayer)
                        playerNetworkDummyController.targetPlayer.InstantGoldHunger();

                    if (pm)
                        ui.UpdateStatusEffects(5);
                    break;
                case 6:
                    if (statusEffects.Count > 6)
                    {
                        statusEffects[6].effectLevelCurrent = statusEffects[6].effectLevelMax;
                        statusEffects[6].effectActive = true;
                        if (statusEffectsVisuals.Count > 6)
                        {
                            statusEffectsVisuals[6].gameObject.SetActive(true);
                            statusEffectsVisuals[6].StartEffect();
                        }
                    }
        
                    if (playerNetworkDummyController && playerNetworkDummyController.targetPlayer)
                        playerNetworkDummyController.targetPlayer.InstantCold();
                    if (pm)
                        ui.UpdateStatusEffects(6);
                    break;
                case 7:
                    if (statusEffects.Count > 7)
                    {
                        statusEffects[7].effectLevelCurrent = statusEffects[7].effectLevelMax;
                        statusEffects[7].effectActive = true;
                        if (statusEffectsVisuals.Count > 7)
                        {
                            statusEffectsVisuals[7].gameObject.SetActive(true);
                            statusEffectsVisuals[7].StartEffect();
                        }
                    }
        
                    if (playerNetworkDummyController && playerNetworkDummyController.targetPlayer)
                        playerNetworkDummyController.targetPlayer.InstantLove();
                    if (pm)
                        ui.UpdateStatusEffects(7);
                    break;
            }
        
        if (offset < 0) // DAMAGE
        {
            if (mobAudio) mobAudio.Damage();
            if (mobPartsController) mobPartsController.Damaged(mobPartsController.transform.position, mobPartsController.transform.position);
            if (mobGroundMovement)
            {
                HealthController damager = null;
                if (affectReputation)
                {
                    if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                    {
                        damager = PlayerMovement.instance.hc;
                    }
                    else
                    {
                        damager = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
                    }
                }
                mobGroundMovement.Damaged(damager);
            }
        }
        
        if (health <= 0 && gameObject.activeInHierarchy)
        {
            // die
            Death(null, affectReputation);
            //StartCoroutine(DeathOnClient(null));
        }
    }
    
    public void Damage(float dmg, Vector3 bloodPosition, Vector3 damageOrigin, 
        MobBodyPart part, string damageMessage, bool _damagedByPlayer, string damager, 
        HealthController damagerHc, StatussEffectsOnAttack effectsOnAttack, bool ignoreDistance)
    {
        if (pm && _damagedByPlayer && currentFaceEater != null)
        {
            currentFaceEater.hc.Damage(dmg, currentFaceEater.transform.position, currentFaceEater.transform.position,
                null, currentFaceEater.hc.damagedByPlayerMessage[gm.language], true, null, this, null, true);
        }

        HealthController targetPlayer = PlayerMovement.instance.hc;
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            targetPlayer = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
        }

        if (invincible || (mobPartsController && !ignoreDistance && (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false) && Vector3.Distance(transform.position, targetPlayer.transform.position) > 50))
            return;
        
        if (damageCooldown <= 0 && health > 0)
        {
            int effect = -1;
            if ((pm || player) && lastChance && health - dmg - dmg * armor <= 0 && health - dmg - dmg * armor >= -50f)
            {
                health = 50;
                lastChance = false;
            }
            else
            {
                if (pm || player)
                {
                    damagedOnLevel = true;
                    
                    if (health <= healthMax * 0.33f)
                        health -= (dmg - dmg * armor) * 0.66f; // light damage
                    else if (health <= healthMax * 0.66f)
                        health -= (dmg - dmg * armor); // normal damage
                    else 
                        health -= (dmg  - dmg * armor) * 2f;   
                    /*
                    if (loveAndGoldActive)
                        dmg *= 0.5f;
                    
                    wc = WeaponControls.instance;
                    
                    // scale damage
                    if (gm.tutorialPassed == 0)
                        dmg *= 0.5f;
                    else if (wc.activeWeapon && wc.activeWeapon.weapon == WeaponPickUp.Weapon.Shield && !wc.activeWeapon.broken && !wc.activeWeapon.hidden)
                    {
                        dmg *= 0.5f;
                        wc.activeWeapon.LoseDurability();
                    }
                
                    if (psc.horrorMode)
                    {
                        dmg *= 3;
                        health -= (dmg - dmg * armor); // normal damage
                    }
                    else
                    {
                        if (gm.difficultyLevel == GameManager.GameMode.StickyMeat)
                        {
                            if (health <= healthMax * 0.33f)
                                health -= (dmg - dmg * armor) * 0.33f;
                            else if (health <= healthMax * 0.5f)
                                health -= (dmg - dmg * armor) * 0.75f;
                            else 
                                health -= (dmg  - dmg * armor);
                        }
                        else if (gm.difficultyLevel == GameManager.GameMode.MeatZone)
                        {
                            if (health <= healthMax * 0.33f)
                                health -= (dmg - dmg * armor) * 0.66f; // light damage
                            else if (health <= healthMax * 0.66f)
                                health -= (dmg - dmg * armor); // normal damage
                            else 
                                health -= (dmg  - dmg * armor) * 2f;   
                        }
                    }*/
                }
                else
                {
                    if (PlayerSkillsController.instance.horrorMode)
                    {
                        dmg *= 3;
                    }
                    
                    if (boss && !_damagedByPlayer)
                        dmg *= 0.1f;
                    
                    if (mobGroundMovement && mobGroundMovement.monsterState == MobGroundMovement.State.Idle)
                        dmg *= 2;
                    
                    var d = dmg - dmg  * armor;

                    if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                    {
                        // UNIT DAMAGED ON SOLO
                        health -= d;   
                    }
                    else if (!playerNetworkDummyController)
                    {
                        if (effectsOnAttack != null && effectsOnAttack.effects.Count > 0)
                        {
                            switch (effectsOnAttack.effects[0].effectType)
                            {
                                case StatusEffects.StatusEffect.Poison:
                                    effect = 0;
                                    break;
                                case StatusEffects.StatusEffect.Fire:
                                    effect = 1;
                                    break;
                                case StatusEffects.StatusEffect.Bleed:
                                    effect = 2;
                                    break;
                                case StatusEffects.StatusEffect.Rust:
                                    effect = 3;
                                    break;
                                case StatusEffects.StatusEffect.HealthRegen:
                                    effect = 4;
                                    break;
                                case StatusEffects.StatusEffect.GoldHunger:
                                    effect = 5;
                                    break;
                                case StatusEffects.StatusEffect.Cold:
                                    effect = 6;
                                    break;
                                case StatusEffects.StatusEffect.InLove:
                                    effect = 7;
                                    break;
                            }
                        }
                        GLNetworkWrapper.instance.UnitHealthChanged(gameObject, -d, effect, _damagedByPlayer);
                    }
                }
            }
            
            if (bossStateChanger)
                bossStateChanger.BossDamaged();
            
            if (mobNeedleMovement && _damagedByPlayer) mobNeedleMovement.DamagedByPlayer();
            
            if (setEffectToTileOnDamage) setEffectToTileOnDamage.Damaged(this);
            
            if (deerActivator) deerActivator.Activate();

            if (health > 0 && effectsOnAttack != null && effectsOnAttack.effects.Count > 0 && effectsOnAttack.effectsValues.Count > 0)
            {
                //for (int i = 0; i < effectsOnAttack.effects.Count; i++)
                {
                    switch (effectsOnAttack.effects[0].effectType)
                    {
                        case StatusEffects.StatusEffect.Poison:
                            effect = 0;
                            if (statusEffects.Count > 0)
                            {
                                statusEffects[0].effectLevelCurrent += effectsOnAttack.effectsValues[0];
                                if (statusEffects[0].effectLevelCurrent >= statusEffects[0].effectLevelMax)
                                {
                                    statusEffects[0].effectLevelCurrent = statusEffects[0].effectLevelMax;
                                    statusEffects[0].effectActive = true;
                                    
                                    GutQuestsController.instance.UnitPoisoned(this);
                                    if (statusEffectsVisuals.Count > 1)
                                    {
                                        statusEffectsVisuals[0].gameObject.SetActive(true);
                                        statusEffectsVisuals[0].StartEffect();
                                    }
                                }
                            }

                            if (pm)
                                ui.UpdateStatusEffects(0);
                            break;
                        
                        case StatusEffects.StatusEffect.Fire:
                            effect = 1;
                            if (statusEffects.Count > 1)
                            {
                                statusEffects[1].effectLevelCurrent += effectsOnAttack.effectsValues[0];
                                if (statusEffects[1].effectLevelCurrent >= statusEffects[1].effectLevelMax)
                                {
                                    statusEffects[1].effectLevelCurrent = statusEffects[1].effectLevelMax;
                                    statusEffects[1].effectActive = true;
                                    
                                    GutQuestsController.instance.UnitOnFire(this);
                                    if (statusEffectsVisuals.Count > 1)
                                    {
                                        statusEffectsVisuals[1].gameObject.SetActive(true);
                                        statusEffectsVisuals[1].StartEffect();
                                    }

                                    StartFire();
                                }
                            }

                            if (pm)
                                ui.UpdateStatusEffects(1);
                            break;
                        
                        case StatusEffects.StatusEffect.Bleed:
                            effect = 2;
                            if (statusEffects.Count > 2)
                            {
                                statusEffects[2].effectLevelCurrent += effectsOnAttack.effectsValues[0];
                                if (statusEffects[2].effectLevelCurrent >= statusEffects[2].effectLevelMax)
                                {
                                    statusEffects[2].effectLevelCurrent = statusEffects[2].effectLevelMax;
                                    statusEffects[2].effectActive = true;
                                    
                                    GutQuestsController.instance.UnitBleeding(this);
                                    if (statusEffectsVisuals.Count > 2)
                                    {
                                        statusEffectsVisuals[2].gameObject.SetActive(true);
                                        statusEffectsVisuals[2].StartEffect();
                                    }
                                }
                            }

                            if (pm)
                                ui.UpdateStatusEffects(2);
                            break;
                        
                        case StatusEffects.StatusEffect.Rust:
                            effect = 3;
                            if (statusEffects.Count > 3)
                            {
                                statusEffects[3].effectLevelCurrent += effectsOnAttack.effectsValues[0];
                                if (statusEffects[3].effectLevelCurrent >= statusEffects[3].effectLevelMax)
                                {
                                    statusEffects[3].effectLevelCurrent = statusEffects[3].effectLevelMax;
                                    statusEffects[3].effectActive = true;
                                    if (statusEffectsVisuals.Count > 3)
                                    {
                                        statusEffectsVisuals[3].gameObject.SetActive(true);
                                        statusEffectsVisuals[3].StartEffect();
                                    }
                                }
                            }

                            if (pm)
                                ui.UpdateStatusEffects(3);
                            break;
                        
                        case StatusEffects.StatusEffect.HealthRegen:
                            effect = 4;
                            if (statusEffects.Count > 4)
                            {
                                statusEffects[4].effectLevelCurrent += effectsOnAttack.effectsValues[0];
                                if (statusEffects[4].effectLevelCurrent >= statusEffects[4].effectLevelMax)
                                {
                                    statusEffects[4].effectLevelCurrent = statusEffects[4].effectLevelMax;
                                    statusEffects[4].effectActive = true;
                                    /*
                                    statusEffectsVisuals[4].gameObject.SetActive(true);
                                    statusEffectsVisuals[4].StartEffect();
                                    */  
                                }
                            }
                            if (pm)
                                ui.UpdateStatusEffects(4);
                            break;
                        
                        case StatusEffects.StatusEffect.GoldHunger:
                            effect = 5;
                            if (statusEffects.Count > 5)
                            {
                                statusEffects[5].effectLevelCurrent += effectsOnAttack.effectsValues[0];
                                if (statusEffects[5].effectLevelCurrent >= statusEffects[5].effectLevelMax)
                                {
                                    statusEffects[5].effectLevelCurrent = statusEffects[5].effectLevelMax;
                                    statusEffects[5].effectActive = true;
                                    if (statusEffectsVisuals.Count > 5)
                                    {
                                        statusEffectsVisuals[5].gameObject.SetActive(true);
                                        statusEffectsVisuals[5].StartEffect();
                                    }
                                }
                            }

                            if (pm)
                                ui.UpdateStatusEffects(5);
                            break;
                        case StatusEffects.StatusEffect.Cold:
                            effect = 6;
                            if (statusEffects.Count > 6)
                            {
                                statusEffects[6].effectLevelCurrent += effectsOnAttack.effectsValues[0];
                                if (statusEffects[6].effectLevelCurrent >= statusEffects[6].effectLevelMax)
                                {
                                    statusEffects[6].effectLevelCurrent = statusEffects[6].effectLevelMax;
                                    statusEffects[6].effectActive = true;
                                    if (statusEffectsVisuals.Count > 6)
                                    {
                                        statusEffectsVisuals[6].gameObject.SetActive(true);
                                        statusEffectsVisuals[6].StartEffect();
                                    }
                                }   
                            }
                            if (pm)
                                ui.UpdateStatusEffects(6);
                            break;
                        case StatusEffects.StatusEffect.InLove:
                            effect = 7;
                            if (statusEffects.Count > 7)
                            {
                                statusEffects[7].effectLevelCurrent += effectsOnAttack.effectsValues[0];
                                if (statusEffects[7].effectLevelCurrent >= statusEffects[7].effectLevelMax)
                                {
                                    statusEffects[7].effectLevelCurrent = statusEffects[7].effectLevelMax;
                                    statusEffects[7].effectActive = true;
                                    if (statusEffectsVisuals.Count > 7)
                                    {
                                        statusEffectsVisuals[7].gameObject.SetActive(true);
                                        statusEffectsVisuals[7].StartEffect();
                                    }
                                }   
                            }
                            if (pm)
                                ui.UpdateStatusEffects(7);
                            break;
                    }
                }
            }
            
            if (pm)
            {
                il.playerCurrentHealth = health;
                //il.AddToBadReputation(-0.1f);
                ui.DamagedMessage(damageMessage);
                ui.UpdateHealthbar();
                PlayerSkillsController.instance.PlayerDamaged(dmg, effectsOnAttack);
                ui.DamageFeedback();
            }
            else if (_damagedByPlayer)
            {
                peaceful = false;
                damagedByPlayer = true;
                if (wall)
                {
                    ui = UiManager.instance;
                    ui.DamagedMessage(damageMessage);
                }
            }

            
            /*
            if (playerNetworkDummyController)
            {
                if (effect >= 0)
                {
                    // set effect to player
                    if (playerNetworkDummyController.targetPlayer)
                    {
                        switch (effect)
                        {
                            case 0:
                                playerNetworkDummyController.targetPlayer.InstantPoison();
                                break;
                            case 1:
                                playerNetworkDummyController.targetPlayer.StartFire();
                                break;
                            case 2:
                                playerNetworkDummyController.targetPlayer.InstantBleed();
                                break;
                            case 3:
                                playerNetworkDummyController.targetPlayer.InstantRust();
                                break;
                            case 4:
                                playerNetworkDummyController.targetPlayer.InstantRegen();
                                break;
                            case 5:
                                playerNetworkDummyController.targetPlayer.InstantGoldHunger();
                                break;
                            case 6:
                                playerNetworkDummyController.targetPlayer.InstantCold();
                                break;
                            case 7:
                                playerNetworkDummyController.targetPlayer.InstantLove();
                                break;
                        }
                    }
                }
                playerNetworkDummyController.DummyHealthChanged(-dmg, effect);   
            }
            */
            
            if (mobGroundMovement)
            {
                mobGroundMovement.Damaged(damagerHc);
            }

            if (npcInteractor)
            {
                npcInteractor.HideDialogue();
                //npcInteractor.gameObject.SetActive(false);
                if (gameObject.activeInHierarchy)
                    StartCoroutine(NpcInteractorSetActive());
            }

            if (wallTrap)
            {
                if (health > 0)
                    wallTrap.Damage();
                else
                    wallTrap.Death();
            }
            if (mobPartsController)
            {
                peaceful = false;
                mobPartsController.Damaged(bloodPosition, damageOrigin);
            }
            
            if (faceEaterBehaviour) faceEaterBehaviour.Damaged();
            
            if (mobJumperMovement)
            {
                mobJumperMovement.Damaged(damagerHc);
            }
            if (mobHideInCorners)
                mobHideInCorners.Damaged(damagerHc);

            if (pac) pac.Damaged();

            if (part && door)
            {
                door.Break(part);
            }

            if (health <= 0)
            {
                if (pm)
                {
                    if (damagerHc != null && damagerHc.eyeEater && il.lostAnEye == 0)
                    {
                        ui.LostAnEye();
                        il.LostAnEye();
                        health = 50;
                    }
                }
                else
                {
                    if (wallBlockerController) wallBlockerController.Death();
                    if (door && mobPartsController)
                        mobPartsController.Death();
                }

                // DEATH
                if (health <= 0 && gameObject.activeInHierarchy)
                    Death(damager, _damagedByPlayer);
            }
            else
            {
                if (mobAudio)
                {
                    mobAudio.Damage();
                }

                if (mobPartsController)
                {
                    if (mobPartsController.anim)
                        mobPartsController.anim.SetTrigger("Damaged");

                    if (part && Random.value > 0.5f)
                    {
                        mobPartsController.KillBodyPart(part);
                    }
                }

                damageCooldown = damageCooldownMax;
                if (damageCooldown > 0)
                    StartCoroutine(DamageCooldown());

                if (pm) // only for player
                {
                    if (Random.value > 0.1f)
                        pm.cameraAnimator.SetTrigger("Damage");
                    else
                        pm.cameraAnimator.SetTrigger("Earthquake");

                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    {
                        GLNetworkWrapper.instance.localPlayer.connectedDummy.CmdSetHealth(health, effect);
                    }
                }
            }
        }
    }

    IEnumerator NpcInteractorSetActive()
    {
        if (!mobGroundMovement && !mobJumperMovement && mobMeleeAttack == null && !mobHideInCorners) 
            yield return new WaitForSeconds(5);
        else
        {
            while (!peaceful)
            {
                if (npcInteractor)
                    npcInteractor.gameObject.SetActive(false);
                yield return new WaitForSeconds(1);    
            }
        }

        if (health > 0 && npcInteractor && npcInteractor.gameObject && peaceful)
        {
            if (npcInteractor.interactable && npcInteractor.interactable.weaponPickUp &&
                npcInteractor.interactable.weaponPickUp.weaponDataRandomier &&
                npcInteractor.interactable.weaponPickUp.weaponDataRandomier.dead)
                yield break;
                
            npcInteractor.gameObject.SetActive(true);
        }
    }

    public void Kill()
    {
        print("try to kill " + gameObject.name);
        Damage(healthMax, transform.position + Vector3.up * 1.5f,transform.position + Vector3.up * 1.5f,
            null, null, false, null,null,null,true);
    }
    
    IEnumerator DamageCooldown()
    {
        while (damageCooldown > 0)
        {
            damageCooldown -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        damageCooldown = 0;
    }

    public void Death(string damager, bool affectReputation)
    {
        //if (LevelGenerator.instance && LevelGenerator.instance.levelgenOnHost)
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.DeathOnClient(gameObject, damager, affectReputation);
        }
        else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            StartCoroutine(DeathOnClient(damager, affectReputation));
        }
    }
    
    public IEnumerator DeathOnClient(string damager, bool affectReputation)
    {
        print(names[gm.language] +  " with unit index" + gm.units.IndexOf(this) + " is dead is " + dead);

        if (dead)
        {
            print(names[gm.language] +  " with unit index" + gm.units.IndexOf(this) + " is dead is " + dead);
            yield break;
        }

        dead = true;

        if (propController)
        {
            if (propController.spawnedObject)
            {
                propController.spawnedObject.ReleaseItemWithExplosion();   
                propController.spawnedObject = null;   
            }   
            
        }
        
        if (boss)
        {
            ui.bossHpParent.SetActive(false);
            ui.bossNameText.text = "";
            var sa = SteamAchievements.instance;
            if (sa)
                sa.UnlockSteamAchievement(mobKilledAchievementID);
        }

        for (int i = deathParticlesMulti.Count - 1; i >= 0; i--)
        {
            if (deathParticlesMulti[i] == null)
            {
                deathParticlesMulti.RemoveAt(i);
                continue;
            }
            deathParticlesMulti[i].transform.parent = null;
            deathParticlesMulti[i].gameObject.SetActive(true);
            deathParticlesMulti[i].Play();
        }
        
        if (deathParticles)
        {
            deathParticles.transform.parent = null;
            deathParticles.gameObject.SetActive(true);
            deathParticles.Play();
            if (deathAudioSource) deathAudioSource.Play();
        }


        if (npcInteractor)
        {
            if (npcInteractor.interactable && npcInteractor.interactable.itemInsideMeatHole != null 
                && npcInteractor.interactable.itemInsideMeatHole.isActiveAndEnabled)
            {
                npcInteractor.interactable.itemInsideMeatHole.transform.position = mobPartsController.dropPosition.position;
                npcInteractor.interactable.itemInsideMeatHole.SpawnByMeatHole();
            }

            if (npcInteractor.roseNpc >= 0)
            {
                if (!gm.roseNpcsInteractedInHub.Contains(npcInteractor.roseNpc))
                    gm.roseNpcsInteractedInHub.Add(npcInteractor.roseNpc);
            }
            
            npcInteractor.HideDialogue();
        }

        if (fireParticles)
        {
            firePartilcesEmission.rateOverTime = 0;
            fireAu.volume = 0;
            fireAu.Stop();   
        }
        
        if (activeNpc) ActiveNpcManager.instance.NpcIsDead();

        print("mobparts and wallmaster: " + mobPartsController + " and " + wallMasterTile);
        if (mobPartsController)
        {
            if (wallMasterTile)
            {
                
                //ONLY ON SERVER OR IN SOLO
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive &&
                    LevelGenerator.instance.levelgenOnHost)
                {
                    print("[COOP] WallMasterTile.DestroyWall");
                    wallMasterTile.CreateTileBehindBrokenWall(this);
                }
                else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                {
                    wallMasterTile.CreateTileBehindBrokenWall(this);
                }
            }
            
            if (monsterTrapTrigger)
                monsterTrapTrigger.enabled = false;
            
            if (meatTrap) meatTrap.Death();

            if (ikarusMovement) ikarusMovement.Killed();
            
            if (mobAudio)
            {
                mobAudio.Damage();
                mobAudio.Death();
            }

            if (mobGroundMovement)
            {
                mobGroundMovement.agent.enabled = false;
                mobGroundMovement.Death();
            }

            if (mobMeleeAttack)
            {
                mobMeleeAttack.Death();
            }

            if (mobHideInCorners)
                mobHideInCorners.Death();

            if (npcInteractor)
            {
                if (npcInteractor.interactable && npcInteractor.interactable.weaponPickUp)
                {
                    npcInteractor.interactable.weaponPickUp.weaponDataRandomier.dead = true;
                    npcInteractor.interactable.weaponPickUp.weaponDataRandomier.UpdateDescription();
                }
                Destroy(npcInteractor.gameObject);
                
                /*
                if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                {
                    Destroy(npcInteractor.gameObject);
                }
                else
                {
                    var index = il.interactables.IndexOf(npcInteractor.interactable);
                    if (index >= 0)
                        GLNetworkWrapper.instance.DestroyInteractable(index);
                }
                */
            }
            else if (weaponPickUp)
            {
                weaponPickUp.weaponDataRandomier.dead = true;
                weaponPickUp.weaponDataRandomier.UpdateDescription();
            }


            if (door)
                door.DoorDestroyed();

            if (mobGroundMovement || mobMeleeAttack || mobHideInCorners || mobJumperMovement)
            {
                SpawnController.instance.mobsInGame.Remove(this);
                SpawnController.instance.mobsInGameStatic.Remove(this);
                sc.MobKilled();
            }

            print("Affect reputation: " + affectReputation);
            if (affectReputation)
                il.AddToBadReputation(addBadRepOnDeath);
            
            /*
            if (!pm && !player)
                gm.units.Remove(this);
                */
        }
        

        yield return new WaitForEndOfFrame();

        if (startQuestOnDeath >= 0)
        {
            QuestManager.instance.StartQuest(startQuestOnDeath);
        }

        if (pm || player)
        {
            Time.timeScale = 1;
            
            HubItemsSpawner.instance.StopAllCoroutines();
            if (pac) pac.Death();
            StartCoroutine(HubItemsSpawner.instance.RespawnPlayerAfterDeath());
        }
        else
        {
            print("FFFFFFFFFFFFFFFFFFFF");
            GutQuestsController.instance.UnitKilled(this);
            if (trapController)
                trapController.Death();

            RemoveUnitOnClient();
            
        }
    }

    public void RespawnPlayer()
    {
        pm.cameraAnimator.SetBool("Death", false);
        PlayerMovement.instance.hc.invincible = false;
        PlayerMovement.instance.controller.enabled = true;
        health = healthMax / 2;
        il.playerCurrentHealth = health;
        ui.UpdateHealthbar();
    }

    public void RemoveUnitOnClient()
    {
        print("UUUUUUUUUUUUU");
        if (mobPartsController)
            mobPartsController.Death();


        //print("REMOVE UNIT WITH INDEX OF " + gm.units.IndexOf(this));
        //gm.units.Remove(this);
        Destroy(gameObject, 5f);
    }

    public void PlayerGetGold()
    {
        statusEffects[5].effectLevelCurrent -= statusEffects[5].effectLevelMax / 5;
        if (statusEffects[5].effectLevelCurrent < 0)
        {
            statusEffects[5].effectActive = false;
            statusEffects[5].effectLevelCurrent = 0;
        }
    }
    
    public void Heal(float amount)
    {
        StopBleeding();
        if (amount < 0)
        {
            health += healthMax / 3;   
        }
        else
        {
            health += amount;
        }
        
        if (health > healthMax) health = healthMax;

        if (pm)
        {
            il.playerCurrentHealth = health;
            ui.UpdateHealthbar();
        }
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.connectedDummy.CmdSetHealth(health, -1);
        }
    }

    public void Antidote()
    {
        for (int i = 0; i < statusEffects.Count; i++)
        {
            statusEffects[i].effectActive = false;
            statusEffects[i].effectLevelCurrent = 0;
            
            /*
            statusEffects[i].effectLevelCurrent -= statusEffects[i].effectLevelMax;
            if (statusEffects[i].effectLevelCurrent <= 0)
            {
                statusEffects[i].effectActive = false;
                statusEffects[i].effectLevelCurrent = 0;
            }
            */   
        }

        if (pm)
        {
            firePartilcesEmission.rateOverTime = 0;
            fireAu.volume = 0;
            fireAu.Stop();   
        } 

        inLove = false;
    }

    public void Anticold()
    {
        if (statusEffects.Count > 6 && statusEffects[6].effectValue > 0)
        {
            statusEffects[6].effectLevelCurrent -= statusEffects[6].effectLevelMax;
            
            if (statusEffects[6].effectLevelCurrent < 0)
            {
                statusEffects[6].effectActive = false;
                statusEffects[6].effectLevelCurrent = 0;
            }   
        }
    }
    
    public void StopBleeding()
    {
        if (statusEffects.Count > 2 && statusEffects[2].effectLevelCurrent > 0)
        {
            statusEffects[2].effectLevelCurrent -= statusEffects[2].effectLevelMax;
            if (statusEffects[2].effectLevelCurrent < 0)
            {
                statusEffects[2].effectActive = false;
                statusEffects[2].effectLevelCurrent = 0;
            }   
        }
    }
    
    public void AddPoison(float amount)
    {
        statusEffects[0].effectLevelCurrent += amount;
        print(statusEffects[0].effectLevelCurrent);
        if (statusEffects[0].effectLevelCurrent >= statusEffects[0].effectLevelMax)
        {
            statusEffects[0].effectActive = true;
            statusEffects[0].effectLevelCurrent = statusEffects[0].effectLevelMax;
            GutQuestsController.instance.UnitPoisoned(this);
        }
    }
    
    public void AddFire(float amount)
    {
        statusEffects[1].effectLevelCurrent += amount;
        if (statusEffects[1].effectLevelCurrent >= statusEffects[1].effectLevelMax)
        {
            statusEffects[1].effectActive = true;
            statusEffects[1].effectLevelCurrent = statusEffects[1].effectLevelMax;
            GutQuestsController.instance.UnitOnFire(this);
            StartFire();
        }
    }

    public void AddBleed(float amount)
    {
        statusEffects[2].effectLevelCurrent += amount;
        if (statusEffects[2].effectLevelCurrent >= statusEffects[2].effectLevelMax)
        {
            statusEffects[2].effectActive = true;
            statusEffects[2].effectLevelCurrent = statusEffects[2].effectLevelMax;
            GutQuestsController.instance.UnitBleeding(this);
        }
    }
    
    public void AddRust(float amount)
    {
        statusEffects[3].effectLevelCurrent += amount;
        if (statusEffects[3].effectLevelCurrent >= statusEffects[3].effectLevelMax)
        {
            statusEffects[3].effectActive = true;
            statusEffects[3].effectLevelCurrent = statusEffects[3].effectLevelMax;
        }
    }
    
    public void AddRegen(float amount)
    {
        statusEffects[4].effectLevelCurrent += amount;
        if (statusEffects[4].effectLevelCurrent >= statusEffects[4].effectLevelMax)
        {
            statusEffects[4].effectActive = true;
            statusEffects[4].effectLevelCurrent = statusEffects[4].effectLevelMax;
        }
    }
    public void AddGoldHunger(float amount)
    {
        statusEffects[5].effectLevelCurrent += amount;
        if (statusEffects[5].effectLevelCurrent >= statusEffects[5].effectLevelMax)
        {
            statusEffects[5].effectActive = true;
            statusEffects[5].effectLevelCurrent = statusEffects[5].effectLevelMax;
        }
    }
    public void AddCold(float amount)
    {
        statusEffects[6].effectLevelCurrent += amount;
        if (statusEffects[6].effectLevelCurrent >= statusEffects[6].effectLevelMax)
        {
            statusEffects[6].effectActive = true;
            statusEffects[6].effectLevelCurrent = statusEffects[6].effectLevelMax;
        }
    }
    
    public void Antirust()
    {
        statusEffects[3].effectLevelCurrent -= statusEffects[3].effectLevelMax;
        if (statusEffects[3].effectLevelCurrent < 3)
        {
            statusEffects[3].effectActive = false;
            statusEffects[3].effectLevelCurrent = 0;
        }
        
        // repair weapon
        if (pm)
        {
            if (wc)
            {
                if (wc.activeWeapon)
                {
                    wc.activeWeapon.FixWeapon();
                }

                il.SaveWeapons();
                ui.UpdateWeapons();
            }   
        }
    }

    void HealOverTime(float amount)
    {
        StopBleeding();
        health += amount;
        if (health > healthMax) health = healthMax;
        if (pm)
        {
            ui.UpdateHealthbar();
            il.playerCurrentHealth = health;
        }
    }

    public void UpdateHealthbarOnLoad()
    {
        UiManager.instance.UpdateHealthbar();
    }

    private void OnDestroy()
    {
        //GutQuestsController.instance.UnitKilled(this);
        if (gm.units.Contains(this))
            gm.units.Remove(this);
        
        if (npcInteractor)
            Destroy(npcInteractor.gameObject);

        if (dynamicMapMark != null)
        {
            Destroy(dynamicMapMark.gameObject);
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.layer == 21) // tile
        {
            TileController newTile = coll.gameObject.GetComponent<TileController>();

            if (newTile)
            {
                usedTile = newTile;   
                if (pac)
                    pac.GetNewFootSteps(usedTile.floorType);
            }
        }
    }

    public void SpawnDynamicMapMark(int index)
    {
        DynamicMapMark newMark = Instantiate(sc.dynamicMapMarksPrefabs[index], transform.position, Quaternion.identity);
        newMark.master = this;
        dynamicMapMark = newMark;
    }
}

[Serializable]
public class StatusEffects
{
    public enum StatusEffect
    {
        Null,
        Poison,
        Fire,
        Bleed,
        Rust,
        HealthRegen,
        GoldHunger,
        Cold,
        InLove
    }

    public StatusEffect effectType = StatusEffect.Null;
    public float fillSpeed = 10f;
    public float depletionSpeed = 5;
    [Header("This value has various effects depending on effect type")]
    public float effectValue = 1;
    
    public bool effectImmune = false;
    public float effectLevelCurrent = 0;
    public float effectLevelMax = 100;
    public bool effectActive = false;
}

[Serializable]
public class StatussEffectsOnAttack
{
    public List<StatusEffects> effects = new List<StatusEffects>();
    public List<float> effectsValues = new List<float>();
}
