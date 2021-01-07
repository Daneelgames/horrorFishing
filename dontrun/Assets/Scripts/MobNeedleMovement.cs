using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class MobNeedleMovement : MonoBehaviour
{
    public float stepDelay = 1;
    float stepDelayNormal = 1f;
    float stepDelayDamaged = 0.3f;
    public float stepDistanceMax = 30;

    public float deniedDistance = 10;
    public Vector3 deniedPosition = new Vector3(0,0,-30);
    
    private Vector3 targetPos;
    private LevelGenerator lg;
    public List<TileController> tempTiles;
    public AudioSource au;
    public AudioSource au2;
    public AudioSource au3;
    private PlayerMovement pm;

    private bool authorative = true;
    
    void Start()
    {
        stepDelayNormal = stepDelay;
        
        lg = LevelGenerator.instance;
        pm = PlayerMovement.instance;
        targetPos = transform.position;


        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive &&
            GLNetworkWrapper.instance.localPlayer.isServer == false)
            authorative = false;
        
        if (authorative)
            StartCoroutine(FollowPlayer());
    }

    public void DamagedByPlayer()
    {
        //if (authorative && Vector3.Distance(pm.transform.position, transform.position) >= 25)
        
        if (authorative)
        {
            stepDelay = stepDelayDamaged;
        }
    }

    IEnumerator FollowPlayer()
    {
        int j = 0;
        while (true)
        {
            var closestPlayer = PlayerMovement.instance.hc;
            if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
                closestPlayer = GLNetworkWrapper.instance.GetClosestPlayer(transform.position);
            
            tempTiles = new List<TileController>(lg.levelTilesInGame);
            float currentMinDistance = stepDistanceMax;
            float newDist = stepDistanceMax;
            for (int i = tempTiles.Count - 1; i >= 0; i--)
            {
                newDist = Vector3.Distance(transform.position, tempTiles[i].transform.position);

                if (newDist > stepDistanceMax || tempTiles[i].trapTile || Vector3.Distance(tempTiles[i].transform.position, deniedPosition) <= deniedDistance)
                {
                    tempTiles.RemoveAt(i);
                    continue;
                }

                
                // compare this tile
                newDist = Vector3.Distance(tempTiles[i].transform.position, closestPlayer.transform.position);
                if (newDist <= currentMinDistance)
                {
                    currentMinDistance = newDist;
                    targetPos = tempTiles[i].transform.position;
                }

            }

            if (newDist <= stepDistanceMax)
            {
                targetPos = closestPlayer.transform.position;   
            }

            au.Stop();
            au.pitch = Random.Range(0.75f, 1.25f);
            au.Play();
            
            /////
            // GO UP
            ///////
            stepDelay = stepDelayNormal;
            float t = stepDelay / 2;
            
            j++;
            if (j == 10 || j == 20 || j == 21 || j == 22 || j == 23)
            {
                t /= 4;
                
                au3.Stop();
                au3.pitch = Random.Range(0.75f, 1.25f);
                au3.Play();
            }

            if (closestPlayer.health > closestPlayer.healthMax * 0.3f)
            {
                if (Vector3.Distance(targetPos, closestPlayer.transform.position) <= 6.5f)
                {
                    
                    targetPos = closestPlayer.transform.position;
                }    
            }
            
            float tCurr = 0;
            while (tCurr < t)
            {
                tCurr += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, targetPos + Vector3.up * 10, tCurr);
                yield return null;
            }

            transform.position = targetPos + Vector3.up * 10;

            au2.Stop();
            au2.pitch = Random.Range(0.75f, 1.25f);
            au2.Play();

            
            ///////
            // GO DOWN
            ///////

            t = stepDelay;
            if (j == 10 || j == 20 || j == 21 || j == 22 || j == 23)
            {
                t /= 4;
            }

            if (j >= 23)
            {
                j = 0;
            }

            if (closestPlayer.health > closestPlayer.healthMax * 0.3f)
            {
                if (Vector3.Distance(targetPos, closestPlayer.transform.position) <= 6.5f)
                {
                    targetPos = closestPlayer.transform.position;
                }
            }
            while (tCurr < t)
            {
                tCurr += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, targetPos, tCurr);
                yield return null;
            }  
            transform.position = targetPos;
        }
    }
}
