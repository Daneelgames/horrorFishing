using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteTextGenerator : MonoBehaviour
{
    public enum NoteType
    {
        ItemInfo, FixedText, RandomPoem, CassetteTape
    }

    public NoteType type = NoteType.ItemInfo;
    public List<string> fixedText = new List<string>(); 
    public TextMeshProUGUI textField;
    public PoemData poemDataRandomizer;
    public List<TextMeshProUGUI> poemLineFields = new List<TextMeshProUGUI>();
    public SpellScroll spellScroll;
    
    private GameManager gm;
    private ItemsList il;
    private UiManager ui;
    
    
    private int toolIndex = -1;
    
    void Start()
    {
        gm = GameManager.instance;
        il = ItemsList.instance;
        ui = UiManager.instance;
        
        if (type == NoteType.ItemInfo)
            GenerateItemInfo();
        else if (type == NoteType.FixedText)
            GenerateFixedText();
        else if (type == NoteType.RandomPoem)
            GeneratePoem();
        
    }

    void GeneratePoem()
    {
        var linesTemp = new List<Lines>(poemDataRandomizer.lines);
        
        Lines firstLines = linesTemp[Random.Range(0, linesTemp.Count)];
        linesTemp.Remove(firstLines);
        Lines secondLines = linesTemp[Random.Range(0, linesTemp.Count)];

        if (gm.language == 1) // rus
        {
            SetPoemText(firstLines.rusLines, secondLines.rusLines, firstLines, secondLines);
        }
        else if (gm.language == 2) // esp
        {
            SetPoemText(firstLines.espLines, secondLines.espLines, firstLines, secondLines);
        }
        else if (gm.language == 3) // ger
        {
            SetPoemText(firstLines.gerLines, secondLines.gerLines, firstLines, secondLines);
        }
        else // eng. add languages before that if needed
        {
            SetPoemText(firstLines.engLines, secondLines.engLines, firstLines, secondLines);
        }
    }

    void SetPoemText(List<string> _firstlines, List<string> _secondlines, Lines firstLines, Lines secondLines)
    {
        int tempLineIndex = Random.Range(0, _firstlines.Count);
        poemLineFields[0].text = _firstlines[tempLineIndex];
        //firstLines.rusLines.RemoveAt(tempLineIndex);

        // find spellwords here in english
        if (spellScroll)
            spellScroll.FindKeywordsInText(firstLines.engLines[tempLineIndex]);
        
        if (poemLineFields.Count > 1)
        {
            tempLineIndex = Random.Range(0, _secondlines.Count);
            poemLineFields[1].text = _secondlines[tempLineIndex];
            //secondLines.rusLines.RemoveAt(tempLineIndex);
            
            // find spellwords here in english
            if (spellScroll)
                spellScroll.FindKeywordsInText(secondLines.engLines[tempLineIndex]);
        }

        if (poemLineFields.Count > 2)
        {
            tempLineIndex = Random.Range(0, _firstlines.Count);
            poemLineFields[2].text = _firstlines[tempLineIndex];
            //firstLines.rusLines.RemoveAt(tempLineIndex);
            
            // find spellwords here in english
            if (spellScroll)
                spellScroll.FindKeywordsInText(firstLines.engLines[tempLineIndex]);
        }

        if (poemLineFields.Count > 3)
        {
            tempLineIndex = Random.Range(0, _secondlines.Count);
            poemLineFields[3].text = _secondlines[tempLineIndex];
            
            // find spellwords here in english
            if (spellScroll)
                spellScroll.FindKeywordsInText(secondLines.engLines[tempLineIndex]);
        }
        if (poemLineFields.Count > 4)
        {
            tempLineIndex = Random.Range(0, _firstlines.Count);
            poemLineFields[4].text = _firstlines[tempLineIndex];
            //firstLines.engLines.RemoveAt(tempLineIndex);
            
            // find spellwords here in english
            if (spellScroll)
                spellScroll.FindKeywordsInText(firstLines.engLines[tempLineIndex]);
        }

        if (poemLineFields.Count > 5)
        {
            tempLineIndex = Random.Range(0, _secondlines.Count);
            poemLineFields[5].text = _secondlines[tempLineIndex];
            
            // find spellwords here in english
            if (spellScroll)
                spellScroll.FindKeywordsInText(secondLines.engLines[tempLineIndex]);
        }
        if (poemLineFields.Count > 6)
        {
            tempLineIndex = Random.Range(0, _firstlines.Count);
            poemLineFields[6].text = _firstlines[tempLineIndex];
            //firstLines.engLines.RemoveAt(tempLineIndex);
            
            // find spellwords here in english
            if (spellScroll)
                spellScroll.FindKeywordsInText(firstLines.engLines[tempLineIndex]);
        }

        if (poemLineFields.Count > 7)
        {
            tempLineIndex = Random.Range(0, _secondlines.Count);
            poemLineFields[7].text = _secondlines[tempLineIndex];
            
            // find spellwords here in english
            if (spellScroll)
                spellScroll.FindKeywordsInText(secondLines.engLines[tempLineIndex]);   
        }
    }

    void GenerateItemInfo()
    {
        toolIndex = Random.Range(0, il.savedTools.Count);
        var tool = il.savedTools[toolIndex];
        
        string newString = tool.info[gm.language];
        newString += " " + tool.effectHint[gm.language];
        
        // find spellwords here in english
        if (spellScroll)
        {
            spellScroll.FindKeywordsInText(tool.info[0]);   
            spellScroll.FindKeywordsInText(tool.effectHint[0]);   
        }
        
        textField.text = newString;
    }

    void GenerateFixedText()
    {
        // find spellwords here in english
        if (spellScroll)
            spellScroll.FindKeywordsInText(fixedText[0]);
        
        textField.text = fixedText[gm.language];
    }

    public void SetCassetteTapeName(string n)
    {
        textField.text = n;
    }

    public void Interaction() // player picked this up
    {
        if (type == NoteType.ItemInfo)
        {
            il.savedTools[toolIndex].known = true;
            ui.UpdateTools();   
        }
    }
}