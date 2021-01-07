using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class WeaponConnector : MonoBehaviour
{
    public bool generated = true;
    public WeaponPickUp weaponPickUp;
    public WeaponController weaponController;

    public GameObject meatVisual;

    public WeaponBarrel singleBarrel;
    
    public List<WeaponBarrel> barrelsLvl_1;
    public List<WeaponBarrel> barrelsLvl_2;
    
    public List<WeaponBarrel> activeBarrels;

    [Header("ReadOnly")]
    public List<int> savedBarrelslvl_1;
    public List<int> savedBarrelslvl_2;
    
    public int barrelsCount = 1;
    public float clipSize = 1;

    public Animator crazyGunAnim;

    public bool crazyGun = false;
    
    public void GenerateOnSpawn()
    {
        foreach (var b in barrelsLvl_1)
        {
            b.gameObject.SetActive(false);
        }
        foreach (var b in barrelsLvl_2)
        {
            b.gameObject.SetActive(false);
        }

        if (generated)
        {
            savedBarrelslvl_1.Clear();
            savedBarrelslvl_2.Clear();   
        }
        
        int r = Random.Range(0, GutProgressionManager.instance.currentLevelDifficulty);

        if (generated)
            barrelsCount = r + 1;
        else
            r = Mathf.Clamp(barrelsCount - 1, 0, 50);

        if (barrelsCount == 1)
        {
            transform.localRotation = Quaternion.identity;
            
            meatVisual.SetActive(false);
            activeBarrels.Add(singleBarrel);
            singleBarrel.gameObject.SetActive(true);

            if (weaponPickUp)
            {
                switch (weaponPickUp.weapon)
                {
                    case WeaponPickUp.Weapon.Pistol:
                        clipSize = Random.Range(7,12);
                        break;
                    
                    case WeaponPickUp.Weapon.Revolver:
                        clipSize = Random.Range(4,9);
                        break;
                    
                    case WeaponPickUp.Weapon.Shotgun:
                        clipSize = Random.Range(1, 6);
                        break;
                    
                    case WeaponPickUp.Weapon.TommyGun:
                        clipSize = Random.Range(15, 45);
                        break;
                    
                    case WeaponPickUp.Weapon.OldPistol:
                        clipSize = 1;
                        break;
                } 
            }
            else if (weaponController)
            {
                switch (weaponController.weapon)
                {
                    case WeaponPickUp.Weapon.Pistol:
                        clipSize = Random.Range(7,12);
                        break;
                    
                    case WeaponPickUp.Weapon.Revolver:
                        clipSize = Random.Range(4,9);
                        break;
                    
                    case WeaponPickUp.Weapon.Shotgun:
                        clipSize = Random.Range(1, 6);
                        break;
                    
                    case WeaponPickUp.Weapon.TommyGun:
                        clipSize = Random.Range(15, 45);
                        break;
                    
                    case WeaponPickUp.Weapon.OldPistol:
                        clipSize = 1;
                        break;
                } 
            }
            
            savedBarrelslvl_1.Add(-1);
        }
        else
        {
            Quaternion newRot = transform.localRotation;
            newRot.eulerAngles = new Vector3(Random.Range(-20,7), Random.Range(-15,7), Random.Range(-90,90));
            transform.localRotation = newRot;

            if (generated)
            {
                crazyGun = Random.value < 0.33f;
                
                int scaler = 0;
                switch (weaponPickUp.weapon)
                {
                    case WeaponPickUp.Weapon.Pistol:
                        scaler = Random.Range(6, 12 + 1);
                        break;
                    case WeaponPickUp.Weapon.Revolver:
                        scaler = Random.Range(3, 9 + 1);
                        break;
                    case WeaponPickUp.Weapon.Shotgun:
                        scaler = Random.Range(barrelsCount, 7 + 1);
                        break;
                    case WeaponPickUp.Weapon.TommyGun:
                        scaler = Random.Range(15, 50 + 1);
                        break;
                    case WeaponPickUp.Weapon.OldPistol:
                        scaler = Random.Range(barrelsCount, 2 + 1);
                        break;
                    default:
                        scaler = 1;
                        break;
                }
            
                clipSize = Random.Range(1, r + 1) * scaler;
            }
            
            if (weaponPickUp) weaponPickUp.ammoClipMax = clipSize;

            int averallBarrels = barrelsLvl_1.Count + barrelsLvl_2.Count;
            if (r >= averallBarrels - 1) r = averallBarrels;
        
            List <WeaponBarrel> barrelsTemp = new List<WeaponBarrel>(barrelsLvl_1);

            bool secondRow = false;
        
            for (int i = 0; i <= r; i++)
            {
                int rr = Random.Range(0, barrelsTemp.Count);
                activeBarrels.Add(barrelsTemp[rr]);
                if (!secondRow)
                {
                    savedBarrelslvl_1.Add(barrelsLvl_1.IndexOf(barrelsTemp[rr]));
                }
                else
                {
                    savedBarrelslvl_2.Add(barrelsLvl_2.IndexOf(barrelsTemp[rr]));
                }
                
                barrelsTemp[rr].gameObject.SetActive(true);

                if (crazyGun)
                {
                    // random ass guns
                    Quaternion newRot2 = barrelsTemp[rr].transform.localRotation;
                    newRot2.eulerAngles = new Vector3(Random.Range(-20,7), Random.Range(-15,7), Random.Range(-90,90));
                    barrelsTemp[rr].transform.localRotation = newRot2;
                }
                
                barrelsTemp.RemoveAt(rr);
                if (barrelsTemp.Count <= 0)
                {
                    if (!secondRow)
                    {
                        secondRow = true;
                        barrelsTemp = new List<WeaponBarrel>(barrelsLvl_2);   
                    }
                    else
                    {
                        // no more barrels
                        break;
                    }
                }
            }
        }
    }

    public void PickWeapon(WeaponPickUp w)
    {
        if (weaponController)
        {
            weaponController.weaponConnector.clipSize = w.weaponConnector.clipSize;
            
            weaponController.ammoClipMax = w.weaponConnector.clipSize;
            weaponController.weaponConnector.barrelsCount = w.weaponConnector.barrelsCount;
            weaponController.weaponConnector.crazyGun = w.weaponConnector.crazyGun;
            weaponController.weaponConnector.savedBarrelslvl_1 = new List<int>(w.weaponConnector.savedBarrelslvl_1);
            weaponController.weaponConnector.savedBarrelslvl_2 = new List<int>(w.weaponConnector.savedBarrelslvl_2);
        }
        ConfigureBarrels(w.weaponConnector.savedBarrelslvl_1, w.weaponConnector.savedBarrelslvl_2, w.weaponConnector.transform.localRotation.eulerAngles, w.weaponConnector.crazyGun);
    }
    
    /*
    public void DropWeapon(WeaponController w)
    {
        ConfigureBarrels(w.weaponConnector.savedBarrelslvl_1, w.weaponConnector.savedBarrelslvl_2, w.weaponConnector.transform.localRotation.eulerAngles, w.weaponConnector.crazyGun);
        clipSize =  w.weaponConnector.clipSize;
        barrelsCount = w.weaponConnector.barrelsCount;
    }
    */

    public void DropWeapon(List<int> _savedBarrelslvl_1, List<int> _savedBarrelslvl_2, Vector3 _eulerAngles, bool _crazyGun,
        float _clipSize, int _barrelsCount)
    {
        ConfigureBarrels(_savedBarrelslvl_1, _savedBarrelslvl_2, _eulerAngles, _crazyGun);
        clipSize =  _clipSize;
        barrelsCount = _barrelsCount;
    }
    
    // ON LOAD LEVEL
    public void ConfigureBarrels(List<int> barrels_1, List<int> barrels_2, Vector3 rot, bool _crazyGun)
    {
        Quaternion newRot = transform.localRotation;
        newRot.eulerAngles = rot;
        transform.localRotation = newRot;
        
        savedBarrelslvl_1 = new List<int>(barrels_1);
        savedBarrelslvl_2 = new List<int>(barrels_2);

        if (weaponController)
            clipSize = weaponController.ammoClipMax;
        else if (weaponPickUp)
            clipSize = weaponPickUp.ammoClipMax;
        
        barrelsCount = savedBarrelslvl_1.Count + savedBarrelslvl_2.Count;
        crazyGun = _crazyGun;
        
        GenerateBarrels();
    }

    void GenerateBarrels()
    {
        if (barrelsCount > 1)
        {
            foreach (var i in savedBarrelslvl_1)
            {
                barrelsLvl_1[i].gameObject.SetActive(true);
                activeBarrels.Add(barrelsLvl_1[i]);
            }
            foreach (var i in savedBarrelslvl_2)
            {
                barrelsLvl_2[i].gameObject.SetActive(true);
                activeBarrels.Add(barrelsLvl_2[i]);
            }
        }
        else
        {
            if (crazyGunAnim)
            {
                crazyGunAnim.speed = 0;
                print("crazy gun anim speed = " + crazyGunAnim.speed);
            }
            
            meatVisual.SetActive(false);
            singleBarrel.gameObject.SetActive(true);
            activeBarrels.Add(singleBarrel);
        }

        WeaponDataRandomizer data = null;

        if (weaponController && weaponController.dataRandomizer) data = weaponController.dataRandomizer;
        else if (weaponPickUp && weaponPickUp.weaponDataRandomier) data = weaponPickUp.weaponDataRandomier;

        if (data != null)
        {
            bool dead = data.dead;
            if (crazyGun && crazyGunAnim && !dead)
            {
                crazyGunAnim.speed = Random.Range(0.1f, 2f);
                print("crazy gun anim speed = " + crazyGunAnim.speed);
            }
            else if (crazyGunAnim)
            {
                crazyGunAnim.speed = 0;
                print("crazy gun anim speed = " + crazyGunAnim.speed);
            }
        
            if (data)
                data.UpdateDescription();   
        }
    }

    void OnEnable()
    {
        if (crazyGun && crazyGunAnim)
        {
            crazyGunAnim.speed = Random.Range(0.1f, 2f);
            print("crazy gun anim speed = " + crazyGunAnim.speed);   
        }
    }
}