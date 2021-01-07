using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.TextCore;
using Random = UnityEngine.Random;

[Serializable]
public class MobWallsJumperMovementStats
{
    public float idleJumpTime = 10;
    public float attackJumpTime = 3;
    public float damagedJumpTime = 1;
    public float jumpDistance = 30;
    public float jumpSpeed = 5;
    public float reactionDistance = 30;
    public float reactionDistanceBattle = 100;
}
public class MobWallsJumperMovement : MonoBehaviour
{
    enum State {Idle, Attack, Damaged}
    State mobState = State.Idle;

    public bool activeOnStart = true;

    public List<MobWallsJumperMovementStats> levels;
    [Header("Search for units")]
    public LayerMask searchLayerMask;

    public HealthController hc;
    [Header("Search for place to jump")]
    public LayerMask layerMask;

    float currentTime;
    
    public AudioSource jumperAttackSource;

    public Animator mobVisual;
    public MobPartsController mobParts;
    PlayerMovement pm;
    GameManager gm;
    RaycastHit searchHit;
    Coroutine moveToAttack;
    RaycastHit jumpHit;
    Collider currentCollider;
    Coroutine jumpCoroutine;
    LevelGenerator lg;

    public float coldModifier = 1;

    public HealthController target;

    public List<TileController> tempTiles;
    private float reactionDistance = 30;
    Coroutine moveToPlace;

    public List<SmoothFollowTarget> separatedMobParts;
    private bool awaken = false;

    private bool authorathive = true;

    private void Start()
    {
        authorathive = !(GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive &&
                         LevelGenerator.instance && LevelGenerator.instance.levelgenOnHost == false);

        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        lg = LevelGenerator.instance;

        if (activeOnStart)
        {
            transform.position += Vector3.up * 2;
            Init();   
        }
    }

    public void Init()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        lg = LevelGenerator.instance;
        reactionDistance = levels[GetLevel()].reactionDistance;
        
        target = gm.player;
        StartCoroutine(SearchPlayer());
        StartCoroutine(SearchNewPlace()); 
    }
    
    int GetLevel()
    {
        return Mathf.Clamp(mobParts.level, 0, levels.Count - 1);
    }


    private void Update()
    {
        if (target && target == pm.hc)
            mobVisual.transform.LookAt(pm.cameraAnimator.transform.position);

        if (coldModifier < 1)
            coldModifier += Time.deltaTime / 5;
    }

    IEnumerator SearchPlayer()
    {
        while (hc.health > 0)
        {
            if (hc.inLove)
            {
                target  = gm.GetClosestUnit(transform.position, false);
            }
            else
            {
                target = pm.hc;
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    target = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
            }
            
            if (Vector3.Distance(transform.position, target.transform.position) <= reactionDistance)
            {
                if (!awaken && separatedMobParts.Count > 0)
                {
                    for (var index = 0; index < separatedMobParts.Count; index++)
                    {
                        var smp = separatedMobParts[index];
                        smp.enabled = true;
                    }

                    awaken = true;
                }
                
                if (target == null)
                    target = pm.hc;
            
                if (mobState == State.Idle && !hc.peaceful)
                {
                    if (Physics.Raycast(transform.position + Vector3.up * 2, (target.transform.position + Vector3.up)- transform.position, out searchHit, 100, searchLayerMask))
                    {
                        if (searchHit.collider.gameObject.layer == 9 || searchHit.collider.gameObject.layer == 11) // if hit layer HANDS or unit
                        {
                            currentTime = 0;
                            mobState = State.Attack;
                            reactionDistance = levels[GetLevel()].reactionDistanceBattle;
                        }
                    }
                }   
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator SearchNewPlace()
    {
        while (true)
        {
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                target = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
            
            if (Vector3.Distance(transform.position, target.transform.position) <= reactionDistance)
            {
                switch (mobState)
                {
                    case State.Idle:
                        if (currentTime < levels[GetLevel()].idleJumpTime)
                        {
                            currentTime += 1;
                        }
                        else
                        {
                            StartCoroutine(JumpOnWall());
                            currentTime = 0;
                        }
                        break;

                    case State.Attack:
                        if (currentTime < levels[GetLevel()].attackJumpTime)
                        {
                            if (target.health > target.healthMax / 4)
                                currentTime += 1;
                            else
                                currentTime += 0.5f;
                        }
                        else
                        {
                            AttackPlayer();
                            currentTime = 0;
                        }
                        break;

                    case State.Damaged:
                        if (currentTime < levels[GetLevel()].damagedJumpTime)
                        {
                            currentTime += 1;
                        }
                        else
                        {
                            StartCoroutine(JumpOnWall());
                            mobState = State.Idle;
                            currentTime = 0;
                        }
                        break;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator JumpOnWall()
    {
        // choose tile
        lg = LevelGenerator.instance;
        tempTiles.Clear();
        tempTiles = new List<TileController>(lg.levelTilesInGame);
        for (int i = tempTiles.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(transform.position, tempTiles[i].transform.position) > levels[GetLevel()].jumpDistance /* || tempTiles[i].propOnTile != null*/)
            {
                tempTiles.RemoveAt(i);
            }
        }
        TileController targetTile = null;

        while (targetTile == null)
        {
            if (tempTiles.Count <= 1)
                break;
            int r = Random.Range(0, tempTiles.Count);

            if (tempTiles[r] != null && Physics.Raycast(transform.position, tempTiles[r].transform.position - transform.position, out jumpHit, 100, layerMask))
            {
                if (jumpHit.collider.gameObject.layer == 20)
                {
                    targetTile = tempTiles[r];
                    break;
                }
            }
            if (targetTile == null)
            {
                tempTiles.RemoveAt(r);
            }

            yield return null;
        }
        if (moveToPlace != null)
            StopCoroutine(moveToPlace);

        moveToPlace = StartCoroutine(MoveToPlace(new Vector3(jumpHit.point.x, Random.Range(2f, 7f), jumpHit.point.z)));
    }

    public void AttackPlayer() // any target
    {
        if (target && Physics.Raycast(transform.position, (target.transform.position + Vector3.up) - transform.position, out jumpHit, 200, layerMask))
        {
            if (jumpHit.collider.gameObject && jumpHit.collider != currentCollider && (jumpHit.collider.gameObject.layer == 16 || jumpHit.collider.gameObject.layer == 18 ||jumpHit.collider.gameObject.layer == 20))
            {
                currentCollider = jumpHit.collider;
                if (moveToPlace != null)
                    StopCoroutine(moveToPlace);
                moveToPlace = StartCoroutine(MoveToPlace(new Vector3(jumpHit.point.x, Random.Range(1, 3), jumpHit.point.z) + jumpHit.normal * 2));
            }
        }
    }

    IEnumerator MoveToPlace(Vector3 newPos)
    {
        yield return null;
        
        jumperAttackSource.Play();
        
        if (!authorathive)
            yield break;
        
        mobVisual.SetBool("Jump", true);
        
        while (Vector3.Distance(transform.position, newPos) > 0.2f)
        {
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                target = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
            
            if (target.health > target.healthMax * 0.33f && Vector3.Distance(transform.position, target.transform.position + Vector3.up) < 3f)
            {
                // 100% deal damage
                transform.position = target.transform.position + Vector3.up;
                
                mobVisual.SetBool("Jump", false);
                jumperAttackSource.Stop();
                mobState = State.Idle;
                
                if (moveToPlace != null)
                    StopCoroutine(moveToPlace);
            }
            else
                transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * levels[GetLevel()].jumpSpeed * coldModifier);
            
            yield return new WaitForEndOfFrame();
        }

        transform.position = newPos;
        mobVisual.SetBool("Jump", false);
        jumperAttackSource.Stop();
        mobState = State.Idle;
    }
    
    public void Damaged(HealthController damager)
    {
        pm = PlayerMovement.instance;

        if (!hc.inLove)
        {
            if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
                target = pm.hc;
            else
            {
                target = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
            }
        }
        
        if (gameObject.activeInHierarchy)
            StartCoroutine(JumpOnWall());
        
        if (mobState != State.Damaged)
        {
            currentTime = 0;
            mobState = State.Damaged;
        }
    }

    public void Death()
    {
        /*
        if (separatedMobParts.Count > 0)
        {
            for (int i = separatedMobParts.Count - 1; i >= 0; i--)
            {
                separatedMobParts[i].gameObject.SetActive(false);
            }
        }*/
        gameObject.SetActive(false);
    }

    public void JumpOnTeleport()
    {
        if (authorathive)
            mobVisual.SetBool("Jump", true);
    }
}