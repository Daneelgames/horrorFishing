using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class AnimatorTriggerOnColliderTrigger : MonoBehaviour
{
    public Animator anim;
    public string triggerName;
    
    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject == PlayerMovement.instance.gameObject)
        {
            anim.SetTrigger(triggerName);
        }
    }
}