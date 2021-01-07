using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;

public class FaceEaterBehaviour : MonoBehaviour
{
    enum State
    { Moving, OnFace }
    private State mobState = State.Moving;

    public HealthController hc;
    public NavMeshAgent agent;
    public Animator anim;

    public float rangeToMove = 50;
    public float rangeToAggro = 20;
    public float damage = 200;
    public float faceJumpDistance = 10;
    public int maxAttacksOnFace = 3;
    int curAttacksOnFace = 0;
    private string attackingString = "Attacking";

    private PlayerMovement pm;
    private GameManager gm;
    private Coroutine jumpCoroutine;

    private bool authorative = true;
    
    void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;

        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive &&
            GLNetworkWrapper.instance.localPlayer.isServer == false)
        {
            authorative = false;
            agent.enabled = false;
            return;
        }
        
        StartCoroutine(UpdateCycle());
    }

    IEnumerator UpdateCycle()
    {
        while (hc.health > 0)
        {
            // move around player for some time
            // than try to jump on players face if player wasnt jumped on for like 30 seconds or more
            
            yield return new WaitForSeconds(1);

            if (mobState == State.OnFace)
            {
                if (curAttacksOnFace >= maxAttacksOnFace)
                {
                    JumpOffFace();
                }
                else
                {
                    pm.hc.Damage(damage,  transform.position, transform.position,
                        null, hc.playerDamagedMessage[gm.language], false, hc.names[gm.language], hc, null, true);
                    curAttacksOnFace++;
                }
            }
            else if (mobState == State.Moving)
            {
                var dist = Vector3.Distance(transform.position, pm.transform.position);
                
                if (pm.hc.faceHuggedCooldown <= 0)
                {
                    if (dist <= faceJumpDistance)
                    {
                        // can facehug the player
                        JumpOnFace();   
                    }   
                    else if (dist <= rangeToAggro)
                    {
                        agent.SetDestination(pm.transform.position);                        
                    }
                }
                else if (dist < rangeToMove)
                {
                    agent.SetDestination(gm.lg.levelTilesInGame[Random.Range(0, gm.lg.levelTilesInGame.Count)].transform.position);
                }
            }
        }
    }

    public void Damaged()
    {
        if (mobState == State.OnFace)
            JumpOffFace();
        else if (authorative)
        {
            if (jumpCoroutine != null)
                StopCoroutine(jumpCoroutine);
            agent.SetDestination(gm.lg.levelTilesInGame[Random.Range(0, gm.lg.levelTilesInGame.Count)].transform.position);
        }
            
    }
    
    void JumpOnFace()
    {
        hc.mobAudio.Attack();
        agent.isStopped = true;
        agent.enabled = false;
        ChangeLayersRecursively(transform, 9);
        transform.parent = pm.portableTransform;
        anim.SetBool(attackingString, true);
        
        jumpCoroutine = StartCoroutine(MoveToPlayerFace());    
    }

    void JumpOffFace()
    {
        mobState = State.Moving;
        transform.parent = null;
        anim.SetBool(attackingString, false);
        ChangeLayersRecursively(transform, 11);
        pm.hc.ResetFaceHugCooldown(null);
        
        if (!authorative) return;
        
        agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(gm.lg.levelTilesInGame[Random.Range(0, gm.lg.levelTilesInGame.Count)].transform.position);
    }
    
    void ChangeLayersRecursively(Transform trans, int layer)
    {
        foreach (Transform child in trans)
        {
            child.gameObject.layer = layer;
            ChangeLayersRecursively(child, layer);
        }
    }

    IEnumerator MoveToPlayerFace()
    {
        float t = 0;

        while (t < 1)
        {
            yield return null;
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, t);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, t);
            t += Time.deltaTime;
        }
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        mobState = State.OnFace;
        pm.hc.ResetFaceHugCooldown(this);
        curAttacksOnFace = 0;
    }
}
