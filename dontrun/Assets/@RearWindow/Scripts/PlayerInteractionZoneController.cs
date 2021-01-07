using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionZoneController : MonoBehaviour
{
    public InteractiveObjectController closestObject;

    public List<InteractiveObjectController> objectsInRange = new List<InteractiveObjectController>();
    RearWindowGameManager gm;
    RearWindowPlayerController pc;

    void Start()
    {
        gm = RearWindowGameManager.instance;
        pc = gm.pc;
        pc.interactionZone = this;
        transform.SetParent(null);
    }

    void Update()
    {
        transform.position = pc.transform.position;
        transform.rotation = pc.transform.rotation;

        if (pc.canMove)
        {
            if (objectsInRange.Count > 0)
            {
                float distance = 10;
                foreach (InteractiveObjectController obj in objectsInRange)
                {
                    if (obj != null)
                    {
                        float newDistance = Vector3.Distance(pc.transform.position, obj.transform.position);
                        if (newDistance >= distance)
                        {
                            obj.ToggleFeedback(false);
                        }
                        else
                        {
                            distance = newDistance;
                            obj.ToggleFeedback(true);
                            closestObject = obj;
                        }   
                    }
                }
            }
        }
    }

    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            if (objectsInRange.Count > 0)
            {
                foreach (InteractiveObjectController obj in objectsInRange)
                {
                    if (obj.gameObject == other.gameObject)
                    {
                        return;
                    }
                }
            }

            foreach(InteractiveObjectController objInGm in gm.interactiveObjectControllers)
            {
                if (other.gameObject == objInGm.gameObject)
                {
                    objectsInRange.Add(objInGm);
                    return;
                }
            }
        }
    }
    */

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            if (objectsInRange.Count > 0)
            {
                foreach (InteractiveObjectController obj in objectsInRange)
                {
                    if (obj.gameObject == other.gameObject)
                    {
                        return;
                    }
                }
            }
            foreach (InteractiveObjectController objInGm in gm.interactiveObjectControllers)
            {
                if (other.gameObject == objInGm.gameObject)
                {
                    objectsInRange.Add(objInGm);
                    return;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectsInRange.Count > 0)
        {
            foreach (InteractiveObjectController objInGm in gm.interactiveObjectControllers)
            {
                if (other.gameObject == objInGm.gameObject)
                {
                    objectsInRange.Remove(objInGm);
                    objInGm.ToggleFeedback(false);
                    if (objectsInRange.Count == 0)
                        closestObject = null;
                    return;
                }
            }
        }
    }
}