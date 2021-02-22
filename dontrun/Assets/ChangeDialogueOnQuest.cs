using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDialogueOnQuest : MonoBehaviour
{
    [Tooltip("Set phrases if INT quest is active")]
    public List<RandomizedPhrasesData> phrasesOnQuests = new List<RandomizedPhrasesData>();
    public NpcController npcController;

    void Awake()
    {
        if (HubItemsSpawner.instance.dialogueOnQuestsChangers.Contains(this))
            return;
        
        HubItemsSpawner.instance.dialogueOnQuestsChangers.Add(this);
    }

    public void UpdateDialogue()
    {
        QuestManager qm = QuestManager.instance;

        if (phrasesOnQuests.Count > qm.activeQuestsIndexes[qm.activeQuestsIndexes.Count - 1] 
            && npcController.randomizedPhrasesData != phrasesOnQuests[qm.activeQuestsIndexes[qm.activeQuestsIndexes.Count - 1]])
        {
            npcController.randomizedPhrasesData = phrasesOnQuests[qm.activeQuestsIndexes[qm.activeQuestsIndexes.Count - 1]];
            npcController.InitPhrases();   
        }
    }
}
