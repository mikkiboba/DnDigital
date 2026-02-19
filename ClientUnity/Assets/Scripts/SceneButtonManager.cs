using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonManager : MonoBehaviour
{
    [SerializeField] private string targetSceneName;

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(targetSceneName)) 
        {
            Debug.LogWarning("Target scene name is empty!");
            return;
        }
        
        SceneManager.LoadScene(targetSceneName);
    }
}