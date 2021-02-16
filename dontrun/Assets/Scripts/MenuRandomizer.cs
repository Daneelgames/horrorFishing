using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRandomizer : MonoBehaviour
{
    public GameObject startView;
    public List<GameObject> viewsPool;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    public void Init()
    {
        foreach(GameObject v in viewsPool)
        {
            v.SetActive(false);
        }
        startView.SetActive(true);

        StartCoroutine(ChangeCameraVew());   
    }
    
    IEnumerator ChangeCameraVew()
    {
        yield return new WaitForSeconds(Random.Range(10,12.5f));
        startView.SetActive(false);
        int r = 0;

        while (true)
        {
            foreach (GameObject v in viewsPool)
            {
                v.SetActive(false);
            }
            viewsPool[r].SetActive(true);

            yield return new WaitForSeconds(Random.Range(10,15));
            viewsPool[r].SetActive(false);
            r++;
            if (r >= viewsPool.Count)
                r = 0;
        }
    }
}
