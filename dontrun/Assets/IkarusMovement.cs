using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class IkarusMovement : MonoBehaviour
{
    private PlayerMovement pm;
    private bool dead = false;
    public float moveSpeed = 30;

    public Rigidbody rb;
    // Start is called before the first frame update
    void OnEnable()
    {
        pm = PlayerMovement.instance;
        StartCoroutine(MoveToplayer());
    }

    IEnumerator MoveToplayer()
    {
        float distance = Vector3.Distance(transform.position, pm.cameraAnimator.transform.position);

        while (distance > 2 && !dead)
        {
            // move ikarus to player
            distance = Vector3.Distance(transform.position, pm.cameraAnimator.transform.position);
            if (distance > 100)
                rb.velocity = ((pm.cameraAnimator.transform.position + pm.cameraAnimator.transform.forward) - transform.position).normalized * moveSpeed * Time.deltaTime;
            else
                rb.velocity = ((pm.cameraAnimator.transform.position + pm.cameraAnimator.transform.forward) - transform.position).normalized * (moveSpeed / 3) * Time.deltaTime;
            print(distance);
            transform.LookAt(pm.cameraAnimator.transform.position);
            yield return null;
        }

        if (!dead)
        {
            rb.velocity = Vector3.zero;
            transform.LookAt(pm.cameraAnimator.transform.position);
        
            // ikarus is close to player
            UiManager.instance.ikarusHands.SetActive(true);
            PlayerMovement.instance.hc.invincible = true;
            PlayerMovement.instance.goldenLightAnimator.SetBool("GoldenLight", true);
            //UiManager.instance.welcomeToMeatZone.gameObject.SetActive(true);
            PlayerMovement.instance.controller.enabled = false;
        
            ItemsList.instance.PlayerFinishedLevel();
            GameManager.instance.NextLevel();
        }
    }

    public void Killed()
    {
        dead = true;
        StopAllCoroutines();
    }
}
