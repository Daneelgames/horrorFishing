using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;

public class AddTutorialToItemName : MonoBehaviour
{
    public Interactable interactableToTutorialize;
    void Start()
    {
        var action = PlayerInputManager.instance.playerInput.Interaction;
        interactableToTutorialize.itemName += " - ";
        for (int t = 0; t < action.Bindings.Count; t++)
        {
            if (action.Bindings[t].DeviceClass == InputDeviceClass.Keyboard || action.Bindings[t].DeviceClass == InputDeviceClass.Mouse)
                interactableToTutorialize.itemName += action.Bindings[t].Name + " / ";   
        }
        
        interactableToTutorialize.itemName += " R1 ";

        for (int i = 0; i < interactableToTutorialize.itemNames.Count; i++)
        {
            interactableToTutorialize.itemNames[i] = interactableToTutorialize.itemName;
        }
    }
}
