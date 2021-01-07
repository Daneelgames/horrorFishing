using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomTvController : MonoBehaviour
{
    public SpriteRenderer screen;
    public List<Frames> frames = new List<Frames>();
    private int currentFramesIndex = 0;
    
    void Start()
    {
        currentFramesIndex = Random.Range(0, frames.Count);
        StartCoroutine(ChangeFrames());
    }

    IEnumerator ChangeFrames()
    {
        while (true)
        {
            yield return  new WaitForSeconds(Random.Range(1,5));
            screen.sprite = frames[currentFramesIndex].frames[Random.Range(0, frames[currentFramesIndex].frames.Count)];
        }
    }
}

[Serializable]
public class Frames
{
    public List<Sprite> frames = new List<Sprite>();
}