using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoSplashManager : MonoBehaviour
{
    [Header("Komponen Video")]
    public VideoPlayer splashVideoPlayer;

    [Header("Tujuan Scene Selanjutnya")]
    public string mainMenuSceneName = "MainMenu";

    private bool isSkipping = false;

    private void Start()
    {
        if (splashVideoPlayer != null)
        {
            splashVideoPlayer.loopPointReached += EndReached;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isSkipping)
        {
            isSkipping = true;
            LoadMainMenu();
        }
    }

    private void EndReached(VideoPlayer vp)
    {
        if (!isSkipping)
        {
            LoadMainMenu();
        }
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}