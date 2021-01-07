using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    public enum ToolType {Heal, Antidote, Antirust, AttrackMonsters, Fire, Regen, Poison, Rust, 
                            Grenade, Null, Love, Bleed, GoldHunger, BoneShiver, Insight}
    public ToolType type = ToolType.Heal;
    public int toolIndex = 0;
    public float throwPower = 30;
    private PlayerMovement pm;
    private GameManager gm;
    private UiManager ui;
    private ItemsList il;
    private SpawnController sc;
    public Collider _collider;
    public EffectVisualController grenadeExplosion;

    public EffectVisualController deathParticles;

    private int spreadSize = 0;
    private float spreadDelay = 0;
    private float lifeTime = 0;
    private bool thrown = false;

    public Rigidbody rb;
    public bool forceKnown = false;
    
    void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        ui = UiManager.instance;
        il = ItemsList.instance;
        sc = SpawnController.instance;
    }

    public void UseTool()
    {
        float dickheadStomachScaler = 1;
        if (PlayerSkillsController.instance.dickheadStomach) dickheadStomachScaler = 1.7f;
        
        switch (type)
        {
            case ToolType.Heal:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                pm.hc.Heal((pm.hc.healthMax / 3) * dickheadStomachScaler);
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.Antidote:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                pm.hc.Antidote();
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.Poison:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                pm.hc.InstantPoison();
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                if (!gm.demo && SteamAchievements.instance)
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_6");
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.Regen:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                pm.hc.InstantRegen();
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.Fire:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                pm.hc.StartFire();
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                if (!gm.demo && SteamAchievements.instance)
                    SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_7");
                
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.Rust:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                pm.hc.InstantRust();
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.Bleed:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                pm.hc.InstantBleed();
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.BoneShiver:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                pm.hc.InstantCold();
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.GoldHunger:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                pm.hc.InstantGoldHunger();
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.Antirust:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                pm.hc.Antirust();
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case  ToolType.AttrackMonsters:
                if (!ui.playerHealed)
                    ui.playerHealed = true;

                if (!gm.hub)
                {
                    //StartCoroutine(sc.SpawnMobOnClientCoroutine());   
                    sc.SpawnMob();
                }
                
                sc.AttractMonsters(pm.transform.position, true);
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.Grenade:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                        
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    GLNetworkWrapper.instance.CreateExplosion(transform.position);
                else
                {
                    var newGrenade = Instantiate(grenadeExplosion, pm.transform.position, Quaternion.identity);
                    newGrenade.DestroyEffect(true);
                }
                
                //pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                //PlayerAudioController.instance.PlayHeal();
                break;
            
            case ToolType.Love:
                if (!ui.playerHealed)
                    ui.playerHealed = true;
                // player can not attack
                pm.hc.InstantLove();
                pm.hc.Heal((pm.hc.healthMax / 6) * dickheadStomachScaler);
                PlayerAudioController.instance.PlayHeal();
                break;
            
             case ToolType.Insight:
                 MouseLook.instance.AddDebugMapTime(45f);
                 break;
        }
        
        il.savedTools[toolIndex].amount--;
        il.savedTools[toolIndex].known = true;


        StartCoroutine(UpdateTools(0.5f, toolIndex));
    }

    IEnumerator UpdateTools(float t, int toolIndex)
    {
        yield return new WaitForSeconds(t);
        
        if (il.savedTools[toolIndex].amount <= 0)
            pm.hc.wc.FindNewTool();
        else
            ui.UpdateTools();
    }

    public void ThrowTool()
    {
        pm = PlayerMovement.instance;
        ToolController toolProjectile = Instantiate(this, transform.position, transform.rotation);
        //Vector3 newPos = new Vector3(pm.transform.position.x, pm.cameraAnimator.transform.position.y, pm.transform.position.z) + pm.cameraAnimator.transform.forward;
        Vector3 newPos = pm.cameraAnimator.transform.position + pm.cameraAnimator.transform.forward;
        toolProjectile.transform.position = newPos;
        
        toolProjectile.gameObject.layer = 22;
        toolProjectile.transform.localScale = Vector3.one;
        
        toolProjectile.gameObject.SetActive(true);
        
        foreach (Transform t in toolProjectile.transform)
        {
            t.gameObject.layer = 22;
        }
        toolProjectile.rb.isKinematic = false;
        toolProjectile.ThrowProjectile(pm);
        
        il.savedTools[toolIndex].amount--;
        il.savedTools[toolIndex].known = true;
        
        StartCoroutine(UpdateTools(0.7f, toolIndex));
        //gameObject.SetActive(false);
    }

    public void ThrowProjectile(PlayerMovement player)
    {
        thrown = true;
        _collider.isTrigger = true;
        rb.AddForce(player.cameraAnimator.transform.forward * throwPower, ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider coll)
    {
        if (thrown && !rb.isKinematic && coll.gameObject != pm.gameObject)
        {
            thrown = false;
            if (!gm.hub)
            {
                // find closest tile
                LevelGenerator lg = LevelGenerator.instance;
                TileController closestTile = null;
                float newDistance = 100;
                
                foreach (var t in lg.levelTilesInGame)
                {
                    float dist = Vector3.Distance(transform.position, t.transform.position);
                    if (dist <= newDistance)
                    {
                        newDistance = dist;
                        closestTile = t;
                        if (newDistance <= 5)
                            break;
                    }
                }

                if (closestTile != null)
                {
                    if (type == ToolType.Poison)
                    {
                        spreadSize = 3;
                        spreadDelay = 0.5f;
                        lifeTime = 20;
                    }
                    else if (type == ToolType.Fire)
                    {
                        spreadSize = 5;
                        spreadDelay = 1;
                        lifeTime = 15;
                    }
                    else if (type == ToolType.Rust)
                    {
                        spreadSize = 4;
                        spreadDelay = 2;
                        lifeTime = 15;
                        sc.MobHearNoise(transform.position, 20);
                    }
                    else if (type == ToolType.Regen)
                    {
                        spreadSize = 0;
                        spreadDelay = 0;
                        lifeTime = 30000;
                        sc.MobHearNoise(transform.position, 20);
                    }
                    else if (type == ToolType.Antidote) // clean tiles for now
                    {
                        spreadSize = 3;
                        spreadDelay = 1;
                        lifeTime = 3;
                        sc.MobHearNoise(transform.position, 20);
                    }
                    else if (type == ToolType.Antirust) // clean tiles for now
                    {
                        spreadSize = 5;
                        spreadDelay = 1;
                        lifeTime = 30;
                        sc.MobHearNoise(transform.position, 20);
                    }
                    else if (type == ToolType.Heal) // clean tiles for now
                    {
                        var newMob = GetMonsterInRange(15);
                        if (newMob) newMob.Heal(newMob.healthMax * 0.25f);

                        spreadSize = 2;
                        spreadDelay = 1;
                        lifeTime = 30;
                        
                        sc.MobHearNoise(transform.position, 20);
                    }
                    else if (type == ToolType.AttrackMonsters)
                    {
                        spreadSize = 0;
                        spreadDelay = 0;
                        lifeTime = 0;
                        //StartCoroutine(sc.SpawnMobOnClientCoroutine());   
                        sc.SpawnMob();
                        sc.AttractMonsters(closestTile.transform.position, true);
                    }
                    else if (type == ToolType.Grenade)
                    {
                        spreadSize = 0;
                        spreadDelay = 0;
                        lifeTime = 0;       
                        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                            GLNetworkWrapper.instance.CreateExplosion(transform.position);
                        else
                        {
                            var newGrenade = Instantiate(grenadeExplosion, transform.position, Quaternion.identity);
                            newGrenade.DestroyEffect(true);
                        }
                        
                        sc.MobHearNoise(transform.position, 70);
                    }
                    else if (type == ToolType.Love)
                    {
                        spreadSize = 2;
                        spreadDelay = 0.5f;
                        lifeTime = 30;
                        
                        sc.MobHearNoise(transform.position, 20);
                    }
                    else if (type == ToolType.Bleed)
                    {
                        spreadSize = 3;
                        spreadDelay = 5;
                        lifeTime = 20;
                        sc.MobHearNoise(transform.position, 20);
                    }
                    else if (type == ToolType.GoldHunger)
                    {
                        spreadSize = 4;
                        spreadDelay = 2;
                        lifeTime = 20;
                        sc.MobHearNoise(transform.position, 20);
                    }
                    else if (type == ToolType.BoneShiver)
                    {
                        spreadSize = 6;
                        spreadDelay = 2;
                        lifeTime = 40;
                        sc.MobHearNoise(transform.position, 20);
                    }
                    else if (type == ToolType.Insight)
                    {
                        spreadSize = 3;
                        spreadDelay = 1;
                        lifeTime = 3;
                        sc.MobHearNoise(transform.position, 20);
                        
                        PlayerSkillsController.instance.InstantTeleport(LevelGenerator.instance.GetClosestTile(transform.position).transform.position);
                        //PlayerSkillsController.instance.SwapProps();
                    }
                    
                    closestTile.SpreadToolEffect(type, spreadSize, spreadDelay, lifeTime, null);
                }
                else
                {
                    Debug.LogError("Tool didn't found a tile");
                }
            }
            else
            {
                // thrown in hub
                ItemThrownInHub();
            }

            if (deathParticles)
            {
                var newParticles = Instantiate(deathParticles, transform.position, transform.rotation);
                StartCoroutine(DestroyEffect(newParticles));
            }
            Destroy(gameObject);
        }
    }

    void ItemThrownInHub()
    {
        
    }

    HealthController GetMonsterInRange(float range)
    {
        HealthController newHc = null;
        float distance = range;
        float newDist = 0;

        for (int i = 0; i < gm.units.Count; i++)
        {
            if (gm.units[i].door || gm.units[i].trapController ||gm.units[i].meatTrap || gm.units[i].wallTrap || gm.units[i].wallBlockerController || gm.units[i].mobPartsController == null || gm.units[i].player)
                continue;
            newDist = Vector3.Distance(gm.units[i].transform.position, transform.position);
            if (newDist <= distance)
            {
                distance = newDist;
                newHc = gm.units[i];
            }
        }
        
        return newHc;
    }

    IEnumerator DestroyEffect(EffectVisualController efc)
    {
        yield return new WaitForSeconds(0.5f);
        efc.DestroyEffect(true);
    }
}
