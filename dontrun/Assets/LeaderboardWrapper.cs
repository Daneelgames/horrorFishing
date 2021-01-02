using System.Collections;
using System.Collections.Generic;
using EasySteamLeaderboard;
using UnityEngine;

public class LeaderboardWrapper : MonoBehaviour
{
    public static LeaderboardWrapper instance;

    void Awake()
    {
        instance = this;
    }
    
    public void UploadScoreToLeaderboard(string lbid, int score)
    {
        /*
        int score = 0;
        string lbid = Upload_IDField.text; //get id from input field from user
        if (!Upload_ScoreField.text.Equals(""))
            score = int.Parse(Upload_ScoreField.text);
        */

        EasySteamLeaderboards.Instance.UploadScoreToLeaderboard(lbid, score, (result) =>
        {
            //check if leaderboard successfully fetched
            if (result.resultCode == ESL_ResultCode.Success)
            {
                Debug.Log("Succesfully Uploaded!");

            }
            else
            {
                Debug.Log("Failed Uploading: " + result.resultCode.ToString());
            }
        });
    }
}
