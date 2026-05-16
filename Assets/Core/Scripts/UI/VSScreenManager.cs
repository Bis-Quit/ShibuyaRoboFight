using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VSScreenManager : MonoBehaviour
{
    [Header("Database")]
    public CharacterData[] characterDatabase;

    [Header("3D Model Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("Transition Settings")]
    public float delayTime = 3f;
    public string arenaSceneName = "ArenaTesting";

    private void Start()
    {
        SetupVSScreen();
        StartCoroutine(TransitionToArena());
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

        if (playerData.visualPrefab != null)
        {
            Instantiate(playerData.visualPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        }

        if (enemyData.visualPrefab != null)
        {
            Instantiate(enemyData.visualPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
        }
    }

    private IEnumerator TransitionToArena()
    {
        yield return new WaitForSeconds(delayTime);

        SceneManager.LoadScene(arenaSceneName);
    }
}