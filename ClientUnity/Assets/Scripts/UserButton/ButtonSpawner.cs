using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonSpawner : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private UserButton userButtonPrefab;
    [SerializeField] private Transform contentContainer;


    [Header("Testing data")]
    private Color[] colors = {
        new Color(251f/255f, 77f/255f, 61f/255f),
        new Color(158f/255f, 227f/255f, 125f/255f),
        new Color(170f/255f, 239f/255f, 223f/255f)
    };


    void Start()
    {
        if (Persist.IsLoaded) InitializeCharacterButtons();
        else Persist.OnDataLoaded += InitializeCharacterButtons;
    }


    void InitializeCharacterButtons()
    {
        List<Character> characters = Persist.GetCharacters();

        for (int i = 0; i < Mathf.Min(characters.Count, colors.Length); i++)
        {
            CreateButton(text: characters[i].Name, selectedClass: characters[i].Class, col: colors[i]);
        } 
    }

    public void CreateButton(string text, string selectedClass, Color col)
    {
        UserButton newBtn = Instantiate(userButtonPrefab, contentContainer);

        newBtn.Configure(
            text,
            selectedClass,
            col,
            () => OnButtonClicked(text)
        );
    }


    private void OnButtonClicked(string name)
    {
        PlayerPrefs.SetString("name", name);
        return;
    }
}
