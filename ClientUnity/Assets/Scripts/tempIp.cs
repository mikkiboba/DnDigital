using UnityEngine;

public class tempIp : MonoBehaviour
{
    [SerializeField] private string cameraIp = "127.0.0.1";
    [SerializeField] private string projectorIp = "127.0.0.1";

    void Awake()
    {
        Persist.CameraIP = cameraIp;
        Persist.ProjectorIP = projectorIp;
    }
}
