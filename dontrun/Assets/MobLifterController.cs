using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class MobLifterController : MonoBehaviour
{
    public float height = 2;
    
    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.layer == 11 && coll.gameObject != PlayerMovement.instance.gameObject)
        {
            var newMob = GetMob(coll.gameObject);
            if (newMob != null)
            {
                AddCurrentMobLifter(newMob);
            }
        }
    }
    
    void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.layer == 11 && coll.gameObject != PlayerMovement.instance.gameObject)
        {
            var newMob = GetMob(coll.gameObject);
            if (newMob != null)
            {
                RemoveCurrentMobLifter(newMob);
            }
        }
    }

    HealthController GetMob(GameObject go)
    {
        var gm = GameManager.instance;
        HealthController foundMob = null;
        for (int i = gm.units.Count - 1; i >= 0; i--)
        {
            if (gm.units[i] != null && gm.units[i].health > 0 && gm.units[i].gameObject == go)
            {
                // alive mob found
                foundMob = gm.units[i];
                break;
            }
        }

        return foundMob;
    }

    void AddCurrentMobLifter(HealthController mob)
    {
        if (mob.mobGroundMovement && mob.mobGroundMovement.currentMobLifter != this)
            mob.mobGroundMovement.currentMobLifter = this;
        else if (mob.bombBehaviour && mob.bombBehaviour.currentMobLifter != this)
            mob.bombBehaviour.currentMobLifter = this;
    }
    
    void RemoveCurrentMobLifter(HealthController mob)
    {
        if (mob.mobGroundMovement && mob.mobGroundMovement.currentMobLifter == this)
            mob.mobGroundMovement.currentMobLifter = null;
        else if (mob.bombBehaviour && mob.bombBehaviour.currentMobLifter == this)
            mob.bombBehaviour.currentMobLifter = null;
    }
}
