using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager instance;

    public PlayerInput playerInput;
    
    void Awake()
    {
        instance = this;
        InitInput();
    }

    void InitInput()
    {
        // use default for now
        playerInput = new PlayerInput();
        playerInput.LookLeft.AddDefaultBinding(InputControlType.RightStickLeft);
        playerInput.LookLeft.AddDefaultBinding(Mouse.NegativeX);
        playerInput.LookUp.AddDefaultBinding(InputControlType.RightStickUp);
        playerInput.LookUp.AddDefaultBinding(Mouse.PositiveY);
        playerInput.LookRight.AddDefaultBinding(InputControlType.RightStickRight);
        playerInput.LookRight.AddDefaultBinding(Mouse.PositiveX);
        playerInput.LookDown.AddDefaultBinding(InputControlType.RightStickDown);
        playerInput.LookDown.AddDefaultBinding(Mouse.NegativeY);
    
        playerInput.Aim.AddDefaultBinding(InputControlType.LeftTrigger);
        playerInput.Aim.AddDefaultBinding(Mouse.RightButton);
        
        playerInput.MoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
        playerInput.MoveLeft.AddDefaultBinding(Key.A);
        playerInput.MoveForward.AddDefaultBinding(InputControlType.LeftStickUp);
        playerInput.MoveForward.AddDefaultBinding(Key.W);
        playerInput.MoveRight.AddDefaultBinding(InputControlType.LeftStickRight);
        playerInput.MoveRight.AddDefaultBinding(Key.D);
        playerInput.MoveBack.AddDefaultBinding(InputControlType.LeftStickDown);
        playerInput.MoveBack.AddDefaultBinding(Key.S);
        
        playerInput.Crouch.AddDefaultBinding(InputControlType.Action2);
        playerInput.Crouch.AddDefaultBinding(Key.LeftControl);
        
        playerInput.Dash.AddDefaultBinding(InputControlType.Action1);
        playerInput.Dash.AddDefaultBinding(Key.Space);
        
        playerInput.Sprint.AddDefaultBinding(InputControlType.LeftBumper);
        playerInput.Sprint.AddDefaultBinding(Key.LeftShift);
        
        playerInput.Pause.AddDefaultBinding(InputControlType.Command);
        playerInput.Pause.AddDefaultBinding(Key.Escape);
        
        playerInput.Journal.AddDefaultBinding(InputControlType.LeftStickButton);
        playerInput.Journal.AddDefaultBinding(Key.Tab);
        
        playerInput.Interaction.AddDefaultBinding(InputControlType.RightBumper);
        playerInput.Interaction.AddDefaultBinding(Key.E);
        
        playerInput.Attack.AddDefaultBinding(InputControlType.RightTrigger);
        playerInput.Attack.AddDefaultBinding(Mouse.LeftButton);
        
        playerInput.SwitchWeapon.AddDefaultBinding(InputControlType.Action4);
        playerInput.SwitchWeapon.AddDefaultBinding(Key.Q);
        
        playerInput.Reload.AddDefaultBinding(InputControlType.Action3);
        playerInput.Reload.AddDefaultBinding(Key.R);
        
    }
}
