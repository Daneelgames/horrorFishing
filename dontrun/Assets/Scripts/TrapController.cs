using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    public Animator anim;
    public float damage = 200;
    public float dangerWindow = 0.25f;
    float dangerTime = 0;
    public float attackCooldownMax = 4;
    float attackCooldownCurrent = 0;
    public AudioSource au;
    public List<string> playerDamagedMessage = new List<string>(); 
    GameManager gm;
    public List<string> names = new List<string>();
    private bool closeToPlayer = false;
    public StatussEffectsOnAttack effectsOnAttack;
    public float attackDelay = 0;
    float attackDelayCurrent = 0;

    public HealthController ownHc;
    private bool dead = false;


    private void Start()
    {
        gm = GameManager.instance;
        transform.parent = null;
        StartCoroutine(GetDistance());
    }

    private void Update()
    {
        if (!dead)
        {
            if (attackCooldownCurrent > 0) attackCooldownCurrent -= Time.deltaTime;
            if (dangerTime > 0) dangerTime -= Time.deltaTime;   
        }
    }

    IEnumerator GetDistance()
    {
        while (!dead)
        {
            if (gm.player == null)
                break;
            
            Vector3 playerPos = gm.player.transform.position;

            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                playerPos = GLNetworkWrapper.instance.GetClosestPlayer(transform.position).transform.position;
            
            if (Vector3.Distance(transform.position, playerPos) <= 40)
                closeToPlayer = true;
            else
                closeToPlayer = false;   
            
            yield return new WaitForSeconds(1);
        }
    }
    

    private void OnTriggerStay(Collider other)
    {
        if (!dead && closeToPlayer)
        {
            if (attackDelayCurrent > 0)
                attackDelayCurrent -= Time.deltaTime;
            else
            {
                if (other.gameObject.layer == 11 && (!ownHc || other.gameObject != ownHc.gameObject)) // if unit
                {
                    HealthController hc = other.gameObject.GetComponent<HealthController>();
                    
                    
                    if (attackCooldownCurrent <= 0)
                    {
                        if (hc != null && hc.health > 0)
                        {
                            Attack();
                        }
                    }
                    else if (dangerTime > 0)
                    {
                        attackDelayCurrent = attackDelay;
                        DamageUnit(hc);
                    }
                }   
            }
        }
    }

    void Attack()
    {
        if (anim == null) return;
        
        anim.SetTrigger("Attack");
        attackCooldownCurrent = attackCooldownMax;
        au.pitch = Random.Range(0.9f, 1.1f);
        au.Play();
    }

    public void SetDanger()
    {
        dangerTime = dangerWindow;
    }

    void DamageUnit(HealthController hc)
    {
        if (hc == null || hc.health <= 0 || (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && (hc.playerMovement || LevelGenerator.instance.levelgenOnHost == false)))
            return;
        
        hc.Damage(damage, hc.transform.position + Vector3.up * 2, transform.position, null, playerDamagedMessage[gm.language], false, names[gm.language], ownHc, effectsOnAttack, false);
        dangerTime = 0;
    }
    
    public void Death()
    {
        dead = true;
    }
}