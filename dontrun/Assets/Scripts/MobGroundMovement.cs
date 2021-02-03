using System;
using System.Collections;
using System.Collections.Generic;
using CerealDevelopment.TimeManagement;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]
public class MobGroundMovementStats
{
    public float sightDistance = 10;
    public float sightDistanceIdleMaximum = 30;
    public float sightDistanceGrowRate = 0.5f;
    public float sightDistanceChase = 100;
    public float idleSpeed = 1;
    public float chaseSpeed = 1;
    public float chaseTime = 20;
    public float hideDistanceFromPlayerMin = 10;
    public float hideDistanceFromPlayerMax = 50;

}
public class MobGroundMovement : MonoBehaviour, IUpdatable
{
    public MobPartsController mobParts;
    public enum State {Idle, Chase, Death };
    public State monsterState = State.Idle;

    public List<MobGroundMovementStats> levels;
    GameManager gm;

    public NavMeshAgent agent;

    float sightDistanceCurrent = 10;
    float targetSpeed = 1;
    
    public NavMeshObstacle obstacle;

    public HealthController hc;
    public bool stunned = false;

    public MobProjectileShooter projectileShooter;

    public Transform raycastOrigin;
    public LayerMask searchLayerMask;
    public MobAudioManager mobAu;

    Vector3 currentPatrolTarget;
    Vector3 lastKnownPlayerPosition;
    NavMeshPath path;
    Coroutine followPlayer;
    Coroutine hideCoroutine;
    Coroutine hideAfterTimeCoroutine;
    ItemsList il;

    [Header("Speed scales by this")]
    public float coldModifier = 1;    
    public float limbsModifier = 1;    
    
    public HealthController target;

    private GutProgressionManager gpm;

    private string chaseString = "Chase";                         
    private string peacefulString = "Peaceful";               
    
    public MobLifterController currentMobLifter;
    
    bool authorathive = true;
    public bool rotateToTargetInUpdate = true;


    void Start()
    {
        /*
        gm = GameManager.instance;
        gpm = GutProgressionManager.instance;
        agent = GetComponent<NavMeshAgent>();
        il = ItemsList.instance;
        path = new NavMeshPath();
        hc = GetComponent<HealthController>();*/
        
        authorathive = !(GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive &&
                         LevelGenerator.instance && LevelGenerator.instance.levelgenOnHost == false);
        Init();
    }

    public void Init()
    {
        gm = GameManager.instance;
        gpm = GutProgressionManager.instance;
        agent = GetComponent<NavMeshAgent>();
        il = ItemsList.instance;
        path = new NavMeshPath();
        hc = GetComponent<HealthController>();


        if (authorathive)
        {
            targetSpeed = levels[GetLevel()].idleSpeed;
            agent.speed = targetSpeed * coldModifier * limbsModifier;   
            
            if (hc.peaceful) mobParts.anim.SetBool(peacefulString, true);
            else if (gm)
            {
                if (hc.npcInteractor) 
                    hc.npcInteractor.gameObject.SetActive(false);
                
                target = gm.player;
                //target = gm.GetClosestUnit(transform.position, true, hc);
            
                mobParts.anim.SetBool(peacefulString, false);   
            }
        }
        else
        {
            agent.enabled = false;
        }
        

        if (hc.boss)
        {
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                target = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);   
            }
            else
            {
                target = gm.player;
            }
        }

        
        StartCoroutine(OpenDoors());   
        
        mobAu.IdleAmbient();
    }

    int GetLevel()
    {
        return Mathf.Clamp(mobParts.level, 0, levels.Count - 1);
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
        if (!target || (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && GLNetworkWrapper.instance.localPlayer.isServer == false))
            return;

        if (hc.health > 0 && gameObject.activeInHierarchy)
        {
            if (coldModifier < 1)
                coldModifier += Time.deltaTime / 5;

            if (target.health < target.healthMax * 0.2f)
                limbsModifier = 0.1f;
            else if (mobParts.ikMonsterAnimator)
                limbsModifier = mobParts.ikMonsterAnimator.GetLimbsSpeedModifier();
            
            if (rotateToTargetInUpdate && monsterState == State.Chase && agent.enabled)
            {
                if (Vector3.Distance(transform.position, target.transform.position) < agent.stoppingDistance)
                {
                    Vector3 direction = (target.transform.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    Vector3 newRot = lookRotation.eulerAngles;
                    newRot.x = 0;
                    newRot.z = 0;
                    lookRotation.eulerAngles = newRot;
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 360);
                    /*
                    if (mobParts.ikMonsterAnimator.animate == false)
                        mobParts.ikMonsterAnimator.SetAnimate(true);*/
                }
                //agent.speed = Mathf.Lerp(agent.speed, targetSpeed * coldModifier, 2 * Time.deltaTime);   
            }

            if (agent)
            {
                if (currentMobLifter != null)
                    SetAgentBaseOffset(currentMobLifter.height);
                else
                    SetAgentBaseOffset(0);   
            }
        }
    }

    void SetAgentBaseOffset(float newOffset)
    {
        if (agent.baseOffset < newOffset - 0.05f)
        {
            // mob climb up
            agent.speed = targetSpeed * coldModifier * limbsModifier * 0.33f;
            agent.baseOffset += Time.deltaTime * 5;   
        }
        else if (agent.baseOffset > newOffset + 0.05f)
        {
            // mob goes down
            agent.speed = targetSpeed * coldModifier * limbsModifier * 0.75f;
            agent.baseOffset -= Time.deltaTime * 5;   
        }
        else
        {
            agent.speed = Mathf.Lerp(agent.speed, targetSpeed * coldModifier * limbsModifier, 2 * Time.deltaTime);   
        } 
    }

    public void Damaged(HealthController damager)
    {
        if (damager)
            target = damager;
        else
            target  = GameManager.instance.GetClosestUnit(transform.position, false, hc);
        
        if (hideAfterTimeCoroutine != null)
            StopCoroutine(hideAfterTimeCoroutine);

        if (authorathive)
        {
            mobParts.anim.SetBool(peacefulString, false);
            if (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString)
                mobParts.anim.SetBool(chaseString, true);  
            
            if (mobParts.ikMonsterAnimator)
                mobParts.ikMonsterAnimator.SetAnimate(true); 
        }
        
        //targetSpeed += Random.Range(0, targetSpeed);
        mobAu.Damage();
        if (monsterState == State.Idle)
        {
            if (damager != null && damager.player)
            {
                followPlayer = StartCoroutine(FollowPlayer());
                hideCoroutine = StartCoroutine(CheckHide());  
                
                /*
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    if (LevelGenerator.instance.levelgenOnHost)
                    {
                        // HOST
                    }
                }
                else
                {
                    // SOLO
                    followPlayer = StartCoroutine(FollowPlayer());
                    hideCoroutine = StartCoroutine(CheckHide());    
                } */
            }
            else
            {
                Hide(false);
                
                /*
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    if (LevelGenerator.instance.levelgenOnHost)
                    {
                        // HOST
                        Hide(false);
                    }
                }
                else
                {
                    // SOLO
                    Hide(false);
                } */
            }
        }
    }

    IEnumerator OpenDoors() // and search for player
    {
        RaycastHit searchHit;
        
        while (hc.health > 0)
        {
            if (!hc.inLove)
            {
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    target = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
                }
                
                if (monsterState == State.Idle)
                {
                    if (!mobAu.idleAu.isPlaying)
                    {
                        mobAu.IdleAmbient();
                    }
                }
                else if (monsterState == State.Chase)
                {
                    if (!mobAu.chaseAu.isPlaying)
                    {
                        mobAu.ChaseAmbient();
                    }
                }
                
                if (target && !hc.peaceful)
                {
                    if (sightDistanceCurrent > 0 && Physics.Raycast(raycastOrigin.position, (target.transform.position + Vector3.up) - raycastOrigin.position, out searchHit, sightDistanceCurrent, searchLayerMask))
                    {
                        //print(gameObject.name + " hit " + searchHit.collider.gameObject.name + " while was checking for " + target.gameObject.name);
                        if (searchHit.collider.gameObject.layer == 18) // hit door
                        {
                            if (Vector3.Distance(transform.position, searchHit.collider.gameObject.transform.position) < 10)
                            {
                                DoorController newDoor = searchHit.collider.gameObject.GetComponent<DoorController>();
                                if (newDoor != null)
                                {
                                    if (!newDoor.open)
                                    {
                                        newDoor.OpenDoor(transform.position);   
                                    }
                                }
                            }
                        }
                        //else if (searchHit.collider.gameObject == gm.player.gameObject)
                        else if (searchHit.collider.gameObject.layer == 9 || searchHit.collider.gameObject.layer == 11)
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
                            
                                if (foundPart && foundPart.hc && foundPart.hc.player)
                                    foundPlayer = true;
                            }
                            else
                            {
                                foundPlayer = true;
                            }

                            if (foundPlayer)
                            {
                                if (followPlayer == null)
                                {
                                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                                    {
                                        if (LevelGenerator.instance.levelgenOnHost)
                                        {
                                            // HOST
                                            followPlayer = StartCoroutine(FollowPlayer());  
                                        }
                                    }
                                    else
                                    {
                                        // SOLO
                                        followPlayer = StartCoroutine(FollowPlayer());    
                                    } 
                                }

                                if (hideCoroutine == null)
                                {
                                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                                    {
                                        if (LevelGenerator.instance.levelgenOnHost)
                                        {
                                            // HOST
                                            hideCoroutine = StartCoroutine(CheckHide());
                                        }
                                    }
                                    else
                                    {
                                        // SOLO
                                        hideCoroutine = StartCoroutine(CheckHide());
                                    } 
                                }   
                            }
                        }
                    }

                    if (sightDistanceCurrent > 0 && Vector3.Distance(transform.position, target.transform.position) <= levels[GetLevel()].sightDistanceIdleMaximum)
                    {
                        if (sightDistanceCurrent < levels[GetLevel()].sightDistanceIdleMaximum)
                        {
                            if (!gm.player.pm.crouching)
                                sightDistanceCurrent += levels[GetLevel()].sightDistanceGrowRate;
                            else
                                sightDistanceCurrent += levels[GetLevel()].sightDistanceGrowRate * 0.33f;
                        }
                    }
                    else
                    {
                        if (sightDistanceCurrent > levels[GetLevel()].sightDistance)
                            sightDistanceCurrent -= levels[GetLevel()].sightDistanceGrowRate;
                    }
                }
                ///////////
                ///
                
                if (target && target.player && target.health < target.healthMax / 3)
                    yield return new WaitForSeconds(2);
                else
                    yield return new WaitForSeconds(1);
                if (gm == null)
                    yield break;
                
                if (!hc.npcInteractor || !hc.npcInteractor.gameObject.activeInHierarchy)
                {
                    var newTarget = gm.player;
                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                        newTarget = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
                    
                    //float newDist = Vector3.Distance(transform.position, gm.player.transform.position);
                    float newDist = Vector3.Distance(transform.position, newTarget.transform.position);

                    if (newDist < 50)
                    {
                        if (hc.inLove)
                        {
                            target  = gm.GetClosestUnit(transform.position, false, hc);
                        }
                        else
                        {
                            target = newTarget;
                        }

                        agent.Warp(transform.position);
                        // this makes mobs to stop if they pass near the player
                        bool access = ((agent.enabled && agent.remainingDistance > 5) || agent.enabled == false);
                        
                        if (monsterState == State.Idle && access) // if mob moves for hide
                        {
                            if (newDist > 30 && Random.value > 0.9f)
                            {   
                                NavMeshHit hit;
                                var pm = PlayerMovement.instance; 
                                var playerPos = new Vector3(pm.transform.position.x, pm.transform.position.y, pm.transform.position.z) + Random.insideUnitSphere * 30;
                            
                                if (NavMesh.SamplePosition(playerPos, out hit, 30.0f, NavMesh.AllAreas))
                                {
                                    MoveCloser(hit.position, true);
                                }   
                            }
                            else
                            {
                                // raycast to player
                                if (Physics.Raycast(raycastOrigin.position,
                                    (target.transform.position + Vector3.up) - raycastOrigin.position, out searchHit,
                                    100, searchLayerMask))
                                {
                                    if (searchHit.collider.gameObject == gm.player.gameObject)
                                    {
                                        if (authorathive)
                                            agent.isStopped = true;
                                    
                                        mobAu.IdleAmbient();
                                    
                                        if (authorathive && (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString))
                                            mobParts.anim.SetBool(chaseString, false);
                                    }
                                }
                            }
                        }
                    }
                }

            }
            else // follow player if in love
            {
                if (agent.enabled)
                {
                    if (target == null || target.health <= 0)
                    {
                        var targetTemp = gm.GetClosestUnit(transform.position, true, hc);

                        if (targetTemp == null ||
                            Vector3.Distance(transform.position, targetTemp.transform.position) > 50)
                        {   
                            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                                targetTemp = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
                            else
                                targetTemp = gm.player;
                        }
                        target = targetTemp;
                    }
                    
                    agent.SetDestination(target.transform.position);
                    //agent.SetDestination(gm.player.transform.position);   
                }
                
                yield return new WaitForSeconds(1);
            }
            
            if (mobParts && (mobParts.mobType == MobPartsController.Mob.Walker || mobParts.mobType == MobPartsController.Mob.Castle))
            {
                if ((monsterState == State.Chase && target && Vector3.Distance(transform.position, target.transform.position) > agent.stoppingDistance) || agent.velocity.magnitude > 0f)
                {
                    if (authorathive && (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString))
                        mobParts.anim.SetBool(chaseString, true);   
                    
                    if (!mobAu.chaseAu.isPlaying)
                    {
                        mobAu.ChaseAmbient();
                    }
                }
                else
                {
                    if (!mobAu.idleAu.isPlaying)
                    {
                        mobAu.IdleAmbient();
                    }

                    if (!authorathive) yield break;

                    if (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString)
                    {
                        if (Vector3.Distance(agent.destination, transform.position) > agent.stoppingDistance)
                        {
                            mobParts.anim.SetBool(chaseString, true);
                            if (mobParts.ikMonsterAnimator)
                                mobParts.ikMonsterAnimator.SetAnimate(true);
                        }
                        else
                        {
                            mobParts.anim.SetBool(chaseString, false);
                            if (mobParts.ikMonsterAnimator)
                                mobParts.ikMonsterAnimator.SetAnimate(false);
                        }   
                    }
                }   
            }
        }
    }

    IEnumerator FollowPlayer()
    {
        if (hc.inLove)
        {
            target  = gm.GetClosestUnit(transform.position, false, hc);
        }
        else
        {
            var newTarget = gm.player;
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                newTarget = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
                            
            target = newTarget;
        }

        monsterState = State.Chase;
        mobAu.ChaseAmbient();
        mobAu.Damage();

        if (authorathive)
        {
            agent.Warp(transform.position);
            if (agent.enabled)
                agent.isStopped = false;   
        }
        
        if (projectileShooter)
            projectileShooter.ToggleDangerous(true);

        while (hc.health > 0 && monsterState == State.Chase)
        {
            if (authorathive)
            {
                if (target && agent.enabled)
                {
                    targetSpeed = levels[GetLevel()].chaseSpeed * coldModifier * limbsModifier;
                    lastKnownPlayerPosition = target.transform.position;
                    //lastKnownPlayerPosition.y = 0;
                    
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(lastKnownPlayerPosition, out hit, 30.0f, NavMesh.AllAreas))
                    {
                        lastKnownPlayerPosition = hit.position;
                    }   
                    
                    agent.Warp(transform.position);
                    path = new NavMeshPath();
                    agent.CalculatePath(lastKnownPlayerPosition, path);
                    agent.SetPath(path);
                }                        
                if (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString)
                    mobParts.anim.SetBool(chaseString, true);
                
                if (mobParts.ikMonsterAnimator)
                    mobParts.ikMonsterAnimator.SetAnimate(true);
                
                mobParts.anim.SetBool(peacefulString, false);   
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        if (projectileShooter)
            projectileShooter.ToggleDangerous(false);
        
        followPlayer = null;
    }

    IEnumerator CheckHide() // from player
    {
        if (hc.inLove)
        {
            target  = gm.GetClosestUnit(transform.position, false, hc);
        }
        else
        {
            var newTarget = gm.player;
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                newTarget = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
                            
            target = newTarget;
        }

        while (monsterState == State.Chase)
        {
            if (target.player && target.health < target.healthMax / 3)
                yield return new WaitForSeconds(levels[GetLevel()].chaseTime / 2);
            else
                yield return new WaitForSeconds(levels[GetLevel()].chaseTime);

            RaycastHit searchHit;
            if (target && Physics.Raycast(raycastOrigin.position , target.transform.position - transform.position, out searchHit, levels[GetLevel()].sightDistanceChase, searchLayerMask))
            {
                if (searchHit.collider.gameObject != target.gameObject)
                    Hide(false);
            }
            else
                Hide(false);
        }
        hideCoroutine = null;
    }

    public void Hide(bool hide) // hiding = !chasing
    {
        if (monsterState != State.Death)
        {
            gm = GameManager.instance;
            
            if (hideAfterTimeCoroutine != null)
                StopCoroutine(hideAfterTimeCoroutine);
            
            if (hide)
            {
                mobAu.IdleAmbient();
                
                if (authorathive)
                {
                    if (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString)
                        mobParts.anim.SetBool(chaseString, false);

                    //targetSpeed = levels[GetLevel()].idleSpeed;
                    targetSpeed = levels[GetLevel()].chaseSpeed;   
                }
            }
            else
            {
                mobAu.ChaseAmbient();

                if (authorathive)
                {
                    if (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString)
                        mobParts.anim.SetBool(chaseString, true);
                    
                    if (mobParts.ikMonsterAnimator)
                        mobParts.ikMonsterAnimator.SetAnimate(true);
                    targetSpeed = levels[GetLevel()].chaseSpeed;
                }

                hideAfterTimeCoroutine = StartCoroutine(HideAfterTime(3));
            }
            
            if ((!target || target.player == false) && hc.npcInteractor && !hc.damagedByPlayer)
                hc.peaceful = true;
                
            if (hc.peaceful)
            {
                if (hc.npcInteractor) hc.npcInteractor.gameObject.SetActive(true);
                if (authorathive)
                    mobParts.anim.SetBool(peacefulString, true);   
            }

            if (hc && (authorathive && agent.enabled || !authorathive)) // stop movement, choose new position and start movement
            {
                monsterState = State.Idle;
                
                if (!authorathive)
                    return;
                
                agent.isStopped = true;
                
                if (target == null || target.gameObject == null || !target.gameObject.activeInHierarchy)
                {
                    if (!hc.inLove)
                        target = gm.GetClosestUnit(transform.position, false, hc);
                    else
                        target = gm.player;
                }

                if (target != null)
                {
                    Vector3 newpos = transform.position;
                    if (GameManager.instance.hub == false)
                    {
                        List<Vector3> tempPositions = new List<Vector3>();
                        for (var index = 0; index < LevelGenerator.instance.levelTilesInGame.Count; index++)
                        {
                            TileController tile = LevelGenerator.instance.levelTilesInGame[index];
                            if (tile.tileStatusEffect != StatusEffects.StatusEffect.Null) continue;

                            float newDistance = Vector3.Distance(tile.transform.position, target.transform.position);

                            if (newDistance > levels[GetLevel()].hideDistanceFromPlayerMin &&
                                newDistance < levels[GetLevel()].hideDistanceFromPlayerMax)
                                tempPositions.Add(tile.transform.position);
                        }


                        if (tempPositions.Count > 0)
                            newpos = tempPositions[Random.Range(0, tempPositions.Count)];
                        else
                        {
                            newpos = target.transform.position;
                        }   
                    }
                    else
                    {
                        NavMeshHit hit;
                        var pm = PlayerMovement.instance; 
                        var playerPos = new Vector3(pm.transform.position.x, pm.transform.position.y, pm.transform.position.z) + Random.insideUnitSphere * 30;
                            
                        if (NavMesh.SamplePosition(playerPos, out hit, 30.0f, NavMesh.AllAreas))
                        {
                            newpos = hit.position;
                        }   
                    }

                    path = new NavMeshPath();    
                    agent.CalculatePath(newpos, path);
                    agent.SetPath(path);
                    agent.isStopped = false;   
                }
            }
        }
    }

    public void MoveCloser(Vector3 newPos, bool hide)
    {
        if (mobParts && mobParts.ikMonsterAnimator)
            mobParts.ikMonsterAnimator.SetAnimate(true);
        
        if (hide)
        {
            mobAu.IdleAmbient();
            if (!mobParts) mobParts = hc.mobPartsController;
            if (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString)
                mobParts.anim.SetBool(chaseString, false);
            //targetSpeed = levels[GetLevel()].idleSpeed * (gm.player.health / gm.player.healthMax); -- mobs are super slow in coop
            targetSpeed = levels[GetLevel()].idleSpeed;
        }
        else
        {
            mobAu.ChaseAmbient();
            if (!mobParts) mobParts = hc.mobPartsController;
            if (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString)
                mobParts.anim.SetBool(chaseString, true);
            //targetSpeed = levels[GetLevel()].idleSpeed * (gm.player.health / gm.player.healthMax); -- mobs are super slow in coop
            targetSpeed = levels[GetLevel()].chaseSpeed;
            
            if (hideAfterTimeCoroutine != null)
                StopCoroutine(hideAfterTimeCoroutine);
            
            hideAfterTimeCoroutine = StartCoroutine(HideAfterTime(3));
        }

        if (!hc.peaceful && !hc.npcInteractor)
        {
            if (!gm) gm = GameManager.instance;
            target = gm.player;   
        }
        
        monsterState = State.Idle;
        sightDistanceCurrent = levels[Mathf.Clamp(gm.level.mobsLevel, 0, levels.Count - 1)].sightDistance / 2;
        if (!agent) 
            agent = GetComponent<NavMeshAgent>();
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        
            path = new NavMeshPath();
            agent.CalculatePath(newPos, path);
            agent.SetPath(path);
            agent.isStopped = false;   
        }
    }

    IEnumerator HideAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        
        mobAu.IdleAmbient();
        if (mobParts.simpleWalker == false || mobParts.simpleWalkerString != chaseString)
            mobParts.anim.SetBool(chaseString, false);
        
        if (mobParts.ikMonsterAnimator)
            mobParts.ikMonsterAnimator.SetAnimate(false);
        
        targetSpeed = levels[GetLevel()].idleSpeed;
        hideAfterTimeCoroutine = null;
    }
    
    public void Death()
    {
        if (hideAfterTimeCoroutine != null)
            StopCoroutine(hideAfterTimeCoroutine);
        
        monsterState = State.Death;
        if (obstacle)
            obstacle.enabled = false;
    }
}