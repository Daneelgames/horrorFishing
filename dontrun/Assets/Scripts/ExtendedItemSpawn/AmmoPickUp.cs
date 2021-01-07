using System.Collections;
using UnityEngine;

namespace ExtendedItemSpawn
{
    public class AmmoPickUp : MonoBehaviour
    {
        public WeaponPickUp.Weapon weaponType;
        public float amount = 7;
        public AudioClip pickUpClip;
        public Rigidbody rb;
        
        public Interactable interactable;
        SpawnController spawner;

        private void Start()
        {
            spawner = SpawnController.instance;
            StartCoroutine(CheckIfOutsideTheLevel());
        }

        IEnumerator CheckIfOutsideTheLevel()
        {
            while (true)
            {
                yield return new WaitForSeconds(3);
                
                if (transform.position.y < -100 && spawner.spawners.Count > 0)
                    transform.position = spawner.spawners[Random.Range(0, spawner.spawners.Count)].transform.position + Vector3.up;
            }
        }
    }
}