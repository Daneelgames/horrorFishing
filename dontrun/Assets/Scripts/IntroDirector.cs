    using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class IntroDirector : MonoBehaviour
{
    private PlayerMovement pm;
    public Animator handAnim;
    public AudioSource au;
    public AudioSource au2;

    public Transform holeCenter;
    
    void Start()
    {
        pm = PlayerMovement.instance;
        StartCoroutine(CheckForRose());
    }

    IEnumerator CheckForRose()
    {
        while (!pm.interactionController.objectInHands)
        {
            yield return new WaitForSeconds(0.1f);
        }

        while (Vector3.Distance(handAnim.transform.position, pm.transform.position) > LevelGenerator.instance.tileSize * 3)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        handAnim.SetTrigger("Active");
        au.Play();
        au2.Play();
        
        Camera cam = pm.mouseLook.mainCamera;
        pm.mouseLook.canAim = false;
        
        float t = 0;
        float time = 5;
        while (t < time)
        {
            cam.backgroundColor = Color.Lerp(cam.backgroundColor, Color.black, t / time);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, Color.black, t / time);
            RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, 20f, t / time);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 30, t / time);
            t += Time.deltaTime;
            yield return null;
        }
        
        while (Vector3.Distance(pm.transform.position, holeCenter.transform.position) > 6)
        {
            yield return new WaitForSeconds(0.1f);
        }

        pm.fallingInHole = true;
        
        while (pm)
        {
            pm.controller.Move((new Vector3(holeCenter.position.x, pm.transform.position.y, holeCenter.position.z) -
                                pm.transform.position).normalized * 0.5f);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
}
