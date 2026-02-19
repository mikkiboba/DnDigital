using UnityEngine;

public class DraggableAsset : GridAsset
{
    #region Fields

    private Camera mainCamera;
    private Vector3 dragOffset;
    private bool isDragging;
    public bool Dropped { get; private set; }

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    #endregion

    #region Drag Logic

    private void OnMouseDown()
    {
        if (AssignedEntity is Character c && c.Assigned)
            return;

        isDragging = true;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorldPos;
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;
        SnapToGrid();
    }

    #endregion

    #region Grid Placement

    private void SnapToGrid()
    {
        RectTransform grid = MapManager.Instance.GridCanvas;
        float cellSize = MapManager.Instance.CellSize;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            grid,
            mainCamera.WorldToScreenPoint(transform.position),
            mainCamera,
            out Vector2 localPoint))
        {
            ResetAsset();
            return;
        }

        float gridWidth = Cols * cellSize;
        float gridHeight = Rows * cellSize;

        float startX = -(gridWidth - cellSize) / 2f;
        float startY = -(gridHeight - cellSize) / 2f;

        int col = Mathf.RoundToInt((localPoint.x - startX) / cellSize);
        int row = Mathf.RoundToInt((localPoint.y - startY) / cellSize);

        if (!IsInsideGrid(row, col))
        {
            ResetAsset();
            return;
        }

        GridX = col;
        GridY = row;

        transform.SetParent(grid, true);
        transform.localPosition = new Vector3(
            startX + col * cellSize,
            startY + row * cellSize,
            0f
        );

        ProcessDropOnCell(MapManager.Instance.MasterMode);
        Dropped = true;
    }

    #endregion

    #region Helpers

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        pos.z = -8f;
        return pos;
    }

    #endregion
}