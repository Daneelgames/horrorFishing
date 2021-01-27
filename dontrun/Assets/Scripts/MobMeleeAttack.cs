using System;
using System.Collections;
using System.Collections.Generic;
using FirstGearGames.Mirrors.Assets.FlexNetworkAnimators;
using PlayerControls;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class MobMeleeAttackStats
{
    public float damage = 300;

    public float attackCooldown = 2;
    public float attackCooldownMax = 3.5f;
    public float attackCooldownMin = 1.75f;
}

public class MobMeleeAttack : MonoBehaviour
{
    public MobPartsController mobParts;
    public List<MobMeleeAttackStats> levels;
    public MobGroundMovement mobGroundMovement;
    public HealthController hc;
    
    public bool thiefBehaviour = false;
    public MobCharacterControllerMovement mobCharacterControllerMovement;
    private PlayerMovement pm;
    [HideInInspector]
    public Coroutine attackCoroutine;

    public StatussEffectsOnAttack effectsOnAttack;

    private void Start()
    {
        pm = PlayerMovement.instance;
        levels[GetLevel()].attackCooldown = 0;
        mobGroundMovement = GetComponent<MobGroundMovement>();
    }
    
    int GetLevel()
    {
        return Mathf.Clamp(mobParts.level, 0, levels.Count - 1);
    }


    private void Update()
    {
        if (levels[GetLevel()].attackCooldown > 0)
        {
            if (pm.hc.health > pm.hc.healthMax / 5)
                levels[GetLevel()].attackCooldown -= Time.deltaTime;
            else
                levels[GetLevel()].attackCooldown -= Time.deltaTime * 0.5f;
        }
    }

    public void Attack()
    {
        if (!hc.peaceful)
        {
            if (attackCoroutine != null && gameObject.activeInHierarchy)
                StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(AttackOverTime());
            
        }
    }

    public void Cooldown()
    {
        levels[GetLevel()].attackCooldown = Random.Range(levels[GetLevel()].attackCooldownMin , levels[GetLevel()].attackCooldownMax);
    }

    public IEnumerator AttackOverTime()
    {
        yield break;
        
        /*
        if (hc.mobPartsController)
        {
            if (mobGroundMovement && mobGroundMovement.agent && mobGroundMovement.agent.enabled)
                mobGroundMovement.agent.isStopped = true;
            
            if (pm.hc.health > pm.hc.healthMax / 4)
                yield return new WaitForSeconds(levels[GetLevel()].swingTime * 0.5f);
            
            hc.mobPartsController.anim.SetTrigger(damageArea.attackAnimString);
            if (_flexNetworkAnimator) _flexNetworkAnimator.SetTrigger(Animator.StringToHash(damageArea.attackAnimString));
            
            hc.mobAudio.Attack();
            
                yield return new WaitForSeconds(levels[GetLevel()].swingTime);
                
            damageArea.ToggleDangerous(true);
            
            yield return new WaitForSeconds(levels[GetLevel()].attackTime);
            
            damageArea.ToggleDangerous(false);
            
            yield return new WaitForSeconds(levels[GetLevel()].returnTime);

            if (mobGroundMovement && mobGroundMovement.agent &&  mobGroundMovement.agent.enabled)
                mobGroundMovement.agent.isStopped = false;
            
            if (castle)
                castle.StopAttack(this);
        }*/
    }

    public void StopAttackAnimation()
    {
        /*
        hc.mobPartsController.anim.SetTrigger("Damaged");
        hc.mobPartsController.anim.SetBool("Attack", false);
        */
    }

    public void BreakAttack()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        StopAttackAnimation();
    }

    public void Death()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
    }
}