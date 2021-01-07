using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;

public class MobDeerActivator : MonoBehaviour
{
    public MobWallsJumperMovement jumpController;
    public MobGroundMovement groundMovement;
    public List<SmoothFollowTarget> deerParts;
    public List<LookAtTarget> lookAtTargetScripts;
    public AudioSource deerAudioAmbient;
    public LayerMask searchLayerMask;
    public NavMeshAgent agent;
    private float dist = 0;
    public float reactionDistance = 10;
    public Transform raycastOrigin;
    
    public bool aggro = false;
    
    [Header("Set this up if mob is hiding on wall or smthng")]
    public SphereCollider coll;
    public Vector3 initialColliderPosition;
    public Vector3 aggroColliderPosition;

    private PlayerMovement pm;

    RaycastHit searchHit;
    bool authorathive = true;


    
    void OnEnable()
    {
        authorathive = !(GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive &&
                         LevelGenerator.instance && LevelGenerator.instance.levelgenOnHost == false);
        
        pm = PlayerMovement.instance;
        StartCoroutine(GetDistanceToPlayer());
    }

    void Start()
    {
        if (coll)
        {
            coll.center = initialColliderPosition;
        }

        if (GameManager.instance.hub)
            return;
        
        // kill mob if too close to player on start of the game
        if (Vector3.Distance(transform.position, ElevatorController.instance.playerSpawner.transform.position) <= 30)
        {
            print("try to remove deer activator");
            var hc = GetComponent<HealthController>();
            if (hc)
                hc.Damage(hc.healthMax, transform.position, transform.position, null, null, false, null, null, null, true);   
        }
    }
    
    IEnumerator GetDistanceToPlayer()
    {
        Transform targetTransform = PlayerMovement.instance.transform;
        
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                targetTransform = GLNetworkWrapper.instance.GetClosestPlayer(transform.position).transform;
            
            dist = Vector3.Distance(raycastOrigin.position, targetTransform.position);
            if (dist <= reactionDistance)
            {
                //print(dist);
                if (Physics.Raycast(raycastOrigin.position, (targetTransform.position + Vector3.up) - raycastOrigin.position, out searchHit, 100, searchLayerMask))
                {
                    //print(searchHit.collider.gameObject.name);
                    if (searchHit.collider.gameObject.layer == 9 || searchHit.collider.gameObject.layer == 11) // if hit layer HANDS or Units
                    {
                        bool foundPlayer = false;
                            
                        // hit either player or other unit
                        if (searchHit.collider.gameObject.layer == 11)
                        {
                            /*
                            HealthController newHc = searchHit.collider.gameObject.GetComponent<HealthController>();
                            if (newHc && newHc.player)
                                foundPlayer = true;*/
                                
                            MobBodyPart foundPart = searchHit.collider.gameObject.GetComponent<MobBodyPart>();
                            
                            if (foundPart && foundPart.hc.player)
                                foundPlayer = true;
                        }
                        else
                        {
                            foundPlayer = true;
                        }
                        
                        if (foundPlayer)
                            Activate();
                    }
                }
            }
        }
    }

    public void Activate()
    {
        if (!aggro)
        {
            transform.parent = null;
            aggro = true;
            deerAudioAmbient.pitch = Random.Range(0.75f, 1.25f);
            deerAudioAmbient.Play();
            
            if (coll)
            {
                coll.center = aggroColliderPosition;
            }
            
            if (jumpController)
            {
                jumpController.Init();
                jumpController.AttackPlayer();   
            }
            
            if (groundMovement)
            {
                if (authorathive)
                    agent.enabled = true;
                
                groundMovement.enabled = true;
                groundMovement.Init();
                groundMovement.Damaged(pm.hc);   
            }

            if (deerParts.Count > 0)
            {
                for (var index = 0; index < deerParts.Count; index++)
                {
                    var dp = deerParts[index];
                    dp.enabled = true;
                }   
            }

            if (lookAtTargetScripts.Count > 0)
            {
                for (var index = 0; index < lookAtTargetScripts.Count; index++)
                {
                    var lookAtTarget = lookAtTargetScripts[index];
                    lookAtTarget.enabled = true;
                }   
            }
        }
    }
}
