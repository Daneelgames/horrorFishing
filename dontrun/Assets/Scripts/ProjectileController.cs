using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public Rigidbody rb;
    private string attackerName;
    private float damage = 100;
    private HealthController hc; // owner / attacker's hc
    private GameManager gm;
    private PlayerMovement pm;

    public StatussEffectsOnAttack effectsOnAttack;
    
    public ParticleSystem deathParticles;

    void Start()
    {
        pm = PlayerMovement.instance;
    }
    
    public void Shoot(Vector3 direction, float speed, float d, string _attackerName, HealthController _hc, GameManager _gm)
    {
        deathParticles.gameObject.SetActive(false);
        deathParticles.transform.parent = transform;
        deathParticles.transform.localPosition = Vector3.zero;
        damage = d;
        gm = _gm;
        hc = _hc;
        attackerName = _attackerName;
        rb.AddForce(direction * speed, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        rb.AddForce(((pm.transform.position + Vector3.up * 1.5f) - transform.position).normalized * 5);
    }

    void OnTriggerEnter(Collider coll)
    {
        switch (coll.gameObject.layer)
        {
            // solids
            case 10:
                DeathParticles();
                gameObject.SetActive(false);
                break;
            // units
            case 11:
            {
                var newHealth = coll.GetComponent<HealthController>();

                if (!newHealth || newHealth == hc) return;
                
                newHealth.Damage(damage,
                    coll.transform.position + Vector3.up * 1.5f, transform.position,
                    null, hc.playerDamagedMessage[gm.language], false, attackerName, hc, effectsOnAttack, true);
                
                DeathParticles();
                gameObject.SetActive(false);
                break;
            }
            // walls
            case 16:
                gameObject.SetActive(false);
                break;
            // doors
            case 18:
                var door = coll.gameObject.GetComponent<DoorController>();
                if (door)
                {
                    
                    door.DoorDestroyed();
                    DeathParticles();
                    gameObject.SetActive(false);
                }
                break;
        }
    }

    void DeathParticles()
    {
        deathParticles.transform.position = transform.position;
        deathParticles.transform.parent = null;
        deathParticles.gameObject.SetActive(true);
    }
}