using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class NoseWithTeethController : MonoBehaviour
{
    private PlayerMovement playerMovement;
    public Animator anim;
    public int damage = 30;
    private bool dangerous = true;
    public float attackCooldown = 1;    
    List<MobBodyPart> damagedInOnHit = new List<MobBodyPart>();
    private bool attacking = false;
    private GameManager gm;
    
    public List<string> damageString = new List<string>();
    private PlayerSkillsController psc;
    
    void Start()
    {
        gm = GameManager.instance;
        psc = PlayerSkillsController.instance;
        playerMovement = PlayerMovement.instance;
    }

    private void OnTriggerStay(Collider coll)
    {
        if ((coll.gameObject.layer == 11 || coll.gameObject.layer == 10 ) && coll.gameObject != playerMovement.gameObject)
        {
            if (dangerous && !attacking)
            {
                damagedInOnHit.Clear();
                StartCoroutine(AttackProgress());
            }
            else if (dangerous && attacking)
            {
                bool alreadyHit = false;
                for (var index = 0; index < damagedInOnHit.Count; index++)
                {
                    MobBodyPart part = damagedInOnHit[index];
                    if (coll && part && part.gameObject == coll.gameObject)
                        alreadyHit = true;
                }

                if (!alreadyHit)
                {
                    var part = coll.gameObject.GetComponent<MobBodyPart>();
                    if (part != null && part.hc != null)
                    {
                        string s = damageString[gm.language] + part.hc.damagedByPlayerMessage[gm.language];
                        
                        if (psc.activeCult == PlayerSkillsController.Cult.bleedingCult)
                            playerMovement.hc.Heal(damage / 10);
                        
                        part.hc.Damage(damage, transform.position, transform.position, part, s, 
                            true, null,playerMovement.hc, null, false);   
                        
                        if (part.hc.health <= 0)
                        {
                            gm.SaveNewKill();
                            gm.itemList.AddToBadReputation(part.hc.addBadRepOnDeath);
                            if (!gm.demo && part.hc.mobKilledAchievementID.Length > 0)
                            {
                                SteamAchievements.instance.UnlockSteamAchievement(part.hc.mobKilledAchievementID);
                            }
                        }
                        else if (psc.fireAttack && Random.value > 0.75f)
                            part.hc.StartFire();
                    }
                    damagedInOnHit.Add(part);
                }
            }            
        }
    }

    IEnumerator AttackProgress()
    {
        attacking = true;
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown / 2);
        attacking = false;
        dangerous = false;
        yield return new WaitForSeconds(attackCooldown / 2);
        dangerous = true;
    }
}
