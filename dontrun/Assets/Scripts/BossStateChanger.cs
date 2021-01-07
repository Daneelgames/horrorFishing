using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStateChanger : MonoBehaviour
{
    [Range(0,1)]
    public float healthPercentage = 0.5f;
    public HealthController hc;
    
    private int currentState = 0;

    public CapsuleCollider damageCollider;
    public Vector3 damageColliderSecondPhaseCenter;
    public float damageColliderSecondPhaseRadius;
    public float damageColliderSecondPhaseHeight;
    public string newPhaseAnimString;
    public float changeStateTime = 2;
    
    public void BossDamaged()
    {
        if (hc.health / hc.healthMax <= healthPercentage && currentState == 0)
        {
            currentState = 1;
            damageCollider.center = damageColliderSecondPhaseCenter;
            damageCollider.radius = damageColliderSecondPhaseRadius;
            damageCollider.height = damageColliderSecondPhaseHeight;
            hc.mobPartsController.anim.SetBool(newPhaseAnimString, true);
            StartCoroutine(StopWhileChangingState());
        }
    }

    IEnumerator StopWhileChangingState()
    {
        hc.mobPartsController.agent.enabled = false;
        yield return  new WaitForSeconds(changeStateTime);
        hc.mobPartsController.agent.enabled = true;
    }
}