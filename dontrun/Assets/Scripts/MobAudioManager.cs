using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MobAudioManager : MonoBehaviour
{
    public AudioSource idleAu;
    public AudioSource chaseAu;
    public AudioSource attackAu;
    public List<AudioSource> steps;
    public AudioSource damageAu;
    public AudioSource resurrectAu;

    void Awake()
    {
        if (SpawnController.instance) SpawnController.instance.mobAudioManagers.Add(this);
        
        if (resurrectAu && Random.value > 0.5f)
        {
            Resurrect();
        }
    }

    void OnDestroy()
    {
        if (SpawnController.instance && SpawnController.instance.mobAudioManagers.Contains(this)) 
            SpawnController.instance.mobAudioManagers.Remove(this);
    }
    
    public void Step(int index)
    {
        // from anim,dont sync
        steps[index].pitch = Random.Range(0.75f, 1.25f);
        steps[index].Play();
    }

    public void Resurrect()
    {
        resurrectAu.pitch = Random.Range(0.75f, 1.25f);
        resurrectAu.Play();
    }

    public void IdleAmbient()
    {
        // should be synced
        int _action = 0;
        int _myIndex = SpawnController.instance.mobAudioManagers.IndexOf(this);
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.PlayMobSoundOnClient(_myIndex, _action);
        }
        else
        {
            PlaySoundOnClient(_action);
        }
    }

    public void ChaseAmbient()
    {
        // should be synced
        int _action = 1;
        int _myIndex = SpawnController.instance.mobAudioManagers.IndexOf(this);
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.PlayMobSoundOnClient(_myIndex, _action);
        }
        else
        {
            PlaySoundOnClient(_action);
        }
    }
    
    public void Attack()
    {
        // should be synced
        int _action = 2;
        int _myIndex = SpawnController.instance.mobAudioManagers.IndexOf(this);
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.PlayMobSoundOnClient(_myIndex, _action);
        }
        else
        {
            PlaySoundOnClient(_action);
        }
    }

    public void Damage()
    {
        // should be synced
        int _action = 3;
        int _myIndex = SpawnController.instance.mobAudioManagers.IndexOf(this);
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.PlayMobSoundOnClient(_myIndex, _action);
        }
        else
        {
            PlaySoundOnClient(_action);
        }
    }
    
    public void Death()
    {
        // should be synced
        int _action = 4;
        int _myIndex = SpawnController.instance.mobAudioManagers.IndexOf(this);
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.PlayMobSoundOnClient(_myIndex, _action);
        }
        else
        {
            PlaySoundOnClient(_action);
        }
    }

    public void PlaySoundOnClient(int actionType)
    {
        switch (actionType)
        {
            case 0: 
                // idle ambient
                if (idleAu.isPlaying)
                    break;
                
                chaseAu.Stop();

                idleAu.pitch = Random.Range(0.75f, 1.25f);
                idleAu.Play();
                break;
            
            case 1:
                // chase
                if (chaseAu.isPlaying)
                    break;

                idleAu.Stop();

                chaseAu.pitch = Random.Range(0.75f, 1.25f);
                chaseAu.Play();
                break;
            
            case 2:
                // attack
                if (attackAu)
                {
                    attackAu.pitch = Random.Range(0.9f, 1.1f);
                    attackAu.Play();
                }
                else if (damageAu)
                {
                    damageAu.pitch = Random.Range(0.75f, 1.25f);
                    damageAu.Play();    
                }
                break;
            
            case 3:
                // damage
                if (!damageAu.isPlaying)
                {
                    damageAu.pitch = Random.Range(0.75f, 1.25f);
                    damageAu.Play();   
                }
                break;
            
            case 4:
                //death
                if (idleAu) idleAu.enabled = false;
                if (chaseAu) chaseAu.enabled = false;
                if (attackAu) attackAu.enabled = false;
                foreach (var s in steps)
                {
                    s.enabled = false;
                }
                break;
        }
    }
}
