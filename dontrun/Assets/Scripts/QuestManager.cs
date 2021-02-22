using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    
    [Header("All quests list")]
    public QuestData questData;
    
    public List<int> activeQuestsIndexes = new List<int>();
    public List<int> completedQuestsIndexes = new List<int>();

    private GameManager gm;
    
    void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        gm = GameManager.instance;
        
        /*
        // check floor quest
        // iterate through quests
        if (gm.tutorialPassed == 1)
        {
            for (int i = 1; i <= GutProgressionManager.instance.playerFloor + 1; i++)
            {
                CompleteQuest(i);
            }   
        }
        UiManager.instance.UpdateQuest();
        */
        StartQuest(0);
    }

    public void StartQuest(int questIndex)
    {
        if (questIndex == -1 || activeQuestsIndexes.Contains(questIndex) || completedQuestsIndexes.Contains(questIndex)) return;

        if (questIndex >= questData.quests.Count)
        {
            Debug.LogError("No such quest");
        }
        else
        {
            activeQuestsIndexes.Add(questIndex);
            UiManager.instance.UpdateJournalIcon();
        
            if (!gm) gm = GameManager.instance;
        }

        HubItemsSpawner.instance.UpdateHub();
        gm.SaveGame();
    }
    
    public void CompleteQuest(int index)
    {
        print("Try to complete quest with index " + index);
        
        if (!activeQuestsIndexes.Contains(index))
            return;
        
        activeQuestsIndexes.Remove(index);
        if (!completedQuestsIndexes.Contains(index))
        {
            completedQuestsIndexes.Add(index);
            
            if (questData.quests.Count > index)
                StartQuest(questData.quests[index].startQuestOnCompletion);   
        }
        
        HubItemsSpawner.instance.UpdateHub();
        gm.SaveGame();
    }

    public void ClearCompletedQuestsFromActive()
    {
        for (int i = activeQuestsIndexes.Count - 1; i >= 0; i--)
        {
            if (completedQuestsIndexes.Contains(activeQuestsIndexes[i]))
                activeQuestsIndexes.RemoveAt(i);
        }
    }
}