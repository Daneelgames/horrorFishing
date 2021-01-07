using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;

public class HubWeatherController : MonoBehaviour
{
    private Camera playerCamera;
    public List<Weather> colorsLine = new List<Weather>();
    public Light directionalLight;
    public float timeBetweenColors = 30;
    private int i = 0;


    IEnumerator Start()
    {
        while (playerCamera == null)
        {
            playerCamera = MouseLook.instance.mainCamera;
            yield return new WaitForSeconds(1);
        }

        StartCoroutine(DayCycle(Random.Range(0, colorsLine.Count)));
    }

    IEnumerator DayCycle(int startIndex)
    {
        while (playerCamera != null)
        {
            for (i = startIndex; i < colorsLine.Count; i++)
            {
                var colorStart = colorsLine[i].mainColor;
                var colorEnd = colorsLine[i].mainColor;
                
                float fogStart = colorsLine[i].fogStart;
                float fogEnd = colorsLine[i].fogEnd;
                float fogStart0 = colorsLine[i].fogStart;
                float fogEnd0 = colorsLine[i].fogEnd;
                
                if (i >= colorsLine.Count - 2)
                {
                    i = 0;
                    colorEnd = colorsLine[0].mainColor;
                    fogStart0 = colorsLine[0].fogStart;
                    fogEnd0 = colorsLine[0].fogEnd;
                }
                else
                {
                    colorEnd = colorsLine[i + 1].mainColor;
                    fogStart0 = colorsLine[i+1].fogStart;
                    fogEnd0 = colorsLine[i+1].fogEnd;
                }
                
                yield return StartCoroutine(AnimateColor(colorStart, colorEnd, fogStart, fogEnd, fogStart0, fogEnd0));
            }
        }
    }

    IEnumerator AnimateColor(Color colorStart, Color colorEnd, float fogStart, float fogEnd, float fogStart0, float fogEnd0)
    {
        float t = 0;
        float tTarget = timeBetweenColors;
        while (t < tTarget)
        {
            var newColor = Color.Lerp(colorStart, colorEnd, t / tTarget);
            playerCamera.backgroundColor = newColor;
            RenderSettings.fogColor = newColor;
            directionalLight.color = newColor;

            float newFogStart = Mathf.Lerp(fogStart, fogStart0, t / tTarget);
            float newFogEnd = Mathf.Lerp(fogEnd, fogEnd0, t / tTarget);
            RenderSettings.fogStartDistance = newFogStart;
            RenderSettings.fogEndDistance = newFogEnd;
            
            t += Time.deltaTime;
            yield return null;
        }
    }
}

[Serializable]
public class Weather
{
    public Color mainColor;
    public float fogStart = 20;
    public float fogEnd = 150;
}
