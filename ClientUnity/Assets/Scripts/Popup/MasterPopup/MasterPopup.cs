using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class MasterPopup : MonoBehaviour
{
    public class Entity
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

    [SerializeField] GameObject draggablePrefab;
    private List<Entity> allEntities = new List<Entity>();

    // Press-and-hold variables
    private Dictionary<GameObject, WorldEntity> buttonEntityMap = new Dictionary<GameObject, WorldEntity>();
    private Dictionary<GameObject, float> buttonPressStartTime = new Dictionary<GameObject, float>();
    private const float HOLD_DURATION = 0.5f; 
    private GameObject currentDraggingButton;


    public void Start()
    {
        string path = "Sprites/Icons/enemyIcon";

        foreach (Monster m in Persist.GetMonsters())
        {
            allEntities.Add(new Entity(path, m.Name));
        }


        path = "Sprites/Icons/obstacleIcon";
        foreach (Obstacle o in Persist.GetObstacles())
        {
            allEntities.Add(new Entity(path, o.Name));
        }

        GenerateList();

    }

    public void GenerateList()
    {
        List<WorldEntity> entityList = Persist.GetMonsters().Cast<WorldEntity>()
                                .Concat(Persist.GetObstacles().Cast<WorldEntity>()).ToList();

        for (int i = 0; i < entityList.Count; i++)
        {
            EntityPopup newEntity = Instantiate(entityPrefab, canvas);
            newEntity.Init(newSprite: allEntities[i].icon, newName: allEntities[i].name);
            buttonEntityMap[newEntity.gameObject] = entityList[i];
            AddPressAndHoldEvents(newEntity.gameObject);
        }    
    }


    private void AddPressAndHoldEvents(GameObject button)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.AddComponent<EventTrigger>();
        }

        // Pointer Down - Start tracking hold
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { OnButtonPointerDown(button, data); });
        trigger.triggers.Add(pointerDownEntry);

        // Pointer Up - Cancel hold
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { release(button, data); });
        trigger.triggers.Add(pointerUpEntry);

        // Pointer Exit - Cancel hold if mouse leaves button
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => { release(button, data); });
        trigger.triggers.Add(pointerExitEntry);
    }

    void OnButtonPointerDown(GameObject button, BaseEventData data)
    {
        buttonPressStartTime[button] = Time.time;
        currentDraggingButton = button;

        StartCoroutine(CheckForHoldCompletion(button));

        //Debug.Log("Button pressed down - starting hold timer");
    }

    void release(GameObject button, BaseEventData data) // your inibithions feel the rain on your skin
    {

        ResetButtonVisual(button);

        buttonPressStartTime.Remove(button);
        currentDraggingButton = null;
    }

    private IEnumerator CheckForHoldCompletion(GameObject button)
    {
        float startTime = Time.time;

        // Show initial visual feedback
        UpdateButtonVisual(button, 0f);

        while (buttonPressStartTime.ContainsKey(button) &&
               Time.time - startTime < HOLD_DURATION)
        {
            float progress = (Time.time - startTime) / HOLD_DURATION;

            UpdateButtonVisual(button, progress);
            yield return null;
        }

        if (buttonPressStartTime.ContainsKey(button))
        {
            // Complete the visual
            UpdateButtonVisual(button, 1f);
            yield return new WaitForSeconds(0.05f); 

            CreateAndStartDragging(button);
            buttonPressStartTime.Remove(button);

            ResetButtonVisual(button);
        }
        else
        {
            // Hold was cancelled
            ResetButtonVisual(button);
        }
    }

    private void UpdateButtonVisual(GameObject button, float progress) // slightly changes the appearence when holding the button
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            //  blueish
            buttonImage.color = Color.Lerp(Color.white, new Color(0.7f, 0.8f, 1f, 1f), progress);
        }

    }

    private void ResetButtonVisual(GameObject button)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = Color.white;
        }

        button.transform.localScale = Vector3.one;
    }

    private void CreateAndStartDragging(GameObject button)
    {
        if (!buttonEntityMap.ContainsKey(button))
            return;

        WorldEntity e = buttonEntityMap[button];

        if (draggablePrefab == null)
        {
            //Debug.LogError("draggablePrefab is not assigned!");
            return;
        }

        // Get current mouse position in world space
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 200f; // Distance from camera

        // Convert screen position to world position
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Create the draggable object
        GameObject draggableObj = Instantiate(draggablePrefab, worldPos, Quaternion.identity);

        // Set position (ensure it's visible)
        draggableObj.transform.position = new Vector3(worldPos.x, worldPos.y, 200f);

        // Get the DraggableAsset component
        DraggableAsset draggable = draggableObj.GetComponent<DraggableAsset>();

        if (draggable != null)
        {
            string spriteName = e switch
            {
                Monster => "enemyIcon",
                Obstacle => "obstacleIcon",
                _ => "enemyIcon"
            };

            draggable.gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Sprites/Icons/{spriteName}");
            draggable.gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            // Initialize with monster data
            draggable.AssignedEntity = e;
            StartCoroutine(StartDraggingNextFrame(draggable));

            // Add to manager
            MapManager.Instance.AddDraggableAsset(draggable);
            
            //Debug.Log($"Created draggable for {e.Name} and started dragging");
        }
        else
        {
            //Debug.LogError("Failed to get DraggableAsset component!");
            Destroy(draggableObj);
        }
    }

    //sta roba è sus sono due coroutine innestate ma va bene così
    private IEnumerator StartDraggingNextFrame(DraggableAsset draggable)
    {
        // Wait one frame for everything to initialize
        yield return null;

        if (draggable.GetComponent<Collider2D>() != null)
        {
            // Set it as the currently dragged object
            draggable.gameObject.transform.position = GetCurrentMouseWorldPos();

            // Manually trigger OnMouseDown by sending a message (if collider exists)
            draggable.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
        }

        // * ---- nuovo codice
        if (canvas != null)
            canvas.gameObject.SetActive(false);
        // * ----

        StartCoroutine(UpdateDraggablePosition(draggable));
    }

    private IEnumerator UpdateDraggablePosition(DraggableAsset draggable)
    {
        // Keep updating position until mouse is released
        while (Input.GetMouseButton(0))
        {
            Vector3 mousePos = GetCurrentMouseWorldPos();
            draggable.transform.position = mousePos;
            yield return null;
        }


        // * ---- nuovo codice
        if (canvas != null)
            canvas.gameObject.SetActive(true);
        // * ----

        // Mouse released - trigger the drop
        draggable.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver);
    }

    private Vector3 GetCurrentMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 200f;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 200f; // le z sono a cazzo di cane ma per ora basta che sono davanti alla griglia ops
        return worldPos;
    }

    void OnClickButton(GameObject button)
    {
        if (buttonEntityMap.ContainsKey(button))
        {
            WorldEntity e = buttonEntityMap[button];
        }
    }

}
