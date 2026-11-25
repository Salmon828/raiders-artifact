using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private string gameSceneName = "Main";

    // Hidden in web builds, since browsers don't let a page close itself
    [SerializeField]
    private GameObject quitButton;

    private void Awake()
    {
#if UNITY_WEBGL
        if (quitButton != null)
        {
            quitButton.SetActive(false);
        }
#endif
    }

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
