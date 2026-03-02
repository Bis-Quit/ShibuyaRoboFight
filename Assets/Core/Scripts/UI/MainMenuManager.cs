using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [SerializeField] private string gameplaySceneName = "Testing";

    public void Onclick_PlayGame()
    {
        Debug.Log("Memuat Scene Gameplay...");

        UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
    }

    public void Onclick_Exit()
    {
        Debug.Log("Keluar dari Game!");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
