using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

[Serializable]
public class LevelParametersWitchFireAttack
{
    public float attackDistance = 50f;
    public float attackRunDistance = 10f;
    public float attackCooldown = 3;
    public float distanceBetweenFlames = 5;   
}
public class MobWitchFireAttack : MonoBehaviour
{
    public List<LevelParametersWitchFireAttack> levels;
    float attackCooldownCurrent = 0;
    public FireController firePrefab;
    public MobPartsController mobParts;
    PlayerMovement pm;

    public HealthController target;

    private void Start()
    {
        pm = PlayerMovement.instance;
    }
    public void SeePlayer(MobHideInCorners mob)
    {
        target = mob.target;
        if (target && attackCooldownCurrent <= 0 && mob.hc.health > 0)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance <= levels[GetLevel()].attackDistance && distance > levels[GetLevel()].attackRunDistance)
            {
                attackCooldownCurrent = levels[GetLevel()].attackCooldown;
                StartCoroutine(TargetAttack(distance, mob));
            }
            else if (distance <= levels[GetLevel()].attackRunDistance)
            {
                attackCooldownCurrent = levels[GetLevel()].attackCooldown;
                CircleAttack();
            }
        }
    }

    int GetLevel()
    {
        return Mathf.Clamp(mobParts.level, 0, levels.Count - 1);
    }

    private void Update()
    {
        if (attackCooldownCurrent > 0)
        {
            if (pm.hc.health > pm.hc.healthMax / 4)
                 attackCooldownCurrent -= Time.deltaTime;
            else
                attackCooldownCurrent -= Time.deltaTime * 0.5f;
        }
    }

    IEnumerator TargetAttack(float distance, MobHideInCorners mob)
    {
        int i = 0;
        while( distance > levels[GetLevel()].distanceBetweenFlames)
        {
            if (mob.hc.health > 0)
            {
                Vector3 newPos = target.transform.position + (transform.position - target.transform.position).normalized * i * levels[GetLevel()].distanceBetweenFlames;
                i++;
                CreateFlame(newPos);
                distance -= levels[GetLevel()].distanceBetweenFlames;
            }
            else distance = levels[GetLevel()].distanceBetweenFlames;  

            yield return new WaitForSeconds(0.1f);
        }
    }

    void CircleAttack()
    {
        CreateFlame(transform.position);
    }

    void CreateFlame(Vector3 pos)
    {
        Instantiate(firePrefab, pos, Quaternion.identity);
    }
}
