using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextController : MonoBehaviour
{
    public List<TextMeshProUGUI> textObjects;
    public Animator textBackground;
    public Animator letterboxAnim;

    public void SetText(string newText, Color color)
    {
        textObjects[0].color = color;
        foreach(TextMeshProUGUI text in textObjects)
        {
            text.text = newText;
        }
    }
}