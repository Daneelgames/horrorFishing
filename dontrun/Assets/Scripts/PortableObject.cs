using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class PortableObject : MonoBehaviour
{
    public enum QuestPortable
    {
        Null, GunnWood
    }

    public QuestPortable portableQuestType = QuestPortable.Null;

    public int noteIndex = -1;
    public bool inHands = false;
    public AudioSource au;
    public AudioClip pickSound;

    public Rigidbody rb;
    public Collider physicsCollider;

    InteractionController ic;
    GameManager gm;
    PlayerMovement pm;

    private bool wasPickedUp = false;

    Coroutine moveInHands;
    private Coroutine canPickUpCoroutine;
    bool canPickUp = true;
    float cooldownPickup = 2;

    public bool giveRandomStatus = false;
    public List<int> giveMementoOnPickUp = new List<int>();

    public NoteTextGenerator noteTextGenerator;
    public MeatBrainController meatBrainController;

    private bool savingCassette = false;

    private bool followPortableTransform = false;

    private void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
        ic = InteractionController.instance;
    }

    public void PickUp()
    {
        if (!savingCassette && canPickUp)
        {
            au.pitch = Random.Range(0.75f, 1.25f);
            au.clip = pickSound;
            au.Play();
            rb.isKinematic = true;
            rb.useGravity = false;
            physicsCollider.enabled = false;
            if (pm.hc.wc.activeWeapon)
                pm.hc.wc.activeWeapon.canAct = false;
            inHands = true;
            ic.objectInHands = this;
            SetLayerRecursively(gameObject, 9);
            //transform.parent = pm.portableTransform;
            moveInHands = StartCoroutine(MoveInHands());
            if (meatBrainController) meatBrainController.PickUp();

            if (giveMementoOnPickUp.Count > 0)
            {
                for (int i = 0; i < giveMementoOnPickUp.Count; i++)
                {
                    var skill = gm.itemList.skillsData.skills[giveMementoOnPickUp[i]];
                    PlayerSkillsController.instance.AddSkill(skill);
                }
            }

            if (giveRandomStatus)
            {
                gm.GiveRandomStatus();
            }

            if (noteTextGenerator && noteTextGenerator.type == NoteTextGenerator.NoteType.CassetteTape && !wasPickedUp)
            {
                ItemsList.instance.UnlockNewCassette(noteTextGenerator);
                gm.tapesFoundOfFloors.Add(GutProgressionManager.instance.playerFloor);
            }
            
            wasPickedUp = true;

            if (noteIndex >= 0)
            {
                HubItemsSpawner.instance.NewNotePickedUp(noteIndex);
            }
        }
    }

    public void UseAsQuestPortable()
    {
        if (WeaponControls.instance.activeWeapon)
            WeaponControls.instance.activeWeapon.canAct = true;
        
        followPortableTransform = false;
        au.Play();
        transform.parent = null;
        canPickUp = false;
        InteractionController.instance.objectInHands = null;
    }

    IEnumerator MoveInHands()
    {
        float t = 0;
        while (t < 1f && inHands)
        {
            transform.position = Vector3.Lerp(transform.position, pm.portableTransform.position, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, pm.portableTransform.rotation, t);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        followPortableTransform = true;
        //transform.localPosition = Vector3.zero;
        
        if (noteTextGenerator && noteTextGenerator.spellScroll)
            noteTextGenerator.spellScroll.ToggleSpellInHands(true);
    }

    public void Throw()
    {
        if (savingCassette) return;
        
        if (noteTextGenerator && noteTextGenerator.type == NoteTextGenerator.NoteType.CassetteTape)
        {
            SaveCassetteTape();
            return;    
        }

        
        followPortableTransform = false;
        var lastPos = transform.position;

        inHands = false;
        ic.objectInHands = null;
        //transform.parent = null;
        if (pm.hc.wc.activeWeapon)
            pm.hc.wc.activeWeapon.canAct = true;
        
        if (meatBrainController) meatBrainController.Drop();
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.isKinematic = false;

        if (moveInHands != null)
            StopCoroutine(moveInHands);

        rb.AddForce(pm.cameraAnimator.transform.forward.normalized * 30f, ForceMode.Impulse);
        rb.MovePosition(lastPos);
        SetLayerRecursively(gameObject, 13);
        if (canPickUpCoroutine != null)
            StopCoroutine(canPickUpCoroutine);
        canPickUpCoroutine = StartCoroutine(CanPickUp());
        
        if (noteTextGenerator && noteTextGenerator.spellScroll)
            noteTextGenerator.spellScroll.ToggleSpellInHands(false);
    }

    public void Drop()
    {
        if (savingCassette) return;

        if (noteTextGenerator && noteTextGenerator.type == NoteTextGenerator.NoteType.CassetteTape)
        {
            SaveCassetteTape();
            return;    
        }

        var lastPos = transform.position;

        inHands = false;
        ic.objectInHands = null;
        followPortableTransform = false;
        //transform.parent = null;
        if (meatBrainController) meatBrainController.Drop();
        rb.useGravity = true;
        rb.isKinematic = false;

        if (pm.hc.wc.activeWeapon)
            pm.hc.wc.activeWeapon.canAct = true;

        if (moveInHands != null)
            StopCoroutine(moveInHands);

        rb.MovePosition(lastPos);

        rb.AddExplosionForce(10, transform.position + new Vector3(Random.Range(-1,1), Random.Range(-1, 1), Random.Range(-1, 1)), 10);
        SetLayerRecursively(gameObject, 13);
        if (canPickUpCoroutine != null)
            StopCoroutine(canPickUpCoroutine);
        canPickUpCoroutine = StartCoroutine(CanPickUp());
        
        if (noteTextGenerator && noteTextGenerator.spellScroll)
            noteTextGenerator.spellScroll.ToggleSpellInHands(false);
    }

    void Update()
    {
        if (followPortableTransform)
        {
            transform.position = pm.portableTransform.position;
            transform.rotation = pm.portableTransform.rotation;
        }
    }

    void SaveCassetteTape()
    {
        savingCassette = true;
        ic.objectInHands = null;
        transform.parent = null;
        if (pm.hc.wc.activeWeapon)
            pm.hc.wc.activeWeapon.canAct = true;
        
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = false;
        rb.isKinematic = true;
        
        if (moveInHands != null)
            StopCoroutine(moveInHands);
        if (canPickUpCoroutine != null)
            StopCoroutine(canPickUpCoroutine);

        StartCoroutine(MoveToPlayerInventory());
    }

    IEnumerator MoveToPlayerInventory()
    {
        float t = 0;
        float tTarget = 1;
        var startPos = transform.position;
        
        while (t < tTarget)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, pm.transform.position, t / tTarget);
            yield return null;
        }
        Destroy(gameObject);
    }

    IEnumerator CanPickUp()
    {
        physicsCollider.enabled = true;
        yield return new WaitForSeconds(cooldownPickup);
        canPickUp = true;
        canPickUpCoroutine = null;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child != null && child.gameObject.layer != 19) // if not map marker
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}