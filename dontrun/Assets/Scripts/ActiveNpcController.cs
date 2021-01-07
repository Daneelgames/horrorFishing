using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveNpcController : MonoBehaviour
{
    private ActiveNpc.Goal goalFirst = ActiveNpc.Goal.CollectGold;
    private ActiveNpc.Goal goalSecond = ActiveNpc.Goal.SpawnRegenField;
    
    private ActiveNpc.Goal goalOpenToPlayerFirst = ActiveNpc.Goal.CollectGold;
    private ActiveNpc.Goal goalOpenToPlayerSecond = ActiveNpc.Goal.SpawnRegenField;
    
    public List<ActiveNpc.Goal> goalsOpenedToPlayer = new List<ActiveNpc.Goal>();

    public void Init()
    {
        
        if (goalsOpenedToPlayer.Contains(goalFirst)) goalOpenToPlayerFirst = goalFirst;
        else
        {
            int r = Random.Range(0, goalsOpenedToPlayer.Count);
            goalOpenToPlayerFirst = goalsOpenedToPlayer[r];
            goalsOpenedToPlayer.RemoveAt(r);
        }
        
        if (goalsOpenedToPlayer.Contains(goalSecond)) goalOpenToPlayerSecond = goalSecond;
        else
        {
            goalOpenToPlayerSecond = goalsOpenedToPlayer[Random.Range(0, goalsOpenedToPlayer.Count)];
        }
        
    }
}
