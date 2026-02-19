using UnityEngine;

public class PopupDestroyer : MonoBehaviour
{
    public GameObject popupToDestroy;


    public void CloseAndDestroy()
    {
        if (popupToDestroy != null) Destroy(popupToDestroy);
        Destroy(gameObject);
    }
}
