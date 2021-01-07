using UnityEngine;

namespace ExtendedItemSpawn
{
    [CreateAssetMenu(menuName = "LevelSpawnData/AmmoSpawnContainer", order = 0)]
    public class AmmoSpawnContainer : ScriptableObject
    {
            public Interactable bullets;
            public Interactable bulletPack;
    }
}