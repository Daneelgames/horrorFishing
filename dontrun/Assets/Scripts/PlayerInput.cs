using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;

public class PlayerInput : PlayerActionSet
{
    public PlayerAction MoveForward;
    public PlayerAction MoveBack;
    public PlayerAction MoveRight;
    public PlayerAction MoveLeft;
    public PlayerAction Sprint;
    public PlayerAction Dash;
    public PlayerAction Crouch;
    public PlayerAction Interaction;
    public PlayerAction Attack;
    public PlayerAction Aim;
    public PlayerAction SwitchWeapon;
    public PlayerAction Reload;
    public PlayerAction Journal;
    
    //nonrebindable
    public PlayerAction LookLeft;
    public PlayerAction LookUp;
    public PlayerAction LookRight;
    public PlayerAction LookDown;
    public PlayerTwoAxisAction Look;
    public PlayerTwoAxisAction Move;
    public PlayerAction Pause;

    public PlayerInput()
    {
        MoveForward = CreatePlayerAction("MoveForward");
        MoveBack = CreatePlayerAction("MoveBack");
        MoveRight = CreatePlayerAction("MoveRight");
        MoveLeft = CreatePlayerAction("MoveLeft");
        Sprint = CreatePlayerAction("Sprint");
        Dash = CreatePlayerAction("Dash");
        Crouch = CreatePlayerAction("Crouch");
        Interaction = CreatePlayerAction("Interaction");
        Attack = CreatePlayerAction("Attack");
        Aim = CreatePlayerAction("Aim");
        SwitchWeapon = CreatePlayerAction("Switch Weapon. Hold to holster");
        Reload = CreatePlayerAction("Reload");
        Journal = CreatePlayerAction("Journal");
        
        // non rebindable
        Pause = CreatePlayerAction("Pause");
        LookLeft = CreatePlayerAction("LookLeft");
        LookUp = CreatePlayerAction("LookUp");
        LookRight = CreatePlayerAction("LookRight");
        LookDown = CreatePlayerAction("LookDown");
        Look = CreateTwoAxisPlayerAction(LookLeft, LookRight, LookDown, LookUp);
        Move = CreateTwoAxisPlayerAction(MoveLeft, MoveRight, MoveBack, MoveForward);

    }
}
