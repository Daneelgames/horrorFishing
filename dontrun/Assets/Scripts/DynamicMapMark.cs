using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicMapMark : MonoBehaviour
{
    public HealthController master;
    public Transform rotatingTransform;
    
    void Start()
    {
        transform.parent = null;

        if (master && master.player && master.playerMovement)
        {
            rotatingTransform = master.playerMovement.mouseLook.transform;
        }
    }
    
    void Update()
    {
        if (master != null && master.gameObject.activeInHierarchy)
        {
            transform.position = master.transform.position;

            if (rotatingTransform)
            {
                transform.rotation = rotatingTransform.localRotation;
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y,0);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
