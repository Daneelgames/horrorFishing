using System.Collections;
using System.Collections.Generic;
using System.Timers;
using PlayerControls;
using UnityEngine;

public class WallBlockerController : MonoBehaviour
{
    private PlayerMovement pm;
    public List<Collider> colliders;
    public float damageOverTime = 0;
    public Animator anim;
    public bool turnToPlayer = false;
    public HealthController hc;
    private GameManager gm;
    private Vector3 newEulerAngles;
    public MobAudioManager mobAu;

    private string attackString = "Attack";
    
    void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        transform.parent = null;
    }
    
    void OnTriggerStay(Collider coll)
    {
        if (hc && hc.health > 0 && !hc.inLove)
        {
            if (pm && coll.gameObject == pm.gameObject)
            {
                pm.wallBlocker = this;
                if (damageOverTime > 0)
                {
                    if (turnToPlayer)
                    {
                        transform.LookAt(pm.transform.position);
                        newEulerAngles = transform.rotation.eulerAngles;
                        newEulerAngles.x = 0;
                        newEulerAngles.z = 0;
                        transform.root.eulerAngles = newEulerAngles;
                    }
                    
                    if (hc.statusEffects[3].effectActive) // rust
                    {
                        if (damageOverTime > 0)
                            pm.hc.DamageOverTime(damageOverTime / 2 * Time.deltaTime, hc.names[gm.language], hc.playerDamagedMessage[gm.language]);   
                    }
                    else if (damageOverTime > 0)
                        pm.hc.DamageOverTime(damageOverTime * Time.deltaTime, hc.names[gm.language], hc.playerDamagedMessage[gm.language]);


                    if (anim)
                        anim.SetBool(attackString, true);

                    if (mobAu)
                    {
                        if (!mobAu.attackAu.isPlaying)
                        {
                            mobAu.attackAu.pitch = Random.Range(0.75f, 1.25f);
                            mobAu.attackAu.Play();   
                        }
                    }
                }

                if (hc.statusEffects[0].effectActive)
                {
                    pm.hc.AddPoison(5 * Time.deltaTime);   
                }   
                if (hc.statusEffects[1].effectActive)
                    pm.hc.AddFire(10 * Time.deltaTime);
                if (hc.statusEffects[2].effectActive)
                    pm.hc.AddBleed(10 * Time.deltaTime);
                if (hc.statusEffects[3].effectActive) // rust
                    pm.hc.AddRust(10 * Time.deltaTime);   
                if (hc.statusEffects[4].effectActive)
                    pm.hc.AddRegen(10 * Time.deltaTime);
                if (hc.statusEffects[5].effectActive)
                    pm.hc.AddGoldHunger(10 * Time.deltaTime);
                if (hc.statusEffects[6].effectActive)
                    pm.hc.AddCold(10 * Time.deltaTime);
            }
        }
    }

    void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject == pm.gameObject && pm.wallBlocker == this)
        {
            PlayerExit();
        }
    }

    public void PlayerExit()    
    {
        pm.wallBlocker = null;
            
        if (anim)
            anim.SetBool(attackString, false);
            
        if (mobAu)
        {
            mobAu.attackAu.Stop();   
        } 
    }

    public void Death()
    {
        if (pm.wallBlocker && pm.wallBlocker == this)
            pm.wallBlocker = null;
        foreach (var coll in colliders)
        {
            coll.enabled = false;   
        }
    }

    void OnDestroy()
    {
        pm = PlayerMovement.instance;
        
        if (pm.wallBlocker && pm.wallBlocker == this)
        {
            pm.wallBlocker = null;
        }
        
        foreach (var coll in colliders)
        {
            if (coll)
                coll.enabled = false;   
        }
    }
}