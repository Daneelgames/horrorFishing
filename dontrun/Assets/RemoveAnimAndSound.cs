using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveAnimAndSound : MonoBehaviour
{
    [Range(0, 1)] public float chanceToRemove = 0;

    public Animator animToRemove;
    public AudioSource audioToRemove;
    public List<GameObject> objectsToRemove;
    void Start()
    {
        if (Random.value < chanceToRemove)
        {
            if (animToRemove)
                Destroy(animToRemove);
            
            if (audioToRemove)
                Destroy(audioToRemove);
            
            if (objectsToRemove.Count == 0) 
                return;

            for (int i = objectsToRemove.Count - 1; i >= 0; i--)
            {
                Destroy(objectsToRemove[i]);
            }
        }
    }
}
