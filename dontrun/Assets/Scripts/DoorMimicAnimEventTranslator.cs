using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMimicAnimEventTranslator : MonoBehaviour
{
    public TrapController trap;
    
    public void SetDanger()
    {
        trap.SetDanger();
    }
}
