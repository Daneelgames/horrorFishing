using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CultGenerator : MonoBehaviour
{
    public StatusEffects.StatusEffect chosenCult = StatusEffects.StatusEffect.Null;
    private List<TileController> cultTiles = new List<TileController>();
    public List<RandomizedPhrasesData> cultLeadersPhrases = new List<RandomizedPhrasesData>();
    public List<RandomizedPhrasesData> cultFollowersPhrases = new List<RandomizedPhrasesData>();
    public List<HealthController> cultistsInGame = new List<HealthController>();
    
    private GameManager gm;
    private SpawnController sc;

    public static CultGenerator instance;
    
    // first add tiles from the levelgen

    void Awake()
    {
        if (instance != null)
            return;

        instance = this;
    }
    public void AddTileCult(TileController tile)
    {
        cultTiles.Add(tile);
    }
    
    // then create cult
    public void Init()
    {
        if (PlayerSkillsController.instance.activeCult != PlayerSkillsController.Cult.none) return;
        gm = GameManager.instance;
        var gpm = GutProgressionManager.instance;
        if (gm.tutorialPassed == 0 || gpm.playerFloor < 5 || gpm.playerFloor == gpm.bossFloor)
            return;
        
        sc = SpawnController.instance;

        int random = Random.Range(0, 4);
        int monstersAmount = Random.Range(3, 8);
        switch (random)
        {
            case 0:
                chosenCult = StatusEffects.StatusEffect.Poison;
                break;
            case 1:
                chosenCult = StatusEffects.StatusEffect.Fire;
                break;
            case 2:
                chosenCult = StatusEffects.StatusEffect.Bleed;
                break;
            case 3:
                chosenCult = StatusEffects.StatusEffect.GoldHunger;
                break;
        }
        
        for (var index = 0; index < cultTiles.Count; index++)
        {
            var tile = cultTiles[index];
            tile.tileStatusEffect = chosenCult;
        }
        
        // create monsters
        for (int i = 0; i < monstersAmount && cultTiles.Count > 0; i++)
        {
            int r = Random.Range(0, cultTiles.Count);
            sc.SpawnCultistMob(i, cultTiles[r].transform.position);
            if (cultTiles.Count <= 0)
            {
                break;
            }
            cultTiles.RemoveAt(r);
            
          // spawn random mob from mob list and make him friendly
          // give some of them a chase animation?
          
          // give the first one the leader phrases
          // give all others followers phrases
        }
    }

    public void AddCultist(HealthController cultist)
    {
        cultistsInGame.Add(cultist);
        int index = 0;
        switch (chosenCult)
        {
            case StatusEffects.StatusEffect.Fire:
                index = 1;
                break;
            case StatusEffects.StatusEffect.Bleed:
                index = 2;
                break;
            case StatusEffects.StatusEffect.GoldHunger:
                index = 5;
                break;
        }

        if (cultistsInGame.Count == 1)
        {
            SetCultist(cultist, index, true);   
        }
        else
        {
            SetCultist(cultist, index, false);   
        }
    }

    void SetCultist(HealthController cultist, int effectIndex, bool leaderPhrases)
    {
        int unitIndex = gm.units.IndexOf(cultist);
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.SetCultistOnClient(unitIndex, effectIndex, leaderPhrases);
        }
        else
        {
            SetCultistOnClient(unitIndex, effectIndex, leaderPhrases);
        }
    }

    public void SetCultistOnClient(int unitIndex, int effectIndex, bool leaderPhrases)
    {
        StartCoroutine(SetCultistOnClientCoroutine(unitIndex, effectIndex, leaderPhrases));        
        
    }

    IEnumerator SetCultistOnClientCoroutine(int unitIndex, int effectIndex, bool leaderPhrases)
    {
        print("SetCultistOnClient index " + unitIndex);
        while (unitIndex >= GameManager.instance.units.Count)
        {
            print("Don't enough units for making a cult. Wait");
            yield return null;
        }

        var cultist = GameManager.instance.units[unitIndex];
        List<RandomizedPhrasesData> phrases;
        
        if (leaderPhrases)
            phrases = new List<RandomizedPhrasesData>(cultLeadersPhrases);
        else
            phrases = new List<RandomizedPhrasesData>(cultFollowersPhrases);
        
        cultist.statusEffects[effectIndex].effectImmune = true;
        cultist.npcInteractor.randomizedPhrasesData = phrases[effectIndex];
        cultist.setEffectToTileOnDamage.effectOnTileOnDamage = chosenCult;
        
        cultist.npcInteractor.InitPhrases();
    }
}