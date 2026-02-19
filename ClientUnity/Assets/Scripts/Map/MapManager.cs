using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;

public class MapManager : MonoBehaviour
{
    // * Singleton
    public static MapManager Instance { get; private set; }
    [SerializeField] private bool _masterMode;
    public bool MasterMode => _masterMode;

    [Header("Map Settings")]
    [SerializeField] private MatrixReceiver _matrixReceiver;
    [SerializeField] private Cell _tilePrefab;
    [SerializeField] private RectTransform _gridCanvas;
    public RectTransform GridCanvas => _gridCanvas;

    [SerializeField] private float _cellSize = 100f;
    public float CellSize => _cellSize;

    [Header("Entity settings")]
    [SerializeField] private GameObject draggablePrefab;
    private List<GridAsset> _masterAssets;
    private List<GridAsset> _provamasterAssets = new List<GridAsset>();
    private Dictionary<Character, Vector2Int> players = new Dictionary<Character, Vector2Int>();
    public Dictionary<Character, Vector2Int> Players
        {
            get => players;
            set => players = value;
        }

    [SerializeField] private PopupManager popupManager;
    

    [Header("Data")]
    private int _rows, _cols;
    private int[] inputMatrix;
    private List<Cell> cells = new List<Cell> { };
    public List<Cell> Cells => cells;


    public void AssignInstanceInGrid(int color, WorldEntity instance, int pf)

    {
       
        for (int i = 0; i < inputMatrix.Length; i++)
        {
            if (inputMatrix[i] == color)
            {
                int row = i / _cols;
                int col = i % _cols;
                GridAsset asset = GridAssetFactory.CreateGridAsset<GridAsset>(
                            instance,
                            row,
                            col,
                            pf
                        );
                AddDraggableAsset(asset);

                asset.ProcessDropOnCell(_masterMode);
            }
        }
    }

    public void processInstances(List<Requester.Instance> instances, int type, bool isFirst) {
        foreach (Requester.Instance instance in instances)
        {
            if (_masterMode && isFirst)
            {
                if (type==0) { Requester.DeleteMonster(instance.x, instance.y); }
                else if (type ==1) { Requester.DeleteObstacle(instance.x, instance.y); }
                Debug.Log($"cancello istanza {instance.type},{instance.x}, {instance.y},{instance.curr_pf}");             
            }
            else
            {
                IEnumerable<WorldEntity> targetList = type switch
                {
                    0 => Persist.Monsters,
                    1 => Persist.Obstacles,
                    _ => Enumerable.Empty<WorldEntity>() // default empty list
                };
                foreach (WorldEntity m in targetList)
                {
                    if (m.Name == instance.type)
                    {
                        AssignInstanceInGrid(5, m, instance.curr_pf);
                        
                    }
                }
            }
        }
        //Debug.Log($" instances = {_masterAssets.Count}");
    }

    public async void UpdateMonstersAndObstaclesInstances(bool isFirst)
    {
        _masterAssets.Clear();
        List<Requester.Instance> monsterInstances = await Requester.GetMonsterInstances();
        foreach (var obs in monsterInstances)
        {
            Debug.Log($"Obstacle instance: type={obs.type}, x={obs.x}, y={obs.y}, pf={obs.curr_pf}");
        }
        processInstances(monsterInstances, 0, isFirst);
        List<Requester.Instance> obstacleInstances = await Requester.GetObstacleInstances();

        foreach (var obs in obstacleInstances)
        {
            Debug.Log($"Obstacle instance: type={obs.type}, x={obs.x}, y={obs.y}, pf={obs.curr_pf}");
        }
        processInstances(obstacleInstances, 1, isFirst);        
    }
    public void AssignAllCharacters()
    {
        List<Character> characters = Persist.Characters;

        for (int i = 0; i < inputMatrix.Length; i++)
        {
            if (inputMatrix[i] != 0 && cells[i].IsButton) //non assegno il player rosso così posso ancora testare il drag and drop
            {
                foreach (Character c in characters)
                {
                    if (c.Color == inputMatrix[i])
                    {
                        int row = i / _cols;
                        int col = i % _cols;

                        players[c] = new Vector2Int(col, row);
                        //Debug.Log($" ASSIGNED {c.Name} in cell ({row},{col})");
                        cells[i].CanAcceptDrop = false;
                    }
                    else
                    {
                        //Debug.Log($" No match for {c.Name}");
                    }
                }
            }
        }
        //Debug.Log($"Final players assigned: {players.Count}");
    }

    public void AssignCharacterInGrid(int color, Character player)
    {
        for (int i = 0; i < inputMatrix.Length; i++)
        {
            if (inputMatrix[i] == color)
            {
                int row = i / _cols;
                int col = i % _cols;

                players[player] = new Vector2Int(col, row);
            }
        }
    }

    public Character FindCharacterAtPosition(int x, int y)
    {
        Vector2Int targetPosition = new Vector2Int(x, y);

        foreach (var kvp in players)
        {
            if (kvp.Value == targetPosition)
            {
                return kvp.Key;
            }
        }
        return null; // No character found at this position
    }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    void Start()
    {
        if (_matrixReceiver != null)
            _matrixReceiver.OnMatrixReady += HandleNewMapData;
        else
            Debug.Log("Matrix Receiver has not been assigned in the inspector.");

        _masterAssets = new List<GridAsset>();
        DraggableAsset.Rows = _rows;
        DraggableAsset.Cols = _cols;
    }


    void Update()
    {
        HandleTouchInputRaycast();
    }


    void OnDestroy()
    {
        if (_matrixReceiver != null)
            _matrixReceiver.OnMatrixReady -= HandleNewMapData;
    }


    void HandleNewMapData(int[] newMatrix, int newRows, int newCols)
    {
        if (newRows != _rows || newCols != _cols)
        {
            _rows = newRows;
            _cols = newCols;
            PrintMatrix(newMatrix);

            DraggableAsset.Rows = _rows;
            DraggableAsset.Cols = _cols;
            inputMatrix = new int[_rows * _cols];

            ClearGrid();
            DrawGrid();
            UpdateGrid(newMatrix);
            AssignAllCharacters();
            UpdateMonstersAndObstaclesInstances(true);
            return;

        }
        UpdateGrid(newMatrix);
        UpdateMonstersAndObstaclesInstances(false);


    }


    void ClearGrid()
    {
        if (cells == null) return;

        foreach (var cell in cells)
            if (cell != null) Destroy(cell.gameObject);

        cells.Clear();
    }


    void DrawGrid()
    {
        Debug.Log("drawwww");
        PrintMatrix(inputMatrix);
        if (_rows <= 0 || _cols <= 0) return;

        // * canvas size
        float gridWidth = _cols * _cellSize;
        float gridHeight = _rows * _cellSize;
        _gridCanvas.sizeDelta = new Vector2(gridWidth, gridHeight);

        Vector3 targetScale = new Vector3(_cellSize, _cellSize, 1f);

        // * start position so the grid is centered
        float startX = -(gridWidth - _cellSize) / 2f;
        float startY = -(gridHeight - _cellSize) / 2f;

        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                int index = row * _cols + col;
                if (index >= inputMatrix.Length) break;

                float posX = startX + (col * _cellSize);
                float posY = startY + (row * _cellSize);

                var spawnedTile = Instantiate(_tilePrefab);
                spawnedTile.transform.SetParent(_gridCanvas, false);

                spawnedTile.transform.localPosition = new Vector3(posX, posY, 0);
                spawnedTile.transform.localScale = targetScale;

                spawnedTile.name = $"Tile ({row}x{col})";
                spawnedTile.Init(row, col, inputMatrix[index]);
                cells.Add(spawnedTile);
            }
        }
    }
    public void UpdateGrid(int[] newMatrix)
    {
        
        List<int> changedIndices = new List<int>();
        List<int> values = new List<int>();
        List<Sprite> sprites = new List<Sprite>();

        for (int i = 0; i < inputMatrix.Length; i++)
        {
            if (inputMatrix[i] != newMatrix[i])
            {
                changedIndices.Add(i);
                values.Add(inputMatrix[i]);
                //Debug.Log($"Something has changed at index {i}, old value {inputMatrix[i]} - new value {newMatrix[i]}");
            }
        }

        if (changedIndices.Count > 2)
        {
            inputMatrix = newMatrix;
            ClearGrid();
            DrawGrid();
            return;
        }



        if (changedIndices.Count == 2)
        {
            if (
                newMatrix[changedIndices[0]] != newMatrix[changedIndices[1]] && 
                newMatrix[changedIndices[0]] != 0 &&
                newMatrix[changedIndices[1]] != 0 &&
                inputMatrix[changedIndices[0]] == 0 &&
                inputMatrix[changedIndices[1]] == 0
            )
            {
                inputMatrix = newMatrix;
                ClearGrid();
                DrawGrid();
                return;
            } 
            else
            {
                swap(changedIndices[0], changedIndices[1]/*, values[0], values[1]*/);
            }
        }

        inputMatrix = (int[])newMatrix.Clone();

        if (changedIndices.Count == 1)
        {
            int color = newMatrix[changedIndices[0]];

            if (color == 0)
            {
                int row = changedIndices[0] / _cols;
                int col = changedIndices[0] % _cols;
                Character c = FindCharacterAtPosition(col, row);
                if (c!=null)
                {
                    players.Remove(c);
                }
                cells[changedIndices[0]].SetEmpty();
            }
            else {
                List<Character> characters = Persist.Characters;
                foreach (Character c in characters)
                {
                    if (c.Color == color)
                    {
                        AssignCharacterInGrid(color, c);
                    }
                }
                cells[changedIndices[0]].UpdateValue(color);
            }
        }
        
    }



    private void swap(int indexA, int indexB/*, int objectTypeA, int objectTypeB*/)
    {
        if (indexA >= cells.Count || indexB >= cells.Count) return;

        Cell cellA = cells[indexA];
        Cell cellB = cells[indexB];
        int rowA = indexA / _cols;
        int colA = indexA % _cols;
        int rowB = indexB / _cols;
        int colB = indexB % _cols;

        Character movingPlayer = FindCharacterAtPosition(colA, rowA);
        
        if (movingPlayer != null)
        {
            Vector2Int pos = players[movingPlayer];
            pos.x = colB;
            pos.y = rowB;
            players[movingPlayer] = pos;
        }
        else
        {
            movingPlayer = FindCharacterAtPosition(colB, rowB);
            if (movingPlayer!=null){
                Vector2Int pos = players[movingPlayer];
                pos.x = colA;
                pos.y = rowA;
                players[movingPlayer] = pos;

            }


        }


        for (int i = 0; i < _masterAssets.Count; i++)
        {
            if (_masterAssets[i].GridX == cellA.X && _masterAssets[i].GridY == cellA.Y)
            {
                _masterAssets[i].GridX = cellB.X;
                _masterAssets[i].GridY = cellB.Y;
                if (_masterAssets[i].AssignedEntity is Monster && _masterMode)
                {
                    Requester.DeleteMonster(cellA.Y, cellA.X);
                    string name = _masterAssets[i].AssignedEntity.Name;
                    int pf = _masterAssets[i].AssignedEntity.Current_Pf;
                    Requester.AddMonster(name, pf, cellB.Y, cellB.X);
                    Debug.Log("aggiornamento riuscito");
                }
                else if (_masterAssets[i].AssignedEntity is Obstacle && _masterMode)
                {
                    Requester.DeleteObstacle(cellA.Y, cellA.X);
                    string name = _masterAssets[i].AssignedEntity.Name;
                    int pf = _masterAssets[i].AssignedEntity.Current_Pf;
                    Requester.AddObstacle(name, pf, cellB.Y, cellB.X);
                    Debug.Log("aggiornamento riuscito");
                }
            }
            else if (_masterAssets[i].GridX == cellB.X && _masterAssets[i].GridY == cellB.Y)
            {
                _masterAssets[i].GridX = cellA.X;
                _masterAssets[i].GridY = cellA.Y;
                if (_masterAssets[i].AssignedEntity is Monster && _masterMode)
                {
                    Requester.DeleteMonster(cellA.Y, cellA.X);
                    string name = _masterAssets[i].AssignedEntity.Name;
                    int pf = _masterAssets[i].AssignedEntity.Current_Pf;
                    Requester.AddMonster(name, pf, cellB.Y, cellB.X);
                }
                else if (_masterAssets[i].AssignedEntity is Obstacle && _masterMode)
                {
                    Requester.DeleteObstacle(cellA.Y, cellA.X);
                    string name = _masterAssets[i].AssignedEntity.Name;
                    int pf = _masterAssets[i].AssignedEntity.Current_Pf;
                    Requester.AddObstacle(name, pf, cellB.Y, cellB.X);
                }
            }
        }

        Sprite tempSprite = cellA.CellBackground.sprite;
        cellA.CellBackground.sprite = cellB.CellBackground.sprite;
        cellB.CellBackground.sprite = tempSprite;

        int tempX = cellA.X;
        int tempY = cellA.Y;

        cellA.UpdateCoordinates(cellB.X, cellB.Y);
        cellB.UpdateCoordinates(tempX, tempY);
        Vector3 tempPos = cellA.transform.localPosition;
        cellA.transform.localPosition = cellB.transform.localPosition;
        cellB.transform.localPosition = tempPos;
        cells[indexA] = cellB;
        cells[indexB] = cellA;

    }

    public void AddDraggableAsset(GridAsset asset)
    {
        if (_masterAssets == null)
            _masterAssets = new List<GridAsset>();

        _masterAssets.Add(asset);
    }

    void HandleTouchInputRaycast()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 rayPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero);

            if (hit.collider != null)
            {
                Cell touchedCell = hit.collider.GetComponentInParent<Cell>();
                if (touchedCell != null)
                {
                    touchedCell.HighlightForOneSecond(Color.green);
                    //Debug.Log($"Clicked cell at ({touchedCell.X}, {touchedCell.Y})");
                    OnCellTouched(touchedCell.X, touchedCell.Y, touchedCell);
                }
            }
        }

        // For touch input on mobile
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Touch touch = Input.touches[0];
            Vector2 rayPos = Camera.main.ScreenToWorldPoint(touch.position);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero);

            if (hit.collider != null)
            {
                Cell touchedCell = hit.collider.GetComponentInParent<Cell>();
                if (touchedCell != null)
                {
                    touchedCell.HighlightForOneSecond(Color.green);
                    //Debug.Log($"Touched cell at ({touchedCell.X}, {touchedCell.Y})");
                    OnCellTouched(touchedCell.X, touchedCell.Y, touchedCell);
                }
            }
        }
    }


    async Task OnCellTouched(int x, int y, Cell cell)
    {
        if (cell.IsButton)
        {
            //Debug.Log($"Button cell touched! Coordinates: ({x}, {y})");
            //FOR MASTER
            for (int i = 0; i < _masterAssets.Count; i++)
            {   
                Debug.Log($"popup mostro {_masterAssets[i].GridX} , {_masterAssets[i].GridY}");
                if (_masterAssets[i].GridX == cell.X && _masterAssets[i].GridY == cell.Y)
                {
                    
                    Vector2 position = GetCellCoordinates(cell, true);
                    if (cell.Y > 0)
                    {
                        popupManager.ShowPopup(_masterAssets[i].AssignedEntity, new Vector3(position.x, position.y, 100));
                    }
                    else { popupManager.ShowPopup(_masterAssets[i].AssignedEntity, new Vector3(position.x, position.y + 2, 100)); }

                    return;
                }
            }
            //FOR PLAYERS
            Character character = FindCharacterAtPosition(cell.Y, cell.X);
            if (character != null)
            {
                character.Current_Pf = await Requester.GetPlayerPf(character.Name);
                Vector2 position = GetCellCoordinates(cell, true);
                if (cell.Y > 0) { popupManager.ShowPopup(character, new Vector3(position.x, position.y, 100)); }
                else { popupManager.ShowPopup(character, new Vector3(position.x, position.y + 2, 100)); }
            }
        }
    }

    public void HighlightArea(int color, int range)
    {
    List<Coordinate> highlightedCoords = new List<Coordinate>();

    for (int i = 0; i < inputMatrix.Length; i++)
    {
        if (inputMatrix[i] == color)
        {
            int row = i / _cols;
            int col = i % _cols;

            // bounds
            int startX = col - range;
            int endX = col + range;
            int startY = row - range;
            int endY = row + range;

            startX = Mathf.Max(startX, 0);
            endX = Mathf.Min(endX, _cols - 1);
            startY = Mathf.Max(startY, 0);
            endY = Mathf.Min(endY, _rows - 1);

            // Highlight the square area
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    int index = y * _cols + x;
                    if (index >= 0 && index < cells.Count)
                    {
                        Cell c = cells[index];
                        if (c != null)
                        {
                            c.HighlightForOneSecond(Color.red);
                            
                            // Add the coordinates to our list
                            highlightedCoords.Add(new Coordinate { x = x, y = y });

                            if (c.IsButton) {
                                    Character victim = FindCharacterAtPosition(c.Y, c.X);
                                    if (victim != null) {
                                        UserPopup user = FindAnyObjectByType<UserPopup>();
                                        int newCurrentPf = ActionPerformer.PerformActionOnEntity(user.ActionPerformed, victim);
                                        ActionRequest actionRequest = new ActionRequest
                                        {
                                            newPF = newCurrentPf,
                                            victim = victim
                                        };
                                        if (index != i) { 
                                            pendingActions.Add(actionRequest);
                                        }
                                        
                                    }
                                    
                            }
                        }
                    }
                }
            }
        }
    }
    
    // If we found and highlighted cells, send them to the server
    if (highlightedCoords.Count > 0)
    {
        _ = SendHighlightsToServer(highlightedCoords, color);
    }
}

    public Vector2 GetCellCoordinates(Cell cell, bool worldCoordinates)
    {
        if (cell == null)
        {
            Debug.LogError("Cell is null!");
            return Vector2.zero;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return Vector2.zero;
        }

        // Get the RectTransform of the cell
        RectTransform cellRect = cell.GetComponent<RectTransform>();
        if (cellRect != null)
        {           
            Vector3 worldPos = cellRect.position;
            Vector2 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            bool isOnScreen = screenPos.x >= 0 && screenPos.x <= Screen.width &&
                             screenPos.y >= 0 && screenPos.y <= Screen.height;
            Debug.Log($"Is on screen: {isOnScreen}");
            if (worldCoordinates) { return worldPos; }

            return screenPos;
        }

        // Fallback if no RectTransform
        Vector2 fallbackPos = mainCamera.WorldToScreenPoint(cell.transform.position);
        Debug.Log($"Using fallback position (no RectTransform): {fallbackPos}");
        return fallbackPos;
    }

    [System.Serializable]
    public class Coordinate
    {
        public int x;
        public int y;
    }

    [System.Serializable]
    public class HighlightPayload
    {
        public List<Coordinate> coordinates;
        public int id;

    }


    [Header("Server Settings")]
    private string pythonServerUrl = $"http://{Persist.ProjectorIP}:3487/highlight"; 

    private struct ActionRequest
    {
        public int newPF;
        public Character victim;
    }

    private List <ActionRequest> pendingActions = new List<ActionRequest>();

    // Add this inside the MapManager class
    private async Task SendHighlightsToServer(List<Coordinate> coords, int id)
    {
        HighlightPayload payload = new HighlightPayload { coordinates = coords, id=id };
        string jsonData = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(pythonServerUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request
            var operation = request.SendWebRequest();

            // Wait for it to finish asynchronously
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error sending highlights to server: {request.error}");
            }
            else
            {
                Debug.Log("Successfully sent highlight coordinates to Python server.");
                foreach (var action in pendingActions)
                {
                    Requester.SetPlayerPf(action.victim.Name, action.newPF);
                    action.victim.Current_Pf = action.newPF; 
                    
                }
                Handheld.Vibrate();
            }
        }
    }


    void PrintMatrix(int[] mat)
    {
        string s = "";
        for (int i = 0; i < mat.Length; i++)
        {
            if (i % _cols == 0) s += "\n";
            s += mat[i];
        }
        Debug.Log(s);
    }

}