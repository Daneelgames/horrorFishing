using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Steamworks;

namespace EasySteamLeaderboard
{
	public class EasySteamLeaderboards : RFG_ESL_Singleton<EasySteamLeaderboards>
	{

		//vars
		bool globalFetchComplete;
		bool friendsFetchComplete;
		bool steamUserFetchComplete;
		bool uploadComplete;

		ESL_Leaderboard latestLeaderboard;
		ESL_UploadResult latestUploadResult;

		Coroutine waitCR;
		Coroutine timeoutUploadCR;
		Coroutine timeoutFetchCR;
		float timeout = 20;
		//timeout after 20 seconds

		//callback results
		System.Action<ESL_Leaderboard> fetchResultCallBack;
		System.Action<ESL_UploadResult> uploadResultCallBack;

		ESL_Manager.FindResultReason resultType;

		void OnEnable()
		{
			ESL_Manager.onLeaderboardEntriesFetch += SteamLeaderboardsManager_onLeaderboardEntriesFetch;
			ESL_Manager.onFriendsLeaderboardEntriesFetch += SteamLeaderboardsManager_onFriendsLeaderboardEntriesFetch;
			ESL_Manager.onCurrentUserLeaderboardEntryFetch += SteamLeaderboardsManager_onCurrentUserLeaderboardEntryFetch;
			ESL_Manager.onLeaderboardDoesNotExist += SteamLeaderboardsManager_onLeaderboardDoesNotExist;
			ESL_Manager.onUploadScore += ESL_Manager_onUploadScore;
		}

		void OnDisable()
		{
			ESL_Manager.onLeaderboardEntriesFetch -= SteamLeaderboardsManager_onLeaderboardEntriesFetch;
			ESL_Manager.onFriendsLeaderboardEntriesFetch -= SteamLeaderboardsManager_onFriendsLeaderboardEntriesFetch;
			ESL_Manager.onCurrentUserLeaderboardEntryFetch -= SteamLeaderboardsManager_onCurrentUserLeaderboardEntryFetch;
			ESL_Manager.onLeaderboardDoesNotExist -= SteamLeaderboardsManager_onLeaderboardDoesNotExist;
			ESL_Manager.onUploadScore -= ESL_Manager_onUploadScore;
		}

		void ESL_Manager_onUploadScore(ESL_LeaderboardEntry entry)
		{
			if (entry != null) //else it will be failed by default
				latestUploadResult.resultCode = ESL_ResultCode.Success;
			else
				latestUploadResult.resultCode = ESL_ResultCode.Failed;

			latestUploadResult.updatedEntry = entry;
			uploadComplete = true;
			uploadResultCallBack(latestUploadResult);
		}

		void SteamLeaderboardsManager_onLeaderboardDoesNotExist()
		{
			StopAllCoroutines();
			ResetParams();

			//return with no success
			if (resultType == ESL_Manager.FindResultReason.Fetch)
			{
				latestLeaderboard.resultCode = ESL_ResultCode.DoesNotExist;
				fetchResultCallBack(latestLeaderboard);
			}
			else //upload lb id does not exist
			{
				latestUploadResult.resultCode = ESL_ResultCode.DoesNotExist;
				uploadResultCallBack(latestUploadResult);
			}
		}

		void SteamLeaderboardsManager_onCurrentUserLeaderboardEntryFetch(List<ESL_LeaderboardEntry> entries)
		{
			if (latestLeaderboard != null)
			{
				if (entries.Count > 0)
				{
					ESL_LeaderboardEntry userEntry = new ESL_LeaderboardEntry(entries[0]);
					latestLeaderboard.SteamUserEntry = userEntry;
				}
				else
				{
					latestLeaderboard.SteamUserEntry = null;
				}

				//set fetch complete
				steamUserFetchComplete = true;
			}
		}

		void SteamLeaderboardsManager_onFriendsLeaderboardEntriesFetch(List<ESL_LeaderboardEntry> entries)
		{
			if (latestLeaderboard != null)
			{
				if (entries.Count > 0)
				{
					//copy to new list
					List<ESL_LeaderboardEntry> lbEntries = new List<ESL_LeaderboardEntry>(entries);
					latestLeaderboard.FriendsEntries = lbEntries;
				}
				else
				{
					latestLeaderboard.FriendsEntries = new List<ESL_LeaderboardEntry>();
				}

				//set fetch complete
				friendsFetchComplete = true;
				print("try to get friends list. " + friendsFetchComplete);
			}
		}

		void SteamLeaderboardsManager_onLeaderboardEntriesFetch(List<ESL_LeaderboardEntry> entries)
		{
			if (latestLeaderboard != null)
			{
				if (entries.Count > 0)
				{
					//copy to new list
					List<ESL_LeaderboardEntry> lbEntries = new List<ESL_LeaderboardEntry>(entries);
					latestLeaderboard.GlobalEntries = lbEntries;
				}
				else
				{
					latestLeaderboard.GlobalEntries = new List<ESL_LeaderboardEntry>();
				}
				//set fetch complete
				globalFetchComplete = true;
			}

		}

		/// <summary>
		/// Fetches the leaderboard with custom range
		/// By default 1-100 will be fetched.
		/// </summary>
		/// <param name="leaderbaordID">Leaderbaord ID.</param>
		/// <param name="resultCallback">Result callback.</param>
		/// <param name="startRange">Start range.</param>
		/// <param name="endRange">End range default first 100 records will be fetched</param>
		public void GetLeaderboard(string leaderboardID, System.Action<ESL_Leaderboard> resultCallback, int startRange = 1, int endRange = 100)
		{
			StopAllCoroutines();
			ResetParams();

			latestLeaderboard = new ESL_Leaderboard();
			latestLeaderboard.ID = leaderboardID;
			fetchResultCallBack = resultCallback;
			resultType = ESL_Manager.FindResultReason.Fetch;

			if (!SteamManager.Initialized)
			{
				latestLeaderboard.resultCode = ESL_ResultCode.SteamworksNotInitialized;
				resultCallback(latestLeaderboard); //return empty leaderboard
				return;
			}

			ESL_Manager.Instance.GetLeaderBoardEntries(leaderboardID, startRange, endRange);

			//wait for fetching of results
			if (waitCR != null)
				StopCoroutine(waitCR);
			
			waitCR = StartCoroutine(WaitTillFetchComplete(resultCallback));
			timeoutFetchCR = StartCoroutine(WaitForFetchTimeOut(resultCallback));
				
		}

		/// <summary>
		/// NOT RECOMMNEDED To use this since a leaderboard might have a huge number of records
		/// Fetchs the full leaderboard with all entries
		/// </summary>
		/// <param name="leaderbaordID">Leaderbaord I.</param>
		/// <param name="resultCallback">Result callback.</param>
		public void GetFullLeaderboard(string leaderboardID, System.Action<ESL_Leaderboard> resultCallback)
		{
			StopAllCoroutines();
			ResetParams();
			
			latestLeaderboard = new ESL_Leaderboard();
			latestLeaderboard.ID = leaderboardID;
			fetchResultCallBack = resultCallback;
			resultType = ESL_Manager.FindResultReason.Fetch;

			if (!SteamManager.Initialized)
			{
				latestLeaderboard.resultCode = ESL_ResultCode.SteamworksNotInitialized;
				resultCallback(latestLeaderboard); //return empty leaderboard
				return;
			}

			ESL_Manager.Instance.GetAllLeaderBoardEntries(leaderboardID);

			//wait for fetching of results
			if (waitCR != null)
				StopCoroutine(waitCR);

			waitCR = StartCoroutine(WaitTillFetchComplete(resultCallback));

			//no time out for full fetch since it might take a lot of time!
		}

		public void UploadScoreToLeaderboard(string leaderboardID, int score, System.Action<ESL_UploadResult> resultCallback)
		{
			StopAllCoroutines();
			ResetParams();

			latestUploadResult = new ESL_UploadResult();
			uploadResultCallBack = resultCallback;
			resultType = ESL_Manager.FindResultReason.Upload;

			if (!SteamManager.Initialized)
			{
				latestUploadResult.resultCode = ESL_ResultCode.SteamworksNotInitialized;
				resultCallback(latestUploadResult);
				return;
			}

			ESL_Manager.Instance.UploadScoreToLeaderboard(leaderboardID, score);

			//wait for fetching of results
			if (waitCR != null)
				StopCoroutine(waitCR);

			waitCR = StartCoroutine(WaitTillUploadComplete(resultCallback));
			timeoutUploadCR = StartCoroutine(WaitForUploadTimeOut(resultCallback));
		}

		IEnumerator WaitTillUploadComplete(System.Action<ESL_UploadResult> resultCallback)
		{
			while (!uploadComplete)
			{
				yield return null;
			}

			//uploaded, now callback
			latestUploadResult.resultCode = ESL_ResultCode.Success;
			resultCallback(latestUploadResult);

			if (timeoutUploadCR != null)
			{
				StopCoroutine(timeoutUploadCR);
				timeoutUploadCR = null;
			}

			//reset flags for next call
			ResetParams();
		}

		IEnumerator WaitTillFetchComplete(System.Action<ESL_Leaderboard> resultCallback)
		{
			//while (!globalFetchComplete || !friendsFetchComplete || !steamUserFetchComplete)
			while (!globalFetchComplete || !steamUserFetchComplete)
			{
				print(globalFetchComplete + " / " + friendsFetchComplete + " / " + steamUserFetchComplete);
				yield return null;
			}

			//all fetched, now callback
			latestLeaderboard.resultCode = ESL_ResultCode.Success;
			resultCallback(latestLeaderboard);

			if (timeoutFetchCR != null)
			{
				StopCoroutine(timeoutFetchCR);
				timeoutFetchCR = null;
			}

			//reset flags for next call
			ResetParams();
		}

		IEnumerator WaitForFetchTimeOut(System.Action<ESL_Leaderboard> resultCallback)
		{
			yield return new WaitForSecondsRealtime(timeout);
			if (latestLeaderboard.resultCode == ESL_ResultCode.Failed) //not yet fetched .. timeout and return
			{
				latestLeaderboard.resultCode = ESL_ResultCode.TimedOut; //specify timed out error code
				if (waitCR != null)
					StopCoroutine(waitCR);

				resultCallback(latestLeaderboard);
				//reset flags for next call
				ResetParams();

				timeoutFetchCR = null;
			}
		}

		IEnumerator WaitForUploadTimeOut(System.Action<ESL_UploadResult> resultCallback)
		{
			yield return new WaitForSecondsRealtime(timeout);
			if (latestUploadResult.resultCode == ESL_ResultCode.Failed) //not yet fetched .. timeout and return
			{
				latestUploadResult.resultCode = ESL_ResultCode.TimedOut; //specify timed out error code
				if (waitCR != null)
					StopCoroutine(waitCR);

				resultCallback(latestUploadResult);
				//reset flags for next call
				ResetParams();

				timeoutUploadCR = null;
			}
		}

		void ResetParams()
		{
			globalFetchComplete = friendsFetchComplete = steamUserFetchComplete = uploadComplete = false;
			waitCR = null;
		}

		public void LoadSteamPicture(CSteamID id, System.Action<Sprite> callback)
		{
			StartCoroutine(ESL_Manager.Instance.LoadAvatar(id, callback));
		}
	}
}
