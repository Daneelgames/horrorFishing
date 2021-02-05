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
        var blocker = blockers[i];
        
        float t = 0;
        float tt = Random.Range(1f,10f);
        
        yield return new WaitForSeconds(tt);
        
        var startScale = blocker.transform.localScale; 
        while (t < tt)
        {
            if (!blocker)
                yield break;
            
            blocker.transform.localScale = startScale + new Vector3(Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
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
