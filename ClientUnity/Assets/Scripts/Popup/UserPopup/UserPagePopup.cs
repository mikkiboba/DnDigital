using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UserPagePopup : MonoBehaviour
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

    private Character currentCharacter;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && fadeOut)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }


    public void Initialize(Character character)
    {
        currentCharacter = character;
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
        if (currentCharacter == null) return;

        if (nameText != null)
            nameText.text = currentCharacter.Name ?? "Unknown";

        if (classLevelText != null)
            classLevelText.text = $"{currentCharacter.Class ?? "Class"} | Level {currentCharacter.Level}";

        if (descriptionText != null)
            descriptionText.text = "Description";

        if (healthText != null)
            healthText.text = $"{currentCharacter.Current_Pf}/{currentCharacter.Max_Pf}";

        if (mainStatsText != null)
            mainStatsText.text = $"STR: {currentCharacter.Str} \nDEX: {currentCharacter.Dex} \nCON: {currentCharacter.Con}\nINT: {currentCharacter.Int}\nWIS: {currentCharacter.Wis}\nCHA: {currentCharacter.Cha}";

        if (extraStatsText != null)
            extraStatsText.text = $"AC: {currentCharacter.AC}\nInit: {currentCharacter.Initiative}\nPC: {currentCharacter.Pass_Perc}\n{currentCharacter.Race ?? "Unknown"}";
    }

}