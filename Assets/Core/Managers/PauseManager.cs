using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseOvelay;
    public CanvasGroup overlayCanvasGroup;
    public RectTransform pauseWindow;

    [Header("Animation Setting")]
    public float animDuration = 0.3f;
    private bool isPaused = false;

    private void Start()
    {
        if (pauseOvelay != null)
        {
            pauseOvelay.SetActive(false);
            overlayCanvasGroup.alpha = 0f;
            pauseWindow.localScale = Vector3.zero;
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseOvelay.SetActive(true);

        overlayCanvasGroup.DOFade(1f, animDuration).SetUpdate(true);
        pauseWindow.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack).SetUpdate(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        overlayCanvasGroup.DOFade(0f, animDuration).SetUpdate(true);
        pauseWindow.DOScale(Vector3.zero, animDuration).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
        {
            pauseOvelay.SetActive(false);
        });
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToSelectCharacter()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("CharacterSelect");
    }
}
