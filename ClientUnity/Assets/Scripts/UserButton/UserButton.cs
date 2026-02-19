using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UserButton : MonoBehaviour
{
    [Header("Child References")]
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Image classImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private Button buttonComponent;

    public void Configure(string text, string dndClass, Color buttonColor, Action onClickAction)
    {
        dndClass = dndClass.ToLower();
        if (buttonText != null) buttonText.text = text;

        if (classImage != null)
        {
            string path = "Sprites/Boxes/PlayerBox/";
            switch (dndClass)
            {
                case "bard":
                    classImage.sprite = Resources.Load<Sprite>(path + "ClassImage/Bard");
                    borderImage.sprite = Resources.Load<Sprite>(path + "ClassBorder/Bard");
                    break;
                
                case "monk":
                case "thief":
                    classImage.sprite = Resources.Load<Sprite>(path + "ClassImage/Thief");
                    borderImage.sprite = Resources.Load<Sprite>(path + "ClassBorder/Rogue");
                    break;
                
                case "assassin":
                    classImage.sprite = Resources.Load<Sprite>(path + "ClassImage/Assassin");
                    borderImage.sprite = Resources.Load<Sprite>(path + "ClassBorder/Rogue");
                    break;
                
                case "ranger":
                    classImage.sprite = Resources.Load<Sprite>(path + "ClassImage/Ranger");
                    borderImage.sprite = Resources.Load<Sprite>(path + "ClassBorder/Ranger");
                    break;

                case "wizard":
                case "mage":
                    classImage.sprite = Resources.Load<Sprite>(path + "ClassImage/Mage");
                    borderImage.sprite = Resources.Load<Sprite>(path + "ClassBorder/Sorcerer");
                    break;

                case "warrior":
                    classImage.sprite = Resources.Load<Sprite>(path + "ClassImage/Warrior");
                    borderImage.sprite = Resources.Load<Sprite>(path + "ClassBorder/Fighter");
                    break;
            }
        }

        if (buttonComponent != null) 
        {
            buttonComponent.image.color = buttonColor;
        }
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(() => onClickAction.Invoke());
    }
}
