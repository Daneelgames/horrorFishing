using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [Header("This script breaks light ")]
    public Light light;
    public ParticleSystem lightOffVfx;
    public AudioSource soundSource;
    public AudioClip lightOffSfx;
    public bool working = false;
    public bool broken = false;

    // Start is called before the first frame update
    void Start()
    {
        AiDirector.instance.lights.Add(this);
        light.gameObject.SetActive(false);
        soundSource.Stop();

        //if (Random.value <= GameManager.instance.level.lightRate)
        if (Random.value <= 0.5f)
        {
            working = true;
            light.gameObject.SetActive(true);
            soundSource.pitch = Random.Range(0.5f, 1.5f);
            soundSource.Play();
        }
    }

    public void LightOff()
    {
        StartCoroutine(BreakLight());
    }

    IEnumerator BreakLight()
    {
        broken = true;
        working = false;
        soundSource.maxDistance *= 3;
        soundSource.clip = lightOffSfx;
        soundSource.pitch = Random.Range(0.5f, 1.5f);
        soundSource.volume = 1;
        soundSource.loop = false;
        soundSource.Play();
        lightOffVfx.Play();
        light.intensity += 10;
        light.range += 10;
        yield return new WaitForSeconds(0.1f);
        light.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        AiDirector.instance.lights.Remove(this);
    }
}