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
                
                if (statsCharacter != null)
                {
                    statsCharacter.baseData = selectedData;
                }

                if (DiceManager.Instance != null)
                {
                    DiceManager.Instance.playerStats = statsCharacter;
                }

                if (GameOverManager.Instance != null)
                {
                    GameOverManager.Instance.playerRobot = statsCharacter;
                }

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

                CardEffectManager cardEffectManager = FindFirstObjectByType<CardEffectManager>();
                if (cardEffectManager != null && statsCharacter != null)
                {
                    cardEffectManager.playerStats = statsCharacter;
                    cardEffectManager.playerAnim = characterObject.GetComponentInChildren<CharacterAnimator>();
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
        int finalEnemyID;

        if (PlayerPrefs.HasKey("CurrentEnemyID"))
        {
            finalEnemyID = PlayerPrefs.GetInt("CurrentEnemyID");
            Debug.Log($"<color=green>Retry Aktif! Musuh menggunakan ID: {finalEnemyID}</color>");
        }
        else
        {
            finalEnemyID = Random.Range(0, characterDatabase.Length);

            if (characterDatabase.Length > 1)
            {
                while (finalEnemyID == playerID)
                {
                    finalEnemyID = Random.Range(0, characterDatabase.Length);
                }
            }

            PlayerPrefs.SetInt("CurrentEnemyID", finalEnemyID);
            PlayerPrefs.Save();
            Debug.Log($"<color=yellow>Memilih Enemy dengan random: ID {finalEnemyID}</color>");
        }

        CharacterData enemyData = characterDatabase[finalEnemyID];

        if (enemyData.visualPrefab != null)
        {
            GameObject enemyObject = Instantiate(enemyData.visualPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
            enemyObject.transform.localScale = enemySpawnPoint.localScale; 

            RobotStats statsEnemy = enemyObject.GetComponent<RobotStats>();
            
            if (statsEnemy != null)
            {
                statsEnemy.baseData = enemyData;
            }

            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.enemyStats = statsEnemy;
            }

            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.enemyRobot = statsEnemy;
            }

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

            CardEffectManager cardEffectManager = FindFirstObjectByType<CardEffectManager>();
            if (cardEffectManager != null && statsEnemy != null)
            {
                cardEffectManager.enemyStats = statsEnemy;
                cardEffectManager.enemyAnim = enemyObject.GetComponentInChildren<CharacterAnimator>();
            }
        }
        else
        {
            Debug.LogError("Waduh, Prefab Musuh Kosong bro!");
        }
    }
}