using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoseToValCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject == GameManager.instance.player.gameObject)
        {
            if (!GameManager.instance.demo && InteractionController.instance.objectInHands &&
                InteractionController.instance.objectInHands.gameObject.name == "Rose")
            {
                SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_2_7");
            }
        }
    }
}
