using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpriteToMaterial : MonoBehaviour
{
    public List<Texture> sprites;
    public SpriteRenderer spriteRenderer;
    
    void Start()
    {
        //spriteRenderer.material.mainTexture = sprites[0];
        spriteRenderer.material.SetTexture(0, sprites[Random.Range(0, sprites.Count)]);
    }

}
