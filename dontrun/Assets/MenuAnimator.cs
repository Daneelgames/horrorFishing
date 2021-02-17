using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuAnimator : MonoBehaviour
{
    public GameObject roseGo;
    public GameObject uiParent;
    public GameObject uiParentSettings;
    public AudioSource menuSelectAu;
    public AudioSource buttonPressedAu;
    
    private Quaternion roseInitRotation;

    private Coroutine buttonAnimateCoroutine;
    private GameObject lastButtonToAnimate;
    private Vector3 lastButtonToAnimateScale;

    void Awake()
    {
        if (roseGo)
            roseInitRotation = roseGo.transform.rotation;
    }
    
    void OnEnable()
    {
        if (roseGo)
            StartCoroutine(AnimateRose());
    }

    public void PointerOnButton(GameObject button)
    {
        menuSelectAu.Stop();
        menuSelectAu.pitch = Random.Range(0.5f, 1.5f);
        menuSelectAu.Play();
        if (buttonAnimateCoroutine != null)
        {
            StopCoroutine(buttonAnimateCoroutine);
            lastButtonToAnimate.transform.localScale = lastButtonToAnimateScale;
        }

        lastButtonToAnimate = button;
        lastButtonToAnimateScale = lastButtonToAnimate.transform.localScale;
        buttonAnimateCoroutine = StartCoroutine(AnimateButtonOnSelection(button));
    }

    public void ButtonPressed()
    {
        buttonPressedAu.Stop();
        buttonPressedAu.pitch = Random.Range(0.5f, 1.5f);
        buttonPressedAu.Play();
        if (uiParent)
            StartCoroutine(AnimateUiParentOnPress(uiParent));
        if (uiParentSettings)
            StartCoroutine(AnimateUiParentOnPress(uiParentSettings));
    }

    IEnumerator AnimateButtonOnSelection(GameObject button)
    {
        float t = 0;
        float scaleRandomOffset = button.transform.localScale.x / 20f;
        while (t < 0.5f)
        {
            yield return null;
            button.transform.localScale = lastButtonToAnimate.transform.localScale + Vector3.one * Random.Range(-scaleRandomOffset, scaleRandomOffset);
            t += Time.unscaledDeltaTime;
        }
        
        button.transform.localScale = lastButtonToAnimate.transform.localScale;
    }
    IEnumerator AnimateUiParentOnPress(GameObject uiparent)
    {
        Vector3 initScale = Vector3.one;
        float t = 0;
        while (t < 0.25f)
        {
            yield return null;
            uiparent.transform.localScale = Vector3.one + Vector3.one * Random.Range(-0.05f, 0.05f);
            t += Time.unscaledDeltaTime;
        }
        
        uiparent.transform.localScale = initScale;
    }

    IEnumerator AnimateRose()
    {
        float t = 0;
        float tt = 10;
        Quaternion roseStartRotation = new Quaternion();
        Quaternion roseEndRotation = new Quaternion();
        
        while (gameObject.activeInHierarchy)
        {
            t = 0;
            roseStartRotation = roseGo.transform.rotation;
            roseEndRotation.eulerAngles = new Vector3(roseInitRotation.eulerAngles.x - 5, roseInitRotation.eulerAngles.y, roseInitRotation.eulerAngles.z);
            while (t < tt)
            {
                t += Time.deltaTime;
                roseGo.transform.rotation = Quaternion.Slerp(roseStartRotation, roseEndRotation, t / tt);
                yield return null;
            }

            t = 0;
            roseStartRotation = roseGo.transform.rotation;
            roseEndRotation.eulerAngles = new Vector3(roseInitRotation.eulerAngles.x + 5, roseInitRotation.eulerAngles.y, roseInitRotation.eulerAngles.z);
            while (t < tt)
            {
                t += Time.deltaTime;
                roseGo.transform.rotation = Quaternion.Slerp(roseStartRotation, roseEndRotation, t / tt);
                yield return null;
            }
            yield return null;
        }
    }
}
