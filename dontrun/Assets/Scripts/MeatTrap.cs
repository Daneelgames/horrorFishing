using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class MeatTrap : MonoBehaviour
{
    public int spreadRangeMin = 2;
    public int spreadRangeMax = 6;
    public StatusEffects.StatusEffect effect = StatusEffects.StatusEffect.Null;
    public AudioSource trapAu;
    public List<string> stepOnTrapStrings = new List<string>();
    
    private bool activated = false;
    private TileController usedTile = null;
    private Collider collider;
    
    private LevelGenerator lg;
    private GameManager gm;
    private HealthController hc;
    private PlayerMovement pm;
    
    void Start()
    {
        gm = GameManager.instance;
        lg = LevelGenerator.instance;
        collider = GetComponent<Collider>();
        pm = PlayerMovement.instance;
        hc = GetComponent<HealthController>();
        
        foreach (var t in lg.levelTilesInGame)
        {
            if (Vector3.Distance(transform.position, t.transform.position) <= 1)
            {
                usedTile = t;
                break;
            }
        }

        var gpm = GutProgressionManager.instance;
        var r = Random.value;
        if (r <= 0.15f)
        {
            effect = StatusEffects.StatusEffect.Poison;
        }
        else if (r <= 0.3f) 
        {
            effect = StatusEffects.StatusEffect.Fire;
        }
        else if (r <= 0.45f) 
        {
            effect = StatusEffects.StatusEffect.Bleed;
        }
        else if (r <= 0.6f) 
        {
            effect = StatusEffects.StatusEffect.Rust;
        }
        else if (r <= 0.75f) 
        {
            if (gpm.playerFloor < 6)
                effect = StatusEffects.StatusEffect.Poison;
            else
                effect = StatusEffects.StatusEffect.GoldHunger;
        }
        else if (r <= 0.8f) 
        {
            if (gpm.playerFloor < 4)
                effect = StatusEffects.StatusEffect.Fire;
            else
                effect = StatusEffects.StatusEffect.Cold;
        }
        else
        {
            effect = StatusEffects.StatusEffect.Poison;
        }
    }

    void OnTriggerStay(Collider coll)
    {
        if (!activated)
        {
            if (coll.gameObject.layer == 11 && Vector3.Distance(transform.position, pm.transform.position) <= 30)
            {
                if (coll.gameObject == PlayerMovement.instance.gameObject)
                    UiManager.instance.MeatTrapFeedback(stepOnTrapStrings[gm.language]);

                Activate();
            }
        }
    }

    public void Disarm()
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.DisarmMeatTrap(gm.units.IndexOf(hc));
        }
        else
        {
            DisarmOnClient();
        }
    }

    public void DisarmOnClient()
    {
        activated = true;
        collider.enabled = false;
        //if (InteractionController.instance.selectedItem == hc.npcInteractor)
        if (UiManager.instance.dialogueInteractor == hc.npcInteractor)
            UiManager.instance.HideDialogue(3);
        
        if (hc.npcInteractor)
            hc.npcInteractor.gameObject.SetActive(false);   
    }

    public void Activate()
    {
        int range = Random.Range(spreadRangeMin, spreadRangeMax + 1);
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.ActivateMeatTrap(gm.units.IndexOf(hc), range);
        }
        else
        {
            ActivateOnClient(range);
        }
        
        Disarm();
    }

    public void ActivateOnClient(int range)
    {
        trapAu.pitch = Random.Range(0.75f, 1.25f);
        trapAu.Play();
                
        usedTile.SpreadStatusEffect(effect, range, 1, 30, null);

        if (UiManager.instance.dialogueInteractor == hc.npcInteractor)
            UiManager.instance.HideDialogue(0.1f);   
    }

    public void Death()
    {
        Disarm();
    }
}
