using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using Random = UnityEngine.Random;

public  class ScrollSpellsData: MonoBehaviour
{
    public List<string> spellCodeList = new List<string>();
    public List<SpellList> spellLists = new List<SpellList>();
    
    public static ScrollSpellsData instance;

    void Awake()
    {
        instance = this;
    }

    public void ActivateScrollEffect(SpellScroll scroll, int randomSpell)
    {
        // effect chousen, now let's remove letters
        
        // letters removed, create event and give feedback to player
        print("Choose spell " + scroll.keywordsFound[randomSpell]);
        switch (scroll.keywordsFound[randomSpell])
        {
            case 0:
                // heal
                PlayerMovement.instance.hc.Heal(-1);
                break;
            
            case 1:
                // loud run
                PlayerMovement.instance.runNoiseDistance *= 2;
                
                break;
            case 2:
                // you shoot loudly
                if (WeaponControls.instance.activeWeapon) WeaponControls.instance.activeWeapon.noiseDistance *= 2;
                if (WeaponControls.instance.secondWeapon) WeaponControls.instance.secondWeapon.noiseDistance *= 2;
                
                break;
            case 3:
                // you hit quietly
                if (WeaponControls.instance.activeWeapon) WeaponControls.instance.activeWeapon.noiseDistance /= 2;
                if (WeaponControls.instance.secondWeapon) WeaponControls.instance.secondWeapon.noiseDistance /= 2;
                break;
            
            case 4:
                // if the legs are intact
                SpawnController.instance.SpawnMobByIndex(0);
                SpawnController.instance.SpawnMobByIndex(0);
                SpawnController.instance.SpawnMobByIndex(0);
                break;
            case 5:
                // it hurts me to watch
                PlayerMovement.instance.hc.Damage(100, transform.position, transform.position, null, null, false,
                    null,
                    null, null, false);
                break;
            case 6:
                // meat gold digging vultures
                ItemsList.instance.GetGold(30);
                break;
            
            case 7:
                // i'm a blind oldman
                UiManager.instance.LostAnEye();
                ItemsList.instance.LostAnEye();
                break;
            case 8:
                // I was swimmin' in the ocean of blood
                PlayerMovement.instance.hc.InstantBleed();
                break;
            case 9:
                // meats were hiding behind the blood
                SpawnController.instance.SpawnMobByIndex(3);
                SpawnController.instance.SpawnMobByIndex(3);
                SpawnController.instance.SpawnMobByIndex(3);
                break;
            
            case 10:
                // meat look at you
                SpawnController.instance.SpawnMobByIndex(3);
                break;
            case 11:
                // i want you to kiss meat
                PlayerMovement.instance.hc.usedTile.SpreadStatusEffect(StatusEffects.StatusEffect.InLove, 5, 5, 20, null);
                break;
            case 12:
                //I broke a weapon after hitting a fatty.
                if (WeaponControls.instance.activeWeapon) WeaponControls.instance.activeWeapon.Broke();
                break;
            
            case 13:
                // Explosion explode.
                var explosion = Instantiate(GameManager.instance.player.wc.allTools[0].grenadeExplosion, PlayerMovement.instance.transform.position, Quaternion.identity);
                explosion.DestroyEffect(true);
                
                break;
            case 14:
                // I got poisoned.
                PlayerMovement.instance.hc.InstantPoison();
                break;
            case 15:
                // I got cured .
                PlayerMovement.instance.hc.Antidote();
                break;
            
            case 16:
                // I started to bleed.
                PlayerMovement.instance.hc.InstantBleed();
                break;
            case 17:
                // My wounds healed.
                PlayerMovement.instance.hc.Heal(-1);
                break;
            case 18:
                // Door exploded.
                
                break;
            case 19:
                // Mimic jumped on me.
                SpawnController.instance.AttractMonsters(PlayerMovement.instance.transform.position, true);
                break;
            
            case 20:
                // Mimic tore my thigh.
                SpawnController.instance.AttractMonsters(PlayerMovement.instance.transform.position, true);
                break;
            case 21:
                // Mr. Legs  jumped on me.
                SpawnController.instance.SpawnMobByIndex(2);
                break;
            case 22:
                // I found a shotgun.
                
                break;
            
            case 23:
                // I found gold.
                ItemsList.instance.GetGold(30);
                
                break;
            case 24:
                // I gnawed at the gold.
                ItemsList.instance.LoseGold(30);
                
                break;
            case 25:
                // The gold in my skull buzzed.
                var skill = ItemsList.instance.skillsData.skills[1];
                PlayerSkillsController.instance.AddSkill(skill);
                break;
            
            case 26:
                // Eye Taker tore me apart.
                SpawnController.instance.SpawnMobByIndex(12);
                
                break;
            case 27:
                // I pulled out my eye.
                UiManager.instance.LostAnEye();
                ItemsList.instance.LostAnEye();
                
                break;
            case 28:
                // Blood flooded my dress.
                PlayerMovement.instance.hc.InstantBleed();
                break;
            
            case 29:
                // The meat grabbed my hair.
                SpawnController.instance.SpawnMobByIndex(4);
                
                break;
            case 30:
                // Gut jumper scratched my breast!
                SpawnController.instance.SpawnMobByIndex(2);
                SpawnController.instance.SpawnMobByIndex(2);
                
                break;
            case 31:
                // The room filled with poison.
                PlayerMovement.instance.hc.usedTile.SpreadStatusEffect(StatusEffects.StatusEffect.Poison, 3, 1, 40, null);
                break;
            
            case 32:
                // Everyone attacked me.
                SpawnController.instance.StartBadRepChase();
                break;
            case 33:
                // I started to run.
                SpawnController.instance.StartBadRepChase();
                break;
            case 34:
                // The fire lit.
                PlayerMovement.instance.hc.usedTile.SpreadStatusEffect(StatusEffects.StatusEffect.Fire, 5, 5, 20, null);
                break;
            case 35:
                // Something stirred in my chest.
                PlayerMovement.instance.hc.usedTile.SpreadStatusEffect(StatusEffects.StatusEffect.HealthRegen, 2, 2, 30, null);
                break;
            case 36:
                // The meat mole dragged me away.
                PlayerSkillsController.instance.InstantTeleport(PlayerMovement.instance.transform.position);
                break;
        }

        string resultString = spellLists[scroll.keywordsFound[randomSpell]].spells[GameManager.instance.language];
        scroll.SetScrollResult(resultString);
    }

    [Serializable]
    public class SpellList
    {
        public List<string> spells;
    }
}