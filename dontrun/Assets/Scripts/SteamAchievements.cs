using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamAchievements : MonoBehaviour
{
    public static SteamAchievements instance;
    private bool unlockTest = false;
    

    void Awake()
    {
        instance = this;
    }

    public void UnlockSteamAchievement(string ID)
    {
        TestSteamAchievement(ID);
        if (!unlockTest)
        {
            SteamUserStats.SetAchievement(ID);
            SteamUserStats.StoreStats();
        }
    }

    
    //test if achievement already gotten
    void TestSteamAchievement(string ID)
    {
        SteamUserStats.GetAchievement(ID, out unlockTest);
    }
}
