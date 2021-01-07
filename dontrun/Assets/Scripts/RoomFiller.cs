using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFiller : MonoBehaviour
{
    public RoomController masterRoom;
    public bool spawner = true;
    public int tileIndex = 0;
    public bool cultTile = false;
    public StatusEffects.StatusEffect statusEffect;

    public int steps = 1;
    // Start is called before the first frame update
    void Start()
    {
        if (steps == 0) steps = 1;
        
        LevelGenerator.instance.roomFillers.Add(this);
    }
}
