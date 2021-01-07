using System.Collections;
using System.Collections.Generic;

namespace EasySteamLeaderboard
{
	public enum ESL_ResultCode
	{
		Failed,
		Success,
		DoesNotExist,
		TimedOut,
		SteamworksNotInitialized
	}
	
	public class ESL_Leaderboard 
	{
		public string ID;
		public List<ESL_LeaderboardEntry> GlobalEntries;
		public List<ESL_LeaderboardEntry> FriendsEntries;
		public ESL_LeaderboardEntry SteamUserEntry;
		public ESL_ResultCode resultCode;

	}

	public class ESL_LeaderboardEntry
	{
		public string PlayerName;
		public string Score;
		public int GlobalRank;

		public ESL_LeaderboardEntry(){}

		public ESL_LeaderboardEntry(string name, string score, int rank)
		{
			this.PlayerName = name;
			this.Score = score;
			this.GlobalRank = rank;
		}

		public ESL_LeaderboardEntry(ESL_LeaderboardEntry entry)
		{
			this.PlayerName = entry.PlayerName;
			this.Score = entry.Score;
			this.GlobalRank = entry.GlobalRank;
		}
	}

	public class ESL_UploadResult
	{
		public ESL_LeaderboardEntry updatedEntry;
		public ESL_ResultCode resultCode;
	}
}
