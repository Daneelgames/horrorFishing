using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelBlockerController : MonoBehaviour
{
    public GameObject deathParticle;
    public List<GameObject> blockers = new List<GameObject>();

    public void DestroyBlockers()
    {
        for (int i = 0; i < blockers.Count; i++)
        {
            StartCoroutine(DestroyBlocker(i));
        }
        StartCoroutine(WaitUntilBlockersDestroyed());
    }

    IEnumerator DestroyBlocker(int i)
    {
        float t = 0;
        float tt = Random.Range(1f,10f);
        
        yield return new WaitForSeconds(tt);
        
        var blocker = blockers[i];
        var startScale = blocker.transform.localScale; 
        while (t < tt)
        {
            blocker.transform.localScale = startScale + new Vector3(Random.Range(-0.2f, 0.2f),
                Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f)) * t;
            t += Time.deltaTime;
            yield return null;
        }

        Instantiate(deathParticle, blocker.transform.position, quaternion.identity);
        Destroy(blocker);
    }
    
    IEnumerator WaitUntilBlockersDestroyed()
    {
        while (blockers.Count > 0)
        {
            yield return null;
            for (int i = blockers.Count - 1; i >= 0; i--)
            {
                if (blockers[i] == null)
                    blockers.RemoveAt(i);
                yield return null;
            }
        }
        Destroy(gameObject);
    }
}
