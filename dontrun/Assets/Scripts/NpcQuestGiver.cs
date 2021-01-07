using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcQuestGiver : MonoBehaviour
{
    public static NpcQuestGiver instance;
    public NpcController npcController;
    
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            ChooseQuest();
        }
    }

    public void ChooseQuest()
    {
        int currentLevel = GutProgressionManager.instance.playerFloor;

        switch (currentLevel)
        {
            
        }
    }
    
    
}
