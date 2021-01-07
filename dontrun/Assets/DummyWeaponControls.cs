using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyWeaponControls : MonoBehaviour
{
    public List<Animator> tools;
    public List<Animator> weaponsAnims;

    private Animator activeWeapon;
    private Animator activeTool;
    
    private int weaponIndex = -1;
    private int toolIndex = -1;
    private bool noseSkill = false;

    public Animator meatBroAnim;
    public GameObject meatNoseAnim;



    void Start()
    {
        PlayerNetworkObject connectedPNO = null;
        for (int i = 0; i < GLNetworkWrapper.instance.playerNetworkObjects.Count; i++)
        {
            var dummy = GLNetworkWrapper.instance.playerNetworkObjects[i].connectedDummy;
            if (dummy && dummy.dwc && dummy.dwc == this)
            {
                connectedPNO = GLNetworkWrapper.instance.playerNetworkObjects[i];
                SetWeaponAndTool(connectedPNO.weaponInArmsIndex, connectedPNO.toolInArmsIndex, connectedPNO.noseSkill);
                break;
            }
        }
    }

    public void SetWeaponAndTool(int _weaponIndex,  int _toolIndex, bool _noseSkill)
    {
        print(_weaponIndex + " ; " + _toolIndex);
        
        if (weaponIndex != _weaponIndex)
        {
            weaponIndex = _weaponIndex;
            
            HideAll(weaponsAnims);
            
            if (weaponIndex != -1)
            {
                weaponsAnims[_weaponIndex].gameObject.SetActive(true);
            }
        }

        if (toolIndex != _toolIndex)
        {
            toolIndex = _toolIndex;
            
            HideAll(tools);
            
            if (toolIndex != -1)
            {
                tools[_toolIndex].gameObject.SetActive(true);
            }   
        }

        if (noseSkill != _noseSkill)
        {
            noseSkill = _noseSkill;
            meatNoseAnim.SetActive(noseSkill);
        }
    }

    void HideAll(List<Animator> animators)
    {
        for (int i = animators.Count - 1; i >= 0; i--)
        {
            animators[i].gameObject.SetActive(false);
        }
    }

    public void Attack(bool _self)
    {
        if (_self)
        {
            if (weaponIndex == 0 || weaponIndex == 5 || weaponIndex == 6 || weaponIndex == 7 || weaponIndex == 8
                || weaponIndex == 9 || weaponIndex == 10 || weaponIndex == 11 || weaponIndex == 13)
            {
                meatBroAnim.SetTrigger("AttackMeleeSelf");
            }
            else
            {
                meatBroAnim.SetTrigger("AttackRangeSelf");
            }
        }
        else
        {
            if (weaponIndex == 0)
            {
                //vertical slash
                meatBroAnim.SetTrigger("AttackMeleeVertical");
            }
            else if (weaponIndex == 5 || weaponIndex == 6 || weaponIndex == 8 || weaponIndex == 9 || weaponIndex == 13)
            {
                //horizontal slash
                meatBroAnim.SetTrigger("AttackMeleeHorizontal");
            }
            else if (weaponIndex == 7 || weaponIndex == 11)
            {
                // direct stab
                meatBroAnim.SetTrigger("AttackMeleeStab");
            }
            else if (weaponIndex == 1 || weaponIndex == 2 || weaponIndex == 3 || weaponIndex == 4 || weaponIndex == 12)
            {
                meatBroAnim.SetTrigger("AttackRange");
            }
            else if (weaponIndex == 10)
            {
                meatBroAnim.SetTrigger("AttackShield");
            }
        }
    }

    public void UseTool(bool _throw)
    {
        if (!_throw)
            meatBroAnim.SetTrigger("EatTool");
        else
            meatBroAnim.SetTrigger("ThrowTool");
    }

    public void EatWeapon()
    {
        meatBroAnim.SetTrigger("EatWeapon");
    }
}
