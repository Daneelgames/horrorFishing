using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class SteamworksLobby : MonoBehaviour
{
    public static SteamworksLobby instance;
    
    [SerializeField]private GameObject buttons;
    [SerializeField]private GameObject connectionFeedback;
    [SerializeField]private GameObject closeCoopButton;
    private NetworkManager _networkManager;

    protected Callback<LobbyCreated_t> lobbyCreatedCallback;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    private const string HostAddressKey = "HostAddress";
    
    public List<Toggle> biomesList = new List<Toggle>();
    public List<GameObject> biomesListDummies = new List<GameObject>();
    private int lastCheckpoint = 0;

    public GameObject chooseLevel;
    
    public bool hosting = false;
    public int selectedCheckpoint = 0;
    
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        _networkManager = NetworkManager.singleton;
        
        if (!SteamManager.Initialized) return;

        lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void SetCheckpoints(int checkpoints)
    {
        lastCheckpoint = checkpoints;
    }

    public void HostLobby()
    {
        //SteamFriends.ActivateGameOverlay("Players");
        hosting = true;
        ToggleButtons(false);
        ToggleConnectionFeedback(true);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, _networkManager.maxConnections);
    }

    public void ToggleButtons(bool active)
    {
        buttons.SetActive(active);
        chooseLevel.SetActive(active);
        closeCoopButton.SetActive(active);
        if (active)
        {
            SetStartBiome(0);
        }
    }

    public void ToggleConnectionFeedback(bool active)
    {
        connectionFeedback.SetActive(active);
        closeCoopButton.SetActive(active);
    }

    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            hosting = false;
            ToggleButtons(true);
            return;
        }
        _networkManager.StartHost();
        
        /*
         OLD
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamFriends.ActivateGameOverlay("Players");
        */
        
        var lobbyId = new CSteamID(callback.m_ulSteamIDLobby); 

        SteamMatchmaking.SetLobbyData(
            lobbyId,
            HostAddressKey, SteamUser.GetSteamID().ToString());
           
        SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);   
    }

    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        print("OnGameLobbyJoinRequested");
        _networkManager.StopServer();
        
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    void OnLobbyEntered(LobbyEnter_t callback)
    {
        print("Lobby entered. Network server active is " + NetworkServer.active);
        if (NetworkServer.active) return;
        
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        _networkManager.networkAddress = hostAddress;
        _networkManager.StartClient();
        ToggleButtons(false);
    }

    public void SetStartBiome(int index)
    {
        selectedCheckpoint = index;
        
        // uncheck every other biome
        for (int i = 0; i < biomesList.Count; i++)
        {
            if (i > lastCheckpoint)
            {
                biomesList[i].gameObject.SetActive(false);
                biomesListDummies[i].gameObject.SetActive(true);
            }
            else
            {
                biomesList[i].gameObject.SetActive(true);
                biomesListDummies[i].gameObject.SetActive(false);
            }
        }
    }
}