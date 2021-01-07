using System.Collections;
using System.Collections.Generic;
using ExtendedItemSpawn;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEpigraph", menuName = "Epigraph Data")]
public class EpigraphsData : ScriptableObject
{
    public List<string> epigraphs = new List<string>();
}