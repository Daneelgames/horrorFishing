using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class IronMaidenController : NetworkBehaviour
{
    public static IronMaidenController instance;
    private MeatBrainController brainOnLevel;
    public float workingDistance = 8;
    private float currentDistance = 1000;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (GutProgressionManager.instance.playerFloor == 1)
        {
            // clear inventory on start of coop play
            ItemsList.instance.ResetPlayerInventory();
        }
    }

    public void SetBrainOnLevel(MeatBrainController brain)
    {
        if (brainOnLevel == null)
            brainOnLevel = brain;
        else
        {
            // GAME OVER
            // RESTART THE FLOOR
            ItemsList.instance.savedTools.Clear();
            ItemsList.instance.ResetPlayerInventory();
        
            GameManager.instance.SaveGame();

            if (isServer)
                GLNetworkWrapper.instance.BothPlayersDied(true);
            return;
        }
        
        // ONLY ON SERVER
        if (!isServer) return; 
        
        StartCoroutine(GetBrainDistance());
    }

    IEnumerator GetBrainDistance()
    {
        currentDistance = 1000;
        
        while (currentDistance > workingDistance)
        {
            yield return new WaitForSeconds(0.1f);
            //if (brainOnLevel.pickedUp)
            currentDistance = Vector3.Distance(transform.position, brainOnLevel.transform.position);
        }
        
        // brain is close
        print("FUCK YOU MULTIPLAYER");
        GLNetworkWrapper.instance.RespawnDeadPlayer();
    }
    public void ReleasePlayerOnClient()
    {
        print("Release player on client");
        brainOnLevel.ReleasePlayer();
        brainOnLevel = null;
    }
}
