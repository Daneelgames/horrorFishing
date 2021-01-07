using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RearWindowPlayerController : MonoBehaviour
{
    public static RearWindowPlayerController instance;

    public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
    public float m_Speed = 12f;                 // How fast the tank moves forward and back.
    public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.

    private Rigidbody m_Rigidbody;              // Reference used to move the tank.
    private float m_MovementInputValue;         // The current value of the movement input.
    private float m_TurnInputValue;             // The current value of the turn input.

    public Animator playerAnim;
    public WheelsController wheelsController;
    AudioSource wheelChairAudio;

    RearWindowGameManager gm;

    [HideInInspector]
    public PlayerInteractionZoneController interactionZone;
    [HideInInspector]
    public PlayerZoneColliderController zoneCollider;
    public bool canMove = true;
    public Camera fpsCam;
    bool aiming = false;
    bool canMoveGun = false;
    float aimDelay = 0;
    float aimMaxDelay = 0.2f;
    bool canInteract = true;

    public RearWindowPlayerController weapon;
    Vector3 weaponInitialPos = new Vector3(0, 1.25f, 0.5f);
    Vector3 mouseInitAimPosition;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        m_Rigidbody = GetComponent<Rigidbody>();

        gm = RearWindowGameManager.instance;
        if (gm.pc == null)
            gm.pc = this;
        wheelChairAudio = wheelsController.gameObject.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }

    private void Update()
    {
        m_MovementInputValue = Input.GetAxis("Vertical");
        m_TurnInputValue = Input.GetAxis("Horizontal");

        if (canMove && (m_MovementInputValue != 0 || m_TurnInputValue != 0))
        {
            wheelChairAudio.volume = Mathf.Lerp(wheelChairAudio.volume, 1, Time.deltaTime);
        }
        else if (!canMove || (m_MovementInputValue == 0 && m_TurnInputValue == 0))
        {
            wheelChairAudio.volume = Mathf.Lerp(wheelChairAudio.volume, 0, 10 * Time.deltaTime);
        }

        Animate();

        if (canInteract)
            Interact();

        if (aimDelay > 0)
            aimDelay -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (canMove && !aiming)
        {
            Move();
            Turn();
        }
    }

    void Interact()
    {
        if (Input.GetButtonDown("Dash") || Input.GetKeyDown("e"))
        {
            if (!aiming && interactionZone.closestObject != null)
            {
                if (canMove)
                {
                    canMove = false;
                    interactionZone.closestObject.Interact();
                }
                else
                {
                    interactionZone.closestObject.Interact();
                }
            }
        }
        /*
        if (Input.GetButton("Fire2"))
        {
            if (canMove && aimDelay <= 0)
            {
                aiming = true;
                playerAnim.SetBool("Aiming", true);
                gm.tc.letterboxAnim.SetBool("Active", true);
                canMove = false;
                gm.activeCameraZone.cam.gameObject.SetActive(false);
                fpsCam.gameObject.SetActive(true);
                aimDelay = aimMaxDelay;
                StartCoroutine(LiftGun(true));
            }
            if (canMoveGun)
            {
                ControlGun();
            }
        }
        if (Input.GetButtonUp("Fire2"))
        {
            if (aiming)
            {
                Invoke("CancelAim" ,0.2f);
            }
        }*/
    }

    IEnumerator LiftGun(bool up)
    {
        float t = 0f;
        Vector3 startPos = weapon.transform.localPosition;
        Vector3 newPos;
        if (up)
            newPos = weaponInitialPos;
        else
            newPos = new Vector3(0, 0.7f, 0);

        while (t < 0.25f)
        {
            t += Time.deltaTime;
            weapon.transform.localPosition = Vector3.Lerp(startPos, newPos, t / 0.25f);
            yield return null;
        }
        canMoveGun = up;
        mouseInitAimPosition = Input.mousePosition;
    }

    void CancelAim()
    {
        playerAnim.SetBool("Aiming", false);
        gm.tc.letterboxAnim.SetBool("Active", false);
        canMove = true;
        aiming = false;
        gm.activeCameraZone.cam.gameObject.SetActive(true);
        fpsCam.gameObject.SetActive(false);
        aimDelay = aimMaxDelay;
        StartCoroutine(LiftGun(false));
    }

    void ControlGun()
    {
        Vector3 newMouseOffset = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);
        Vector3 objPosition = fpsCam.ScreenToWorldPoint(newMouseOffset);

        weapon.transform.localPosition = objPosition;
    }

    void Animate()
    {

        if (canMove)
        {
            if (m_MovementInputValue > 0)
            {
                playerAnim.SetBool("MoveForward", true);
                playerAnim.SetBool("MoveBackwards", false);
                wheelsController.Forward();
            }
            else if (m_MovementInputValue < 0)
            {
                playerAnim.SetBool("MoveBackwards", true);
                playerAnim.SetBool("MoveForward", false);
                wheelsController.Backwards();
            }
            else if (m_MovementInputValue == 0)
            {
                playerAnim.SetBool("MoveForward", false);
                playerAnim.SetBool("MoveBackwards", false);
            }

            if (m_TurnInputValue > 0)
            {
                playerAnim.SetBool("TurnRight", true);
                playerAnim.SetBool("TurnLeft", false);

                if (m_MovementInputValue == 0)
                    wheelsController.Right();
            }
            else if (m_TurnInputValue < 0)
            {
                playerAnim.SetBool("TurnLeft", true);
                playerAnim.SetBool("TurnRight", false);

                if (m_MovementInputValue == 0)
                    wheelsController.Left();
            }
            else
            {
                playerAnim.SetBool("TurnLeft", false);
                playerAnim.SetBool("TurnRight", false);
            }
        }
        else
        {
            playerAnim.SetBool("TurnLeft", false);
            playerAnim.SetBool("TurnRight", false);
            playerAnim.SetBool("MoveForward", false);
            playerAnim.SetBool("MoveBackwards", false);
        }
    }

    private void Move()
    {
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }

    private void Turn()
    {
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }

    public IEnumerator CanMove()
    {
        canInteract = false;
        canMove = true;
        yield return new WaitForSeconds(0.5f);
        canInteract = true;
    }

    public void ParentZones(Transform parent, bool active)
    {
        zoneCollider.transform.SetParent(parent);
        zoneCollider.activeCamera = null;
        zoneCollider.gameObject.SetActive(active);

        interactionZone.transform.SetParent(parent);
        interactionZone.closestObject = null;
        interactionZone.objectsInRange.Clear();
        interactionZone.gameObject.SetActive(active);
    }
}