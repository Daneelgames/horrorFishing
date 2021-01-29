using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using PlayerControls;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class FootSteps
{
    public List<AudioClip> walk;
    public List<AudioClip> run;
}

public class PlayerAudioController : MonoBehaviour
{
    public static PlayerAudioController instance;

    public float runNoiseDistance = 50f;

    public AudioSource ambientSource;
    public List<AudioClip> ambients;
    public AudioClip chaseTheme;
    public AudioClip badRepChaseTheme;
    public List<AudioClip> bossAmbients;
    public AudioSource itemSource;
    public AudioSource damagedSource;
    public AudioSource loPitchDamagedSource;
    public AudioSource interactSource;
    public AudioSource weaponReloadingSource;
    public AudioSource heartbeatSource;
    public AudioSource dashSource;
    public AudioSource questSource;
    public AudioSource heavyBreathingSource;
    public AudioSource poisoneffectSource;
    public AudioSource flashBackSource;
    public List<AudioClip> interacts;
    public List<AudioSource> gunShotSources;
    public List<AudioSource> footStepSources;
    public List<AudioClip> footStepWalk;
    public List<AudioClip> footStepRun;
    
    [Header("0 - wood, 1 - metal, 2 - tiles, 3 - snow, 4 - squeeze wood")]
    public List<FootSteps> footStepsList = new List<FootSteps>();
    public AudioClip dash;
    public AudioClip switchWeapon;
    public AudioClip healSound;

    
    [Header("Greater value = more time between steps")]
    public float walkStepFrequencyCoefficient = 5;
    public float tiredStepFrequencyCoefficient = 12;
    public float runStepFrequencyCoefficient = 8;
    public float dashStepFrequencyCoefficient = 1;
    public float dashNoStaminaStepFrequencyCoefficient = 1;
    public float smallStepCooldown = 0.5f;
    private float _timeFromPreviousSmallStep;
    
    [SerializeField] 
    private PlayerMovementStats playerMovementStats;
    
    int lastLeg = 0;
    int lastGunShot = 0;
    float currentStepTime = 0;
    
    /// <summary>
    /// If true character not moved when previous PlaySteps() was called.
    /// Used to find small steps.
    /// </summary>
    private bool _previousFrameWasIdle;

    
    PlayerMovement pm;
    private SpawnController sc;
    PlayerNoiseMaker noiseMaker;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        sc = SpawnController.instance;
    }
    
    private void Update()
    {
        _timeFromPreviousSmallStep += Time.deltaTime;
        if (playerMovementStats.movementState == MovementState.Idle)
            _previousFrameWasIdle = true;

        heavyBreathingSource.volume =
            playerMovementStats.movementState == MovementState.Tired ||
            playerMovementStats.movementState == MovementState.DashingNoStamina
                ? Mathf.Lerp(heavyBreathingSource.volume, 1, Time.deltaTime * 0.3f)
                : Mathf.Lerp(heavyBreathingSource.volume, 0, Time.deltaTime * 0.3f);
    }

    public void GetNewFootSteps(int floorType)
    {
        footStepWalk = new List<AudioClip>(footStepsList[floorType].walk);
        footStepRun = new List<AudioClip>(footStepsList[floorType].run);
    }

    public void Init()
    {
        pm = PlayerMovement.instance;
        noiseMaker = PlayerNoiseMaker.instance;
        ambientSource.loop = true;
        ambientSource.volume = 0.44f;
        heartbeatSource.Play();

        if (!GameManager.instance.hub)
        {
            if (GutProgressionManager.instance.playerFloor == GutProgressionManager.instance.bossFloor)
                ambientSource.clip = bossAmbients[Random.Range(0, bossAmbients.Count)];
            else
                ambientSource.clip = ambients[GameManager.instance.level.ambientIndex];
            
            ambientSource.pitch = Random.Range(0.5f, 1.5f);
            ambientSource.Play();   
        }
    }

    public void PlayHeal()
    {
        gunShotSources[0].clip = healSound;
        gunShotSources[0].pitch = Random.Range(0.75f, 1.25f);
        gunShotSources[0].Play();
    }

    public void PlayPoisonEffect()
    {
        poisoneffectSource.Stop();
        poisoneffectSource.pitch = Random.Range(0.75f, 1.25f);
        poisoneffectSource.Play();
    }
    
    public void PlayDash(bool withoutStamina = false)
    {
        if (!withoutStamina)
        {
            dashSource.pitch = Random.Range(0.75f, 1.25f);
            dashSource.Play();
        }
        else
        {
            //todo: play another sound for dash without stamina 
            dashSource.pitch = Random.Range(0.75f, 1.25f);
            dashSource.Play();
        }
    }
    
    public void PlaySteps() // every frame 
    {
        pm = PlayerMovement.instance;
        currentStepTime += Time.deltaTime;

        switch (playerMovementStats.movementState)
        {
            case MovementState.Idle:
                _previousFrameWasIdle = true;
                if (currentStepTime >= walkStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed)
                {
                    PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.75f);
                }
                else
                    currentStepTime = 0;
                break;
            
            case MovementState.Tired:
                if (currentStepTime >= tiredStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed) 
                    PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.75f);
                else if (_previousFrameWasIdle && _timeFromPreviousSmallStep > smallStepCooldown)
                {
                    PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.75f);
                    _timeFromPreviousSmallStep = 0;
                }
                _previousFrameWasIdle = false;
                break;

            case MovementState.Walking:
                if (!playerMovementStats.isRunning) //Walk
                {
                    if (currentStepTime >= walkStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed)
                    {
                        if (!pm.crouching)
                        {
                            PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.75f);
                            
                            if (SpawnController.instance)
                            {
                                if (!PlayerSkillsController.instance.softFeet)
                                    SpawnController.instance.MobHearNoise(transform.position, pm.walkNoiseDistance);
                                else
                                    SpawnController.instance.MobHearNoise(transform.position, pm.walkNoiseDistance/ 2);   
                            }   
                        }
                        else
                        {
                            PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.5f);
                        }
                    }
                    else if(_previousFrameWasIdle && _timeFromPreviousSmallStep > smallStepCooldown)
                    {
                        PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.5f);
                        _timeFromPreviousSmallStep = 0;
                    }
                    _previousFrameWasIdle = false;
                }
                else //Run
                {
                    if ((!pm.hc.wc.activeWeapon || pm.hc.wc.activeWeapon.attackingMelee == false)
                        && currentStepTime >= runStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed)
                    {
                        if (SpawnController.instance)
                        {
                            if (!PlayerSkillsController.instance.softFeet)
                                SpawnController.instance.MobHearNoise(transform.position, pm.runNoiseDistance);
                            else
                                SpawnController.instance.MobHearNoise(transform.position, pm.runNoiseDistance / 2);   
                        }
                        
                        PlayStep(footStepRun[Random.Range(0, footStepRun.Count - 1)], 1);
                    }
                    else if(_previousFrameWasIdle && _timeFromPreviousSmallStep > smallStepCooldown)
                    {
                        PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 1);
                        _timeFromPreviousSmallStep = 0;
                    }
                    _previousFrameWasIdle = false;
                }
                break;
                
            case MovementState.Dashing:
                _previousFrameWasIdle = false;
                if ((!pm.hc.wc.activeWeapon || pm.hc.wc.activeWeapon.attackingMelee == false) && currentStepTime >= dashStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed)
                    PlayStep(footStepRun[Random.Range(0, footStepRun.Count - 1)], 1);
                break;
            
            case MovementState.DashingNoStamina:
                _previousFrameWasIdle = false;
                if ((!pm.hc.wc.activeWeapon || pm.hc.wc.activeWeapon.attackingMelee == false) && currentStepTime >= dashNoStaminaStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed)
                    PlayStep(footStepRun[Random.Range(0, footStepRun.Count - 1)], 1);
                break;
                
            default:
                throw new ArgumentOutOfRangeException(nameof(playerMovementStats.movementState), "Unexpected enum value");
        }
    }

    public void PlaySwitchWeapon()
    {
        interactSource.Stop();
        interactSource.clip = switchWeapon;
        interactSource.pitch = Random.Range(0.75f, 1.25f);
        interactSource.Play();
    }

    public void PlayBrokehWeapon()
    {
        // PLAY BROKEN SOUND
    }

    void PlayStep(AudioClip step, float volume)
    {
        currentStepTime = 0;
        if (lastLeg == 0) lastLeg = 1;
        else lastLeg = 0;

        footStepSources[lastLeg].volume = volume;
        footStepSources[lastLeg].pitch = Random.Range(0.75f, 1.25f);
        footStepSources[lastLeg].clip = step;
        footStepSources[lastLeg].Play();
    }

    public void PlayInteract()
    {
        interactSource.clip = interacts[Random.Range(0, interacts.Count - 1)];
        interactSource.pitch = Random.Range(0.75f, 1.25f);
        interactSource.Play();
    }

    public void PlaySwingSound(AudioClip clip)
    {
        if (lastGunShot == 0) lastGunShot = 1;
        else lastGunShot = 0;

        gunShotSources[lastGunShot].pitch = Random.Range(0.75f, 1.25f);
        gunShotSources[lastGunShot].clip = clip;
        gunShotSources[lastGunShot].Play();
    }

    public void PlayShotSound(AudioClip clip, int repeats)
    {
        StartCoroutine(PlayShotSoundOverTime(clip, repeats));
    }

    IEnumerator PlayShotSoundOverTime(AudioClip clip, int repeats)
    {
        for (int i = 0; i <= repeats; i++)
        {
            gunShotSources[lastGunShot].pitch = Random.Range(0.75f, 1.25f);
            gunShotSources[lastGunShot].clip = clip;
            gunShotSources[lastGunShot].Play();
            
            lastGunShot++;
            if (lastGunShot >= gunShotSources.Count)
                lastGunShot = 0;
            yield return new WaitForSecondsRealtime(Random.Range(0.005f, 0.01f));
        }
    }

    public void ItemSound(AudioClip clip)
    {
        itemSource.clip = clip;
        itemSource.pitch = Random.Range(0.75f, 1.25f);
        itemSource.Play();
    }
    public void PlayReload(AudioClip clip)
    {
        weaponReloadingSource.loop = true;
        weaponReloadingSource.clip = clip;
        weaponReloadingSource.Play();
    }

    public void StopReload(AudioClip clip)
    {
        weaponReloadingSource.Stop();
        weaponReloadingSource.loop = false;
        if (clip)
        {
            weaponReloadingSource.clip = clip;
            weaponReloadingSource.Play();
        }
    }

    public void Damaged()
    {
        if (GameManager.instance.lowPitchDamage == 1)
        {
            loPitchDamagedSource.Stop();
            loPitchDamagedSource.pitch = Random.Range(0.5f, 1.5f);
            loPitchDamagedSource.Play();
            return;
        }
        damagedSource.Stop();
        damagedSource.pitch = Random.Range(0.5f, 1.5f);
        damagedSource.Play();
    }

    public void ToggleQuests()
    {
        questSource.Stop();
        questSource.pitch = Random.Range(0.5f, 1.5f);
        questSource.Play();
    }

    public void StartChase(bool repChase)
    {
        if (repChase)
        {
            ambientSource.clip = badRepChaseTheme;   
            ambientSource.pitch = Random.Range(0.5f, 1f);
        }
        else
        {
            ambientSource.clip = chaseTheme;   
            ambientSource.pitch = Random.Range(0.75f, 1.5f);
        }
        
        ambientSource.priority = 2;
        var elevator = ElevatorController.instance;
            if (elevator)
                elevator.StopMusic();
        ambientSource.volume = 0.75f;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    public void FlashBackOver()
    {
        flashBackSource.pitch = Random.Range(0.5f, 1.5f);
        flashBackSource.Play();
    }
    
    public void EndChase()
    {
        ambientSource.Stop();
        GameManager.instance.finishLevelJingleSource.Play();
    }
}