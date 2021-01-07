using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBarrel : MonoBehaviour
{
    public WeaponPickUp.Weapon type = WeaponPickUp.Weapon.Revolver;
    public Transform shotDirection;
    public ParticleSystem shotVfx;

    public List<GameObject> barrels;
}
