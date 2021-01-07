using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.AI;

public class DoorController : MonoBehaviour
{
    enum Position {Inside, Outside}
    Position playerPosition = Position.Inside;

    public Collider mainCollider;
    public Animator masterAnim;
    public bool locked = false;
    public bool open = false;

    public Transform insideTransform;
    public Transform outsideTransform;

    public AudioClip openDoor;
    public AudioClip openningDoor;
    public AudioClip closeDoor;
    public AudioSource audioSourceBreak;
    public AudioSource audioSourceShort;
    public AudioSource audioSourceLong;
    
    public Interactable padlock;

    public NavMeshObstacle obstacle;

    GameManager gm;
    PlayerMovement pm;

    Coroutine openDoorCoroutine;
    Coroutine closeDoorCoroutine;
    bool canBeInteracted = true;

    public MobPartsController mobParts;
    public MobBodyPart lockerDoorPart;

    public Interactable interactableController;

    void Awake()
    {
        if (padlock)
            padlock.gameObject.SetActive(false);
    }
    
    private void Start()
    {
        gm = GameManager.instance;
        pm = PlayerMovement.instance;
    }

    public void OpenDoor(Vector3 openerPosition)
    {
        if (!locked && !open && (!padlock || !padlock.isActiveAndEnabled))
        {
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                GLNetworkWrapper.instance.OpenDoor(ItemsList.instance.interactables.IndexOf(interactableController), openerPosition);
            }
            else
            {
                OpenDoorOnClient(openerPosition);
            }
        }
    }

    public void OpenDoorOnClient(Vector3 openerPosition)
    {
        if (Vector3.Distance(insideTransform.position, openerPosition) < Vector3.Distance(outsideTransform.position, openerPosition))
            playerPosition = Position.Inside;
        else
            playerPosition = Position.Outside;

        if (openDoorCoroutine != null)
            StopCoroutine(openDoorCoroutine);
        if (gameObject.activeInHierarchy)
            openDoorCoroutine = StartCoroutine(OpenDoor());

        if (obstacle)
            obstacle.carving = true;
    }

    public void CreateLockOnDoor()
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.CreateLockOnClient(ItemsList.instance.interactables.IndexOf(interactableController));
        }
        else
        {
            CreateLockOnClient();
        }
    }

    public void CreateLockOnClient()
    {
        locked = true;
        padlock.gameObject.SetActive(true);
    }

    // not used in coop rn
    public void Unlock()
    {
        locked = false;
    }

    public void CloseDoor()
    {
        if (!locked && canBeInteracted && open)
        {
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
            {
                GLNetworkWrapper.instance.CloseDoor(ItemsList.instance.interactables.IndexOf(interactableController));
            }
            else
            {
                CloseDoorOnClient();
            }
        }
    }

    public void CloseDoorOnClient()
    {
        if (openDoorCoroutine != null)
            StopCoroutine(openDoorCoroutine);

        if (closeDoorCoroutine != null)
            StopCoroutine(closeDoorCoroutine);

        masterAnim.SetBool("Open", false);

        audioSourceShort.clip = closeDoor;
        audioSourceShort.pitch = Random.Range(0.75f, 1.25f);
        audioSourceShort.Play();
        open = false;

        if (obstacle)
            obstacle.carving = false;

        if (gameObject.activeInHierarchy)
            StartCoroutine(CanBeInteracted());
    }

    IEnumerator CanBeInteracted()
    {
        canBeInteracted = false;
        yield return new WaitForSeconds(1f);
        canBeInteracted = true;
    }

    IEnumerator OpenDoor()
    {
        if (canBeInteracted)
        {
            open = true;
            if (openDoor)
            {
                audioSourceShort.clip = openDoor;
                audioSourceShort.pitch = Random.Range(0.75f, 1.25f);
                audioSourceShort.Play();   
            }
            StartCoroutine(CanBeInteracted());

            switch (playerPosition)
            {
                case Position.Inside:
                    masterAnim.SetBool("Outside", true);
                    masterAnim.SetBool("Inside", false);
                    masterAnim.SetBool("Open", true);
                    break;

                case Position.Outside:
                    masterAnim.SetBool("Outside", false);
                    masterAnim.SetBool("Inside", true);
                    masterAnim.SetBool("Open", true);
                    break;
            }

            yield return new WaitForSeconds(0.5f);
            
            audioSourceLong.pitch = Random.Range(0.75f, 1.25f);
            audioSourceLong.Play();

            yield return new WaitForSeconds(1.5f);
            //mainCollider.isTrigger = true;

            if (closeDoorCoroutine != null)
                StopCoroutine(closeDoorCoroutine);
            closeDoorCoroutine = StartCoroutine(CloseDoorAuto());
        }
    }

    IEnumerator CloseDoorAuto()
    {
        while (open)
        {
            yield return new WaitForSeconds(Random.Range(1, 90));
            //mainCollider.isTrigger = false;
            if (!pm)
                pm = PlayerMovement.instance;

            if (Vector3.Distance(transform.position, pm.transform.position) > 5)
            {
                CloseDoor();
            }
        }
    }

    public void Break(MobBodyPart part)
    {
        int partIndex = -1;
        if (part != null && mobParts != null)
            partIndex = mobParts.bodyParts.IndexOf(part);
        
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.BreakDoorPartOnClient(ItemsList.instance.interactables.IndexOf(interactableController), partIndex);
        }
        else
        {
            BreakDoorPartOnClient(partIndex);
        }
    }

    public void BreakDoorPartOnClient(int index)
    {
        if (index == -1)
        {
            DestroyDoorOnClient();
            return;    
        }
        
        var part = mobParts.bodyParts[index];

        if (part == lockerDoorPart && locked)
        {
            padlock.gameObject.SetActive(false);
        }
        
        if (part.deathParticle)
        {
            part.deathParticle.transform.parent = null;
            part.deathParticle.transform.localScale = Vector3.one;
            part.deathParticle.Play();   
        }
        part.gameObject.SetActive(false);

        mobParts.bodyParts.Remove(part);
        
        SpawnController.instance.MobHearNoise(transform.position, 50);
        
        if (mobParts.bodyParts.Count < mobParts.bodyPartMax / 2)
            DoorDestroyed();
        else
        {
            if (audioSourceBreak)
            {
                audioSourceBreak.pitch = Random.Range(1f, 1.5f);
                audioSourceBreak.Play();   
            }    
        }
    }

    public void DoorDestroyed()
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.BreakDoorPartOnClient(GameManager.instance.units.IndexOf(mobParts.hc), -1);
        }
        else
        {
            DestroyDoorOnClient();
        }
    }

    public void DestroyDoorOnClient()
    {
        if (mobParts != null)
        {
            locked = false;
            if (padlock) Destroy(padlock);
        
            SpawnController.instance.MobHearNoise(transform.position, 100);
            
            if (mobParts)
            {
                foreach(MobBodyPart part in mobParts.bodyParts)
                {
                    if(part.gameObject.activeInHierarchy)
                    {
                        part.deathParticle.transform.parent = null;
                        part.deathParticle.transform.localScale = Vector3.one;
                        part.deathParticle.Play();
                        part.gameObject.SetActive(false);
                    }
                }   
            }

            if (audioSourceBreak)
            {
                audioSourceBreak.transform.parent = null;
                audioSourceBreak.pitch = Random.Range(0.5f, 1.1f);
                audioSourceBreak.Play();   
            }
        
            gameObject.SetActive(false);   
        }
        else
        {
            locked = false;
            if (padlock) Destroy(padlock);
            
            gameObject.SetActive(false);
        }
    }
}