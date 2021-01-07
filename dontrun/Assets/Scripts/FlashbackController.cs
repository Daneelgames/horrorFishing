using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlashbackController : MonoBehaviour
{
    public Transform playerSpawner;
    public GameObject transitionEvent;
    public float delayBeforeLevelStart = 0;

    public static FlashbackController instance;

    public float minimumTimeToExplore = 15;
    float currentTimeToExplore = 0;
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        if (!GameManager.instance.hub)
        {
            MovePlayerIntoRoom();
        }
    }

    void Update()
    {
        if (currentTimeToExplore < minimumTimeToExplore)
            currentTimeToExplore += Time.deltaTime;
    }

    void MovePlayerIntoRoom()
    {
        var p = PlayerMovement.instance;
        UiManager.instance.Init(true);
        p.hc.invincible = true;
        p.teleport = true;
        p.transform.position = playerSpawner.position;
        p.playerHead.position = p.transform.position;
        PlayerAudioController.instance.GetNewFootSteps(0);
        p.teleport = false;
        p.StartLevel();
    }

    public void ActivateTransitionEventBeforeStartTheLevel()
    {
        StartCoroutine(StartLevelAfterDelay());
    }

    IEnumerator StartLevelAfterDelay()
    {
        while (currentTimeToExplore < minimumTimeToExplore)
        {
            yield return null;
        }
        
        if (transitionEvent)
            transitionEvent.SetActive(true);
        
        yield return new WaitForSeconds(delayBeforeLevelStart);
        
        UiManager.instance.Init(false);

        var lg = LevelGenerator.instance;
        var pm = PlayerMovement.instance;
        // move player back to elevator
        if (pm.interactionController.objectInHands)
            pm.interactionController.objectInHands.Drop();
        PlayerAudioController.instance.FlashBackOver();
        pm.Teleport(true);
        pm.transform.position = lg.playerSpawner.position;
        GameManager.instance.ls.PlayerTeleported();
        pm.playerHead.position = pm.transform.position;
        PlayerMovement.instance.StartLevel();
        pm.Teleport(false);
        pm.hc.invincible = false;
        
        // unload flashbackScene 
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(2));
    }
}
