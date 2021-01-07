using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class MoveBikeToPlayer : MonoBehaviour
{
    public List<GameObject> bikesToMove = new List<GameObject>();

    private PlayerMovement pm;
    
    void Start()
    {
        if (GameManager.instance.snoutFound == 1)
        {
            pm = PlayerMovement.instance;
            Vector3 targetPos = pm.transform.position;
            targetPos = PlayerCheckpointsController.instance.checkpoints[GameManager.instance.hubActiveCheckpointIndex]
                .transform.position;
            
            for (var index = 0; index < bikesToMove.Count; index++)
            {
                bool active = false;
                var b = bikesToMove[index];
                if (b != null)
                {
                    active = b.activeInHierarchy;
                    if (active)
                    {
                        b.SetActive(false);
                    }
                    b.transform.position = targetPos;
                    if (active)
                    {
                        b.SetActive(true);
                    }   
                }
            }
        }
    }
}
