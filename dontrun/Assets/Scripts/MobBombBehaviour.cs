using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using CerealDevelopment.TimeManagement;
using UnityEngine.AI;

public class MobBombBehaviour : MonoBehaviour, IUpdatable
{
    public NavMeshAgent agent;

    private PlayerMovement pm;
    public AudioSource ambientSource;
    public float deathAfterStopSeconds = 1;

    private HealthController hc;
    public bool followPlayer = false;

    public MobLifterController currentMobLifter;
    
    HealthController target;
    private bool authorative = true;
    
    void Start()
    {
        pm = PlayerMovement.instance;
        hc = GetComponent<HealthController>();
        
        /*
        if (Random.value > 0.5f)
            followPlayer = true;
        */

        if (!GLNetworkWrapper.instance || GLNetworkWrapper.instance.coopIsActive == false)
        {
            // SOLO
            target = pm.hc;
            MoveBomb(target.transform.position);   
        }
        else if (LevelGenerator.instance.levelgenOnHost)
        {
            // Host
            target = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
            MoveBomb(target.transform.position);
        }
        else
        {
            authorative = false;
        }
    }

    public void MoveBomb(Vector3 targetPost)
    {
        targetPost += new Vector3(Random.Range(-8, 8), 0, Random.Range(-8, 8));
        agent.SetDestination(targetPost);
        StartCoroutine(CheckDistance(targetPost));
    }
    
    private void OnEnable()
    {
        this.EnableUpdates();
    }

    private void OnDisable()
    {
        this.DisableUpdates();
    }

    void IUpdatable.OnUpdate()
    {
        if (!authorative) return;
        
        if (currentMobLifter != null)
            SetAgentBaseOffset(currentMobLifter.height);
        else
            SetAgentBaseOffset(0);
    }

    void SetAgentBaseOffset(float newOffset)
    {
        if (agent.baseOffset < newOffset - 0.05f)
            agent.baseOffset += Time.deltaTime * 5;
        else if (agent.baseOffset > newOffset + 0.05f)
            agent.baseOffset -= Time.deltaTime * 5;
    }
    IEnumerator CheckDistance(Vector3 _target)
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            /*
            if (followPlayer)
            {
                agent.SetDestination(target.transform.position);
            }*/
            
            if (Vector3.Distance(_target, transform.position) <= 1)
            {
                break;
            }
        }
        ambientSource.Stop();
        yield return new WaitForSeconds(deathAfterStopSeconds);
        hc.Kill();
    }
}
