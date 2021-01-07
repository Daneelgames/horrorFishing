using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableObjectOnDisable : MonoBehaviour
{
    public GameObject objectToEnable;

    void OnDisable()
    {
        objectToEnable.SetActive(true);
    }
}
