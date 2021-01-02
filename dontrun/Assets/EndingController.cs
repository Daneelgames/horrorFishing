using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class EndingController : MonoBehaviour
{
    public Light mainLight;
    public Color finalLevelColor;
    public Color finalLevelColorAmbientLight;
    public Color darknessPlayerColor;
    private Color defaultFogColor;
    private Color defaultLevelColor;
    private Color defaultLevelColorAmbientLight;
    private Color playerLightDefaultColor;
    private Color defaultCameraColor;
    
    public static EndingController instance;
    public GameObject endingLevelLayout;
    
    public List<HealthController> mobsInTheDarkness = new List<HealthController>();

    private Coroutine darknessCoroutine;
    public bool darknessActive = false;
    private bool infiniteDarkness = false;
    
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        /*
        defaultFogColor = RenderSettings.fogColor;
        defaultLevelColor = mainLight.color;
        defaultLevelColorAmbientLight = RenderSettings.ambientLight;
        */
        var gm = GameManager.instance;
        if (gm.darkness == 1)
        {
            if (darknessCoroutine == null)
                StartCoroutine(StartDarkness());
            //LoadIntoDarkness();
        }    
        else if (gm.hubVisits > 2)
        {
            var r = Random.value;
            int floor = GutProgressionManager.instance.playerFloor;
            
            /*
            if (floor >= 1  && floor <= 3 && r < 0.05f)
                StartDarknessBeforeEnding();
            else if (floor >= 4 && floor <= 6 && r < 0.1)
                StartDarknessBeforeEnding();
            else if (floor >= 10 && floor <= 12 && r < 0.1)
                StartDarknessBeforeEnding();
            else if (floor >= 13 && floor <= 15 && r < 0.1)
                StartDarknessBeforeEnding();
                */
        }
    }

    public void InfiniteDarkness()
    {
        infiniteDarkness = true;
        StartCoroutine(StartDarkness());
    }

    public void StartEndLevel()
    {
        return;
        
        GameManager.instance.darkness = 1;
        if (darknessCoroutine == null)
            StartCoroutine(StartDarkness());
    }

    public void StartDarknessBeforeEnding()
    {
        if (darknessCoroutine == null)
            StartCoroutine(StartDarkness());
        StartCoroutine(StopDarkness());
    }


    IEnumerator StartDarkness()
    {
        if (darknessActive == false)
        {
            darknessActive = true;
            Camera cam = PlayerMovement.instance.mouseLook.mainCamera;
            float t = 0;
            float time = 5;

            // save colors
            defaultFogColor = RenderSettings.fogColor;
            defaultLevelColor = mainLight.color;
            defaultLevelColorAmbientLight = RenderSettings.ambientLight;
            playerLightDefaultColor = MouseLook.instance.playerLight.color;
            defaultCameraColor = cam.backgroundColor;

            ElevatorController.instance.Deactivate();


            MouseLook.instance.playerLight.color = darknessPlayerColor;

            ToggleEnvironment(true);

            while (t < time)
            {
                RenderSettings.fogEndDistance = Mathf.Lerp(150, 30, t / time);
                cam.backgroundColor = Color.Lerp(defaultCameraColor, Color.black, t / time);
                mainLight.color = Color.Lerp(defaultLevelColor, finalLevelColor, t / time);
                RenderSettings.ambientLight =
                    Color.Lerp(defaultLevelColorAmbientLight, finalLevelColorAmbientLight, t / time);
                RenderSettings.fogColor = Color.Lerp(defaultFogColor, Color.black, t / time);
                //RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, 40f, t / time);
                //cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 30, t / time);
                t += Time.deltaTime;
                yield return null;
            }

            
            for (int i = mobsInTheDarkness.Count - 1; i >= 0; i--)
            {
                if (mobsInTheDarkness[i] != null)
                    mobsInTheDarkness[i].gameObject.SetActive(true);
            }
            HubTimeLimiter.instance.StartDarkness();
        }
        else
        {
            yield return null;
        }
    }

    void ToggleEnvironment(bool active)
    {
        endingLevelLayout.SetActive(active);
    }

    IEnumerator StopDarkness()
    {
        var gm = GameManager.instance;
        yield return new WaitForSeconds(Random.Range(gm.hubVisits, Mathf.Clamp(gm.hubVisits * gm.overallDeaths, 60, 180)));
        if (!infiniteDarkness)
        {
            StopDarknessInstantly();
            for (int i = mobsInTheDarkness.Count - 1; i >= 0; i--)
            {
                if (mobsInTheDarkness[i] != null)
                    mobsInTheDarkness[i].gameObject.SetActive(false);
                //mobsInTheDarkness[i].Damage(mobsInTheDarkness[i].healthMax, mobsInTheDarkness[i].transform.position, mobsInTheDarkness[i].transform.position,null, null, false, null, null, null, true);
            }   
        }
    }

    public void StopDarknessInstantly()
    {
        if (!infiniteDarkness)
        {
            ToggleEnvironment(false);
            mobsInTheDarkness.Clear();
            //RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogEndDistance = 150f;
            RenderSettings.fogColor = defaultFogColor;
            mainLight.color = defaultLevelColor;
            ElevatorController.instance.Activate();
        
            var cam = MouseLook.instance.mainCamera;
            cam.backgroundColor =  Color.white;
        
            MouseLook.instance.playerLight.color = playerLightDefaultColor;
            darknessActive = false;   
        }
    }
}
