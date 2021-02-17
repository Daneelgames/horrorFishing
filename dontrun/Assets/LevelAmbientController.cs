using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class LevelAmbientController : MonoBehaviour
{
    public List<SphereCollider> triggers = new List<SphereCollider>();
    public float maxVolume = 1;
    public AudioSource ambientSource;

    private PlayerMovement pm;
    
    void Start()
    {
        pm = PlayerMovement.instance;
        StartCoroutine(GetPlayerDistance());
    }

    IEnumerator GetPlayerDistance()
    {
        float closestDistance = 100;
        SphereCollider closestTrigger = null;
        while (true)
        {
            closestDistance = 5000;
            for (int i = 0; i < triggers.Count; i++)
            {
                var newDist = Vector3.Distance(triggers[i].transform.position, pm.transform.position);
                if (newDist < closestDistance)
                {
                    closestDistance = newDist;
                    closestTrigger = triggers[i];
                }
                yield return new WaitForSeconds(0.1f);   
            }

            if (closestDistance < closestTrigger.radius)
            {
                if (ambientSource.isPlaying == false)
                    ambientSource.Play();
                
                ambientSource.volume = Mathf.Clamp(1 - closestDistance / closestTrigger.radius , 0, maxVolume);
            }
            else if (ambientSource.isPlaying)
            {
                ambientSource.volume = 0;
                ambientSource.Stop();
            }
            
            yield return new WaitForSeconds(1);
        }
    }
}
