using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class DestroyOnCondition : MonoBehaviour
{
    public float distanceCondition = -1;
    public bool and = false;
    public int destroyIfQuestNotActive = -1;
    public int destroyIfQuestActive = -1;
    
    
    IEnumerator Start()
    {
        while (true)
        {
            if (GetDestroy())
                Destroy(gameObject);

            yield return new WaitForSeconds(1);
        }   
    }


    bool GetDestroy()
    {
        bool destroy = false;

        if (!and) // OR CONDITION
        {
            if (distanceCondition > 0 &&
                Vector3.Distance(transform.position, PlayerMovement.instance.transform.position) > distanceCondition)
                destroy = true;
            else if (destroyIfQuestNotActive >= 0 &&
                     !QuestManager.instance.activeQuestsIndexes.Contains(destroyIfQuestNotActive))
                destroy = true;
            else if (destroyIfQuestActive >= 0 &&
                     QuestManager.instance.activeQuestsIndexes.Contains(destroyIfQuestActive))
                destroy = true;
        }
        else if (distanceCondition > 0 || destroyIfQuestNotActive >= 0 || destroyIfQuestActive >= 0) // AND CONDITION
        {
            if (distanceCondition < 0 || Vector3.Distance(transform.position, PlayerMovement.instance.transform.position) > distanceCondition
                && (destroyIfQuestNotActive < 0 || !QuestManager.instance.activeQuestsIndexes.Contains(destroyIfQuestNotActive))
                && (destroyIfQuestActive < 0 ||  QuestManager.instance.activeQuestsIndexes.Contains(destroyIfQuestActive)))
                destroy = true;
        }
        return destroy;
    }
}
