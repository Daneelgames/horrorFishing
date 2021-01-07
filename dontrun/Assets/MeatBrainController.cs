using System.Collections;
using System.Collections.Generic;
using Mirror;
using PlayerControls;
using UnityEngine;

public class MeatBrainController : NetworkBehaviour
{
    // when this object is taken to the iron maiden, resurrect the dead player

    private PlayerMovement pm;
    public PortableObject portable;
    public Rigidbody rb;
    public GameObject visualToHideOnDeadClient;
    public Interactable interactable;

    public int playerIndex = 0;
    public bool pickedUp = false;
    public Collider triggerCollider;

    private Coroutine pickUpCoroutine;

    void Start()
    {
        pm = PlayerMovement.instance;

        if (pm.hc.health <= 0 && pm.transform.parent == null)
        {
            print("take player to brain");
            visualToHideOnDeadClient.SetActive(false);
            interactable.enabled = false;
            triggerCollider.enabled = false;
            pm.MoveToMeatBrain(this);
            
            UiManager.instance.ToggleGameUi(false, false);
        }
        else
        { 
            print("not take player to brain");
            pm = null;   
        }
        
        IronMaidenController.instance.SetBrainOnLevel(this);        
    }

    #region pick up and drop
    public void PickUp()
    {
        if (pickUpCoroutine != null)
            StopCoroutine(pickUpCoroutine);

        pickUpCoroutine = StartCoroutine(PickUpCoroutine(true));
    }
    
    public void Drop()
    {
        if (pickUpCoroutine != null)
            StopCoroutine(pickUpCoroutine);
        
        pickUpCoroutine = StartCoroutine(PickUpCoroutine(false));
    }

    IEnumerator PickUpCoroutine(bool pickUp)
    {
        yield return new WaitForSeconds(1);
        
        /*
        pickedUp = pickUp;
        rb.useGravity = !pickUp;
        */
        GLNetworkWrapper.instance.TogglePickUpBrain(interactable.gameObject, pickUp);
    }

    public void TogglePickUpBrainOnClient(bool active)
    {
        print(active);
        pickedUp = active;
        rb.useGravity = !active;
        rb.isKinematic = active;
    }
    #endregion
    
    public void ReleasePlayer()
    {
        print("RELEASE PLAYER");
        if (pm)
        {
            // on dead client
            if (portable.inHands)
                portable.Drop();
            
            pm.MoveToMeatBrain(null);
            UiManager.instance.ToggleGameUi(true, false);
            
            pm = null;
        }
        else
        {
            var dummy = GLNetworkWrapper.instance.playerNetworkObjects[playerIndex].connectedDummy;
            dummy.Resurrect(dummy.hc.healthMax / 2);
        }
            
        Destroy(gameObject);
    }
}
