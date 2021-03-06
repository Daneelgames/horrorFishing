using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

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

        public AudioSource changePoseAudioSource;
        public AudioClip changePoseAudioClip;
        
        private bool updateSolvers = false;
        void Start()
        {
            StartCoroutine(Init());
        }

        public IEnumerator Init()
        {
            updateSolvers = true;
            hipsLocalPosition = hipsBone.localPosition;
            
            RandomizeBones();

            
            foreach (var ik in IkSolvers)
            {
                //ik.animateScale = false;
                ik.canAnimate = false;
            }
            yield return new WaitForSeconds(1f);
            if (simulateGravity)
                StartCoroutine(SimulateGravity());   
        }

        private void LateUpdate()
        {
            if (updateSolvers)
            {
                for (int i = 0; i < IkSolvers.Count; i++)
                {
                    IkSolvers[i].UpdateSolver();
                }
            }
        }

        private Collider[] hitColliders;
        private bool foundGround = false;
        IEnumerator SimulateGravity()
        {
            do
            {
                foundGround = false;
                for (int i = 0; i < IkSolvers.Count; i++)
                {
                    //hitColliders = Physics.OverlapSphere(IkSolvers[i].Ankle.transform.position, IkSolvers[i].transform.localScale.x, groundMask);
                    hitColliders = Physics.OverlapSphere(IkSolvers[i].Ankle.transform.position, 0.5f, groundMask);

                    for (int index = 0; index < hitColliders.Length; index++)
                    {
                        yield return null;
                        
                        if (hitColliders[index] == null || hitColliders[index].transform.IsChildOf(transform))
                        {
                            continue;
                        }

                        foundGround = true;
                        break;
                    }

                    yield return new WaitForSeconds(0.1f);
                }

                grounded = foundGround;
            } while (grounded);

            foreach (var ik in IkSolvers)
            {
                ik.animateScale = false;
            }
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.mass = 10;
            rb.angularDrag = 1;
            rb.drag = 1;
            StartCoroutine(WaitForRigidbodyToSleep(rb));
        }

        IEnumerator WaitForRigidbodyToSleep(Rigidbody rb)
        {
            do
            {
                yield return new WaitForSeconds(0.1f);
            } while (rb.velocity.magnitude > 0.1f);
            
            Destroy(rb);
            
            
            yield return StartCoroutine(NewPoseInRuntime());
            
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

        private Vector3 tempPosForContactBone;
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
        
            FindNewGroundContactPoints();

            for (int i = 0; i < removedGroundContactBones.Count; i++)
            {
                // NEW
                tempPosForContactBone = hipsLocalPosition + Random.insideUnitSphere * Random.Range(2, 10);
                if (Physics.Raycast(tempPosForContactBone + Vector3.up * 100, Vector3.down, out var hit, 1000f, groundMask))
                {
                    removedGroundContactBones[i].transform.position = hit.point + Vector3.up * Random.Range(1f,5f);
                }
            }

            hipsBone.localPosition = hipsLocalPosition;
        }
        
        IEnumerator NewPoseInRuntime()
        {
            foreach (var ik in IkSolvers)
            {
                //ik.canAnimate = true;
                ik.SetAnimateInstantly(true);
            }

            changePoseAudioSource.pitch = Random.Range(0.5f, 1.5f);
            changePoseAudioSource.loop = true;
            changePoseAudioSource.clip = changePoseAudioClip;
            changePoseAudioSource.time = changePoseAudioSource.clip.length * Random.Range(0.1f, 0.9f); 
            changePoseAudioSource.Play();
            
            FindNewGroundContactPoints();

            float rTime = Random.Range(1f, 3f);
            float t = 0;

            for (int i = 0; i < IkSolvers.Count; i++)
            {
                tempPosForContactBone = transform.position + hipsLocalPosition + Random.insideUnitSphere * Random.Range(2, 10);
                
                if (Physics.Raycast(tempPosForContactBone + Vector3.up * 100, Vector3.down, out var hit, 1000f, groundMask))
                {
                    StartCoroutine(MoveBoneToPoint(IkSolvers[i].Target.transform, IkSolvers[i].Target.transform.position, hit.point + Vector3.up * Random.Range(1f, 5f), rTime));
                }
            }

            Vector3 hipsStartPos = hipsBone.localPosition;
            
            while (t < rTime)
            {
                hipsBone.localPosition = Vector3.Lerp(hipsStartPos, hipsLocalPosition, t / rTime);
                t += Time.deltaTime;
                yield return null;
            }
            
            foreach (var ik in IkSolvers)
            {
                //ik.canAnimate = true;
                ik.SetAnimateInstantly(false);
            }
            
            changePoseAudioSource.Stop();
        }

        void FindNewGroundContactPoints()
        {
            float distance = 1000;
            float newDistance = 0;
            Vector3 finalPoint;
            for (int i = 0; i < groundContactBones.Count; i++)
            {
                var hits = Physics.RaycastAll(groundContactBones[i].position + Random.insideUnitSphere * Random.Range(0, 10) + Vector3.up * 100, Vector3.down, 500);
                finalPoint = groundContactBones[i].transform.position;
                distance = 1000;
                newDistance = 0;
                for (var j = 0; j < hits.Length; j++)
                {
                    var hit = hits[j];

                    if (hit.collider.gameObject.layer != 23 && hit.collider.gameObject.layer != 28 &&
                        hit.collider.gameObject.layer != 29 && hit.collider.gameObject.layer != 30 &&
                        hit.collider.gameObject.layer != 25 && hit.collider.gameObject.layer != 20) // floor
                        continue;

                    newDistance = Vector3.Distance(groundContactBones[i].transform.position, hit.point);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        finalPoint = hit.point;
                    }    
                }

                StartCoroutine(MoveBoneToPoint(groundContactBones[i].transform,
                    groundContactBones[i].transform.position, finalPoint, 1));
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
            yield return StartCoroutine(MoveBoneToPoint(randomIkSolver.Target.transform, randomIkSolver.Target.transform.position, newPos, Random.Range(1f, 5f)));
            randomIkSolver.canAnimate = false;
        }

        IEnumerator MoveBoneToPoint(Transform bone, Vector3 startPos, Vector3 endPos, float tt)
        {
            float t = 0;
            while (t < tt)
            {
                bone.transform.position = Vector3.Lerp(startPos, endPos, t / tt);
                t += Time.smoothDeltaTime;
                yield return null;
            }
        }
    }
}
