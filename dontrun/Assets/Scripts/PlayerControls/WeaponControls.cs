using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using PlayerControls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


public enum WeaponPickUpAction
{
    TakeToActiveSlot,
    TakeToSecondSlot
}

public class WeaponControls : MonoBehaviour
{
    public static WeaponControls instance;

    public Animator crosshair;
    public WeaponController activeWeapon;
    public WeaponController secondWeapon;
    public int currentToolIndex = 0;
    
    public List<ToolController> allTools;
    
    [Tooltip("0 - Axe; 1 - Pistol; 2 - Revolver; 3 - Shotgun")]
    public List<WeaponController> weaponList;

    public NoseWithTeethController noseWithTeeth;

    string Hide = "Hide";

    UiManager ui;

    PlayerMovement pm;
    InteractionController ic;
    private PlayerSkillsController psc;
    SpawnController sc;
    ItemsList il;
    PlayerAudioController pac;
    GameManager gm;
    bool usingTool = false;
    bool alreadyBroke = false;

    private string nextTool = "";
    private string prevTool = "";
    private float hideWeaponTime = 0.75f;
    private float hideWeaponTimeCurrent = 0;
    private float eatWeaponTime = 0.75f;
    private float eatWeaponTimeCurrent = 0;
    private float dropWeaponTime = 0.75f;
    private float dropWeaponTimeCurrent = 0;

    private float changeToolCooldown = 0.1f;
    
    public WeaponPickUpAction weaponPickUpAction = WeaponPickUpAction.TakeToSecondSlot;

    private string selfAttackString = "SelfAttack";
    private string attackString = "Fire1";
    private string useToolString = "Use Tool";
    private string dropString = "Drop";
    private string throwToolString = "Throw Tool";
    private string reloadString = "Reload";
    private string switchWeaponString = "SwitchWeapon";
    private string mouseWheelString = "Mouse ScrollWheel";
    private string eatWeaponTrigger = "EatWeapon";

    public bool eatingWeapon = false;
    private float eatCooldown = 0;
    private float eatCooldownMax = 0.5f;
    private float dropCooldown = 0;
    private float dropCooldownMax = 0.5f;

    private int dPadPreviousFrameY = 0;
    private bool canEatWeapon = true;
    
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        ic = InteractionController.instance;
        il = ItemsList.instance;
        ui = UiManager.instance;
        sc = SpawnController.instance;
        pac = PlayerAudioController.instance;
        psc = PlayerSkillsController.instance;
    }

    public void Init()
    {
        GetTools();
        FindNewTool();
        StartCoroutine(TorchLoseDurability());
    }

    public void ResetInventory()
    {
        if (secondWeapon != null)
            RemoveWeapon(1);
        
        if (activeWeapon != null)
            RemoveWeapon(0);
    }
    
    void GetTools()
    {
        for (int i = 0; i < il.savedTools.Count; i++)
        {
            allTools[i].type = il.savedTools[i].type;
        }
    }

    void Update()
    {
        if (pm.hc.health > 0 && pm.controller.enabled && !gm.paused /*&& !gm.questWindow */&& !gm.mementoWindow && (gm.lg == null || !gm.lg.generating))
        {
            if (!eatingWeapon)
            {
                Attacking();
                //AttackYourself();
                Reloading();
            
                SwitchWeapon();
                HideWeapon();
                
                /*
                if (dropCooldown <= 0)
                {
                    DropWeaponInput();
                    DropToolInput();   
                }*/
            }
            
            ItemInHands();

            if (eatCooldown <= 0)
            {
                //EatWeapon();
                UseTool();   
            }
            
            //ThrowTool();
            ChangeTool();
            
             
            if (Input.GetAxisRaw(throwToolString) < 0 || Input.GetAxisRaw(useToolString) < 0)
                dPadPreviousFrameY = -1;  
            else if (Input.GetAxisRaw(throwToolString) > 0 || Input.GetAxisRaw(useToolString) > 0)
                dPadPreviousFrameY = 1;  
            else 
                dPadPreviousFrameY = 0;

            if (eatCooldown > 0)
                eatCooldown -= Time.deltaTime;

            if (dropCooldown > 0)
                dropCooldown -= Time.deltaTime;
        }

        if (changeToolCooldown > 0)
            changeToolCooldown -= Time.deltaTime;
    }

    private string takePainkillerString = "TakePainkiller";
    void UseTool()
    {
        /*
        if ((Input.GetButtonUp(useToolString) ||
             KeyBindingManager.GetKeyUp(KeyAction.UseTool) ||
             ((Math.Abs(Input.GetAxisRaw(throwToolString)) < 0.1f ||
               Math.Abs(Input.GetAxisRaw(useToolString)) < 0.1f) &&
              dPadPreviousFrameY < -0.1f)))
              */
        if (KeyBindingManager.GetKeyUp(KeyAction.UseTool) ||
            ((Math.Abs(Input.GetAxisRaw(throwToolString)) < 0.1f ||
              Math.Abs(Input.GetAxisRaw(useToolString)) < 0.1f) &&
             dPadPreviousFrameY < -0.1f))
        {
            if (!usingTool && eatWeaponTimeCurrent < eatWeaponTime
                           && ic.objectInHands == null && (activeWeapon == null || activeWeapon.canAct) 
                           && il.savedTools[currentToolIndex].amount > 0)
            {
                ui.UseTool(currentToolIndex);
                eatCooldown = eatCooldownMax;
                StartCoroutine(UsingTool());
        
                pm.crosshairAnimator.SetTrigger(takePainkillerString);

                eatWeaponTimeCurrent = 0;
                if (activeWeapon)
                    StartCoroutine(activeWeapon.UseTool(2));   
            }
        
            eatWeaponTimeCurrent = 0;  
        }
    }

    void EatWeapon()
    {
        //if (Input.GetButtonDown(useToolString) || KeyBindingManager.GetKeyDown(KeyAction.UseTool))
        if (KeyBindingManager.GetKeyDown(KeyAction.UseTool))
        {
            eatWeaponTimeCurrent = 0;
            canEatWeapon = true;
        }
        
        //if (canEatWeapon && (Input.GetButton(useToolString) || KeyBindingManager.GetKey(KeyAction.UseTool) || dPadPreviousFrameY < -0.1f))
        if (canEatWeapon && (KeyBindingManager.GetKey(KeyAction.UseTool) || dPadPreviousFrameY < -0.1f))
        {
            if (eatWeaponTimeCurrent < eatWeaponTime)
            {
                eatWeaponTimeCurrent += Time.deltaTime;
            }
            else
            {
                if (activeWeapon && !activeWeapon.hidden && !activeWeapon.reloading && activeWeapon.canAct)
                {
                    if (activeWeapon && activeWeapon.reloading)
                        activeWeapon.StopReloading();

                    canEatWeapon = false;
                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    {
                        GLNetworkWrapper.instance.localPlayer.EatWeapon();
                    }
                    StartCoroutine(EatWeaponOverTime());
                    eatCooldown = eatCooldownMax;
                }      
            }
        }
    }

    void DropToolInput()
    {
        // add drop weapon and item here later
        if (KeyBindingManager.GetKeyUp(KeyAction.Drop) ||
            ((Math.Abs(Input.GetAxisRaw(dropString)) < 0.1f ||
              Math.Abs(Input.GetAxisRaw(dropString)) < 0.1f) &&
             dPadPreviousFrameY < -0.1f))
        {
            if (!usingTool && dropWeaponTimeCurrent < dropWeaponTime
                           && ic.objectInHands == null && (activeWeapon == null || activeWeapon.canAct) 
                           && il.savedTools[currentToolIndex].amount > 0)
            {
                dropCooldown = dropCooldownMax;
                StartCoroutine(DropTool());

                dropWeaponTimeCurrent = 0;
            }
        
            dropWeaponTimeCurrent = 0;  
        }
    }

    IEnumerator DropTool()
    {
        il = ItemsList.instance;
        
        //and instantiate it
        
        if (GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false)
        {
            // solo
            Instantiate(il.savedTools[currentToolIndex].toolController, pm.portableTransform.position,
                Quaternion.identity);   
        }
        else
        {
            // coop
            GLNetworkWrapper.instance.DropTool(currentToolIndex, pm.portableTransform.position);
        }
        
        yield return null;
        
        il.savedTools[currentToolIndex].amount--;
        GiveToolToNpc();
    }
    
    void DropWeaponInput()
    {
        // add drop weapon and item here later
        //if (Input.GetButtonDown(useToolString) || KeyBindingManager.GetKeyDown(KeyAction.UseTool))
        if (KeyBindingManager.GetKeyDown(KeyAction.Drop))
        {
            dropWeaponTimeCurrent = 0;
            canEatWeapon = true;
        }
        
        //if (canEatWeapon && (Input.GetButton(useToolString) || KeyBindingManager.GetKey(KeyAction.UseTool) || dPadPreviousFrameY < -0.1f))
        if (canEatWeapon && (KeyBindingManager.GetKey(KeyAction.Drop) || dPadPreviousFrameY < -0.1f))
        {
            if (dropWeaponTimeCurrent < dropWeaponTime)
            {
                dropWeaponTimeCurrent += Time.deltaTime;
            }
            else
            {
                if (activeWeapon && !activeWeapon.hidden && !activeWeapon.reloading && activeWeapon.canAct)
                {
                    dropCooldown = dropCooldownMax;
                    if (activeWeapon && activeWeapon.reloading)
                        activeWeapon.StopReloading();

                    canEatWeapon = false;
                    var newWeapon = activeWeapon;
                    activeWeapon = null;
                    
                    DropWeapon(newWeapon);
                    
                    if (secondWeapon != null)
                        StartCoroutine(SwitchWeaponOverTime());
                }      
            }
        }
    }
    
    IEnumerator EatWeaponOverTime()
    {
        eatingWeapon = true;
        activeWeapon.canAct = false;
        activeWeapon.weaponMovementAnim.SetTrigger(eatWeaponTrigger);
        pm.crosshairAnimator.SetTrigger(eatWeaponTrigger);
        yield return new WaitForSeconds(0.25f);

        if (psc.wallEater) AiDirector.instance.BreakClosestWall(false, 16f);
        
        PlayerAudioController.instance.PlayHeal();
        ui.EatWeapon();
        // give effects here
        pm.hc.PlayerAteWeapon(activeWeapon.durability / activeWeapon.durabilityMax, activeWeapon.effectsOnAttack);
        
        yield return new WaitForSeconds(0.75f);
        
        //destroy weapon
        //eatWeaponTimeCurrent = 0;
        RemoveWeapon(0);
    }
    
    IEnumerator TorchLoseDurability()
    {
        while (true)
        {
            if (pm.controller.enabled && activeWeapon && activeWeapon.weapon == WeaponPickUp.Weapon.Torch && !activeWeapon.hidden && activeWeapon.durability > 0)
            {
                activeWeapon.LoseDurabilityEverySecond();
            }
            yield return new WaitForSeconds(1);
        }
    }

    public bool AlreadyHaveThisWeapon(WeaponPickUp.Weapon weapon)
    {
        return (activeWeapon && activeWeapon.weapon == weapon) || (secondWeapon && secondWeapon.weapon == weapon);
    }
    
    
    void ItemInHands()
    {
        if (activeWeapon)
        {
            if (ic.objectInHands)
            {
                activeWeapon.weaponMovementAnim.SetBool(Hide, true);
            }
            else
                activeWeapon.weaponMovementAnim.SetBool(Hide, false);
        }
    }

    void AttackYourself()
    {
        if (!usingTool && activeWeapon && ic.objectInHands == null)
        {
            //if (Input.GetButtonDown(selfAttackString) || KeyBindingManager.GetKeyDown(KeyAction.SelfAttack))
            if (KeyBindingManager.GetKeyDown(KeyAction.SelfAttack))
            {
                ui.HitYourself();
                if (activeWeapon.durability > 0)
                    activeWeapon.AttackSelf();
                else
                {
                    if (!activeWeapon.broken)
                    {
                        activeWeapon.Broke();
                    }
                    ui.WeaponBroke(activeWeapon);   
                }
            }
        }
    }
    
    void Attacking()
    {
        // if player holds something - throw it away first
        //if ((Input.GetButtonDown(attackString)  || Input.GetAxis(attackString) > 0.5f || KeyBindingManager.GetKeyDown(KeyAction.Fire1)) && ic.objectInHands)
        if ((Input.GetAxis(attackString) > 0.5f || KeyBindingManager.GetKeyDown(KeyAction.Fire1)) && ic.objectInHands)
        {
            ic.objectInHands.Throw();
        }
        else // and only than attack
        {
            if (!usingTool && activeWeapon)
            {
                if (!activeWeapon.hidden)
                {
                    //if ((Input.GetButtonDown(attackString) || Input.GetAxis(attackString) > 0.5f || KeyBindingManager.GetKeyDown(KeyAction.Fire1)) && activeWeapon.weaponType != WeaponController.Type.RangeAuto)
                    if ((Input.GetAxis(attackString) > 0.5f || KeyBindingManager.GetKeyDown(KeyAction.Fire1)) && activeWeapon.weaponType != WeaponController.Type.RangeAuto)
                    {
                        if (pm.hc.inLove) // if player in love
                        {
                            ui.InLove();
                        }
                        else if (!ic.objectInHands)
                        {
                            if (psc.dashOnAttack) pm.Dash();
                            
                            if (activeWeapon.durability > 0)
                                activeWeapon.Attack();
                            else
                            {
                                if (!activeWeapon.broken)
                                {
                                    activeWeapon.Broke();
                                }
                                ui.WeaponBroke(activeWeapon);   
                            }   
                        }
                    }
                    //else if ((Input.GetButton(attackString) || Input.GetAxis(attackString) > 0.5f || KeyBindingManager.GetKey(KeyAction.Fire1)) && activeWeapon.weaponType == WeaponController.Type.RangeAuto)
                    else if ((Input.GetAxis(attackString) > 0.5f || KeyBindingManager.GetKey(KeyAction.Fire1)) && activeWeapon.weaponType == WeaponController.Type.RangeAuto)
                    {
                        if (pm.hc.inLove) // if player in love
                        {
                            ui.InLove();
                        }
                        else if (!ic.objectInHands)
                        {
                            if (psc.dashOnAttack) pm.Dash();
                            if (activeWeapon.durability > 0)
                                activeWeapon.Attack();
                            else
                            {
                                if (!activeWeapon.broken)
                                {
                                    activeWeapon.Broke();
                                }

                                ui.WeaponBroke(activeWeapon);
                            }
                        }
                    } 
                }
                else
                {
                    // show weapon
                    //if (Input.GetButtonDown(attackString) || Input.GetAxis(attackString) > 0.5f || KeyBindingManager.GetKeyDown(KeyAction.Fire1))
                    if (Input.GetAxis(attackString) > 0.5f || KeyBindingManager.GetKeyDown(KeyAction.Fire1))
                    {
                        if (ic.objectInHands)
                        {
                            ic.objectInHands.Throw();
                        }
                        else
                        {
                            ShowWeapon();
                        }
                    }
                }
            }
        }
    }


    public void GiveToolToNpc()
    {
        pm.crosshairAnimator.SetTrigger("ThrowTool");  
        if (activeWeapon)
            StartCoroutine(activeWeapon.UseTool(1));
        
        FindNewTool();
    }
    public void RemoveWeapon(int weaponIndex) // 0 - main 1 - second
    {
        GameObject weaponToDropObject;
        if (weaponIndex == 0)
        {
            (weaponToDropObject = activeWeapon.gameObject).SetActive(false);
            il.activeWeapon = WeaponPickUp.Weapon.Null;
            activeWeapon = null;
        }
        else
        {
            (weaponToDropObject = secondWeapon.gameObject).SetActive(false);
            il.secondWeapon = WeaponPickUp.Weapon.Null;
            secondWeapon = null;
        }
        
        Destroy(weaponToDropObject);
        ui.UpdateAmmo();
        ui.UpdateWeapons();
        if (secondWeapon)
            StartCoroutine(SwitchWeaponOverTime());
        else 
            eatingWeapon = false;
        if (Time.timeScale < 1 && !gm.paused && !gm.mementoWindow)
            Time.timeScale = 1;
    }

    void ThrowTool()
    {
        /*
        if (!usingTool && (Input.GetButtonDown(throwToolString) || 
                            Input.GetAxisRaw(throwToolString) > 0 || 
                            Input.GetAxisRaw(useToolString) > 0 || 
                           KeyBindingManager.GetKeyDown(KeyAction.ThrowTool)) 
                       && ic.objectInHands == null && (activeWeapon == null || activeWeapon.canAct) 
                       && il.savedTools[currentToolIndex].amount > 0)
                       */
            
        if (!usingTool && (Input.GetAxisRaw(throwToolString) > 0 || 
                           Input.GetAxisRaw(useToolString) > 0 || 
                           KeyBindingManager.GetKeyDown(KeyAction.ThrowTool)) 
                       && ic.objectInHands == null && (activeWeapon == null || activeWeapon.canAct) 
                       && il.savedTools[currentToolIndex].amount > 0)
        {
            ui.ThrowTool(currentToolIndex);
            StartCoroutine(ThrowingTool());
            allTools[currentToolIndex].ThrowTool();
            //CheckIfToolIsNull();

            pm.crosshairAnimator.SetTrigger("ThrowTool");
        
            if (activeWeapon)
                StartCoroutine(activeWeapon.UseTool(1));   
        }
    }

    IEnumerator UsingTool()
    {
        for (int i = 0; i < allTools.Count; i++)
        {
            if (i != currentToolIndex) 
                allTools[i].gameObject.SetActive(false);
            else
                allTools[i].gameObject.SetActive(true);
        }
        usingTool = true;
        if (activeWeapon && activeWeapon.reloading)
            activeWeapon.StopReloading();
        
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.EatTool();
        }
        
        yield return new WaitForSeconds(1f);
        
        allTools[currentToolIndex].UseTool();
        if (psc.wallEater) AiDirector.instance.BreakClosestWall(false, 16f);
        
        yield return new WaitForSeconds(1f);
        //CheckIfToolIsNull();
        usingTool = false;
    }
    
    IEnumerator ThrowingTool()
    {
        for (int i = 0; i < allTools.Count; i++)
        {
            if (i != currentToolIndex) 
                allTools[i].gameObject.SetActive(false);
            else
                allTools[i].gameObject.SetActive(true);
        }
        usingTool = true;
        if (activeWeapon && activeWeapon.reloading)
            activeWeapon.StopReloading();
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.localPlayer.ThrowTool();
        }
        
        yield return new WaitForSeconds(1f);
        //CheckIfToolIsNull();
        usingTool = false;
    }

    public void FindNewTool()
    {
        gm = GameManager.instance;
        
        bool found = false;
        for (int i = 0; i < il.savedTools.Count; i++)
        {
            if (il.savedTools[i].amount > 0)
            {
                currentToolIndex = i;
                found = true;
                break;
            }
        }
        
        for (int i = 0; i < allTools.Count; i++)
        {
            if (!gm.hub)
            {
                if (found && i == currentToolIndex)
                {
                        allTools[i].gameObject.SetActive(true);
                }
                else 
                    allTools[i].gameObject.SetActive(false);   
            }
            else
            {
                allTools[i].gameObject.SetActive(false);
            }
        }
        
        if (il.savedTools.Count > 0 && il.savedTools[currentToolIndex].amount <= 0)
            allTools[currentToolIndex].gameObject.SetActive(false);
        
        CheckIfToolIsNull();
        ui = UiManager.instance;
        ui.UpdateTools();
    }
    
    void Reloading()
    {
        //if ((Input.GetButtonDown(reloadString) || KeyBindingManager.GetKeyDown(KeyAction.Reload)) && ic.objectInHands == null)
        if (KeyBindingManager.GetKeyDown(KeyAction.Reload) && ic.objectInHands == null)
        {
            if (!activeWeapon || activeWeapon.hidden) return;
            if (!(activeWeapon.durability > 0) || !activeWeapon.canAct ||
                activeWeapon.ammoClip >= activeWeapon.ammoClipMax) return;
            if (!ui.playerReloaded)
                ui.playerReloaded = true;

            activeWeapon.Reload();   
        }
    }

    public void WeaponBroke(WeaponController w)
    {
        if (w == activeWeapon && activeWeapon.durability <= 0 && !alreadyBroke)
        {
            // broke effect
            StartCoroutine(BrokeWeaponOverTime());
        }
    }

    IEnumerator BrokeWeaponOverTime()
    {
        alreadyBroke = true;
        ui.WeaponBroke(activeWeapon);
        
        pac.PlayBrokehWeapon();

        // remove ammo from broken weapon
        if (activeWeapon.weaponType != WeaponController.Type.Melee)
        {
            float newAmmo = activeWeapon.ammoClip;
            activeWeapon.ammoClip = 0;
            il.ammoDataStorage.AddAmmo(activeWeapon.weapon, newAmmo);   
        }
        
        yield return new WaitForSeconds(0.5f);
        
        ui.UpdateWeapons();
        ui.UpdateAmmo();
        il.SaveWeapons();

        yield return new WaitForSeconds(0.5f);
        
        alreadyBroke = false;
        
    }


    void HideWeapon()
    {
        //if (Input.GetButton(switchWeaponString) || KeyBindingManager.GetKey(KeyAction.SwitchWeapon))
        if (KeyBindingManager.GetKey(KeyAction.SwitchWeapon))
        {
            if (hideWeaponTimeCurrent < hideWeaponTime)
            {
                hideWeaponTimeCurrent += Time.deltaTime;
            }
            else
            {
                if (activeWeapon && !activeWeapon.hidden && !activeWeapon.reloading && activeWeapon.canAct)
                {
                    if (activeWeapon && activeWeapon.reloading)
                        activeWeapon.StopReloading();
        
                    StartCoroutine(HideWeaponOverTime());
                }      
            }
        }

        //if ((Input.GetButtonUp(switchWeaponString) || KeyBindingManager.GetKeyUp(KeyAction.SwitchWeapon)) && hideWeaponTimeCurrent < hideWeaponTime && activeWeapon && activeWeapon.hidden)
        if (KeyBindingManager.GetKeyUp(KeyAction.SwitchWeapon) && hideWeaponTimeCurrent < hideWeaponTime && activeWeapon && activeWeapon.hidden)
        {
            ShowWeapon();
        }
    }
    
    void ShowWeapon()
    {
        if (activeWeapon && activeWeapon.hidden)
        {
            StartCoroutine(ShowWeaponOverTime());
        }
    }
    
    void SwitchWeapon()
    {
        //if (Input.GetButtonDown(switchWeaponString) || KeyBindingManager.GetKeyDown(KeyAction.SwitchWeapon))
        if (KeyBindingManager.GetKeyDown(KeyAction.SwitchWeapon))
        {
            hideWeaponTimeCurrent = 0;
        }
        //if (Input.GetButtonUp(switchWeaponString) || KeyBindingManager.GetKeyUp(KeyAction.SwitchWeapon) && hideWeaponTimeCurrent < hideWeaponTime)
        if (KeyBindingManager.GetKeyUp(KeyAction.SwitchWeapon) && hideWeaponTimeCurrent < hideWeaponTime)
        {
            if (secondWeapon && activeWeapon && !activeWeapon.reloading && activeWeapon.canAct && ic.objectInHands == null)
            {
                if (activeWeapon && activeWeapon.reloading)
                    activeWeapon.StopReloading();
            
                if (!ui.playerSwitchedWeapon)
                    ui.playerSwitchedWeapon = true;
                
                StartCoroutine(SwitchWeaponOverTime());
            }
            else if (!activeWeapon && secondWeapon)
            {
                if (!ui.playerSwitchedWeapon)
                    ui.playerSwitchedWeapon = true;
                StartCoroutine(SwitchWeaponOverTime());
            }
        }
    }

    IEnumerator HideWeaponOverTime()
    {
        crosshair.SetTrigger("Switch");
        activeWeapon.canAct = false;
        activeWeapon.hidden = true;

        pac.PlaySwitchWeapon();

        yield return new WaitForSeconds(0.5f);

        activeWeapon.gameObject.SetActive(false);

        ui.UpdateWeapons();
        il.SaveWeapons();
        activeWeapon.playerMovement.mouseLook.activeWeaponHolderAnim = activeWeapon.weaponMovementAnim;

        yield return new WaitForSeconds(0.5f);
        activeWeapon.canAct = true;
    
        il.activeWeaponDescriptions = new List<string>(activeWeapon.descriptions);
        hideWeaponTimeCurrent = 0;   
    }

    IEnumerator ShowWeaponOverTime()
    {
        crosshair.SetTrigger("Switch");
        activeWeapon.canAct = false;

        pac.PlaySwitchWeapon();

        yield return new WaitForSeconds(0.5f);

        activeWeapon.gameObject.SetActive(true);

        ui.UpdateWeapons();
        il.SaveWeapons();
        activeWeapon.playerMovement.mouseLook.activeWeaponHolderAnim = activeWeapon.weaponMovementAnim;

        WeaponPhrase();
        yield return new WaitForSeconds(0.5f);
        activeWeapon.hidden = false;
        activeWeapon.canAct = true;
        
        il.activeWeaponDescriptions = new List<string>(activeWeapon.descriptions);
    }

    private string activeString = "Active";
    void WeaponPhrase()
    {
        if (!activeWeapon || !activeWeapon.randomPhrases) return;
        
        ui = UiManager.instance;
        var tempDialogue = activeWeapon.randomPhrases.dialogues[Random.Range(0, activeWeapon.randomPhrases.dialogues.Count)];
        ui.dialogueSpeakerName.text = activeWeapon.dataRandomizer.generatedName[gm.language];
        ui.dialoguePhrase.text = tempDialogue.phrases[Random.Range(0, tempDialogue.phrases.Count)];
        ui.dialogueChoice.text = String.Empty;
        ui.dialogueAnim.SetTrigger(activeString);
        ui.HideDialogue(4);
    }
    
    IEnumerator SwitchWeaponOverTime()
    {
        crosshair.SetTrigger("Switch"); 
        if (activeWeapon)
            activeWeapon.canAct = false;
        
        if (secondWeapon)
            secondWeapon.canAct = false;
        pac.PlaySwitchWeapon();

        yield return new WaitForSeconds(0.5f);
        if (activeWeapon)
            activeWeapon.gameObject.SetActive(false);
        if (secondWeapon)
            secondWeapon.gameObject.SetActive(true);
        WeaponController tempWeapon = null;
        if (activeWeapon)
            tempWeapon = activeWeapon;
        
        activeWeapon = secondWeapon;
        
        if (activeWeapon)
            secondWeapon = tempWeapon;

        ui.UpdateWeapons();
        il.SaveWeapons();
        
        if (activeWeapon)
            activeWeapon.playerMovement.mouseLook.activeWeaponHolderAnim = activeWeapon.weaponMovementAnim;

        UiManager.instance.UpdateAmmo();
        yield return new WaitForSeconds(0.5f);
        
        if (activeWeapon)
        {
            activeWeapon.hidden = false;
            activeWeapon.canAct = true;   
            activeWeapon.weaponMovementAnim.SetBool("Broken", activeWeapon.broken);
            
            il.activeWeaponDescriptions = new List<string>(activeWeapon.descriptions);
        }

        if (secondWeapon)
        {
            secondWeapon.canAct = true;
            il.secondWeaponDescriptions = new List<string>(secondWeapon.descriptions);   
        }
        WeaponPhrase();
        eatingWeapon = false;
        ui.UpdateWeapons();
    }

    private IEnumerator TakeWeapon(WeaponController newWeapon, WeaponPickUp weaponPickUp)
    {
        if (activeWeapon && activeWeapon.reloading)
            activeWeapon.StopReloading();

        crosshair.SetTrigger("Switch");
        if (activeWeapon)
         activeWeapon.canAct = false;
        if (secondWeapon)
         secondWeapon.canAct = false;

        yield return new WaitForSeconds(0.5f);

        switch (weaponPickUpAction)
        {
            case WeaponPickUpAction.TakeToActiveSlot:
                TakeToActiveSlot(newWeapon, weaponPickUp);
                break;
            
            case WeaponPickUpAction.TakeToSecondSlot:
                if(!activeWeapon) 
                    TakeToActiveSlot(newWeapon, weaponPickUp);
                else
                    TakeToSecondSlot(newWeapon, weaponPickUp);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        WeaponPhrase();
        yield return new WaitForSeconds(0.5f);
        activeWeapon.weaponMovementAnim.SetBool("Broken", activeWeapon.broken);
        activeWeapon.canAct = true;

        il.activeWeaponDescriptions = new List<string>(activeWeapon.descriptions);

        if (secondWeapon)
        {
            secondWeapon.canAct = true;
            il.secondWeaponDescriptions = new List<string>(secondWeapon.descriptions);
        }

        il.SaveWeapons();

        ui.UpdateWeapons();
        ui.UpdateAmmo();
        activeWeapon.playerMovement.mouseLook.activeWeaponHolderAnim = activeWeapon.weaponMovementAnim;
    }

    private void TakeToSecondSlot(WeaponController newWeapon, WeaponPickUp weaponPickUp)
    {
        if (secondWeapon)
            DropWeapon(secondWeapon);
        
        secondWeapon = newWeapon;
        if (!weaponPickUp.useBaseWeaponDurability)
            secondWeapon.durability = weaponPickUp.durability;
        if (!weaponPickUp.hasFullAmmo)
        {
            var ammoThatCanBeRestored =
                Math.Min(il.ammoDataStorage.GetAmmoCount(secondWeapon.weapon), weaponPickUp.ammo);
            il.ammoDataStorage.ReduceAmmo(secondWeapon.weapon, ammoThatCanBeRestored);
            secondWeapon.ammoClip = ammoThatCanBeRestored;
        }
        else
            secondWeapon.ammoClip = secondWeapon.ammoClipMax;
    }

    private void TakeToActiveSlot(WeaponController newWeapon, WeaponPickUp weaponPickUp)
    {
        if (activeWeapon)
        {
            if (secondWeapon) // drop active Weapon
                DropWeapon(activeWeapon);
            else
            {
                secondWeapon = activeWeapon;
                secondWeapon.gameObject.SetActive(false);
            }
        }

        activeWeapon = newWeapon;
        if (!weaponPickUp.useBaseWeaponDurability)
            activeWeapon.durability = weaponPickUp.durability;
        if (!weaponPickUp.hasFullAmmo)
        {
            var ammoThatCanBeRestored =
                Math.Min(il.ammoDataStorage.GetAmmoCount(activeWeapon.weapon), weaponPickUp.ammo);
            il.ammoDataStorage.ReduceAmmo(activeWeapon.weapon, ammoThatCanBeRestored);
            activeWeapon.ammoClip = ammoThatCanBeRestored;
        }
        else
            activeWeapon.ammoClip = activeWeapon.ammoClipMax;

        activeWeapon.gameObject.SetActive(true);
        activeWeapon.weaponMovementAnim.SetBool("Broken", activeWeapon.broken);
    }

    private void DropWeapon(WeaponController w)
    {
        int weaponEnumIndex = (int)w.weapon;
        var pos = transform.position + Vector3.up * 2 + pm.cameraAnimator.transform.forward * 1.25f;
        List<int> barrels1 = new List<int>();
        List<int> barrels2 = new List<int>();
        Vector3 eulerAngles = Vector3.zero;
        bool crazyGun = false;
        float clipSize = 0;
        int barrelsCount = 0;

        if (w.weaponConnector)
        {
            barrels1 = w.weaponConnector.savedBarrelslvl_1;
            barrels2 = w.weaponConnector.savedBarrelslvl_2;
            eulerAngles = w.weaponConnector.transform.localRotation.eulerAngles;
            crazyGun = w.weaponConnector.crazyGun;
            clipSize = w.weaponConnector.clipSize;
            barrelsCount = w.weaponConnector.barrelsCount;
        }
        
        // SYNC HERE
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.DropWeapon(weaponEnumIndex, w.ammoClip, w.durability, w.dataRandomizer.generatedName, w.dataRandomizer.generatedDescriptions, w.dataRandomizer.r1, w.dataRandomizer.r2, w.dataRandomizer.r3, w.dataRandomizer.r4,
                w.dataRandomizer.statusEffect, w.dataRandomizer.dead, w.dataRandomizer.npc, w.dataRandomizer.npcPaid, barrels1, barrels2, eulerAngles,
                crazyGun, clipSize, barrelsCount, pos);
        }
        else
        {
            DropWeaponOnClient(weaponEnumIndex, w.ammoClip, w.durability, w.dataRandomizer.generatedName, w.dataRandomizer.generatedDescriptions, w.dataRandomizer.r1, w.dataRandomizer.r2, w.dataRandomizer.r3, w.dataRandomizer.r4,
                w.dataRandomizer.statusEffect, w.dataRandomizer.dead, w.dataRandomizer.npc, w.dataRandomizer.npcPaid, barrels1, barrels2, eulerAngles,
                crazyGun, clipSize, barrelsCount, pos);
        }
        
        GameObject weaponToDropObject;
        (weaponToDropObject = w.gameObject).SetActive(false);
        Destroy(weaponToDropObject);
        ui.UpdateWeapons();
        ui.UpdateAmmo();  
    }

    public void DropWeaponOnClient(int weaponEnumIndex, float ammoClip, float durability, List<string> generatedName,List<string> generatedDescriptions, int r1, int r2, int r3, int r4, int statusEffect, bool dead, bool npc, bool npcPaid,
                                    List<int> savedBarrelslvl_1, List<int> savedBarrelslvl_2, Vector3 eulerAngles, bool crazyGun, float clipSize, int barrelsCount, Vector3 pos)
    {
        WeaponPickUp.Weapon weaponEnumNew = (WeaponPickUp.Weapon)weaponEnumIndex;
        
        var dropBase = sc.weaponPickUpPrefabs
            .Single(drop => drop.weaponPickUp.weapon == weaponEnumNew).weaponPickUp;
        
        var weaponToDrop = Instantiate(dropBase, pos, pm.cameraAnimator.transform.rotation);
        weaponToDrop.useBaseWeaponDurability = false;
        weaponToDrop.hasFullAmmo = false;
        weaponToDrop.interactable.ChangeMarkerToCross();

        // only on player who dropped it
        il.ammoDataStorage.AddAmmo(weaponEnumNew, ammoClip);
        
        weaponToDrop.durability = durability;
        weaponToDrop.ammo = ammoClip;
        weaponToDrop.weaponDataRandomier.NewDescription(generatedName, generatedDescriptions, 
            r1, r2, r3, r4,
            statusEffect, dead, npc, npcPaid);

        if (weaponToDrop.weaponConnector)
        {
            //weaponToDrop.weaponConnector.DropWeapon(w);
            weaponToDrop.weaponConnector.DropWeapon(savedBarrelslvl_1, savedBarrelslvl_2, eulerAngles, crazyGun, clipSize, barrelsCount);
        }
    }

    public void PickUpWeapon(WeaponPickUp weaponPickUp)
    {
        var baseController = weaponList.Single(w => w.weapon == weaponPickUp.weapon);
        var baseTransform = baseController.transform;
        var pickedWeapon = Instantiate(baseController, baseTransform.position, baseTransform.rotation);
        pickedWeapon.transform.parent = baseController.transform.parent;
        pickedWeapon.transform.localScale = Vector3.one;

        pickedWeapon.dataRandomizer.NewDescription(weaponPickUp.weaponDataRandomier.generatedName, weaponPickUp.weaponDataRandomier.generatedDescriptions, weaponPickUp.weaponDataRandomier.r1, weaponPickUp.weaponDataRandomier.r2, weaponPickUp.weaponDataRandomier.r3, weaponPickUp.weaponDataRandomier.r4, weaponPickUp.weaponDataRandomier.statusEffect, weaponPickUp.weaponDataRandomier.dead, weaponPickUp.weaponDataRandomier.npc, weaponPickUp.weaponDataRandomier.npcPaid);
        
        if (pickedWeapon.weaponConnector)
            pickedWeapon.weaponConnector.PickWeapon(weaponPickUp);
        StartCoroutine(TakeWeapon(pickedWeapon, weaponPickUp));
    }

    void ChangeTool()
    {
        if (!usingTool && changeToolCooldown <= 0)
        {
            if (il.savedTools.Count > 0)
            {
                if (Input.GetAxis(mouseWheelString) > 0)
                {
                    for (int i = 0; i < il.savedTools.Count; i++) 
                    {
                        currentToolIndex--;

                        if (currentToolIndex < 0)
                            currentToolIndex = allTools.Count - 1;

                        if (il.savedTools[currentToolIndex].amount > 0)
                        {
                            break;
                        }

                        ui.ChangeToolHint();
                    }
                    for (int i = 0; i < allTools.Count; i++)
                    {
                        if (i == currentToolIndex)
                        {
                            changeToolCooldown = 0.1f;
                            allTools[i].gameObject.SetActive(true);   
                        }
                        else 
                            allTools[i].gameObject.SetActive(false);
                    }
                    CheckIfToolIsNull();
                    ui.UpdateTools();
                }
        
                if (Input.GetAxis(mouseWheelString) < 0)
                {
                    for (int i = 0; i < il.savedTools.Count; i++) 
                    {
                        currentToolIndex++;

                        if (currentToolIndex >= il.savedTools.Count)
                            currentToolIndex = 0;

                        if (il.savedTools[currentToolIndex].amount > 0)
                        {
                            break;
                        }
                        
                        ui.ChangeToolHint();
                    }
                    for (int i = 0; i < allTools.Count; i++)
                    {
                        if (i == currentToolIndex)
                        {
                            changeToolCooldown = 0.1f;
                            allTools[i].gameObject.SetActive(true);   
                        }
                        else 
                            allTools[i].gameObject.SetActive(false);
                    }

                    CheckIfToolIsNull();
                    ui.UpdateTools();
                }      
            }
        }
    }

    void CheckIfToolIsNull()
    {
        if (il.savedTools.Count > 0)
        {
            if (il.savedTools[currentToolIndex].amount <= 0)
                allTools[currentToolIndex].gameObject.SetActive(false);
            else
                allTools[currentToolIndex].gameObject.SetActive(true);   
        }
        else
        {
            for (var index = 0; index < allTools.Count; index++)
            {
                var t = allTools[index];
                t.gameObject.SetActive(false);
            }
        }
    }

    public void MakeWeaponsEvil()
    {
        if (activeWeapon)
            activeWeapon.dataRandomizer.RandomStatusEffect(false);
        if (secondWeapon)
            secondWeapon.dataRandomizer.RandomStatusEffect(false);
        
        /*
        if (activeWeapon != null)
        {
            activeWeapon.dataRandomizer.GenerateOnSpawn(false, true);
        }
        if (secondWeapon != null)
        {
            secondWeapon.dataRandomizer.GenerateOnSpawn(false, true);
        }*/
    }

    public void SetWeaponsEffectByIndex(int index)
    {
        if (activeWeapon)
            activeWeapon.dataRandomizer.SetStatusEffect(index);
        if (secondWeapon)
            secondWeapon.dataRandomizer.SetStatusEffect(index);
    }

    public void RepairWeapons()
    {
        if (activeWeapon)
            activeWeapon.FixWeapon();
        if (secondWeapon)
            activeWeapon.FixWeapon();
        
        il.SaveWeapons();
        ui.UpdateWeapons();
    }
    public void RandomizeWeaponsStatuses()
    {
        if (activeWeapon)
            activeWeapon.dataRandomizer.RandomStatusEffect(false);
        if (secondWeapon)
            activeWeapon.dataRandomizer.RandomStatusEffect(false);
        
        il.SaveWeapons();
        ui.UpdateWeapons();
    }
}