using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;

public class MatrixReceiver : MonoBehaviour
{
    [Header("Network Settings")]
    public string serverIP = $"{Persist.CameraIP}";
    public int port = 6969;

    private Thread receiveThread;
    private bool isRunning = true;

    [Header("Shared data")]
    private int[] latestArray;
    private int latestRows;
    private int latestCols;
    private readonly object lockObject = new object();

    // Flag to tell the Main Thread we have new data
    private volatile bool _hasNewData = false;

    public event Action<int[], int, int> OnMatrixReady;

    void Start()
    {
        receiveThread = new Thread(new ThreadStart(BackgroundReceive));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void Update()
    {
        // Check if the background thread flagged new data
        if (_hasNewData)
        {
            int[] dataToSend;
            int r, c;

            // Quickly lock just to copy the data to local variables
            lock (lockObject)
            {
                dataToSend = (int[])latestArray.Clone(); // Clone to be safe
                r = latestRows;
                c = latestCols;
                _hasNewData = false; // Reset flag
            }

            // Notify the Manager (and anyone else listening)
            //Debug.Log("Dispatching new matrix to Manager...");
            OnMatrixReady?.Invoke(dataToSend, r, c);
        }
    }

    private void BackgroundReceive()
{
    while (isRunning)
    {
        try
        {
            using (TcpClient client = new TcpClient(serverIP, port))
            using (NetworkStream stream = client.GetStream())
            {
                // Read header
                byte[] headerBuffer = new byte[16];
                ReadFully(stream, headerBuffer, 16);

                int rows = BitConverter.ToInt32(headerBuffer, 4);
                int cols = BitConverter.ToInt32(headerBuffer, 8);

                // Read body
                int totalElements = rows * cols;
                byte[] bodyBuffer = new byte[totalElements * 4];
                ReadFully(stream, bodyBuffer, totalElements * 4);

                int[] tempArray = new int[totalElements];
                for (int i = 0; i < totalElements; i++)
                    tempArray[i] = BitConverter.ToInt32(bodyBuffer, i * 4);

                lock (lockObject)
                {
                    latestArray = tempArray;
                    latestRows = rows;
                    latestCols = cols;
                    _hasNewData = true;
                }
            }
        }
        catch
        {
            Thread.Sleep(50);
        }
    }
}

    private void ReadFully(NetworkStream stream, byte[] buffer, int size)
    {
        int offset = 0;
        while (offset < size)
        {
            int read = stream.Read(buffer, offset, size - offset);
            if (read == 0) throw new Exception("Socket closed");
            offset += read;
        }
    }

    private void OnApplicationQuit()
    {
        isRunning = false;
        //if (receiveThread != null) receiveThread.Abort();
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Join();
    }
}