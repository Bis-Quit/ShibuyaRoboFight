using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.UI;

public class BattleIntroManager : MonoBehaviour
{
    [Header("UI & Canvas")]
    public GameObject introOverlay;
    public GameObject battleUI;
    public Slider loadingBar;

    [Header("Cinemachine CAM")]
    public CinemachineCamera vcamPlayer;
    public CinemachineCamera vcamEnemy;
    public CinemachineCamera vcamArena;

    [Header("Timer Settings")]
    public float readingTime = 5f;

    public void Awake()
    {
        if (battleUI != null)
        {
            battleUI.SetActive(false);
        }
    }

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        if (introOverlay != null)
        {
            introOverlay.SetActive(true);
        }
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
        }

        float elapsed = 0f;
        while (elapsed < readingTime)
        {
            elapsed += Time.deltaTime;
            
            if (loadingBar != null) 
            {
                loadingBar.value = elapsed / readingTime;
                Debug.Log($"<color=cyan>Loading Bar: {loadingBar.value}</color>");
            }
            else
            {
                Debug.Log("<color=red>KABEL LOADING BAR KOSONG/LEPAS!</color>");
            }
            
            yield return null; 
        }

        if (loadingBar != null)
        {
            loadingBar.value = 1f;
        }
        if (introOverlay != null)
        {
            introOverlay.SetActive(false);
        }

        if (vcamPlayer != null && vcamEnemy != null && vcamArena != null)
        {
            vcamPlayer.Priority = 20;
            vcamEnemy.Priority = 10;
            vcamArena.Priority = 10;
            yield return new WaitForSeconds(3f);

            vcamPlayer.Priority = 10;
            vcamEnemy.Priority = 20;
            vcamArena.Priority = 10;
            yield return new WaitForSeconds(3f);

            vcamPlayer.Priority = 10;
            vcamEnemy.Priority = 10;
            vcamArena.Priority = 20;
            yield return new WaitForSeconds(3f);
        }

        if (battleUI != null)
        {
            battleUI.SetActive(true);
        }
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.StartMatchAfterIntro();
        }
    }
}
