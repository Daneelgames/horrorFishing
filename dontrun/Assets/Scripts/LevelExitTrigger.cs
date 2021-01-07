 using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class LevelExitTrigger : MonoBehaviour
{
    private PlayerMovement pm;
    public bool arena = false;

    void Start()
    {
        pm = PlayerMovement.instance;
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject == pm.gameObject)
        {
            if (!arena)
                GameManager.instance.IntroLevelFinish();
            else
                GameManager.instance.LoadArena();
        }
    }
}
