using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class MobBodyPart : MonoBehaviour
{
    [Range(0, 10)]
    public float damageModificator = 1;
    public Collider coll;
    public AudioSource au;
    public ParticleSystem deathParticle;

    public HealthController hc;

    [Header("Part is used for attacks")] 
    public bool usedForAttack = false;
    private bool closeToPlayer = false;
    private PlayerMovement pm;
    public MobMeleeAttack mobAttack;
    private GameManager gm;

    public List<CustomIKJoint> ikTargets;
    private void Start()
    {
        if (au)
        {
            au.Stop();
            au.pitch = Random.Range(0.75f, 1.25f);
            au.Play();
        }
        pm = PlayerMovement.instance;
        gm = GameManager.instance;
    }
    
    void OnEnable()
    {
        StartCoroutine(GetDistance());
    }
    private void OnDisable()
    {
        StopCoroutine(GetDistance());
    }

    IEnumerator GetDistance()
    {
        while (true)
        {
            if (mobAttack != null && mobAttack.gameObject.activeInHierarchy)
            {
                var distance = 1000f;

                if (pm)
                {
                    var playerPos = pm.transform.position;
                    
                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    {
                        playerPos = GLNetworkWrapper.instance.GetClosestPlayer(transform.position).transform.position;
                    }
                 
                    distance = Vector3.Distance(transform.position, playerPos);

                }
                
                if (distance <= 50)
                {
                    closeToPlayer = true;   
                }
                else
                {
                    closeToPlayer = false;
                }   
            }
            else
                closeToPlayer = false;
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (usedForAttack == false || mobAttack.levels[0].attackCooldown > 0 || closeToPlayer == false || !other.gameObject.activeInHierarchy) return;
        if (!mobAttack || !mobAttack.hc || mobAttack.hc.peaceful) return;
        if (!mobAttack.gameObject.activeInHierarchy) return;

        switch (other.gameObject.layer)
        {
            // if hitting other monster (not his part)
            case 11 when mobAttack && other.gameObject != mobAttack.gameObject:
            {
                HealthController _hc = other.gameObject.GetComponent<HealthController>();
                
                //print(hc);
                if (_hc == null || _hc == mobAttack.hc || (_hc.player && mobAttack.hc.inLove))
                    return;
                
                Vector3 bloodSpawnPosition = _hc.transform.position + Vector3.up;
                
                if (_hc.mobPartsController && _hc.mobPartsController.dropPosition)
                    bloodSpawnPosition = _hc.mobPartsController.dropPosition.position;
                
                if (_hc.damageCooldown <= 0 && _hc.health > 0)
                {
                    mobAttack.Cooldown();
                    
                    _hc.Damage(mobAttack.levels[Mathf.Clamp(mobAttack.mobParts.level, 0, mobAttack.levels.Count-1)].damage, bloodSpawnPosition, transform.position + Vector3.up, null, mobAttack.hc.playerDamagedMessage[gm.language], false, mobAttack.hc.names[gm.language], mobAttack.hc, mobAttack.effectsOnAttack, false);
                }
                break;
            }
        }
    }

    
    [ContextMenu("Get Collider and death particles in children")]
    void GetPositions()
    {
        coll = GetComponent<Collider>();
        deathParticle = GetComponentInChildren<ParticleSystem>(true);
    }
}
