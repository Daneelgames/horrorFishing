using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponDataRandomizer : MonoBehaviour
{
    [Header("Uncheck for tutorial / story weapons")]
    public bool generated = true;

    [Header("Hide this if weapon is dead / normal")]
    public GameObject npcVisual;
    
    public WeaponPickUp pickUp;
    public WeaponController wc;

    public bool dead = false;
    public bool npc = true;
    public bool npcPaid = false;

    public WeaponData weaponData;
    
    /*
    [Header("0 - eng, 1 - ru")]
    public List<GeneratedText> firstNames = new List<GeneratedText>();
    public List<GeneratedText> lastNames = new List<GeneratedText>();
    public List<GeneratedText> howWasMade = new List<GeneratedText>();
    public List<GeneratedText> previousOwnerDeath = new List<GeneratedText>();
    */
    
    [Header("READONLY")]
    public List<string> generatedDescriptions = new List<string>();
    public List<string> generatedName = new List<string>();

    private ItemsList il;

    public int r1 = 0;
    public int r2 = 0;
    public int r3 = 0;
    public int r4 = 0;
    public List<string> effectInName = new List<string>();
    [Header("STATUS EFFECT: 0 - poison, 1 - fire, 2 - bleed, 3 - rust, 4 - regen, 5 - gold hunger, 6 - cold")]
    public int statusEffect = 0;

    private GameManager gm;
    private WeaponGenerationManager wgm;

    void Start()
    {
        gm = GameManager.instance;
        il = ItemsList.instance;
        wgm = WeaponGenerationManager.instance;
    }
    
    public void GenerateOnSpawn(bool _dead, bool _forcedNpc)
    {
        wgm = WeaponGenerationManager.instance;
        r1 = Random.Range(0, weaponData.firstNames.Count);
        r2 = Random.Range(0, weaponData.lastNames.Count);
        r3 = Random.Range(0, weaponData.howWasMade.Count);
        r4 = Random.Range(0, weaponData.previousOwnerDeath.Count);
        
        if (generated)
        {
            statusEffect = wgm.GetStatusEffect();
            gm = GameManager.instance;
            if (_forcedNpc)
            {
                // do nothing
            }
            else
            {
                //dead = _dead;
                dead = true;
            
                if (dead || Random.value <= gm.level.deadWeaponRate) // dead weapon
                {
                    r3 = -1;
                    statusEffect = -1;

                    if (pickUp && pickUp.npc) // npc weapon
                    {
                        KillWeapon();
                    }
                }
                if (pickUp && pickUp.npc) // npc weapon
                {
                    if (Random.value > gm.level.npcWeaponRate)
                    {
                        NoNpc();
                    }
                }   
            }
        }
        
        generatedName = new List<string>();
        UpdateDescription();
    }

    public void RandomStatusEffect(bool forceNpc)
    {
        if (dead) // pick random status
        {
            dead = false;
            statusEffect = wgm.GetStatusEffect();
        }
        else // pick different status
        {
            statusEffect = wgm.GetNewStatusEffect(statusEffect);
        }

        if (forceNpc)
            npc = true;
        
        r3 = Random.Range(0, weaponData.howWasMade.Count);
        
        SendInfo(false);
        UpdateDescription();
        wc.NewStatusEffect();
    }

    public void SetStatusEffect(int effectIndex)
    {
        dead = false;
        statusEffect = effectIndex;
            
        SendInfo(false);
        UpdateDescription();
        wc.NewStatusEffect();
    }

    public void NpcPaid()
    {
        npcPaid = true;
    }
    
    public void KillWeapon()
    {
        //print("Kill weapon: " + gameObject.name);
        dead = true;
        npc = false;
        r3 = -1;
        statusEffect = -1;

        if (pickUp)
        {
            if (pickUp.npc)
                Destroy(pickUp.npc.gameObject);
            pickUp.npc = null;
        }

        if (npcVisual)
        {
            npcVisual.SetActive(false);
        }
    }

    void NoNpc()
    {
        npc = false;
        if (pickUp)
        {
            if (pickUp.npc)
                Destroy(pickUp.npc.gameObject);
            pickUp.npc = null;
        }
    }

    public void SendInfo(bool secondWeapon)
    {
        il = ItemsList.instance;
        
        if (!secondWeapon)
        {
            il.activeWeaponInfo = new List<int>();
            il.activeWeaponInfo.Add(r1);
            il.activeWeaponInfo.Add(r2);
            il.activeWeaponInfo.Add(r3);
            if (r3 < 0) dead = true;
            il.activeWeaponInfo.Add(r4);
            il.activeWeaponInfo.Add(statusEffect);
            il.activeWeaponNpc = npc;
            il.activeWeaponNpcPaid = npcPaid;

        }
        else
        {
            il.secondWeaponInfo = new List<int>();
            il.secondWeaponInfo.Add(r1);
            il.secondWeaponInfo.Add(r2);
            il.secondWeaponInfo.Add(r3);
            if (r3 < 0) dead = true;
            il.secondWeaponInfo.Add(r4);
            il.secondWeaponInfo.Add(statusEffect);
            il.secondWeaponNpc = npc;
            il.secondWeaponNpcPaid = npcPaid;
        }
    }

    public void ReturnInfo(bool secondWeapon)
    {
        il = ItemsList.instance;
        
        if (!secondWeapon)
        {
            r1 = il.activeWeaponInfo[0];
            r2 = il.activeWeaponInfo[1];
            r3 = il.activeWeaponInfo[2];
            if (r3 < 0) dead = true;
            r4 = il.activeWeaponInfo[3];
            statusEffect = il.activeWeaponInfo[4];
            npc = il.activeWeaponNpc;
            npcPaid = il.activeWeaponNpcPaid;
        }
        else
        {
            r1 = il.secondWeaponInfo[0];
            r2 = il.secondWeaponInfo[1];
            r3 = il.secondWeaponInfo[2];
            if (r3 < 0) dead = true;
            r4 = il.secondWeaponInfo[3];
            statusEffect = il.secondWeaponInfo[4];
            npc = il.secondWeaponNpc;
            npcPaid = il.secondWeaponNpcPaid;
        }
        UpdateDescription();
    }
    
    public void UpdateDescription()
    {
        generatedDescriptions.Clear();
        generatedName.Clear();
        effectInName.Clear();
        
        if (dead) 
            KillWeapon();
        if (!npc)
            NoNpc();
        
        for (int i = 0; i < weaponData.firstNames[0].text.Count; i++) // iterate through languages locals 
        {
            string ammo = "";
            WeaponConnector connector = null;
            if (pickUp && pickUp.weaponConnector)
                connector = pickUp.weaponConnector;
            else if (wc && wc.weaponConnector)
                connector = wc.weaponConnector;

            if (connector)
            {
                if (i == 0)
                    ammo = "Amount of barrels: " + connector.barrelsCount + ". Can hold " +
                           connector.clipSize + " ammo.";
                else if (i == 1)
                    ammo = "Количество стволов: " + connector.barrelsCount + ". Вмещает патронов: " +
                           connector.clipSize + ".";
                else if (i == 2)
                    ammo = "Numero de cañones: " + connector.barrelsCount + ". Puede llevar " +
                           connector.clipSize + " rondas.";
                else if (i == 3)
                    ammo = "Anzahl an Läufen: " + connector.barrelsCount + ". Hält " +
                           connector.clipSize + " Patronen.";
            }

            string howWasMade = "";
            if (r3 > -1)
            {
                howWasMade = weaponData.howWasMade[r3].text[i];
            }
            else if (dead)
            {
                if (i == 0)
                    howWasMade = "Dead.";
                else if (i == 1)
                    howWasMade = "Мертв.";
                else if (i == 2)
                    howWasMade = "Muerto.";
                else if (i == 3)
                    howWasMade = "Tot.";
            }

            string statusEffectString = "";

            if (statusEffect > -1)
            {
                statusEffectString = weaponData.effect[statusEffect].text[i];
            }
            else
            {
                if (i == 0)
                    howWasMade += "Damage halved.";
                else if (i == 1)
                    howWasMade += "Дамаг снижен вдвое.";
                else if (i == 2)
                    howWasMade += "Daño reducido a la mitad.";
                else if (i == 3)
                    howWasMade += "Schaden halbiert.";
            }

            effectInName.Add("");
            
            if (i == 0)
            {
                switch (statusEffect)
                {
                    case -1:
                        effectInName[i] = "Dead";
                        break;
                
                    case 0:
                        effectInName[i] = "Poison";
                        break;
                
                    case 1:
                        effectInName[i] = "Fire";
                        break;
                
                    case 2:
                        effectInName[i] = "Bleeding";
                        break;
                
                    case 3:
                        effectInName[i] = "Rust";
                        break;
                
                    case 4:
                        effectInName[i] = "Regeneration";
                        break;
                
                    case 5:
                        effectInName[i] = "Gold Hunger";
                        break;
                }
            }
            else if (i == 1)
            {
                switch (statusEffect)
                {
                    case -1:
                        effectInName[i] = "Мертв";
                        break;
                
                    case 0:
                        effectInName[i] = "Яд";
                        break;
                
                    case 1:
                        effectInName[i] = "Огонь";
                        break;
                
                    case 2:
                        effectInName[i] = "Кровотечение";
                        break;
                
                    case 3:
                        effectInName[i] = "Ржавчина";
                        break;
                
                    case 4:
                        effectInName[i] = "Регенерация";
                        break;
                
                    case 5:
                        effectInName[i] = "Голод по золоту";
                        break;
                }
            }
            else if (i == 2)
            {
                switch (statusEffect)
                {
                    case -1:
                        effectInName[i] = "Muerto";
                        break;

                    case 0:
                        effectInName[i] = "Veneno";
                        break;

                    case 1:
                        effectInName[i] = "Fuego";
                        break;

                    case 2:
                        effectInName[i] = "Sangrando";
                        break;

                    case 3:
                        effectInName[i] = "Oxido";
                        break;

                    case 4:
                        effectInName[i] = "Regeneración";
                        break;

                    case 5:
                        effectInName[i] = "Aurofagia";
                        break;
                }
            }
            else if (i == 3)
            {
                switch (statusEffect)
                {
                    case -1:
                        effectInName[i] = "Tot";
                        break;

                    case 0:
                        effectInName[i] = "Gift";
                        break;

                    case 1:
                        effectInName[i] = "Flamme";
                        break;

                    case 2:
                        effectInName[i] = "Blutung";
                        break;

                    case 3:
                        effectInName[i] = "Rost";
                        break;

                    case 4:
                        effectInName[i] = "Regeneration";
                        break;

                    case 5:
                        effectInName[i] = "Goldfieber";
                        break;
                }
            }

            string npcLine = "";
            if (i == 0)
            {
                if (npc)
                    npcLine = "Smart - applies [" + effectInName[i].ToUpper() + "] on you when attacking.";
            }
            else if (i == 1)
            {
                if (npc)
                    npcLine = "Разумный - накладывает [" + effectInName[i].ToUpper() + "] на тебя при атаке.";
            }
            else if (i == 2)
            {
                if (npc)
                    npcLine = "Smart - applies [" + effectInName[i].ToUpper() + "] on you when attacking.";
            }
            else if (i == 3)
            {
                if (npc)
                    npcLine = "Smart - applies [" + effectInName[i].ToUpper() + "] on you when attacking.";
            }

            string noise = "";
            if (wc)
            {
                string newString = "";
                if  (i == 0)
                    newString = "It can be heard in " + wc.noiseDistance + " steps away. ";
                else if (i == 1)
                    newString = "Его слышно с " + wc.noiseDistance + " шагов. ";
                else if (i == 2)
                    newString = "Se puede escuchar en " + wc.noiseDistance + " pasos. ";
                else if (i == 3)
                    newString = "Es ist in " + wc.noiseDistance + " schritten zu hören. ";
                
                noise = newString;
            }
            

            generatedName.Add(weaponData.firstNames[r1].text[i] + " " + weaponData.lastNames[r2].text[i]);
            string specialEffect = ". ";
            if (weaponData.specialEffect.text.Count > 0)
            {
                specialEffect += weaponData.specialEffect.text[i];
            }
            generatedDescriptions.Add(generatedName[i]  + ". "
                                            + statusEffectString + ". "
                                            + npcLine + " "
                                            + howWasMade + " "
                                            + ammo + " "
                                            + noise
                                            + weaponData.previousOwnerDeath[r4].text[i]
                                            + specialEffect);
            
            gm = GameManager.instance;
            
        }

        if (wc)
            wc.descriptions = new List<string>(generatedDescriptions);

        if (dead)
        {
            if (pickUp && pickUp.npc)
            {
                pickUp.npc.gameObject.SetActive(false);
            }
            
            if (npcVisual)
            {
                npcVisual.SetActive(false);
            }
        }
    }

    public void NewDescription(List<string> generatedNam, List<string> generatedDesc, 
        int rr1, int rr2, int rr3, int rr4, 
        int statusEffect2, bool _dead, bool _npc, bool _npcPaid)
    {
        generatedName = new List<string>(generatedNam);
        r1 = rr1;
        r2 = rr2;
        r3 = rr3;
        r4 = rr4;
        dead = _dead;
        npc = _npc;
        npcPaid = _npcPaid;
        if (r3 < 0) dead = true;

        if (dead) 
            KillWeapon();
        if (!npc)
            NoNpc();
        statusEffect = statusEffect2;
        
        generatedDescriptions = new List<string>(generatedDesc);
        if (wc) wc.descriptions = new List<string>(generatedDesc);
        
        UpdateDescription();
    }
}

[Serializable]
public class GeneratedText
{
    public List<string> text = new List<string>();
}
