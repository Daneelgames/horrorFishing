using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelsController : MonoBehaviour
{
    public float speed = 1;

    public GameObject leftBigWheel;
    public GameObject rightBigWheel;
    public GameObject leftWheel;
    public GameObject rightWheel;

    public void Forward()
    {
        leftBigWheel.transform.Rotate(Vector3.right * speed, Space.Self);
        leftWheel.transform.Rotate(Vector3.right * speed, Space.Self);
        rightBigWheel.transform.Rotate(Vector3.right * speed, Space.Self);
        rightWheel.transform.Rotate(Vector3.right * speed, Space.Self);
    }

    public void Backwards()
    {
        leftBigWheel.transform.Rotate(Vector3.left * speed, Space.Self);
        leftWheel.transform.Rotate(Vector3.left * speed, Space.Self);
        rightBigWheel.transform.Rotate(Vector3.left * speed, Space.Self);
        rightWheel.transform.Rotate(Vector3.left * speed, Space.Self);
    }

    public void Left()
    {
        leftBigWheel.transform.Rotate(Vector3.left * speed, Space.Self);
        leftWheel.transform.Rotate(Vector3.left * speed, Space.Self);
        rightBigWheel.transform.Rotate(Vector3.right * speed, Space.Self);
        rightWheel.transform.Rotate(Vector3.right * speed, Space.Self);
    }

    public void Right()
    {
        leftBigWheel.transform.Rotate(Vector3.right * speed, Space.Self);
        leftWheel.transform.Rotate(Vector3.right * speed, Space.Self);
        rightBigWheel.transform.Rotate(Vector3.left * speed, Space.Self);
        rightWheel.transform.Rotate(Vector3.left * speed, Space.Self);
    }
}