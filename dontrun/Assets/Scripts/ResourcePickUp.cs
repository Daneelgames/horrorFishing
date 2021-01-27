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