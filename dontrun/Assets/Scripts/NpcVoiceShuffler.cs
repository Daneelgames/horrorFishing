using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcVoiceShuffler : MonoBehaviour
{
    public List<AudioClip>  clips = new List<AudioClip>();
    public List<int> clipsPool = new List<int>();
    
    public AudioClip GetVoiceClip()
    {
        if (clipsPool.Count <= 1)
        {
            clipsPool.Clear();
            for (int i = 0; i < clips.Count; i++)
            {
                clipsPool.Add(i);
            }
        }

        int r = Random.Range(0, clipsPool.Count);
        
        var newClip = clips[clipsPool[r]];
        clipsPool.RemoveAt(r);
        
        return newClip;
    }
}
