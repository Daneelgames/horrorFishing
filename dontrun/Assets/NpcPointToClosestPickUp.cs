using System.Collections;
using System.Collections.Generic;
using Assets;
using UnityEngine;

public class NpcPointToClosestPickUp : MonoBehaviour
{
    public RandomizedPhrasesData pointerPhrases;
    public bool alreadyPointed = false;
    public HumanPropBonesRandomizer bonesRandomizer;

    private DynamicObstaclesManager dom;
    public void PointOnClosestPickUp()
    {
        alreadyPointed = true;
        Interactable closestPickUp = null;

        Vector3 targetPos = transform.position;
        
        List<Vector3> tempList = new List<Vector3>();
        if (dom.spawnedLeg)
            tempList.Add(dom.spawnedLeg.transform.position);
        if (dom.spawnedRevolver)
            tempList.Add(dom.spawnedRevolver.transform.position);

        float distance = 1000f;
        float newDistance = 0f;
        for (int i = 0; i < tempList.Count; i++)
        {
            newDistance = Vector3.Distance(tempList[i], transform.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                targetPos = tempList[i];
            }
        }
        
        StartCoroutine(bonesRandomizer.PointToInteractable(targetPos));
    }

    public bool CanInitPointerDialogue()
    {
        dom = DynamicObstaclesManager.instance;
        bool can = false;

        if (alreadyPointed)
            return false;
        
        if (dom.spawnedLeg != null ||
            dom.spawnedRevolver != null)
        {
            can = true;
        }

        return can;
    }
}
