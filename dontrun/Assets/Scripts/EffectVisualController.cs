using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectVisualController : MonoBehaviour
{
    public bool grenadeExplosion = false;
    private bool active = true;
    
    public List<ParticleSystem> particles;
    public List<Light> lights;
    public List<AudioSource> audioSources;

    public float currentTime = 0;
    public float timeToFinish = 2;
    public float timeToDestroy = 10;
    
    List<float> initEmissions = new List<float>();
    List<float> initLightIntensities = new List<float>();
    List<float> initvolumes = new List<float>();

    private bool destroying = false;

    private Coroutine destroyCoroutine;
    
    void Awake()
    {
        foreach (var p in particles)
        {
            initEmissions.Add(p.emission.rateOverTime.constant);
        }
        
        foreach (var p in lights)
        {
            initLightIntensities.Add(p.intensity);
        }
        
        foreach (var p in audioSources)
        {
            initvolumes.Add(p.volume);
        }
        
        if (grenadeExplosion)
            DestroyEffect(true);
    }

    public void StartEffect()
    {
        if (destroyCoroutine != null)
            StopCoroutine(destroyCoroutine);
        
        active = true;
        destroying = false;
        if (initEmissions.Count > 0)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                ParticleSystem.EmissionModule emission = particles[i].emission;
                emission.rateOverTime = initEmissions[i];
            }   
        }

        if (initLightIntensities.Count > 0)
        {
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].intensity = initLightIntensities[i];
            }   
        }

        if (audioSources.Count > 0 && initvolumes.Count > 0)
        {
            for (int i = 0; i < audioSources.Count; i++)
            {
                audioSources[i].volume = initvolumes[i];
            }   
        }
    }
    
    void OnEnable()
    {
        StartEffect();
        foreach (var audio in audioSources)
        {
            audio.Play();
        }
    }
    
    public void DestroyEffect(bool destroy)
    {
        if (!destroying)
        {
            destroying = true;
            if (active)
            {
                active = false;
                if (destroyCoroutine != null)
                    StopCoroutine(destroyCoroutine);
            
                if (gameObject.activeInHierarchy)
                    destroyCoroutine = StartCoroutine(DestroyOverTime(destroy));
            }
        }
    }

    IEnumerator DestroyOverTime(bool destroy)
    {
        currentTime = 0;
        if (particles.Count > 0)
        {
            foreach (var p in particles)
            {
                ParticleSystem.EmissionModule emission = p.emission;
                emission.rateOverTime = 0;
            }   
        }

        while (currentTime < timeToFinish)
        {
            if (lights.Count > 0)
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    lights[i].intensity -= Time.deltaTime * initLightIntensities[i];
                }
            }

            if (audioSources.Count > 0)
            {
                foreach (var au in audioSources)
                {
                    au.volume -= Time.deltaTime;
                }   
            }
            
            currentTime += Time.deltaTime;
            yield return null;
        }

        destroyCoroutine = null;

        if (destroy)
        {
            yield return new WaitForSeconds(timeToDestroy);
            Destroy(gameObject, timeToDestroy);   
        }
        else
        {
            yield return new WaitForSeconds(timeToDestroy);
            gameObject.SetActive(false);
        }
    }
    
}
