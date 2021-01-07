using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class QuestItemController : MonoBehaviour
{
    public enum QuestItem
    {
        HerHead,
        HerPalm,
        HerLeg,
        HerArm,
        HerBody,
        GoldenHeart,
        Carl,
        Val,
        Camera
    }

    public int questToStartOnPickUp = -1;
    
    public QuestItem item = QuestItem.HerHead;

    void Start()
    {
        if (item == QuestItem.HerHead || item == QuestItem.HerPalm || item == QuestItem.HerBody || item == QuestItem.HerLeg|| item == QuestItem.HerArm || item == QuestItem.GoldenHeart)
        {
            SpawnController.instance.StopSpawningOnLevel();
        }
    }

    public IEnumerator PickUp()
    {
        QuestManager.instance.StartQuest(questToStartOnPickUp);

        if (item == QuestItem.HerHead || item == QuestItem.HerPalm || item == QuestItem.HerBody || item == QuestItem.HerLeg|| item == QuestItem.HerArm || item == QuestItem.GoldenHeart)
        {
            var il = ItemsList.instance;
            int floor = GutProgressionManager.instance.playerFloor;
            if (!il.foundHerPiecesOnFloors.Contains(floor))
                il.foundHerPiecesOnFloors.Add(floor);
            
            PlayerMovement.instance.hc.invincible = true;
            PlayerMovement.instance.goldenLightAnimator.SetBool("GoldenLight", true);
            PlayerMovement.instance.controller.enabled = false;

            print("here");
            
            //yield return new WaitForSecondsRealtime(2);
            
            float t = 0;
            float tTarget = 2;
            transform.parent = PlayerMovement.instance.portableTransform;
            var startPos = transform.localPosition;
            var endPos = PlayerMovement.instance.portableTransform.position + transform.parent.forward * 2;
            
            while (t < tTarget)
            {
                yield return null;
                transform.localPosition = Vector3.Lerp(startPos, endPos, t / tTarget);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, t / tTarget);
                t += Time.deltaTime;
            }
            print("here");

            GutProgressionManager.instance.PlayerFinishLevel();
            ItemsList.instance.PlayerFinishedLevel();
            GameManager.instance.ReturnToHub(false, true);
            
            /*
            if (GameManager.instance.difficultyLevel != GameManager.GameMode.MeatZone)
                GameManager.instance.ReturnToHub(false);
            else
                GameManager.instance.NextLevel();
            */
        }
        gameObject.SetActive(false);
    }
}