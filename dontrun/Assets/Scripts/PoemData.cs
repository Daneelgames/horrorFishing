using System;
using System.Collections;
using System.Collections.Generic;
using ExtendedItemSpawn;
using UnityEngine;

[CreateAssetMenu(fileName = "PoemData/NewData", menuName = "Poem Data")]
public class PoemData : ScriptableObject
{
    public List<Lines> lines; // single line - single endig type

}

[Serializable]
public class Lines
{
    public List<string> engLines;
    public List<string> rusLines;
    public List<string> espLines;
    public List<string> gerLines;
    public List<string> itaLines;
    public List<string> spbrLines;
}
