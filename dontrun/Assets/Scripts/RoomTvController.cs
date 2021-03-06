using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomTvController : MonoBehaviour
{
    public SpriteRenderer screen;
    public List<Sprite> frames = new List<Sprite>();
    
    void OnEnable()
    {
        StartCoroutine(ChangeFrames());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator ChangeFrames()
    {
        while (true)
        {
            yield return  new WaitForSeconds(Random.Range(1,3));
            screen.sprite = frames[Random.Range(0, frames.Count)];
        }
    }
}