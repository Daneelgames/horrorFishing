using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPropBonesRandomizer1 : MonoBehaviour
{
    [Range(0, 1)] public float changeToAnimate = 0; 
    public List<Transform> boneList = new List<Transform>();
    public List<Vector3> bonesEulearAnglesListTpose = new List<Vector3>();
    public List<Vector3> bonesLocalPositionsListTpose = new List<Vector3>();
    
    [Header("Generated Pose")]
    List<Transform> bonesToAnimate = new List<Transform>();
    public List<Vector3> bonesEulearAnglesListTemp = new List<Vector3>();
    public Vector2 animateBonesAnglesMinMax = Vector2.zero;
    
    public List<Transform> groundContactBones = new List<Transform>();
    public List<Vector3> groundContactBonesPositions = new List<Vector3>();
    
    public List<Transform> removedGroundContactBones = new List<Transform>();
    public List<Transform> armsBonesTargets = new List<Transform>();
    public Transform headBoneTarget;
    public Transform hipsBone;

    void Start()
    {
        RandomizeBones();
        return;
        
        if (bonesToAnimate.Count > 0 && Random.value < changeToAnimate)
            StartCoroutine(AnimateBones());
    }

    IEnumerator AnimateBones()
    {
        while (true)
        {
            for (int i = 0; i < bonesToAnimate.Count; i++)
            {
                bonesToAnimate[i].transform.eulerAngles = bonesEulearAnglesListTemp[i] + new Vector3(
                    Random.Range(animateBonesAnglesMinMax.x,
                        animateBonesAnglesMinMax.y),
                    Random.Range(animateBonesAnglesMinMax.x,
                        animateBonesAnglesMinMax.y),
                    Random.Range(animateBonesAnglesMinMax.x,
                        animateBonesAnglesMinMax.y));

                if (groundContactBones.Count > groundContactBonesPositions.Count)
                    yield break;
                
                for (int j = 0; j < groundContactBones.Count; j++)
                {
                    groundContactBones[j].transform.position = groundContactBonesPositions[j];
                }
                yield return null;   
            }
        }
    }

    [ContextMenu("SaveAngles")]
    void SaveAngles()
    {
        bonesEulearAnglesListTpose.Clear();
        bonesLocalPositionsListTpose.Clear();
        
        for (int i = 0; i < boneList.Count; i++)
        {
            bonesEulearAnglesListTpose.Add(boneList[i].transform.eulerAngles);
            bonesLocalPositionsListTpose.Add(boneList[i].transform.localPosition);
        }
    }
    
    [ContextMenu("RandomizeAngles")]
    void RandomizeBones()
    {
        bonesEulearAnglesListTemp.Clear();
        groundContactBonesPositions.Clear();
        
        for (int i = 0; i < boneList.Count; i++)
        {
            boneList[i].transform.eulerAngles = bonesEulearAnglesListTpose[i] + new Vector3(Random.Range(-40, 40),Random.Range(-40, 40),Random.Range(-40, 40));
            if (Random.value > 0.5f)
            {
                bonesToAnimate.Add(boneList[i]);   
                bonesEulearAnglesListTemp.Add(boneList[i].transform.eulerAngles);
            }
        }

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
        
        if (Random.value > 0.75f)
            groundContactBones.Add(hipsBone);
        
        if (Random.value > 0.75f)
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
            var hits = Physics.RaycastAll(groundContactBones[i].position + Vector3.up, Vector3.down, 500);
            for (var j = 0; j < hits.Length; j++)
            {
                var hit = hits[j];

                if (hit.collider.gameObject.layer != 23 && hit.collider.gameObject.layer != 28 &&
                    hit.collider.gameObject.layer != 29 && hit.collider.gameObject.layer != 30 &&
                    hit.collider.gameObject.layer != 25) // floor
                    continue;

                
                groundContactBones[i].transform.position = hit.point;
                groundContactBonesPositions.Add(hit.point);
            }
        }

        for (int i = 0; i < removedGroundContactBones.Count; i++)
        {
            removedGroundContactBones[i].transform.position += Random.insideUnitSphere * Random.Range(0, 10);
        }
    }
    
    [ContextMenu("RestoreAngles")]
    void RestoreBones()
    {
        bonesEulearAnglesListTemp.Clear();

        for (int i = 0; i < boneList.Count; i++)
        {
            boneList[i].transform.eulerAngles = bonesEulearAnglesListTpose[i];
            boneList[i].transform.localPosition = bonesLocalPositionsListTpose[i];
        }
    }
}
