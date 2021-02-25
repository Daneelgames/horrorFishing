using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.PlayerLoop;
using UnityEngine.SubsystemsImplementation;
using Random = UnityEngine.Random;

public class IkMonsterAnimator : MonoBehaviour
{
    public HealthController hc;
    public List<Transform> groundContactBones = new List<Transform>();

    public List<Transform> removedGroundContactBones = new List<Transform>();
    public List<Transform> armsBonesTargets = new List<Transform>();
    public Transform headBoneTarget;
    public Transform hipsBone;

    public float headRemovedBoneChance = 0.9f;
    public float armsRemovedBonesChance = 0.66f;
    
    public bool limbsAffectsSpeed = true;
    [Header("Animation timings")] 
    public bool randomize = true;
    public float stepDelay = 1;
    public float stepOffsetScale = 0.5f;
    public float hipsMoveHeight = 1;
    public Vector2 groundPointStepUpPositionMinMax = new Vector2(1, 5);
    
    List<Coroutine> boneMoveCoroutines = new List<Coroutine>();
    
    bool sideOffsetRight = true;
    Vector3 sideOffset = Vector3.right;
    float currentStepDelay = 0;

    public bool animate = true;
    
    public List<CustomIKSolver> ikSolvers = new List<CustomIKSolver>();

    public float distanceFromBoneToPlayerToBeAbleToAttack = 5;

    public bool initInStart = true;
    private bool updateSolvers = false;

    public AudioSource walkingAmbient;
    public LerpMoveToPlayer lerpMovetoPlayer;

    public AssetReference stepVfxReference;
    void Start()
    {
        if (initInStart)
        {
            Init();
        }
    }

    public void Init()
    {
        if (walkingAmbient) walkingAmbient.Play();
        updateSolvers = true;
        if (randomize)
        {
            stepDelay += Random.Range(-stepDelay, stepDelay) / 2;
            stepOffsetScale += Random.Range(-stepOffsetScale, stepOffsetScale) / 2;
            hipsMoveHeight += Random.Range(-hipsMoveHeight, hipsMoveHeight) / 2;
        }

        InitBones();
        if (!animate)
        {
            RandomizePose();
            return;   
        }
        
        StartCoroutine(AnimateBody());
        StartCoroutine(AnimateGroundContact());
    }

    private void LateUpdate()
    {
        if (updateSolvers)
        {
            for (int i = 0; i < ikSolvers.Count; i++)
            {
                ikSolvers[i].UpdateSolver();
            }
        }
    }

    private float limbsSpeedModifier = 1;
    public float GetLimbsSpeedModifier()
    {
        return limbsSpeedModifier;
    }

    public void SetAnimate(bool a)
    {
        animate = a;
        
        for (var index = 0; index < ikSolvers.Count; index++)
        {
            ikSolvers[index].canAnimate = animate;
        }

        if (animate)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateBody());
            StartCoroutine(AnimateGroundContact());
        }
    }

    public void RemoveIkTarget(List<CustomIKJoint> removedIkTargets)
    {
        for (int i = 0; i < removedIkTargets.Count; i++)
        {
            if (groundContactBones.Contains(removedIkTargets[i].transform))
            {
                groundContactBones.Remove(removedIkTargets[i].transform);
            }
            
            if (removedGroundContactBones.Contains(removedIkTargets[i].transform))
            {
                removedGroundContactBones.Remove(removedIkTargets[i].transform);
            }

            if (armsBonesTargets.Contains(removedIkTargets[i].transform))
                armsBonesTargets.Remove(removedIkTargets[i].transform);

            if (removedIkTargets[i].transform == headBoneTarget)
                headBoneTarget = null;
        }

        if (groundContactBones.Count == 0)
        {
            if (removedGroundContactBones.Count > 0)
            {
                groundContactBones.Add(removedGroundContactBones[Random.Range(0, removedGroundContactBones.Count)]);
            }
            else if (headBoneTarget)
                groundContactBones.Add(headBoneTarget);
            else
                groundContactBones.Add(hipsBone);
        }

        InitBones();
    }

    public void RestoreIkTarget(List<CustomIKJoint> restoredIkTargets)
    {
        for (int i = 0; i < restoredIkTargets.Count; i++)
        {
            if (!groundContactBones.Contains(restoredIkTargets[i].transform))
            {
                groundContactBones.Add(restoredIkTargets[i].transform);
            }
        }
        
        InitBones();
    }
    
    public void RandomizePose()
    {
        for (int i = 0; i < groundContactBones.Count; i++)
        {
            var hits = Physics.RaycastAll(transform.position + Random.insideUnitSphere * Random.Range(1, 3) + Vector3.up * 100f, Vector3.down, 10000);
            if (hits == null || hits.Length <= 0)
            {
                continue;   
            }
                
            for (var j =  hits.Length - 1; j >= 0; j--)
            {
                var hit = hits[j];

                if (hit.collider.gameObject.layer != 23 && hit.collider.gameObject.layer != 28 &&
                    hit.collider.gameObject.layer != 29 && hit.collider.gameObject.layer != 30 &&
                    hit.collider.gameObject.layer != 25 && hit.collider.gameObject.layer != 20) // floor
                    continue;

                StartCoroutine(MoveGroundContactToPos(groundContactBones[i], groundContactBones[i].transform.position,
                    hit.point));
                break;
            }
        }
        

        for (int i = 0; i < removedGroundContactBones.Count; i++)
        {
            Vector3 newOffset = Random.insideUnitSphere * Random.Range(0, hipsMoveHeight);
            newOffset.y = Mathf.Clamp(newOffset.y, 0, 100);
            removedGroundContactBones[i].transform.position += newOffset;
        }
        SetAnimate(false);
    }

    IEnumerator AnimateGroundContact()
    {
        while (true)
        {
            while (animate == false)
            {
                yield return null;
            }
            
            for (int i = 0; i < groundContactBones.Count; i++)
            {
                var hits = Physics.RaycastAll(transform.position + Random.insideUnitSphere * Random.Range(1, 3) + Vector3.up * 100f, Vector3.down, 500);
                if (hits == null || hits.Length <= 0)
                {
                    yield return new WaitForSeconds(stepDelay / groundContactBones.Count);
                    continue;   
                }
                
                //for (var j =  hits.Length - 1; j >= 0; j--)
                for (var j =  0; j < hits.Length; j++)
                {
                    var hit = hits[j];

                    if (hit.collider.gameObject.layer != 23 && hit.collider.gameObject.layer != 28 &&
                        hit.collider.gameObject.layer != 29 && hit.collider.gameObject.layer != 30 &&
                        hit.collider.gameObject.layer != 25 && hit.collider.gameObject.layer != 20) // floor
                        continue;

                    yield return StartCoroutine(MoveGroundContactToPos(groundContactBones[i], groundContactBones[i].transform.position, hit.point));
                    break;
                }
                yield return new WaitForSeconds(stepDelay / groundContactBones.Count);
            }

            if (lerpMovetoPlayer) lerpMovetoPlayer.ResetTime();
        }
    }
    
    IEnumerator AnimateBody()
    {
        while (true)
        {
            while (animate == false)
            {
                
                yield return null;
            }
            currentStepDelay = stepDelay + Random.Range(-stepDelay, stepDelay) / 2;
            MoveBody(Vector3.up);
            
            yield return new WaitForSeconds(currentStepDelay);

            currentStepDelay = stepDelay + Random.Range(-stepDelay, stepDelay) / 2;
            sideOffsetRight = !sideOffsetRight;
            MoveBody(Vector3.down);
            
            yield return new WaitForSeconds(currentStepDelay);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private Vector3 tempNewEndForBone;
    void MoveBody(Vector3 upDownDirection)
    {
        for (var index = 0; index < boneMoveCoroutines.Count; index++)
        {
            StopCoroutine( boneMoveCoroutines[index]);
        }
        boneMoveCoroutines.Clear();
            
        sideOffset = GetSideOffset();

        
        for (int i = 0; i < removedGroundContactBones.Count; i++)
        {
            if (!hc.inLove && Vector3.Distance(PlayerMovement.instance.transform.position,removedGroundContactBones[i].transform.position) <= distanceFromBoneToPlayerToBeAbleToAttack)
            {
                // ATTACK
                tempNewEndForBone = PlayerMovement.instance.transform.position + new Vector3(Random.Range(-hipsMoveHeight,hipsMoveHeight), Random.Range(hipsMoveHeight / 5,hipsMoveHeight * 5), Random.Range(-hipsMoveHeight,hipsMoveHeight));   
            }
            else
                tempNewEndForBone = transform.position + new Vector3(Random.Range(-hipsMoveHeight,hipsMoveHeight), Random.Range(hipsMoveHeight / 5,hipsMoveHeight * 5), Random.Range(-hipsMoveHeight,hipsMoveHeight));
            
            boneMoveCoroutines.Add(StartCoroutine(MoveBoneToPos(removedGroundContactBones[i], removedGroundContactBones[i].transform.position,
                tempNewEndForBone + upDownDirection * hipsMoveHeight + sideOffset)));
        }
    }
    
    Vector3 GetSideOffset()
    {
        if (sideOffsetRight)
            return transform.right * stepOffsetScale;

        return -transform.right * stepOffsetScale;
    }
    
    IEnumerator MoveBoneToPos(Transform bone, Vector3 startPos, Vector3 newPos)
    {
        float t = 0;
        float tScaler = Random.Range(0.5f, 3);
        newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, transform.position.y, transform.position.y + Random.Range(groundPointStepUpPositionMinMax.x, groundPointStepUpPositionMinMax.y)), newPos.z);
        
        while (t < currentStepDelay)
        {
            t += Time.deltaTime;
            bone.transform.position = Vector3.Lerp(startPos, newPos, t / currentStepDelay * tScaler);
            yield return null;
        }

        Vector3 staticPos = bone.transform.position;
        while (animate)
        {
            bone.transform.position = staticPos + new Vector3(Random.Range(-0.1f,0.1f),Random.Range(-0.1f,0.1f),Random.Range(-0.1f,0.1f));
            yield return null;
        }
    }
    
    IEnumerator MoveGroundContactToPos(Transform bone, Vector3 startPos, Vector3 newPos)
    {
        float t = 0;
        float smallerStepDelay = stepDelay / groundContactBones.Count * Random.Range(0.5f, 2f);
        Vector3 stepUpPosition = newPos + Vector3.up * Random.Range(groundPointStepUpPositionMinMax.x, groundPointStepUpPositionMinMax.y);
        Vector3 stepUpPositionCurrent = stepUpPosition;
        
        while (t < smallerStepDelay)
        {
            t += Time.deltaTime;
            bone.transform.position = Vector3.Lerp(startPos, stepUpPositionCurrent , t / smallerStepDelay);
            stepUpPositionCurrent = Vector3.Lerp(stepUpPosition, newPos , t / smallerStepDelay);
            yield return null;
        }
        hc.mobAudio.Step();
           
        /*
        if (stepVfxReference == null)
            yield break; 
             
        for (int i = 0; i < ikSolvers.Count; i++)
        {
            if (ikSolvers[i].Target.transform == bone)
            {
                AssetSpawner.instance.Spawn(stepVfxReference, ikSolvers[i].Ankle.transform.position, AssetSpawner.ObjectType.Vfx);
            }
        }*/
    }
    
    [ContextMenu("RandomizeAngles")]
    void InitBones()
    {
        if (hipsBone && !removedGroundContactBones.Contains(hipsBone))
            removedGroundContactBones.Add(hipsBone);

        if (headBoneTarget)
        {
            if (Random.value > headRemovedBoneChance && !groundContactBones.Contains(headBoneTarget))
                groundContactBones.Add(headBoneTarget);
            else if (!removedGroundContactBones.Contains(headBoneTarget))
                removedGroundContactBones.Add(headBoneTarget);   
        }
            
        for (int i = 0; i < armsBonesTargets.Count; i++)
        {
            if (Random.value > armsRemovedBonesChance && !groundContactBones.Contains(armsBonesTargets[i]))
                groundContactBones.Add(armsBonesTargets[i]);
            else if (!removedGroundContactBones.Contains(armsBonesTargets[i]))
                removedGroundContactBones.Add(armsBonesTargets[i]);
        }
        
        for (int i = 0; i < groundContactBones.Count; i++)
        {
            groundContactBones[i].transform.parent = null;
            if (removedGroundContactBones.Contains(groundContactBones[i]))
                removedGroundContactBones.Remove(groundContactBones[i]);
        }

        if (limbsAffectsSpeed)
            limbsSpeedModifier = Mathf.Clamp(groundContactBones.Count * 0.25f, 0.1f, 1f);
    }

    void OnDestroy()
    {
        for (int i = groundContactBones.Count - 1; i >= 0; i--)
        {
            if (groundContactBones[i])
                Destroy(groundContactBones[i].gameObject);
        }
        
        for (int i = armsBonesTargets.Count - 1; i >= 0; i--)
        {
            Destroy(armsBonesTargets[i]);
        }
    }
}
