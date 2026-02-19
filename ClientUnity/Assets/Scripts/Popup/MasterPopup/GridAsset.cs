using UnityEngine;

public class GridAsset : MonoBehaviour
{
    #region Fields

    [Header("Assigned Entities")]
    private WorldEntity assignedEntity;
    public WorldEntity AssignedEntity
    {
        get => assignedEntity;
        set
        {
            assignedEntity = value;
            UpdateColor();
        }
    }

    protected SpriteRenderer spriteRenderer;

    public int GridX { get; set; } = -1;
    public int GridY { get; set; } = -1;

    public int currPF;

    #endregion

    #region Grid Settings

    public static int Rows { get; set; }
    public static int Cols { get; set; }

    #endregion

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    #endregion

    #region Grid Placement

    protected bool IsInsideGrid(int row, int col)
    {
        return row >= 0 && row < Rows && col >= 0 && col < Cols;
    }

    public void ProcessDropOnCell(bool masterMode)
    {
        int cellIndex = GridY * Cols + GridX;

        if (cellIndex < 0 || cellIndex >= MapManager.Instance.Cells.Count)
        {
            ResetAsset();
            return;
        }

        Cell targetCell = MapManager.Instance.Cells[cellIndex];

        if (targetCell == null || !targetCell.CanAcceptDrop)
        {
            ResetAsset();
            return;
        }

        int objectType = GetObjectType();

        if (objectType == 0)
        {
            ResetAsset();
            return;
        }

        targetCell.UpdateValue(objectType);
        GridX = targetCell.X;
        GridY = targetCell.Y;

        targetCell.AddAsset(spriteRenderer);

        OnSuccessfullyPlaced();
        if (masterMode) { UpdateDatabase(); }
        ResetAsset();
    }

    #endregion

    #region Database Operations

    protected async void UpdateDatabase()
    {
        if (GetObjectType() == 1)
        {
            if (await Requester.ObstacleFound(GridY, GridX))
            {
                Requester.DeleteObstacle(GridY, GridX);
            }
            if (await Requester.MonsterFound(GridY, GridX))
            {
                Requester.DeleteMonster(GridY, GridX);
            }
            Requester.AddMonster(AssignedEntity.Name, AssignedEntity.Current_Pf, GridY, GridX);
            Debug.Log("aggiornamento riuscito");
        }
        else if (GetObjectType() == 2)
        {
            if (await Requester.ObstacleFound(GridY, GridX))
            {
                Requester.DeleteObstacle(GridY, GridX);
            }
            if (await Requester.MonsterFound(GridY, GridX))
            {
                Requester.DeleteMonster(GridY, GridX);
            }
            Requester.AddObstacle(AssignedEntity.Name, AssignedEntity.Current_Pf, GridY, GridX);
            Debug.Log("aggiornamento riuscito");
        }
    }

    #endregion

    #region Helpers

    protected int GetObjectType()
    {
        if (AssignedEntity is Monster) return 1;
        if (AssignedEntity is Character) return 3;
        if (AssignedEntity is Obstacle) return 2;
        return 0;
    }

    protected void UpdateColor()
    {
        if (spriteRenderer == null)
            return;

        if (AssignedEntity is Monster)
            spriteRenderer.color = Color.red;
        else if (AssignedEntity is Character)
            spriteRenderer.color = Color.green;
        else
            spriteRenderer.color = Color.white;
    }

    protected void OnSuccessfullyPlaced()
    {
        Debug.Log($"Placed at ({GridX}, {GridY})");
    }

    protected void ResetAsset()
    {
        Destroy(gameObject);
    }

    public void Initialize(WorldEntity entity, int gridX, int gridY, int pf)
    {
        AssignedEntity = entity;
        GridX = gridX;
        GridY = gridY;
        currPF = pf;

        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        if (entity != null)
        {
            // Try to get sprite from the entity first (if it has one)
            Sprite entitySprite = null;

            if (entity is Monster)
                entitySprite = Resources.Load<Sprite>("Sprites/Icons/enemyIcon");
            else if (entity is Character)
                entitySprite = Resources.Load<Sprite>("Sprites/Icons/characterIcon");
            else if (entity is Obstacle)
                entitySprite = Resources.Load<Sprite>("Sprites/Icons/obstacleIcon");

            //// Fallback to enemyIcon if nothing else worked
            if (entitySprite == null)
            {
                entitySprite = Resources.Load<Sprite>("Sprites/Icons/obstacleIcon");
            }

            //// Assign the sprite
            if (entitySprite != null)
            {
                spriteRenderer.sprite = entitySprite;
            }

            UpdateColor();
        }
    }
    #endregion
}

public static class GridAssetFactory
{
    public static T CreateGridAsset<T>(T prefab, WorldEntity entity, int gridX, int gridY, int pf, Transform parent = null) where T : GridAsset
    {
        if (prefab == null)
        {
            Debug.LogError("GridAsset prefab is null!");
            return null;
        }

        T asset = GameObject.Instantiate(prefab, parent);

        asset.Initialize(entity, gridX, gridY, pf);

        return asset;
    }

   
    public static GridAsset CreateGridAsset<T>(WorldEntity entity, int gridX, int gridY, int pf) where T : GridAsset
    {
        GameObject go = new GameObject($"{entity.Name}_GridAsset");
        T asset = go.AddComponent<T>();
        asset.Initialize(entity, gridX, gridY, pf);
        return asset;
    }
}