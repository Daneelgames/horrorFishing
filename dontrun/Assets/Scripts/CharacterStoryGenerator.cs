using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterStoryGenerator : MonoBehaviour
{
    [Header("Stats")]
    public string currentName;
    public int currentBirthYear = 1985;
    public int workSince = 2000;
    public List<string> currentEvents;

    [Header ("Data")]
    public int currentYear = 2000;
    public int birthYearMin = 1945;
    public int birthYearMax = 1982;

    public List<string> firstNames;
    public List<string> lastNames;

    public List<string> commonEventsList;
    [Tooltip("0 - 12")]
    public List<string> childEventsList; // 0 - 12
    [Tooltip("12 - 18")]
    public List<string> youngEventsList; // 12 - 18
    [Tooltip("18 - 40")]
    public List<string> adultEventsList; // 18 - 40
    [Tooltip("40 - 55")]
    public List<string> oldEventsList; // 40 - 55

    [Header ("Links")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI birthYearText;
    public TextMeshProUGUI workSinceYear;
    public List<TextMeshProUGUI> eventsYears;
    public List<TextMeshProUGUI> events;

    private void Start()
    {
        GenerateName();
        GenerateBirthYear();
        GenerateWorksSince();

        GenerateEvents();

    }

    void GenerateName()
    {
        currentName = firstNames[Random.Range(0, firstNames.Count)] + " " + lastNames[Random.Range(0, lastNames.Count)];
        nameText.text = currentName;
    }

    void GenerateBirthYear()
    {
        currentBirthYear = Random.Range(birthYearMin, birthYearMax + 1);
        birthYearText.text = currentBirthYear.ToString();
    }

    void GenerateWorksSince()
    {
        workSince = Random.Range(currentBirthYear + 18, currentYear + 1);
    }

    void GenerateEvents()
    {
        int eventsAmount = Random.Range(1, 6);
        int age = 0;

        List<string> common = new List<string>(commonEventsList);
        List<string> child = new List<string>(childEventsList);
        List<string> young = new List<string>(youngEventsList);
        List<string> adult = new List<string>(adultEventsList);
        List<string> old = new List<string>(oldEventsList);

        for (int i = 0; i < eventsYears.Count; i ++)
        {
            age += Random.Range(0, 20);

            eventsYears[i].text = (currentBirthYear + age).ToString();
            if (Random.value > 0.75f)
            {
                int r = Random.Range(0, common.Count);

                events[i].text = commonEventsList[r];
                common.RemoveAt(r);
            }
            else
            {
                if (age <= 12)
                {
                    int r = Random.Range(0, child.Count);

                    events[i].text = child[r];
                    child.RemoveAt(r);
                }
                if (age > 12 && age <= 18)
                {
                    int r = Random.Range(0, young.Count);

                    events[i].text = young[r];
                    young.RemoveAt(r);
                }
                if (age > 18 && age <= 40)
                {
                    int r = Random.Range(0, adult.Count);

                    events[i].text = adult[r];
                    adult.RemoveAt(r);
                }
                else
                {
                    int r = Random.Range(0, old.Count);

                    events[i].text = old[r];
                    old.RemoveAt(r);
                }
            }

            if (age > 55 || age + currentBirthYear > workSince)
            {
                eventsYears[i].text = "";
                events[i].text = "";
            }
        }
    }
}