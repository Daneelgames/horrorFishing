using System.Collections;
using PlayerControls;
using UnityEngine;
public class ButtonReciever : MonoBehaviour
{
    public bool hubButton = false; 
    public GameObject objectToSetActive;
    public int goTofloor = 0;
    public int setCurrentDifficultyLevel = 0;

    private Coroutine nextLevelcoroutine;

    public void Pressed()
    {
        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            GLNetworkWrapper.instance.ButtonPressed();
        }
        else
        {
            ButtonPressedOnClient(false);
        }
    }

    public void ButtonPressedOnClient(bool coop)
    {
        if (objectToSetActive)
            objectToSetActive.SetActive(true);
        PlayerMovement.instance.hc.invincible = true;
        PlayerMovement.instance.goldenLightAnimator.SetBool("GoldenLight", true);
        PlayerMovement.instance.controller.enabled = false;

        if (coop)
        {
            GLNetworkWrapper.instance.NextLevelOnServer();
        }
        else
        {
            StartCoroutine(NextLevel(hubButton));
        }
    }

    void Update()
    {
        if (hubButton == false && Input.GetKey("g") && Input.GetKey("z"))
        {
            if (Input.GetKeyDown("n"))
                Pressed();
        }
    }

    public void NextLevelOnServer()
    {
        if (nextLevelcoroutine == null)
            nextLevelcoroutine = StartCoroutine(NextLevel(false));
    }
    
    IEnumerator NextLevel(bool hubButton)
    {
        if (!hubButton)
        {
            if (GutProgressionManager.instance.GetChaseScene())
                PlayerAudioController.instance.EndChase();
        }
        yield return new WaitForSeconds(2);
            
        
        if (!hubButton)
        {
            GutProgressionManager.instance.PlayerFinishLevel();  
        }
        else if (goTofloor >= 1)
        {
            if (GameManager.instance.demo)
            {
                goTofloor = 1;
                setCurrentDifficultyLevel = 0;
            }

            var gpm = GutProgressionManager.instance;
            var il = ItemsList.instance;
            
            if (GameManager.instance.difficultyLevel == GameManager.GameMode.StickyMeat)
            {
                switch (goTofloor)
                {
                    case 1: // office
                        if (!il.foundHerPiecesOnFloors.Contains(3)) // player didnt finish the biome
                        {
                            if (gpm.checkpointsOnFloors.Contains(3))
                                goTofloor = 3;
                            else if (gpm.checkpointsOnFloors.Contains(2))
                                goTofloor = 2;
                        }
                        break;
                
                    case 4: // factory
                        if (!il.foundHerPiecesOnFloors.Contains(6)) // player didnt finish the biome
                        {
                            if (gpm.checkpointsOnFloors.Contains(6))
                                goTofloor = 6;
                            else if (gpm.checkpointsOnFloors.Contains(5))
                                goTofloor = 5;
                        }
                        break;
                    
                    case 7: // cages. no button exit
                        if (!il.foundHerPiecesOnFloors.Contains(9)) // player didnt finish the biome
                        {
                            if (gpm.checkpointsOnFloors.Contains(9))
                                goTofloor = 9;
                            else if (gpm.checkpointsOnFloors.Contains(8))
                                goTofloor = 8;
                        }
                        break;
                    
                    case 10: // backstreets
                        if (!il.foundHerPiecesOnFloors.Contains(12)) // player didnt finish the biome
                        {
                            if (gpm.checkpointsOnFloors.Contains(12))
                                goTofloor = 12;
                            else if (gpm.checkpointsOnFloors.Contains(11))
                                goTofloor = 11;
                        }
                        break;
                    
                    case 13: // cabins
                        if (!il.foundHerPiecesOnFloors.Contains(15)) // player didnt finish the biome
                        {
                            if (gpm.checkpointsOnFloors.Contains(15))
                                goTofloor = 15;
                            else if (gpm.checkpointsOnFloors.Contains(14))
                                goTofloor = 14;
                        }
                        break;
                    
                    case 16: // curtains
                        if (!il.foundHerPiecesOnFloors.Contains(18)) // player didnt finish the biome
                        {
                            if (gpm.checkpointsOnFloors.Contains(18))
                                goTofloor = 18;
                            else if (gpm.checkpointsOnFloors.Contains(17))
                                goTofloor = 17;
                        }
                        break;
                }   
            }
            
            gpm.SetLevel(goTofloor, setCurrentDifficultyLevel);
        }
        ItemsList.instance.PlayerFinishedLevel();


        if (GLNetworkWrapper.instance && GLNetworkWrapper.instance.coopIsActive)
        {
            // FLOOR COMPLETED
            // RESTART THE SCENE WITH A NEW FLOOR

            print("ELEVATOR BUTTON PRESSED ON CLIENT PLAYER");
            GLNetworkWrapper.instance.LevelCompleted();
        }
        else
            GameManager.instance.NextLevel();
    }
    
}
