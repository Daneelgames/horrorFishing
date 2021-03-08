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
    public List<GameObject> gameObjectsToActivate = new List<GameObject>();
    public HealthController hcToTurnInvincibilityOff;
    
    public void MobStep(int index)
    {
        mobAu.Step();
    }

    public void ActivateGameObjects()
    {
        for (var index = 0; index < gameObjectsToActivate.Count; index++)
        {
            gameObjectsToActivate[index].SetActive(true);
        }
    }
    
    public void PlayShotParticle()
    {
        shotAu.pitch = Random.Range(0.75f, 1f);
        rangeShotParticles.Play();
        shotAu.Play();
    }

    public void TurnInvincibilityOff()
    {
        hcToTurnInvincibilityOff.invincible = false;
    }
    
    public void MakeBoss()
    {
        hcToTurnInvincibilityOff.boss = true;
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

    public void IntroCompleted()
    {
        StartCoroutine(GameManager.instance.IntroCompleted());
    }

    public void KillGunn()
    {
        var gunn = HubItemsSpawner.instance.gunnWalkableWithHeads;
        if (gunn == null) return;
        gunn.invincible = false;
        gunn.Kill();
        HubItemsSpawner.instance.gunnWalkableWithHeads = null;
        HubItemsSpawner.instance.SpawnFinishedBoat();
    }

    public void CompleteGame()
    {
        StartCoroutine(GameManager.instance.GameCompleted());
    }

    public void DisablePlayerInput()
    {
        PlayerMovement.instance.canControl = false;
    }
}