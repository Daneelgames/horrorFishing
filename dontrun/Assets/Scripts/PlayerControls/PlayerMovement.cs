using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerControls
{
    public class PlayerMovement : MonoBehaviour
    {
        public static PlayerMovement instance;

        private float _dashCooldownCurrent = 0;

        public Transform playerHead;
        public Animator goldenLightAnimator;
        public Animator cameraAnimator;
        public Animator crosshairAnimator;
        public CharacterController controller;
        
        public float _x = 0;
        public float _z = 0;

        [Header("Noise")] 
        public float walkNoiseDistance = 5;
        public float runNoiseDistance = 20;
        public float dashNoiseDistance = 10;
        
        public int dashDamage = 100;
        public float dashDamageSelf = 50;
        public float _dashTimeCurrent = 1;

        private float _currentCameraJiggle = 0;
        private float _cameraChangeSpeed = 2;

        private UiManager _uiManager;
        private GameManager _gameManager;
        private InteractionController _interactionController;
        private StaminaController _staminaController;
        
        public PlayerMovementStats movementStats;
        public StaminaStats staminaStats;

        [HideInInspector]
        public float currentCrosshairJiggle = 0;

        private float _crosshairChangeSpeed = 0.1f;
        private Vector3 _move;

        private Vector3 _velocity;
        [SerializeField] private float gravity = -3f;
        public Transform groundCheck;
        public bool fallingInHole = false;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;
    
        [SerializeField] private Transform camHolder;

        public MouseLook mouseLook;
        public InteractionController interactionController;

        public bool _grounded = false;
        [HideInInspector]
        public HealthController hc;

        public Collider dashCollider;
        public CapsuleCollider damagecollider;
        private bool dangerousDash = false;
        public bool crouching = false;
        public TransportObject inTransport;
        
        public Transform movementTransform;
        public PlayerAudioController playerAudio;
        public Collider doorCollider;

        public bool teleport = false; 

        public Transform portableTransform;

        private PlayerSkillsController psc;

        public WallBlockerController wallBlocker;

        private string speedString = "Speed";
        private string dashString = "Dash";
        private string crouchString = "Crouch";
        private string runningString = "Run";
        private string runningStringWeapon = "Running";
        private string jiggleString = "Jiggle";
        private string horizontalString = "Horizontal";
        private string verticalString = "Vertical";

        bool ableToChooseRespawn = false;
        bool adrenaline = false;
        public float coldScaler = 1;
        private Rigidbody rb;

        public float weightSpeedScaler = 1;
        
        private void Awake()
        {
            instance = this;
            controller.enabled = false;

            GameManager.instance.player = hc;
        }

        private void Start()
        {
            if (TwitchManager.instance)
            {
                TwitchManager.instance.ToggleCanvasAnim(false);
                TwitchManager.instance.votingAnim.SetBool("Active", false);
            }
            Time.timeScale = 1;
            rb = GetComponent<Rigidbody>();
            _gameManager = GameManager.instance;
            _uiManager = UiManager.instance;
            _interactionController = InteractionController.instance;
            movementStats.currentMoveSpeed = 0;
            _dashTimeCurrent = movementStats.dashTime;
            psc = PlayerSkillsController.instance;
            
            dashCollider.enabled = false;
            
            playerHead.parent = null;

            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                instance.goldenLightAnimator.SetBool("GoldenLight", false);   
                _uiManager.Init(true);
            }
        }

        public void UpdateWeightSpeedScaler(int itemsAmount)
        {
            if (itemsAmount <= 2)
                weightSpeedScaler = 1;
            else if (itemsAmount < 4)
                weightSpeedScaler = 0.95f;
            else if (itemsAmount < 6)
                weightSpeedScaler = 0.90f;
            else if (itemsAmount < 8)
                weightSpeedScaler = 0.80f;
            else if (itemsAmount < 10)
                weightSpeedScaler = 0.6f;
            else
                weightSpeedScaler = 0.33f;

            psc = PlayerSkillsController.instance;
            if (psc.steelSpine)
                weightSpeedScaler = Mathf.Clamp(weightSpeedScaler + 0.15f, 0.5f, 1);
        }

        private void Update()
        {
            if (hc.health > 0 && controller.enabled && !inTransport && !_gameManager.paused && !teleport && !_gameManager.questWindow)
            {
                Movement();
            }

            // smooth camera movement when character controller is stepping up
            
            if(!inTransport && !teleport)
                playerHead.position = Vector3.Lerp(playerHead.position, transform.position, 5 * Time.deltaTime);
            else
                playerHead.position = transform.position;   

            if (!teleport && controller.enabled && !inTransport && !_gameManager.paused && !_gameManager.questWindow)
                Gravity();

            if (ableToChooseRespawn)
            {
                if (Input.GetButtonDown(crouchString) || KeyBindingManager.GetKeyDown(KeyAction.Crouch)) // to hub
                {
                    _gameManager.Restart(0);
                }
                /*
                else if (Input.GetButtonDown(dashString) ||KeyBindingManager.GetKeyDown(KeyAction.Dash)) // to level
                {
                    if (_gameManager.wasInHub == 1)
                    {
                        _gameManager.Restart(1);
                    }
                }*/
            }
        }

        public void MoveToMeatBrain(MeatBrainController brain)
        {
            print("player move to meat brain. brain is " + brain);
            if (brain == null)
            {
                GLNetworkWrapper.instance.ToggleSpectatorCameraFromLocalPlayer(false);
                transform.parent = null;
                transform.rotation = Quaternion.identity;
                controller.stepOffset = 1f;
                controller.enabled = true;
                hc.Heal(hc.healthMax / 2);
                hc.Antidote();
                
                mouseLook = MouseLook.instance;
                if (mouseLook.debugMap)
                    mouseLook.debugMap.SetActive(false);
                
                if (WeaponControls.instance.activeWeapon) WeaponControls.instance.activeWeapon.canAct = true;
                
                GLNetworkWrapper.instance.localPlayer.connectedDummy.Resurrect(hc.health);
                return;
            }

            GLNetworkWrapper.instance.ToggleSpectatorCameraFromLocalPlayer(true);

            if (mouseLook.debugMap)
            {
                mouseLook.debugMap.SetActive(true);
                
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    mouseLook.spectatorCam.gameObject.SetActive(true);   
                }
                else
                {
                    mouseLook.spectatorCam.gameObject.SetActive(false);
                }
            }
            
            cameraAnimator.SetFloat(speedString, 0);
            controller.enabled = false;
            transform.parent = brain.transform;
            transform.localPosition = Vector3.zero;
            ToggleCrouch(true);
            
            Time.timeScale = 1;
        }
        
        public void ToggleCrouch(bool c)
        {
            if (crouching != c)
            {
                crouching = c;
                mouseLook.ToggleCrouch(c);

                if (crouching)
                {
                    controller.height = 1;
                    controller.center = Vector3.up * 0.5f;
                    damagecollider.height = 1;
                    damagecollider.center = Vector3.up * 0.5f;
                }
                else
                {
                    controller.height = 2f;
                    controller.center = Vector3.up;
                    damagecollider.height = 2;
                    damagecollider.center = Vector3.up;
                }
                
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    GLNetworkWrapper.instance.PlayerCrouch(crouching);
                }
            }
        }

        public void DisableCameraAnimatorBools()
        {
            cameraAnimator.SetBool("MeleeSwing", false);
            cameraAnimator.SetBool("MeleeAttack", false);
            cameraAnimator.SetBool("MeleeReturn", false);
            
            cameraAnimator.Play("Camera");
        }

        public void Dash()
        {
            if (_dashCooldownCurrent <= 0 && !fallingInHole)
            {
                ToggleCrouch(false);
                wallBlocker = null;

                float staminaBuffScaler = 1f;
                if (hc.rustAndPoisonActive) staminaBuffScaler = 0.5f;
                
                if (!adrenaline)
                    staminaStats.CurrentValue -= staminaStats.dashCostCurrent * staminaBuffScaler;
                else
                    staminaStats.CurrentValue -= staminaStats.dashCostCurrent * staminaBuffScaler * 0.75f;
            
                var right = movementTransform.right;
                var forward = movementTransform.forward;
            
                _move = Mathf.Approximately(_z, 0) && Mathf.Approximately(_x, 0)
                    ? right * 0 + forward * -1
                    : right * _x + forward * _z;

                if (!psc.cheapDash)
                    _dashCooldownCurrent = movementStats.dashCooldown;
                else
                    _dashCooldownCurrent = movementStats.dashCooldown * 0.8f;
                
                cameraAnimator.SetTrigger(dashString);
                _uiManager.playerDashed = true;
                _dashTimeCurrent = 0;

                if (SpawnController.instance)
                {
                    if (!psc.softFeet)
                        SpawnController.instance.MobHearNoise(transform.position, dashNoiseDistance);
                    else
                        SpawnController.instance.MobHearNoise(transform.position, dashNoiseDistance / 2);   
                }
                
                if (staminaStats.CurrentValue >= staminaStats.dashCostCurrent)
                {
                    movementStats.movementState = MovementState.Dashing;
                    playerAudio.PlayDash();
                    psc.PlayerDashed();
                    if (psc.dashAttack)
                    {
                        dangerousDash = true;
                        dashCollider.enabled = true;
                    }
                
                    if (hc.statusEffects[1].effectLevelCurrent > 0) //todo: move to proper class
                        hc.statusEffects[1].effectLevelCurrent -= 25;
                }
                else
                {
                    movementStats.movementState = MovementState.DashingNoStamina;
                    playerAudio.PlayDash(true);
                    if (psc.dashAttack)
                    {
                        dangerousDash = true;
                        dashCollider.enabled = true;
                    }
                    if (hc.statusEffects[1].effectLevelCurrent > 0) //todo: move to proper class
                        hc.statusEffects[1].effectLevelCurrent -=  10;
                }
            }
        }
        
        bool forw = false;
        bool back = false;
        bool right = false;
        bool left = false;
        private float aimingSpeedScaler = 1;
        private void Movement()
        {
            /*
            _x = Input.GetAxisRaw(horizontalString);
            _z = Input.GetAxisRaw(verticalString);
            */
            
            if (mouseLook.aiming) 
                aimingSpeedScaler = 0.3f;
            else 
                aimingSpeedScaler = 1;
            
            forw = KeyBindingManager.GetKey(KeyAction.Forward);
            back = KeyBindingManager.GetKey(KeyAction.Backwards);
            right = KeyBindingManager.GetKey(KeyAction.RightStrafe);
            left = KeyBindingManager.GetKey(KeyAction.LeftStrafe);
            
            if (right)
                _x = 1;
            else if (left)
                _x = -1;
            else
                _x = 0;
            
            if (forw)
                _z = 1;
            else if (back)
                _z = -1;
            else
                _z = 0;

            //if ((Input.GetButtonDown(crouchString) || KeyBindingManager.GetKeyDown(KeyAction.Crouch)) && movementStats.movementState != MovementState.Dashing && movementStats.movementState != MovementState.DashingNoStamina)
            if (KeyBindingManager.GetKeyDown(KeyAction.Crouch) && movementStats.movementState != MovementState.Dashing && movementStats.movementState != MovementState.DashingNoStamina)
            {
                ToggleCrouch(!crouching);
            }
            
            if (fallingInHole)
            {
                _x = 0;
                _z = 0;
            }
            
            movementTransform.eulerAngles = new Vector3(0, movementTransform.eulerAngles.y, 0);

            //if (Input.GetButtonDown("Dash") && _dashCooldownCurrent <= 0 && NotAttackingInMelee())
            //if ((Input.GetButtonDown(dashString) || KeyBindingManager.GetKeyDown(KeyAction.Dash)))
            if (KeyBindingManager.GetKeyDown(KeyAction.Dash))
            {
                Dash();
            }

            //Tired if minus stamina
            if (movementStats.movementState != MovementState.Dashing &&
                movementStats.movementState != MovementState.DashingNoStamina &&
                staminaStats.CurrentValue <= 0)
                movementStats.movementState = MovementState.Tired;
            else if (movementStats.movementState != MovementState.Dashing &&
                     movementStats.movementState != MovementState.DashingNoStamina)
                movementStats.movementState = MovementState.Walking;

            float dashTimeScaler = 1;
            if (psc.cheapDash)
                dashTimeScaler = 0.8f;
            
            if (_dashTimeCurrent >= movementStats.dashTime * dashTimeScaler &&
                movementStats.movementState == MovementState.Dashing) // DASH END
            {
                movementStats.movementState = MovementState.Idle;
                dashCollider.enabled = false;
                dangerousDash = false;
            }
            else if (_dashTimeCurrent >= movementStats.dashTimeNoStamina && movementStats.movementState == MovementState.DashingNoStamina) // DASH END
            {
                movementStats.movementState = MovementState.Idle;
                dashCollider.enabled = false;
                dangerousDash = false;
            }
            else
                _dashTimeCurrent += Time.deltaTime;

            _dashCooldownCurrent -= Time.deltaTime;
            if (_dashCooldownCurrent < 0) 
                _dashCooldownCurrent = 0;
            
            // slowed by meat wall coeff
            float inWallCoeff = 1;
            if (wallBlocker)
                inWallCoeff = 0.2f;
            
            // IF DASHING
            if (movementStats.movementState == MovementState.Dashing || movementStats.movementState == MovementState.DashingNoStamina)
            {
                float dashSpeedScaler = 1;
                if (psc.cheapDash) dashSpeedScaler = 1.25f;
                if (wallBlocker && psc.dashAttack)
                    inWallCoeff = 1f;
                
                var dashSpeed = movementStats.movementState == MovementState.Dashing 
                    ? movementStats.dashSpeed * coldScaler * dashSpeedScaler * weightSpeedScaler
                    : movementStats.dashSpeedNoStamina * coldScaler * dashSpeedScaler * weightSpeedScaler;
                
                movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, dashSpeed, Time.deltaTime);
                controller.Move(_move.normalized * inWallCoeff * (movementStats.currentMoveSpeed * Time.deltaTime) * coldScaler * weightSpeedScaler); //Keep this multiply order, its affect performance.
                
                controller.stepOffset = 2f;
                
                
                _uiManager.playerSprinted = true;
            }
            else
            {
                controller.stepOffset = 1f;
                // MOVEMENT DIRECTION
                ////////////////////
                _move = movementTransform.right * _x + movementTransform.forward * _z;

                if(movementStats.movementState == MovementState.Tired)
                    movementStats.movementState = _move.magnitude < 0.3f ? MovementState.Idle : MovementState.Tired;
                else
                    movementStats.movementState = _move.magnitude < 0.3f ? MovementState.Idle : MovementState.Walking;

                var crouchSpeedCoeff = 1f;
                if (crouching) crouchSpeedCoeff = 0.33f;
                
                if (!_grounded) _move /= 100; // FALLING

                if (hc.wc.activeWeapon && hc.wc.activeWeapon.weaponType == WeaponController.Type.Melee && !hc.wc.activeWeapon.canAct) // if attacking
                    controller.Move(_move.normalized * aimingSpeedScaler * inWallCoeff * movementStats.currentMoveSpeed * coldScaler * weightSpeedScaler * crouchSpeedCoeff / 2f * Time.deltaTime);
                else if (_z > 0) // MOVING FORWARD
                {
                    if (!psc.fastStrafing)
                        controller.Move(_move.normalized *  aimingSpeedScaler * inWallCoeff * (movementStats.currentMoveSpeed * coldScaler * weightSpeedScaler * crouchSpeedCoeff * Time.deltaTime)); //Keep this multiply order, its affect performance.
                    else
                        controller.Move(_move.normalized *  aimingSpeedScaler * inWallCoeff * (movementStats.currentMoveSpeed * coldScaler * weightSpeedScaler * crouchSpeedCoeff * 1.25f * Time.deltaTime)); //Keep this multiply order, its affect performance.
                }
                else if (Mathf.Approximately(_z, 0) && !Mathf.Approximately(_x, 0)) // STRAIFING
                {
                    if (!psc.fastStrafing)
                        controller.Move(_move.normalized *  aimingSpeedScaler * inWallCoeff * (movementStats.currentMoveSpeed * coldScaler * weightSpeedScaler * crouchSpeedCoeff * 0.75f * Time.deltaTime));//Keep this multiply order, its affect performance.
                    else
                        controller.Move(_move.normalized *  aimingSpeedScaler * inWallCoeff * (movementStats.currentMoveSpeed * coldScaler * weightSpeedScaler  * crouchSpeedCoeff * 1.25f * Time.deltaTime));//Keep this multiply order, its affect performance.
                }
                //else if (Mathf.Approximately(_z, -1)) // walking && strafing backwards
                else if (_z < 0) // walking && strafing backwards
                {
                    if (!psc.fastStrafing)
                    {
                        controller.Move(_move.normalized *  aimingSpeedScaler * inWallCoeff * movementStats.currentMoveSpeed * coldScaler * weightSpeedScaler  * crouchSpeedCoeff / 2 * Time.deltaTime);
                    }
                    else
                    {
                        controller.Move(_move.normalized *  aimingSpeedScaler * inWallCoeff * (movementStats.currentMoveSpeed * coldScaler * weightSpeedScaler  * crouchSpeedCoeff * 1.25f * Time.deltaTime));//Keep this multiply order, its affect performance.
                    }

                }
                else if (Mathf.Approximately(_z, 0) && Mathf.Approximately(_x, 0)) // to idle
                {
                    movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, 0, Time.deltaTime);
                    controller.Move(_move.normalized * aimingSpeedScaler * inWallCoeff * (movementStats.currentMoveSpeed * coldScaler * weightSpeedScaler  * Time.deltaTime));
                }

                // RUNNING
                //////////
                //if ((Input.GetAxis(runningString) > 0 || KeyBindingManager.GetKey(KeyAction.Run)) && !mouseLook.aiming)
                if (KeyBindingManager.GetKey(KeyAction.Run) && !mouseLook.aiming)
                {
                    ToggleCrouch(false);
                    
                    if ((!psc.fastStrafing && _z > 0) || psc.fastStrafing)
                    {
                        if (staminaStats.CurrentValue >= - staminaStats.runRegenCurrent * Time.deltaTime)
                        {
                            movementStats.isRunning = true;
                            movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, 
                                movementStats.baseMoveSpeed + movementStats.runSpeedBonusCurrent, Time.deltaTime);
                            _uiManager.playerSprinted = true;

                            if (hc.statusEffects[1].effectLevelCurrent > 0) //todo: move to proper class
                                hc.statusEffects[1].effectLevelCurrent -= Time.deltaTime * 15f;
                        }
                        else
                        {
                            movementStats.keepsRunningWithZeroStamina = true;
                            movementStats.movementState = MovementState.Tired;
                            movementStats.isRunning = false;
                            
                            if (!adrenaline)
                                movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, 
                                movementStats.tiredSpeed, Time.deltaTime * 2);
                            else
                                movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, 
                                    movementStats.tiredSpeed * 1.25f, Time.deltaTime * 2);
                        }   
                    }
                }
                else if(movementStats.movementState == MovementState.Tired)
                {
                    movementStats.keepsRunningWithZeroStamina = false;
                    movementStats.isRunning  = false;
                    movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, 
                        movementStats.tiredSpeed, Time.deltaTime * 2);
                }
                else
                {
                    movementStats.keepsRunningWithZeroStamina = false;
                    movementStats.isRunning  = false;
                    movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, 
                        movementStats.baseMoveSpeed, Time.deltaTime * 2);
                }
            }


            if (_move.magnitude > 0) 
                playerAudio.PlaySteps();
            
            // HEARTBEAT
            /////////////
            if (movementStats.isRunning || movementStats.movementState == MovementState.Dashing)
            {
                //RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, gm.fogDensityMax, Time.deltaTime * 0.1f);
                playerAudio.heartbeatSource.volume = Mathf.Lerp(playerAudio.heartbeatSource.volume, 1, Time.deltaTime * 0.1f);
            }
            else
            {
                //RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, gm.fogDensityMin, Time.deltaTime * 0.2f);
                playerAudio.heartbeatSource.volume = Mathf.Lerp(playerAudio.heartbeatSource.volume, 0, Time.deltaTime * 0.1f);
            }
            
            // VISUAL FEEDBACK
            //////////////////

            if (hc.wc.activeWeapon)
            {
                if (!psc.shootOnrun)
                    hc.wc.activeWeapon.weaponMovementAnim.SetBool(runningStringWeapon, movementStats.isRunning); //todo: move to proper class
                else
                {
                    hc.wc.activeWeapon.weaponMovementAnim.SetBool(runningStringWeapon,false);
                }
            }
            
            _currentCameraJiggle = ControlJiggle(_currentCameraJiggle, _cameraChangeSpeed);
            currentCrosshairJiggle = ControlJiggle(currentCrosshairJiggle, _crosshairChangeSpeed);
            cameraAnimator.SetFloat(speedString, _currentCameraJiggle);
            cameraAnimator.speed = 1; //todo: check ???
            crosshairAnimator.SetFloat(jiggleString, currentCrosshairJiggle);
        }

        private bool NotAttackingInMelee()
        {
            return !hc.wc.activeWeapon || hc.wc.activeWeapon.weaponType != WeaponController.Type.Melee || !hc.wc.activeWeapon.attackingMelee;
        }

        public void ShotJiggle(float amount)
        {
            _currentCameraJiggle += amount;
            if (_currentCameraJiggle > 1)
                _currentCameraJiggle = 1;

            currentCrosshairJiggle += amount;
            if (currentCrosshairJiggle > 1)
                currentCrosshairJiggle = 1;
        }

        public void OnTriggerEnter(Collider coll)
        {
            // dash attack
            if (psc.dashAttack && dangerousDash && coll.gameObject != gameObject && (coll.gameObject.layer == 18 || coll.gameObject.layer == 11 || coll.gameObject.layer == 10))
            {
                MobBodyPart part  = coll.gameObject.GetComponent<MobBodyPart>();

                if (part != null && part.hc != null)
                {
                    if ( part.hc.door)
                    {
                        _uiManager.BreakDoor();
                        part.hc.door.DoorDestroyed();
                        if (hc.health > hc.healthMax / 20)
                            hc.Damage(dashDamageSelf * 1.5f, transform.position,transform.position, null, null, true, null, null, null, false);
                    }
                    else
                    {
                        _uiManager.DashAttack(part.hc.damagedByPlayerMessage[_gameManager.language]);
                        float damageScaler = 1f;
                            if (psc.vertigo) damageScaler = 1.5f;
                            if (hc.fireAndBloodActive)
                                damageScaler *= 2;
                            
                            
                        if (hc.health > hc.healthMax / 20)
                            hc.Damage(dashDamageSelf, transform.position,transform.position, null, null, true, null, null, null, false);
                        
                        part.hc.Damage(dashDamage * damageScaler * part.damageModificator, part.hc.transform.position + Vector3.up * 1.5f, transform.position + Vector3.up * 1.5f, part, null, true, null, hc, null, false);
                        if (part.hc.health <= 0 && !_gameManager.demo)
                        {
                            SteamAchievements.instance.UnlockSteamAchievement("NEW_ACHIEVEMENT_1_29");
                        }
                    }
                    
                    //staminaStats.CurrentValue -= staminaStats.dashCostCurrent;
                        
                    if (psc.fireAttack && Random.value > 0.9f)
                        part.hc.StartFire();
                }
                
                /*
                HealthController collHc = coll.gameObject.GetComponent<HealthController>();
                
                if (collHc != null)
                {
                    if (collHc.door)
                        collHc.Damage(1000, 0, 0, collHc.transform.position + Vector3.up * 1.5f, transform.position + Vector3.up * 1.5f, null);
                    else if (collHc.mobPartsController)
                    {
                        collHc.Damage(dashDamage, 0, 0, collHc.transform.position + Vector3.up * 1.5f, transform.position + Vector3.up * 1.5f, null);
                        staminaStats.CurrentValue -= staminaStats.dashCostCurrent;
                    }
                }
                */
            }
        }
        
        
        private void Gravity()
        {
            _grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (_grounded)
                _velocity.y = -2f;
            else
                _velocity.y = gravity;
            
            /*
            if (_grounded && _velocity.y < 0 && transform.position.y > 0.5f)
            {
                _velocity.y = -2f;
            }

            if (transform.position.y >= 0)
            {
                _velocity.y += gravity * Time.deltaTime;
            }
            else if (fallingInHole || _gameManager.hub)
                _velocity.y += gravity * 0.2f * Time.deltaTime;
            else
                _velocity.y += gravity * Time.deltaTime;
                */

            controller.Move(_velocity * Time.deltaTime);
        }

        private float ControlJiggle(float jiggle, float changeSpeed)
        {
            if (_move.sqrMagnitude <= 0.01f)
            {
                if (jiggle > 0.01f)
                    jiggle = Mathf.Lerp(jiggle, 0, changeSpeed * 0.5f * Time.deltaTime);
                else
                    jiggle = 0;
            }
            else
            {
                if (movementStats.isRunning)
                {
                    if (jiggle < 1)
                        jiggle = Mathf.Lerp(jiggle, 1, changeSpeed * 2 * Time.deltaTime);
                }
                else if (movementStats.movementState == MovementState.Dashing)
                {
                    if (jiggle < 1)
                        jiggle = Mathf.Lerp(jiggle, 1, changeSpeed * 3 * Time.deltaTime);
                }
                else
                    jiggle = Mathf.Lerp(jiggle, 0.5f, changeSpeed * Time.deltaTime);
            }

            return jiggle;
        }

        public void StartLevel()
        {
            if (goldenLightAnimator)
                goldenLightAnimator.SetBool("GoldenLight", false);
            
            hc.wc.Init();
            controller.enabled = true;
            mouseLook.canControl = true;
            mouseLook.debugMap.SetActive(false);
            mouseLook.spectatorCam.SetActive(false);
            mouseLook.mainCamera.cullingMask = mouseLook.defaultCullingMask;
            
            if (GameManager.instance.questWindow)
                GameManager.instance.ToggleQuests();
            
            if (GameManager.instance.paused)
                GameManager.instance.TogglePause();
            
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                mouseLook.playerLight.gameObject.SetActive(true);
                UiManager.instance.ToggleGameUi(true, false);
            }

            if (HubItemsSpawner.instance)
            {
                //PlayerSkillsController.instance.InstantTeleport(HubItemsSpawner.instance.itemSpawners[Random.Range(0, HubItemsSpawner.instance.itemSpawners.Count)].transform.position);
            }
        }

        public void Death()
        {
            goldenLightAnimator.SetBool("GoldenLight", true);
            controller.enabled = false;
            mouseLook.canControl = false;
            StartCoroutine(AbleToChooseWhereToRespawn());
        }

        IEnumerator AbleToChooseWhereToRespawn()
        {
            yield return new WaitForSeconds(1);
            ableToChooseRespawn = true;
        }
        
        public void Earthquake()
        {
            cameraAnimator.SetTrigger("Earthquake");
        }

        public void StartAdrenaline()
        {
            adrenaline = true;
        }

        public void Teleport(bool active)
        {
            teleport = active;
            controller.enabled = !active;
        }
    }
}
