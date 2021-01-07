using System.Collections.Generic;
using Mirror;
using PlayerControls;
using UnityEngine;
using Random = UnityEngine.Random;

public class ElevatorController : MonoBehaviour
{
    public static ElevatorController instance;
    
    public List<Interactable> padlocksPrefabs;
    public List<Interactable> padlocksInGame;
    public Transform padlocksParent;
    public InteractableContainer key;
    public AudioSource au;
    public InteractiveButton elevatorButton;
    public GameObject musicAndLight;

    public Transform playerSpawner;

    GameManager gm;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        var lg = LevelGenerator.instance;
        if (lg)
        {
            lg.playerSpawner = playerSpawner;
        }

        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            PlayerMovement.instance.Teleport(true);
            PlayerMovement.instance.transform.position = playerSpawner.transform.position;
            PlayerMovement.instance.Teleport(false);   
        }
        
        /*
        if (GameManager.instance.hub && (QuestManager.instance.activeQuestsIndexes.Contains(2) || ItemsList.instance.herPiecesApplied.Count > 0))
            Deactivate();
            */
    }

    /*
    void OnEnable()
    {
        if (GameManager.instance.hub && (QuestManager.instance.activeQuestsIndexes.Contains(2) || ItemsList.instance.herPiecesApplied.Count > 0))
            Deactivate();
    }
    */

    public GameObject CreatePadlocks(int i)
    {
        if (padlocksPrefabs.Count > 0)
        {
            var padlocksTemp = new List<Interactable>(padlocksPrefabs);
        
            int r = Random.Range(0, padlocksTemp.Count);
        
            var newPadlock = Instantiate(padlocksTemp[r], padlocksParent.position, Quaternion.identity);
            padlocksTemp.RemoveAt(r);
        
            if (padlocksTemp.Count < 1)
                padlocksTemp = new List<Interactable>(padlocksPrefabs);
        
            padlocksInGame.Add(newPadlock);
            var padlockTransform = newPadlock.transform;
            padlockTransform.parent = padlocksParent;
            // this next line makes chains on clients to move to vec zero gobal
            // FIX IT
            padlockTransform.localPosition = Vector3.zero + Vector3.forward * (0.25f * i); //Keep multiply order for peformance
            padlockTransform.Rotate(Vector3.up, 180);
            padlockTransform.parent = null;
            
            return newPadlock.gameObject;
        }
        return null;
    }

    public void StopMusic()
    {
        if (au)
            au.Stop();
    }

    public void Activate()
    {
        elevatorButton.Activate();
        musicAndLight.SetActive(true);
    }
    public void Deactivate()
    {
        elevatorButton.Deactivate();
        musicAndLight.SetActive(false);
    }
}
