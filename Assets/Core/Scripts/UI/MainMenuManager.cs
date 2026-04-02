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
    [SerializeField] private string kuisArSatu = "AR(Testing)";

    public void Onclick_PlayGame()
    {
        Debug.Log("Memuat Scene Gameplay...");

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void Onclick_ArSatu()
    {
        Debug.Log("Memuat Scene ArSatu...");

        SceneManager.LoadScene(kuisArSatu);
    }

    public void Onclick_HowToPlay()
    {
        Debug.Log("Memuat Scene How To Play...");

        SceneManager.LoadScene(howtoplaySceneName);
    }

    public void Onclick_Credits()
    {
        Debug.Log("Memuat Scene Credits...");

        SceneManager.LoadScene(creditsSceneName);
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

        SceneManager.LoadScene(materiSceneName);
    }

    public void Onclick_Kuis()
    {
        Debug.Log("Memuat Scene Kuis...");

        SceneManager.LoadScene(kuisSceneName);
    }

    public void Onclick_MainMenu()
    {
        Debug.Log("Memuat Scebe Main Menu...");

        SceneManager.LoadScene(mainmenuSceneName);
    }
}
