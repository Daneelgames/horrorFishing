using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassController : MonoBehaviour
{
    public Transform target;

    private Vector3 vector;
    
    // Update is called once per frame
    void Update()
    {
        vector.z = -target.eulerAngles.y;
        transform.localEulerAngles = vector;
    }
}
