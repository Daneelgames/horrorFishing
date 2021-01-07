using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    public ParticleSystem particles;
    ParticleSystem.EmissionModule emission;
    public AudioSource au;
    public float lifeTime = 5f;
    bool dangerous = false;
    public Light light;
    GameManager gm;
    float burnCooldown = 0;

    private void Start()
    {
        emission = particles.emission;
        gm = GameManager.instance;
        StartCoroutine(StartFire());
    }

    IEnumerator StartFire()
    {
        au.volume = 0;
        StartCoroutine(FireStart());
        yield return new WaitForSeconds(1.5f);

        dangerous = true;

        yield return new WaitForSeconds(lifeTime);
        dangerous = false;
        emission.rateOverTime = 0;
        StartCoroutine(FireEnd());
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }

    IEnumerator FireStart()
    {
        while (au.volume < 1)
        {
            au.volume += Time.deltaTime;
            if (light.intensity < 2)
                light.intensity += Time.deltaTime * 4;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FireEnd()
    {
        while(true)
        {
            au.volume -= Time.deltaTime;
            light.intensity -= Time.deltaTime * 4;
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (dangerous)
        {
            if (other.gameObject.layer == 11)
            {
                for (var index = 0; index < gm.units.Count; index++)
                {
                    HealthController hc = gm.units[index];
                    if (hc != null && hc.health > 0 && hc.gameObject == other.gameObject && hc.fireParticles)
                    {
                        hc.AddFire(30 * Time.deltaTime);
                    }
                }
            }
        }
    }
}
