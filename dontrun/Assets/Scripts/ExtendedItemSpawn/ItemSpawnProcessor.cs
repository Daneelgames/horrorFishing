using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ExtendedItemSpawn
{
    public class ItemSpawnProcessor
    {
        private readonly GameManager _gameManager;
        private readonly List<WeaponPickUp.Weapon> _generatedWeapons = new List<WeaponPickUp.Weapon>();

        public ItemSpawnProcessor(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        public List<Interactable> SpawnItems(List<SpawnGroup> groups, List<AmmoSpawnInfo> ammoInfo, bool showLog)
        {
            if(groups == null) throw new ArgumentNullException(nameof(groups));
            if(ammoInfo == null) throw new ArgumentNullException(nameof(ammoInfo));

            _generatedWeapons.Clear();
            var result = new GenerationResult();
            foreach (var group in groups)
            {
                result.Log += "--- Processing new group ---\n";
                var generatedChance = Random.Range(0, 100);
                var needGenerate = generatedChance < group.groupChance;
                result.Log += $"Group chance: {group.groupChance} random roll: {generatedChance} generate this group: {needGenerate}\n";
                result.AddResult(ProcessGroup(group));
            }
            result.AddResult(ProcessAmmo(ammoInfo));

            if (showLog && result.HasWarnings)
                Debug.LogWarning(result.Log);
            else if (showLog)
                Debug.Log(result.Log);

            return result.Data;
        }

        private GenerationResult ProcessAmmo(List<AmmoSpawnInfo> ammoInfo)
        {
            if(ammoInfo == null) throw new ArgumentNullException(nameof(ammoInfo));
            
            var result = new GenerationResult();
            foreach (var info in ammoInfo)
            {
                var validationErrors = ValidateAmmo(info);
                if (string.IsNullOrEmpty(validationErrors))
                    result.AddResult(GenerateAmmo(info));
                else
                {
                    result.HasWarnings = true;
                    result.Log += validationErrors;
                }
            }

            return result;
        }

        private GenerationResult GenerateAmmo(AmmoSpawnInfo info)
        {
            if(info == null) throw new ArgumentNullException(nameof(info));

            var weaponType = info.value.bulletPack
                ? info.value.bulletPack.ammoPickUp.weaponType
                : info.value.bullets.ammoPickUp.weaponType;

            var haveWeapon = !info.playerHaveWeapon || _gameManager.player.wc.AlreadyHaveThisWeapon(weaponType);
            var spawnedWeapon = !info.weaponSpawned || _generatedWeapons.Any(w => w == weaponType);
            
            if(!haveWeapon && !spawnedWeapon)
                return new GenerationResult
                {
                    Log = $"Ammo for {(weaponType)} skipped, player have this weapon: {_gameManager.player.wc.AlreadyHaveThisWeapon(weaponType)}" +
                          $", this weapon spawned on level: {_generatedWeapons.Any(w => w == weaponType)}\n"
                };
            
            var ammoToSpawn = Random.Range(info.minCount, info.maxCount);
            if (info.reduceByPlayerOwnedAmmo)
                ammoToSpawn = Math.Max(ammoToSpawn - _gameManager.itemList.ammoDataStorage.GetAmmoCount(weaponType), 0);
            
            return CalculateAmmoPacks(ammoToSpawn, weaponType, info);
        }

        private GenerationResult CalculateAmmoPacks(int ammoToSpawn, WeaponPickUp.Weapon weaponType, AmmoSpawnInfo info)
        {
            if(info == null) throw new ArgumentNullException(nameof(info));

            string log;
            var result = new GenerationResult();
            if (!info.value.bulletPack) //This weapon have no ammo packs
            {
                for (var i = 0; i < ammoToSpawn; i++)
                    result.Data.Add(info.value.bullets);
                log = $"+ Generated {ammoToSpawn} single ammo for {weaponType}";
            }
            else if(!info.value.bullets) //This weapon have no single bullets
            {
                var packsNumber = ammoToSpawn / info.value.bulletPack.ammoPickUp.amount;
                for (var i = 0; i < packsNumber; i++)
                    result.Data.Add(info.value.bulletPack);
                log = $"+ Generated {packsNumber} packs of ammo for {weaponType} (requested {ammoToSpawn} ammo)";
            }
            else //Have both ammo packs and single bullets
            {
                var packsNumber = ammoToSpawn / info.value.bulletPack.ammoPickUp.amount;
                var singleAmmoNumber = Math.Min(ammoToSpawn % info.value.bulletPack.ammoPickUp.amount, 10);
                for (var i = 0; i < packsNumber; i++)
                    result.Data.Add(info.value.bulletPack);
                for (var i = 0; i < singleAmmoNumber; i++)
                    result.Data.Add(info.value.bullets);
                log = $"+ Generated {packsNumber} packs of ammo and {singleAmmoNumber} single ammo for {weaponType} " +
                      $"({packsNumber * info.value.bulletPack.ammoPickUp.amount + singleAmmoNumber} total)";
            }

            result.Log = log;
            return result;
        }

        private GenerationResult ProcessGroup(SpawnGroup group)
        {
            if(group == null) throw new ArgumentNullException(nameof(group));

            var validationErrors = ValidateGroup(group);
            if(!string.IsNullOrEmpty(validationErrors))
                return new GenerationResult
                {
                    Log = $"{validationErrors}Group was skipped because of validation errors\n",
                    HasWarnings = true
                };

            var result = new GenerationResult();
            var groupSpawnCount = Random.Range(group.minCount, group.maxCount);
            
            var usualItemsChecked = CheckSimpleItems(group.simpleItems, result);
            var ammoWeaponsChecked = CheckWeapons(group.weapons, result);

            var totalItemsInGroup = usualItemsChecked.Count + ammoWeaponsChecked.Count;
            result.Log += $"Items in group: {totalItemsInGroup}, generate count setting min: {group.minCount} max: {group.maxCount} randomly chosen: {groupSpawnCount} \n";
            
            result.AddResult(GenerateGroupItems(usualItemsChecked, ammoWeaponsChecked, groupSpawnCount));
            return result;
        }

        private GenerationResult GenerateGroupItems(List<SimpleItemSpawnInfo> usualItems, List<WeaponSpawnInfo> weapons, int groupSpawnCount)
        {
            if(usualItems == null) throw new ArgumentNullException(nameof(usualItems));
            if(weapons == null) throw new ArgumentNullException(nameof(weapons));
            
            var result = new GenerationResult();
            for (var i = 0; i < groupSpawnCount && (usualItems.Count > 0 || weapons.Count > 0); i++)
            {
                var totalWeight = usualItems
                    .Select(item => item.spawnWeight)
                    .Concat(weapons.Select(item => item.spawnWeight))
                    .Sum();

                // result.Log += $"Total weight: {totalWeight}, usual items left: {usualItems.Count}, " +
                //               $"weapons with ammo left: {weapons.Count}, iterations left: {groupSpawnCount - i}\n";

                var randomWeight = Random.Range(1, totalWeight + 1);
                // result.Log += $"Rolled weight: {randomWeight}\n";
                result.AddResult(ItemGenerationIteration(randomWeight, usualItems, weapons));
            }
            return result;
        }

        private GenerationResult ItemGenerationIteration(int targetWeight, List<SimpleItemSpawnInfo> usualItems, List<WeaponSpawnInfo> weapons)
        {
            if(usualItems == null) throw new ArgumentNullException(nameof(usualItems));
            if(weapons == null) throw new ArgumentNullException(nameof(weapons));
            
            SimpleItemSpawnInfo simpleItemToGenerate = null;
            foreach (var item in usualItems)
            {
                if (targetWeight <= item.spawnWeight)
                {
                    simpleItemToGenerate = item;
                    break;
                }
                targetWeight -= item.spawnWeight;
            }
            if (simpleItemToGenerate != null)
            {
                usualItems.Remove(simpleItemToGenerate);
                return GenerateUsualItem(simpleItemToGenerate);
            }
            
            WeaponSpawnInfo weaponToGenerate = null;
            foreach (var item in weapons)
            {
                if (ChechWeaponInHandsRules(item)) // if cant spawn
                {
                    // null
                }
                else if (targetWeight <= item.spawnWeight)
                {
                    weaponToGenerate = item;
                    break;
                }
                targetWeight -= item.spawnWeight;
            }
            if (weaponToGenerate != null)
            {
                weapons.Remove(weaponToGenerate);
                return GenerateWeapon(weaponToGenerate);
            }

            //You should never get to this point :C
            throw new InvalidOperationException("Item generation broken! Check this fucking code and fix bugs with weight!");
        }

        bool ChechWeaponInHandsRules(WeaponSpawnInfo item)
        {
            bool remove = false;
            switch (item.playerHaveThis)
            {
                case WeaponInHandsRule.NotCheckThis:
                    break;
                case WeaponInHandsRule.Yes:
                    if (!_gameManager.player.wc.AlreadyHaveThisWeapon(item.weapon.value.weaponPickUp.weapon))
                    {
                        remove = true;
                    };
                    break;
                case WeaponInHandsRule.No:
                    if (_gameManager.player.wc.AlreadyHaveThisWeapon(item.weapon.value.weaponPickUp.weapon))
                    {
                        remove = true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return remove;
        }

        //Все особые условия для пушек с патриками тут пихать
        private GenerationResult GenerateWeapon(WeaponSpawnInfo item)
        {
            
            var countRoll = Random.Range(item.minCount, item.maxCount + 1);
            var result = new GenerationResult();
            for (var i = 0; i < countRoll; i++)
                result.Data.Add(item.weapon.value);

            if (countRoll > 0)
            {
                _generatedWeapons.Add(item.weapon.value.weaponPickUp.weapon);
                result.Log += $"+ Generated {countRoll} {item.weapon.value.itemName}\n";
            }
            
            return result;
        }

        //Все особые условия для простых предметов тут пихать
        private GenerationResult GenerateUsualItem(SimpleItemSpawnInfo simpleItem)
        {
            //Range(X, Y) Y - exclused from results, to to roll 1,2,3 u need Range(1, 4);
            var countRoll = Random.Range(simpleItem.minCount, simpleItem.maxCount + 1);
            var result = new GenerationResult();
            for (var i = 0; i < countRoll; i++)
                result.Data.Add(simpleItem.item.value);
            
            if (countRoll > 0) result.Log += $"+ Generated {countRoll} {simpleItem.item.value.itemName}\n";
            
            return result;
        }

        private static List<WeaponSpawnInfo> CheckWeapons(WeaponSpawnInfo[] weapons, GenerationResult result)
        {
            if(weapons == null) throw new ArgumentNullException(nameof(weapons));
            if(result == null) throw new ArgumentNullException(nameof(result));

            var validatedItems = new List<WeaponSpawnInfo>();
        
            foreach (var item in weapons)
            {
                var validationErrors = string.Empty;
                if (item.minCount < 0)
                    validationErrors = $"Configuration error! {nameof(item.minCount)} must be >= 0 actual value: {item.minCount}\n";
                if (item.maxCount < 1)
                    validationErrors = $"Configuration error! {nameof(item.maxCount)} must be >= 1 actual value: {item.maxCount}\n";
                if (item.maxCount < item.minCount)
                    validationErrors = $"Configuration error! {nameof(item.maxCount)} must be >= {nameof(item.minCount)} " +
                                       $"actual values: {item.maxCount} < {item.minCount}\n";
                if (item.spawnWeight <= 0)
                    validationErrors = $"Configuration error! {nameof(item.spawnWeight)} must be > 0 actual value: {item.spawnWeight}\n";
                
                if (!string.IsNullOrEmpty(validationErrors))
                {
                    result.Log += $"{validationErrors}Item {item.weapon.value.itemName} was skipped because of validation errors\n";
                    result.HasWarnings = true;
                }
                else
                    validatedItems.Add(item);
            }

            return validatedItems;
        }

        private static List<SimpleItemSpawnInfo> CheckSimpleItems(SimpleItemSpawnInfo[] usualItems, GenerationResult result)
        {
            if(usualItems == null) throw new ArgumentNullException(nameof(usualItems));
            if(result == null) throw new ArgumentNullException(nameof(result));

            var validatedItems = new List<SimpleItemSpawnInfo>();
        
            foreach (var usualItem in usualItems)
            {
                var validationErrors = string.Empty;
                if (usualItem.minCount < 0)
                    validationErrors = $"Configuration error! {nameof(usualItem.minCount)} must be >= 0 actual value: {usualItem.minCount}\n";
                if (usualItem.maxCount < 1)
                    validationErrors = $"Configuration error! {nameof(usualItem.maxCount)} must be >= 1 actual value: {usualItem.maxCount}\n";
                if (usualItem.maxCount < usualItem.minCount)
                    validationErrors = $"Configuration error! {nameof(usualItem.maxCount)} must be >= {nameof(usualItem.minCount)} " +
                                       $"actual values: {usualItem.maxCount} < {usualItem.minCount}\n";
                if (usualItem.spawnWeight <= 0)
                    validationErrors = $"Configuration error! {nameof(usualItem.spawnWeight)} must be > 0 actual value: {usualItem.spawnWeight}\n";
                
                if (!string.IsNullOrEmpty(validationErrors))
                {
                    result.Log += $"{validationErrors}Item {usualItem.item.value.itemName} was skipped because of validation errors\n";
                    result.HasWarnings = true;
                }
                else
                    validatedItems.Add(usualItem);
            }

            return validatedItems;
        }

        private static string ValidateAmmo(AmmoSpawnInfo item)
        {
            if(item == null) throw new ArgumentNullException(nameof(item));
            
            var validationErrors = string.Empty;
            if (item.minCount < 0)
                validationErrors = $"Configuration error! {nameof(item.minCount)} must be >= 0 actual value: {item.minCount}\n";
            if (item.maxCount < 1)
                validationErrors = $"Configuration error! {nameof(item.maxCount)} must be >= 1 actual value: {item.maxCount}\n";
            if (item.maxCount < item.minCount)
                validationErrors = $"Configuration error! {nameof(item.maxCount)} must be >= {nameof(item.minCount)} " +
                                   $"actual values: {item.maxCount} < {item.minCount}\n";
            return validationErrors;
        }
        
        private static string ValidateGroup(SpawnGroup group)
        {
            if(group == null) throw new ArgumentNullException(nameof(group));
            
            var validationErrors = string.Empty;
            if (group.minCount < 0)
                validationErrors = $"Configuration error! {nameof(group.minCount)} must be >= 0 actual value: {group.minCount}\n";
            if (group.maxCount < 1)
                validationErrors = $"Configuration error! {nameof(group.maxCount)} must be >= 1 actual value: {group.maxCount}\n";
            if (group.maxCount < group.minCount)
                validationErrors = $"Configuration error! {nameof(group.maxCount)} must be >= {nameof(group.minCount)} " +
                                   $"actual values: {group.maxCount} < {group.minCount}\n";
            if (group.simpleItems.Length == 0 && group.weapons.Length == 0)
                validationErrors = $"Configuration error! group has no items!\n";
            
            return validationErrors;
        }
        
        private sealed class GenerationResult
        {
            internal List<Interactable> Data { get; } = new List<Interactable>();
            internal string Log { get; set; }
            internal bool HasWarnings { get; set; }
            
            internal void AddResult(GenerationResult result)
            {
                Log += result.Log;
                Data.AddRange(result.Data);
            }
        }
    }
}