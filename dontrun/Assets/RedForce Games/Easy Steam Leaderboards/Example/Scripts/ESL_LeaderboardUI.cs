using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using EasySteamLeaderboard;

public class ESL_LeaderboardUI : MonoBehaviour
{

	//ase
	public GameObject EntriesContainer;
	public ESL_LeaderboardEntryUI LBEntryPrefab;
	public InputField Fetch_IDField;
	public InputField Upload_IDField;
	public InputField Upload_ScoreField;
	public ESL_LeaderboardEntryUI yourEntryUI;

	//enum
	public enum LeaderboardFilter
	{
		Global,
		Friends
	}

	//vars
	LeaderboardFilter currentFilter;
	List<GameObject> entriesObjs = new List<GameObject>();
	ESL_Leaderboard lbCache;


	void OnEnable()
	{
		ESL_LeaderboardFilterSelector.onFilterSelected += ESL_LeaderboardFilterSelector_onFilterSelected;
	}

	void OnDisable()
	{
		ESL_LeaderboardFilterSelector.onFilterSelected -= ESL_LeaderboardFilterSelector_onFilterSelected;
	}

	void ESL_LeaderboardFilterSelector_onFilterSelected(LeaderboardFilter filter)
	{
		currentFilter = filter;

		//if cached then repopulate
		if (lbCache != null)
			PopulateEntriedBasedOnFilter();
			
	}

	void PopulateEntriedBasedOnFilter()
	{
		StopAllCoroutines();
		if (gameObject.activeInHierarchy)
		{
			if (currentFilter == LeaderboardFilter.Global)
				StartCoroutine(PopulateEntries(lbCache.GlobalEntries));
			else if (currentFilter == LeaderboardFilter.Friends)
				StartCoroutine(PopulateEntries(lbCache.FriendsEntries));	
		}
	}

	IEnumerator PopulateEntries(List<ESL_LeaderboardEntry> entries)
	{
		//reset current ui
		ResetUI();

		//populate your entry if it exists
		yourEntryUI.Initialize(lbCache.SteamUserEntry);

		for (int i = 0; i < entries.Count; i++)
		{
			//instantiate prefab
			ESL_LeaderboardEntryUI entry = Instantiate(LBEntryPrefab);
			
			//set transform to container
			entry.transform.SetParent(EntriesContainer.transform);
			entry.transform.localScale = Vector3.one;

			//if gloabl entries show global rank
			if (currentFilter == LeaderboardFilter.Global)
				entry.Initialize(entries[i]);
			else if (currentFilter == LeaderboardFilter.Friends) //if friends, then show local rank among friends
				entry.Initialize(entries[i].PlayerName, (i + 1), entries[i].Score);


			//local obj cache
			entriesObjs.Add(entry.gameObject);

			yield return null;
		}
	}

	void ResetUI()
	{
		for (int i = 0; i < entriesObjs.Count; i++)
		{
			Destroy(entriesObjs[i]);
		}

		entriesObjs.Clear();

		//reset your entry ui
		yourEntryUI.Reset();
	}

	void FetchLeaderboardWithID(string lbid, int startRange, int endRange)
	{
		EasySteamLeaderboards.Instance.GetLeaderboard(lbid, (result) =>
			{
				print("fetch completed with ");
				//check if leaderboard successfully fetched
				if (result.resultCode == ESL_ResultCode.Success)
				{
					print("success");
					lbCache = result;
					PopulateEntriedBasedOnFilter();
				}
				else
				{
					Debug.Log("Failed Fetching: " + result.resultCode.ToString());
					StopAllCoroutines();
					ResetUI();
				}
			}, startRange, endRange); //fetch top 20 entries
	}

	//ID fetched from input field directly
	public void FetchLeaderboard(string leaderboardName)
	{
		//string lbid = Fetch_IDField.text; //get id from input field from user
		string lbid = leaderboardName;
		FetchLeaderboardWithID(lbid, 1, 20);
	}

	//id and score got from input field directly
	public void UploadScoreToLeaderboard(string lbid, int score)
	{
		/*
		int score = 0;
		string lbid = Upload_IDField.text; //get id from input field from user
		if (!Upload_ScoreField.text.Equals(""))
			score = int.Parse(Upload_ScoreField.text);
		*/

		print("try to upload " + score + " to " + lbid);
		EasySteamLeaderboards.Instance.UploadScoreToLeaderboard(lbid, score, (result) =>
		{
			//check if leaderboard successfully fetched
			if (result.resultCode == ESL_ResultCode.Success)
			{
				Debug.Log("Succesfully Uploaded!");

				//refresh lbid
				FetchLeaderboardWithID(lbid, 1, 20);
			}
			else
			{
				Debug.Log("Failed Uploading: " + result.resultCode.ToString());
				StopAllCoroutines();
				ResetUI();
			}
		});
	}
}
