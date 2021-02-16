using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEditor;
using UnityEngine;

public class PlayerSkillsController : MonoBehaviour
{
    public enum  Cult
    {
        poisonCult, fireCult, bleedingCult, rustCult, regenCult, goldCult, coldCult, none
    }

    public Cult activeCult = Cult.none;
    
    public static PlayerSkillsController instance;

    //public List<SkillsData.Skill> playerSkills;
    public List<SkillInfo> playerSkills;

    private ItemsList il;
    private UiManager ui;
    private PlayerMovement pm;
    private GameManager gm;
    private LevelGenerator lg;
    private MouseLook ml;

    public Animator teleportAnim;

    public bool fireAttack = false;
    public bool randomTeleport = false;
    public bool dashAttack = false;
    public bool cheapDash = false;
    public bool fastStrafing = false;
    public bool usefulAmmo = false;
    public bool healthForGold = false;
    public bool ammoForHealth = false;
    public bool vertigo = false;
    public bool shootOnrun = false;
    public bool strongWeapon = false;
    public bool hallucinations = false;
    public bool mirrorDamage = false;
    public bool goldShooter = false;
    public bool nervousHunk = false;
    public bool pipeLover = false;
    public bool knifeLover = false;
    public bool axeLover = false;
    public bool pistolLover = false;
    public bool revolverLover = false;
    public bool shotgunLover = false;
    public bool cheapRun = false;
    public bool fastRun = false;
    public bool tommyLover = false;
    public bool dashOnAttack = false;
    public bool thirtyFingers = false;
    public bool knowTrapEffect = false;
    public bool handsome = false;
    public bool softFeet = false;
    public bool steelSpine = false;
    public bool dickheadStomach = false;
    public bool horrorMode = false;
    public bool noseWithTeeth = false;
    public bool slowMoDash = false;
    public bool wallEater = false;
    public bool traitor = false;

    public float secondsToNextTeleport = 0;
    
    private Coroutine magnetoCoroutine;
    private Coroutine vertigoCoroutine;
    private Coroutine teleportCoroutine;
    private Coroutine milkCoroutine;
    private Coroutine slowMoDashCoroutine;

    private Coroutine pauseVertigoCoroutine;
    private Coroutine damageEveryoneCoroutine;
    string teleportString = "Teleport";    

    private SpawnController sc;

    public AudioSource slowMoAu;
    public List<AudioClip> slowMoClips = new List<AudioClip>();
    
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        ui = UiManager.instance;
        ml = MouseLook.instance;
        lg = LevelGenerator.instance;
        il = ItemsList.instance;
        sc = SpawnController.instance;
        
        pm.staminaStats.runRegenCurrent = pm.staminaStats.runRegen;
        pm.staminaStats.dashCostCurrent = pm.staminaStats.dashCost;
        pm.movementStats.runSpeedBonusCurrent = pm.movementStats.runSpeedBonus;
    }

    public void AddSkill(SkillInfo chosenSkill)
    {
        if (!playerSkills.Contains(chosenSkill))
        {
            // add random skill
            if (chosenSkill != null)
            {
                playerSkills.Add(chosenSkill);
                il.skillsPool.Remove(chosenSkill);   
            }
            
            StartSkillsAction();
    
            il.UpdateSkills();   
            ui.UpdateJournalIcon();   
        }
    }

    public void RemoveSkill(SkillInfo skill)
    {
        if (playerSkills.Contains(skill))
        {
            // add random skill
            if (skill != null)
            {
                playerSkills.Remove(skill);
                il.skillsPool.Add(skill);   
            }
            
            StartSkillsAction();
    
            il.UpdateSkills();   
            ui.UpdateJournalIcon();   
        }
    }

    public void StartSkillsAction()
    {
        //StopAllCoroutines();

        ResetSkills();
        il = ItemsList.instance;
        pm = PlayerMovement.instance;
        gm = GameManager.instance;

        if (il.savedQuestItems.Contains(5))
            pm.hc.armor = 0.25f;
        else
            pm.hc.armor = 0f;
        
        if (!GameManager.instance.paused && GameManager.instance.mementoWindow)
            Time.timeScale = 1;
    
        foreach (var skill in playerSkills)
        {
            switch (skill.skill)
            {
                case SkillsData.Skill.RunCostLower:
                    cheapRun = true;
                    pm.staminaStats.runRegenCurrent = pm.staminaStats.boostedRunRegen;
                    break;
                
                case SkillsData.Skill.DashCostLower:
                    cheapDash = true;
                    pm.staminaStats.dashCostCurrent = pm.staminaStats.boostedDashCost;
                    break;
                
                case  SkillsData.Skill.RunSpeedBoost:
                    fastRun = true;
                    pm.movementStats.runSpeedBonusCurrent = pm.movementStats.runSpeedBonusBoosted;
                    break;
                
                case SkillsData.Skill.Hallucinations:
                    hallucinations = true;
                    if (milkCoroutine == null)
                        milkCoroutine = StartCoroutine(Hallucinate());
                    break;
                
                case SkillsData.Skill.FireAttack:
                    fireAttack = true;
                    break;
                
                case SkillsData.Skill.ResourceMagneto:
                    if (magnetoCoroutine == null)
                        magnetoCoroutine = StartCoroutine(ResourceMagneto());
                    break;
                
                case SkillsData.Skill.RandomTeleport:
                    randomTeleport = true;
                    if (teleportCoroutine == null)
                        teleportCoroutine = StartCoroutine(RandomTeleport());   
                    break;
                
                case SkillsData.Skill.Vertigo:
                    vertigo = true;
                    if(vertigoCoroutine == null)
                        vertigoCoroutine = StartCoroutine(Vertigo());
                    break;
                
                case SkillsData.Skill.HpUp:
                    pm.hc.armor = 0.33f;
                    
                    if (il.savedQuestItems.Contains(5))
                        pm.hc.armor = 0.6f;
                    break;
                
                case  SkillsData.Skill.DashAttack:
                    dashAttack = true;
                    break;
                
                case SkillsData.Skill.FastStraifing:
                    fastStrafing = true;
                    break;
                
                case SkillsData.Skill.ShootOnRun:
                    shootOnrun = true;
                    break;
                
                case SkillsData.Skill.StrongWeapon:
                    strongWeapon = true;
                    break;
                
                case SkillsData.Skill.AmmoForHealth:
                    ammoForHealth = true;
                break;
                    
                case SkillsData.Skill.HealthForGold:
                    healthForGold = true;        
                    break;
                
                case SkillsData.Skill.MirrorDamage:
                    mirrorDamage = true;        
                    break;
                
                case SkillsData.Skill.GoldShooter:
                    goldShooter = true;
                    break;
                
                case SkillsData.Skill.Hunk:
                    nervousHunk = true;
                    break;
                
                case SkillsData.Skill.PipeLover:
                    pipeLover = true;
                    break;
                
                case SkillsData.Skill.KnifeLover:
                    knifeLover = true;
                    break;
                
                case SkillsData.Skill.AxeLover:
                    axeLover = true;
                    break;
                
                case SkillsData.Skill.PistolLover:
                    pistolLover = true;
                    break;
                
                case SkillsData.Skill.RevolverLover:
                    revolverLover = true;
                    break;
                
                case SkillsData.Skill.ShotgunLover:
                    shotgunLover = true;
                    break;
                
                case SkillsData.Skill.TommyLover:
                    tommyLover = true;
                    break;
                
                case SkillsData.Skill.DashOnAttack:
                    dashOnAttack = true;
                    break;
                
                case SkillsData.Skill.ThirtyFingers:
                    thirtyFingers = true;
                    break;
                
                case SkillsData.Skill.NoseWithTeeth:
                    noseWithTeeth = true;
                    WeaponControls.instance.noseWithTeeth.gameObject.SetActive(true);
                    break;
                
                case SkillsData.Skill.TrapLover:
                    knowTrapEffect = true;
                    break;
                
                case SkillsData.Skill.Handsome:
                    handsome = true;
                    break;
                
                case SkillsData.Skill.SoftFeet:
                    softFeet = true;
                    break;
                
                case SkillsData.Skill.SteelSpine:
                    steelSpine = true;
                    break;
                
                case SkillsData.Skill.DickheadStomach:
                    dickheadStomach = true;
                    break;
                
                case SkillsData.Skill.HorrorMode:
                    horrorMode = true;
                    if (!gm.hub)
                        RenderSettings.fogEndDistance = 10;
                    break;
                
                case SkillsData.Skill.SlowMoDash:
                    slowMoDash = true;
                    break;
                
                case SkillsData.Skill.WallEater:
                    wallEater = true;
                    break;
                
                case SkillsData.Skill.Traitor:
                    traitor = true;
                    break;
            }
        }
        
        // cult effects
        switch (activeCult)
        {
            case Cult.poisonCult:
                pm.hc.statusEffects[0].effectImmune = true;
                pm.hc.setEffectToTileOnDamage.effectOnTileOnDamage = StatusEffects.StatusEffect.Poison;
                break;
            case Cult.fireCult:
                pm.hc.statusEffects[1].effectImmune = true;
                pm.hc.setEffectToTileOnDamage.effectOnTileOnDamage = StatusEffects.StatusEffect.Fire;
                break;
            case Cult.bleedingCult:
                pm.hc.statusEffects[2].effectImmune = true;
                pm.hc.setEffectToTileOnDamage.effectOnTileOnDamage = StatusEffects.StatusEffect.Bleed;
                break;
             case Cult.goldCult:
                pm.hc.statusEffects[5].effectImmune = true;
                pm.hc.setEffectToTileOnDamage.effectOnTileOnDamage = StatusEffects.StatusEffect.GoldHunger;
                break;
        }
        
        // athletic achievement
        if (fastRun && cheapRun && cheapDash && !gm.demo)
            SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_2_8");
    }

    void ResetSkills()
    {
         fireAttack = false;
         randomTeleport = false;
         dashAttack = false;
         cheapDash = false;
         fastStrafing = false;
         usefulAmmo = false;
         healthForGold = false;
         ammoForHealth = false;
         vertigo = false;
         shootOnrun = false;
         strongWeapon = false;
         hallucinations = false;
         mirrorDamage = false;
         goldShooter = false;
         nervousHunk = false;
         pipeLover = false;
         knifeLover = false;
         axeLover = false;
         pistolLover = false;
         revolverLover = false;
         shotgunLover = false;
         cheapRun = false;
         fastRun = false;
         tommyLover = false;
         dashOnAttack = false;
         thirtyFingers = false;
         knowTrapEffect = false;
         handsome = false;
         softFeet = false;
         steelSpine = false;
         dickheadStomach = false;
         horrorMode = false;
         slowMoDash = false;
         noseWithTeeth = false;
    }

    public void PlayerDamaged(float dmg, StatussEffectsOnAttack effectsOnAttack)
    {
        if (ammoForHealth)
        {
            if (il.activeWeapon != null)
            {
                if (!AddAmmoForHealth(il.activeWeapon, dmg) && il.secondWeapon != null)
                {
                    AddAmmoForHealth(il.secondWeapon, dmg);
                }
            }
        }

        /*
        if (vertigo)
        {
            if (pauseVertigoCoroutine != null)
                StopCoroutine(pauseVertigoCoroutine);

            pauseVertigoCoroutine = StartCoroutine(PauseVertigoCoroutine());
        }
        */

        if (mirrorDamage)
        {
            if (damageEveryoneCoroutine == null)
                damageEveryoneCoroutine = StartCoroutine(DamageEveryone(dmg, effectsOnAttack));
        }
    }

    IEnumerator DamageEveryone(float dmg, StatussEffectsOnAttack effectsOnAttack)
    {
        for (int i = gm.units.Count - 1; i > 0; i--)
        {
            if (gm.units.Count <= i)
                break;
            
            if (gm.units[i] != pm.hc && Vector3.Distance(gm.units[i].transform.position, pm.transform.position) < 30)
            {
                if (effectsOnAttack != null)
                    gm.units[i].Damage(dmg,gm.units[i].transform.position, 
                        gm.units[i].transform.position, null, null, true, null, pm.hc, effectsOnAttack, false);
                else
                    gm.units[i].Damage(dmg,gm.units[i].transform.position, 
                        gm.units[i].transform.position, null, null, true, null, pm.hc, null, false);
            }
            yield return new WaitForSeconds(0.1f);
        }

        damageEveryoneCoroutine = null;
    }

    public void PlayerDashed()
    {
        if (slowMoDash && pm.staminaStats.CurrentValue >= pm.staminaStats.MaxValue * 0.33f)
        {
            if (slowMoDashCoroutine != null)
                StopCoroutine(slowMoDashCoroutine);
            else
            {
                slowMoAu.clip = slowMoClips[0];
                slowMoAu.pitch = Random.Range(0.8f, 1.15f);
                slowMoAu.Play();   
            }
            
            slowMoDashCoroutine = StartCoroutine(SlowMoOnDash());
        }
    }

    IEnumerator SlowMoOnDash()
    {
        while (pm.movementStats.movementState == MovementState.Dashing)
        {
            if (!gm.paused && !gm.mementoWindow)
            {
                ui.SetSharpen(true);
                Time.timeScale = Mathf.Lerp(Time.timeScale, 0.5f, pm._dashTimeCurrent / pm.movementStats.dashTime);   
                gm.mixer.SetFloat("MasterPitch",  Mathf.Lerp(Time.timeScale, 0.5f, pm._dashTimeCurrent / pm.movementStats.dashTime));
            }
            
            yield return null;
        }

        float t = 0;
        float tt = 0.5f;
        
        slowMoAu.clip = slowMoClips[1];
        slowMoAu.pitch = Random.Range(0.8f, 1.15f);
        slowMoAu.Play();
        
        while (t < tt)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, t/tt);  
            ui.SetSharpen(false);
            gm.mixer.SetFloat("MasterPitch",  Mathf.Lerp(Time.timeScale, 1, t/tt));
            
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = 1;               
        ui.SetSharpen(false);
        gm.mixer.SetFloat("MasterPitch", 1);
        
        StopCoroutine(slowMoDashCoroutine);
    }

    IEnumerator PauseVertigoCoroutine()
    {
        vertigo = false;

        yield return new WaitForSeconds(10);
        
        vertigo = true;
        pauseVertigoCoroutine = null;
    }

    bool AddAmmoForHealth(WeaponPickUp.Weapon weapon , float dmg)
    {
        bool found = false;
        int step = 0;
                
        switch (weapon)
        {
            case WeaponPickUp.Weapon.Pistol:
                found = true;
                
                il.ammoDataStorage.AddAmmo(weapon, dmg / 50);
                ui.UpdateAmmo();
                break;
                    
            case WeaponPickUp.Weapon.Revolver:
                found = true;
                il.ammoDataStorage.AddAmmo(weapon, dmg / 75);
                ui.UpdateAmmo();
                break;
                    
            case WeaponPickUp.Weapon.Shotgun:
                found = true;
                il.ammoDataStorage.AddAmmo(weapon, dmg / 100);
                ui.UpdateAmmo();
                break;
                    
            case WeaponPickUp.Weapon.TommyGun:
                found = true;
                il.ammoDataStorage.AddAmmo(weapon, dmg / 50);
                ui.UpdateAmmo();
                break;
        }

        /*
        if (found)
        {
            for (int i = 0; i < newDmg; i += step)
            {
                il.ammoDataStorage.AddAmmo(weapon, 1);
                ui.UpdateAmmo();
            }   
        }
*/
        return found;
    }

    public void PlayerLoseGold(float amount)
    {
        if (healthForGold)
        {
            pm.hc.Heal(amount * 10);
        }
    }
    
    IEnumerator Vertigo()
    {
        while (true)
        {
            float v = Random.Range(15, 45);
            if (vertigo)
                gm.mouseLookSpeedCurrent = Random.Range(gm.mouseLookSpeed / 3, gm.mouseLookSpeed);
            else
                gm.mouseLookSpeedCurrent = gm.mouseLookSpeed;
            
            // change FOV
            float t = 0;
            float targetAim = Random.Range(20f, 40f);
            float targetIdle = Random.Range(50f, 120f);
            
            while (t < v)
            {
                if (ml)
                {
                    if (vertigo)
                    {
                        ml.cameraFovAim = Mathf.Lerp(ml.cameraFovAim, targetAim, t / v);
                        ml.cameraFovIdle = Mathf.Lerp(ml.cameraFovIdle, targetIdle, t / v);   
                    }
                    else
                    {
                        ml.cameraFovAim = ml.cameraFovAimInit;
                        ml.cameraFovIdle = ml.cameraFovIdleInit;
                    }   
                }
                t += Time.deltaTime;
                yield return  new WaitForEndOfFrame();
            }
        }    
    }

    public void InstantTeleport(Vector3 pos)
    {
        StartCoroutine(TeleportOnce(pos));
    }

    IEnumerator TeleportOnce(Vector3 pos)
    {
        Vector3 finalPos = pos;

        if (!gm.hub && Vector3.Distance(finalPos, PlayerMovement.instance.transform.position) < 2)
        {
            // choose random tile
            
            List<TileController> tempTiles = new List<TileController>();
            foreach (var tile in lg.levelTilesInGame)
            {
                if (!tile.trapTile && tile.propOnTile == null)
                {
                    float newDist = Vector3.Distance(pm.transform.position, tile.transform.position);

                    Vector3 elevatorPos = Vector3.zero;
                    if (ElevatorController.instance != null)
                        elevatorPos = ElevatorController.instance.transform.position;
                    
                    if (newDist > 20f && Vector3.Distance(tile.transform.position, elevatorPos) > 20)
                    {
                        foreach (Interactable item in il.interactables)
                        {
                            if (item.pickUp || item.weaponPickUp || item.npc)
                            {
                                if (Vector3.Distance(item.transform.position, tile.transform.position) < 30)
                                {
                                    tempTiles.Add(tile);
                                    break;
                                }
                            }
                        }        
                    }   
                }
            }
            
            if (tempTiles.Count > 0)
            {
                // mole hand
                //teleportAnim.SetTrigger(teleportString);
                yield return new WaitForSeconds(0.5f);

                //Time.timeScale = 0;
                int r = Random.Range(0, tempTiles.Count);

                finalPos = tempTiles[r].transform.position;
            }   
        }

        StartCoroutine(TeleportToPosition(finalPos));
    }

    IEnumerator TeleportToPosition(Vector3 pos)
    {
        pm = PlayerMovement.instance;
        
        pm.Teleport(true);
                
        pm.transform.position = pos;
        gm.ls.PlayerTeleported();
        pm.playerHead.position = pm.transform.position;
        while (gm.paused)
        {
            yield return new WaitForEndOfFrame();   
        }
            
        pm.transform.position = pos;
        pm.playerHead.position = pm.transform.position;
        pm.Teleport(false);
    }
    
    IEnumerator RandomTeleport()
    {
        while (true)
        {
            secondsToNextTeleport = Random.Range(130f, 300f);
            while (secondsToNextTeleport > 0)
            {
                yield return new WaitForSeconds(1);
                secondsToNextTeleport--;
//                print(secondsToNextTeleport + "seconds to teleport");
            }
            
            if (!gm.lg.generating)
                StartCoroutine(TeleportOnce(PlayerMovement.instance.transform.position));
        }

        teleportCoroutine = null;
    }
    
    IEnumerator ResourceMagneto()
    {
        while (true)
        {
            if (lg == null) lg = LevelGenerator.instance;
            
            if (!lg.generating)
            {
                for (var index = 0; index < il.interactables.Count; index++)
                {
                    Interactable item = il.interactables[index];
                    if (item.pickUp || item.ammoPickUp)
                    {
                        float newDist = Vector3.Distance(transform.position, item.transform.position);
                        if (newDist < 30f && newDist > 5f)
                        {
                            //item.rb.useGravity = false;
                            Vector3 force = (pm.cameraAnimator.transform.position - item.transform.position).normalized * 4f + Vector3.up;
                            item.rb.AddForce(force, ForceMode.Impulse);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator Hallucinate()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(45f, 120f));
            SwapProps();
        }
    }

    public void SwapProps()
    {
        if (!gm.hub && !gm.lg.generating)
        {
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                GLNetworkWrapper.instance.SwapProps();
            }
            else
                SwapPropsOnServer();
        }
    }

    public void SwapPropsOnServer()
    {
        for (int i = lg.propsInGame.Count - 1; i >= 0; i--)
        {
            if (lg.propsInGame[i] == null)
            {
                lg.propsInGame.RemoveAt(i);
                continue;
            }
                
            float distance = Vector3.Distance(transform.position, lg.propsInGame[i].transform.position);
                
            if (Random.value > 0.5f && distance > 6.5f && distance < 40f)
            {
                lg.propsInGame[i].SwapRandomProp();
            }
        }

        for (int i = 0; i < sc.mobsInGame.Count; i++)
        {
            if (sc.mobsInGame[i].mobGroundMovement)
            {
                sc.mobsInGame[i].mobGroundMovement.Hide(true);
            }
        }
    }
    
    public void KissHer()
    {
        // remove eye patch
        if (il.lostAnEye == 1)
        {
            ui.GotEye();
            il.lostAnEye = 0;
        }
        // random kiss effect
        float r = Random.Range(0, 100);

        pm.hc.Heal(pm.hc.healthMax / 3f);
        
        if (pm.hc.health < pm.hc.healthMax * 0.5f || r > 66)
        {
            SpawnDynamicMobs(Random.Range(0, 3));
            SpawnDynamicProps(Random.Range(0, 3));
        }
        else if (r > 33)
        {
            // spawn human props
            SpawnDynamicProps(Random.Range(5, 10));
            SpawnDynamicMobs(Random.Range(0, 3));
        }
        else
        {
            // spawn mobs
            SpawnDynamicProps(Random.Range(0, 3));
            SpawnDynamicMobs(Random.Range(5, 10));
        }
    }

    void SpawnDynamicMobs(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            DynamicObstaclesManager.instance.SpawnMobAround();
        }
    }

    void SpawnDynamicProps(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            DynamicObstaclesManager.instance.SpawnPropAround();
        }
    }
    
    public void SetCult(Cult cult)
    {
        activeCult = cult;
        il.activeCult = activeCult;
        StartSkillsAction();
    }
}