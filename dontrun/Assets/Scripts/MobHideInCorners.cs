using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]
public class LevelParametersMobHideInCorners
{
    public int speed = 20;
    public float distanceToPlayerToStartRunning = 10;
    public float distanceToHideFromPlayer = 30;
    public float distanceToHidingSpotMin = 20;
    public float distanceToHidingSpotMax = 50;
}
public class MobHideInCorners : MonoBehaviour
{
    public enum State {Hiding, Running }
    public State mobState = State.Hiding;
    public bool seePlayer = false;

    public List<LevelParametersMobHideInCorners> levels;
    
    public LayerMask searchLayerMask;
    public NavMeshAgent agent;
    NavMeshPath path;
    public MobWitchFireAttack fireAttack;

    public Animator spriteAnim;

    [HideInInspector]
    public HealthController hc;
    LevelGenerator lg;
    private GameManager gm;
    PlayerMovement pm;
    RaycastHit searchHit;
    public MobPartsController mobParts;
    Coroutine stopRunning;
    public float visionDistance = 50;
    public HealthController target;

    private void Start()
    {
        gm = GameManager.instance;
        lg = LevelGenerator.instance;
        pm = PlayerMovement.instance;
        path = new NavMeshPath();
        StartCoroutine(SearchPlayer());
    }

    IEnumerator GetDistanceToTarget()
    {
        while (hc.health > 0)
        {
            yield return new WaitForSeconds(3f);

            if (hc.health > 0)
            {
                agent.isStopped = true;
                mobState = State.Hiding;   
            }
        }
    }
    IEnumerator SearchPlayer() // target
    {
        while(hc.health > 0)
        {
            if (hc.inLove)
            {
                target  = gm.GetClosestUnit(transform.position, false);
            }
            else
            {
                target = pm.hc;
            }
            
            if (mobState == State.Hiding && !hc.peaceful && target)
            {
                if (Physics.Raycast(transform.position + Vector3.up, target.transform.position - transform.position, out searchHit, visionDistance, searchLayerMask))
                {
                    if (searchHit.collider.gameObject.layer == 9)
                    {
                        seePlayer = true;
                        fireAttack.SeePlayer(this);
                        if (Vector3.Distance(transform.position, target.transform.position) <= levels[GetLevel()].distanceToPlayerToStartRunning)
                            FindNewCorner();   
                    }
                }
                else
                    seePlayer = false;

            }
            yield return new WaitForSeconds(1f);
        }
    }

    int GetLevel()
    {
        return Mathf.Clamp(mobParts.level, 0, levels.Count - 1);
    }

    public void Damaged(HealthController damager)
    {
        if (damager) target = damager;
            
        // FIND NEW HIDING SPOT
        if (hc.health > 0)
            FindNewCorner();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 18) // hit door
        {
            DoorController newDoor = other.gameObject.GetComponent<DoorController>();
            if (newDoor != null)
            {
                if (!newDoor.open)
                {
                    newDoor.OpenDoor(transform.position);   
                }
                else
                {
                    newDoor.CloseDoor();
                }
            }
        }
    }

    public void FindNewCorner()
    {
        if (GameManager.instance.hub)
        {
            
            NavMeshHit hit;
            var newPos = transform.position + Random.insideUnitSphere * 50;
                            
            if (NavMesh.SamplePosition(newPos, out hit, 50.0f, NavMesh.AllAreas))
            {
                MoveCloser(hit.position);
            }   
        }
        else
        {
            List<TileController> tempTiles = new List<TileController>(lg.levelTilesInGame);

            for (int i = tempTiles.Count - 1; i >=0; i--)
            {
                float distance = Vector3.Distance(transform.position, tempTiles[i].transform.position);
                float distanceToPlayer = Vector3.Distance(target.transform.position, tempTiles[i].transform.position);
                if (tempTiles[i].tileStatusEffect != StatusEffects.StatusEffect.Null || distance < levels[GetLevel()].distanceToHidingSpotMin || distance > levels[GetLevel()].distanceToHidingSpotMax || distanceToPlayer < levels[GetLevel()].distanceToHideFromPlayer)
                {
                    tempTiles.RemoveAt(i);
                }
            }

            if (tempTiles.Count > 0)
            {
                int r = Random.Range(0, tempTiles.Count);
                SetPath(tempTiles[r].transform.position);
            }
        }
        
        mobState = State.Running;

        if (stopRunning != null)
            StopCoroutine(stopRunning);

        stopRunning = StartCoroutine(GetDistanceToTarget());
    }

    void SetPath(Vector3 targetPos)
    {
        agent.CalculatePath(targetPos, path);
        agent.speed = levels[GetLevel()].speed;
        agent.SetPath(path);
        agent.isStopped = false;
    }

    public void MoveCloser(Vector3 newPos)
    {
        mobState = State.Hiding;
        agent.isStopped = true;
        
        agent.CalculatePath(newPos, path);
        agent.SetPath(path);
        agent.isStopped = false;
    }
    public void Death()
    {
        agent.isStopped = false;
        agent.enabled = false;
        StopCoroutine(SearchPlayer());
        StopCoroutine(GetDistanceToTarget());
    }
}