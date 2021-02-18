using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using PlayerControls;
using UnityEngine;

public class WorldHolderMover : MonoBehaviour
{
    public float yPos = -1000;
    private PlayerMovement pm;

    void Start()
    {
        pm = PlayerMovement.instance;
    }
    
    void Update()
    {
        transform.position = new Vector3(pm.transform.position.x, yPos, pm.transform.position.z);
    }
}
