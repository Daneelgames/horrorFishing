using System;
using System.Collections;
using System.Collections.Generic;
using ExtendedItemSpawn;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData/NewQuest", menuName = "Quest Data")]
public class QuestData : ScriptableObject
{
    public List<Quest> quests;

}

[Serializable]
public class Quest
{
    public List<string> names;
    public List<string> descriptions;
    public string achievementOnStart;
    public string achievementOnComplete;

    public int startQuestOnCompletion;
}
