using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class SetRenderTexture : MonoBehaviour
{
    public RawImage targetImage;
    // Start is called before the first frame update
    GameManager gm;
    void Start()
    {
        gm = GameManager.instance;
        
        if (targetImage)
        {
            targetImage.texture = null;
            //targetImage.texture = gm.renderTextureInstance;
            targetImage.texture = gm.renderTexturePrefab;
        }
    }
}