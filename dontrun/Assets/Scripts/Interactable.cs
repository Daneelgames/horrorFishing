using System.Collections;
using System.Collections.Generic;
using ExtendedItemSpawn;
using UnityEngine;
using TMPro;
using CerealDevelopment.TimeManagement;
using PlayerControls;

public class Interactable : MonoBehaviour, 
	IUpdatable
{
    public bool canBeInteracted = true;
    
    public string itemName = "Pistol ammo";
    public List<string> itemNames = new List<string>();
    public ResourcePickUp pickUp;
    public AmmoPickUp ammoPickUp;
    public WeaponPickUp weaponPickUp;
    public LockObject lockObject;
    public DoorController door;
    public PortableObject portable;
    public TransportObject transport;
    public MeatBrainController meatBrain;
    public SetRandomTrackOnStart setRandomTrackOnStart;
    public NoteTextGenerator note;
    public NpcController npc;
    public InteractiveButton button;
    public TextMeshProUGUI nameGui;
    public Animator nameGuiAnim;
    public Transform canvasParent;
    public Light light;
    bool showName = false;

    public GameObject hint;
    [Header("This visual is controller by meat hole")]
    public GameObject visual;
    public float hintDistance = 10;
    public GameObject mapMarker;
    public GameObject mapMarkerCross;
    public Rigidbody rb;

    public bool canCreateMeatHole = false;
    public HealthController activeMeatHole;
    public Interactable itemInsideMeatHole;
    
    [Header("Locals")]
    public List<string> pickedByPlayerMessage = new List<string>();

    Coroutine hideHuiCoroutine;

    UiManager ui;
    GameManager gm;
    ItemsList il;
    private MouseLook ml;
    private PlayerSkillsController psc;
    private InteractionController ic;

    LayerMask floorLayerMask = 1 << 21;

    void Awake()
    {
        gm = GameManager.instance;
        ml = MouseLook.instance;
        psc = PlayerSkillsController.instance;
        ic = InteractionController.instance;

        
        ui = UiManager.instance;
        il = ItemsList.instance;
        
        il.interactables.Add(this);
        
        if (door) door.interactableController = this;
    }
    
    public void Start()
    {
        //if (pickUp || ammoPickUp || weaponPickUp) transform.parent = null;
        
        if (itemNames.Count > gm.language)
            nameGui.text = itemNames[gm.language];
        else
            nameGui.text = itemNames[0];
        
        /*
        if (pickUp && pickUp.resourceType == ItemsList.ResourceType.Skill && gm.itemList.savedSkills.Count > 0 &&
            gm.itemList.savedSkills.Count >= gm.itemList.skillsData.skills.Count)
        {
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                print(il.interactables.IndexOf(this));
                GLNetworkWrapper.instance.DestroyInteractable(ItemsList.instance.interactables.IndexOf(this));
                gameObject.SetActive(false);
            }
            else
                Destroy(gameObject);   
        }
        */

        if (mapMarker)
            mapMarker.transform.parent = null;
        if (mapMarkerCross)
            mapMarkerCross.transform.parent = null;

        if (!door && !button && !transport && !npc && canvasParent)
        {
            canvasParent.parent = il.itemCanvasesParent;
        }

        if (rb)
            rb.isKinematic = true;
        
        if (ammoPickUp) ammoPickUp.interactable = this;

        if (npc) npc.interactable = this;
        
        StartCoroutine(CheckHint());
    }

	private void OnEnable()
	{
		this.EnableUpdates();
        
        if (mapMarker)
            mapMarker.SetActive(true);
        /*
        if (mapMarkerCross)
            mapMarkerCross.SetActive(true);
            */
	}

	private void OnDisable()
	{
        if (mapMarker)
            mapMarker.SetActive(false);
        if (mapMarkerCross)
            mapMarkerCross.SetActive(false);
        
		this.DisableUpdates();
	}

	void IUpdatable.OnUpdate()
	{
		if (gm == null)
			gm = GameManager.instance;
		if (ui == null)
			ui = UiManager.instance;
		if (il == null)
			il = ItemsList.instance;

		if (mapMarker)
		{
			mapMarker.transform.rotation = Quaternion.identity;
			mapMarker.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		}
		if (mapMarkerCross)
		{
			mapMarkerCross.transform.rotation = Quaternion.identity;
			mapMarkerCross.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		}

		if (hint.activeInHierarchy && gm && gm.player)
			hint.transform.LookAt(gm.player.pm.cameraAnimator.transform.position);

		if (showName && nameGui != null)
		{
			nameGui.transform.parent.transform.LookAt(gm.player.pm.cameraAnimator.transform.position);
			if (!door && !button && !transport && !npc)
			{
				if (canvasParent)
					canvasParent.position = transform.position;
			}
		}
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && LevelGenerator.instance.levelgenOnHost == false)
            return;

		if (!gm.hub && rb && rb.useGravity && transform.position.y < -0.5f)
		{
			rb.MovePosition(new Vector3(transform.position.x, 0.5f, transform.position.z));
		}
	}

    public void ChangeMarkerToCross()
    {
        if (mapMarker) mapMarker.SetActive(false);
        if (mapMarkerCross) mapMarkerCross.SetActive(true);
    }

    IEnumerator CheckHint()
    {
        int tick = 0;
        while (true)
        {
            if (gm.player == null)    
                break;

            var target = PlayerMovement.instance.hc;

            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                target = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
            
            float newDist = Vector3.Distance(transform.position, target.transform.position);
            if (!gm)
            {
                gm = GameManager.instance;
            }

            float newHintdistance = hintDistance;
            if (psc.hallucinations) newHintdistance *= 4;
            if (gm.lg == null || !gm.lg.generating)
            {
                if (newDist <= newHintdistance)
                {
                    // meat hole stuff
                    if (canCreateMeatHole && activeMeatHole == null && !gm.hub && ((GLNetworkWrapper.instance == null || GLNetworkWrapper.instance.coopIsActive == false) ||
                        (LevelGenerator.instance.levelgenOnHost)))
                    {
                        // this fires once in 3 seconds
                        if (tick < 3)
                        {
                            tick++;
                        }
                        else
                        {
                            tick = 0;
                            
                            // check if this object is outside the level
                            ElevatorController elevator = ElevatorController.instance;
                            float elevDist = Vector3.Distance(transform.position, elevator.transform.position);
                            float spawnerDist = Vector3.Distance(transform.position, gm.lg.playerSpawner.position);
                            
                            if (elevator && (weaponPickUp) && (elevDist <= 6 || spawnerDist <= 5))
                            {
                                SendToMeatHole(false);
                            }
                            else if (elevator && (pickUp && pickUp.resourceType == ItemsList.ResourceType.Key) && (elevDist <= 15 || spawnerDist <= 15))
                            {
                                SendToMeatHole(false);
                            }
                            else if (transform.position.y < -100)
                            {
                                SendToMeatHole(false);
                            }
                            else if (transform.position.y > 8)
                            {
                                SendToMeatHole(false);
                            }
                            else if (Physics.Raycast(transform.position + Vector3.up * 100, Vector3.down, 1000, floorLayerMask))
                            {
                                // this object is inside the level
                                //print("im inside the level");
                            }
                            else
                            {
                                // this object is outside the level
                                SendToMeatHole(true);
                                //print(itemName + " is outside the level, spawning a meat hole");
                            }
                        }
                    }
                    
                    /*
                    if (rb && rb.isKinematic)
                        rb.isKinematic = false;
                        */

                    if (activeMeatHole == null)
                    {
                        if (portable)
                        {
                            if (!portable.inHands)
                            {
                                hint.transform.LookAt(gm.player.pm.cameraAnimator.transform.position);
                                nameGui.transform.parent.transform.LookAt(gm.player.pm.cameraAnimator.transform.position);
                                hint.SetActive(true);
                                hint.transform.GetChild(0).gameObject.layer = 9;
                                nameGui.gameObject.layer = 9;
                            }
                            else
                            {
                                hint.SetActive(true);
                                hint.transform.GetChild(0).gameObject.layer = 13;
                                nameGui.gameObject.layer = 13;
                                rb.constraints = RigidbodyConstraints.None;
                                /*
                                rb.useGravity = true;
                                rb.isKinematic = false;
                                */
                            }
                        }
                        else if (door)
                        {
                            hint.transform.LookAt(gm.player.pm.cameraAnimator.transform.position);
                            nameGui.transform.parent.transform.LookAt(gm.player.pm.cameraAnimator.transform.position);
                            nameGuiAnim.gameObject.SetActive(true);
                            hint.SetActive(true);
                        }
                        else if (nameGui != null)
                        {
                            hint.transform.LookAt(gm.player.pm.cameraAnimator.transform.position);
                            nameGui.transform.parent.transform.LookAt(gm.player.pm.cameraAnimator.transform.position);
                            hint.SetActive(true);
                            hint.transform.GetChild(0).gameObject.layer = 9;
                            nameGui.gameObject.layer = 9;
                        }
                    }
                    else
                    {
                        hint.SetActive(false);
                        if (rb && !rb.isKinematic && rb.velocity.magnitude < 0.5f)
                            rb.isKinematic = true;
                    }
                }
                else
                {
                    hint.SetActive(false);
                    if (rb && !rb.isKinematic && rb.velocity.magnitude < 0.5f)
                        rb.isKinematic = true;
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void SendToMeatHole(bool putOnTile)
    {
        // called on host or solo

        print("Put On Tile " + putOnTile);
        if (meatBrain || ( putOnTile && pickUp && pickUp.resourceType == ItemsList.ResourceType.Key))
        {
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive && LevelGenerator.instance.levelgenOnHost == false)
                return;

            transform.position = LevelGenerator.instance.GetClosestTile(transform.position).transform.position +
                                 Vector3.up * 0.2f;
        }
        else
        {
            activeMeatHole = SpawnController.instance.GetMeatHole(transform.position);

            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                GLNetworkWrapper.instance.SendToMeatHole(gameObject, activeMeatHole.gameObject);
            }
            else
            {
                SendToMeatHoleOnClient(activeMeatHole.gameObject);
            }   
        }
    }

    public void SendToMeatHoleOnClient(GameObject meatHole)
    {
        activeMeatHole = meatHole.GetComponent<HealthController>();
        
        if (activeMeatHole && activeMeatHole.npcInteractor && activeMeatHole.npcInteractor.interactable)
            activeMeatHole.npcInteractor.interactable.itemInsideMeatHole = this;
                                
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;

        transform.position += Vector3.down * 30;
        if (visual)
            visual.SetActive(false);
    }

    // when meat hole dies
    public void SpawnByMeatHole()
    {
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = false;
        activeMeatHole = null;
        if (visual)
            visual.SetActive(true);
    }

    public void PickWeapon()
    {
        PlayerAudioController.instance.ItemSound(weaponPickUp.pickUpClip);
        //UiManager.instance.GotWeapon(this);
        gm.itemList.PickWeapon(weaponPickUp);
        UiManager.instance.GotItem(this);
        //nameGuiAnim.speed = 0;
        showName = false;
        //nameGuiAnim.transform.parent = null;
        nameGuiAnim.SetBool("Active", false);
        ic.selectedItem = null;
        if (mapMarker)
            mapMarker.SetActive(false);
        if (mapMarkerCross)
            mapMarkerCross.SetActive(false);
        if (!ui.playerInteracted)
        {
            ui.playerInteracted = true;
        }
        //gameObject.SetActive(false);

        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            //print(il.interactables.IndexOf(this));
            GLNetworkWrapper.instance.DestroyInteractable(ItemsList.instance.interactables.IndexOf(this));
            gameObject.SetActive(false);
        }
        else
        {
            gm.itemList.interactables.Remove(this);
            Destroy(gameObject);
        }
    }

    public void InteractThroughMeathole()
    {
        activeMeatHole.Damage(1000, activeMeatHole.transform.position, activeMeatHole.transform.position,
        null, null,false,null, null, null, true);
        activeMeatHole = null;
        if (weaponPickUp && weaponPickUp.npc)
        {
            weaponPickUp.weaponDataRandomier.npc = false;
            weaponPickUp.weaponDataRandomier.UpdateDescription();
        }
        Interact(false);
    }
    
    public void Interact(bool toolForceKnown)
    {
        if (activeMeatHole != null || !canBeInteracted) return;
        
        gm = GameManager.instance;
        ic = InteractionController.instance;
        ui = UiManager.instance;
        il = ItemsList.instance;
        
        if (ammoPickUp)
        {
            canBeInteracted = false;
            StopCoroutine(CheckHint());
            if (PlayerSkillsController.instance.activeCult == PlayerSkillsController.Cult.goldCult)
            {
                il.GetGold(1);
            }
            PlayerAudioController.instance.ItemSound(ammoPickUp.pickUpClip);
            gm.itemList.PickAmmo(ammoPickUp);
            UiManager.instance.GotItem(this);
            
            ic.selectedItem = null;
            //nameGuiAnim.speed = 0;
            showName = false;
            //nameGuiAnim.transform.parent = null;
            nameGuiAnim.SetBool("Active", false);
            if (mapMarker)
                mapMarker.SetActive(false);
            if (mapMarkerCross)
                mapMarkerCross.SetActive(false);
            if (!ui.playerInteracted)
            {
                ui.playerInteracted = true;
            }
            
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                print(il.interactables.IndexOf(this));
                GLNetworkWrapper.instance.DestroyInteractable(ItemsList.instance.interactables.IndexOf(this));
                gameObject.SetActive(false);
            }
            else
            {
                gm.itemList.interactables.Remove(this);
                Destroy(gameObject);
            }

        }
        else if (pickUp)
        {
            canBeInteracted = false;
            StopCoroutine(CheckHint());
            if (toolForceKnown && pickUp.tool)
            {
                pickUp.tool.forceKnown = true;
            }
            if (PlayerSkillsController.instance.activeCult == PlayerSkillsController.Cult.goldCult)
            {
                il.GetGold(1);
            }
            PlayerAudioController.instance.ItemSound(pickUp.pickUpClip);
            gm.itemList.PickResource(pickUp);
            UiManager.instance.GotItem(this);
            //nameGuiAnim.speed = 0;
            showName = false;
            //nameGuiAnim.transform.parent = null;
            ic.selectedItem = null;
            nameGuiAnim.SetBool("Active", false);
            if (mapMarker)
                mapMarker.SetActive(false);
            if (mapMarkerCross)
                mapMarkerCross.SetActive(false);
            if (!ui.playerInteracted)
            {
                ui.playerInteracted = true;
            }

            if (pickUp.questItem)
            {   
                StartCoroutine(pickUp.questItem.PickUp());
            }
            else
            {
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    //print(il.interactables.IndexOf(this));
                    GLNetworkWrapper.instance.DestroyInteractable(ItemsList.instance.interactables.IndexOf(this));
                    gameObject.SetActive(false);
                }
                else
                {
                    gm.itemList.interactables.Remove(this);
                    gameObject.SetActive(false);
                    //Destroy(gameObject);
                }
            }
        }
        else if (weaponPickUp)
        {
            if (!weaponPickUp.npc || !weaponPickUp.npc.gameObject.activeInHierarchy || (weaponPickUp.npc && weaponPickUp.weaponDataRandomier.npcPaid))
            {
                canBeInteracted = false;
                StopCoroutine(CheckHint());
                PickWeapon();
            }
            else
            {
                weaponPickUp.npc.Interact();
            }
        }
        else if (lockObject)
        {
            if (!lockObject.HasItem())
            {
                StopCoroutine(ShowPhrase());
                StartCoroutine(ShowPhrase());
                if (!ui.playerInteracted)
                {
                    ui.playerInteracted = true;
                }
            }
            else
            {
                canBeInteracted = false;
                StopCoroutine(CheckHint());
                PlayerAudioController.instance.ItemSound(lockObject.useItemSound);
                lockObject.UseItem(this);
                
                if (nameGuiAnim != null)
                    Destroy(nameGuiAnim.gameObject);
                
                /*
                nameGuiAnim.gameObject.SetActive(false);
                nameGuiAnim.speed = 0;
                nameGuiAnim.transform.parent = null;
                */
                
                if (!ui.playerInteracted)
                {
                    ui.playerInteracted = true;
                }
                
                /*
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                {
                    GLNetworkWrapper.instance.DestroyInteractable(ItemsList.instance.interactables.IndexOf(this));
                    gameObject.SetActive(false);
                }
                else
                {
                    
                    gm.itemList.interactables.Remove(this);
                    Destroy(gameObject);
                }*/
            }
        }
        else if (portable)
        {
            if (!ui.playerInteracted)
            {
                ui.playerInteracted = true;
            }

            if (!portable.inHands)
            {
                portable.PickUp();
                if (portable.giveRandomStatus || portable.giveMementoOnPickUp.Count > 0 || (portable.noteTextGenerator && portable.noteTextGenerator.type == NoteTextGenerator.NoteType.CassetteTape))
                    UiManager.instance.GotItem(this);
            }
            else
                portable.Drop();
        }
        else if (transport)
        {
            if (!ui.playerInteracted)
            {
                ui.playerInteracted = true;
            }

            if (!transport.playerInside)
            {
                transport.EnterTransport();
                ui.EnterBike();   
            }
            else
            {
                /*
                transport.ExitTransport();
                ui.ExitBike();
                */
            }
        }
        else if (door)
        {
            if (!psc.nervousHunk)
            {
                if (!door.open)
                {
                    if (!ui.playerInteracted)
                    {
                        ui.playerInteracted = true;
                    }

                    door.OpenDoor(gm.player.transform.position);
                }
                else // close door if open
                {
                    if (!ui.playerInteracted)
                    {
                        ui.playerInteracted = true;
                    }
                    door.CloseDoor();
                }   
            }
            else
            {
                door.DoorDestroyed();
                if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                    GLNetworkWrapper.instance.CreateExplosion(door.transform.position + Vector3.up * 2);
                else
                {
                    var newGrenade = Instantiate(ItemsList.instance.savedTools[0].toolController.grenadeExplosion, door.transform.position + Vector3.up * 2, Quaternion.identity);
                    newGrenade.DestroyEffect(true);
                }
            }
        }
        else if (button)
        {
            
            if (!ui.playerInteracted)
            {
                ui.playerInteracted = true;
            }
            button.Press();
        }
        else if (npc && npc.gameObject.activeInHierarchy)
        {
            if (!ui.playerInteracted)
                ui.playerInteracted = true;

            // SPEAK
            npc.Interact();
        }
        else if (setRandomTrackOnStart)
        {
            // change snouts music here
            setRandomTrackOnStart.RandomTrackOnStart();
        }
        
        if (note) note.Interaction();
    }

    IEnumerator ShowPhrase()
    {
        PlayerAudioController.instance.ItemSound(lockObject.phraseSound);
        if (gm.language == 0)
            nameGui.text = lockObject.lockedPhrase;
        else if (gm.language == 1)
            nameGui.text = lockObject.lockedPhraseRu;
        else if (gm.language == 2)
            nameGui.text = lockObject.lockedPhraseEsp;
        else if (gm.language == 3)
            nameGui.text = lockObject.lockedPhraseGer;
        else if (gm.language == 4)
            nameGui.text = lockObject.lockedPhraseIT;
        else if (gm.language == 5)
            nameGui.text = lockObject.lockedPhraseSPBR;

        yield return new WaitForSeconds(3);
        nameGui.text = itemNames[gm.language];
    }

    public void UpdateNameGui()
    {    
        if (nameGuiAnim != null)
        {
            nameGuiAnim.gameObject.SetActive(true);
            showName = true;
            nameGuiAnim.SetBool("Active", true);

            if (hideHuiCoroutine != null) StopCoroutine(hideHuiCoroutine);

            if (gameObject.activeInHierarchy)
                hideHuiCoroutine = StartCoroutine(HideGuiOverTime());   
        }
    }

    IEnumerator HideGuiOverTime()
    {
        yield return new WaitForSeconds(0.1f);
        if (nameGuiAnim != null)
            nameGuiAnim.SetBool("Active", false);
        showName = false;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        il = ItemsList.instance;
        if (il && il.interactables.Contains(this))
        {
            il.interactables.Remove(this);   
        }
        if (mapMarker)
            Destroy(mapMarker);
        if (mapMarkerCross)
            Destroy(mapMarkerCross);
        
        if (canvasParent)
            Destroy(canvasParent.gameObject);
    }
}
