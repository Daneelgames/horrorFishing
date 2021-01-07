using System;
using UnityEngine;

namespace ExtendedItemSpawn
{
    [Serializable]
    public sealed class SpawnGroup
    {
        [Range(0, 100)]
        public int groupChance = 100;

        [Header("Amount of item positions spawned (each position have own count):")]
        [Range(0, 100)]
        public int minCount = 1;
        [Range(1, 100)]
        public int maxCount = 1;
        
        public WeaponSpawnInfo[] weapons;
        public SimpleItemSpawnInfo[] simpleItems;
    }
}