using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [SerializeField] private string gameplaySceneName = "Testing";
    [SerializeField] private string mainmenuSceneName = "MainMenu";
    [SerializeField] private string howtoplaySceneName = "HowToPlay";
    [SerializeField] private string creditsSceneName = "Credits";
    [SerializeField] private string materiSceneName =  "Materi";
    [SerializeField] private string kuisSceneName = "Kuis";

    public void Onclick_PlayGame()
    {
        Debug.Log("Memuat Scene Gameplay...");

        UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
    }

    public void Onclick_HowToPlay()
    {
        Debug.Log("Memuat Scene How To Play...");

        UnityEngine.SceneManagement.SceneManager.LoadScene(howtoplaySceneName);
    }

    public void Onclick_Credits()
    {
        Debug.Log("Memuat Scene Credits...");

        UnityEngine.SceneManagement.SceneManager.LoadScene(creditsSceneName);
    }

    public void Onclick_Exit()
    {
        Debug.Log("Keluar dari Game!");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void onClick_Materi()
    {
        Debug.Log("Memuat Scene Materi...");

        UnityEngine.SceneManagement.SceneManager.LoadScene(materiSceneName);
    }

    public void Onclick_Kuis()
    {
        Debug.Log("Memuat Scene Kuis...");

        UnityEngine.SceneManagement.SceneManager.LoadScene(kuisSceneName);
    }

    public void Onclick_MainMenu()
    {
        Debug.Log("Memuat Scebe Main Menu...");

        UnityEngine.SceneManagement.SceneManager.LoadScene(mainmenuSceneName);
    }
}
