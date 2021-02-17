using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class LoadingHintsController : MonoBehaviour
{
    public static LoadingHintsController instance;
    public List<Hint> hints = new List<Hint>();
    public TextMeshProUGUI hintsField;
    private int currentHint = 0;
    private GameManager gm;

    private Coroutine hintsCoroutine;

    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        gm = GameManager.instance;
    }
    
    public void StartHints()
    {
    }

    public void StopHints()
    {
    }
}

[Serializable]
public class Hint
{
    [Header("0 - eng; 1 - rus; 2 - esp; 3 - ger")] public List<string> hints;
}
