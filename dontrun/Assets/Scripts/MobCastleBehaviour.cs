using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobCastleBehaviour : MonoBehaviour
{
    public List<MobMeleeAttack> meleeAttacks;
    private bool attacking = false;
    public void StartAttack(MobMeleeAttack meleeAttack)
    {
        if (!attacking)
        {
            attacking = true;
            foreach (var m in meleeAttacks)
            {
                if (m != meleeAttack)
                {
                    m.damageArea.gameObject.SetActive(false);
                }
            }   
        }
    }

    public void StopAttack(MobMeleeAttack meleeAttack)
    {
        attacking = false;
        foreach (var m in meleeAttacks)
        {
            if (m != meleeAttack)
                m.damageArea.gameObject.SetActive(true);
            else 
                m.damageArea.gameObject.SetActive(false);
        }   
    }
}
