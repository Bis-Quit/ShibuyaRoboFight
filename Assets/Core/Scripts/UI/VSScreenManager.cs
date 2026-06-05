using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class VSScreenManager : MonoBehaviour
{
    [Header("Database")]
    public CharacterData[] characterDatabase;

    [Header("3D Model Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("UI References")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI playerUsernameText; 
    public Slider loadingBar;
    
    // 👇 WADAH BARU BUAT LOGO VS 👇
    public RectTransform vsImageRect; 

    [Header("Transition Settings")]
    public float minimumDelayTime = 3f; 
    public string arenaSceneName = "ArenaTesting";

    private void Start()
    {
        SetupVSScreen();
        
        if (vsImageRect != null)
        {
            Sequence vsAnim = DOTween.Sequence();
            
            vsAnim.Append(vsImageRect.DOScale(1.25f, 0.1f).SetEase(Ease.OutExpo));
            vsAnim.Join(vsImageRect.DOShakeRotation(0.2f, new Vector3(0, 0, 12f), 30, 90f));
            vsAnim.Append(vsImageRect.DOScale(1f, 0.15f).SetEase(Ease.OutSine));

            vsAnim.Append(vsImageRect.DOScale(1.1f, 0.05f).SetEase(Ease.OutQuad));
            vsAnim.Append(vsImageRect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

            vsAnim.AppendInterval(1.5f);
            vsAnim.SetLoops(-1); 
        }

        StartCoroutine(TransitionToArenaAsync());
    }

    private void SetupVSScreen()
    {
        int playerID = PlayerPrefs.GetInt("SelectedPlayerID", 0);
        int enemyID = Random.Range(0, characterDatabase.Length);
        
        if (characterDatabase.Length > 1)
        {
            while (enemyID == playerID)
            {
                enemyID = Random.Range(0, characterDatabase.Length);
            }
        }

        PlayerPrefs.SetInt("CurrentEnemyID", enemyID);
        PlayerPrefs.Save();

        CharacterData playerData = characterDatabase[playerID];
        CharacterData enemyData = characterDatabase[enemyID];

        if (playerData.visualPrefab != null) Instantiate(playerData.visualPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        if (enemyData.visualPrefab != null) Instantiate(enemyData.visualPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);

        if (playerNameText != null) playerNameText.text = playerData.characterName; 
        if (enemyNameText != null) enemyNameText.text = enemyData.characterName;

        if (playerUsernameText != null)
        {
            string savedUsername = PlayerPrefs.GetString("UsernameKey", "YOUR HERO");

            if (string.IsNullOrEmpty(savedUsername))
            {
                savedUsername = "YOUR HERO";
            }

            playerUsernameText.text = savedUsername;
        }
    }

    private IEnumerator TransitionToArenaAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(arenaSceneName);
        operation.allowSceneActivation = false; 

        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;

            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumDelayTime);

            if (loadingBar != null)
            {
                loadingBar.value = Mathf.Min(loadProgress, timeProgress);
            }

            if (operation.progress >= 0.9f && elapsedTime >= minimumDelayTime)
            {
                if (loadingBar != null) loadingBar.value = 1f;

                if (vsImageRect != null) DOTween.Kill(vsImageRect);

                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}