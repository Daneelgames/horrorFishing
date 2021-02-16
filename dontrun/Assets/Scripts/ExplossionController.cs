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
            
            if (coll.gameObject.layer == 18 || coll.gameObject.layer == 11)
            {
                HealthController hc = coll.gameObject.GetComponent<HealthController>();

                if (hc != null)
                {
                    if (hc.player)
                    {
                        // dont damage the player directly in coop
                        if (hc.playerMovement && GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                            return;
                        
                        explosionDamage /= 4;   
                    }

                    if (!hc.door)
                    {
                        if (!hc.boss && !hc.player)
                            explosionDamage = hc.healthMax;
                        
                        hc.Damage(explosionDamage, hc.gameObject.transform.position + Vector3.one * 2,
                            transform.position, null, damageMessages[GameManager.instance.language], false, names[GameManager.instance.language], null, effectsOnAttack, true);   
                    }
                    else
                        hc.door.DoorDestroyed();
                }
            }
            else if (coll.gameObject.layer == 16 || coll.gameObject.layer == 10) // WALLS or solids
            {
                MobBodyPart part = coll.gameObject.GetComponent<MobBodyPart>();

                if (part != null)
                {
                    if (wallsDamageCoroutine == null)
                        wallsDamageCoroutine = StartCoroutine(DamageWallCoroutine(part));
                    else
                        StartCoroutine(WaitUntilDamageCoroutineIsNull(part));
                }
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
            part.hc.Damage(part.hc.healthMax, part.gameObject.transform.position + Vector3.one * 2,
            transform.position, null, null, false, null, null, null, true);
        
        wallsDamageCoroutine = null;
    }
}
