using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerAltarcontroller : MonoBehaviour
{
    public List<GameObject> herObjects;

    void Start()
    {
        var il = ItemsList.instance;
        
        if (il.herPiecesApplied.Count == 6) // ending
            gameObject.SetActive(false);
        
        for (int i = 0; i < herObjects.Count; i++)
        {
            if (i == il.herPiecesApplied.Count)
                herObjects[i].SetActive(true);
            else
                herObjects[i].SetActive(false);
        }
    }
}
