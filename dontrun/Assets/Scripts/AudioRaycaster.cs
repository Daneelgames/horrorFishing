using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class AudioRaycaster : MonoBehaviour
{
    public List<AudioSource> audioSources = new List<AudioSource>();
    List<float> startVolumes = new List<float>();

    bool playerFound = false;

    PlayerMovement pm;
    public LayerMask searchMask;
    RaycastHit searchHit;

    void Start()
    {
        /*
        pm = PlayerMovement.instance;

        for (int i = 0; i < audioSources.Count; i++)
        {
            startVolumes.Add(audioSources[i].volume);
        }

        //StartCoroutine(FindPlayer());
        
        //StartCoroutine(ChangeVolume());
        */
    }

    IEnumerator FindPlayer()
    {
        while(true)
        {
            if (!pm)
            {
                pm = PlayerMovement.instance;
            }

            if (Physics.Raycast(transform.position + Vector3.up, pm.transform.position - transform.position, out searchHit, 500f, searchMask))
            {
                if (searchHit.collider.gameObject.layer == 11 && searchHit.collider.gameObject == pm.gameObject) // if hit player
                {
                    playerFound = true;
                }
                else
                    playerFound = false;
            }
            else
                playerFound = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ChangeVolume()
    {
        while(true)
        {
            for(int i = 0; i < audioSources.Count; i++)
            {
                if (playerFound)
                    audioSources[i].volume = Mathf.Lerp(audioSources[i].volume, startVolumes[i], Time.deltaTime * 2);
                else
                    audioSources[i].volume = Mathf.Lerp(audioSources[i].volume, startVolumes[i] / 5, Time.deltaTime * 2);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
