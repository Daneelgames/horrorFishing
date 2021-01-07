using PlayerControls;
using UnityEngine;

public class HoverCarControl : MonoBehaviour
{
    public Animator anim;

    public CharacterController controller;
    float m_deadZone = 0.1f;
    public TransportObject transpot;

    public float forwardSpeed = 75;
    public float backwardsSpeed = 20;
    private float currentSpeed = 0;
    public float rotationSpeed = 20;
    float rotationSpeedCurrent = 0;

    public AudioSource au;

    private bool _grounded = false;
    
    private Vector3 _velocity;
    [SerializeField] private Vector3 moveVector;
    [SerializeField] private float gravityCurrent = -3f;
    [SerializeField] private float gravity = -3f;
    [SerializeField] private float gravityMax = -300f;
    public Transform groundCheck;
    public bool fallingInHole = false;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private string verticalString = "Vertical";	
    private string horizontalString = "Horizontal";	
    
    void Update()
    {
        
        /*
        if (transpot.playerInside)
        {
            if (gameObject.layer != 14)
                gameObject.layer = 14; // mob movement coll layer
        }
        else
        {
            if (gameObject.layer != 13)
                gameObject.layer = 13; // pick up
        }
        */
        
        Movement();
        Gravity();
    }

    void Movement()
    {
        if (transpot.playerInside)
        {
            float vert = 0;
            float hor = 0;
            
            if (KeyBindingManager.GetKey(KeyAction.Forward))
                vert = 1;
            else if (KeyBindingManager.GetKey(KeyAction.Backwards))
                vert = -1;
            else
                vert = 0;
            
            if (KeyBindingManager.GetKey(KeyAction.RightStrafe))
                hor = 1;
            else if (KeyBindingManager.GetKey(KeyAction.LeftStrafe))
                hor = -1;
            else
                hor = 0;
            
            /*
            float aclAxis = Input.GetAxis(verticalString);
            float turning = Input.GetAxis(horizontalString);
            */
            float aclAxis = vert;
            float turning = hor;

            if (aclAxis > m_deadZone)
            {
              currentSpeed = Mathf.Lerp(currentSpeed, aclAxis * forwardSpeed, 0.2f * Time.deltaTime);
              au.pitch = 1;
              au.volume += Time.deltaTime;
              anim.SetBool("Forward", true);
              anim.SetBool("Backward", false);
            }
            else if (aclAxis < -m_deadZone)
            {
              currentSpeed = Mathf.Lerp(currentSpeed, aclAxis * backwardsSpeed, 0.2f * Time.deltaTime); 
              anim.SetBool("Forward", false);
              anim.SetBool("Backward", true);
              au.volume += Time.deltaTime;
              au.pitch = 0.75f;
            }
            else
            {
              currentSpeed = Mathf.Lerp(currentSpeed, 0, 0.01f * Time.deltaTime); 
              anim.SetBool("Forward", false);
              anim.SetBool("Backward", false);
              au.volume -= Time.deltaTime;
            }
            
            SetThrust(currentSpeed);

            if (turning > m_deadZone || turning < m_deadZone)
            {
              rotationSpeedCurrent = Mathf.Lerp(rotationSpeedCurrent, turning * rotationSpeed, 10 * Time.deltaTime);
            }
            else
            {
              rotationSpeedCurrent = Mathf.Lerp(rotationSpeedCurrent, 0, Time.deltaTime);
            }
            SetTurning(rotationSpeedCurrent);
        }
        else
        {
          currentSpeed = Mathf.Lerp(currentSpeed, 0, 0.95f * Time.deltaTime); 
          rotationSpeedCurrent = Mathf.Lerp(rotationSpeedCurrent, 0, 0.8f * Time.deltaTime);
          anim.SetBool("Forward", false);
          anim.SetBool("Backward", false);
          au.volume -= Time.deltaTime;
          SetThrust(currentSpeed);
          SetTurning(rotationSpeedCurrent);
        }
    }

    void SetThrust(float newThrust)
    {
        moveVector = transform.forward * (newThrust * Time.deltaTime);
        controller.Move(moveVector);
    }

    void SetTurning(float newTurning)
    {
        transform.Rotate(0, newTurning * Time.deltaTime, 0);
    }
  
    private void Gravity()
    {
        _grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        //if (_grounded && _velocity.y < 0)
        if (_grounded)
        {
          _velocity.y = -2f;
          gravityCurrent = gravity;
        }
        else
          gravityCurrent = Mathf.Lerp(gravityCurrent, gravityMax, 0.3f * Time.deltaTime);

        _velocity.y += gravityCurrent * Time.deltaTime;

        controller.Move(_velocity * Time.deltaTime);
    }
}
