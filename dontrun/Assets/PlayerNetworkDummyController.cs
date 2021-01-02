using System;
using System.Collections;
using System.Collections.Generic;
using FirstGearGames.Mirrors.Assets.FlexNetworkTransforms;
using Mirror;
using Steamworks;
using UnityEngine;

public class PlayerNetworkDummyController : NetworkBehaviour
{
    [SyncVar] public int player = 0;
    [SyncVar] public float currentHp = 1000;

    public static PlayerNetworkDummyController hostInstance;
    public static PlayerNetworkDummyController clientInstance;
    public HealthController targetPlayer;
    public GameObject visualAndSound;
    public HealthController hc;
    public Transform ikSpineBone;
    public Animator dummyAnim;
    private string speedString = "Speed";

    private float checkMovingDelay = 0.1f;
    private float targetAnimSpeed = 0f;
    private float currentAnimSpeed = 0f;
    private float checkMovingDistanceThreshold = 0.1f;
    public bool hiding = false;

    private float checkDeathCooldown = 2;

    public Camera spectatorCam;

    public DummyWeaponControls dwc;

    // INIT
    void Awake()
    {
        hc = gameObject.GetComponent<HealthController>();
        if (hostInstance == null)
            hostInstance = this;
        else
            clientInstance = this;

        Invoke("GetPlayer", 1);
    }

    public void SetHiding(bool crouch)
    {
        hiding = crouch;
    }
    
    void GetPlayer()
    {
        if (player == 0 && hostInstance == this && GLNetworkWrapper.instance.localPlayer.isServer)
            targetPlayer = GameManager.instance.player;
        else if (player == 1 && clientInstance == this && !GLNetworkWrapper.instance.localPlayer.isServer)
            targetPlayer = GameManager.instance.player;
        
        // activate visuals on client
        if (targetPlayer == null)
            visualAndSound.SetActive(true);

        GLNetworkWrapper.instance.playerNetworkObjects[player].connectedDummy = this;
    }


    void Start()
    {
        CmdSetHealth(1000, -1);

        if (targetPlayer == null)
        {
            StartCoroutine(GetMoving());
        }
    }

    IEnumerator GetMoving()
    {
        Vector3 prevPos = transform.position;
        Vector3 newPos = transform.position;
        
        while (true)
        {
            yield return new WaitForSeconds(checkMovingDelay);
            
            newPos = transform.position;
            if (Vector3.Distance(newPos, prevPos) > checkMovingDistanceThreshold)
            {
                targetAnimSpeed = 1;
            }
            else
            {
                targetAnimSpeed = 0;
            }

            prevPos = newPos;
             
        }
    }
    
    void Update()
    {
        if (checkDeathCooldown <= 0)
        {
            if (currentHp <= 0 && hc.health > 0)
            {
                hc.Kill();
            }   
        }
        else
        {
            checkDeathCooldown -= Time.deltaTime;
        }

        if (targetPlayer != null)
        {
            transform.position = targetPlayer.transform.position;
            //transform.rotation = targetPlayer.pm.movementTransform.rotation;
            Vector3 newRot = new Vector3(0, targetPlayer.pm.movementTransform.rotation.y, 0);
            transform.eulerAngles = newRot;

            if (!hiding)
            {
                ikSpineBone.transform.eulerAngles = new Vector3(0, targetPlayer.pm.mouseLook.transform.eulerAngles.y + 90, targetPlayer.pm.mouseLook.transform.eulerAngles.x);   
            }
            else
            {
                ikSpineBone.transform.eulerAngles = new Vector3(0, targetPlayer.pm.mouseLook.transform.eulerAngles.y + 90, 0);   
            }
        }
        else
        {
            currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, targetAnimSpeed, Time.deltaTime);
            dummyAnim.SetFloat(speedString, targetAnimSpeed);
        }
    }


    [Server]
    public void CmdSetHealth(float value, int effect)
    {
        // only for dummies
        SetHealthOnClient(value, false, effect);
    }

    //[Client]
    public void DummyHealthChanged(float value, int effect)
    {
        GLNetworkWrapper.instance.DummyHealthChanged(player, value, effect);
    }

    public void ChangeHealthOnClient(float healthOffset, int effect)
    {
        // only for dummies
        currentHp = Mathf.Max(currentHp + healthOffset, 0);
        SetHealthOnClient(currentHp, false, effect);
    }
    
    public void SetHealthOnClient(float value, bool affectBadRep, int effect)
    {
        currentHp = value;
        hc.health = currentHp;
        print("TARGET PLAYER IS " + targetPlayer + ". Effect is " + effect);
        if (targetPlayer)
        {
            switch (effect)
            {
                case 0:
                    targetPlayer.InstantPoison();
                    break;
                case 1:
                    targetPlayer.StartFire();
                    break;
                case 2:
                    targetPlayer.InstantBleed();
                    break;
                case 3:
                    targetPlayer.InstantRust();
                    break;
                case 4:
                    targetPlayer.InstantRegen();
                    break;
                case 5:
                    targetPlayer.InstantGoldHunger();
                    break;
                case 6:
                    targetPlayer.InstantCold();
                    break;
                case 7:
                    targetPlayer.InstantLove();
                    break;
            }
            
            targetPlayer.health = currentHp;
            
            if (UnityEngine.Random.value > 0.1f)
                targetPlayer.pm.cameraAnimator.SetTrigger("Damage");
            else
                targetPlayer.pm.cameraAnimator.SetTrigger("Earthquake");
                
            targetPlayer.pac.Damaged();
            UiManager.instance.UpdateHealthbar();
            
            if (targetPlayer.health <= 0 || currentHp <= 0)
            {
                targetPlayer.Damage(targetPlayer.healthMax, targetPlayer.transform.position, targetPlayer.transform.position,
                    null,null,affectBadRep, null, null,null,true);
            }
        }

        if (currentHp <= 0)
        {
            hc.mobPartsController.Death();   
        }
    }

    public void Resurrect(float newHealth)
    {
        CmdResurrect(newHealth);
    }

    [Command]
    void CmdResurrect(float newHealth)
    {
        currentHp = newHealth;
        RpcResurrect(newHealth);
    }

    [ClientRpc]
    void RpcResurrect(float newHealth)
    {
        currentHp = newHealth;
        print("RESSURECT");
        hc.health = currentHp;
        hc.mobPartsController.Resurrect();

        checkDeathCooldown = 2;
    }
}