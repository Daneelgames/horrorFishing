using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportController : MonoBehaviour
{
    public string sceneName;
    public string spawner;
    public AudioClip sfx;

    public void Teleport()
    {
        StartCoroutine(RearWindowGameManager.instance.Teleport(sceneName, spawner));
        RearWindowGameManager.instance.sfxSource.clip = sfx;
        RearWindowGameManager.instance.sfxSource.Play();
    }
}