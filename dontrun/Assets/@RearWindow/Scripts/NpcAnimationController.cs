using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcAnimationController : MonoBehaviour
{
    public string trigger = "Action";
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (trigger.Length > 0)
            Invoke("SetTrigger", Random.Range(3,10));
    }

    void SetTrigger()
    {
        anim.SetTrigger(trigger);
        Invoke("SetTrigger", Random.Range(3, 10));
    }
}
