using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class PlayerNoiseMaker : MonoBehaviour
{
    public static PlayerNoiseMaker instance;

    PlayerMovement pm;
    GameManager gm;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        pm = PlayerMovement.instance;
        gm = GameManager.instance;
    }

    public void MakeNoise(float noiseDistance, Vector3 noisePosition)
    {
    }
}