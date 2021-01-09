using System;
using System.Collections;
using PlayerControls;
using UnityEngine;
using Random = UnityEngine.Random;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;

    float mouseX = 0;
    float mouseY = 0;

    float xRotation = 0;
    float yRotation = 0;

    public float cameraFovIdle = 60;
    public float cameraFovAim = 30;
    [HideInInspector]    
    public float cameraFovIdleInit = 60;
    [HideInInspector]
    public float cameraFovAimInit = 30;

    public bool canControl = false;

    [SerializeField]
    GameObject camHolder;
    public Camera mainCamera;
    [SerializeField]
    Camera handsCamera;

    float cameraFov = 60;
    [SerializeField]
    Transform targetRotation;

    public bool aiming = false;
    public bool canAim = true;
    public Animator activeWeaponHolderAnim;
    //public MeshRenderer crosshair;

    GameManager gm;
    PlayerMovement pm;
    HealthController hc;
    WeaponControls wc;

    private bool crouching = false; 
    
    public static MouseLook instance;

    private Coroutine crouchCoroutine;

    private string aimString = "Aim";
    private string aimingString = "Aiming";
    private string mouseXstring = "Mouse X";
    private string mouseYstring = "Mouse Y";

    public Light playerLight;

    private float debugMapTime = 0;
    public GameObject debugMap;
    public GameObject spectatorCam;

    [Header("Culling settings")] 
    public LayerMask defaultCullingMask;
    public LayerMask coopLoadingCullingMask;
    
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        hc = pm.hc;
        wc = WeaponControls.instance;

        cameraFovIdleInit = cameraFovIdle;
        cameraFovAimInit = cameraFovAim;

        mainCamera.cullingMask = coopLoadingCullingMask;
        //activeWeaponHolderAnim = wc.activeWeapon.weaponMovementAnim;
    }

    public void AddDebugMapTime(float time)
    {
        debugMapTime += time;
    }


    public void ToggleCrouch(bool crouch)
    {
        
        if (crouch && !crouching)
        {
            crouching = crouch;
            if (crouchCoroutine != null)
                StopCoroutine(crouchCoroutine);
            
            crouchCoroutine = StartCoroutine(Crouch(1));

        }
        else if (!crouch && crouching)
        {
            crouching = crouch;
            if (crouchCoroutine != null)
                StopCoroutine(crouchCoroutine);

            crouchCoroutine = StartCoroutine(Crouch(2));
        }
    }

    IEnumerator Crouch(float newHeight)
    {
        float t = 0;
        float time = 1f;

        while (t < time)
        {
            camHolder.transform.localPosition = Vector3.Lerp(camHolder.transform.localPosition,Vector3.up * newHeight, t / time);
            transform.localPosition = Vector3.Lerp(transform.localPosition,Vector3.up * newHeight, t / time);
            t += Time.deltaTime;
            yield return null;
        }
    }

    void Update()
    {
        if (!gm.paused && !pm.teleport && !gm.questWindow)
        {
            if ((GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive) || (hc.health > 0 && canControl)) 
            {
                Aiming();
            }
        }

        if (hc.health > 0)
        {
            if (debugMapTime > 0 && Time.timeScale > 0)
            {
                if (!debugMap.gameObject.activeInHierarchy)
                {
                    debugMap.gameObject.SetActive(true);
                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    {
                        spectatorCam.gameObject.SetActive(true);   
                    }
                    else
                    {
                        spectatorCam.gameObject.SetActive(false);
                    }
                }
                
                debugMapTime -= Time.deltaTime;   
            }
            else if (debugMap.gameObject.activeInHierarchy) 
                debugMap.gameObject.SetActive(false);
        }
        else
        {
            if (!debugMap.gameObject.activeInHierarchy)
            {
                debugMapTime = 0;
                debugMap.gameObject.SetActive(true);
                
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    spectatorCam.gameObject.SetActive(true);   
                }
                else
                {
                    spectatorCam.gameObject.SetActive(false);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (!gm.paused && !pm.teleport && !gm.questWindow)
        {
            if ((GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive) || (hc.health > 0 && canControl)) 
            {
                Looking();
            }
        }
    }

    void Aiming()
    {
        
        if (Math.Abs(Input.GetAxisRaw(aimString)) > 0.1f)
            aiming = true;
        else if (Input.GetButton(aimString))
            aiming = true;
        else if (KeyBindingManager.GetKey(KeyAction.Aim))
            aiming = true;
        else
            aiming = false;

        if (hc.damageCooldown > 0 || (wc.activeWeapon && wc.activeWeapon.reloading)) aiming = false;

        if (aiming)
        {
            cameraFov = Mathf.Lerp(cameraFov, cameraFovAim, 3 * Time.deltaTime);
        }
        else
        {
            cameraFov = Mathf.Lerp(cameraFov, cameraFovIdle, 3 * Time.deltaTime);
        }

        mainCamera.fieldOfView = cameraFov;
        handsCamera.fieldOfView = cameraFov;
        
        if (activeWeaponHolderAnim && activeWeaponHolderAnim.GetBool(aimingString) != aiming)
            activeWeaponHolderAnim.SetBool(aimingString, aiming);
    }

    void Looking()
    {
        if (pm == null) return;
        
        mouseX = Input.GetAxis(mouseXstring) * gm.mouseSensitivity;
        mouseY = Input.GetAxis(mouseYstring) * gm.mouseSensitivity;
        if (gm.mouseInvert == 1) mouseY *= -1;

        if (wc.activeWeapon && wc.activeWeapon.weaponType == WeaponController.Type.Melee && !wc.activeWeapon.canAct)
        {
            mouseX /= 1.5f;
            mouseY /= 1.5f;
        }

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -80, 80);

        targetRotation.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation.localRotation, Time.deltaTime * gm.mouseLookSpeedCurrent);
        camHolder.transform.localRotation = Quaternion.Slerp(camHolder.transform.localRotation, transform.localRotation, Time.deltaTime * gm.mouseLookSpeedCurrent);
        gm.player.pm.movementTransform.transform.rotation = camHolder.transform.rotation;
    }

    public void Recoil()
    {
        targetRotation.localRotation = Quaternion.Euler(Random.Range(-50,50), Random.Range(-50, 50), 0);
    }

    public bool PositionIsVisibleToPlayer(Vector3 pos)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(pos);
        bool visible = screenPoint.z > 0.1f && screenPoint.x > 0.1f && screenPoint.x < 0.9f && screenPoint.y > 0.1f && screenPoint.y < 0.9f;
        
        return visible;
    }
}