using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("0 - eng, 1 - ru, 2 - esp, 3 - ger")]
    public List<GeneratedText> firstNames = new List<GeneratedText>();
    public List<GeneratedText> lastNames = new List<GeneratedText>();
    public List<GeneratedText> howWasMade = new List<GeneratedText>();
    public List<GeneratedText> previousOwnerDeath = new List<GeneratedText>();
    public List<GeneratedText> effect = new List<GeneratedText>();
    public List<GeneratedText> randomPhrases = new List<GeneratedText>();
    public GeneratedText specialEffect;
}