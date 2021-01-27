using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class AnimatorEventsReciever : MonoBehaviour
{
    public MobAudioManager mobAu;
    public AudioSource au;
    public bool randomizeOneShot = false;
    public WeaponPickUp weaponToSpawn;
    public Transform dropPosition;
    public ParticleSystem rangeShotParticles;
    public AudioSource shotAu;
    
    public void MobStep(int index)
    {
        mobAu.Step();
    }
    
    public void PlayShotParticle()
    {
        shotAu.pitch = Random.Range(0.75f, 1f);
        rangeShotParticles.Play();
        shotAu.Play();
    }

    public void PlaySound()
    {
        if (randomizeOneShot)
            au.pitch = Random.Range(0.75f, 1.25f);
        
        au.Play();
    }

    public void Earthquake()
    {
        PlayerMovement.instance.Earthquake();
    }

    public void SpawnWeapon()
    {
        WeaponPickUp newDrop = Instantiate(weaponToSpawn, dropPosition.position, Quaternion.identity);

            if (newDrop.weaponConnector)
                newDrop.weaponConnector.GenerateOnSpawn();


        bool deadWeapon = Random.value <= 0.5f;
        bool npcWeapon = Random.value <= 0.5f;
        newDrop.weaponDataRandomier.GenerateOnSpawn(deadWeapon, npcWeapon);
    }
}