using System;
using System.Collections;
using System.Collections.Generic;
using ExtendedItemSpawn;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "RandomizedNamesData/NewData", menuName = "Names Data")]
public class RandomizedNpcNamesData : ScriptableObject
{
    public List<Names> firstNames = new List<Names>();
    public List<Names> secondNames = new List<Names>();
    public List<Names> whereFrom = new List<Names>();
    
    public List<Goal> goalsAndDescriptions = new List<Goal>();
    
}

[Serializable]
public class Names
{
    public List<string> names = new List<string>();
}

[Serializable]
public class Goal
{
    public ActiveNpc.Goal goal;
    public List<string> whatHeDoes = new List<string>();
}
