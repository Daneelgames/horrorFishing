using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobBodyPart : MonoBehaviour
{
    [Range(0, 10)]
    public float damageModificator = 1;
    public Collider coll;
    public AudioSource au;
    public ParticleSystem deathParticle;

    public HealthController hc;
    
    private void Start()
    {
        if (au)
        {
            au.Stop();
            au.pitch = Random.Range(0.75f, 1.25f);
            au.Play();
        }
    }
    
    
    [ContextMenu("Get Collider and death particles in children")]
    void GetPositions()
    {
        coll = GetComponent<Collider>();
        deathParticle = GetComponentInChildren<ParticleSystem>(true);
    }

    //    public Rigidbody rb;
}
