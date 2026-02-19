using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EntityPagePopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI classLevelText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthIcon;
    [SerializeField] private TextMeshProUGUI mainStatsText;
    [SerializeField] private TextMeshProUGUI extraStatsText;

    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private bool fadeOut = true;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Type Colors")]
    [SerializeField] private Color characterColor = Color.white;
    [SerializeField] private Color monsterColor = Color.red;
    [SerializeField] private Color obstacleColor = Color.gray;

    private WorldEntity currentEntity;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && fadeOut)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Initialize(WorldEntity entity)
    {
        currentEntity = entity;
        UpdateUI();

        StartCoroutine(AutoDestroyAfterDelay());
    }

    private IEnumerator AutoDestroyAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (fadeOut && canvasGroup != null)
        {
            float elapsedTime = 0;
            float startAlpha = canvasGroup.alpha;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, elapsedTime / fadeDuration);
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    private void UpdateUI()
    {
        if (currentEntity == null) return;

        // Set name
        if (nameText != null)
            nameText.text = currentEntity.Name ?? "Unknown";

        // Set description
        if (descriptionText != null)
            descriptionText.text = "Description";/*string.IsNullOrEmpty(currentEntity.Bio) ? "No description available" : currentEntity.Bio;*/

        // Set health
        if (healthText != null)
            healthText.text = $"{currentEntity.Current_Pf}/{currentEntity.Max_Pf}";

        // Handle different entity types
        if (currentEntity is Character character)
        {
            DisplayCharacter(character);
        }
        else if (currentEntity is Monster monster)
        {
            DisplayMonster(monster);
        }
        else if (currentEntity is Obstacle obstacle)
        {
            DisplayObstacle(obstacle);
        }
        else
        {
            // Fallback for any other WorldEntity
            DisplayGenericEntity(currentEntity);
        }
    }

    private void DisplayCharacter(Character character)
    {
        // Set color theme
        SetThemeColor(characterColor);

        // Class/Level text
        if (classLevelText != null)
            classLevelText.text = $"{character.Class ?? "Character"} | Level {character.Level}";

        // Main stats (ability scores)
        if (mainStatsText != null)
            mainStatsText.text = $"STR: {character.Str}\nDEX: {character.Dex}\nCON: {character.Con}\nINT: {character.Int}\nWIS: {character.Wis}\nCHA: {character.Cha}";

        // Extra stats
        if (extraStatsText != null)
            extraStatsText.text = $"AC: {character.AC}\nInit: {character.Initiative}\nPassive: {character.Pass_Perc}%\n{character.Race ?? "Unknown"}";
    }

    private void DisplayMonster(Monster monster)
    {
        // Set color theme
        SetThemeColor(monsterColor);

        // Type/CR text
        if (classLevelText != null)
            classLevelText.text = $"{monster.Type ?? "Monster"} | CR: {monster.CR?.ToString() ?? "?"}";

        // Main stats (creature features)
        if (mainStatsText != null)
        {
            string actionsInfo = monster.Actions != null && monster.Actions.Count > 0
                ? $"Actions: {monster.Actions.Count}"
                : "No actions";
            mainStatsText.text = actionsInfo;
        }

        // Extra stats
        if (extraStatsText != null)
            extraStatsText.text = $"AC: {monster.AC}\nInit: {monster.Initiative}";
    }

    private void DisplayObstacle(Obstacle obstacle)
    {
        // Set color theme
        SetThemeColor(obstacleColor);

        // Type text
        if (classLevelText != null)
            classLevelText.text = "Obstacle | Environmental";

        // Main stats
        if (mainStatsText != null)
            mainStatsText.text = "No combat stats";

        // Extra stats
        if (extraStatsText != null)
            extraStatsText.text = "Interactive Object";
    }

    private void DisplayGenericEntity(WorldEntity entity)
    {
        // Generic display for any WorldEntity
        if (classLevelText != null)
            classLevelText.text = "Entity";

        if (mainStatsText != null)
            mainStatsText.text = "";

        if (extraStatsText != null)
            extraStatsText.text = "";
    }

    private void SetThemeColor(Color color)
    {
        if (nameText != null)
            nameText.color = color;

        if (healthIcon != null)
            healthIcon.color = color;
    }
}