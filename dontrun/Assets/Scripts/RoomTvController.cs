using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomTvController : MonoBehaviour
{
    public SpriteRenderer screen;
    public List<Sprite> frames = new List<Sprite>();
    
    void Start()
    {
        StartCoroutine(ChangeFrames());
    }

    IEnumerator ChangeFrames()
    {
        while (true)
        {
            yield return  new WaitForSeconds(Random.Range(1,5));
            screen.sprite = frames[Random.Range(0, frames.Count)];
        }
    }
}

[Serializable]
public class Frames
{
}