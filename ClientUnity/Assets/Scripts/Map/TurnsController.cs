using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Linq;


public class TurnsController : MonoBehaviour
{
    [SerializeField] private ToggleGroup toggleGroup;
    private string pythonServerUrl = "http://192.168.1.14:3487/highlight";

    public enum Turn
    {
        Kabo = 2,
        Karina = 3,
        Cheese = 1
    }

    public TurnSelected OnTurnSelected;

    private void Start()
    {
        if (toggleGroup == null)
            toggleGroup = GetComponentInChildren<ToggleGroup>();

        Toggle[] toggles = GetComponentsInChildren<Toggle>();
        foreach (Toggle toggle in toggles)
        {
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener((isOn) => OnAnyToggleChanged(toggle, isOn));
        }
    }

    private void OnAnyToggleChanged(Toggle changedToggle, bool isOn)
    {
        if (isOn)
        {
            Text textComponent = changedToggle.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                Turn selectedTurn;
                if (Enum.TryParse(textComponent.text, out selectedTurn))
                {
                    OnTurnSelected?.Invoke(selectedTurn);
                    Debug.Log($"Turn selected: {selectedTurn}");
                }
            }

            SendTurnToServer();
        }
    }

    public Turn GetCurrentTurn()
    {
        Toggle activeToggle = toggleGroup.GetFirstActiveToggle();
        if (activeToggle != null)
        {
            Text textComponent = activeToggle.GetComponentInChildren<Text>();
            if (textComponent != null && Enum.TryParse(textComponent.text, out Turn currentTurn))
            {
                return currentTurn;
            }
        }

        return Turn.Kabo; // Default
    }

    public async Task SendTurnToServer()
    {
        List<MapManager.Coordinate> highlightedCoords = new List<MapManager.Coordinate>();
        highlightedCoords.Add(new MapManager.Coordinate { x = -1, y = -1 });
        Turn turn = GetCurrentTurn();
        MapManager.HighlightPayload payload = new MapManager.HighlightPayload { coordinates = highlightedCoords, id = (int)turn };

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
                Debug.LogError($"Error sending turn to server: {request.error}");
            }
            else
            {
                Debug.Log("Successfully sent turn to Python server.");
            }
        }
    }

    [Serializable]
    public class TurnSelected : UnityEngine.Events.UnityEvent<Turn> { }
}