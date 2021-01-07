using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpellScroll : MonoBehaviour
{
    List<TextMeshProUGUI> lines = new List<TextMeshProUGUI>();
    TextMeshProUGUI line;
    public List<int> keywordsFound = new List<int>();
    float readTime = 0;
    public int initialLettersCount = 0;

    private Coroutine removingLettersCoroutine;

    private ScrollSpellsData ssd;

    void Start()
    {
        ssd = ScrollSpellsData.instance;
        for (int i = 0; i < ssd.spellCodeList.Count; i++)
        {
            ssd.spellCodeList[i] = ssd.spellCodeList[i].ToLower();
        }
    }
    
    public void GetLines(List<TextMeshProUGUI> _lines, TextMeshProUGUI _line)
    {    
        ssd = ScrollSpellsData.instance;
        line = _line;
        lines = _lines;

        // extend the reading time
        // might change this later
        if (line)
        {
            for (int i = 0; i < line.text.Length; i++)
            {
                readTime += 0.05f;
                initialLettersCount++;
            }   
        }
        
        for (int i = 0; i < lines.Count; i++)
        {
            readTime += 1.5f;
            for (int j = 0; j < lines[i].text.Length; j++)
            {
                initialLettersCount++;
            }
        }
    }
    
    public void FindKeywordsInText(string inputText)
    {
        ssd = ScrollSpellsData.instance;
        inputText = inputText.ToLower(); // make the string lower case so we can search for the keywords case-insensitive.
        for (int i = 0; i < ssd.spellCodeList.Count; i++)
        {
            if (inputText.IndexOf(ssd.spellCodeList[i]) >= 0)
            {
                keywordsFound.Add(i);
                // keyword found
            }
        }
    }

    // call this when player picks up the note
    // and stop it when the note is dropped
    public void ToggleSpellInHands(bool pickUp)
    {
        if (keywordsFound.Count > 0)
        {
            if (pickUp && removingLettersCoroutine == null)
                removingLettersCoroutine = StartCoroutine(RemoveLetters());
            else if (!pickUp && removingLettersCoroutine != null)
                StopCoroutine(removingLettersCoroutine);   
        }
    }

    public IEnumerator RemoveLetters()
    {
        if (line || lines.Count > 0)
        {
            if (!line)
                line = lines[0];
            
            while (lines.Count > 0)
            {
                yield return new WaitForSeconds(readTime / initialLettersCount);
                bool textClear = true;
            
                if (textClear)
                    for (int i = lines.Count - 1; i >= 0; i--)
                    {
                        if (lines[i].text == "")
                        {
                            print("remove line at " + i);
                            lines.RemoveAt(i);
                        }
                        else
                        {
                            textClear = false;
                            break;
                        }
                    }

                if (!textClear)
                {
                    int lr = Random.Range(0, lines.Count);
                    string letter = lines[lr].text[Random.Range(0, lines[lr].text.Length)].ToString();
                    lines[lr].text = lines[lr].text.Replace(letter, "");
                    if (lines[lr].text == "")
                        lines.RemoveAt(lr);
                    
                    //lines[lr].text.Remove(Random.Range(0, lines[lr].text.Length));
                    print("remove letter from line " + lr);
                }
            }

            if (line)
            {
                while (line.text.Length > 0)
                {
                    yield return new WaitForSeconds(readTime / initialLettersCount);
                    print("remove letter from main line");
                    //line.text.Remove(Random.Range(0, line.text.Length));
                    line.text = line.text.Replace(line.text[Random.Range(0, line.text.Length)].ToString(),"");
                }   
            }
            SetScrollEffect();
        }
        else
        {
            yield return null;
        }
    }

    public void SetScrollResult(string result)
    {
        if (line)
            line.text = result;
        else if (lines[0])
        {
            lines[0].text = result;
        }
    }

    // call this when all the letters are gone
    public void SetScrollEffect()
    {
        int randomSpell = Random.Range(0, keywordsFound.Count);
        ssd.ActivateScrollEffect(this, randomSpell);
    }
}

