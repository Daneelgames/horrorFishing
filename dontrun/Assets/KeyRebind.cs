using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InControl;
using TMPro;
using UnityEngine.UI;

public class KeyRebind : MonoBehaviour
{
    public List<RebindableKey> keysList = new List<RebindableKey>();
    private PlayerInput playerInput;
    
    private InputDevice activeInputDevice;
    private string saveData;
    public bool listening = false;
    void OnEnable()
    {
        UpdateKeys();
    }

    void UpdateKeys()
    {
        activeInputDevice = InputManager.ActiveDevice;
        
        playerInput = PlayerInputManager.instance.playerInput;
        LoadBindings();
        
        for (int i = 0; i < playerInput.Actions.Count; i++)
        {
            var action = playerInput.Actions[i];
            //print(action.Name);
            
            if (GetNonRebindableAction(action))
                continue;

            keysList[i].actionText.text = action.Name;
            keysList[i].savedPlayerAction = action;
            var text = keysList[i].keyText;
            //text.text = action.Bindings.Last().Name;
            text.text = String.Empty;

            //continue;
            for (int t = 0; t < action.Bindings.Count; t++)
            {
                if (action.Bindings[t].DeviceClass == InputDeviceClass.Keyboard || action.Bindings[t].DeviceClass == InputDeviceClass.Mouse)
                    text.text += action.Bindings[t].Name + " ";   
            }
        }                
        StartCoroutine(ListeningDelay());
    }

    void OnDisable()
    {
        SaveBindings();
    }

    bool GetNonRebindableAction(PlayerAction action)
    {
        if (action.Name == "Pause" || 
            action.Name == "LookLeft" || action.Name == "LookUp" || 
            action.Name == "LookRight" || action.Name == "LookDown")
            return true;

        return false;
    }

    public void ActionStartListening(int i)
    {
        if (listening)
            return;

        print("click");
        listening = true;
        
        for (var index = 0; index < keysList.Count; index++)
        {
            if (i == index)
                continue;
            
            keysList[index].savedPlayerAction.StopListeningForBinding();
        }

        keysList[i].keyText.text = "...";
        keysList[i].savedPlayerAction.ListenForBinding();
        
        playerInput.ListenOptions.OnBindingFound = ( action, binding ) =>
        {
            print(binding.Name);
            // Binding sources are comparable, so we can do this.
            if (binding == new KeyBindingSource( Key.Escape ))
            {
                action.StopListeningForBinding();
                StartCoroutine(ListeningDelay());
                return false;
            }
            return true;
        };
        playerInput.ListenOptions.OnBindingAdded = ( action, binding ) =>
        {
            print(binding.Name);
            //keysList[i].keyText.text = binding.Name;
            
            action.StopListeningForBinding();
            SaveBindings();
            listening = false;
            UpdateKeys();
        };
        playerInput.ListenOptions.OnBindingRejected = ( action, binding, rejectionType ) =>
        {
            print(binding.Name);
            action.RemoveBinding(binding);
            
            action.StopListeningForBinding();
            SaveBindings();
            listening = false;
            UpdateKeys();
        };
    }

    IEnumerator ListeningDelay()
    {
        yield return new WaitForSeconds(0.1f);
        listening = false;
    }
    
    void SaveBindings()
    {
        saveData = playerInput.Save();
        PlayerPrefs.SetString( "Bindings", saveData );
    }

    void LoadBindings()
    {
        if (PlayerPrefs.HasKey( "Bindings" ))
        {
            saveData = PlayerPrefs.GetString( "Bindings" );
            playerInput.Load( saveData );
        }
    }
}

[Serializable]
public class RebindableKey
{
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI keyText;
    public PlayerAction savedPlayerAction;
}
