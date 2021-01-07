using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGenerationManager : MonoBehaviour
{
    public static WeaponGenerationManager instance;
    
    [Header("0 - poison, 1 - fire, 2 - bleed, 3 - rust, 4 - health regen, 5 - gold hunger")]
    public List<int> effectsPool = new List<int>();
    public List<int> effectsCurrentPool = new List<int>();

    public List<WeaponDataRandomizer> weaponRandomizers = new List<WeaponDataRandomizer>();
    
    void Awake()
    {
        instance = this;
        Init();
    }

    public void Init()
    {
        effectsCurrentPool.Clear();
        effectsCurrentPool = new List<int>(effectsPool);
    }


    public int GetStatusEffect()
    {
        int newEffect = Random.Range(0, effectsCurrentPool.Count);
        
        effectsCurrentPool.RemoveAt(newEffect);
        
        if (effectsCurrentPool.Count < 2)
            Init();
        
        return newEffect;
    }

    public int GetNewStatusEffect(int currentEffect)
    {
        var tempEffects = new List<int>(effectsPool);
        tempEffects.Remove(currentEffect);

        var sc = WeaponControls.instance.secondWeapon;
        if (sc && sc.dataRandomizer.statusEffect >= 0)
            tempEffects.Remove(sc.dataRandomizer.statusEffect);
        
        int newEffect = Random.Range(0, tempEffects.Count);
        
        return newEffect;
    }
}
