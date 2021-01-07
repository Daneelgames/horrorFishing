using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomTrackOnStart : MonoBehaviour
{
    public List<AudioClip> clips;
    public AudioSource au;
    public bool snout = false;
    public AudioSource oneShotSource;

    public bool onlyForCoop = false;
    
    private Coroutine volumeCoroutine;

    public static SetRandomTrackOnStart snoutMusicPlayerInstance;

    private SnoutMusicBlocker lastMusicBlocker;

    public float minPitch = 0.75f;
    public float maxPitch = 1f;
    void Awake()
    {
        if (snout)
            snoutMusicPlayerInstance = this;
    }
    
    void OnEnable()
    {
        if (onlyForCoop && (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false))
            return;
        
        RandomTrackOnStart();
    }

    public void RandomTrackOnStart()
    {
        au.volume = 1;
        
        if (!snout)
        {
            au.clip = clips[Random.Range(0, clips.Count)];
            au.pitch = Random.Range(minPitch, maxPitch);
            au.Play();   
        }
        else
        {
            RandomTrackOnStartSnout();
        }
    }
    
    void RandomTrackOnStartSnout()
    {
        oneShotSource.Stop();
        oneShotSource.pitch = Random.Range(0.75f, 1);
        oneShotSource.Play();
        
        //on snout tracks should be played from the global list 
        int r = Random.Range(0, ItemsList.instance.unlockedTracks.Count);
        au.clip = clips[ItemsList.instance.unlockedTracks[r]];
        au.pitch = Random.Range(minPitch, maxPitch);
        au.Play();
    }

    public void SetVolume(SnoutMusicBlocker smb, float newVolume)
    {
        if (volumeCoroutine != null)
            StopCoroutine(volumeCoroutine);
     
        lastMusicBlocker = smb;
        volumeCoroutine = StartCoroutine(SetVolumeOverTime(newVolume));
    }
    
    IEnumerator SetVolumeOverTime(float newVolume)
    {
        float t = 0;
        float tt = 2;

        float startVolume = au.volume;
        
        while (t < tt)
        {
            t += Time.deltaTime;
            au.volume = Mathf.Lerp(startVolume, newVolume, t / tt);
            yield return null;
        }

        volumeCoroutine = null;

        float distance = Vector3.Distance(transform.position, lastMusicBlocker.transform.position);
        
        while (lastMusicBlocker != null && distance < lastMusicBlocker.collider.radius)
        {
            distance = Vector3.Distance(transform.position, lastMusicBlocker.transform.position);
            yield return new WaitForSeconds(1);
        }

        t = 0;
        while (t < tt)
        {
            t += Time.deltaTime;
            au.volume = Mathf.Lerp(newVolume, startVolume, t / tt);
            yield return null;
        }

    }
}
