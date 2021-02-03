using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerControls;
using TMPro;
using Random = UnityEngine.Random;

public class NpcController : MonoBehaviour
{
    public enum RandomizedDataOrder
    {
        Before,
        Replace,
        After
    }

    public bool canCreateQuestGiver = false;
    public int roseNpc = -1;
    public int currentDialog = 0;
    public int currentLine = 0;
    public int eventsRepeat = 1;
    public int currentEventRepeat = 0;

    public NpcSetPhrasesOnLevel npcSetPhrasesOnLevel;
    public RandomizedPhrasesData randomizedPhrasesData;
    public bool randomizedPhrasesOrder = false;
    public QuestBlockingNpcProgress blockingQuest;
    public bool inQuest = false;
    public RandomizedDataOrder order = RandomizedDataOrder.Replace;
    public List<Dialogue> dialogues;
    public AudioClip npcVoice;
    public AudioSource audioSource;
    public NpcVoiceShuffler shuffler;
    public float speakDelay = 0.1f;
    public int distanceToCloseDialogue = 8;
    public TextMeshProUGUI phraseGui;
    public TextMeshProUGUI choiceGui;

    public Transform spawnTransform;
    public List<Interactable> toolsPool = new List<Interactable>();
    public List<Interactable> ammoPool = new List<Interactable>();
    public Interactable interactable;

    [Header("RandomSentencesAmount")]
    public int randomDialoguesAmountMin = 2;
    public int randomDialoguesAmountMax = 3;
    public int randomLinesAmountMin = 3;
    public int randomLinesAmountMax = 6;

    private GameObject parentObject;
    
    bool canInteract = true;
    public HealthController hc;
    PlayerMovement pm;
    WeaponControls wc;
    Coroutine hideText;
    UiManager ui;
    ItemsList il;
    private GameManager gm;
    bool choosing = false;
    public bool keySold = false;

    private float inLoveScaler = 1;
    public GameObject gameObjectActivateOnAgree;

    public Interactable mementoPrefab;
    public Interactable interactableToInteractOnDialogueAction;

    public bool snout = false;
    
    private void Start()
    {
        if (canCreateQuestGiver)
        {
            var questGiver = gameObject.AddComponent<NpcQuestGiver>();
            questGiver.npcController = this;
        }
        
        if (dialogues.Count > 0 && dialogues[0].dialogueEvent == Dialogue.DialogueEvent.Taxi)
        {
            parentObject = hc.transform.parent.parent.parent.gameObject;   
        }
                        
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        wc = WeaponControls.instance;
        il = ItemsList.instance;
        ui = UiManager.instance;

        audioSource.clip = npcVoice;
        
        phraseGui.gameObject.SetActive(false);
        choiceGui.gameObject.SetActive(false);

        if (npcSetPhrasesOnLevel)
            randomizedPhrasesData = npcSetPhrasesOnLevel.GetRandomizedData();
        
        InitPhrases();
    }

    public void InitPhrases()
    {
        if (randomizedPhrasesData)
        {
            if (randomizedPhrasesOrder)
            {
                // replace everything in random order
                var tempDialogues = new List<Dialogue>(dialogues);
                
                dialogues.Clear();
                
                // amount of phrases
                int sentencesAmount = Random.Range(randomDialoguesAmountMin, randomDialoguesAmountMax);
                int phrasesInSentenceAmount = Random.Range(randomLinesAmountMin, randomLinesAmountMax);

                for (int i = 0; i < sentencesAmount; i++)
                {
                    Dialogue newSentence = new Dialogue();
                    for (int j = 0; j < phrasesInSentenceAmount; j++)
                    {
                        int randomDialogIndex = Random.Range(0, randomizedPhrasesData.dialogues.Count);
                        Dialogue randomDialogue = randomizedPhrasesData.dialogues[randomDialogIndex];

                        switch (gm.language)
                        {
                            case 0:
                                newSentence.phrases.Add(randomDialogue.phrases[Random.Range(0, randomDialogue.phrases.Count)]);
                                break;
                            case 1:
                                newSentence.phrasesRu.Add(randomDialogue.phrasesRu[Random.Range(0, randomDialogue.phrasesRu.Count)]);
                                newSentence.phrases.Add(randomDialogue.phrases[Random.Range(0, randomDialogue.phrases.Count)]);
                                break;
                            case 2:
                                newSentence.phrasesESP.Add(randomDialogue.phrasesESP[Random.Range(0, randomDialogue.phrasesESP.Count)]);
                                newSentence.phrases.Add(randomDialogue.phrases[Random.Range(0, randomDialogue.phrases.Count)]);
                                break;
                            case 3:
                                newSentence.phrasesGER.Add(randomDialogue.phrasesGER[Random.Range(0, randomDialogue.phrasesGER.Count)]);
                                newSentence.phrases.Add(randomDialogue.phrases[Random.Range(0, randomDialogue.phrases.Count)]);
                                break;
                            case 4:
                                newSentence.phrasesIT.Add(randomDialogue.phrasesIT[Random.Range(0, randomDialogue.phrasesIT.Count)]);
                                newSentence.phrases.Add(randomDialogue.phrases[Random.Range(0, randomDialogue.phrases.Count)]);
                                break;
                            case 5:
                                newSentence.phrasesSPBR.Add(randomDialogue.phrasesSPBR[Random.Range(0, randomDialogue.phrasesSPBR.Count)]);
                                newSentence.phrases.Add(randomDialogue.phrases[Random.Range(0, randomDialogue.phrases.Count)]);
                                break;
                        }
                    }
                    dialogues.Add(newSentence);   
                }
                
                if (order == RandomizedDataOrder.Before && tempDialogues.Count > 0)
                {
                    for (int i = 0; i < tempDialogues.Count; i++)
                    {
                        dialogues.Add(tempDialogues[i]);
                    }
                }
            }
            else
            {
                if (order == RandomizedDataOrder.Before)
                {
                    var tempList = new List<Dialogue>(dialogues);
                    dialogues.Clear();
                    dialogues.Add(randomizedPhrasesData.dialogues[Random.Range(0,randomizedPhrasesData.dialogues.Count)]);
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        dialogues.Add(tempList[i]);
                    }
                }
                else if (order == RandomizedDataOrder.Replace)
                {
                    dialogues.Clear();
                    dialogues.Add(randomizedPhrasesData.dialogues[Random.Range(0,randomizedPhrasesData.dialogues.Count)]);   
                }
                else if (order == RandomizedDataOrder.After)
                {
                    dialogues.Add(randomizedPhrasesData.dialogues[Random.Range(0,randomizedPhrasesData.dialogues.Count)]);
                }   
            }
        }
    }

    private string activeString = "Active";
    public void Interact()
    {
        inLoveScaler = 1;
        
        if (hc)
        {
            if (hc.mobGroundMovement)
            {
                if (!hc.peaceful && !hc.damagedByPlayer)
                {
                    hc.peaceful = true;
                    hc.mobGroundMovement.target = null;
                }
            }   
            if (hc.inLove) inLoveScaler -= 0.33f;
        }

        if (PlayerSkillsController.instance.handsome) inLoveScaler -= 0.33f;
        
        if (canInteract)
        {
            ui.dialogueInteractor = this;
            if (!inQuest)
            {
                if (!choosing)
                {
                    ui.dialogueAnim.SetTrigger(activeString);
                    if (roseNpc >= 0)
                    {
                        if (!gm.roseNpcsInteractedInHub.Contains(roseNpc))
                            gm.roseNpcsInteractedInHub.Add(roseNpc);
                    }
                    StartCoroutine(NextPhrase());
                }
                else
                {
                    ui.dialogueAnim.SetTrigger(activeString);
                    StartCoroutine(MakeChoice());   
                }   
            }
            else
            {
                ui.dialogueAnim.SetTrigger(activeString);
                StartCoroutine(CheckBlockingQuest());
            }
        }
    }

    public void HideDialogue()
    {
        ui = UiManager.instance;
        if (ui.dialogueInteractor == this)
        {
            currentLine = 0;
            if (!ui || !ui.dialogueAnim)
                return;
                
            ui.dialogueAnim.SetTrigger("Inactive");
            ui.dialogueInteractor = null;
            ui.dialoguePhrase.text = "";
            ui.dialogueChoice.text = "";
            choosing = false;
        }
    }

    IEnumerator CheckBlockingQuest()
    {
        canInteract = false;
        string newPhrase = "";
        string newPhraseChoice = "";
        ui.dialogueSpeakerName.text = hc.names[gm.language];
        ui.dialoguePhrase.text = "";
        ui.dialogueChoice.text = "";
        if (shuffler) audioSource.clip = shuffler.GetVoiceClip();
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.Play();

        if (!choosing)
        {
            print("not choosing, check quest for completion");
            
            // check for completion
            if (blockingQuest.toolTarget != ToolController.ToolType.Null)
            {
                for (int i = il.savedTools.Count - 1; i >= 0; i--)
                {
                    if (il.savedTools[i].type == blockingQuest.toolTarget && il.savedTools[i].amount > 0)
                    {
                        blockingQuest.completed = true;
                        break;
                    }
                }
            }
            else if (blockingQuest.weaponTarget != WeaponPickUp.Weapon.Null)
            {
                if ((il.activeWeapon != WeaponPickUp.Weapon.Null && il.activeWeapon == blockingQuest.weaponTarget) || (il.secondWeapon != WeaponPickUp.Weapon.Null && il.secondWeapon == blockingQuest.weaponTarget))
                {
                    blockingQuest.completed = true;
                }
            }

            if (blockingQuest.failed)
            {
                newPhrase = Translator.TranslateText(blockingQuest.questsFailedLine[gm.language]);
                newPhraseChoice = "...";
            }
            else if (!blockingQuest.completed)
            {
                newPhrase = Translator.TranslateText(blockingQuest.questsDidntFinishedLine[gm.language]);
                newPhraseChoice = "...";
            }
            else
            {
                choosing = true;
                newPhrase = Translator.TranslateText(blockingQuest.questsFinishedLine[gm.language]);
                newPhraseChoice = Translator.TranslateText(blockingQuest.getRewardText[gm.language]);
            }
        
            foreach (char c in newPhrase)
            {
                ui.dialoguePhrase.text += c;
                yield return new WaitForSeconds(speakDelay);
            }

      
            foreach (char c in newPhraseChoice)
            {
                ui.dialogueChoice.text += c;
                yield return new WaitForSeconds(speakDelay);
            }
            
            currentLine = 0;
        }
        else // make choice
        {
            print("choose to finish quest");
            
            if (blockingQuest.toolTarget != ToolController.ToolType.Null)
            {
                // remove tool
                for (int i = il.savedTools.Count - 1; i >= 0; i--)
                {
                    if (il.savedTools[i].type == blockingQuest.toolTarget && il.savedTools[i].amount > 0)
                    {
                        il.savedTools[i].amount--;
                        wc.GiveToolToNpc();
                    }
                }
            }
            else if (blockingQuest.weaponTarget != WeaponPickUp.Weapon.Null)
            {
                if (il.activeWeapon == blockingQuest.weaponTarget)
                    wc.RemoveWeapon(0);
                else
                    wc.RemoveWeapon(1);
            }
            
            newPhrase = Translator.TranslateText(blockingQuest.npcThanks[gm.language]);
            foreach (char c in newPhrase)
            {
                ui.dialoguePhrase.text += c;
                yield return new WaitForSeconds(speakDelay);
            }
            
            newPhrase = blockingQuest.rewardTakenText[gm.language];
      
            foreach (char c in newPhrase)
            {
                ui.dialogueChoice.text += c;
                yield return new WaitForSeconds(speakDelay);
            }
            
            blockingQuest.rewarded = true;
        }

        // QUEST FULLY COMPLETE
        if (blockingQuest.rewarded)
        {
            // give reward
            GivePlayerQuestReward();
            
            
            blockingQuest = null;
            inQuest = false;
            if (currentDialog + 1 < dialogues.Count)
                currentDialog++;
            currentLine = 0;
            
            il.AddToBadReputation(-1f);
        }
        
        canInteract = true;
        if (hideText != null)
            StopCoroutine(hideText);

        hideText = StartCoroutine(HideText());
    }
    
    string newPhrase = String.Empty;
    string  newPhrasePhrase = String.Empty;
    IEnumerator NextPhrase()
    {
        canInteract = false;
        newPhrase = String.Empty;
        gm = GameManager.instance;
        if (hc)
        {
            if (hc.names.Count > gm.language)
                ui.dialogueSpeakerName.text = hc.names[gm.language];
            else
                ui.dialogueSpeakerName.text = hc.names[0];   
        }
        else
        {
            if (interactable.itemNames.Count > gm.language)
                ui.dialogueSpeakerName.text = interactable.itemNames[gm.language];
            else
                ui.dialogueSpeakerName.text = interactable.itemNames[0];
        }
        
        ui.dialoguePhrase.text = String.Empty;
        ui.dialogueChoice.text = String.Empty;
        
        if (shuffler) audioSource.clip = shuffler.GetVoiceClip();
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.Play();

        // LAST LINE OF DIALOGUE
        if (currentLine >= dialogues[currentDialog].phrases.Count - 1) // MIGHT REMOVED MINUS ONE HERE
        {
            if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.FixWeapon)
            {
                if (wc.activeWeapon == null || eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Rub ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold into its flesh?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Втереть ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" золота в его плоть?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Fortar ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Oro en su carne?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Reibe ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Gold in sein Fleisch?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Strofinare ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro nella sua carne?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Roçar ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Ouro em sua carne?");

                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.Heal)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;
                    
                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Put ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold inside?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Засунуть ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" золота внутрь?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Poner ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Oro dentro?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Tu ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Gold rein?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Mettere ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro dentro?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Colocar ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Ouro por dentro?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.BuySkill)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Throw away ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Выбросить ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" золота?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Tirar ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Schmeiß ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Gold weg?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Buttare via ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Jogar fora ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" ouro?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.GiveCarl)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Feed Glutton a plush turtle named Carl?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Скормить Обжоре плюшевую черепашку по имени Карл?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("¿Darle de comer a Glutton una tortuga de peluche llamada Carl?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Glutton eine Plüschschildkröte namens Carl füttern?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Dai da mangiare a Glutton una tartaruga di peluche di nome Carl?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Alimentar Glutton com uma tartaruga de pelúcia chamada Carl?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.SetNpcInLove)
            {
                if (currentEventRepeat >= eventsRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Ladyshoe wants to join the party");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Туфелька хочет вступить в пати");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Ladyshoe wants to join the party");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Ladyshoe wants to join the party");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Ladyshoe wants to join the party");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Ladyshoe wants to join the party");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.Pray)
            {
                if (currentEventRepeat >= eventsRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Pray with him?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Помолиться вместе с ним?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Pray with him?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Pray with him?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Pray with him?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Pray with him?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.AgreeSetObjectActiveAndHideNpc)
            {
                if (currentEventRepeat >= eventsRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Agree?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Согласиться?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Agree?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Agree?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Agree?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Agree?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.MeatHole)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Shove your hand in the meat hole?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Засунуть руку в мясную дыру?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("¿Meter tu mano en un agujero de carne?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Schieben Sie Ihre Hand in ein Fleischloch?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Metti una mano nel buco della carne?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Schieben Sie Ihre Hand in ein Fleischloch?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.BuyTool)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Feed ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold to it?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Скормить ему ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" золота?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Alimentarle ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Füttere es mit ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Gold?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Dargli da mangiare ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Alimentar ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" ouro para ele?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.BuyAmmo)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Feed ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold to it?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Скормить ему ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" золота?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Alimentarle ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Füttere es mit ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Gold?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Dargli da mangiare ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Alimentar ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" ouro para ele?");

                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.JoinToxicCult)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Give away  " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + "  gold and join the cult of the Toxic Grave?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Отдать " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " золота и вступить в культ Токсичной могилы?"); 
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Pagar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " y unirse al culto del Toxic Grave?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Bezahle " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " und dem Kult des Baby of Toxic Grave beitreten?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Dare via " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " oro ed aderire al culto della Tomba Tossica?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Doar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " ouro e se juntar ao culto dos derramamentos de sangue?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.JoinFlamesCult)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Give away  " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + "  gold and join the cult of the Baby of the Flames?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Отдать " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " золота и вступить в культ Младенца Пламени?"); 
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Pagar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " y unirse al culto del Baby of The Flames?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Bezahle " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " und dem Kult des Baby of The Flames beitreten?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Dare via " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " oro ed aderire al culto del Bambino delle Fiamme?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Doar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " ouro e se juntar ao culto do bebê das chamas?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.JoinBloodCult)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Give away  " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + "  gold and join the cult of the Bloodshed Swallowers?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Отдать " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " золота и вступить в культ Кровопролитных Глотателей?"); 
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Pagar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " y unirse al culto del Bloodshed Swallowers?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Bezahle " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " und dem Kult des Bloodshed Swallowers beitreten?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Dare via " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " oro ed aderire al culto degli Ingoiatori di Sangue?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Doar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " ouro e se juntar ao culto dos derramamentos de sangue?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.JoinGoldCult)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Give away  " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + "  gold and join the cult of the Golden Phallus?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Отдать " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " золота и вступить в культ Золотого Фаллоса?"); 
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Pagar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost  * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " y unirse al culto del falo dorado?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Bezahle " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " und dem Kult des Goldenen Phallus beitreten?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Dare via " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " und dem Kult des Culto del Fallo Aureo?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Doar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " ouro e se juntar ao Culto ao Falo Dourado?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.UpgradeWeapon)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Give him ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Отдать ему ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" золота?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Darle ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Gib ihm ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Dargli ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Dar a ele ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" ouro?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.BuyKey)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Feed ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold to it?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Скормить ему ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" золота?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Alimentarle ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Füttere es mit ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" Gold?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Dargli da mangiare ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" oro?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Alimentar ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" ouro para ele?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.BuyGoldenKey)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Rub ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold into his hair?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Втереть ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" золота в его волосы?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("¿Frotar ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" de oro en su cabello?");
                    else if (gm.language == 3)
                        newPhrase = Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" gold in sein Haar reiben?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Strofinare ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText("  monete d'oro nei suoi capelli?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Esfregar ") + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + Translator.TranslateText(" de ouro em seu cabelo?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.LoseAnEye)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Give her one eye?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Отдать ей один глаз?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Darle un ojo?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Gib ihr ein Auge?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Darle un occhio?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Dar um olho para ela?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.KillQuest)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Take the job?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Принять заказ на убийство?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Acceptar la orden de matar?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Aktzeptiere den Mordauftrag?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Prendere l'ordine di uccisione?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Receber a ordem de matar?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.ItemQuest)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Agree to get an item for him?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Согласиться добыть предмет для него?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Acuerdo en traer-le un item?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Aktzeptiere ihm ein Gegenstand zu besorgen?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Accetti di prendere un oggetto per lui?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Concorda em conseguir um item para ele?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.WeaponQuest)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Agree to get him the weapon?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Согласиться добыть оружие для него?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Acuerdo en conseguir una arma para él?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Stimmte zu, für ihn eine Waffe zu finden?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Accetti di prendere un arma per lui?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Concorda em pegar uma arma para ele?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.PoisonQuest)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Agree to poison someone's meat?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Согласиться отравить чье-то мясо?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Acuerdo en envenenar la carne de alguien?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Stimme zu. sein Fleisch zu vergiften?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Accetti di avvelenare la carne di qualcuno?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Concorda em envenenar a carne de alguém?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.FireQuest)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Agree to set someone's meat on fire?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Согласиться поджарить чье-то мясо?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Acuerdo en poner en llamas a la carne de alguien?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Stimme zu, sein Essen in Brand zu setzen?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Accetti di dare fuoco alla carne di qualcuno?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Concorda em incendiar a carne de alguém?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.BleedQuest)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Agree to make someone's meat bleed?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Согласиться изрезать чье-то мясо до кровотечения?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Acuerdo en hacer que la carne de alguien sangra?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Stimme zu, jemanden verbluten zu lassen?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Accetti di far sanguinare la carne di qualcuno?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Concorda em fazer sangrar a carne de alguém?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.OneLevelBack)
            {
                if (eventsRepeat <= currentEventRepeat || GutProgressionManager.instance.playerFloor <= 0)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Pay " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " gold and go one floor higher?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Заплатить " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " золота и отправиться на предыдущий этаж?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Pagar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " oro para ir a un piso mas alto?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Bezahle " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " und gehe eine Etage höher?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Pagare " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " oro e andare un piano più in alto?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Pagar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " ouro e subir um andar?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.KissHer)
            {
                if (eventsRepeat <= currentEventRepeat || GutProgressionManager.instance.playerFloor <= 0)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Kiss She?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Поцеловать Ее?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("¿Besarla?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Küss Sie?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Baciarla?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Beije-a?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.ReduceBadRep)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Share your gold with a stranger?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Поделиться золотом с незнакомцем?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Share the gold with a stranger?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Share the gold with a stranger?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Share the gold with a stranger?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Share the gold with a stranger?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.Warm)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Throw " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " gold into the flame?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Бросить " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " золота в пламя?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Tirar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " oro en las llamas?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Wirf " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " gold in das Feuer?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Lanciare " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " nelle fiamme?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Atirar " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " na chama?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.Taxi)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Blink?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Моргнуть?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("¿Parpadeo?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Blinken?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Blink?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Piscar?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.DisarmTrap)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    
                    newPhrasePhrase = String.Empty;
                    if (gm.language == 0)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                    else if (gm.language == 1)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                    else if (gm.language == 2)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                    else if (gm.language == 3)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                    else if (gm.language == 4)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                    else if (gm.language == 5)
                        newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;

                    choosing = true;

                    string chance = "35%";
                    string effect = "";
                    if (PlayerSkillsController.instance.knowTrapEffect)
                    {
                        if (hc.meatTrap.effect == StatusEffects.StatusEffect.Poison)
                        {
                            if (gm.language == 0) effect = "poison";
                            else if (gm.language == 1) effect = "ядовитую";
                            else if (gm.language == 2) effect = "Veneno";
                            else if (gm.language == 3) effect = "Gift";
                            else if (gm.language == 4) effect = "Veleno";
                            else if (gm.language == 5) effect = "Poção";
                        }
                        else if (hc.meatTrap.effect == StatusEffects.StatusEffect.Fire)
                        {
                            if (gm.language == 0) effect = "fire";
                            else if (gm.language == 1) effect = "огненную";
                            else if (gm.language == 2) effect = "fuego";
                            else if (gm.language == 3) effect = "flamme";
                            else if (gm.language == 4) effect = "fiamma";
                            else if (gm.language == 5) effect = "chama";
                        }
                        else if (hc.meatTrap.effect == StatusEffects.StatusEffect.Bleed)
                        {
                            if (gm.language == 0) effect = "bleeding";
                            else if (gm.language == 1) effect = "кровоточащую";
                            else if (gm.language == 2) effect = "Sangrando";
                            else if (gm.language == 3) effect = "Blutung";
                            else if (gm.language == 4) effect = "Sanguinamento";
                            else if (gm.language == 5) effect = "Sangramento";
                        }
                        else if (hc.meatTrap.effect == StatusEffects.StatusEffect.Rust)
                        {
                            if (gm.language == 0) effect = "rusty";
                            else if (gm.language == 1) effect = "ржавую";
                            else if (gm.language == 2) effect = "Óxido";
                            else if (gm.language == 3) effect = "Rost";
                            else if (gm.language == 4) effect = "Ruggine";
                            else if (gm.language == 5) effect = "Ferrugem";
                        }
                        else if (hc.meatTrap.effect == StatusEffects.StatusEffect.GoldHunger)
                        {
                            if (gm.language == 0) effect = "golden";
                            else if (gm.language == 1) effect = "золотую";
                            else if (gm.language == 2) effect = "Oro";
                            else if (gm.language == 3) effect = "Gold";
                            else if (gm.language == 4) effect = "d'oro";
                            else if (gm.language == 5) effect = "dourada";
                        }
                        else if (hc.meatTrap.effect == StatusEffects.StatusEffect.Cold)
                        {
                            if (gm.language == 0) effect = "freeze";
                            else if (gm.language == 1) effect = "ледяную";
                            else if (gm.language == 2) effect = "congelada";
                            else if (gm.language == 3) effect = "gefronen";
                            else if (gm.language == 4) effect = "congelato";
                            else if (gm.language == 5) effect = "congeladas";
                        }   
                    }
                        
                    if (PlayerSkillsController.instance.thirtyFingers)
                        chance = "75%";
                    
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText("Try to disarm a " + effect + " meat trap with a " + chance +" chance?");
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText("Попытаться обезвредить " + effect + " мясную ловушку с шансом в " + chance +"?");
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText("Intenar a desarmar " + effect + " trampa de carne con una probabilidad de " + chance + "?");
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText("Versuche eine " + effect + " Fleischfalle mit a " + chance + "Chance zu entschärfen?");
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText("Provare a disinnescare " + effect + " trappola di carne con una probabilità del " + chance + "?");
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText("Tentar desarmar um " + effect + " armadilha de carne com " + chance + "de chance?");
                    foreach (char c in newPhrase)
                    {
                        ui.dialogueChoice.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
            }
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.GivePlayerWeapon)
            {
                if (eventsRepeat <= currentEventRepeat)
                {
                    if (gm.language == 0)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
                    else if (gm.language == 1)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
                    else if (gm.language == 2)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
                    else if (gm.language == 3)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
                    else if (gm.language == 4)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
                    else if (gm.language == 5)
                        newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
                    foreach (char c in newPhrase)
                    {
                        ui.dialoguePhrase.text += c;
                        yield return new WaitForSeconds(speakDelay);
                    }
                }
                else
                {
                    choosing = true;
                    string choiceText = "";
                    switch (interactable.weaponPickUp.weaponDataRandomier.statusEffect)
                    {
                        case 0: // poison
                            if (gm.language == 0)
                                choiceText = interactable.itemNames[0] + " will [POISON] you, if you'll pick it up. Take it?";
                            else if (gm.language == 1)
                                choiceText = interactable.itemNames[1] + " [ОТРАВИТ] тебя, если возьмешь его в руки. Взять?";
                            else if (gm.language == 2)
                                choiceText = interactable.itemNames[2] + " Te [ENVENENARA] si lo coges. Cogerlo?";
                            else if (gm.language == 3)
                                choiceText = interactable.itemNames[3] + " wird dich [vergiften], falls du es aufhebst. Aufheben?";
                            else if (gm.language == 4)
                                choiceText = interactable.itemNames[4] + " ti [AVVELENA], se lo tocchi. Lo prendi?";
                            else if (gm.language == 5)
                                choiceText = interactable.itemNames[5] + " irá [ENVENENAR] você, se você for pega-lo. Pegar?";
                            break;
                        
                        case 1: // fire
                            if (gm.language == 0)
                                choiceText = interactable.itemNames[0] + " will [FLAME] you, if you'll pick it up. Take it?";
                            else if (gm.language == 1)
                                choiceText = interactable.itemNames[1] + " [ВОСПЛАМЕНИТ] тебя, если возьмешь его в руки. Взять?";
                            else if (gm.language == 2)
                                choiceText = interactable.itemNames[2] + " Te [PONDRA EN LLAMAS] si lo coges. Cogerlo?";
                            else if (gm.language == 3)
                                choiceText = interactable.itemNames[3] + " wird dich [verbrenne], wenn du es aufghebst. Aufheben?";
                            else if (gm.language == 4)
                                choiceText = interactable.itemNames[4] + " ti [BRUCIA], se lo tocchi. Lo prendi?";
                            else if (gm.language == 5)
                                choiceText = interactable.itemNames[5] + " irá [INCINERAR] você, se você for pega-lo. Pegar?";
                            break;
                        
                        case 2: // bleed
                            if (gm.language == 0)
                                choiceText = interactable.itemNames[0] + " will make you [BLEED], if you'll pick it up. Take it?";
                            else if (gm.language == 1)
                                choiceText = interactable.itemNames[1] + " вызовет [КРОВОТЕЧЕНИЕ], если возьмешь его в руки. Взять?";
                            else if (gm.language == 2)
                                choiceText = interactable.itemNames[2] + " Te hara [SANGRAR] si lo coges. Cogerlo?";
                            else if (gm.language == 3)
                                choiceText = interactable.itemNames[3] + " wird dich [bluten lassen], wenn du es aufhebs. Aufheben?";
                            else if (gm.language == 4)
                                choiceText = interactable.itemNames[4] + " ti farà [SANGUINARE], se lo tocchi. Lo prendi?";
                            else if (gm.language == 5)
                                choiceText = interactable.itemNames[5] + " irá [SANGRAR] você, se você for pega-lo. Pegar?";
                            break;
                        
                        case 3: // rust
                            if (gm.language == 0)
                                choiceText = interactable.itemNames[0] + " will cover your arms with [RUST], if you'll pick it up. Take it?";
                            else if (gm.language == 1)
                                choiceText = interactable.itemNames[1] + " покроет твои руки [РЖАВЧИНОЙ], если возьмешь его в руки. Взять?";
                            else if (gm.language == 2)
                                choiceText = interactable.itemNames[2] + " Cubrira tus brazos con [OXIDO] si lo coges. Cogerlo?";
                            else if (gm.language == 3)
                                choiceText = interactable.itemNames[3] + " wird deine Waffen mit [Rost] bedecken, wenn du es aufhebst. Aufheben?";
                            else if (gm.language == 4)
                                choiceText = interactable.itemNames[4] + " ricoprirà le tue mani di [RUGGINE], se lo tocchi. Lo prendi?";
                            else if (gm.language == 5)
                                choiceText = interactable.itemNames[5] + " cobrirá seus braços com [OXIDO], se você conseguir. Pegar?";
                            break;
                        
                        case 4: // regen
                            if (gm.language == 0)
                                choiceText = interactable.itemNames[0] + " wants "+ Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " gold for joining you. Pay him?";
                            else if (gm.language == 1)
                                choiceText = interactable.itemNames[1] + " хочет "+ Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " золота, прежде чем присоединиться к тебе. Заплатить?";
                            else if (gm.language == 2)
                                choiceText = interactable.itemNames[2] + " Quiere " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " para unirse. Pagarle?";
                            else if (gm.language == 3)
                                choiceText = interactable.itemNames[3] + " will " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " Gold, damit er dir beitritt. Zahle Ihn?";
                            else if (gm.language == 4)
                                choiceText = interactable.itemNames[4] + " vuole " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " ore per unirsi a te. Lo paghi?";
                            else if (gm.language == 5)
                                choiceText = interactable.itemNames[5] + " quer " + Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) + " ouro por se juntar a você. Pagar?";
                            break;
                        
                        case 5: // gold hunger
                            if (gm.language == 0)
                                choiceText = interactable.itemNames[0] + " will make you [HUNGER FOR GOLD], if you'll pick it up. Take it?";
                            else if (gm.language == 1)
                                choiceText = interactable.itemNames[1] + " вызовет [ГОЛОД ПО ЗОЛОТУ], если возьмешь его в руки. Взять?";
                            else if (gm.language == 2)
                                choiceText = interactable.itemNames[2] + " Te hara [AUROFAGIA] si lo coges. Cogerlo?";
                            else if (gm.language == 3)
                                choiceText = interactable.itemNames[3] + " macht dich [GOLDHUNGRIG], wenn du es aufhebst. Aufheben?";
                            else if (gm.language == 4)
                                choiceText = interactable.itemNames[4] + " ti darà [FAME DI ORO], se lo tocchi. Lo prendi?";
                            else if (gm.language == 5)
                                choiceText = interactable.itemNames[5] + " vai fazer você ter [FOME POR OURO], se você quiser. Pegar?";
                            break;
                    }


                    newPhrase = choiceText;
                    
                    
                    newPhrasePhrase = String.Empty;
                    if (interactable.weaponPickUp && interactable.weaponPickUp.weaponDataRandomier)
                    {
                        newPhrasePhrase = Translator.TranslateText(interactable.weaponPickUp.weaponDataRandomier.weaponData.randomPhrases[Random.Range(0, interactable.weaponPickUp.weaponDataRandomier.weaponData.randomPhrases.Count)].text[gm.language]);
                    }
                    else
                    {
                        if (gm.language == 0)
                            newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                        else if (gm.language == 1)
                            newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                        else if (gm.language == 2)
                            newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                        else if (gm.language == 3)
                            newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                        else if (gm.language == 4)
                            newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                        else if (gm.language == 5)
                            newPhrasePhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);
                    }

                    ui.dialoguePhrase.text = newPhrasePhrase;
                    ui.dialogueChoice.text = newPhrase;
                    
                    currentLine++;
                }
            }
            else
            {
                if (gm.language == 0)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                else if (gm.language == 1)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                else if (gm.language == 2)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                else if (gm.language == 3)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                else if (gm.language == 4)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                else if (gm.language == 5)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);

                foreach (char c in newPhrase)
                {
                    ui.dialoguePhrase.text += c;
                    yield return new WaitForSeconds(speakDelay);
                }
                //ui.dialogueAnim.SetBool("Active", false);
            }

            if (dialogues[currentDialog].questToStart > 0)
            {
                QuestManager.instance.StartQuest(dialogues[currentDialog].questToStart);
            }
            if (dialogues[currentDialog].questToComplete > 0)
            {
                QuestManager.instance.CompleteQuest(dialogues[currentDialog].questToComplete);
            }
            
            if (!choosing && currentDialog + 1 < dialogues.Count)
            {
                currentDialog++;
            }
            currentLine = 0;
            
        }
        else // LINES IN DIALOGUE
        {
            if (interactable.weaponPickUp && interactable.weaponPickUp.weaponDataRandomier)
            {
                newPhrase = Translator.TranslateText(interactable.weaponPickUp.weaponDataRandomier.weaponData.randomPhrases[Random.Range(0, interactable.weaponPickUp.weaponDataRandomier.weaponData.randomPhrases.Count)].text[gm.language]);
            }
            else
            {
                if (gm.language == 0)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrases[currentLine]);
                else if (gm.language == 1)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesRu[currentLine]);
                else if (gm.language == 2)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesESP[currentLine]);
                else if (gm.language == 3)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesGER[currentLine]);
                else if (gm.language == 4)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesIT[currentLine]);
                else if (gm.language == 5)
                    newPhrase = Translator.TranslateText(dialogues[currentDialog].phrasesSPBR[currentLine]);
            }
            
            foreach (char c in newPhrase)
            {
                ui.dialoguePhrase.text += c;
                yield return new WaitForSeconds(speakDelay);
            }
            currentLine++;
        }

        canInteract = true;
        if (hideText != null)
            StopCoroutine(hideText);

        hideText = StartCoroutine(HideText());
        //Hide();
    }

    string newPhraseChoice = String.Empty;
    IEnumerator MakeChoice()
    {
        ui.dialoguePhrase.text = String.Empty;
        ui.dialogueChoice.text = String.Empty;
        
        if (shuffler) audioSource.clip = shuffler.GetVoiceClip();
        audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        audioSource.Play();
        
        if (Mathf.RoundToInt(il.gold) < Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) && dialogues[currentDialog].dialogueEvent != Dialogue.DialogueEvent.GivePlayerWeapon || 
         (Mathf.RoundToInt(il.gold) < Mathf.RoundToInt(dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion)) && dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.GivePlayerWeapon && interactable.weaponPickUp.weaponDataRandomier.statusEffect == 4) ||
         (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.UpgradeWeapon && il.activeWeapon == WeaponPickUp.Weapon.Null) ||
         (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.LoseAnEye && il.lostAnEye == 1) ||
         (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.Warm && (hc.statusEffects.Count <= 1 || !hc.statusEffects[1].effectActive)))
        // pay gold if weapon wants it
        {
            newPhrase = String.Empty;
            if (gm.language == 0)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhrase);
            else if (gm.language == 1)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseRu);
            else if (gm.language == 2)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseEsp);
            else if (gm.language == 3)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseGer);
            else if (gm.language == 4)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseIT);
            else if (gm.language == 5)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].doesntMetRequirementsPhraseSPBR);
            //ui.dialogueAnim.SetBool("Active", false);

            foreach (char c in newPhrase)
            {
                ui.dialoguePhrase.text += c;
                yield return new WaitForSeconds(speakDelay);
            }
            
            choosing = false;
            canInteract = true;
            
            if (hideText != null)
                StopCoroutine(hideText);

            hideText = StartCoroutine(HideText());
            //Hide();
        }
        else
        {
            canInteract = false;
            newPhrase = String.Empty;
            newPhraseChoice = String.Empty;
            ui.dialogueSpeakerName.text = hc.names[gm.language];
            ui.dialoguePhrase.text = String.Empty;
            ui.dialogueChoice.text = String.Empty;
            currentEventRepeat++;
            
            if (dialogues[currentDialog].dialogueEvent != Dialogue.DialogueEvent.GivePlayerWeapon ||
                (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.GivePlayerWeapon &&
                 interactable.weaponPickUp.weaponDataRandomier.statusEffect == 4))
            {
                float goldToSpend = dialogues[currentDialog].eventCost * inLoveScaler * Mathf.FloorToInt(il.badReputaion);
                il.gold -= goldToSpend;
                il.gold = Mathf.RoundToInt(il.gold);
                if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.BuySkill)
                {
                    il.goldSpentOnSon += Mathf.RoundToInt(goldToSpend);
                    if (!gm.demo && il.goldSpentOnSon >= 500)
                        SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_24");
                }
                PlayerSkillsController.instance.PlayerLoseGold(dialogues[currentDialog].eventCost * inLoveScaler);
                ui.UpdateGold(il.gold);
            }/*
            else if (dialogues[currentDialog].dialogueEvent == Dialogue.DialogueEvent.ReduceBadRep)
            {
                PlayerSkillsController.instance.PlayerLoseGold(il.gold);
                il.gold = 0;
                ui.UpdateGold(il.gold);
            }*/

            switch (dialogues[currentDialog].dialogueEvent)
            {
                case Dialogue.DialogueEvent.FixWeapon:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("Weapon repaired.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Оружие починено.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Arma reparada.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Waffen repariert.");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Arma riparata.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Arma reparada.");

                    pm.hc.wc.activeWeapon.FixWeapon();
                    il.SaveWeapons();
                    ui.UpdateWeapons();
                    break;

                case Dialogue.DialogueEvent.Heal:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("Your wounds healed. Also your hands smell bad now.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Твои раны затянулись. Но руки теперь плохо пахнут.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Tus heridas se han curado. Ademas tus manos huelen mal ahora.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Deine Wunden verheilten. Außerdem riechen deine Hände schlecht.");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Le tue ferite sono curate. Inoltre le tue mani adesso puzzano.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Suas feridas sararam. E suas mãos também cheiram mal agora.");
                    pm.hc.Heal(-1);
                    if (!gm.demo)
                        SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_10");
                    ui.UpdateWeapons();
                    break;
                
                case Dialogue.DialogueEvent.BuySkill:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("Look under your feet.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Проверь под своей ступней.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Mira a tus pies.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Schau unter deine Füße");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Guarda sotto i tuoi piedi.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Olhe sob seus pés.");
                    Instantiate(mementoPrefab, pm.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                    break;
                
                case Dialogue.DialogueEvent.GiveCarl:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("Glutton ate a turtle named Carl");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Обжора сожрал черепашку по имени Карл");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Glutton se comió una tortuga llamada Carl");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Vielfraß aß eine Schildkröte namens Carl");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Glutton ha mangiato una tartaruga di nome Carl");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Glutton comeu uma tartaruga chamada Carl");
                    
                    il.savedQuestItems.Remove(5);
                    gm.qm.CompleteQuest(7);
                    break;
                
                case Dialogue.DialogueEvent.SetNpcInLove:
                    // specific for ladyshoe
                    hc.transform.parent = null;
                    hc.mobPartsController.agent.enabled = false;
                    ///////
                    
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("Ladyshoe joined party!");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Туфелька присоединилась к пати!");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Ladyshoe joined party!");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Ladyshoe joined party!");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Ladyshoe joined party!");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Ladyshoe joined party!");
                    
                    hc.mobPartsController.agent.enabled = true;
                    hc.inLove = true;
                    QuestManager.instance.CompleteQuest(1);
                    break;
                
                
                case Dialogue.DialogueEvent.Pray:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You silently chanted a prayer. You don't know if someone heard it");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты молча пропел молитву. Ты не знаешь, услышал ли тебя кто-либо");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("You silently chanted a prayer. You don't know if someone heard it");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("You silently chanted a prayer. You don't know if someone heard it");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("You silently chanted a prayer. You don't know if someone heard it");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("You silently chanted a prayer. You don't know if someone heard it");
                    
                    PlayerSkillsController.instance.KissHer();
                    break;
                
                case Dialogue.DialogueEvent.AgreeSetObjectActiveAndHideNpc:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You agreed");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты согласился");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("You agreed");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("You agreed");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("You agreed");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("You agreed");

                    if (gameObjectActivateOnAgree)
                        gameObjectActivateOnAgree.SetActive(true);
                    if (interactableToInteractOnDialogueAction)
                        interactableToInteractOnDialogueAction.Interact(false);
                    gameObject.SetActive(false);
                    break;
                
                case Dialogue.DialogueEvent.DisarmTrap:
                    
                    var chance = 0.35f;
                    
                    if (PlayerSkillsController.instance.thirtyFingers)
                        chance = 0.75f;
                    
                    if (Random.value <= chance)
                    {
                        if(gm.language == 0)
                            newPhraseChoice = Translator.TranslateText("You disarmed a meat trap");
                        else if (gm.language == 1)
                            newPhraseChoice = Translator.TranslateText("Ты обезвредил мясную ловушку");
                        else if (gm.language == 2)
                            newPhraseChoice = Translator.TranslateText("Has desarmado una trampa de carne");
                        else if (gm.language == 3)
                            newPhraseChoice = Translator.TranslateText("Du hast eine Falle entschärft");
                        else if (gm.language == 4)
                            newPhraseChoice = Translator.TranslateText("Hai disinnescato una trappola di carne.");
                        else if (gm.language == 5)
                            newPhraseChoice = Translator.TranslateText("Você desarmou uma armadilha de carne");

                        hc.meatTrap.Disarm();
                    }
                    else
                    {
                        if(gm.language == 0)
                            newPhraseChoice = Translator.TranslateText("You accidentally activated a meat trap");
                        else if (gm.language == 1)
                            newPhraseChoice = Translator.TranslateText("Ты случайно активировал мясную ловушку");
                        else if (gm.language == 2)
                            newPhraseChoice = Translator.TranslateText("Has activado una trampa de carne por accidente");
                        else if (gm.language == 3)
                            newPhraseChoice = Translator.TranslateText("Du hast aus versehen eine Falle aktiviert");
                        else if (gm.language == 4)
                            newPhraseChoice = Translator.TranslateText("Hai attivato accidentalmente una trappola di carne");
                        else if (gm.language == 5)
                            newPhraseChoice = Translator.TranslateText("Você ativou acidentalmente uma armadilha de carne");

                        hc.meatTrap.Activate();
                    }
                    break;
                
                case Dialogue.DialogueEvent.BuyTool:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You bought ");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты купил ");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Has comprado ");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast gekauft ");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Hai comprato ");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você comprou ");
                    var newTool = Instantiate(toolsPool[Random.Range(0, toolsPool.Count)], spawnTransform.position, spawnTransform.rotation);
                    newPhraseChoice += newTool.itemNames[gm.language];
                    newTool.Interact(true);
                    break;
                
                case Dialogue.DialogueEvent.BuyKey:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("Here's your key, friend.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Вот твой ключ, друг.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Aqui esta tu llave, amigo.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Hier ist dein Schlüssel, Freund.");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Ecco la tua chiave, amico.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Aqui está sua chave, amigo.");
                    keySold = true;
                    var newKey = Instantiate(toolsPool[0], spawnTransform.position, spawnTransform.rotation);
                    newKey.Interact(false);

                    if (!gm.demo)
                    {
                        if (hc.eyeEater)
                        {
                            SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_28");
                        }
                        else 
                            SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_27");   
                    }
                    
                    break;
                
                case Dialogue.DialogueEvent.MeatHole:
                    if (interactable.itemInsideMeatHole != null && interactable.itemInsideMeatHole.gameObject.activeInHierarchy)
                    {
                        if(gm.language == 0)
                            newPhraseChoice = Translator.TranslateText("You shove your hand into the meat hole. You find something interesting.");
                        else if (gm.language == 1)
                            newPhraseChoice = Translator.TranslateText("Ты засунул руку в мясную дыру. Ты нащупал что-то интересное.");
                        else if (gm.language == 2)
                            newPhraseChoice = Translator.TranslateText("Metes tu mano en el Agujero de Carne. Encuentras algo interesante.");
                        else if (gm.language == 3)
                            newPhraseChoice = Translator.TranslateText("Sie schieben Ihre Hand in ein Fleischloch. Du hast etwas Interessantes gefunden.");
                        else if (gm.language == 4)
                            newPhraseChoice = Translator.TranslateText("Infili la tua mano nel foro di carne. Trovi qualcosa di interessante.");
                        else if (gm.language == 5)
                            newPhraseChoice = Translator.TranslateText("Você enfia a mão no buraco da carne. Você encontrou algo interessante.");

                        interactable.itemInsideMeatHole.InteractThroughMeathole();
                    }
                    else
                    {
                        if(gm.language == 0)
                            newPhraseChoice = Translator.TranslateText("You shove your hand into the meat hole. Something bites your finger off!");
                        else if (gm.language == 1)
                            newPhraseChoice = Translator.TranslateText("Ты засунул руку в мясную дыру. Кто-то откусил тебе палец.");
                        else if (gm.language == 2)
                            newPhraseChoice = Translator.TranslateText("Empujas tu mano en un agujero de carne. Alguien te muerde el dedo.");
                        else if (gm.language == 3)
                            newPhraseChoice = Translator.TranslateText("Sie schieben Ihre Hand in ein Fleischloch. Jemand beißt dir den Finger ab.");
                        else if (gm.language == 4)
                            newPhraseChoice = Translator.TranslateText("Infili la tua mano nel foro di carne. Qualcuno ti stacca il dito a morsi.");
                        else if (gm.language == 5)
                            newPhraseChoice = Translator.TranslateText("Você enfia a mão no buraco da carne. Alguém arrancou seu dedo.");

                        pm.hc.Damage(pm.hc.health / 2, transform.position, pm.mouseLook.transform.position,
                        null, hc.playerDamagedMessage[gm.language], false, null, null, null, false);
                    }
                    break;
                
                
                case Dialogue.DialogueEvent.UpgradeWeapon:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("Your weapon has changed.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Твое оружие изменилось.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Tu arma ha canviado.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Deine Waffe hat sich verändert.");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("La tua arma è cambiata.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Sua arma mudou.");
                    UpgradeWeapon();
                    break;

                
                case Dialogue.DialogueEvent.LoseAnEye:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You gave her your eye");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты отдал ей свой глаз");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Le has dado a Ella un ojo");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du gabst Ih dein Auge");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Le hai dato il tuo occhio");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você olhou para ela");
                    ui.LostAnEye();
                    il.LostAnEye();
                    if (!gm.demo)
                        SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_16");
                    pm.hc.Damage(1, pm.cameraAnimator.transform.position, transform.position, null, null, false, null, null, null, false);
                    break;
                
                case Dialogue.DialogueEvent.OneLevelBack:
                    GutProgressionManager gpm = GutProgressionManager.instance;
                    if (gpm.playerFloor > 0)
                        gpm.playerFloor--;    

                    var newFloor = gpm.playerFloor;

                    if (gpm.playerFloor < 4)
                    {
                        gpm.currentLevelDifficulty = 0;
                        gpm.bossFloor = 3;
                    }
                    else if (gpm.playerFloor < 7)
                    {
                        gpm.currentLevelDifficulty = 1;
                        gpm.bossFloor = 6;
                    }
                    else if (gpm.playerFloor < 10)
                    {
                        gpm.currentLevelDifficulty = 2;
                        gpm.bossFloor = 9;
                    }
                    else if (gpm.playerFloor < 13)
                    {
                        gpm.currentLevelDifficulty = 3;
                        gpm.bossFloor = 12;   
                    }
                    else if (gpm.playerFloor < 16)
                    {
                        gpm.currentLevelDifficulty = 4;
                        gpm.bossFloor = 15;   
                    }
                    else if (gpm.playerFloor < 19)
                    {
                        gpm.currentLevelDifficulty = 5;
                        gpm.bossFloor = 18;   
                    }
                    
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("Elevator will go to the Floor -" + newFloor + " now.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Теперь лифт отправится на Этаж -" + newFloor + ".");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("El ascensor ira ahora al Piso -" + newFloor + ".");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Der Aufzug wird nun zur Etage - " + newFloor + " gehen.");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("L'ascensore andrà al Piano - " + newFloor + " ora.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Elevador irá para o andar - " + newFloor + " agora.");
                    break;
                
                case Dialogue.DialogueEvent.JoinToxicCult:
                    PlayerSkillsController.instance.SetCult(PlayerSkillsController.Cult.poisonCult);
                    il.AddToBadReputation(1f);
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You joined the cult of the Toxic Grave. The Gut is unhappy with you.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты вступил в культ Токсичной Могилы. Кишка злится на тебя.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Te uniste al culto de la Tumba Tóxica. The Gut no está contento contigo.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Sie haben sich dem Kult des Giftgrabes angeschlossen. Der Darm ist unzufrieden mit dir.");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Mi sono unito al culto della Tomba Tossica. L'anziano dice che il VELENO è mio amico ora.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Entrei para o culto da Sepultura Tóxica. O ancião disse que o veneno é meu amigo agora.");
                    break;
                case Dialogue.DialogueEvent.JoinFlamesCult:
                    PlayerSkillsController.instance.SetCult(PlayerSkillsController.Cult.fireCult);
                    il.AddToBadReputation(1f);
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You joined the cult of the Baby of Flames. The Gut is unhappy with you.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты вступил в культ Младенца Огня. Кишка злится на тебя.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Te uniste al culto del Bebé de las Llamas. The Gut no está contento contigo.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast dich dem Kult des Flammenbabys angeschlossen. Der Darm ist unzufrieden mit dir.");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Mi sono unito al culto del Bambino delle Fiamme. L'anziano dice che il FUCO è mio amico ora.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Entrei para o culto do Bebê das Chamas. O ancião disse que FOGO é agora meu amigo");
                    break;
                case Dialogue.DialogueEvent.JoinBloodCult:
                    PlayerSkillsController.instance.SetCult(PlayerSkillsController.Cult.bleedingCult);
                    il.AddToBadReputation(1f);
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You joined the cult of the Bloodshed Swallowers. The Gut is unhappy with you.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты вступил в культ Кровопролитных Глотателей. Кишка злится на тебя.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Te uniste al culto de los Swallowers Bloodshed. The Gut no está contento contigo.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast dich dem Kult der Bloodshed Swallowers angeschlossen. Der Darm ist unzufrieden mit dir.");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Mi sono unito al culto degli Ingoiatori di Sangue. L'anziano dice che il SANGUE è mio amico ora.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Entrei para o culto das andorinhas derramamento de sangue. O ancião disse que SANGUE agora é meu amigo");
                    break;
                case Dialogue.DialogueEvent.JoinGoldCult:
                    PlayerSkillsController.instance.SetCult(PlayerSkillsController.Cult.goldCult);
                    il.AddToBadReputation(1f);
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You joined the cult of the Golden Phallus. The Gut is unhappy with you.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты вступил в культ Золотого Фаллоса. Кишка злится на тебя.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Te uniste al culto del falo dorado. The Gut no está contento contigo.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Sie haben sich dem Kult des Goldenen Phallus angeschlossen. Der Darm ist unzufrieden mit dir.");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Mi sono unito al culto del Fallo Aureo. L'anziano dice che diventerò ricco");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Entrei para o culto do Falo Dourado. O Élder disse que eu ficarei rico");
                    break;
                
                case Dialogue.DialogueEvent.ReduceBadRep:
                    
                    il.AddToBadReputation(-1f);
                    il.LoseGold(il.gold / 2);
                    
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You helped somebody. The Sea likes this");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты помог кому-то. Морю это нравится");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("You helped somebody. The Sea likes this");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("You helped somebody. The Sea likes this");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("You helped somebody. The Sea likes this");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("You helped somebody. The Sea likes this");
                    break;
                
                case Dialogue.DialogueEvent.KissHer:
                    
                    //random kiss effect
                    PlayerSkillsController.instance.KissHer();
                    
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You kissed She. Something is gonna happen...");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты поцеловал Ее. Сейчас что-то случится...");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("La besaste. Algo va a pasar...");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast sie geküsst. Es wird etwas passieren...");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("l'hai baciata. Adesso succederà qualcosa...");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("você a beijou. Algo vai acontecer agora...");
                    break;
                
                case Dialogue.DialogueEvent.BuyAmmo:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You bought ");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты купил ");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Has comprado ");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast gekauft ");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Hai comprato ");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você comprou ");

                    newPhraseChoice += SpawnAmmo();
                    break;
                
                case Dialogue.DialogueEvent.BuyGoldenKey:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You bought Golden Key");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты купил Золотой Ключ");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Compraste Golden Key");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast Golden Key gekauft");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Hai comprato la chiave d'oro");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você comprou a Golden Key");
                    
                    var newGoldenKey = Instantiate(toolsPool[0], spawnTransform.position, spawnTransform.rotation);
                    newGoldenKey.Interact(false);
                    break;
                
                case Dialogue.DialogueEvent.Taxi:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You will be served by a meat taxi");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Тебя обслужит мясное такси");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Serás atendido por un taxi de carne");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Sie werden von einem Fleischtaxi bedient");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Sarai servito da un taxi di carne");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você será servido por um táxi de carnes");

                    if (parentObject == null)
                        parentObject = hc.transform.parent.parent.parent.gameObject;
                    StartCoroutine(PlayerCheckpointsController.instance.TaxiToNeededCheckpoint(parentObject));
                    break;
                
                case Dialogue.DialogueEvent.GivePlayerWeapon:

                    switch (interactable.weaponPickUp.weaponDataRandomier.statusEffect)
                    {
                        case 0:
                            pm.hc.InstantPoison();
                            break;
                        
                        case 1:
                            pm.hc.StartFire();
                            break;
                        
                        case 2:
                            pm.hc.AddBleed(100);
                            break;
                        
                        case 3:
                            pm.hc.AddRust(100);
                            break;
                        
                        case 4:
                            pm.hc.InstantRegen();
                            break;
                        
                        case 5:
                            pm.hc.AddGoldHunger(100);
                            break;
                    }
                    
                    interactable.weaponPickUp.weaponDataRandomier.NpcPaid();
                    interactable.PickWeapon();
                    break;
                
                case Dialogue.DialogueEvent.Warm:
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("Burning hands warmed you.");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Тебя согрели горящие руки.");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Las manos ardientes te calentaron.");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Brennende Hände wärmten dich");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Le mani bruciate ti hanno riscaldato.");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Mãos ardentes o aqueceram.");

                    if (pm.hc.wc.activeWeapon && pm.hc.wc.activeWeapon.weapon == WeaponPickUp.Weapon.Torch)
                    {
                        pm.hc.wc.activeWeapon.FixWeapon();
                        il.SaveWeapons();
                        ui.UpdateWeapons();
                    }
                    pm.hc.Anticold();
                    break;
                
                case Dialogue.DialogueEvent.KillQuest:
                    GenerateQuest(QuestBlockingNpcProgress.QuestType.Kill);
                    
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You agreed to kill [" + blockingQuest.targetName+"]");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты согласился убить [" + blockingQuest.targetName+"]");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Has acordado a matar a [" + blockingQuest.targetName + "]");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast zugestimmt [" + blockingQuest.targetName + "] zu töten");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Hai accettato di uccidere [" + blockingQuest.targetName + "] ");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você concordou em matar [" + blockingQuest.targetName + "] ");

                    break;
                
                case Dialogue.DialogueEvent.ItemQuest:
                    
                    GenerateQuest(QuestBlockingNpcProgress.QuestType.Item);
                    
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You agreed to bring [" + blockingQuest.targetName+"]");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты согласился принести [" + blockingQuest.targetName+"]");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Has acordado a traer un [" + blockingQuest.targetName + "]");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast zugestimmt [" + blockingQuest.targetName + "] zu bringen");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Hai accettato di portare [" + blockingQuest.targetName + "] ");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você concordou em trazer [" + blockingQuest.targetName + "] ");
                    break;
                case Dialogue.DialogueEvent.WeaponQuest:
                    
                    GenerateQuest(QuestBlockingNpcProgress.QuestType.Weapon);
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You agreed to bring  [" + blockingQuest.targetName+"]");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты согласился принести  [" + blockingQuest.targetName+"]");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Has acordado a traer un [" + blockingQuest.targetName + "]");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast zugestimmt [" + blockingQuest.targetName + "] zu bringen");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Hai accettato di portare [" + blockingQuest.targetName + "] ");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você concordou em trazer [" + blockingQuest.targetName + "] ");

                    break;
                case Dialogue.DialogueEvent.PoisonQuest:
                    
                    GenerateQuest(QuestBlockingNpcProgress.QuestType.Poison);
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You agreed to poison  [" + blockingQuest.targetName+"]");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты согласился отравить  [" + blockingQuest.targetName+"]");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Has acordado envenenar a  [" + blockingQuest.targetName + "]");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast zugestimmt  [" + blockingQuest.targetName + "] zu vergiften");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Hai accettato di avvelenare  [" + blockingQuest.targetName + "] ");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você concordou em envenenar  [" + blockingQuest.targetName + "] ");

                    break;
                
                case Dialogue.DialogueEvent.FireQuest:
                    
                    GenerateQuest(QuestBlockingNpcProgress.QuestType.Fire);
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You agreed to set  [" + blockingQuest.targetName + "] on fire");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты согласился поджарить [" + blockingQuest.targetName+"]");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Has acordado en poner a [" + blockingQuest.targetName + "] en llamas");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast aktzeptiert [" + blockingQuest.targetName + "] anzuzünden");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Hai accettato di dare fuoco a [" + blockingQuest.targetName + "] ");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você concordou em colocar [" + blockingQuest.targetName + "] em chamas");

                    break;
                
                case Dialogue.DialogueEvent.BleedQuest:
                    
                    GenerateQuest(QuestBlockingNpcProgress.QuestType.Bleed);
                    
                    if(gm.language == 0)
                        newPhraseChoice = Translator.TranslateText("You agreed to cut  [" + blockingQuest.targetName + "] to bleeding");
                    else if (gm.language == 1)
                        newPhraseChoice = Translator.TranslateText("Ты согласился изрезать  [" + blockingQuest.targetName + "] до кровотечения");
                    else if (gm.language == 2)
                        newPhraseChoice = Translator.TranslateText("Has acordado en cortar a  [" + blockingQuest.targetName + "] hasta que sangre");
                    else if (gm.language == 3)
                        newPhraseChoice = Translator.TranslateText("Du hast aktzeptiert  [" + blockingQuest.targetName + "] zu schneiden bis er blutet");
                    else if (gm.language == 4)
                        newPhraseChoice = Translator.TranslateText("Hai accettato di tagliare  [" + blockingQuest.targetName + "] fino a farlo sanguinare");
                    else if (gm.language == 5)
                        newPhraseChoice = Translator.TranslateText("Você concordou em cortar  [" + blockingQuest.targetName + "] até sangrar");

                    break;
            }

            if (gm.language == 0)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].eventComplete);
            else if (gm.language == 1)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].eventCompleteRu);
            else if (gm.language == 2)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].eventCompleteEsp);
            else if (gm.language == 3)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].eventCompleteGer);
            else if (gm.language == 4)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].eventCompleteIT);
            else if (gm.language == 5)
                newPhrase = Translator.TranslateText(dialogues[currentDialog].eventCompleteSPBR);

            //print(newPhrase);

            ui.dialoguePhrase.text = newPhrase;
            ui.dialogueChoice.text = newPhraseChoice;
            
            choosing = false;
            
            ui.HideDialogue(3);
            
            canInteract = true;
            if (hideText != null)
                StopCoroutine(hideText);

            if (gameObject.activeInHierarchy)
                hideText = StartCoroutine(HideText());
        }
    }

    void GivePlayerQuestReward()
    {
        var newTool = Instantiate(toolsPool[Random.Range(0, toolsPool.Count)], spawnTransform.position, spawnTransform.rotation);
        newTool.Interact(true);
    }

    void UpgradeWeapon()
    {
        wc.activeWeapon.dataRandomizer.RandomStatusEffect(false);
        ui.UpdateWeapons();
    }
    
    string SpawnAmmo()
    {
        // 0 - pistol, 1 - revolver, 2 - shotgun, 3 - tommy, 4 - old pistol
        int ammo = -1;
        switch (il.activeWeapon)
        {
            case WeaponPickUp.Weapon.Pistol:
                ammo = 0;
                break;
            case WeaponPickUp.Weapon.Revolver:
                ammo = 1;
                break;
            case WeaponPickUp.Weapon.Shotgun:
                ammo = 2;
                break;
            case WeaponPickUp.Weapon.TommyGun:
                ammo = 3;
                break;
            case WeaponPickUp.Weapon.OldPistol:
                ammo = 4;
                break;
        }

        if (ammo == -1)
        {
            switch (il.secondWeapon)
            {
                case WeaponPickUp.Weapon.Pistol:
                    ammo = 0;
                    break;
                case WeaponPickUp.Weapon.Revolver:
                    ammo = 1;
                    break;
                case WeaponPickUp.Weapon.Shotgun:
                    ammo = 2;
                    break;
                case WeaponPickUp.Weapon.TommyGun:
                    ammo = 3;
                    break;
                case WeaponPickUp.Weapon.OldPistol:
                    ammo = 4;
                    break;
            }
        }

        if (ammo == -1)
            ammo = Random.Range(0, 4);
        
        var newAmmo = Instantiate(ammoPool[ammo], spawnTransform.position, spawnTransform.rotation);
        newAmmo.Interact(false);
        return newAmmo.itemNames[gm.language];
    }

    IEnumerator HideText()
    {
        bool hidden = false;
        while (!hidden)
        {
            yield return new WaitForSeconds(1f);

            if (snout && pm.inTransport) continue;
            
            if (Mathf.RoundToInt(pm._x) != 0 || Mathf.RoundToInt(pm._z) != 0 || Vector3.Distance(transform.position, pm.transform.position) > distanceToCloseDialogue)
            {
                Hide();
                hidden = true;
            }
        }
    }

    public void Hide()
    {
        currentLine = 0;
        ui.dialoguePhrase.text = "";
        ui.dialogueChoice.text = "";
        choosing = false;
        ui.dialogueAnim.SetBool("Active", false);

        HideDialogue();
    }


    void GenerateQuest(QuestBlockingNpcProgress.QuestType type)
    {
        print("generate quest");
        blockingQuest  = new QuestBlockingNpcProgress();
        blockingQuest.npc = hc;
        switch (type)
        {
            case QuestBlockingNpcProgress.QuestType.Kill:
                List<HealthController> unitsOnLevel = new List<HealthController>(gm.units);
                for (int i = unitsOnLevel.Count - 1; i >= 0; i--)
                {
                    if (unitsOnLevel[i].door != null || unitsOnLevel[i] == hc || unitsOnLevel[i] == gm.player || unitsOnLevel[i].gameObject.activeInHierarchy == false)
                    {
                        unitsOnLevel.RemoveAt(i);
                    }
                }

                if (unitsOnLevel.Count <= 0 && gm.sc.mobsInGame.Count > 0)
                {
                    unitsOnLevel.Add(gm.sc.mobsInGame[Random.Range(0, gm.sc.mobsInGame.Count)]);
                }
                else if (gm.sc.mobsInGameStatic.Count > 0)
                {
                    unitsOnLevel.Add(gm.sc.mobsInGameStatic[Random.Range(0, gm.sc.mobsInGameStatic.Count)]);
                }
                else
                unitsOnLevel.Add(hc);
                
                blockingQuest.killTarget = unitsOnLevel[Random.Range(0, unitsOnLevel.Count)];
                blockingQuest.targetName = blockingQuest.killTarget.names[gm.language];

                blockingQuest.killTarget.SpawnDynamicMapMark(0);
                
                blockingQuest.questsDidntFinishedLine.Add("Maps will show you where " + blockingQuest.killTarget.names[gm.language] + " hides. Kill it!");   
                blockingQuest.questsDidntFinishedLine.Add("Карты покажут, где прячется " + blockingQuest.killTarget.names[gm.language] + ". Убей его!");   
                blockingQuest.questsDidntFinishedLine.Add("Los mapas te señalaran donde se esconde " + blockingQuest.killTarget.names[gm.language] + ". Mátalo!");   
                blockingQuest.questsDidntFinishedLine.Add("Die Karten zeigen dir nun wo sich " + blockingQuest.killTarget.names[gm.language] + " versteckt. Töte es!");
                blockingQuest.questsDidntFinishedLine.Add("Le mappe ti mostreranno dove " + blockingQuest.killTarget.names[gm.language] + " si nasconde. Uccidilo!");
                blockingQuest.questsDidntFinishedLine.Add("Os mapas mostra para onde " + blockingQuest.killTarget.names[gm.language] + " se esconde. Mate ele!");

                blockingQuest.questsFinishedLine.Add("Did you kill "+ blockingQuest.killTarget.damagedByPlayerMessage[gm.language] + "? Great! We can talk now");   
                blockingQuest.questsFinishedLine.Add("Ты убил "+ blockingQuest.killTarget.damagedByPlayerMessage[gm.language] + "? Спасибо! Теперь можем поговорить");   
                blockingQuest.questsFinishedLine.Add("Has matado " + blockingQuest.killTarget.damagedByPlayerMessage[gm.language] + "? Perfecto! Ahora podemos hablar.");
                blockingQuest.questsFinishedLine.Add("Hast du " + blockingQuest.killTarget.damagedByPlayerMessage[gm.language] + "getötet? Gut, dann können wir uns jetzt unterhalten");
                blockingQuest.questsFinishedLine.Add("Hai ucciso " + blockingQuest.killTarget.damagedByPlayerMessage[gm.language] + "? Ottimo! Ora possiamo parlare.");
                blockingQuest.questsFinishedLine.Add("Você matou " + blockingQuest.killTarget.damagedByPlayerMessage[gm.language] + "? Ótimo! Podemos conversar agora.");

                blockingQuest.getRewardText.Add("Yea, it didn't like that");
                blockingQuest.getRewardText.Add("Ага, мясу это не понравилось");
                blockingQuest.getRewardText.Add("Sip, no me gustó");
                blockingQuest.getRewardText.Add("Ja, das mochte es nicht");
                blockingQuest.getRewardText.Add("Si, non gli è piaciuto");
                blockingQuest.getRewardText.Add("Sim, eu não gostei");

                blockingQuest.rewardTakenText.Add("Now we can talk");
                blockingQuest.rewardTakenText.Add("Теперь можем поговорить");
                blockingQuest.rewardTakenText.Add("Ahora podemos hablar");
                blockingQuest.rewardTakenText.Add("Jezt können wir reden");
                blockingQuest.rewardTakenText.Add("Ora possiamo parlare");
                blockingQuest.rewardTakenText.Add("Agora podemos conversar");
                break;
            
            case QuestBlockingNpcProgress.QuestType.Item:

                int r = Random.Range(0, il.savedTools.Count);
                blockingQuest.toolTarget = il.savedTools[r].type;
                blockingQuest.targetName = il.savedTools[r].info[gm.language];
                
                blockingQuest.questsDidntFinishedLine.Add(il.savedTools[r].info[gm.language] + " Find it for me and we will talk");   
                blockingQuest.questsDidntFinishedLine.Add(il.savedTools[r].info[gm.language] + " Найди это для меня и мы поговорим");   
                blockingQuest.questsDidntFinishedLine.Add(il.savedTools[r].info[gm.language] + " Encuentralo para mí y hablaremos");
                blockingQuest.questsDidntFinishedLine.Add(il.savedTools[r].info[gm.language] + " Finde es für mich und wir werden reden");
                blockingQuest.questsDidntFinishedLine.Add(il.savedTools[r].info[gm.language] + " Trovalo per me e parleremo.");
                blockingQuest.questsDidntFinishedLine.Add(il.savedTools[r].info[gm.language] + " Encontre para mim e conversaremos");

                blockingQuest.questsFinishedLine.Add(il.savedTools[r].info[gm.language] + " I can smell it in your pockets. Give me that and we will talk");
                blockingQuest.questsFinishedLine.Add(il.savedTools[r].info[gm.language] + " Я чувствую запах из твоих карманов. Отдай мне это и мы поговорим");
                blockingQuest.questsFinishedLine.Add(il.savedTools[r].info[gm.language] + " Lo puedo holer en tus bolsillos. Dámelo y podremos hablar.");
                blockingQuest.questsFinishedLine.Add(il.savedTools[r].info[gm.language] + " Ich rieche es in deinen Taschen. Gib es mir und wir können reden");
                blockingQuest.questsFinishedLine.Add(il.savedTools[r].info[gm.language] + " Ne sento l'odore dalle tue tue tasche. Dammelo e parleremo.");
                blockingQuest.questsFinishedLine.Add(il.savedTools[r].info[gm.language] + " Sinto o cheiro nos seus bolsos. Me dê isso e conversaremos.");

                blockingQuest.getRewardText.Add("Give away the item he wants?");
                blockingQuest.getRewardText.Add("Отдать предмет, который он хочет?");
                blockingQuest.getRewardText.Add("Darle el item que quiere?");
                blockingQuest.getRewardText.Add("Gib ihm den gewollten Gegenstand?");
                blockingQuest.getRewardText.Add("Dagli l'oggetto che vuole?");
                blockingQuest.getRewardText.Add("Dar a ele o item que ele quer?");

                blockingQuest.rewardTakenText.Add("You gave it");
                blockingQuest.rewardTakenText.Add("Ты отдал это");
                blockingQuest.rewardTakenText.Add("Se lo diste");
                blockingQuest.rewardTakenText.Add("Du gabst ihm");
                blockingQuest.rewardTakenText.Add("Gliel'hai dato");
                blockingQuest.rewardTakenText.Add("Você deu");
                break;
            
            case QuestBlockingNpcProgress.QuestType.Weapon:

                List<WeaponPickUp> tempInteractables = new List<WeaponPickUp>(il.weaponsOnLevel);
                string weaponName = "";
                if (tempInteractables.Count > 0)
                {
                    int rr = Random.Range(0, tempInteractables.Count);
                    blockingQuest.weaponTarget = tempInteractables[rr].weapon;
                    weaponName = tempInteractables[rr].interactable.itemNames[gm.language];
                }
                blockingQuest.targetName = weaponName;
                
                blockingQuest.questsDidntFinishedLine.Add("We will talk if you will bring that " + weaponName);   
                blockingQuest.questsDidntFinishedLine.Add("Мы поговорим если ты принесешь мне " + weaponName);   
                blockingQuest.questsDidntFinishedLine.Add("Hablaremos si me traes ese " + weaponName);
                blockingQuest.questsDidntFinishedLine.Add("Wir werden reden, wenn du mir " + weaponName + " bringst");
                blockingQuest.questsDidntFinishedLine.Add("Parleremo se porterai quel/quella " + weaponName + "");
                blockingQuest.questsDidntFinishedLine.Add("Vamos conversar se você vai trazer isso " + weaponName + " ");


                blockingQuest.questsFinishedLine.Add(weaponName + "! I can smell who it used to be. Give me that and we will talk");
                blockingQuest.questsFinishedLine.Add(weaponName + "! Я чувствую того, кем это было прежде. Отдай мне это и мы поговорим");
                blockingQuest.questsFinishedLine.Add(weaponName + "! Puedo oler lo que solía ser. Dámelo y hablaremos.");
                blockingQuest.questsFinishedLine.Add(weaponName + "! Ich kann riechen was es war. Gib es mir und wir können reden");
                blockingQuest.questsFinishedLine.Add(weaponName + "! Ne sento l'odore dalle tue tue tasche. Dammelo e parleremo.");
                blockingQuest.questsFinishedLine.Add(weaponName + "! Sinto o cheiro nos seus bolsos. Me dê isso e conversaremos.");

                blockingQuest.getRewardText.Add("Give away " + weaponName + "?");
                blockingQuest.getRewardText.Add("Отдать " + weaponName + "?");
                blockingQuest.getRewardText.Add("Regalar " + weaponName + "?");
                blockingQuest.getRewardText.Add("Gib weg " + weaponName + "?");
                blockingQuest.getRewardText.Add("Dai via " + weaponName + "?");
                blockingQuest.getRewardText.Add("Dar " + weaponName + "?");

                blockingQuest.rewardTakenText.Add("You gave " + weaponName);
                blockingQuest.rewardTakenText.Add("Ты отдал " + weaponName);
                blockingQuest.rewardTakenText.Add("Tu diste " + weaponName);
                blockingQuest.rewardTakenText.Add("Du gabst " + weaponName);
                blockingQuest.rewardTakenText.Add("Hai dato " + weaponName);
                blockingQuest.rewardTakenText.Add("Você deu " + weaponName);
                break;
            
            case QuestBlockingNpcProgress.QuestType.Poison:

                List<HealthController> unitsOnLevelForPoison = new List<HealthController>(gm.units);
                for (int i = unitsOnLevelForPoison.Count - 1; i >= 0; i--)
                {
                    if (unitsOnLevelForPoison[i].door || unitsOnLevelForPoison[i] == hc || unitsOnLevelForPoison[i] == gm.player || !unitsOnLevelForPoison[i].gameObject.activeInHierarchy || unitsOnLevelForPoison[i].statusEffects[0].effectImmune)
                    {
                        unitsOnLevelForPoison.RemoveAt(i);
                    }
                }

                if (unitsOnLevelForPoison.Count <= 0 && gm.sc.mobsInGame.Count > 0)
                {
                    unitsOnLevelForPoison.Add(gm.sc.mobsInGame[Random.Range(0, gm.sc.mobsInGame.Count)]);
                }
                else if (gm.sc.mobsInGameStatic.Count > 0)
                {
                    unitsOnLevelForPoison.Add(gm.sc.mobsInGameStatic[Random.Range(0, gm.sc.mobsInGameStatic.Count)]);
                }
                else
                    unitsOnLevelForPoison.Add(hc);
                
                blockingQuest.poisonTarget = unitsOnLevelForPoison[Random.Range(0, unitsOnLevelForPoison.Count)];
                blockingQuest.targetName = blockingQuest.poisonTarget.names[gm.language];
                blockingQuest.poisonTarget.SpawnDynamicMapMark(1);
                
                blockingQuest.questsDidntFinishedLine.Add("Maps will show you where " + blockingQuest.poisonTarget.names[gm.language] + " hides. Go poison him!");   
                blockingQuest.questsDidntFinishedLine.Add("Карты покажут, где прячется " + blockingQuest.poisonTarget.names[gm.language] + ". Иди и травани его!");   
                blockingQuest.questsDidntFinishedLine.Add("Los mapas te señalarán dónde se esconde " + blockingQuest.poisonTarget.names[gm.language] + " . Ve a envenenarle!");
                blockingQuest.questsDidntFinishedLine.Add("Die Karten werden dir zeigen wo sich " + blockingQuest.poisonTarget.names[gm.language] + " versteckt. Geh und vergiffte ihn!");
                blockingQuest.questsDidntFinishedLine.Add("Le mappe ti mostreranno dove " + blockingQuest.poisonTarget.names[gm.language] + " si nasconde. Vai ad avvelenarlo!");
                blockingQuest.questsDidntFinishedLine.Add("Os mapas mostra para onde " + blockingQuest.poisonTarget.names[gm.language] + " se esconde. Vá envenená-lo!");

                blockingQuest.questsFinishedLine.Add("Did you poison "+ blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "? Great! We can talk now");   
                blockingQuest.questsFinishedLine.Add("Ты отравил "+ blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "? Спасибо! Теперь можем поговорить");   
                blockingQuest.questsFinishedLine.Add("Has envenenado " + blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "? Perfecto! Podemos hablar ahora");
                blockingQuest.questsFinishedLine.Add("Hast du " + blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "vergiftet? Gut, dann können wir uns jetzt unterhalten");
                blockingQuest.questsFinishedLine.Add("Hai avvelenato " + blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "? Ottimo! Ora possiamo parlare");
                blockingQuest.questsFinishedLine.Add("Você envenenou " + blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "? Ótimo! podemos conversar agora");

                blockingQuest.questsFailedLine.Add("You killed "+ blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "??? Man, you're maniac! I didn't sign for this...");
                blockingQuest.questsFailedLine.Add("Ты убил "+ blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "??? Чувак, ты маньяк! Я на это не подписывался...");
                blockingQuest.questsFailedLine.Add("Has matado a " + blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "??? Tio, eres un maniático! No me inscribí para esto");
                blockingQuest.questsFailedLine.Add("Du hast " + blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "??? getötet? Man, du bist doch wahnsinnig! Dafür habe ich mich nicht angemeldet");
                blockingQuest.questsFailedLine.Add("Hai ucciso " + blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "? Bro, sei un maniaco! Non ho firmato per questo...");
                blockingQuest.questsFailedLine.Add("Você matou " + blockingQuest.poisonTarget.damagedByPlayerMessage[gm.language] + "? Cara, você é um maníaco! Eu não me inscrevi para isso ...");

                blockingQuest.getRewardText.Add("Yea, it didn't like that");
                blockingQuest.getRewardText.Add("Ага, мясу это не понравилось");
                blockingQuest.getRewardText.Add("Sip, no me gustó");
                blockingQuest.getRewardText.Add("Ja, es mochte es nicht");
                blockingQuest.getRewardText.Add("Si, non gli è piaciuto");
                blockingQuest.getRewardText.Add("Sim, eu não gostei");

                blockingQuest.rewardTakenText.Add("Now we can talk");
                blockingQuest.rewardTakenText.Add("Теперь можем поговорить");
                blockingQuest.rewardTakenText.Add("Ahora podemos hablar");
                blockingQuest.rewardTakenText.Add("Jezt können wir reden");
                blockingQuest.rewardTakenText.Add("Ora possiamo parlare");
                blockingQuest.rewardTakenText.Add("Agora podemos conversar");
                break;
            
            case QuestBlockingNpcProgress.QuestType.Fire:

                List<HealthController> unitsOnLevelForFire = new List<HealthController>(gm.units);
                for (int i = unitsOnLevelForFire.Count - 1; i >= 0; i--)
                {
                    if (unitsOnLevelForFire[i].door || unitsOnLevelForFire[i] == hc || unitsOnLevelForFire[i] == gm.player || 
                        !unitsOnLevelForFire[i].gameObject.activeInHierarchy || unitsOnLevelForFire[i].statusEffects[1].effectImmune)
                    {
                        unitsOnLevelForFire.RemoveAt(i);
                    }
                }

                if (unitsOnLevelForFire.Count <= 0 && gm.sc.mobsInGame.Count > 0)
                {
                    unitsOnLevelForFire.Add(gm.sc.mobsInGame[Random.Range(0, gm.sc.mobsInGame.Count)]);
                }
                else if (gm.sc.mobsInGameStatic.Count > 0)
                {
                    unitsOnLevelForFire.Add(gm.sc.mobsInGameStatic[Random.Range(0, gm.sc.mobsInGameStatic.Count)]);
                }
                else
                    unitsOnLevelForFire.Add(hc);
                
                blockingQuest.fireTarget = unitsOnLevelForFire[Random.Range(0, unitsOnLevelForFire.Count)];
                blockingQuest.targetName = blockingQuest.fireTarget.names[gm.language];
                blockingQuest.fireTarget.SpawnDynamicMapMark(2);
                blockingQuest.questsDidntFinishedLine.Add("Maps will show you where " + blockingQuest.fireTarget.names[gm.language] + " hides. Go set him on fire!");   
                blockingQuest.questsDidntFinishedLine.Add("Карты покажут, где прячется " + blockingQuest.fireTarget.names[gm.language] + ". Иди и подожги его!");   
                blockingQuest.questsDidntFinishedLine.Add("Los mapas te señalarán donde se esconde " + blockingQuest.fireTarget.names[gm.language] + ". Ve a ponerle en llamas!");
                blockingQuest.questsDidntFinishedLine.Add("Die Karten werden dir zeigen wo sich " + blockingQuest.fireTarget.names[gm.language] + " versteckt. Geh und zünde ihn an!");
                blockingQuest.questsDidntFinishedLine.Add("Le mappe ti mostreranno dove " + blockingQuest.fireTarget.names[gm.language] + " si nasconde. Vai a dargli fuoco!");
                blockingQuest.questsDidntFinishedLine.Add("Os mapas mostra para onde " + blockingQuest.fireTarget.names[gm.language] + " se esconde. Vá atirar fogo nele!");

                blockingQuest.questsFinishedLine.Add("Did you set "+ blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + " on fire? Great! We can talk now");   
                blockingQuest.questsFinishedLine.Add("Ты поджог "+ blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + "? Спасибо! Теперь можем поговорить");   
                blockingQuest.questsFinishedLine.Add("Incendiaste a " + blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + " ? Perfecto! Ahora podemos hablar");
                blockingQuest.questsFinishedLine.Add("Hast du " + blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + " in Brand gesteckt? Super! Jetzt können wir reden");
                blockingQuest.questsFinishedLine.Add("Hai dato fuoco a " + blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + " Ottimo! Ora possiamo parlare");
                blockingQuest.questsFinishedLine.Add("Você fez " + blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + " em chamas ? Ótimo! podemos conversar agora");

                blockingQuest.questsFailedLine.Add("You killed "+ blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + "??? Man, you're maniac! I didn't sign for this...");
                blockingQuest.questsFailedLine.Add("Ты убил "+ blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + "??? Чувак, ты маньяк! Я на это не подписывался...");
                blockingQuest.questsFailedLine.Add("Has matado a " + blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + "??? Tio, eres un maniático! No me inscribí para esto");
                blockingQuest.questsFailedLine.Add("Du hast " + blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + "getötet? Man, du bist doch wahnsinnig! Dafür habe ich mich nicht angemeldet");
                blockingQuest.questsFailedLine.Add("Hai ucciso " + blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + "? Bro, sei un maniaco! Non ho firmato per questo...");
                blockingQuest.questsFailedLine.Add("Você matou " + blockingQuest.fireTarget.damagedByPlayerMessage[gm.language] + "? Cara, você é um maníaco! Eu não me inscrevi para isso ...");

                blockingQuest.getRewardText.Add("No problem, it smelled pretty delicious");
                blockingQuest.getRewardText.Add("Без проблем, оно пахло довольно вкусно");
                blockingQuest.getRewardText.Add("No hay problema, olía bastante delicioso");
                blockingQuest.getRewardText.Add("Kein Problem, es hat sehr lecker gerochen");
                blockingQuest.getRewardText.Add("Nessun problema, aveva un odore delizioso");
                blockingQuest.getRewardText.Add("Não tem problema, cheirava muito delicioso");

                blockingQuest.rewardTakenText.Add("Now we can talk");
                blockingQuest.rewardTakenText.Add("Теперь можем поговорить");
                blockingQuest.rewardTakenText.Add("Ahora podemos hablar");
                blockingQuest.rewardTakenText.Add("Jezt können wir reden");
                blockingQuest.rewardTakenText.Add("Ora possiamo parlare");
                blockingQuest.rewardTakenText.Add("Agora podemos conversar");
                break;
            case QuestBlockingNpcProgress.QuestType.Bleed:

                List<HealthController> unitsOnLevelForBleed = new List<HealthController>(gm.units);
                for (int i = unitsOnLevelForBleed.Count - 1; i >= 0; i--)
                {
                    if (unitsOnLevelForBleed[i].door || unitsOnLevelForBleed[i] == hc || unitsOnLevelForBleed[i] == gm.player || 
                        !unitsOnLevelForBleed[i].gameObject.activeInHierarchy || unitsOnLevelForBleed[i].statusEffects[2].effectImmune)
                    {
                        unitsOnLevelForBleed.RemoveAt(i);
                    }
                }

                if (unitsOnLevelForBleed.Count <= 0 && gm.sc.mobsInGame.Count > 0)
                {
                    unitsOnLevelForBleed.Add(gm.sc.mobsInGame[Random.Range(0, gm.sc.mobsInGame.Count)]);
                }
                else if (gm.sc.mobsInGameStatic.Count > 0)
                {
                    unitsOnLevelForBleed.Add(gm.sc.mobsInGameStatic[Random.Range(0, gm.sc.mobsInGameStatic.Count)]);
                }
                else
                    unitsOnLevelForBleed.Add(hc);
                
                blockingQuest.bleedTarget = unitsOnLevelForBleed[Random.Range(0, unitsOnLevelForBleed.Count)];
                blockingQuest.targetName = blockingQuest.bleedTarget.names[gm.language];
                blockingQuest.bleedTarget.SpawnDynamicMapMark(3);
                blockingQuest.questsDidntFinishedLine.Add("Maps will show you where " + blockingQuest.bleedTarget.names[gm.language] + " hides. Go make it bleed!");   
                blockingQuest.questsDidntFinishedLine.Add("Карты покажут, где прячется " + blockingQuest.bleedTarget.names[gm.language] + ". Пусть это мясо кровоточит!");   
                blockingQuest.questsDidntFinishedLine.Add("Los mapas te señalarán donde se esconde " + blockingQuest.bleedTarget.names[gm.language] + ". Ve a hacerle sangrar!");
                blockingQuest.questsDidntFinishedLine.Add("Die Karten werden dir zeigen wo sich " + blockingQuest.bleedTarget.names[gm.language] + " versteckt. Geh und bringe es zum Bluten!");
                blockingQuest.questsDidntFinishedLine.Add("Le mappe ti mostreranno dove " + blockingQuest.bleedTarget.names[gm.language] + " si nasconde. Vai a farlo sanguinare!");
                blockingQuest.questsDidntFinishedLine.Add("Os mapas mostra para onde " + blockingQuest.bleedTarget.names[gm.language] + " se esconde. Vá fazer sangrar!");

                blockingQuest.questsFinishedLine.Add("Did you make "+ blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + " bleed? Great! We can talk now");   
                blockingQuest.questsFinishedLine.Add("Ты изрезал "+ blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + "? Спасибо! Теперь можем поговорить");   
                blockingQuest.questsFinishedLine.Add("Has hecho que " + blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + " Sangre ? Perfecto! Ahora podemos hablar");
                blockingQuest.questsFinishedLine.Add("Hast du " + blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + " zum Bluten gebracht? Gut, dann können wir uns jetzt unterhalten.");
                blockingQuest.questsFinishedLine.Add("Hai fatto sanguinare " + blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + "? Ottimo! Ora possiamo parlare");
                blockingQuest.questsFinishedLine.Add("Você fez " + blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + " sangrar ? Ótimo! podemos conversar agora");

                blockingQuest.questsFailedLine.Add("You killed "+ blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + "??? Man, you're maniac! I didn't sign for this...");
                blockingQuest.questsFailedLine.Add("Ты убил "+ blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + "??? Чувак, ты маньяк! Я на это не подписывался...");
                blockingQuest.questsFailedLine.Add("Has matado a " + blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + "??? Tio, eres un maniático! No me inscribí para esto");
                blockingQuest.questsFailedLine.Add("Du hast " + blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + "getötet? Man, du bist doch wahnsinnig! Dafür habe ich mich nicht angemeldet");
                blockingQuest.questsFailedLine.Add("Hai ucciso " + blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + "? Bro, sei un maniaco! Non ho firmato per questo...");
                blockingQuest.questsFailedLine.Add("Você matou " + blockingQuest.bleedTarget.damagedByPlayerMessage[gm.language] + "? Cara, você é um maníaco! Eu não me inscrevi para isso ...");

                blockingQuest.getRewardText.Add("No problem, that was pretty fun");
                blockingQuest.getRewardText.Add("Без проблем, было весело");
                blockingQuest.getRewardText.Add("No hay problema, eso fue bastante divertido");
                blockingQuest.getRewardText.Add("Kein Problem, das hat ziemlich viel Spaß gemacht");
                blockingQuest.getRewardText.Add("Nessun problema, è stato molto divertente");
                blockingQuest.getRewardText.Add("Não tem problema, isso foi muito divertido");

                blockingQuest.rewardTakenText.Add("Now we can talk");
                blockingQuest.rewardTakenText.Add("Теперь можем поговорить");
                blockingQuest.rewardTakenText.Add("Ahora podemos hablar");
                blockingQuest.rewardTakenText.Add("Jezt können wir reden");
                blockingQuest.rewardTakenText.Add("Ora possiamo parlare");
                blockingQuest.rewardTakenText.Add("Agora podemos conversar");

                break;
        }
        blockingQuest.npcThanks.Add("Thanks! Here, I have some junk for you");
        blockingQuest.npcThanks.Add("Спасибо! Вот тебе подарочек");
        blockingQuest.npcThanks.Add("Gracias! Aquí, tengo algo de basura para ti");
        blockingQuest.npcThanks.Add("Danke! Hier habe ich etwas Müll für dich");
        blockingQuest.npcThanks.Add("Grazie! Ho un regalo per te");
        blockingQuest.npcThanks.Add("Obrigado! Eu tenho um presente para você");

        inQuest = true;
        GutQuestsController.instance.SaveQuestToManager(blockingQuest);
    }

    void OnDestroy()
    {
        HideDialogue();
    }
}

[Serializable]
public class Dialogue
{
    public enum DialogueEvent 
    {Null, FixWeapon, Heal, BuySkill, BuyTool, BuyKey, LoseAnEye, PlaySong, 
        OneLevelBack, GivePlayerWeapon, BuyAmmo, UpgradeWeapon, Warm, 
        KillQuest, ItemQuest, WeaponQuest, PoisonQuest, BleedQuest, FireQuest, 
        ReduceBadRep, DisarmTrap, JoinToxicCult, JoinFlamesCult, JoinBloodCult, JoinGoldCult,
        KissHer, MeatHole, GiveCarl, BuyGoldenKey, Taxi, SetNpcInLove, Pray, AgreeSetObjectActiveAndHideNpc
    }
    public DialogueEvent dialogueEvent;
    public int eventCost = 10;
    public int questToStart = -1;
    public int questToComplete = -1;
    public List<string> phrases = new List<string>();
    public List<string> phrasesRu = new List<string>();
    public List<string> phrasesESP = new List<string>();
    public List<string> phrasesGER = new List<string>();
    public List<string> phrasesIT = new List<string>();
    public List<string> phrasesSPBR = new List<string>();
    public string doesntMetRequirementsPhrase = "Us can't help your meat now";
    public string doesntMetRequirementsPhraseRu = "Сейчас мы не можем помочь твоему мясу";
    public string doesntMetRequirementsPhraseEsp = "Us can't help your meat now";
    public string doesntMetRequirementsPhraseGer = "Us can't help your meat now";
    public string doesntMetRequirementsPhraseIT = "Us can't help your meat now";
    public string doesntMetRequirementsPhraseSPBR = "Us can't help your meat now";
    public string eventComplete = "Good luck to your meat";
    public string eventCompleteRu = "Удачи твоему мясу";
    public string eventCompleteEsp = "Buena suerte fiambre.";
    public string eventCompleteGer = "Viel glück, Fleisch.";
    public string eventCompleteIT = "Buona fortuna alla tua carne";
    public string eventCompleteSPBR = "Boa sorte com a sua carne.";
}

[Serializable]
public class QuestBlockingNpcProgress
{
    public enum QuestType
    {
        Kill, Item, Weapon, Poison, Bleed, Fire, 
    }

    public HealthController npc;
    public bool completed = false;
    public bool failed = false;
    public bool rewarded = false;
    public HealthController killTarget;
    public ToolController.ToolType toolTarget = ToolController.ToolType.Null;
    public WeaponPickUp.Weapon weaponTarget = WeaponPickUp.Weapon.Null; 
    public HealthController poisonTarget;
    public HealthController bleedTarget;
    public HealthController fireTarget;

    public string targetName = "";
    public List<string> questsDidntFinishedLine = new List<string>();
    public List<string> questsFinishedLine = new List<string>();
    public List<string> questsFailedLine = new List<string>();
    public List<string> getRewardText = new List<string>();
    public List<string> rewardTakenText = new List<string>();
    public List<string> npcThanks = new List<string>();

}