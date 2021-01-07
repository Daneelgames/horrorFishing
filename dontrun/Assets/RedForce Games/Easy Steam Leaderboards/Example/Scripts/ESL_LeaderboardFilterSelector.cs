using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESL_LeaderboardFilterSelector : MonoBehaviour 
{
	public static event System.Action<ESL_LeaderboardUI.LeaderboardFilter> onFilterSelected;

	public ESL_LeaderboardUI.LeaderboardFilter filterType;

	public void OnFilterSelected()
	{
		if(onFilterSelected != null)
			onFilterSelected(filterType);
	}
}
