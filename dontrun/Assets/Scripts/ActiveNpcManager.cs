using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ActiveNpcManager : MonoBehaviour
{
    public static ActiveNpcManager instance;
    public RandomizedNpcNamesData namesData;

    public ActiveNpc currentNpc = null;

    private GameManager gm;
    
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gm = GameManager.instance;
    }
    
    // this is called on the start of the main menu
    public void LoadCurrentActiveNpc(SavingData data)
    {
        CreateNpc(data.currentActiveNpcGoalFirst, data.currentActiveNpcGoalSecond,
            data.currentActiveNpcName1,data.currentActiveNpcName2,data.currentActiveNpcName3);
    }

    void CreateNpc(int goal1, int goal2, int name1, int name2, int name3)
    {
        currentNpc = null;
        currentNpc = new ActiveNpc();
        currentNpc.npcGoal1st = (ActiveNpc.Goal)goal1;

        if (goal1 == goal2)
        {
            if (goal1 == 0) goal2 = 6;
            else goal2--;
        }
        
        currentNpc.npcGoal2nd = (ActiveNpc.Goal)goal2;

        currentNpc.name1 = name1;
        currentNpc.name2 = name2;
        currentNpc.name3 = name3;
        
        for (int i = 0; i < namesData.firstNames[0].names.Count; i++)
        {
            string npcName = namesData.firstNames[name1].names[i] + " "
                          + namesData.secondNames[name2].names[i] + " "
                          + namesData.whereFrom[name3].names[i] + " ";
            
            currentNpc.name.Add(npcName);
        }
    }

    public void NpcIsDead()
    {
        CreateNpc(Random.Range(0, System.Enum.GetValues(typeof(ActiveNpc.Goal)).Length), Random.Range(0, System.Enum.GetValues(typeof(ActiveNpc.Goal)).Length),
            Random.Range(0, namesData.firstNames.Count),Random.Range(0, namesData.secondNames.Count), Random.Range(0, namesData.whereFrom.Count));
    }
}

[Serializable]
public class ActiveNpc
{
    public enum Goal
    {
        CollectGold, CollectItems, KillNpcs, SpawnTraps, NpcPricesHigher, NpcPricesLower, 
        SpawnRegenField, SpawnItemDescriptions, DisarmTraps,
    }
    
    public List<string> name = new List<string>();
    public int name1 = 0;
    public int name2 = 0;
    public int name3 = 0;
    public Goal npcGoal1st = Goal.CollectGold;
    public Goal npcGoal2nd = Goal.SpawnRegenField;
}
