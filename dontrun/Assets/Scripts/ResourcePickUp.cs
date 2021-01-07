using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePickUp : MonoBehaviour
{
    public ItemsList.ResourceType resourceType = ItemsList.ResourceType.Tool;

    public int questItemIndex = -1;

    public int amount = 7;
    public AudioClip pickUpClip;
    public bool skill = false;
    public Rigidbody rb;

    public ToolController tool;

    public QuestItemController questItem;

    SpawnController spawner;

    private void Start()
    {
        spawner = SpawnController.instance;
        
        if (resourceType == ItemsList.ResourceType.Key || resourceType == ItemsList.ResourceType.Skill)
            transform.parent = null;
        
        //StartCoroutine(CheckIfOutsideTheLevel());
    }

    IEnumerator CheckIfOutsideTheLevel()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            if (transform.position.y < -100)
                transform.position = spawner.spawners[Random.Range(0, spawner.spawners.Count)].transform.position + Vector3.up;
        }
    }
}