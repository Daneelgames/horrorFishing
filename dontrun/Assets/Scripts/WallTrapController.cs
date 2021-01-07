using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTrapController : MonoBehaviour
{
    public int damage = 200;
    public float attackCooldownMin = 3;
    public float attackCooldownMax = 10;
    public float attackTimeMin = 1;
    public float attackTimeMax = 4;

    bool dangerous = false;
    public AudioSource au;
    public List<Animator> wallTrapsAnims;
    public HealthController hc;
    public List<string> names = new List<string>();
    public StatussEffectsOnAttack effectsOnAttack;

    
    private Coroutine repeatAttacking;

    GameManager gm;

    private void Start()
    {
        gm = GameManager.instance;
        repeatAttacking = StartCoroutine(RepeatAttacking());
    }

    IEnumerator RepeatAttacking()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(attackCooldownMin, attackCooldownMax));
            
            if (hc.health > 0 && Vector3.Distance(transform.position, gm.player.transform.position) < 15)
            {
                dangerous = true;
                au.pitch = Random.Range(0.75f, 1.25f);
                au.Play();

                foreach (Animator a in wallTrapsAnims)
                    a.SetBool("Attack", true);
            }
            yield return new WaitForSeconds(Random.Range(attackTimeMin, attackTimeMax));
            
            dangerous = false;
            au.Stop();
            foreach (Animator a in wallTrapsAnims)
                a.SetBool("Attack", false);
        }
    }

    public void Death()
    {
        StopAllCoroutines();
    }
    

    private void OnTriggerStay(Collider other)
    {
        if (hc.health > 0 && dangerous)
        {
            HealthController newHc = other.gameObject.GetComponent<HealthController>();

            if (newHc != null)
            {
                newHc.Damage(damage, other.transform.position + Vector3.up, transform.position + Vector3.up, null, hc.playerDamagedMessage[gm.language], false, names[gm.language], hc, effectsOnAttack, false);
                dangerous = false;
                au.Stop();
                foreach (Animator a in wallTrapsAnims)
                    a.SetBool("Attack", false);
            }
        }
    }

    public void Damage()
    {
        if (dangerous)
        {
            dangerous = false;
            au.Stop();
            foreach (Animator a in wallTrapsAnims)
                a.SetBool("Attack", false);
            
            StopCoroutine(repeatAttacking);
            repeatAttacking = StartCoroutine(RepeatAttacking());
        }
        else
        {
            StopCoroutine(repeatAttacking);
            
            StartCoroutine(AttackAfterDamage());
        }
    }

    IEnumerator AttackAfterDamage()
    {
        if (hc.health > 0 && Vector3.Distance(transform.position, gm.player.transform.position) < 30)
        {
            dangerous = true;
            au.pitch = Random.Range(0.75f, 1.25f);
            au.Play();

            foreach (Animator a in wallTrapsAnims)
                a.SetBool("Attack", true);
        }
        yield return new WaitForSeconds(Random.Range(attackTimeMin, attackTimeMax));
            
        dangerous = false;
        au.Stop();
        foreach (Animator a in wallTrapsAnims)
            a.SetBool("Attack", false);
        
        repeatAttacking = StartCoroutine(RepeatAttacking());
    }
}
