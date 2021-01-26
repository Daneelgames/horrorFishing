using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CerealDevelopment.TimeManagement;

public class StreamingObject : MonoBehaviour //, IUpdatable
{
    [Header("Use this if animator inside streaming object to reduce lag")]
    public bool streamObjectAboveAnimator = false;
    public List<GameObject> childObjectToHide = new List<GameObject>();
    
    void Start()
    {
	    var ls = LevelStreamer.Instance;
	    if (ls) ls.streamingObjects.Add(this);   
    }

    public void HideChilds()
    {
	    for (var index = 0; index < childObjectToHide.Count; index++)
	    {
		    var child = childObjectToHide[index];
		    child.SetActive(false);
	    }
    }

    public void UnhideChilds()
    {
	    for (var index = 0; index < childObjectToHide.Count; index++)
	    {
		    var child = childObjectToHide[index];
		    child.SetActive(true);
	    }
    }
}