using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class MeatGameController : MonoBehaviour
{
    public bool active = false;
    public MeatGameTile[,] gameTiles;
    public int levelWidth = 12;
    public int levelHeight = 7;
    public List<MeatGameTile> tilesRow = new List<MeatGameTile>();
    public int playerTurns = 2;
    private int playerTurnscurrent = 0;

    public TextMeshProUGUI textFeedback;
    public TextMeshProUGUI textDynamicFeedback;
    public List<string> foundGoldStrings = new List<string>();
    public List<string> killMobStrings = new List<string>();
    public List<string> damagedByMobStrings = new List<string>();
    
    public List<Sprite> sprites;
    public Sprite floorSprite;
    Sprite playerSprite;
    public Sprite monsterSprite;
    public Sprite goldSprite;
    
    private MeatGameTile player;
    private List<MeatGameTile> monsters = new List<MeatGameTile>();
    private float turnCooldown = 0.3f;
    private float turnCooldownCurrent = 0f;
    private string horizontalString = "Horizontal";
    private string verticalString = "Vertical";
    private Vector2 input;
    private float moveX = 0;
    private float moveY = 0;

    private GameManager gm;
    private ItemsList il;
    
    Coroutine goldFeedbackDynamic;
    [Header("0 - step, 1 - gold, 2 - killMob, 3 - damagePlayer")]
    public AudioSource au;
    public List<AudioClip> sfx = new List<AudioClip>();

    public void Init()
    {
        gm = GameManager.instance;
        il = ItemsList.instance;
        
        playerSprite = sprites[Random.Range(0, sprites.Count)];
        gameTiles = new MeatGameTile[levelWidth,levelHeight];

        
        active = true;
        int tileIndex = 0; 
        
        for (int height = 0; height < levelHeight; height++)
        {
            for (var width = 0; width  < levelWidth; width++)
            {
                gameTiles[width, height] = tilesRow[tileIndex]; 
                gameTiles[width, height].Init(MeatGameTile.TileState.Floor, floorSprite, width, height, this); 
                tileIndex++;
            }
        }

        GenerateLevel();
    }

    void Update()
    {
        if (active)
        {
            moveX = Input.GetAxisRaw(horizontalString);
            moveY = Input.GetAxisRaw(verticalString) * -1;

            if (turnCooldownCurrent < turnCooldown)
            {
                turnCooldownCurrent += Time.deltaTime;
            }
            else
            {
                input = new Vector2(moveX, moveY); 
                if (input.magnitude > 0)
                {
                    turnCooldownCurrent = 0;
                    StartCoroutine(MovePlayer());   
                }
            }   
        }
    }

    void GenerateLevel()
    {
        int wallsAmount = 7;
        for (int i = 0; i < wallsAmount; i++)
        {
            int randomX = Random.Range(0, levelWidth);

            gameTiles[randomX,  Mathf.Clamp(0,levelHeight - 1, wallsAmount)].SetWall(floorSprite);
            gameTiles[Mathf.Clamp(0,levelWidth - 1, randomX + 1),  Mathf.Clamp(0,levelHeight - 1, wallsAmount)].SetWall(floorSprite);
            gameTiles[randomX, Mathf.Clamp(0,levelHeight - 1, wallsAmount + 1)].SetWall(floorSprite);
        }
        monsters.Clear();

        SetPlayer();
        SetMonsters();
        SetGold();
    }

    void SetPlayer()
    {
        player = gameTiles[6, 3];
        player.SetPlayer(playerSprite);
    }

    void SetMonsters()
    {
        int monstersAmount = 3 + GutProgressionManager.instance.currentLevelDifficulty;
        for (int i = 0; i < monstersAmount; i++)
        {
            MeatGameTile newMob = new MeatGameTile();
            newMob = GetEmptyTile();
            newMob.SetMonster(monsterSprite);
            monsters.Add(newMob);
        }
    }

    void SetGold()
    {
        int goldAmount = 3 + GutProgressionManager.instance.currentLevelDifficulty;
        for (int i = 0; i < goldAmount; i++)
        {
            GetEmptyTile().SetGold(goldSprite);
        }
    }

    MeatGameTile GetEmptyTile()
    {
        MeatGameTile newTile = null;
        List<MeatGameTile> tilesTemp = new List<MeatGameTile>();
        for (var width = 0; width  < levelWidth; width++)
        {
            for (int height = 0; height < levelHeight; height++)
            {
                if (gameTiles[width, height].state == MeatGameTile.TileState.Floor || height == levelHeight -1)
                {
                    tilesTemp.Add(gameTiles[width, height]);   
                } 
            }
        }
        
        return tilesTemp[Random.Range(0,tilesTemp.Count)];
    }

    void MoveMobs()
    {
        int _moveX = 0;
        int _moveY = 0;
        for (var index = 0; index < monsters.Count; index++)
        {
            var mob = monsters[index];

            if (mob.x != player.x)
            {
                if (mob.x < player.x)
                    _moveX = 1;
                else if (mob.x > player.x)
                    _moveX = -1;   
            }
            else // on the same line
            {
                if (mob.y < player.y)
                    _moveY = 1;
                else if (mob.y > player.y)
                    _moveY = -1;   
            }
            
            MeatGameTile newTile = null;
            if (Mathf.Abs(_moveX) > Mathf.Abs(_moveY))
            {
                newTile = GetObject(mob, _moveX, 0);
            }
            else if (Mathf.Abs(_moveX) < Mathf.Abs(_moveY))
            {
                newTile = GetObject(mob,0, _moveY);
            }

            if (newTile != null)
            {
                switch (newTile.state)
                {
                    case MeatGameTile.TileState.Wall:
                        mob.MoveAnim();
                        break;
                    case MeatGameTile.TileState.Gold:
                        mob.SetFloor(floorSprite);
                        mob = newTile;
                        newTile.SetMonster(monsterSprite);
                        mob.MoveAnim();
                        monsters[index] = mob;
                        break;
                    case MeatGameTile.TileState.Floor:
                        mob.SetFloor(floorSprite);
                        mob = newTile;
                        newTile.SetMonster(monsterSprite);
                        mob.MoveAnim();
                        monsters[index] = mob;
                        break;
                    case MeatGameTile.TileState.Player:
                        il.LoseGold(1);
                        mob.MoveAnim();
                        player.MoveAnim();
                        monsters[index] = mob;
                        textFeedback.text = damagedByMobStrings[gm.language] +  + il.gold;
                        GoldFeedback(-1);
                        PlaySfx(3);
                        break;
                }
            }
        }
    }

    void GoldFeedback(int offset)
    {
        if (goldFeedbackDynamic != null)
            StopCoroutine(goldFeedbackDynamic);
        goldFeedbackDynamic = StartCoroutine(GoldFeedbackDynamic(offset));
    }

    IEnumerator GoldFeedbackDynamic(int goldffset)
    {
        if (goldffset < 0)
        {
            textDynamicFeedback.color = new Color(200,20,0);
            textDynamicFeedback.text = goldffset.ToString();   
        }
        else
        {
            textDynamicFeedback.color = new Color(255,150,0);
            textDynamicFeedback.text = "+" + goldffset;   
        }

        textDynamicFeedback.transform.localPosition = player.transform.localPosition;
        textDynamicFeedback.transform.localScale = Vector3.one;
        float t = 0.5f;
        float curT = 0;

        while (curT < t)
        {
            curT += Time.deltaTime;
            textDynamicFeedback.transform.localPosition = Vector3.Lerp(textDynamicFeedback.transform.localPosition,
                textDynamicFeedback.transform.localPosition + Vector3.up * 10f, curT / t);
            textDynamicFeedback.transform.localScale = Vector3.Lerp(textDynamicFeedback.transform.localScale,
                Vector3.one * 2f, curT / t);
            yield return null;
        }

        t = 2f;
        curT = 0;
        
        while (curT < t)
        {
            curT += Time.deltaTime;
            textDynamicFeedback.transform.localPosition = Vector3.Lerp(textDynamicFeedback.transform.localPosition, textDynamicFeedback.transform.localPosition + Vector3.up * 25f, curT / t);    
            textDynamicFeedback.transform.localScale = Vector3.Lerp(textDynamicFeedback.transform.localScale, Vector3.zero, curT / t);    
            yield return null;
        }
    }
    
    IEnumerator MovePlayer()
    {
        // get direction
        MeatGameTile newTile = null;
        if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
        {
            newTile = GetObject(player,moveX, 0);
        }
        else if (Mathf.Abs(moveX) < Mathf.Abs(moveY))
        {
            newTile = GetObject(player,0, moveY);
        }
             
        if (newTile != null)
        {
            switch (newTile.state)
            {
                case MeatGameTile.TileState.Wall:
                    player.MoveAnim();
                    //textFeedback.text = "";
                    break;
                
                case MeatGameTile.TileState.Gold:
                    il.gold++;
                    textFeedback.text = foundGoldStrings[gm.language] + il.gold;
                    player.SetFloor(floorSprite);
                    player = newTile;
                    newTile.SetPlayer(playerSprite);
                    player.MoveAnim();
                    GoldFeedback(1);
                    PlaySfx(1);
                    
                    MeatGameTile newMob2 = GetEmptyTile();
                    newMob2.SetMonster(monsterSprite);   
                    newMob2.MoveAnim();
                    monsters.Add(newMob2);
                    break;
                
                case MeatGameTile.TileState.Floor:
                    player.SetFloor(floorSprite);
                    player = newTile;
                    newTile.SetPlayer(playerSprite);
                    player.MoveAnim();
                    //textFeedback.text = "";
                    PlaySfx(0);
                    break;
                
                case MeatGameTile.TileState.Monster:
                    // kill monster
                    textFeedback.text = killMobStrings[gm.language];
                    monsters.Remove(newTile);
                    //newTile.Move();
                    newTile.SetFloor(floorSprite);
                    player.MoveAnim();
                    newTile.MoveAnim();
                    PlaySfx(2);
                    GetEmptyTile().SetGold(goldSprite);
                    
                    MeatGameTile newMob = GetEmptyTile();
                    newMob.SetMonster(monsterSprite);
                    newMob.MoveAnim();
                    monsters.Add(newMob);
                    break;
            }

            float t = 0;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                turnCooldownCurrent = 0;
                yield return null;
            }
            
            CheckLoadState();
            if (active)
                MoveMobs();
            /*
            playerTurnscurrent++;
            if (playerTurnscurrent == playerTurns)
            {
                playerTurnscurrent = 0;
                MoveMobs();   
            }
            */
        }
    }

    void PlaySfx(int index)
    {
        au.pitch = Random.Range(0.75f, 1.25f);
        au.clip = sfx[index];
        au.Play();
    }

    MeatGameTile GetObject(MeatGameTile tile, float offsetX, float offsetY)
    {
        if (offsetX > 0) offsetX = 1;
        else if (offsetX < 0) offsetX = -1;
        else if (offsetY > 0) offsetY = 1;
        else if (offsetY < 0) offsetY = -1;
        
        if (tile.x + offsetX >= levelWidth || tile.x + offsetX < 0 ||
            tile.y + offsetY >= levelHeight || tile.y + offsetY < 0)
        {
            return null;
        }
        else 
        {
            return gameTiles[Mathf.RoundToInt(tile.x + offsetX), Mathf.RoundToInt(tile.y + offsetY)];
        }
    }
    
    public void CheckLoadState()
    {
        if (gm.readyToStartLevel)
        {
            active = false;
            StartCoroutine(GameOver());   
        }
    }

    IEnumerator GameOver()
    {
        for (var index0 = 0; index0 < gameTiles.GetLength(0); index0++)
        for (var index1 = 0; index1 < gameTiles.GetLength(1); index1++)
        {
            var tile = gameTiles[index0, index1];
            tile.Hide();
        }

        yield return new WaitForSeconds(1);
        
        gameObject.SetActive(false);
    }
}