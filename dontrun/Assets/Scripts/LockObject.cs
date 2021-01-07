using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class LockObject : MonoBehaviour
{
    public string lockedPhrase = "It's locked.";
    public string lockedPhraseRu = "Заперто.";
    public string lockedPhraseEsp = "Esta cerrado";
    public string lockedPhraseGer = "Es ist verschlossen";
    public string lockedPhraseIT = "È bloccata";
    public string lockedPhraseSPBR = "Está trancado";
    public AudioClip phraseSound;
    public AudioClip useItemSound;
    public ItemsList.ResourceType neededItem = ItemsList.ResourceType.Key;
    public int questItemNeeded = -1;
    private bool used = false;
    public GameObject objectToAtivate;
    public GameObject deathParticles;
    public DoorController door;

    public Animator lockAnimator;
    public float disableTime = 3;

    private Interactable interactable;

    UiManager ui = UiManager.instance;
    private ItemsList il;

    private Coroutine unlockCoroutine;
    
    void Start()
    {
        il = ItemsList.instance;
        ui = UiManager.instance;
    }
    
    public bool HasItem()
    {
        bool returnBool = false;
        switch(neededItem)
        {
            case ItemsList.ResourceType.Key:
                if (ItemsList.instance.keys > 0)
                {
                    returnBool = true;
                }
                break;
            
            case ItemsList.ResourceType.Lockpick:
                if (ItemsList.instance.lockpicks > 0)
                {
                    returnBool = true;
                }
                break;

            case ItemsList.ResourceType.QuestItem:
                if (il.savedQuestItems.Count > 0)
                {
                    for (int i = 0; i < il.savedQuestItems.Count; i++)
                    {
                        if (il.savedQuestItems[i] == questItemNeeded)
                        {
                            returnBool = true;
                        }
                    }   
                }
                break;
            case ItemsList.ResourceType.GoldenKey:
                if (il.goldenKeysAmount > 0)
                {
                    returnBool = true;   
                }
                break;
        }
        return returnBool;
    }

    public void UseKeyOnClient(bool removeKeyFromPlayer)
    {
        if (unlockCoroutine == null)
        {
            unlockCoroutine = StartCoroutine(UnlockCoroutine());
            
            if (removeKeyFromPlayer)
            {
                ItemsList.instance.keys--;
                UiManager.instance.KeyUsed();
                UiManager.instance.UpdateKeys(ItemsList.instance.keys, -1);
            }   
        }
    }

    IEnumerator UnlockCoroutine()
    {
        if (lockAnimator)
            lockAnimator.SetTrigger("Unlock");
        
        yield return new WaitForSeconds(disableTime);
        
        if (deathParticles)
        {
            print("death particles");
            deathParticles.transform.parent = null;
            deathParticles.SetActive(true);
        }
        
        print("unlock golden gate");
        gameObject.SetActive(false);
    }

    public void UseItem(Interactable _interactable)
    {
        interactable = _interactable;
        if (!used)
        {
            used = true;
            switch (neededItem)
            {
                case ItemsList.ResourceType.Key:

                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    {
                        GLNetworkWrapper.instance.UseKeyOnLock(ItemsList.instance.interactables.IndexOf(interactable));
                    }
                    else
                    {
                        UseKeyOnClient(true);
                    }
                    break;
                
                case ItemsList.ResourceType.QuestItem:
                    if (objectToAtivate)
                    {
                        for (int i = 0; i < il.savedQuestItems.Count; i++)
                        {
                            if (il.savedQuestItems[i] == questItemNeeded)
                            {
                                il.savedQuestItems.RemoveAt(i);
                                break;
                            }
                        }
                        objectToAtivate.SetActive(true);
                        il.herPiecesApplied.Add(questItemNeeded);

                        if (il.herPiecesApplied.Count == 6) // all parts found
                        {
                            GameManager.instance.newGamePlus = true;
                            TwitchManager.instance.NewGamePlusUnlocked();
                        }
                        
                        GameManager.instance.SaveGame();

                        if (GameManager.instance.demo)
                        {
                            PlayerMovement.instance.goldenLightAnimator.SetBool("GoldenLight", true);
                            Invoke("EndDemo", 5);
                        }

                        if (questItemNeeded == 7) // last part of her
                        {
                            EndingController.instance.StartEndLevel();
                        }
                    }
                    break;
                
                case ItemsList.ResourceType.GoldenKey:
                    il.goldenKeysAmount--;
                    HubProgressionManager.instance.HubDoorOpened(this);
                    GameManager.instance.SaveGame();
                    UseKeyOnClient(false);
                    break;
                
                case ItemsList.ResourceType.Lockpick:
                    door.Unlock();
                    il.lockpicks--;
                    ui.DoorUnlock();
                    //UiManager.instance.U    pdateLockpicks();
                    break;
            }

        }
    }

    void EndDemo()
    {
        GameManager.instance.ReturnToMenu(true);
    }
}
