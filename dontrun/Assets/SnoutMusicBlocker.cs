using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class SnoutMusicBlocker : MonoBehaviour
{
    public CapsuleCollider collider;
    private PlayerMovement pm;

    void OnTriggerEnter(Collider coll)
    {
        pm = PlayerMovement.instance;

        if (coll.gameObject == pm.gameObject)
        {
            // player is inside blocker, set volume down
            SetRandomTrackOnStart.snoutMusicPlayerInstance.SetVolume(this, 0);
        }
    }
}
