using System.Collections;
using System.Collections.Generic;
using ExtendedItemSpawn;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "RandomizedPhrasesData/NewData", menuName = "Phrases Data")]
public class RandomizedPhrasesData : ScriptableObject
{
    public List<Dialogue> dialogues = new List<Dialogue>();
}
