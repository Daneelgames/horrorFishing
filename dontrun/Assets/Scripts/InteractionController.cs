using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using TMPro;

public class InteractionController : MonoBehaviour
{
    public static InteractionController instance;

    GameManager gm;
    PlayerMovement pm;

    public PortableObject objectInHands = null;
    public GameObject crosshairVisual;

    /*
    public float dropGoldCooldown = 0.5f;
    public float dropGoldCooldownCurrent = 0;
    */
        
    public LayerMask layerMask;
    public float rayDistance = 3;
    RaycastHit hit;
    RaycastHit hitDoor;
    private UiManager ui;

    public Interactable selectedItem;

    private string interactionString = "Interaction";

    private void Awake()
    {
        if (!instance)
            instance = this;
    }

    private void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        ui = UiManager.instance;
        
        StartCoroutine(Raycast());
    }

    private void Update()
    {
        if ((Input.GetButtonDown(interactionString) || KeyBindingManager.GetKeyDown(KeyAction.Interaction)) && !gm.paused &&pm.hc.health > 0)
        {
            if (objectInHands)
            {
                objectInHands.Drop();
            }
            else if (selectedItem)
            {
                if (selectedItem.door)
                    PlayerAudioController.instance.PlayInteract();
                else
                    PlayerAudioController.instance.PlayInteract();
                
                selectedItem.Interact(false);
            }
            
            //if (ui.dialogueInteractor != null && (!selectedItem || selectedItem != ui.dialogueInteractor.interactable))
            if (ui.dialogueInteractor && (!selectedItem || (selectedItem && selectedItem != ui.dialogueInteractor.interactable)))
            {
                ui.dialogueInteractor.HideDialogue();
            }
        }
    }

    IEnumerator Raycast()
    {
        while (true)
        {
            // search for selectedItem

            if (objectInHands == null || !gm.lg.generating)
            {
                Physics.Raycast(pm.cameraAnimator.transform.position + pm.cameraAnimator.transform.forward * -0.5f, crosshairVisual.transform.position - pm.cameraAnimator.transform.position, out hit, rayDistance, layerMask);

                if (hit.collider != null)
                {
                    if (selectedItem && hit.collider.gameObject == selectedItem.gameObject)
                    {
                        // nothing
                    }
                    else if (gm.itemList.interactables.Count > 0 && (hit.collider.gameObject.layer == 13 || hit.collider.gameObject.layer == 18))
                    {
                        for (var index = 0; index < gm.itemList.interactables.Count; index++)
                        {
                            Interactable i = gm.itemList.interactables[index];
                            if (i != null && i.gameObject == hit.collider.gameObject)
                            {
                                selectedItem = i;
                                break;
                            }
                        }

                        if (selectedItem == null)
                        {
                            var cutSceneTrigger = hit.collider.gameObject.GetComponent<CutSceneTrigger>();
                            if (cutSceneTrigger)
                            {
                                cutSceneTrigger.PlayerLookedOnTrigger();
                            }
                        }
                    }
                    else
                    {
                        selectedItem = null;

                    }
                }
                else
                {
                    selectedItem = null;
                }

                if (selectedItem)
                    selectedItem.UpdateNameGui();
            }
            else
                selectedItem = null;

            yield return new WaitForSeconds(0.1f);
        }
    }
}