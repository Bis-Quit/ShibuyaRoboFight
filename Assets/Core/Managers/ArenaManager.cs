using UnityEngine;
using TMPro;

public class ArenaManager : MonoBehaviour
{
    [Header("Character Database")]
    public CharacterData[] characterDatabase;

    [Header("Player Setup")]
    public Transform playerSpawnPoint;
    public RobotUI playerUI;

    [Header("Enemy Setup")]
    public Transform enemySpawnPoint;
    public RobotUI enemyUI;

    public void Awake()
    {
        SpawnSelectedPlayer();
        SpawnEnemy();
    }

    public void SpawnSelectedPlayer()
    {
        int selectedID = PlayerPrefs.GetInt("SelectedPlayerID", 0);

        if (selectedID >= 0 && selectedID < characterDatabase.Length)
        {
            CharacterData selectedData = characterDatabase[selectedID];

            if (selectedData.visualPrefab != null)
            {
                GameObject characterObject = Instantiate(selectedData.visualPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
                characterObject.transform.localScale = playerSpawnPoint.localScale; 

                RobotStats statsCharacter = characterObject.GetComponent<RobotStats>();

                if (playerUI != null && statsCharacter != null)
                {
                    playerUI.BindRobot(statsCharacter);
                    if (playerUI.nameText != null)
                        playerUI.nameText.text = selectedData.characterName;
                }

                ResolutionManager resManager = FindFirstObjectByType<ResolutionManager>();
                if (resManager != null && statsCharacter != null)
                {
                    resManager.playerStats = statsCharacter;
                }

                DraftingManager draftManager = FindFirstObjectByType<DraftingManager>();
                if (draftManager != null && statsCharacter != null)
                {
                    draftManager.playerStats = statsCharacter;
                }

                EnemyAIManager aiManager = FindFirstObjectByType<EnemyAIManager>();
                if (aiManager != null && statsCharacter != null)
                {
                    aiManager.playerStats = statsCharacter;
                }

                GameOverManager gameOverManager = FindFirstObjectByType<GameOverManager>();
                if (gameOverManager != null && statsCharacter != null)
                {
                    gameOverManager.playerRobot = statsCharacter;
                }
            }
        }
        else
        {
            Debug.LogError("Error, Character ID not found!");
        }
    }

    public void SpawnEnemy()
    {
        int playerID = PlayerPrefs.GetInt("SelectedPlayerID", 0);
        int randomEnemyID = Random.Range(0, characterDatabase.Length);

        if (characterDatabase.Length > 1)
        {
            while (randomEnemyID == playerID)
            {
                randomEnemyID = Random.Range(0, characterDatabase.Length);
            }
        }

        CharacterData enemyData = characterDatabase[randomEnemyID];

        if (enemyData.visualPrefab != null)
        {
            GameObject enemyObject = Instantiate(enemyData.visualPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
            enemyObject.transform.localScale = enemySpawnPoint.localScale; 

            RobotStats statsEnemy = enemyObject.GetComponent<RobotStats>();

            if (enemyUI != null && statsEnemy != null)
            {
                enemyUI.BindRobot(statsEnemy);
                if (enemyUI.nameText != null)
                {
                    enemyUI.nameText.text = enemyData.characterName;
                }
            }

            ResolutionManager resManager = FindFirstObjectByType<ResolutionManager>();
            if (resManager != null && statsEnemy != null)
            {
                resManager.enemyStats = statsEnemy;
            }

            EnemyAIManager aiManager = FindFirstObjectByType<EnemyAIManager>();
            if (aiManager != null && statsEnemy != null)
            {
                aiManager.aiStats = statsEnemy;
            }

            GameOverManager gameOverManager = FindFirstObjectByType<GameOverManager>();
            if (gameOverManager != null && statsEnemy != null)
            {
                gameOverManager.enemyRobot = statsEnemy;
            }
        }
        else
        {
            Debug.LogError("Waduh, Prefab Musuh Kosong bro!");
        }
    }
}