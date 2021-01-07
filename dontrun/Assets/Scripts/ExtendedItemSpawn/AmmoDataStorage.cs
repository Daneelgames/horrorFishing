using System;
using System.Linq;
using UnityEngine;

namespace ExtendedItemSpawn
{
    [Serializable]
    public class AmmoDataStorage
    {
        public AmmoData[] allAmmo;

        /// <summary>
        /// Returns existing ammo count for specified weapon,
        /// or zero if no such ammo stored.
        /// </summary>
        public int GetAmmoCount(WeaponPickUp.Weapon weaponType)
        {
            if(weaponType == WeaponPickUp.Weapon.Null) return 0;
            
            return Mathf.FloorToInt(allAmmo.SingleOrDefault(a => a.weaponType == weaponType)?.count ?? 0);
        }

        public void AddAmmo(WeaponPickUp.Weapon weaponType, float count)
        {
            if(weaponType == WeaponPickUp.Weapon.Null) return;
            
            var existingAmmoData = allAmmo.SingleOrDefault(a => a.weaponType == weaponType);
            if (existingAmmoData is null)
            {
                var ammoList = allAmmo.ToList();
                ammoList.Add(new AmmoData
                {
                    weaponType = weaponType,
                    count = count
                });
                allAmmo = ammoList.ToArray();
            }
            else
                existingAmmoData.count += count;
        }
        
        public void ReduceAmmo(WeaponPickUp.Weapon weaponType, float count)
        {
            if(weaponType == WeaponPickUp.Weapon.Null) return;
            
            var existingAmmoData = allAmmo.SingleOrDefault(a => a.weaponType == weaponType);
            if (existingAmmoData != null)
                existingAmmoData.count -= Mathf.FloorToInt(count);
        }

        public void ResetAllAmmo()
        {
            foreach (var ammoData in allAmmo)
                ammoData.count = 0;
        }
    }
}