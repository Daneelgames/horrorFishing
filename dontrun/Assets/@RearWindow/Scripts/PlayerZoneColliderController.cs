using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerZoneColliderController : MonoBehaviour
{
    Rigidbody rb;
    RearWindowPlayerController pc;
    RearWindowGameManager gm;
    public CameraZoneController activeCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        gm = RearWindowGameManager.instance;
        pc = gm.pc;
        pc.zoneCollider = this;

        transform.SetParent(null);
    }

    private void Update()
    {
        transform.position = pc.transform.position;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 9 && gm && pc && pc.canMove )
        {
            float distance = 1000;
            foreach(CameraZoneController camZone in gm.cameraZoneControllers)
            {
                if (camZone.gameObject == col.gameObject)
                {
                    float newDistance = Vector3.Distance(transform.position, camZone.transform.position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        activeCamera = camZone;
                    }
                }
            }
            if (activeCamera)
                activeCamera.SetActive();
        }
    }
}