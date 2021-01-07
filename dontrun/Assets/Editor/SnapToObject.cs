using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

public class SnapToObject : MonoBehaviour
{
    [MenuItem("Custom/Snap To Ground %g")]
    public static void Ground()
    {
        foreach (var transform in Selection.transforms)
        {
            RaycastHit hitInfo;

            var hits = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down, 500);
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.layer != 20) // floor
                    continue;

                transform.position = hit.point;
                
                var normalRotation = Quaternion.LookRotation(hit.normal);
                transform.rotation = normalRotation;
                transform.Rotate(90,0,0);
                transform.Rotate(0,Random.Range(1 ,359f),0);
                break;
            }
        }
    }
}
