using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomExit : MonoBehaviour
{
    void Start()
    {
        LevelGenerator.instance.roomExits.Add(this);
    }
}