using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Mirror.Examples.Chat;
using PlayerControls;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelStreamer : MonoBehaviour
{
    public List<StreamingGroup> streamingGroups;
	private static LevelStreamer _instance;

    public float streamingDistance = 60;
    public static LevelStreamer Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = FindObjectOfType<LevelStreamer>();
			}
			return _instance;
		}
	}

    public List<StreamingObject> streamingObjects;
    public PlayerMovement pm;
    Coroutine streamCoroutine;

    private void Awake()
    {
        GameManager.instance.ls = this;
    }

    public void ClearData()
    {
        if (streamCoroutine != null)
            StopCoroutine(streamCoroutine);
        streamingObjects.Clear();
    }

    private void Start()
    {
        pm = PlayerMovement.instance;
        streamCoroutine = StartCoroutine(Stream());
    }

    public void Init()
    {
        //GetStreamingGroups();
        
        if (streamCoroutine != null)
            StopCoroutine(streamCoroutine);

        StartCoroutine(Stream());
    }
    
    IEnumerator Stream()
    {
        int j = 100;

        HealthController closestPlayer = PlayerMovement.instance.hc;
        
        while(true)
        {
            yield return new WaitForSeconds(0.1f);
            if (!pm)
                pm = PlayerMovement.instance;
            
            for (int i = 0; i < streamingObjects.Count; i++)
            {
                if (streamingObjects[i] != null)
                {
                    //if (Vector3.Distance(pm.transform.position, streamingObjects[i].transform.position) < streamingObjects[i].streamingDistance)
                    
                    if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    {
                        closestPlayer = GLNetworkWrapper.instance.GetClosestPlayer(streamingObjects[i].transform.position);
                    }
                    
                    if (closestPlayer == null)
                        break;
                    
                    /*
                    if ((closestPlayer.transform.position - streamingObjects[i].transform.position).magnitude <
                        MouseLook.instance.mainCamera.farClipPlane)
                        */
                    if ((closestPlayer.transform.position - streamingObjects[i].transform.position).magnitude < streamingDistance)
                    {
                        if (!streamingObjects[i].streamObjectAboveAnimator)
                        {
                            if (!streamingObjects[i].gameObject.activeInHierarchy)
                                streamingObjects[i].gameObject.SetActive(true);   
                        }
                        else
                        {
                            streamingObjects[i].UnhideChilds();   
                        }
                    }
                    else
                    {
                        if (!streamingObjects[i].streamObjectAboveAnimator)
                        {
                            if(streamingObjects[i].gameObject.activeInHierarchy)
                                streamingObjects[i].gameObject.SetActive(false);
                        }
                        else
                            streamingObjects[i].HideChilds();
                    }
                    
                    //print(streamingObjects[i].isActiveAndEnabled);
                }


                if (i == j)
                {
                    j += 50;
                    if (j >= streamingObjects.Count)
                        j = 50;
                    
                    yield return new WaitForSeconds(0.05f);
                }
            }
            
            if (pm == null)
                break;
        }
    }

    public void PlayerTeleported()
    {
        // check every object
        for (int i = 0; i < streamingObjects.Count; i++)
        {
            if (streamingObjects[i] != null)
            {
                var closestPlayer = PlayerMovement.instance.hc;
                
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    closestPlayer = GLNetworkWrapper.instance.GetClosestPlayer(streamingObjects[i].transform.position);
                }
                    
                if (closestPlayer == null)
                    continue;
                
                if ((closestPlayer.transform.position - streamingObjects[i].transform.position).magnitude < streamingDistance)
                {
                    if (!streamingObjects[i].streamObjectAboveAnimator)
                    {
                        if (!streamingObjects[i].gameObject.activeInHierarchy)
                            streamingObjects[i].gameObject.SetActive(true);   
                    }
                    else
                    {
                        streamingObjects[i].UnhideChilds();   
                    }
                }
                else
                {
                    if (!streamingObjects[i].streamObjectAboveAnimator)
                    {
                        if(streamingObjects[i].gameObject.activeInHierarchy)
                            streamingObjects[i].gameObject.SetActive(false);
                    }
                    else
                        streamingObjects[i].HideChilds();
                }
            }
        }
    }
    
    bool IsVisible(Vector3 targetPos)
    {
        Vector3 screenPoint = pm.mouseLook.mainCamera.WorldToViewportPoint(targetPos);
        bool onScreen = screenPoint.x > -5 && screenPoint.x < 5 && screenPoint.y > 5f && screenPoint.y < 5f;
        return onScreen;
    }
}

[Serializable]
public class StreamingGroup
{
    public List<StreamingObject> streamingObjects;
}