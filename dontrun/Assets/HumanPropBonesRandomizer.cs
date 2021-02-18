using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class HumanPropBonesRandomizer : MonoBehaviour
    {
        public bool grounded = false;
        CustomIKJoint lowestBone = null;
        public LayerMask groundMask;
        [Range(0, 1)] public float changeToAnimate = 0; 
        public List<Transform> boneList = new List<Transform>();
        public List<Vector3> bonesEulearAnglesListTpose = new List<Vector3>();
        public List<Vector3> bonesLocalPositionsListTpose = new List<Vector3>();
    
        [Header("Generated Pose")]
        public List<Vector3> bonesEulearAnglesListTemp = new List<Vector3>();
    
        public List<Transform> groundContactBones = new List<Transform>();
    
        public List<Transform> removedGroundContactBones = new List<Transform>();
        public List<Transform> armsBonesTargets = new List<Transform>();
        public Transform headBoneTarget;
        public Transform hipsBone;


        private Vector3 hipsLocalPosition;

        public List<CustomIKSolver> IkSolvers;
        public bool simulateGravity = true;
        
        IEnumerator Start()
        {
            hipsLocalPosition = hipsBone.localPosition;
            
            RandomizeBones();
        
            foreach (var ik in IkSolvers)
            {
                ik.canAnimate = false;
            }
            yield return new WaitForSeconds(1f);
            if (simulateGravity)
                StartCoroutine(SimulateGravity());
        }

        private Collider[] hitColliders;
        private bool foundGround = false;
        IEnumerator SimulateGravity()
        {
            //find lowest ground point
            float lowestY = transform.position.y + 1000;
            for (int i = 0; i < IkSolvers.Count; i++)
            {
                if (groundContactBones.Contains(IkSolvers[i].Target))
                {
                    if (IkSolvers[i].Ankle.transform.position.y < lowestY)
                    {
                        lowestY = IkSolvers[i].Ankle.transform.position.y;
                        lowestBone = IkSolvers[i].Ankle;
                    }
                }
            }
            yield return null;

            do
            {
                yield return new WaitForSeconds(0.5f);

                foundGround = false;
                hitColliders = Physics.OverlapSphere(lowestBone.transform.position, 0.25f, groundMask);

                for (int index = 0; index < hitColliders.Length; index++)
                {
                    if (hitColliders[index].transform.IsChildOf(transform)) continue;

                    foundGround = true;
                    break;
                }

                grounded = foundGround;
            } while (grounded);

            var rb = gameObject.AddComponent<Rigidbody>();
            StartCoroutine(WaitForRigidbodyToSleep(rb));
        }

        IEnumerator WaitForRigidbodyToSleep(Rigidbody rb)
        {
            do
            {
                yield return new WaitForSeconds(1f);
            } while (rb.velocity.magnitude > 0.1f);
            
            Destroy(rb);
            
            
            StartCoroutine(NewPoseInRuntime());
            yield return new WaitForSeconds(1f);
            
            StartCoroutine(SimulateGravity());
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
        
            for (int i = 0; i < boneList.Count; i++)
            {
                boneList[i].transform.eulerAngles = bonesEulearAnglesListTpose[i] + new Vector3(Random.Range(-40, 40),Random.Range(-40, 40),Random.Range(-40, 40));
                if (Random.value > 0.5f)
                {
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
                }
            }

            for (int i = 0; i < removedGroundContactBones.Count; i++)
            {
                removedGroundContactBones[i].transform.position += Random.insideUnitSphere * Random.Range(0, 10);
            }

            hipsBone.localPosition = hipsLocalPosition;
        }
        
        IEnumerator NewPoseInRuntime()
        {
            for (int i = 0; i < IkSolvers.Count; i++)
            {
                IkSolvers[i].Target.position = IkSolvers[i].Ankle.transform.position;
            }
            
            foreach (var ik in IkSolvers)
            {
                ik.canAnimate = true;
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
                }
            }

            for (int i = 0; i < IkSolvers.Count; i++)
            {
                IkSolvers[i].Target.position += Random.insideUnitSphere * Random.Range(0, 10);
            }

            float t = 0;
            Vector3 hipsStartPos = hipsBone.localPosition;
            
            while (t < 1)
            {
                hipsBone.localPosition = Vector3.Lerp(hipsStartPos, hipsLocalPosition, t / 1);
                t += Time.deltaTime;
                yield return null;
            }
            
            foreach (var ik in IkSolvers)
            {
                ik.canAnimate = false;
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
