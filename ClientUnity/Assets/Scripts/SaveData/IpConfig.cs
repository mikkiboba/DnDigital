using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IpConfig : MonoBehaviour
{
    [SerializeField] private TMP_Text ipPlaceholder;
    [SerializeField] private TMP_InputField ipInput;

    [SerializeField] private TMP_Text ip;

    private enum IpType { Camera, Projector };
    [SerializeField] IpType ipType;



    void Start()
    {
        if (ipType == IpType.Camera) ipPlaceholder.text = Persist.CameraIP;
        else if (ipType == IpType.Projector) ipPlaceholder.text = Persist.ProjectorIP;

        ipInput.onEndEdit.AddListener(SaveIP);
    }

    private void SaveIP(string value)
    {
        if (ipType ==  IpType.Camera) Persist.CameraIP = value;
        else if (ipType == IpType.Projector) Persist.ProjectorIP = value;
    }

}

