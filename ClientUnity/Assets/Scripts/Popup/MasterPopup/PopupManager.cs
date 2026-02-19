using UnityEngine;

public class PopupManager : MonoBehaviour
{

    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private Transform canvasTransform;

    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = false;
    //[SerializeField] private Vector3 spawnPosition = Vector3.zero;

    private GameObject activePopup;
    [SerializeField] private bool useScreenSpace = false;

    public void OpenPopup()
    {
        if (activePopup != null) return;
        activePopup = Instantiate(popupPrefab, canvasTransform);

        RectTransform rect = activePopup.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.pivot = new Vector2(1f, 1f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }

    public GameObject ShowPopup(Character character, Vector3 worldPosition)
    {
        //if (popupPrefab == null/* || character == null*/)
        //    return null;

        //activePopup = Instantiate(popupPrefab, worldPosition, Quaternion.identity);

        //UserPagePopup popupComponent = activePopup.GetComponent<UserPagePopup>();
        //if (popupComponent != null)
        //{
        //    popupComponent.Initialize(character);
        //}

        //return activePopup;
        return ShowPopupInternal(character, worldPosition);
    }

    public GameObject ShowPopup(Monster monster, Vector3 worldPosition)
    {
        return ShowPopupInternal(monster, worldPosition);
    }

    public GameObject ShowPopup(Obstacle obstacle, Vector3 worldPosition)
    {
        return ShowPopupInternal(obstacle, worldPosition);
    }

    // Show popup for any WorldEntity at world position
    public GameObject ShowPopup(WorldEntity entity, Vector3 worldPosition)
    {
        return ShowPopupInternal(entity, worldPosition);
    }

    private GameObject ShowPopupInternal(WorldEntity entity, Vector3 position, bool isScreenPosition = false)
    {
        if (popupPrefab == null || entity == null)
            return null;

        // Destroy any existing popup before creating a new one
        if (activePopup != null)
        {
            Destroy(activePopup);
        }


        // Instantiate in world space
        activePopup = Instantiate(popupPrefab, position, Quaternion.identity);


        // Initialize the popup with the entity
        EntityPagePopup popupComponent = activePopup.GetComponent<EntityPagePopup>();
        if (popupComponent != null)
        {
            popupComponent.Initialize(entity);
        }

        return activePopup;
    }
    public void DestroyPopup()
    {
        if (activePopup == null) return;
        Destroy(activePopup);
    }
}