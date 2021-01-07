using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeatGameTile : MonoBehaviour
{
    public Image tileImageLink; 
    public int x = 0;
    public int y = 0;
    
    private MeatGameController mgc;
    
    public enum TileState
    {
        Wall,
        Floor,
        Player,
        Monster,
        Gold
    }

    public TileState state = TileState.Floor;
    private Coroutine scaleOverTime;

    public void Init(TileState newState, Sprite newSprite, int _x, int _y, MeatGameController _mgc)
    {
        mgc = _mgc;
        x = _x;
        y = _y;
        
        tileImageLink = GetComponent<Image>();
        state = newState;

        Quaternion newRot = Quaternion.identity;
        newRot.eulerAngles = new Vector3(0,0, Random.Range(-25,25));
        transform.rotation = newRot;
        
        switch (state)
        {
            case TileState.Floor:
                SetFloor(newSprite);
            break;
            
            case TileState.Wall:
                SetWall(newSprite);
            break;
        }

        MoveAnim();
    }

    public void SetFloor(Sprite newSprite)
    {
        state = TileState.Floor;
        tileImageLink.sprite = newSprite;
        tileImageLink.color = new Color(0.2f + Random.Range(-0.1f, 0.1f), 0.2f, 0.2f);
    }

    public void SetWall(Sprite newSprite)
    {
        state = TileState.Wall;
        tileImageLink.sprite = newSprite;
        tileImageLink.color = Color.black;
    }
    public void SetPlayer(Sprite newSprite)
    {
        state = TileState.Player;
        tileImageLink.sprite = newSprite;
        tileImageLink.color = Color.white;
    }
    public void SetMonster(Sprite newSprite)
    {
        state = TileState.Monster;
        tileImageLink.sprite = newSprite;
        tileImageLink.color = Color.white;
    }
    public void SetGold(Sprite newSprite)
    {
        state = TileState.Gold;
        tileImageLink.sprite = newSprite;
        tileImageLink.color = new Color(1, 0.4f,0);
    }

    public MeatGameTile Move()
    {
        SetFloor(mgc.floorSprite);

        int newX = Mathf.Clamp(0, mgc.levelWidth-1, Random.Range(x - 1, x + 1));
        int newy = Mathf.Clamp(0, mgc.levelHeight-1, Random.Range(y - 1, y + 1));
        mgc.gameTiles[newX, newy].SetMonster(mgc.monsterSprite);
        return mgc.gameTiles[newX, newy];
    }

    public void MoveAnim()
    {
        if (scaleOverTime != null)
            StopCoroutine(scaleOverTime);
        
        scaleOverTime = StartCoroutine(ScaleOverTime());
    }
    public void Hide()
    {
        StartCoroutine(HideOverTime());
    }

    IEnumerator ScaleOverTime()
    {
        float t = 0.33f;
        float curT = 0;
        
        while (curT < t)
        {
            curT += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 1.3f, curT / t);    
            yield return null;
        }
        
        t = 0.6f;
        curT = 0;
        while (curT < t)
        {
            curT += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, curT / t);    
            yield return null;
        }
    }
    IEnumerator HideOverTime()
    {
        float t = 1f;
        float curT = 0;
        
        while (curT < t)
        {
            curT += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, curT / t);    
            yield return null;
        }
    }
}