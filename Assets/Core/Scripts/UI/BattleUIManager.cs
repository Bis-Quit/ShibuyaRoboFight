using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("UI Panels")]
    public GameObject panelMainBattle;
    public GameObject panelDiceScreen;
    public GameObject btnTapToRoll;
    public GameObject actionButtons;

    [Header("Main Cameras")]
    public CinemachineCamera VCamArena;
    public CinemachineCamera VCamPlayer;
    public CinemachineCamera VCamEnemy;

    [Header("🎬 Cinematic Cams - Player POV")]
    public CinemachineCamera vCamPlayerAttack; 
    public CinemachineCamera vCamPlayerBuff;   
    public CinemachineCamera vCamPlayerHit;    

    [Header("🎬 Cinematic Cams - Enemy POV")]
    public CinemachineCamera vCamEnemyAttack;
    public CinemachineCamera vCamEnemyBuff;
    public CinemachineCamera vCamEnemyHit;

    [Header("Priority Settings")]
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = 10;
    [SerializeField] private int cinematicPriority = 30;

    private bool wasDiceScreenActive;
    private bool wasMainBattleActive;
    private CinemachineCamera previousCam;
    
    [HideInInspector] public bool isCinematicActive = false;

    private TurnManager.TurnPhase currentPhaseMemory;

    private void Awake()
    {
        Instance = this;
        ShowMainBattleScreen();
    }

    public void SwitchCinematicPOV(bool isPlayerTurn, string actionPhase)
    {
        if(vCamPlayerAttack != null) vCamPlayerAttack.Priority = inactivePriority;
        if(vCamPlayerBuff != null) vCamPlayerBuff.Priority = inactivePriority;
        if(vCamPlayerHit != null) vCamPlayerHit.Priority = inactivePriority;
        if(vCamEnemyAttack != null) vCamEnemyAttack.Priority = inactivePriority;
        if(vCamEnemyBuff != null) vCamEnemyBuff.Priority = inactivePriority;
        if(vCamEnemyHit != null) vCamEnemyHit.Priority = inactivePriority;

        if (actionPhase == "AttackAction") 
        {
            if (isPlayerTurn && vCamPlayerAttack != null) vCamPlayerAttack.Priority = cinematicPriority;
            else if (!isPlayerTurn && vCamEnemyAttack != null) vCamEnemyAttack.Priority = cinematicPriority;
        }
        else if (actionPhase == "BuffAction") 
        {
            if (isPlayerTurn && vCamPlayerBuff != null) vCamPlayerBuff.Priority = cinematicPriority;
            else if (!isPlayerTurn && vCamEnemyBuff != null) vCamEnemyBuff.Priority = cinematicPriority;
        }
        else if (actionPhase == "Reaction") 
        {
            if (isPlayerTurn && vCamEnemyHit != null) vCamEnemyHit.Priority = cinematicPriority;
            else if (!isPlayerTurn && vCamPlayerHit != null) vCamPlayerHit.Priority = cinematicPriority;
        }
    }

    public void ResetCamera()
    {
        SwitchCinematicPOV(true, "Reset"); 

        bool isPlayerTurn = TurnManager.Instance != null && TurnManager.Instance.CurrentPlayerIndex == 0;

        if (currentPhaseMemory == TurnManager.TurnPhase.FirstRoll || currentPhaseMemory == TurnManager.TurnPhase.RerollPhase)
        {
            if (isPlayerTurn) SetCameraPriority(VCamPlayer, VCamArena, VCamEnemy);
            else SetCameraPriority(VCamEnemy, VCamArena, VCamPlayer);
        }
        else
        {
            SetCameraPriority(VCamArena, VCamPlayer, VCamEnemy);
        }
    }

    private void OnEnable() { TurnManager.OnPhaseChanged += HandlePhaseChange; }
    private void OnDisable() { TurnManager.OnPhaseChanged -= HandlePhaseChange; }

    private void HandlePhaseChange(TurnManager.TurnPhase phase)
    {
        if (isCinematicActive) return;

        currentPhaseMemory = phase;

        bool isPlayerTurn = (TurnManager.Instance.CurrentPlayerIndex == 0);
        if (phase == TurnManager.TurnPhase.FirstRoll) ShowDiceScreen(isPlayerTurn);
        else if (phase == TurnManager.TurnPhase.RerollPhase) {
            if (panelDiceScreen != null) panelDiceScreen.SetActive(true);
            if (actionButtons != null) actionButtons.SetActive(isPlayerTurn);
        } else ShowMainBattleScreen();
    }

    private void ShowMainBattleScreen()
    {
        if (panelMainBattle != null) panelMainBattle.SetActive(true);
        if (panelDiceScreen != null) panelDiceScreen.SetActive(false);
        SetCameraPriority(VCamArena, VCamPlayer, VCamEnemy);
    }

    private void ShowDiceScreen(bool isPlayerTurn)
    {
        if (panelMainBattle != null) panelMainBattle.SetActive(false);
        if (btnTapToRoll != null) btnTapToRoll.SetActive(isPlayerTurn);
        if (actionButtons != null) actionButtons.SetActive(isPlayerTurn);
        if (isPlayerTurn) SetCameraPriority(VCamPlayer, VCamArena, VCamEnemy);
        else SetCameraPriority(VCamEnemy, VCamArena, VCamPlayer);
        StartCoroutine(DelayDiceUI(2f));
    }

    private void SetCameraPriority(CinemachineCamera targetActive, CinemachineCamera low1, CinemachineCamera low2)
    {
        if (targetActive != null) targetActive.Priority = activePriority;
        if (low1 != null) low1.Priority = inactivePriority;
        if (low2 != null) low2.Priority = inactivePriority;
    }

    private IEnumerator DelayDiceUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panelDiceScreen != null) panelDiceScreen.SetActive(true);
    }

    private void SetPanelVisible(GameObject panel, bool isVisible)
    {
        if (panel == null) return;
        
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        cg.alpha = isVisible ? 1f : 0f;
        cg.interactable = isVisible;
        cg.blocksRaycasts = isVisible;
    }

    public void HideUIForCinematic()
    {
        isCinematicActive = true;

        wasDiceScreenActive = panelDiceScreen != null && panelDiceScreen.activeSelf;
        wasMainBattleActive = panelMainBattle != null && panelMainBattle.activeSelf;

        if (wasDiceScreenActive) SetPanelVisible(panelDiceScreen, false);
        if (wasMainBattleActive) SetPanelVisible(panelMainBattle, false);

        if (actionButtons != null) actionButtons.SetActive(false);
        if (HUDManager.Instance != null) HUDManager.Instance.ToggleHUD(false);
    }

    public void RestoreUIAfterCinematic()
    {
        isCinematicActive = false;

        if (wasDiceScreenActive) SetPanelVisible(panelDiceScreen, true);
        if (wasMainBattleActive) SetPanelVisible(panelMainBattle, true);

        if (TurnManager.Instance != null && wasDiceScreenActive)
        {
            bool isPlayerTurn = (TurnManager.Instance.CurrentPlayerIndex == 0);
            if (actionButtons != null) actionButtons.SetActive(isPlayerTurn);
        }
        if (HUDManager.Instance != null) HUDManager.Instance.ToggleHUD(true);
    }
}