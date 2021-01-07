using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using EasySteamLeaderboard;

public class ESL_LeaderboardEntryUI : MonoBehaviour
{
	//ase
	public Text RankText;
	public Text PlayerNameText;
	public Text ScoreText;

	public void Initialize(ESL_LeaderboardEntry entry)
	{
		if (entry == null)
		{
			Reset();
			return;
		}

		PlayerNameText.text = entry.PlayerName;
		RankText.text = entry.GlobalRank.ToString();
		ScoreText.text = entry.Score;
	}

	public void Initialize(string pname, int rank, string score)
	{
		PlayerNameText.text = pname;
		RankText.text = rank.ToString();
		ScoreText.text = score;
	}

	public void Reset()
	{
		RankText.text = "-";
		PlayerNameText.text = "-";
		ScoreText.text = "-";
	}
}
