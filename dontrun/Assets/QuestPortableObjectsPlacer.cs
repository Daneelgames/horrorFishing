using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class QuestPortableObjectsPlacer : MonoBehaviour
{
    public List<Transform> targetParents;
    public int amount = 3;
    public int questToComplete = -1;
    public PortableObject.QuestPortable questPortableType = PortableObject.QuestPortable.GunnWood;
    public GameObject completedVisual;

    private IEnumerator Start()
    {
        while (true)
        {
            if (QuestManager.instance.completedQuestsIndexes.Contains(questToComplete))
            {
                 completedVisual.SetActive(true);
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        if (QuestManager.instance.completedQuestsIndexes.Contains(questToComplete) == false && coll.gameObject == PlayerMovement.instance.gameObject)
        {
            var objInHands = InteractionController.instance.objectInHands;
            if (objInHands && objInHands.portableQuestType == questPortableType)
            {
                StartCoroutine(MoveQuestPortableToTargetParent(objInHands));
            }
        }
    }

    IEnumerator MoveQuestPortableToTargetParent(PortableObject objInHands)
    {
        float t = 0;
        float tt = 1;

        Transform currentTarget = targetParents[Random.Range(0, targetParents.Count)];
        targetParents.Remove(currentTarget);
        objInHands.UseAsQuestPortable();
        objInHands.transform.parent = currentTarget;

        Quaternion startRotation = objInHands.transform.localRotation;
        Vector3 startPosition = objInHands.transform.localPosition;
        while (t < tt)
        {
            objInHands.transform.localPosition = Vector3.Lerp(startPosition, Vector3.zero, t /tt);
            objInHands.transform.localRotation = Quaternion.Slerp(startRotation, quaternion.identity, t /tt);

            t += Time.deltaTime;
            yield return null;
        }

        amount--;

        if (amount <= 0)
        {
            QuestManager.instance.CompleteQuest(questToComplete);
            completedVisual.SetActive(true);
        }
    }
}
