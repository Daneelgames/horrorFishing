using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobPartsSpawner : MonoBehaviour
{
    public MobPartsController mpc;
    public List<MobBodyPart> partsPrefabs;
    public List<Transform> partsSpawners;

    public float fullSpawnTimeMin = 60;
    public float fullSpawnTimeMax = 120;
    float fullSpawnTimeCurrent = 0;
    float spawnRate = 0;
    [Range(0, 1)]
    public float spawnRatio = 0.5f;

    private void Start()
    {
        fullSpawnTimeCurrent = Random.Range(fullSpawnTimeMin, fullSpawnTimeMax);

        spawnRate = fullSpawnTimeCurrent / partsSpawners.Count;

        StartCoroutine(SpawnParts());
    }

    IEnumerator SpawnParts()
    {
        float timeTemp = fullSpawnTimeCurrent;
        List<Transform> tempSpawnersTemp = new List<Transform>(partsSpawners);
        while (tempSpawnersTemp.Count > 0)
        {
            yield return new WaitForSeconds(spawnRate);
            int r = Random.Range(0, tempSpawnersTemp.Count);

            if (Random.value <= spawnRatio)
            {
                MobBodyPart newPart = Instantiate(partsPrefabs[Random.Range(0, partsPrefabs.Count)], tempSpawnersTemp[r].transform.position, Quaternion.identity);
                newPart.transform.localScale = Vector3.one * Random.Range(0.2f, 1.2f);
                newPart.transform.Rotate(Vector3.up, Random.Range(-180, 180));
                newPart.transform.Rotate(Vector3.forward, Random.Range(-180, 180));
                newPart.transform.Rotate(Vector3.right, Random.Range(-180, 180));
                mpc.bodyParts.Add(newPart);
            }
            tempSpawnersTemp.RemoveAt(r);

            timeTemp -= spawnRate;
        }
    }
}