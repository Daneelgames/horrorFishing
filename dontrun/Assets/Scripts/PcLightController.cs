using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PcLightController : MonoBehaviour
{
    [Header("This script only changes the color")]
    public Animator anim;

    private string intName = "Color";

    private PlayerSkillsController psc;
    // Start is called before the first frame update
    
    void OnEnable()
    {
        StartCoroutine(ChangeColor());
    }

    IEnumerator ChangeColor()
    {
        psc = PlayerSkillsController.instance;
        var pf = GutProgressionManager.instance.playerFloor;
        
        while (anim.gameObject.activeInHierarchy)
        {
            for (int i = 0; i < 6; i++)
            {
                if ((psc && psc.hallucinations) || pf == 7 || pf == 8 ||pf == 9)
                    anim.SetInteger(intName, i);
                else
                    anim.SetInteger(intName, GameManager.instance.level.mainLightColor);
                
                yield return new WaitForSeconds(Random.Range(10f,60f));
            }
        }
    }
}
