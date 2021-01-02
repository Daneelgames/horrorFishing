using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class HubTimeLimiter : MonoBehaviour
{
    public static HubTimeLimiter instance;
    public HealthController ikarusPrefab;

    
    void Awake()
    {
        instance = this;
    }
    public void StartDarkness()
    {
        if (Random.value < 0.05f)
        {
            Vector3 newPos = Vector3.up * 200f;

            Instantiate(ikarusPrefab, newPos, Quaternion.identity);   
        }
    }
}
