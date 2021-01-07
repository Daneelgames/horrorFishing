using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLoopRandomizer : MonoBehaviour
{
    public AudioSource au;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShuffleTrack());
    }

    IEnumerator ShuffleTrack()
    {
        while (true)
        {
            au.pitch = Random.Range(0.75f, 1.25f);
            au.loop = true;
            au.Play();
            yield return new WaitForSeconds(Random.Range(2, 16));
            au.Stop();
        }
    }
}
