using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RearWindowGameManager : MonoBehaviour
{
    public static RearWindowGameManager instance;

    public RearWindowPlayerController pc;
    public List<CameraZoneController> cameraZoneControllers;
    public TextController tc;

    public List<InteractiveObjectController> interactiveObjectControllers;
    public CameraZoneController activeCameraZone;

    public List<GameObject> objectsTurnToCamera = new List<GameObject>();
    public Image transitionImage;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
            instance = this;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public IEnumerator Teleport(string sceneName, string spawnerName)
    {
        // CLEAR LINKS
        pc.transform.SetParent(transform);
        cameraZoneControllers.Clear();
        interactiveObjectControllers.Clear();
        objectsTurnToCamera.Clear();
        pc.ParentZones(pc.transform, false);

        // ANIMATE
        float i = 0;
        float newX = 0;
        float newY = 2200;
        while (i < 1)
        {
            newX += Time.deltaTime * 2000f;
            transitionImage.rectTransform.sizeDelta = new Vector2(newX, newY);
            i += Time.deltaTime;
            yield return null;
        }
        // LOAD SCENE
        SceneManager.LoadSceneAsync(sceneName);
        // CONTINUE ANIMATION
        StartCoroutine(FinishTeleport(spawnerName));
    }

    public IEnumerator FinishTeleport(string spawnerName)
    {
        float i = 0;
        float newX = transitionImage.rectTransform.sizeDelta.x;
        float newY = 2200;
        while (i < 1)
        {
            newX += Time.deltaTime * 2000f;
            transitionImage.rectTransform.sizeDelta = new Vector2(newX, newY);
            i += Time.deltaTime;
            yield return null;
        }
        // MOVE PLAYER
        pc.transform.SetParent(null);
        GameObject spawner = GameObject.Find(spawnerName);
        pc.transform.position = spawner.transform.position;
        pc.transform.rotation = spawner.transform.rotation;
        pc.ParentZones(null, true);
        // FINISH ANIMATION
         i = 0;
        while (i < 1)
        {
            newX += Time.deltaTime * 2000f;
            newY -= Time.deltaTime * 2200f;
            transitionImage.rectTransform.sizeDelta = new Vector2(newX, newY);
            i += Time.deltaTime;
            yield return null;
        }
        pc.canMove = true;
    }
}