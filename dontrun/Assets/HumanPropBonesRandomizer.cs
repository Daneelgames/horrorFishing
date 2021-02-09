using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class HumanPropBonesRandomizer : MonoBehaviour
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


        public List<CustomIKSolver> IkSolvers;
        
        void Awake()
        {
            //transform.localScale = Vector3.zero;
        }
        void Start()
        {
            RandomizeBones();
        
            if (bonesToAnimate.Count > 0 && Random.value < changeToAnimate)
                StartCoroutine(AnimateBones());
            else
            {
                foreach (var ik in IkSolvers)
                {
                    ik.canAnimate = false;
                }
            }
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
        public void RandomizeBones()
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
            else
                removedGroundContactBones.Add(hipsBone);
        
            if (Random.value > 0.75f)
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
                var hits = Physics.RaycastAll(groundContactBones[i].position + Random.insideUnitSphere * Random.Range(0, 10) + Vector3.up * 100, Vector3.down, 500);
                for (var j = 0; j < hits.Length; j++)
                {
                    var hit = hits[j];

                    if (hit.collider.gameObject.layer != 23 && hit.collider.gameObject.layer != 28 &&
                        hit.collider.gameObject.layer != 29 && hit.collider.gameObject.layer != 30 &&
                        hit.collider.gameObject.layer != 25 && hit.collider.gameObject.layer != 20) // floor
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

        public IEnumerator PointToInteractable(Vector3 newPos)
        {
            //choose Ik target to pose here
            var randomIkSolver = IkSolvers[Random.Range(1, IkSolvers.Count)];
            randomIkSolver.canAnimate = true;
            yield return StartCoroutine(MoveTransformToPosition(randomIkSolver.Target.transform, newPos, Random.Range(5f, 10f)));
            randomIkSolver.canAnimate = false;
        }

        IEnumerator MoveTransformToPosition(Transform boneTransform, Vector3 newPos, float tt)
        {
            float t = 0;
            Vector3 startPos = boneTransform.position;
            
            while (t < tt)
            {
                boneTransform.position = Vector3.Lerp(startPos, newPos, t / tt);
                t += Time.smoothDeltaTime;
                yield return null;
            }
        }
    }
}
