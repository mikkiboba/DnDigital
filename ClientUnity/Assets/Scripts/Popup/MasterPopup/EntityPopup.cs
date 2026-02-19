using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EntityPopup : MonoBehaviour
{

    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text strName;


    public void Init(Sprite newSprite, string newName)
    {
        SetImage(newSprite);
        SetName(newName);
    }


    public void SetImage(Sprite newSprite)
    {
        icon.sprite = newSprite;
    }

    public void SetName(string newName)
    {
        strName.text = newName;
    }

}
