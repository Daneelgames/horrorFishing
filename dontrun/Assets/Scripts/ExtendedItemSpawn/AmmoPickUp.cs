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
                yield return new WaitForSeconds(0.1f);
                transform.localScale = Vector3.one;
                if (transform.lossyScale.x < 0.001f)
                    continue;
                transform.localScale = new Vector3 (1/transform.lossyScale.x, 1/transform.lossyScale.y, 1/transform.lossyScale.z);
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }
}