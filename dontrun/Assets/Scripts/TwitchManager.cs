using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  System;
using  System.ComponentModel;
using  System.Net.Sockets;
using  System.IO;
using TMPro;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TwitchManager : MonoBehaviour
{
    public static TwitchManager instance;
    
    //public bool integrationEnabled = false;

    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField channelNameInput;
    
    public TextMeshProUGUI voteTimeFeedback;

    public float voteTime = 20;
    float voteTimeCurrent = 20;
    private bool voteActive = false;
    public Image voteTimerFeedback;

    public string username, password, channelName; // get password from https://twitchapps.com/tmi

    public TextMeshProUGUI connectionFeedback;
    public bool meatchWindowActive = false;
    public Animator anim;
    public Animator votingAnim;
    

    public List<string> currentVotedUsers = new List<string>(); 
    public List<int> currentVotes = new List<int>();
    public List<VotingEvent> currentVotingEvents = new List<VotingEvent>();
    
    public List<VotingEvent> votingEventsList = new List<VotingEvent>();
    
    public List<TextMeshProUGUI> votingEventsGui = new List<TextMeshProUGUI>();
    public List<int> eventIndexes = new List<int>();
    private GameManager gm;

    public bool showVotingFeedback = true;
    public bool connected = false;

    private Coroutine createVotedEventCoroutine;

    public RawImage logoImage;
    public RawImage meatchNameImage;

    public string keyString = "The Gut ";
    public List<string> devNickNames = new List<string>();
    public TextMeshProUGUI devMessageText;
    public Animator devMessageAnim;
    public AudioSource devMessageAudio;
    
    public TwitchIrcExample twitchIrcExample;
    
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gm = GameManager.instance;
    }

    public void TryToConnectToTwitch()
    {
        if (twitchClient == null || !twitchClient.Connected)
        {
            channelName = channelNameInput.text.ToLower();
            username = channelName;
            //username = usernameInput.text;
            password = passwordInput.text;
            voteTimeCurrent = voteTime;

            if (username.Length > 0 && password.Length > 0 && channelName.Length > 0)
            {
                Connect();
                StartCoroutine(CheckConnection());   
                if (gm.player && !gm.hub)
                    StartNewVoting();
            }   
        }   
    }

    IEnumerator CheckConnection()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1);
            if (TwitchIrc.Instance.stream == null)
            {
                connectionFeedback.color = Color.red;
                connectionFeedback.text = "NOT CONNECTED";
                
                anim.SetBool("Connected", false);
                Connect();
            }
            else
            {
                anim.SetBool("Connected", true);
                connectionFeedback.color = Color.green;
                connectionFeedback.text = "CONNECTED";
            }

            if (voteActive)
            {
                if (voteTimeCurrent < voteTime)
                {
                    voteTimeCurrent++;
                }
                else
                {
                    voteActive = false;
                    if (createVotedEventCoroutine == null)
                        createVotedEventCoroutine = StartCoroutine(CreateVotedEvent());
                }

                voteTimerFeedback.fillAmount = voteTimeCurrent / voteTime;
            }
            else voteTimerFeedback.fillAmount = 0;
        }
    }

    void Connect()
    {
        twitchIrcExample.Connect(username, channelName, password);
    }

    public void Disconnect()
    {
        twitchIrcExample.Disconnect();
        StopAllCoroutines();
        StopVoting();
        connectionFeedback.color = Color.red;
        connectionFeedback.text = "NOT CONNECTED";
                
        anim.SetBool("Connected", false);
    }


    public void OpenKeyUrl()
    {
        Application.OpenURL("https://twitchapps.com/tmi");
    }

    public void ToggleCanvasAnim(bool active)
    {
        meatchWindowActive = active;
        anim.SetBool("Active", active);
    }

    public void GetUserMessage(string user, string message)
    {
        print(user + ": " + message);
        if (voteActive && !gm.hub && gm.player && !currentVotedUsers.Contains(user))
        {
            int m = -1;
            if (message == ":1") m = 0;
            else if (message == ":2") m = 1;
            else if (message == ":3") m = 2;
            if (m >= 0)
            {
                currentVotes[m]++;
                currentVotedUsers.Add(user);
            }
            UpdateVotingEventsText();
        }

        if (devNickNames.Contains(user))
        {
            SetDevMessageText(message);
        }
    }

    public void SetDevMessageText(string message)
    {
        devMessageText.text = message;
        devMessageAnim.SetTrigger("Update");
        devMessageAudio.pitch = Random.Range(0.75f, 1.25f);
        devMessageAudio.Play();
    }

    public void NewGamePlusUnlocked()
    {
        string message;
        switch (GameManager.instance.language)
        {
            case 1:
                message = "МЯСНАЯ ЗОНА РАЗБЛОКИРОВАНА";
                break;
            
            case 2:
                message = "ZONA DE CARNE DESBLOQUEADA";
                break;
            
            case 3:
                message = "FLEISCHZONE ENTSPERRT";
                break;
            
            case 4:
                message = "ZONA CARNE SBLOCCATA";
                break;
            
            case 5:
                message = "ZONA DE CARNE DESBLOQUEADA";
                break;
            
            case 6:
                message = "肉ゾーンのロックが解除されました";
                break;
            
            default:
                message = "MEAT ZONE UNLOCKED";
                break;
        }
        SetDevMessageText(message);
    }

    public void StartNewVoting()
    {
        currentVotedUsers.Clear();
        currentVotes.Clear();
        currentVotingEvents.Clear();
        voteTimeCurrent = 0;

        var psc = PlayerSkillsController.instance;
        
        eventIndexes.Clear();
        for (int i = 0; i < votingEventsList.Count; i++)
        {
            if ((psc.mirrorDamage && votingEventsList[i].eventOnVote == VotingEvent.Event.MeatMirrorMemento)
                || (psc.hallucinations && votingEventsList[i].eventOnVote == VotingEvent.Event.MilkMemento)
                || (psc.randomTeleport && votingEventsList[i].eventOnVote == VotingEvent.Event.MeatMoleMemento)
                || (psc.cheapRun && votingEventsList[i].eventOnVote == VotingEvent.Event.CheapRunMemento)
                || (psc.fastRun && votingEventsList[i].eventOnVote == VotingEvent.Event.FastRunMemento)
                || (ItemsList.instance.badReputaion <= 2 &&
                    votingEventsList[i].eventOnVote == VotingEvent.Event.GutForgiveness)
                || (ItemsList.instance.badReputaion >= 5 &&
                    votingEventsList[i].eventOnVote == VotingEvent.Event.GutHatred)
                || (ItemsList.instance.activeWeapon == WeaponPickUp.Weapon.Null &&
                    ItemsList.instance.secondWeapon == WeaponPickUp.Weapon.Null &&
                    votingEventsList[i].eventOnVote == VotingEvent.Event.RepairWeapons)
                || (SpawnController.instance.chase && votingEventsList[i].eventOnVote == VotingEvent.Event.StartChase)
                ||votingEventsList[i].eventOnVote == VotingEvent.Event.EvilWeapons) // also remove evil weapons event
            {
                // skip this event
                print("skip event " + i);
            }
            else
                eventIndexes.Add(i);
            
        }

        for (int j = 0; j < 3; j++)
        {
            var r = Random.Range(0, eventIndexes.Count);
            
            currentVotingEvents.Add(votingEventsList[eventIndexes[r]]);
            eventIndexes.RemoveAt(r);
            currentVotes.Add(0);
        }

        voteActive = true;
        votingAnim.SetBool("Active", true);
        UpdateVotingEventsText();
    }

    public void PlayerDead()
    {
        votingAnim.SetBool("Active", false);
        voteActive = false;
        StopVoting();
    }

    void UpdateVotingEventsText()
    {
        for (int j = 0; j < 3; j++)
        {
            if (voteActive)
            {
                votingEventsGui[j].text = j + 1 + " - " + currentVotingEvents[j].eventDescriptions[gm.language];

                if (showVotingFeedback)
                {
                    votingEventsGui[j].text += " x" +
                                               currentVotes[j];
                }
            }
            else
                votingEventsGui[j].text = "";
        }
    }

    IEnumerator CreateVotedEvent()
    {
        // find most voted event
        int highestScore = 0;
        VotingEvent votedEvent = null;
        int _index = 0;
        for (int i = 0; i < votingEventsGui.Count; i++)
        {
            if (currentVotes[i] > highestScore)
            {
                _index = i;
                highestScore = currentVotes[i];
                votedEvent = currentVotingEvents[i];
            }
        }
        

        if (votedEvent == null)
        {
            _index = Random.Range(0, currentVotingEvents.Count);
            votedEvent = currentVotingEvents[_index]; 
        }
        
        for (int j = 0; j < 3; j++)
        {
            if (j != _index)
            {
                votingEventsGui[j].text = "";
            }
            else
            {
                votingEventsGui[j].text = j + 1 + " - " + currentVotingEvents[j].eventDescriptions[gm.language];

                if (showVotingFeedback)
                {
                    votingEventsGui[j].text += " x" +
                                               currentVotes[j];   
                }   
            }
        }
        
        //apply it
        var lg = LevelGenerator.instance;
        print("create event");
        if (!gm.hub && gm.player && gm.player.health > 0)
        {
            switch (votedEvent.eventOnVote)
            {
                case VotingEvent.Event.Heal:
                    for (int i = gm.units.Count - 1; i >= 0; i--)
                    {
                        if (gm.units[i] && gm.units[i].health > 0)
                            gm.units[i].Heal(gm.units[i].healthMax * 0.5f);
                    }
                    break;
                
                case VotingEvent.Event.ClearLevel:
                    for (var index = 0; index < LevelGenerator.instance.levelTilesInGame.Count; index++)
                    {
                        var t = LevelGenerator.instance.levelTilesInGame[index];
                        //t.SpreadToolEffect(ToolController.ToolType.Antidote, 0, 0, 0, null);
                        t.tileStatusEffect = StatusEffects.StatusEffect.Null;
                    }
                    break;
                
                case VotingEvent.Event.EvilWeapons:
                    SpawnController.instance.SpawnRandomWeapon();
                    ItemsList.instance.MakeAllWeaponsEvil();
                    gm.player.wc.MakeWeaponsEvil();
                    UiManager.instance.UpdateWeapons();
                    break;
                
                case VotingEvent.Event.GutForgiveness:
                    ItemsList.instance.AddToBadReputation(-2);
                    break;
                
                case VotingEvent.Event.GutHatred:
                    ItemsList.instance.AddToBadReputation(2);
                    break;
                
                case VotingEvent.Event.MilkMemento:
                    var skill = gm.itemList.skillsData.skills[0];
                    PlayerSkillsController.instance.AddSkill(skill);
                    gm.DifficultyUp();
                    break;
                
                case VotingEvent.Event.PlayerExplode:
                    for (int i = gm.units.Count - 1; i >= 0; i--)
                    {
                        if (gm.units.Count > i && gm.units[i] && gm.units[i].health > 0)
                        {
                            var explosion = Instantiate(gm.player.wc.allTools[0].grenadeExplosion, gm.units[i].transform.position, Quaternion.identity);
                            explosion.DestroyEffect(true);
                            yield return new WaitForSeconds(0.5f);
                        }
                    }
                    break;
                
                case VotingEvent.Event.PlumberMemento:
                    skill = gm.itemList.skillsData.skills[25];
                    PlayerSkillsController.instance.AddSkill(skill);
                    gm.DifficultyUp();
                    break;
                
                case VotingEvent.Event.PoisonLevel:
                    lg = LevelGenerator.instance;
                    for (int i = lg.levelTilesInGame.Count - 1; i >= 0; i--)
                    {
                        lg.levelTilesInGame[i].tileStatusEffect = StatusEffects.StatusEffect.Poison;
                        yield return new WaitForSeconds(0.1f);
                    }
                    break;
                
                case VotingEvent.Event.RegenWeapon:
                    gm.player.wc.SetWeaponsEffectByIndex(4);
                    break;
                
                case VotingEvent.Event.RepairWeapons:
                    gm.player.wc.RepairWeapons();
                    break;
                
                case VotingEvent.Event.SpawnRegen:
                    lg = LevelGenerator.instance;
                    for (int j = lg.levelTilesInGame.Count - 1; j >= 0; j--)
                    {
                        if (lg.levelTilesInGame[j].corridorTile && lg.levelTilesInGame[j].propOnTile == null &&
                            !lg.levelTilesInGame[j].trapTile &&
                            lg.levelTilesInGame[j].tileStatusEffect == StatusEffects.StatusEffect.Null)
                        {
                            lg.levelTilesInGame[j].StartStatusEffectOnTile(4, 90);
                            break;
                        }
                    }
                    break;
                
                case VotingEvent.Event.StartChase:
                    SpawnController.instance.StartBadRepChase();
                    break;
                
                case VotingEvent.Event.CheapRunMemento:
                    skill = gm.itemList.skillsData.skills[3];
                    PlayerSkillsController.instance.AddSkill(skill);
                    gm.DifficultyUp();
                    break;
                
                case VotingEvent.Event.FastRunMemento:
                    skill = gm.itemList.skillsData.skills[5];
                    PlayerSkillsController.instance.AddSkill(skill);
                    gm.DifficultyUp();
                    break;
                
                case VotingEvent.Event.MeatMirrorMemento:
                    skill = gm.itemList.skillsData.skills[15];
                    PlayerSkillsController.instance.AddSkill(skill);
                    gm.DifficultyUp();
                    break;
                
                case VotingEvent.Event.MeatMoleMemento:
                    skill = gm.itemList.skillsData.skills[0];
                    PlayerSkillsController.instance.AddSkill(skill);
                    gm.DifficultyUp();
                    break;
                
                case VotingEvent.Event.MoreMeatTraps:
                    SpawnController.instance.SpawnMeatTraps(15);
                    break;
                
                case VotingEvent.Event.RandomWeaponEffect:
                    WeaponControls.instance.RandomizeWeaponsStatuses();
                    SpawnController.instance.SpawnRandomWeapon();
                    gm.itemList.GenerateAllWeaponsOnFloor();
                    break;
                
                case VotingEvent.Event.SpawnEyeTaker:
                    SpawnController.instance.SpawnEyeTaker(1);
                    break;
                
                case VotingEvent.Event.SpawnEye2Takers:
                    SpawnController.instance.SpawnEyeTaker(2);
                    break;
                case VotingEvent.Event.SpawnEye3Takers:
                    SpawnController.instance.SpawnEyeTaker(3);
                    break;
                case VotingEvent.Event.SpawnEye5Takers:
                    SpawnController.instance.SpawnEyeTaker(5);
                    break;
                case VotingEvent.Event.SpawnEye10Takers:
                    SpawnController.instance.SpawnEyeTaker(10);
                    break;
            }
        }
        yield return new WaitForSeconds(voteTime);
        if (gm.player && gm.player.health > 0 && !gm.hub)
            StartNewVoting();
        
        createVotedEventCoroutine = null;
    }

    public void ToggleVoteTime()
    {
        voteTime += 10;
        if (voteTime > 90)
            voteTime = 10;
        else if (voteTime < 10)
            voteTime = 90;
        
        voteTimeFeedback.text = voteTime + "s";
    }

    public void ToggleCounter()
    {
        showVotingFeedback = !showVotingFeedback;
    }

    public void ToggleMeatch()
    {
        ToggleCanvasAnim(!meatchWindowActive);
    }

    public void ToggleMeatchImages(bool active)
    {
        meatchNameImage.enabled = active;
        logoImage.enabled = active;
    }

    public void StopVoting()
    {
        voteActive = false;
        if (createVotedEventCoroutine != null)
            StopCoroutine(createVotedEventCoroutine);
        
        for (int j = 0; j < 3; j++)
        {
            votingEventsGui[j].text = "";
        }
        
        voteTimerFeedback.fillAmount = 0;
        
        ToggleMeatch();
    }
}

[Serializable]
public class VotingEvent
{
    public enum Event
    {
        Heal, StartChase, MeatMirrorMemento, MeatMoleMemento, MilkMemento, PoisonLevel, 
        PlayerExplode, RepairWeapons, RegenWeapon, EvilWeapons, RandomWeaponEffect,
        SpawnEyeTaker, MoreMeatTraps, PlumberMemento, FastRunMemento, CheapRunMemento,
        GutForgiveness, GutHatred, ClearLevel, SpawnRegen, SpawnEye2Takers, SpawnEye3Takers, SpawnEye5Takers, SpawnEye10Takers
    }
    
    // if regen weapon / talking weapon and player has nothing - spawn needed weapon somewhere

    public Event eventOnVote = Event.Heal;
    public List<string> eventDescriptions;
}
