using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcSetPhrasesOnLevel : MonoBehaviour
{
    public List<RandomizedPhrasesData> phrasesOnLevels = new List<RandomizedPhrasesData>();

    public RandomizedPhrasesData GetRandomizedData()
    {
        var gpm = GutProgressionManager.instance;
        if (gpm.playerFloor >= phrasesOnLevels.Count)
            return phrasesOnLevels[Random.Range(0, phrasesOnLevels.Count)];
        
        // else
        return phrasesOnLevels[gpm.playerFloor];
    }

}
