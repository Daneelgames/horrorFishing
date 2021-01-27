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
            StartCoroutine(SetLossyScale());
        }

        IEnumerator SetLossyScale()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                transform.localScale = Vector3.one;
                transform.localScale = new Vector3 (1/transform.lossyScale.x, 1/transform.lossyScale.y, 1/transform.lossyScale.z);
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }
}