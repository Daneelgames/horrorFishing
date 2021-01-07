using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GutQuestsController : MonoBehaviour
{
    public static GutQuestsController instance;
    
    public List<QuestBlockingNpcProgress> quests;
    
    void Awake()
    {
        instance = this;
    }
    
    public void SaveQuestToManager(QuestBlockingNpcProgress quest)
    {
        quests.Add(quest);    
    }
    
    public void UnitKilled(HealthController hc)
    {
        //print(hc.name + " killed");
        if (quests.Count > 0)
        {
            for (int i = quests.Count - 1; i >= 0; i--)
            {
                if (quests[i].killTarget != null && quests[i].killTarget == hc)
                {
                    CompleteQuest(quests[i]);
                }
                else if (quests[i].poisonTarget != null && quests[i].poisonTarget == hc)
                    FailQuest(quests[i]);
                else if (quests[i].fireTarget != null && quests[i].fireTarget == hc)
                    FailQuest(quests[i]);
                else if (quests[i].bleedTarget != null && quests[i].bleedTarget == hc)
                    FailQuest(quests[i]);
            }   
        }
    }

    void CompleteQuest(QuestBlockingNpcProgress quest)
    {
        print("try to complete quest");
        if (quest.npc != null && quest.npc.npcInteractor != null && quest.npc.npcInteractor.gameObject.activeInHierarchy && quest.npc.npcInteractor.inQuest)
            quest.npc.npcInteractor.blockingQuest.completed = true;
    }
    void FailQuest(QuestBlockingNpcProgress quest)
    {
        print("try to fail quest");
        if (quest.npc != null && quest.npc.npcInteractor != null && quest.npc.npcInteractor.gameObject.activeInHierarchy && quest.npc.npcInteractor.inQuest)
            quest.npc.npcInteractor.blockingQuest.failed = true;
    }

    public void UnitPoisoned(HealthController hc)
    {
        print(hc.name + " poisoned");
        if (quests.Count > 0)
        {
            for (int i = quests.Count - 1; i >= 0; i--)
            {
                if (quests[i].poisonTarget != null && quests[i].poisonTarget == hc)
                {
                    CompleteQuest(quests[i]);
                }
            }   
        }
    }
    public void UnitOnFire(HealthController hc)
    {
        print(hc.name + " on fire");
        if (quests.Count > 0)
        {
            for (int i = quests.Count - 1; i >= 0; i--)
            {
                if (quests[i].fireTarget != null && quests[i].fireTarget == hc)
                {
                    CompleteQuest(quests[i]);
                }
            }   
        }
    }

    public void UnitBleeding(HealthController hc)
    {
        print(hc.name + " bleeding");
        if (quests.Count > 0)
        {
            for (int i = quests.Count - 1; i >= 0; i--)
            {
                if (quests[i].bleedTarget != null && quests[i].bleedTarget == hc)
                {
                    CompleteQuest(quests[i]);
                }
            }   
        }
    }

    public void ClearQuests()
    {
        quests.Clear();
    }
}
