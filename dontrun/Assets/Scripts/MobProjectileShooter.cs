using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class MobProjectileShooter : MonoBehaviour
{
    public List<MobShooterLevel> levels = new List<MobShooterLevel>();
    private bool dangerous = false;
    private Coroutine attackCoroutine;
    public List<ProjectileController> projectiles;
    private int lastProjectile = 0;
    private PlayerMovement pm;
    private GameManager gm;
    public Transform shootPosition;
    public MobGroundMovement mobGroundMovement;

    [HideInInspector]
    public HealthController hc;

    private GutProgressionManager gpm;
    
    void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        gpm = GutProgressionManager.instance;
    }
    
    public void ToggleDangerous(bool d)
    {
        dangerous = d;

        if (dangerous)
            attackCoroutine = StartCoroutine(Attack());
        else
            StopCoroutine(attackCoroutine);
    }

    IEnumerator Attack()
    {
        while (dangerous)
        {
            if(!gm) gm = GameManager.instance;
            
            yield return new WaitForSeconds(levels[Mathf.Clamp(gm.level.mobsLevel, 0, levels.Count - 1)].shotDelay);
            if (mobGroundMovement.target)
            {
                var newProjectile = projectiles[lastProjectile];
                newProjectile.rb.velocity = Vector3.zero;
                newProjectile.transform.parent = null;
                newProjectile.transform.position = shootPosition.position;
                newProjectile.gameObject.SetActive(true);
                newProjectile.Shoot((mobGroundMovement.target.transform.position + Vector3.up * 1.5f - shootPosition.transform.position).normalized, 
                    levels[Mathf.Clamp(gm.level.mobsLevel, 0, levels.Count - 1)].projectileSpeed, 
                    levels[Mathf.Clamp(gm.level.mobsLevel, 0, levels.Count - 1)].projectileDamage,
                    hc.names[gm.language], hc, gm);
        
                lastProjectile++;
                if (lastProjectile >= projectiles.Count)
                    lastProjectile = 0;   
            }   
        }
    }
}

[Serializable]
public class MobShooterLevel
{
    public float shotDelay = 1.5f;
    public float projectileSpeed = 5;
    public float projectileDamage = 150;
}