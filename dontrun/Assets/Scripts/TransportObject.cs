using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TransportObject : MonoBehaviour
{
    public Transform playerParentTransform;
    public bool playerInside = false;

    private bool canInteract = true;

    private Quaternion resetQuaternion;
    private Vector3 resetEuler;
    
    private PlayerMovement pm;
    private Transform parentTransform;

    private float newForce = 0;
    private float newTorque = 0;

    public Collider sitCollider;

    public AudioSource au;
    
    void Start()
    {
        pm = PlayerMovement.instance;
        parentTransform = transform.parent;
    }
    
    public void EnterTransport()
    {
        if (canInteract)
            StartCoroutine(EnterTransportOverTime());
    }

    IEnumerator EnterTransportOverTime()
    {
        GameManager.instance.SnoutFound();
        
        if (au != null)
            au.gameObject.SetActive(true);
        
        pm.ToggleCrouch(false);
        sitCollider.enabled = false;
        canInteract = false;
        //pm.controller.enabled = false;
        pm.transform.parent = playerParentTransform;
        transform.parent = null;

        //rb.constraints = RigidbodyConstraints.FreezeRotationZ;
        //rb.constraints = RigidbodyConstraints.FreezeRotationX;
        
        Quaternion newRot = new Quaternion();
        Vector3 newEuler = new Vector3(0, transform.rotation.eulerAngles.y, 0);
        newRot.eulerAngles = newEuler;
        
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime;
            pm.transform.localPosition = Vector3.Lerp(pm.transform.localPosition, Vector3.zero, t);
            //transform.rotation = Quaternion.Lerp(transform.rotation, newRot, t);
            yield return new WaitForEndOfFrame();
        }
        pm.inTransport = this;
        pm.transform.localPosition = Vector3.zero;

        pm.cameraAnimator.SetFloat("Speed", 0);
        pm.currentCrosshairJiggle = 0;
        playerInside = true;
        canInteract = true;
    }

    public void ExitTransport()
    {
        if (canInteract)
        {
            if (au != null)
                au.gameObject.SetActive(true);
                
            StartCoroutine(ExitTransportOverTime());
        }
    }
    
    IEnumerator ExitTransportOverTime()
    {
        canInteract = false;
        pm.transform.parent = null;
        
        yield return new WaitForEndOfFrame();
        
        //rb.constraints = RigidbodyConstraints.None;
        playerInside = false;
        canInteract = true;
        pm.inTransport = null;
        //pm.controller.enabled = true;
        
        transform.parent = parentTransform;
        
        sitCollider.enabled = true;
    }

    void FixedUpdate()
    {
        //if (playerInside)
        {
            transform.position = Vector3.Lerp(transform.position, parentTransform.position, 10 * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, parentTransform.rotation, 10 * Time.fixedDeltaTime);
        }
    }
}
