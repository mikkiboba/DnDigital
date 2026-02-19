using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class UserPopup : MonoBehaviour
{
    private Character currentPlayer;
    private int color;
    private CreatureEntity.Action actionPerformed;
    public CreatureEntity.Action ActionPerformed
    {
        get => actionPerformed;
        set
        {
            actionPerformed = value;
        }
    }

    public class Entity //bottone
    {
        public Sprite icon;
        public string name;

        public Entity(string iconPath, string name)
        {
            icon = Resources.Load<Sprite>(iconPath);
            this.name = name;
        }
    }

    [SerializeField] private Transform canvas;
    [SerializeField] private EntityPopup entityPrefab;
    private string path = "Sprites/Icons/enemyIcon"; //non so come michele ha creato le sprite in quel formato

    private List<Entity> allEntities = new List<Entity>();

    private async void Start()
    {
        currentPlayer = Persist.Characters.FirstOrDefault(p => p.Name == PlayerPrefs.GetString("name"));
        color = currentPlayer.Color;
        Debug.Log($"Current player set to: {currentPlayer?.Name}");

        if (currentPlayer?.Actions != null)
        {
            foreach (CreatureEntity.Action m in currentPlayer.Actions)
            {
                allEntities.Add(new Entity(path, m.actionName));
            }
            Debug.Log($"Added {allEntities.Count} entities from {currentPlayer.Actions.Count} actions");
        }

        GenerateList();
        //MapManager.Instance.AssignCharacterInGrid(color, currentPlayer);
    }

    public void GenerateList()
    {

        if (currentPlayer == null || currentPlayer.Actions == null)
        {
            Debug.LogError("Can't generate list - no player or actions");
            return;
        }

        for (int i = 0; i < currentPlayer.Actions.Count; i++)
        {
            if (i >= allEntities.Count)
            {
                Debug.LogWarning($"Index {i} out of range for allEntities");
                continue;
            }

            EntityPopup newEntity = Instantiate(entityPrefab, canvas);
            newEntity.Init(newSprite: allEntities[i].icon, newName: allEntities[i].name);


            Button actionButton = newEntity.GetComponent<Button>();
            if (actionButton == null)
            {
                Debug.Log("Adding Button component to EntityPopup");
                actionButton = newEntity.gameObject.AddComponent<Button>();

                Image buttonImage = newEntity.GetComponent<Image>();
                if (buttonImage != null)
                {
                    actionButton.targetGraphic = buttonImage;
                }
            }

            CreatureEntity.Action currentAction = currentPlayer.Actions[i];

            actionButton.onClick.AddListener(() =>
            {
                Debug.Log($"Action button clicked: {currentAction.actionName}");
                OnActionButtonClicked(currentAction);
            });

        }

        Debug.Log($"Created {currentPlayer.Actions.Count} action buttons");
    }

    void OnActionButtonClicked(CreatureEntity.Action action)
    {
        Debug.Log($"Range: {action.range}\n");
        actionPerformed = action;
        DisplayRange(action.range);
    }

    public void DisplayRange(int range)
    {
        if (range != 101) { MapManager.Instance.HighlightArea(color, range); } // 101 voleva dire no range se non sbaglio

    }
}