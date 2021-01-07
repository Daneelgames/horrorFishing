using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

namespace EasySteamLeaderboard
{
	public class ESL_Manager : RFG_ESL_Singleton<ESL_Manager>
	{
		public static event System.Action<List<ESL_LeaderboardEntry>> onLeaderboardEntriesFetch;
		public static event System.Action<List<ESL_LeaderboardEntry>> onFriendsLeaderboardEntriesFetch;
		public static event System.Action<List<ESL_LeaderboardEntry>> onCurrentUserLeaderboardEntryFetch;
		public static event System.Action<ESL_LeaderboardEntry> onUploadScore;
		public static event System.Action onLeaderboardDoesNotExist;

		private SteamLeaderboard_t m_SteamLeaderboard;
		[HideInInspector]
		public  SteamLeaderboardEntries_t m_SteamLeaderboardEntries;
		[HideInInspector]
		public  SteamLeaderboardEntries_t m_SteamLeaderboardEntryCurrentUser;
		[HideInInspector]
		public  SteamLeaderboardEntries_t m_SteamLeaderboardEntriesFriends;

		private CallResult<LeaderboardFindResult_t> OnLeaderboardFindResultCallResult;
		private CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloadedCallResult;
		private CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloadedFriendsCallResult;
		private CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloadedForCurrentUserCallResult;
		private CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploadedCallResult;
		private CallResult<LeaderboardUGCSet_t> OnLeaderboardUGCSetCallResult;



		//vars
		int fetchRangeStart = -1;
		int fetchRangeEnd = -1;
		int scoreToUpload;
		//if find reason is uplaod then this score will be used to upload the score
		FindResultReason findReason;

		public enum FindResultReason
		{
			Fetch,
			Upload
		}

		#region IInitCallBack implementation

		public void Start()
		{
			//init stuff
			OnLeaderboardFindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
			OnLeaderboardScoresDownloadedCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
			OnLeaderboardScoresDownloadedFriendsCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloadedFriends);
			OnLeaderboardScoresDownloadedForCurrentUserCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloadedForCurrentUser);
			OnLeaderboardScoreUploadedCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
			//OnLeaderboardUGCSetCallResult = CallResult<LeaderboardUGCSet_t>.Create(OnLeaderboardUGCSet);
		}

		#endregion

		void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure)
		{
			//Debug.Log("[" + LeaderboardFindResult_t.k_iCallback + " - LeaderboardFindResult] - " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_bLeaderboardFound);

			if (pCallback.m_bLeaderboardFound != 0)
			{
				//found the lb so
				m_SteamLeaderboard = pCallback.m_hSteamLeaderboard;

				if (findReason == FindResultReason.Fetch)
				{
					if (fetchRangeStart == -1)
					{
						SteamAPICall_t resultHandleAll = SteamUserStats.DownloadLeaderboardEntries(m_SteamLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, SteamUserStats.GetLeaderboardEntryCount(m_SteamLeaderboard)); //fetch all
						OnLeaderboardScoresDownloadedCallResult.Set(resultHandleAll);
					}
					else
					{
						SteamAPICall_t resultHandleRange = SteamUserStats.DownloadLeaderboardEntries(m_SteamLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, fetchRangeStart, fetchRangeEnd); //fetch first 100 enuf
						OnLeaderboardScoresDownloadedCallResult.Set(resultHandleRange);
					}

					//also find entries for friends
					CSteamID[] friends = GetFriends();
					SteamAPICall_t resultHandleFriends = SteamUserStats.DownloadLeaderboardEntriesForUsers(m_SteamLeaderboard, friends, friends.Length);
					OnLeaderboardScoresDownloadedFriendsCallResult.Set(resultHandleFriends);

					//also find entry for current steam user
					CSteamID[] currUser = new CSteamID[1]{ SteamUser.GetSteamID() };
					SteamAPICall_t resultHandleCurrUser = SteamUserStats.DownloadLeaderboardEntriesForUsers(m_SteamLeaderboard, currUser, 1);
					OnLeaderboardScoresDownloadedForCurrentUserCallResult.Set(resultHandleCurrUser);
				}
				else
				{
					//Debug.Log("Uploading score");
					SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(m_SteamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, scoreToUpload, null, 0);
					OnLeaderboardScoreUploadedCallResult.Set(handle);
				}
			}
			else
			{
				//leaderboard does not exist
				if (onLeaderboardDoesNotExist != null)
					onLeaderboardDoesNotExist();
			}
		}

		void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
		{
			//Debug.Log("[" + LeaderboardScoresDownloaded_t.k_iCallback + " - LeaderboardScoresDownloaded] - " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_hSteamLeaderboardEntries + " --C: " + pCallback.m_cEntryCount);

			m_SteamLeaderboardEntries = pCallback.m_hSteamLeaderboardEntries;

			if (onLeaderboardEntriesFetch != null)
				onLeaderboardEntriesFetch(GetFormattedLeaderboardEntries(m_SteamLeaderboardEntries, pCallback.m_cEntryCount));
		}

		void OnLeaderboardScoresDownloadedForCurrentUser(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
		{
			//Debug.Log("[" + LeaderboardScoresDownloaded_t.k_iCallback + " - LeaderboardScoresDownloaded] - " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_hSteamLeaderboardEntries + " --C: " + pCallback.m_cEntryCount);

			m_SteamLeaderboardEntryCurrentUser = pCallback.m_hSteamLeaderboardEntries;

			if (onCurrentUserLeaderboardEntryFetch != null)
				onCurrentUserLeaderboardEntryFetch(GetFormattedLeaderboardEntries(m_SteamLeaderboardEntryCurrentUser, pCallback.m_cEntryCount));
		}

		void OnLeaderboardScoresDownloadedFriends(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
		{
			//Debug.Log("[" + LeaderboardScoresDownloaded_t.k_iCallback + " - LeaderboardScoresDownloaded] - " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_hSteamLeaderboardEntries + " --C: " + pCallback.m_cEntryCount);

			m_SteamLeaderboardEntriesFriends = pCallback.m_hSteamLeaderboardEntries;

			if (onFriendsLeaderboardEntriesFetch != null)
				onFriendsLeaderboardEntriesFetch(GetFormattedLeaderboardEntries(m_SteamLeaderboardEntriesFriends, pCallback.m_cEntryCount));
		}

		void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
		{
			//Debug.Log("[" + LeaderboardScoreUploaded_t.k_iCallback + " - LeaderboardScoreUploaded] - " + pCallback.m_bSuccess + " -- " + pCallback.m_hSteamLeaderboard + " -- " + pCallback.m_nScore + " -- " + pCallback.m_bScoreChanged + " -- " + pCallback.m_nGlobalRankNew + " -- " + pCallback.m_nGlobalRankPrevious);

			if (pCallback.m_bSuccess == 1)
			{
				ESL_LeaderboardEntry newEntry = new ESL_LeaderboardEntry(GetSteamUserName(), pCallback.m_nScore.ToString(), pCallback.m_nGlobalRankNew);

				if (onUploadScore != null)
					onUploadScore(newEntry);
			}
			else
			{
				//failed
				if (onUploadScore != null)
					onUploadScore(null); //pass null cos it failed
			}

			//Debug.Log("Uploade score ==== " + pCallback.m_bSuccess);
		}

		/*
		void OnLeaderboardUGCSet(LeaderboardUGCSet_t pCallback, bool bIOFailure)
		{
			//Debug.Log("[" + LeaderboardUGCSet_t.k_iCallback + " - LeaderboardUGCSet] - " + pCallback.m_eResult + " -- " + pCallback.m_hSteamLeaderboard);
		}
		*/


		CSteamID[] GetFriends()
		{
			int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
			List<CSteamID> friendsIDS = new List<CSteamID>();

			for (int i = 0; i < friendCount; i++)
			{
				CSteamID csid = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
				friendsIDS.Add(csid);
			}

			//also add current user to the list since it does not include and we need it to be there
			friendsIDS.Add(SteamUser.GetSteamID());

			//convert list to array
			CSteamID[] friendsArray = friendsIDS.ToArray();

			return friendsArray;
		}

		List<ESL_LeaderboardEntry> GetFormattedLeaderboardEntries(SteamLeaderboardEntries_t lbentries, int entryCount)
		{
			List<ESL_LeaderboardEntry> listOfEntries = new List<ESL_LeaderboardEntry>();

			for (int i = 0; i < entryCount; i++)
			{
				LeaderboardEntry_t lbEntry;
				SteamUserStats.GetDownloadedLeaderboardEntry(lbentries, i, out lbEntry, null, 0);
				ESL_LeaderboardEntry lbEntryFormatted = new ESL_LeaderboardEntry();
				lbEntryFormatted.PlayerName = SteamFriends.GetFriendPersonaName(lbEntry.m_steamIDUser);
				lbEntryFormatted.Score = lbEntry.m_nScore.ToString();
				lbEntryFormatted.GlobalRank = lbEntry.m_nGlobalRank;

				listOfEntries.Add(lbEntryFormatted);
			}

			return listOfEntries;
		}

		public void GetLeaderBoardEntries(string leaderboardID, int startRange, int endRange)
		{
			if (!SteamManager.Initialized)
				return;

			fetchRangeStart = startRange;
			fetchRangeEnd = endRange;

			findReason = FindResultReason.Fetch;

			SteamAPICall_t resultHandle = SteamUserStats.FindLeaderboard(leaderboardID);
			OnLeaderboardFindResultCallResult.Set(resultHandle);
		}

		public void GetAllLeaderBoardEntries(string leaderboardID)
		{
			if (!SteamManager.Initialized)
				return;

			fetchRangeStart = -1; // fetch all
			fetchRangeEnd = -1; // fetch all

			findReason = FindResultReason.Fetch;

			SteamAPICall_t resultHandle = SteamUserStats.FindLeaderboard(leaderboardID);
			OnLeaderboardFindResultCallResult.Set(resultHandle);
		}

		public void UploadScoreToLeaderboard(string leaderboardID, int score)
		{
			findReason = FindResultReason.Upload;
			scoreToUpload = score;
			
			SteamAPICall_t resultHandle = SteamUserStats.FindLeaderboard(leaderboardID);
			OnLeaderboardFindResultCallResult.Set(resultHandle);
		}

		public string GetSteamUserName()
		{
			if (!SteamManager.Initialized)
				return "??";

			return SteamFriends.GetPersonaName();
		}


		public IEnumerator LoadAvatar(CSteamID id, System.Action<Sprite> callback)
		{
			int avatarInt = Steamworks.SteamFriends.GetLargeFriendAvatar(id);
			while (avatarInt == -1)
				yield return null;

			if (avatarInt > 0)
			{
				Texture2D tex = GetSteamImageAsTexture2D(avatarInt);
				Sprite spr = GetSpriteFromTexutre2D(tex);

				if (callback != null)
					callback(spr);
			}
		}

		Texture2D GetSteamImageAsTexture2D(int iImage)
		{
			Texture2D tex = null;
			uint ImageWidth;
			uint ImageHeight;
			bool bIsValid = SteamUtils.GetImageSize(iImage, out ImageWidth, out ImageHeight);

			if (bIsValid)
			{
				byte[] Image = new byte[ImageWidth * ImageHeight * 4];

				bIsValid = SteamUtils.GetImageRGBA(iImage, Image, (int)(ImageWidth * ImageHeight * 4));
				if (bIsValid)
				{
					tex = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
					tex.LoadRawTextureData(Image);
					tex.Apply();
				}
			}

			return tex;
		}

		public static Sprite GetSpriteFromTexutre2D(Texture2D tex)
		{
			if (tex != null)
				return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
			else
				return null;
		}
	}

}
