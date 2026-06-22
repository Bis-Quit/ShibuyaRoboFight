using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI Canvas (Pastikan Sejajar)")]
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject battleUICanvas;

    [Header("Elemen UI")]
    [Tooltip("Masukin komponen Image buat Mahkota/Tengkorak")]
    [SerializeField] private Image iconTopImage; 
    
    [Tooltip("TextMeshPro buat Nama Robot")]
    [SerializeField] private TMP_Text robotNameText; 
    
    [Tooltip("Masukin komponen Image buat tulisan VICTORY/DEFEAT")]
    [SerializeField] private Image statusTextImage; 

    [Header("Katalog Gambar")]
    [SerializeField] private Sprite crownSprite;
    [SerializeField] private Sprite skullSprite;
    [SerializeField] private Sprite victorySprite;
    [SerializeField] private Sprite defeatSprite;

    [Header("Buttons")]
    [SerializeField] private Button selectCharButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Kamera Selebrasi")]
    [SerializeField] private CinemachineCamera vcamVictoryPlayer;
    [SerializeField] private CinemachineCamera vcamVictoryEnemy;

    [Header("Scene")]
    [SerializeField] private string selectCharacterSceneName = "CharacterSelect";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Robot Reference")]
    public RobotStats playerRobot;
    public RobotStats enemyRobot;

    [Header("VFX Defeat")]
    public GameObject defeatExplosionPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);

        if (selectCharButton != null) selectCharButton.onClick.AddListener(LoadSelectCharacter);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(LoadMainMenu);
    }

    public void TriggerGameOver(bool isPlayerWin)
    {
        SaveMatchResult(isPlayerWin);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
        }

        if (battleUICanvas != null) battleUICanvas.SetActive(false);

        RobotStats loserRobot = isPlayerWin ? enemyRobot : playerRobot;
        if (defeatExplosionPrefab != null && loserRobot != null)
        {
            RobotVFXManager loserVFX = loserRobot.GetComponent<RobotVFXManager>();
            Transform explosionCenter = (loserVFX != null && loserVFX.hitPoint != null) ? loserVFX.hitPoint : loserRobot.transform;
            
            Instantiate(defeatExplosionPrefab, explosionCenter.position, Quaternion.identity);
        }

        string winningRobotName = "UNKNOWN ROBOT";
        if (isPlayerWin && playerRobot != null && playerRobot.baseData != null)
        {
            winningRobotName = playerRobot.baseData.characterName;
        }
        else if (!isPlayerWin && enemyRobot != null && enemyRobot.baseData != null)
        {
            winningRobotName = enemyRobot.baseData.characterName;
        }

        if (robotNameText != null) robotNameText.text = winningRobotName.ToUpper();

        if (AudioManager.Instance != null && AudioManager.Instance.victoryBGM != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.victoryBGM);
        }

        if (isPlayerWin)
        {
            if (iconTopImage != null) iconTopImage.sprite = crownSprite;
            if (statusTextImage != null) statusTextImage.sprite = victorySprite;
            if (vcamVictoryPlayer != null) vcamVictoryPlayer.Priority = 20;
        }
        else
        {
            if (iconTopImage != null) iconTopImage.sprite = skullSprite;
            if (statusTextImage != null) statusTextImage.sprite = defeatSprite;
            if (vcamVictoryEnemy != null) vcamVictoryEnemy.Priority = 20;
        }

        Invoke("ShowGameOverUI", 2f);
    }

    private void ShowGameOverUI()
    {
        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
    }

    public void LoadSelectCharacter()
    {
        SceneManager.LoadScene(selectCharacterSceneName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SaveMatchResult(bool isPlayerWin)
    {
        if (PlayerPrefs.GetInt("IsLoggedIn", 0) == 1)
        {
            string currentUser = PlayerPrefs.GetString("LoggedUsername", "");
            PlayerDatabase db = SaveSystem.LoadDatabase();

            int index = db.accountList.FindIndex(x => x.playerName == currentUser);

            if (index != -1)
            {
                if (isPlayerWin) db.accountList[index].totalWins += 1;
                else db.accountList[index].totalLosses += 1;

                SaveSystem.SaveDatabase(db);
                Debug.Log("<color=green>[SaveSystem] Statistik Ter-Update! W: " + db.accountList[index].totalWins + " | L: " + db.accountList[index].totalLosses + "</color>");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y)) TriggerGameOver(true);
        if (Input.GetKeyDown(KeyCode.U)) TriggerGameOver(false);
    }
}