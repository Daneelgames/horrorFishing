using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorShaftController : MonoBehaviour
{
    public static ElevatorShaftController instance;
    public Animator anim;
    public AudioSource au;
    public AudioSource au2;
    public Light light;
    public Collider wallCollider;
    
    void Awake()
    {
        instance = this;
    }

    public void StopLoading()
    {
        if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
            StopLoadingOnClient();
        else if (LevelGenerator.instance.levelgenOnHost)
        {
            GLNetworkWrapper.instance.ElevatorShaftStopLoading();
        }
    }

    public void StopLoadingOnClient()
    {
        anim.SetBool("Active", false);
        wallCollider.enabled = false;
        StartCoroutine(TurnOffAu());   
    }
    
    IEnumerator TurnOffAu()
    {
        float t = 0;
        float initVolume = au.volume;
        float initLight = light.intensity;
        while (t < 2)
        {
            t += Time.deltaTime;
            au.volume = Mathf.Lerp(initVolume, 0, t / 2);
            au2.volume = Mathf.Lerp(initVolume, 0, t / 2);
            light.intensity = Mathf.Lerp(initLight, 0, t / 2);

            yield return null;
        }
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}