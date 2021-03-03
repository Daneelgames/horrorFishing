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

    public void SyncCameraFov()
    {
        cameraFov = mainCamera.fieldOfView;
    }

    public void ToggleCrouch(bool crouch)
    {
        
        if (crouch && !crouching)
        {
            crouching = crouch;
            if (crouchCoroutine != null)
                StopCoroutine(crouchCoroutine);
            
            crouchCoroutine = StartCoroutine(Crouch(0.5f));

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
        if (gm.paused || !pm || !hc || pm.teleport || !(hc.health > 0) || !canControl) return;
        
        Aiming();
    }

    private void LateUpdate()
    {
        if (!gm || gm.paused || !pm || pm.teleport) return;
        
        if ((GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive) || (hc.health > 0 && canControl)) 
        {
            Looking();
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
        
        if (activeWeaponHolderAnim && activeWeaponHolderAnim.gameObject.activeInHierarchy && activeWeaponHolderAnim.GetBool(aimingString) != aiming)
            activeWeaponHolderAnim.SetBool(aimingString, aiming);
    }

    void Looking()
    {
        if (gm.player == null || pm == null || pm.hc.health <= 0)
        {
            if (camHolder) camHolder.transform.localRotation = Quaternion.Slerp(camHolder.transform.localRotation, Quaternion.identity, Time.deltaTime * gm.mouseLookSpeedCurrent);
                
            return;
        }
        
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
        pm.movementTransform.transform.rotation = camHolder.transform.rotation;
    }

    public void Recoil()
    {
        targetRotation.localRotation = Quaternion.Euler(Random.Range(-50,50), Random.Range(-50, 50), 0);
    }

    public bool PositionIsVisibleToPlayer(Vector3 pos)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(pos);
        bool visible = screenPoint.z > 0.1f && screenPoint.x > -0.1f && screenPoint.x < 1.1f && screenPoint.y > -0.1f && screenPoint.y < 1.1f;
        
        return visible;
    }
}