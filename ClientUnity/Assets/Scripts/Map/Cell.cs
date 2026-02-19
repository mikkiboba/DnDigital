using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Cell : MonoBehaviour
{
    //[SerializeField] private GameObject manager;

    [Header("Position of the cell")]
    [SerializeField] private int x;
    public int X => x;
    [SerializeField] private int y;
    public int Y => y;

    [Header("Sprite for the entities")]
    [SerializeField] private Sprite redPlayer;
    [SerializeField] private Sprite greenPlayer;
    [SerializeField] private Sprite bluePlayer;
    [SerializeField] private Sprite orangePlayer;

    [Header("Highlight settings")]
    [SerializeField] [Range(0f, 1f)] private float highlightIntensity = 0.3f;
    private Color originalBackgroundColor;
    private bool isHighlighted = false;
    private Coroutine highlightCoroutine;

    [Header("Content settings")]
    [SerializeField] private Image cellBackground;
    public Image CellBackground
    {
        get => cellBackground;
        set => cellBackground = value;
    }
    [SerializeField] private Image contentRenderer;
    public Image ContentRenderer
    {
        get => contentRenderer;
        set => contentRenderer = value;
    }

    private bool canAcceptDrop = false;
    public bool CanAcceptDrop
    {
        get => canAcceptDrop;
        set => canAcceptDrop = value;
    }
    private int objectType;
    public int ObjectType => objectType;
    private bool isButton = false;
    public bool IsButton => isButton;

    void Start()
    {
        // Store the original background color
        if (cellBackground != null)
        {
            originalBackgroundColor = cellBackground.color;
        }
    }

    public Sprite getColor(int type)
    {
        switch (type)
        {
            case 1: return redPlayer;
            case 2: return greenPlayer;
            case 3: return bluePlayer;
            case 5: return orangePlayer;
            default: return null;
        }
    }


    public void UpdateValue(int newValue)
    {
        objectType = newValue;

        if (newValue != 0) isButton = true;

        if (contentRenderer != null)
        {
            if (newValue == 0)
            {
                contentRenderer.enabled = false;
                contentRenderer.sprite = null;
                isButton = false;
                canAcceptDrop = false;
            }
            else
            {
                contentRenderer.enabled = true;
                contentRenderer.sprite = getColor(newValue);
                isButton = true;
                canAcceptDrop = true;
            }
        }
        else
        {
            Debug.LogError($"ContentRenderer is null on {gameObject.name}. Drag the child SpriteRenderer into this slot in the Inspector!");
        }
    }


    public void Init(int x, int y, int objectType, bool generateBackground = true, Sprite sprite = null)
    {
        this.objectType = objectType;
        this.x = x;
        this.y = y;

        if (generateBackground)
            GenerateSprite();

        if (objectType != 0)
        {
            isButton = true;
            canAcceptDrop = true;

            if (contentRenderer != null)
            {
                contentRenderer.enabled = true;
                if (sprite != null) contentRenderer.sprite = sprite;
                else contentRenderer.sprite = getColor(objectType);
            }
        }
        else
        {
            contentRenderer.enabled = false;
            isButton = false;
        }
    }


    public void AddAsset(SpriteRenderer asset)
    {
        if (contentRenderer != null && isButton && canAcceptDrop && asset != null)
        {
            contentRenderer.enabled = true;
            contentRenderer.sprite = asset.sprite;
            contentRenderer.color = asset.color;
            //isButton = true;
            canAcceptDrop = false;
        }
    }


    private void GenerateSprite()
    {
        System.Random rnd = new System.Random();
        int value = rnd.Next(1, 5);
        string strVal = value.ToString();
    string path = $"Sprites/Cells/{strVal}";
        cellBackground.sprite = Resources.Load<Sprite>(path);
    }


    public void UpdateCoordinates(int newX, int newY)
    {
        x = newX;
        y = newY;
    }

    public void HighlightForOneSecond(Color color)
    {
        if (cellBackground == null) return;

        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
        }

        highlightCoroutine = StartCoroutine(HighlightForSecondsCoroutine(color, 1f));
    }

    private IEnumerator HighlightForSecondsCoroutine(Color color, float duration)
    {
        if (cellBackground == null) yield break;

        Color tintedColor = Color.Lerp(originalBackgroundColor, color, highlightIntensity);
        cellBackground.color = tintedColor;
        isHighlighted = true;

        yield return new WaitForSeconds(duration);

        // Restore original color
        cellBackground.color = originalBackgroundColor;
        isHighlighted = false;
        highlightCoroutine = null;
    }

    public void SetEmpty()
    {
        contentRenderer.enabled = false;
        contentRenderer.sprite = null;
        objectType = 0;
    }

}