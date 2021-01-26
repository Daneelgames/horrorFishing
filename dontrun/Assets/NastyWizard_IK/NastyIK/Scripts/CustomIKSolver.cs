using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class CustomIKSolver : MonoBehaviour {

    //public string _name; // this is just a label to tell what solver this is if you have multiple on the same game object
    //[Space(25)]
    public string label = "Enter Limb Name";

    public List<CustomIKJoint> Joints; // list of joints that are part of the calculation for IK

    public CustomIKJoint Ankle; // foot end effector location
    public Transform Target; // where the IK is pointing at

    public int Itterations = 3; // how many times the IK is calculated per frame
    public float maxScale = 2;

    public bool Visualise;
    public bool canAnimateScale = true;
    private bool animateScale = true;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);
        animateScale = canAnimateScale;
    }

    private void LateUpdate()
    {
        SolveIK();
        
        if (animateScale)
            AnimateScale();
    }

    private Quaternion rotTarget;
    private Quaternion r;
    private Vector3 j;    
    private Vector3 p;
    private float distanceToTarget = 0;
    
    void SolveIK()
    {
        for (int k = 0; k < Itterations; k++)
        {
            for (int i = 0; i < Joints.Count; i++)
            {
                p = Ankle.transform.position;
                j = Joints[i].transform.position;
                r = Joints[i].transform.rotation;

                rotTarget = Quaternion.FromToRotation(p - j, Target.position - j) * r;

                Joints[i].transform.rotation = Quaternion.Slerp(Joints[i].transform.rotation, rotTarget, (float)(i + 1) / Joints.Count);
            }
        }
    }

    void AnimateScale()
    {
        distanceToTarget = Vector3.Distance(Ankle.transform.position, Target.position);
        
        if (distanceToTarget > 0.1f)
            transform.localScale = Vector3.ClampMagnitude(transform.localScale * (1 + Time.deltaTime * 3), maxScale);
        else if (transform.localScale.x > 1)
            transform.localScale *= 1 - Time.deltaTime * 3;
    }
}
