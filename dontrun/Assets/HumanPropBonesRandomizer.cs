using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;

public class HumanPropBonesRandomizer : MonoBehaviour
{
    public List<Transform> groundContactBones = new List<Transform>();

    public List<Transform> removedGroundContactBones = new List<Transform>();
    public List<Transform> armsBonesTargets = new List<Transform>();
    public Transform headBoneTarget;
    public Transform hipsBone;

    [Header("Animation timings")] 
    
    public float stepDelay = 1;
    public float stepOffsetScale = 0.5f;
    public float hipsMoveHeight = 1;
    
    List<Coroutine> boneMoveCoroutines = new List<Coroutine>();
    
    bool sideOffsetRight = true;
    Vector3 sideOffset = Vector3.right;
    float currentStepDelay = 0;

    public bool animate = false;
    void Start()
    {
        stepDelay += Random.Range(-stepDelay, stepDelay) / 2;
        stepOffsetScale += Random.Range(-stepOffsetScale, stepOffsetScale) / 2;
        hipsMoveHeight += Random.Range(-hipsMoveHeight, hipsMoveHeight) / 2;
        
        InitBones();
        if (!animate)
        {
            RandomizePose();
            return;   
        }
        
        StartCoroutine(AnimateBody());
        StartCoroutine(AnimateGroundContact());
    }

    public void RandomizePose()
    {
        for (int i = 0; i < groundContactBones.Count; i++)
        {
            var hits = Physics.RaycastAll(transform.position + Random.insideUnitSphere * Random.Range(1, 3) + Vector3.up * 100f, Vector3.down, 500);
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
    }
    
    IEnumerator AnimateGroundContact()
    {
        while (true)
        {
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

                    yield return StartCoroutine(MoveGroundContactToPos(groundContactBones[i], groundContactBones[i].transform.position,
                        hit.point));
                    break;
                }
                yield return new WaitForSeconds(stepDelay / groundContactBones.Count);
            }
            // move removed bones more realistically
        }
    }
    
    IEnumerator AnimateBody()
    {
        while (true)
        {
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

    private Vector3 tempNewStartForBone;
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
            /*
            boneMoveCoroutines.Add(StartCoroutine(MoveBoneToPos(removedGroundContactBones[i], removedGroundContactBones[i].transform.localPosition,
                removedGroundContactBones[i].transform.localPosition + upDownDirection * hipsMoveHeight + sideOffset)));
                */
            
            tempNewStartForBone =
                transform.position + new Vector3(Random.Range(-hipsMoveHeight,hipsMoveHeight), Random.Range(hipsMoveHeight / 2,hipsMoveHeight * 2), Random.Range(-hipsMoveHeight,hipsMoveHeight));
            
            boneMoveCoroutines.Add(StartCoroutine(MoveBoneToPos(removedGroundContactBones[i], removedGroundContactBones[i].transform.position,
                tempNewStartForBone + upDownDirection * hipsMoveHeight + sideOffset)));
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
        
        while (t < currentStepDelay)
        {
            t += Time.deltaTime;
            bone.transform.position = Vector3.Lerp(startPos, newPos, t / currentStepDelay * tScaler);
            yield return null;
        }

        Vector3 staticPos = bone.transform.position;
        while (true)
        {
            bone.transform.position = staticPos + new Vector3(Random.Range(-0.1f,0.1f),Random.Range(-0.1f,0.1f),Random.Range(-0.1f,0.1f));
            yield return null;
        }
    }
    
    IEnumerator MoveGroundContactToPos(Transform bone, Vector3 startPos, Vector3 newPos)
    {
        float t = 0;
        float smallerStepDelay = stepDelay / groundContactBones.Count / 2;
        Vector3 stepUpPosition = newPos + Vector3.up * hipsMoveHeight * Random.Range(1f, 3f);;
        
        while (t < smallerStepDelay)
        {
            t += Time.deltaTime;
            bone.transform.position = Vector3.Lerp(startPos, stepUpPosition , t / smallerStepDelay);
            yield return null;
        }

        t = 0;
        startPos = bone.transform.position;
        
        while (t < smallerStepDelay)
        {
            t += Time.deltaTime;
            bone.transform.position = Vector3.Lerp(startPos, newPos, t / smallerStepDelay);
            yield return null;
        }
    }
    
    [ContextMenu("RandomizeAngles")]
    void InitBones()
    {
        if (Random.value > 0.5f)
        {
            int removeContactsAmount = Random.Range(1, groundContactBones.Count - 1);
            for (int i = removeContactsAmount; i >= 0; i--)
            {
                if (groundContactBones.Count <= 1)
                    break;
                
                int removedContactIndex = Random.Range(0, groundContactBones.Count);
                removedGroundContactBones.Add(groundContactBones[removedContactIndex]);
                groundContactBones.RemoveAt(removedContactIndex);
            }
        }
        
        removedGroundContactBones.Add(hipsBone);
        
        if (Random.value > 0.9f)
            groundContactBones.Add(headBoneTarget);
        else
            removedGroundContactBones.Add(headBoneTarget);
            
        for (int i = 0; i < armsBonesTargets.Count; i++)
        {
            if (Random.value > 0.66f)
                groundContactBones.Add(armsBonesTargets[i]);
            else
                removedGroundContactBones.Add(armsBonesTargets[i]);
        }
        
        for (int i = 0; i < groundContactBones.Count; i++)
        {
            groundContactBones[i].transform.parent = null;
        }
    }
}
