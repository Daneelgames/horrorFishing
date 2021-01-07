using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoneController : MonoBehaviour
{
    [HideInInspector]
    public Camera cam;
    AudioListener camAudioListener;
    RearWindowGameManager gm;

    private void Start()
    {
        cam = transform.parent.gameObject.GetComponent<Camera>();
        transform.SetParent(null);
        camAudioListener = cam.gameObject.GetComponent<AudioListener>();
        gm = RearWindowGameManager.instance;
        gm.cameraZoneControllers.Add(this);
        SetActive();
    }

    public void SetActive()
    {
        cam.gameObject.SetActive(true);
        gm.activeCameraZone = this;
        foreach (CameraZoneController camZone in gm.cameraZoneControllers)
        {
            if (camZone != this)
            {
                camZone.SetInactive();
            }
        }
        foreach (GameObject o in gm.objectsTurnToCamera)
        {
            o.transform.LookAt(cam.transform);
        }
    }

    public void SetInactive()
    {
        cam.gameObject.SetActive(false);
    }
}