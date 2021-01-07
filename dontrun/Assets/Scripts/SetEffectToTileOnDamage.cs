using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Chat;
using PlayerControls;
using UnityEngine;

public class SetEffectToTileOnDamage : MonoBehaviour
{
    public StatusEffects.StatusEffect effectOnTileOnDamage = StatusEffects.StatusEffect.Null;
    public int spreadSize = 2;
    [Header("If effect threshold is 1, spread delay will do different thing. Look it up")]
    public float effectThreshold = 0.5f;
    public float spreadDelay = 5;
    public float lifeTime = 30;
 
    [Header("Use these for bonefire")]
    public bool clearTiles = false;
    public int conditionEffectIndex = 0;


    void Start()
    {
        if (effectThreshold >= 1)
        {
            var hc = gameObject.GetComponent<HealthController>();
            StartCoroutine(KeepSpawningEffect(hc));   
        }
    }

    // WORKS ONLY IN SOLO
    
    IEnumerator KeepSpawningEffect(HealthController hc)
    {
        while (hc.health > 0)
        {
            yield return new WaitForSeconds(spreadDelay);
            if (Vector3.Distance(transform.position, PlayerMovement.instance.transform.position) <= 30)
                Damaged(hc);
        }
    }
    
    public void Damaged(HealthController hc)
    {
        if (hc.health / hc.healthMax <= effectThreshold)
        {
            var lg = LevelGenerator.instance;
            float distance = 100;
            TileController usedTile = null;

            for (var index = 0; index < lg.levelTilesInGame.Count; index++)
            {
                var t = lg.levelTilesInGame[index];
                
                if (t == null)
                    continue;
                
                var newDist = Vector3.Distance(t.transform.position, transform.position);
                if (newDist <= distance)
                {
                    distance = newDist;
                    usedTile = t;
                }
            }

            if (usedTile != null)
            {
                if (!clearTiles)
                    usedTile.SpreadStatusEffect(effectOnTileOnDamage, spreadSize, spreadDelay, lifeTime, null);
                else // clear tiles
                {
                    if (hc.statusEffects[conditionEffectIndex].effectActive)
                    {
                        usedTile.SpreadStatusEffect(effectOnTileOnDamage, spreadSize, spreadDelay, lifeTime, null);                        
                    }
                }
            }   
        }
    }
}
