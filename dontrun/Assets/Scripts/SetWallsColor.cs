using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetWallsColor : MonoBehaviour
{
    public List<MeshRenderer> walls; 
    public List<MeshRenderer> wallsBroken; 
    public List<MeshRenderer> columns; 
    public List<MeshRenderer> doorways;

    public void SetColor(Materials materials)
    {
        foreach(MeshRenderer mr in walls)
        {
            if (mr != null)
                mr.material = materials.walls[Random.Range(0, materials.walls.Count)];
        }
        foreach (MeshRenderer mr in wallsBroken)
        {
            if (mr != null)
                mr.material = materials.wallsBroken[Random.Range(0, materials.wallsBroken.Count)];
        }
        foreach (MeshRenderer mr in columns)
        {
            if (mr != null)
                mr.material = materials.columns[Random.Range(0, materials.columns.Count)];
        }
        foreach (MeshRenderer mr in doorways)
        {
            if (mr != null)
                mr.material = materials.doorways[Random.Range(0, materials.doorways.Count)];
        }
    }
}
