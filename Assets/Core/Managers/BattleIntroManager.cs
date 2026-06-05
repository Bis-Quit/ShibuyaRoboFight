using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.UI;

public class BattleIntroManager : MonoBehaviour
{
    [Header("UI & Canvas")]
    public GameObject introOverlay;
    public GameObject battleUI;
    public GameObject yellowGuideNotif; 
    public Slider loadingBar;
    
    public GameObject tapToNextImage; 

    [Header("Cinemachine CAM")]
    public CinemachineCamera vcamPlayer;
    public CinemachineCamera vcamEnemy;
    public CinemachineCamera vcamArena;

    [Header("Timer Settings")]
    public float readingTime = 5f;

    public void Awake()
    {
        if (battleUI != null) battleUI.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        if (introOverlay != null) introOverlay.SetActive(true);
        if (loadingBar != null) loadingBar.value = 0f;

        if (tapToNextImage != null) tapToNextImage.SetActive(false); 

        if (yellowGuideNotif != null && NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification(yellowGuideNotif);
        }

        float elapsed = 0f;
        while (elapsed < readingTime)
        {
            elapsed += Time.deltaTime;
            if (loadingBar != null) loadingBar.value = elapsed / readingTime;

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("<color=yellow>BattleIntro: Player nge-skip loading!</color>");
                break;
            }

            yield return null; 
        }

        if (loadingBar != null) loadingBar.value = 1f;

        if (tapToNextImage != null) tapToNextImage.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
        
        Debug.Log("<color=green>BattleIntro: Player lanjut! Mulai kamera sinematik...</color>");

        if (yellowGuideNotif != null && NotificationManager.Instance != null)
        {
            NotificationManager.Instance.HideNotification(yellowGuideNotif);
            yield return new WaitForSeconds(0.2f); 
        }

        if (introOverlay != null) introOverlay.SetActive(false);

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

        if (battleUI != null) battleUI.SetActive(true);

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.StartMatchAfterIntro();
        }
    }
}