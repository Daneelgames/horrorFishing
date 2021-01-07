using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{
    void Start()
    {
        RearWindowGameManager.instance.objectsTurnToCamera.Add(gameObject);
    }
}
