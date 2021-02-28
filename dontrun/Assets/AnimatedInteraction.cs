using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimatedInteraction : MonoBehaviour
{
    public AudioSource au;
    public Animator anim;
    public float interactionCooldown = 3f;
    
    private bool canInteract = true;
    private string interactionString = "Interaction";
    
    public List<GameObject> objectsToActivate = new List<GameObject>();
    private int nextObjectIndex = 0;
    
    public void Interact()
    {
        if (!canInteract) return;

        anim.SetTrigger(interactionString);
        canInteract = false;

        if (au)
        {
            au.pitch = Random.Range(0.75f, 1.25f);
            au.Play();   
        }
        
        if (objectsToActivate[nextObjectIndex].activeInHierarchy == false)
        {
            objectsToActivate[nextObjectIndex].SetActive(true);
            if (objectsToActivate.Count > nextObjectIndex + 1)
                nextObjectIndex++;   
        }
        
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        yield return  new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
}
