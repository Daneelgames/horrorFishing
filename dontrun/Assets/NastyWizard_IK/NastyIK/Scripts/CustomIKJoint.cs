using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomIKJoint : MonoBehaviour {

    //public Vector3 Axis;

    //public float MinAngle; // unused for now, hope to use soon
    //public float MaxAngle;

    //public float AngleOffset;

    //public bool Visualise;


    [HideInInspector] public Vector3 StartOffset;

    [HideInInspector] public bool detailToggle = false;
    //[HideInInspector] public float rotation;

    void Awake()
    {
        StartOffset = transform.localPosition;
    }
}
