using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEditor;
using UnityEngine;

public class ExplossionController : MonoBehaviour
{
    public float explosionDamage = 800;
    public List<string> names = new List<string>();
    public List<string> damageMessages = new List<string>();
    private bool dangerous = true;
    public StatussEffectsOnAttack effectsOnAttack;
    private Coroutine wallsDamageCoroutine;
    
    List<HealthController> damagedHc = new List<HealthController>();
    void Start()
    {
        StartCoroutine(Disable());
    }

    IEnumerator Disable()
    {
        yield return new WaitForSeconds(0.1f);
        dangerous = false;
    }
    
    void OnTriggerEnter(Collider coll)
    {
        if (dangerous)
        {
            if (GLNetworkWrapper.instance  && GLNetworkWrapper.instance.coopIsActive && GLNetworkWrapper.instance.localPlayer.isServer == false)
                return;
            
            if (coll.gameObject.layer == 11)
            {
                HealthController hc;
                
                var part = coll.gameObject.GetComponent<MobBodyPart>();
                    
                if (part) 
                    hc = part.hc;
                else
                    hc = coll.gameObject.GetComponent<HealthController>();

                if (!hc || damagedHc.Contains(hc)) return;
                
                damagedHc.Add(hc);
                
                if (hc.player) explosionDamage /= 5;   
                else if (hc.boss) explosionDamage /= 1.5f;

                if (!hc.boss && !hc.player)
                    explosionDamage = hc.healthMax;
                
                hc.Damage(explosionDamage, hc.gameObject.transform.position + Vector3.one * 2,
                    transform.position, null, damageMessages[GameManager.instance.language], true, names[GameManager.instance.language], null, effectsOnAttack, true);   
            }
            else if (coll.gameObject.layer == 16 || coll.gameObject.layer == 10) // WALLS or solids
            {
                MobBodyPart part = coll.gameObject.GetComponent<MobBodyPart>();

                if (part == null)
                    return;
                
                if (wallsDamageCoroutine == null)
                    wallsDamageCoroutine = StartCoroutine(DamageWallCoroutine(part));
                else
                    StartCoroutine(WaitUntilDamageCoroutineIsNull(part));
            }
        }
    }

    IEnumerator WaitUntilDamageCoroutineIsNull(MobBodyPart part)
    {
        while (wallsDamageCoroutine != null)
        {
            yield return new WaitForSeconds(0.1f);
            print("WaitUntilDamageCoroutineIsNull");
        }
        
        wallsDamageCoroutine = StartCoroutine(DamageWallCoroutine(part));
    }

    IEnumerator DamageWallCoroutine(MobBodyPart part)
    {
        yield return null;
        if (part != null)
        {
            if (damagedHc.Contains(part.hc))
            {
                wallsDamageCoroutine = null;
                yield break;
            }
            
            damagedHc.Add(part.hc);
            
            if (part.hc.boss) explosionDamage /= 1.5f;
            
            part.hc.Damage(explosionDamage, part.gameObject.transform.position + Vector3.one * 2,
                transform.position, null, null, true, null, null, null, true);
        }
        wallsDamageCoroutine = null;
    }
}
