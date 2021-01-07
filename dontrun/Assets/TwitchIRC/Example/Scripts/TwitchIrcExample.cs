using System;
using System.Collections.Generic;
using Irc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TwitchIrcExample : MonoBehaviour
{
    public TMP_InputField UsernameText;
    public TMP_InputField TokenText;
    public TMP_InputField ChannelText;

    private TwitchManager tm;

    void Start()
    {
        tm = TwitchManager.instance;
        
        //Subscribe for events
        TwitchIrc.Instance.OnChannelMessage += OnChannelMessage;
        TwitchIrc.Instance.OnUserLeft += OnUserLeft;
        TwitchIrc.Instance.OnUserJoined += OnUserJoined;
        TwitchIrc.Instance.OnServerMessage += OnServerMessage;
        TwitchIrc.Instance.OnExceptionThrown += OnExceptionThrown;
    }

    public void Connect(string name, string channel, string password)
    {
        /*
        TwitchIrc.Instance.Username = UsernameText.text;
        TwitchIrc.Instance.OauthToken = TokenText.text;
        TwitchIrc.Instance.Channel = ChannelText.text;
        */
        
        TwitchIrc.Instance.Username = name;
        TwitchIrc.Instance.OauthToken = password;
        TwitchIrc.Instance.Channel = channel;

        TwitchIrc.Instance.Connect();
    }

    public void Disconnect()
    {
        TwitchIrc.Instance.Disconnect();
    }

    //Open URL
    public void GoUrl(string url)
    {
        Application.OpenURL(url);
    }

    //Receive message from server
    void OnServerMessage(string message)
    {
        Debug.Log(message);
    }

    //Receive username that has been left from channel 
    void OnChannelMessage(ChannelMessageEventArgs channelMessageArgs)
    {
        tm.GetUserMessage(channelMessageArgs.From, channelMessageArgs.Message);
        print(channelMessageArgs.Message);
        //Debug.Log("MESSAGE: " + channelMessageArgs.From + ": " + channelMessageArgs.Message);
    }

    //Get the name of the user who joined to channel 
    void OnUserJoined(UserJoinedEventArgs userJoinedArgs)
    {
        Debug.Log("USER JOINED: " + userJoinedArgs.User);
    }


    //Get the name of the user who left the channel.
    void OnUserLeft(UserLeftEventArgs userLeftArgs)
    {
        Debug.Log("USER JOINED: " + userLeftArgs.User);
    }

    //Receive exeption if something goes wrong
    private void OnExceptionThrown(Exception exeption)
    {
        Debug.Log(exeption);
    }
   
}
