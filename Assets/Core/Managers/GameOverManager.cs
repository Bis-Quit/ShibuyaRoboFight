using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI Canvas")]
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private TMP_Text robotNameText;
    [SerializeField] private GameObject battleUICanvas;

    [Header("UI Images")]
    [SerializeField] private Image statusImage;
    [SerializeField] private Sprite victoryTextSprite;
    [SerializeField] private Sprite defeatTextSprite;

    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite victoryIconSprite;
    [SerializeField] private Sprite defeatIconSprite;

    [Header("Buttons")]
    [SerializeField] private Button selectCharButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineCamera vcamVictoryPlayer;
    [SerializeField] private CinemachineCamera vcamVictoryEnemy;

    /*[Header("Robot Animators")]
    [SerializeField] private Animator playerRobotAnimator;
    [SerializeField] private Animator enemyRobotAnimator; */

    [Header("Scene Settings")]
    [SerializeField] private string selectCharacterSceneName = "CharacterSelect";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);

        selectCharButton.onClick.AddListener(LoadSelectCharacter);
        mainMenuButton.onClick.AddListener(LoadMainMenu);
    }

    public void TriggerGameOver(bool isPlayerWin, string winningRobotName)
    {
        if (battleUICanvas != null) battleUICanvas.SetActive(false);
        robotNameText.text = winningRobotName;

        if (isPlayerWin)
        {
            statusImage.sprite = victoryTextSprite;
            if (iconImage != null) iconImage.sprite = victoryIconSprite;

            vcamVictoryPlayer.Priority = 20;
            /*playerRobotAnimator.SetTrigger("PlayerVictory");
            enemyRobotAnimator.SetTrigger("EnemyDefeat"); */
        }
        else
        {
            statusImage.sprite = defeatTextSprite;
            if (iconImage != null) iconImage.sprite = defeatIconSprite;

            vcamVictoryEnemy.Priority = 20;
            /*enemyRobotAnimator.SetTrigger("EnemyVictory");
            playerRobotAnimator.SetTrigger("PlayerDefeat"); */
        }

        Invoke("ShowGameOverUI", 2f);
    }

    private void ShowGameOverUI()
    {
        gameOverCanvas.SetActive(true);
    }

    public void LoadSelectCharacter()
    {
        SceneManager.LoadScene(selectCharacterSceneName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TriggerGameOver(true, "PLAYER ROBO (CHEAT)");
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            TriggerGameOver(false, "ENEMY ROBO (CHEAT)");
        }
    }
}
