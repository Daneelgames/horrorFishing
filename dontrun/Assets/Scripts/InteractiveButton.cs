using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveButton : MonoBehaviour
{
    public float pressCooldown = 0;
    float pressCooldownMax = 1;
    public bool pressOnce = true;
    public bool pressed = false;
    public bool canPress = true;
    public ButtonReciever reciever;
    public AudioSource au;

    public Interactable _interactable;
    public Collider coll;

    public void Press()
    {
        if (canPress)
        {
            if (pressed && pressOnce)
                return;

            if (pressCooldown <= 0)
            {
                pressed = true;
                au.pitch = Random.Range(0.75f, 1.25f);
                reciever.Pressed();
                StartCoroutine(Cooldown());
            }
        }
    }

    IEnumerator Cooldown()
    {
        canPress = false;
        yield return new WaitForSeconds(pressCooldown);

        canPress = true;
    }

    public void Activate()
    {
        _interactable.enabled = true;
        coll.enabled = true;
    }
    public void Deactivate()
    {
        _interactable.enabled = false;
        coll.enabled = false;
    }
}
