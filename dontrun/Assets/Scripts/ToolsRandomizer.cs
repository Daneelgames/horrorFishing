using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ToolsRandomizer : MonoBehaviour
{
    public static ToolsRandomizer instance;
    
    public List<ToolDescription> descriptions;
    public List<Effect> effects;

    private ItemsList il;
    private GameManager gm;
    
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gm = GameManager.instance;
        il = ItemsList.instance;
    }
    
    public void Init()
    {
        List<ToolDescription> tempDescriptions = new List<ToolDescription>(descriptions);
        List <Effect> tempEffects = new List<Effect>(effects);
        //tempEffects = new List<Effect>(RemoveDangerousEffects(tempEffects));
        ItemsList.instance.savedTools.Clear();
        
        
        for (int i = 0; i < tempDescriptions.Count; i++) // вариаций описаний и визуалов (самих предметов) больше чем видов эффектов.
        {
            if (tempEffects.Count < 1)
            {
                tempEffects = new List<Effect>(effects);
            }
            
            int randomEffect = UnityEngine.Random.Range(0, tempEffects.Count);

            int descriptionIndex = descriptions.IndexOf(tempDescriptions[i]);
            int effectIndex = effects.IndexOf(tempEffects[randomEffect]);
            
                
            if (LevelGenerator.instance && LevelGenerator.instance.levelgenOnHost)
            {
                // HOST
                GLNetworkWrapper.instance.SaveTool(descriptionIndex, effectIndex);
            }
            else if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
            {
                // solo
                SaveToolOnClient(descriptionIndex, effectIndex);
            }
            
            tempEffects.RemoveAt(randomEffect);
        }
    }

    public void SaveToolOnClient(int descriptionIndex, int effectIndex)
    {
        Tool newTool = new Tool();
        newTool.amount = 0;
        newTool.maxAmount = 2;
        newTool.known = false;
        newTool.toolController = descriptions[descriptionIndex].tool;
        newTool.type = effects[effectIndex].type;
            
        for (int j = 0; j < descriptions[descriptionIndex].info.Count; j++)
        {
            newTool.info.Add(descriptions[descriptionIndex].info[j] + " " + effects[effectIndex].info[j]);    
        }
        for (int j = 0; j < descriptions[descriptionIndex].unknownInfo.Count; j++)
        {
            newTool.unknownInfo.Add(descriptions[descriptionIndex].unknownInfo[j] + " " + effects[effectIndex].unknownInfo[j]);    
        }
        for (int j = 0; j < descriptions[descriptionIndex].useMessage.Count; j++)
        {
            newTool.useMessage.Add(descriptions[descriptionIndex].useMessage[j] + " " + effects[effectIndex].useMessage[j]);    
        }
        for (int j = 0; j < descriptions[descriptionIndex].throwMessage.Count; j++)
        {
            newTool.throwMessage.Add(descriptions[descriptionIndex].throwMessage[j] + " " + effects[effectIndex].throwMessage[j]);    
        }
        for (int j = 0; j < descriptions[descriptionIndex].effectHints.Count; j++)
        {
            newTool.effectHint.Add(descriptions[descriptionIndex].effectHints[j] + " " + effects[effectIndex].effectHints[j]);    
        }
            
            
        ItemsList.instance.savedTools.Add(newTool);
    }
    
    /*
    public void SaveToolOnClient(int descriptionIndex)
    {
        Tool newTool = new Tool();
        newTool.amount = 0;
        newTool.maxAmount = 2;
        newTool.known = false;
        newTool.toolController = tempDescriptions[i].tool;
        newTool.toolController = descriptions[descriptionIndex].tool;
        newTool.type = tempEffects[randomEffect].type;
            
        for (int j = 0; j < tempDescriptions[i].info.Count; j++)
        {
            newTool.info.Add(tempDescriptions[i].info[j] + " " + tempEffects[randomEffect].info[j]);    
        }
        for (int j = 0; j < tempDescriptions[i].unknownInfo.Count; j++)
        {
            newTool.unknownInfo.Add(tempDescriptions[i].unknownInfo[j] + " " + tempEffects[randomEffect].unknownInfo[j]);    
        }
        for (int j = 0; j < tempDescriptions[i].useMessage.Count; j++)
        {
            newTool.useMessage.Add(tempDescriptions[i].useMessage[j] + " " + tempEffects[randomEffect].useMessage[j]);    
        }
        for (int j = 0; j < tempDescriptions[i].throwMessage.Count; j++)
        {
            newTool.throwMessage.Add(tempDescriptions[i].throwMessage[j] + " " + tempEffects[randomEffect].throwMessage[j]);    
        }
        for (int j = 0; j < tempDescriptions[i].effectHints.Count; j++)
        {
            newTool.effectHint.Add(tempDescriptions[i].effectHints[j] + " " + tempEffects[randomEffect].effectHints[j]);    
        }
            
            
        tempEffects.RemoveAt(randomEffect);
        il.savedTools.Add(newTool);
    }*/
    
}

[Serializable]
public class ToolDescription
{
    public ToolController tool;
    [Header("0 - eng; 1-  rus; 2 - esp; 3 - ger")] 
    public List<string> info = new List<string>();
    public List<string> unknownInfo = new List<string>();
    
    public List<string> useMessage = new List<string>();
    public List<string> throwMessage = new List<string>();
    
    public List<string> effectHints = new List<string>();
}

[Serializable]
public class Effect
{
    public ToolController.ToolType type = ToolController.ToolType.Heal;
    
    public List<string> info = new List<string>();
    public List<string> unknownInfo = new List<string>();
    
    public List<string> useMessage = new List<string>();
    public List<string> throwMessage = new List<string>();
    
    public List<string> effectHints = new List<string>();
}