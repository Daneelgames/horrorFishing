using System.Collections;
using System.Collections.Generic;
using CerealDevelopment.TimeManagement;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;

public class MobCharacterControllerMovement : MonoBehaviour
{
    public enum MovementType
    {
        RunAway, Follow
    }

    public float followDistance = 50;

    public MovementType movementType = MovementType.RunAway;

    private PlayerMovement pm;
    public Animator anim;
    public NavMeshAgent agent;

    private string moveString = "Move";

    private Coroutine followCoroutine;
    private Coroutine runawayCoroutine;

    private GameManager gm;
    public LayerMask layerMask;
    public Transform raycastOrigin;

    void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
    }

    private void OnEnable()
    {
        movementType = MovementType.Follow;
        
        if (followCoroutine != null) StopCoroutine(followCoroutine);
            followCoroutine = StartCoroutine(AgentFollowPlayer());
    }

    public void RunAway(float secondsToFollowPlayer)
    {
        if (followCoroutine != null) StopCoroutine(followCoroutine);
        movementType = MovementType.RunAway;
        runawayCoroutine = StartCoroutine(RunAway());
        
        
        if (secondsToFollowPlayer > 0)
            Invoke("FollowPlayer", secondsToFollowPlayer);
    }

    IEnumerator RunAway()
    {
        while (gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(1);
            Vector3 runPos = transform.position + (transform.position - pm.transform.position).normalized * 10;

            if (gameObject.activeInHierarchy && agent.enabled)
            {
                NavMeshHit hit;
                if (Vector3.Distance(transform.position, pm.transform.position) <= followDistance &&
                    NavMesh.SamplePosition(runPos, out hit, 10.0f, NavMesh.AllAreas))
                {
                    anim.SetBool(moveString, true);
                    agent.SetDestination(runPos);
                }
                else
                {
                    agent.SetDestination(transform.position);   
                    anim.SetBool(moveString, false);
                }   
            }
        }
    }
    
    public void FollowPlayer()
    {
        movementType = MovementType.Follow;
        if (followCoroutine != null) StopCoroutine(followCoroutine);
        
            followCoroutine = StartCoroutine(AgentFollowPlayer());
    }
    
    IEnumerator AgentFollowPlayer()
    {
        while (gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(1);
            if (agent.enabled && gameObject.activeInHierarchy)
            {
                pm = PlayerMovement.instance;
                if (Vector3.Distance(transform.position, pm.transform.position) <= followDistance)
                {
                    RaycastHit raycastHit;
                    if (Physics.Raycast(raycastOrigin.position, (pm.transform.position + Vector3.up) - raycastOrigin.position, out raycastHit, followDistance * 0.75f, layerMask))
                    {
                        if (raycastHit.collider.gameObject.layer == 9)
                        {
                            NavMeshHit hit;
                            var playerPos = new Vector3(pm.transform.position.x, pm.transform.position.y, pm.transform.position.z);
                            
                            if (NavMesh.SamplePosition(playerPos, out hit, 30.0f, NavMesh.AllAreas))
                            {
                                var newPos = hit.position;
                                agent.SetDestination(newPos);
                                anim.SetBool(moveString, true);
                            }   
                        }
                    }
                }
                else
                {
                    agent.SetDestination(transform.position);
                    anim.SetBool(moveString, false);   
                }   
            }
        }
    }
}
