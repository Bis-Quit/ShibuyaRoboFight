using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("UI Panels")]
    public GameObject panelMainBattle;
    public GameObject panelDiceScreen;
    public GameObject btnTapToRoll;
    public GameObject actionButtons;
    public GameObject marketClickIndicator;
    public RectTransform fightPanel;
    public GameObject notifDice;
    private bool hasPlayedFightIntro = false;

    [Header("Main Cameras")]
    public CinemachineCamera VCamArena;
    public CinemachineCamera VCamPlayer;
    public CinemachineCamera VCamEnemy;

    [Header("Cinematic Cams - Player POV")]
    public CinemachineCamera vCamPlayerAttack; 
    public CinemachineCamera vCamPlayerBuff;   
    public CinemachineCamera vCamPlayerHit;    

    [Header("Cinematic Cams - Enemy POV")]
    public CinemachineCamera vCamEnemyAttack;
    public CinemachineCamera vCamEnemyBuff;
    public CinemachineCamera vCamEnemyHit;
    
    public CinemachineCamera vCamTokenMove;

    [Header("Priority Settings")]
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = 10;
    [SerializeField] private int cinematicPriority = 30;

    private bool wasDiceScreenActive;
    private bool wasMainBattleActive;
    private bool wasNotifActive;
    private bool wasTapToRollActive;
    private CinemachineCamera previousCam;
    
    [HideInInspector] public bool isCinematicActive = false;

    private TurnManager.TurnPhase currentPhaseMemory;

    private void Awake()
    {
        Instance = this;
        
        if (panelMainBattle != null) panelMainBattle.SetActive(false);
        if (panelDiceScreen != null) panelDiceScreen.SetActive(false);
    }

    public void SwitchCinematicPOV(bool isPlayerTurn, string animName, Transform dynamicTarget = null)
    {
        if(vCamPlayerAttack != null) vCamPlayerAttack.Priority = inactivePriority;
        if(vCamPlayerBuff != null) vCamPlayerBuff.Priority = inactivePriority;
        if(vCamPlayerHit != null) vCamPlayerHit.Priority = inactivePriority;
        if(vCamEnemyAttack != null) vCamEnemyAttack.Priority = inactivePriority;
        if(vCamEnemyBuff != null) vCamEnemyBuff.Priority = inactivePriority;
        if(vCamEnemyHit != null) vCamEnemyHit.Priority = inactivePriority;
        if(vCamTokenMove != null) vCamTokenMove.Priority = inactivePriority;

        switch (animName)
        {
            case "Attack":
            case "destruct":
                if (isPlayerTurn && vCamPlayerAttack != null) vCamPlayerAttack.Priority = cinematicPriority;
                else if (!isPlayerTurn && vCamEnemyAttack != null) vCamEnemyAttack.Priority = cinematicPriority;
                break;

            case "restore":
            case "get power":
            case "Bragging":
            case "Signaling":
                if (isPlayerTurn && vCamPlayerBuff != null) vCamPlayerBuff.Priority = cinematicPriority;
                else if (!isPlayerTurn && vCamEnemyBuff != null) vCamEnemyBuff.Priority = cinematicPriority;
                break;

            case "got attacked":
            case "Reaction":
                if (isPlayerTurn && vCamPlayerHit != null) vCamPlayerHit.Priority = cinematicPriority;
                else if (!isPlayerTurn && vCamEnemyHit != null) vCamEnemyHit.Priority = cinematicPriority;
                break;

            case "TokenMove":
                if (vCamTokenMove != null && dynamicTarget != null) 
                {
                    vCamTokenMove.LookAt = dynamicTarget;
                    vCamTokenMove.Follow = dynamicTarget;
                    vCamTokenMove.Priority = cinematicPriority;
                }
                break;

            case "Reset":
                break;
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
        
        if (marketClickIndicator != null) 
        {
            bool canShow = (phase == TurnManager.TurnPhase.CardDrafting) && isPlayerTurn;
            
            if (HandInspectManager.Instance != null && HandInspectManager.Instance.inspectPanel != null)
            {
                if (HandInspectManager.Instance.inspectPanel.activeSelf) canShow = false; 
            }

            marketClickIndicator.SetActive(canShow);
        }
        
        if (phase == TurnManager.TurnPhase.FirstRoll) 
        {
            if (!hasPlayedFightIntro)
            {
                StartCoroutine(IntroFightRoutine(isPlayerTurn));
            }
            else
            {
                ShowDiceScreen(isPlayerTurn);
            }
        }
        else if (phase == TurnManager.TurnPhase.RerollPhase) 
        {
            if (panelDiceScreen != null) panelDiceScreen.SetActive(true);
            if (actionButtons != null) actionButtons.SetActive(isPlayerTurn);
        } 
        else ShowMainBattleScreen();
    }

    private void ShowMainBattleScreen()
    {
        if (panelMainBattle != null) panelMainBattle.SetActive(true);
        if (panelDiceScreen != null) panelDiceScreen.SetActive(false);
        if (EnemyCardContainer.Instance != null) EnemyCardContainer.Instance.SetHandVisible(true);
        SetCameraPriority(VCamArena, VCamPlayer, VCamEnemy);
    }

    private Coroutine diceUIDelayCoroutine;

    private void ShowDiceScreen(bool isPlayerTurn)
    {
        if (panelMainBattle != null) panelMainBattle.SetActive(false);
        if (btnTapToRoll != null) btnTapToRoll.SetActive(isPlayerTurn);
        if (actionButtons != null) actionButtons.SetActive(isPlayerTurn);
        if (EnemyCardContainer.Instance != null) EnemyCardContainer.Instance.SetHandVisible(false);
        if (isPlayerTurn) SetCameraPriority(VCamPlayer, VCamArena, VCamEnemy);
        else SetCameraPriority(VCamEnemy, VCamArena, VCamPlayer);

        if (diceUIDelayCoroutine != null) StopCoroutine(diceUIDelayCoroutine);
        diceUIDelayCoroutine = StartCoroutine(DelayDiceUI(2f));
    }

    private IEnumerator IntroFightRoutine(bool isPlayerTurn)
    {
        hasPlayedFightIntro = true; 

        if (panelMainBattle != null) panelMainBattle.SetActive(false);
        if (HUDManager.Instance != null) HUDManager.Instance.ToggleHUD(false);

        SetCameraPriority(VCamArena, VCamPlayer, VCamEnemy);

        if (fightPanel != null)
        {
            fightPanel.gameObject.SetActive(true);

            fightPanel.localScale = Vector3.one * 8f;
            fightPanel.localRotation = Quaternion.Euler(0, 0, -15f);

            Sequence fightAnim = DOTween.Sequence();

            fightAnim.Join(fightPanel.DOScale(Vector3.one, 0.2f).SetEase(Ease.InExpo));
            fightAnim.Join(fightPanel.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutBack));

            fightAnim.Append(fightPanel.DOShakeAnchorPos(0.4f, strength: 40f, vibrato: 30));

            if (AudioManager.Instance != null && AudioManager.Instance.fightSFX != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.fightSFX);
            }

            fightAnim.AppendInterval(0.6f); 
            fightAnim.Append(fightPanel.DOScale(new Vector3(1.3f, 0.7f, 1f), 0.15f));

            fightAnim.Append(fightPanel.DOScale(Vector3.one * 15f, 0.2f).SetEase(Ease.InExpo));
        }

        yield return new WaitForSeconds(1.6f); 

        if (fightPanel != null)
        {
            DOTween.Kill(fightPanel); 
            fightPanel.gameObject.SetActive(false);
            
            fightPanel.localScale = Vector3.one; 
            fightPanel.localRotation = Quaternion.identity; 
            fightPanel.anchoredPosition = Vector2.zero; 
        }

        if (panelMainBattle != null) panelMainBattle.SetActive(true);
        if (HUDManager.Instance != null) HUDManager.Instance.ToggleHUD(true);

        ShowDiceScreen(isPlayerTurn);
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

        if (isVisible && !panel.activeSelf) 
        {
            panel.SetActive(true);
        }

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        cg.alpha = isVisible ? 1f : 0f;
        cg.interactable = isVisible;
        cg.blocksRaycasts = isVisible;

        if (!isVisible) 
        {
            panel.SetActive(false);
        }
    }

    public void HideUIForCinematic()
    {
        isCinematicActive = true;

        if (diceUIDelayCoroutine != null) StopCoroutine(diceUIDelayCoroutine);

        if (notifDice != null) { wasNotifActive = notifDice.activeSelf; notifDice.SetActive(false); }
        if (btnTapToRoll != null) { wasTapToRollActive = btnTapToRoll.activeSelf; btnTapToRoll.SetActive(false); }

        SetPanelVisible(panelDiceScreen, false);
        SetPanelVisible(panelMainBattle, false);

        if (EnemyCardContainer.Instance != null) EnemyCardContainer.Instance.SetHandVisible(false);
        if (actionButtons != null) actionButtons.SetActive(false);
        if (HUDManager.Instance != null) HUDManager.Instance.ToggleHUD(false);
        HideMarketIndicator();

        if (PassiveCardManager.Instance != null && PassiveCardManager.Instance.playerHand != null)
        {
            PassiveCardManager.Instance.playerHand.gameObject.SetActive(false);
        }
    }

    public void RestoreUIAfterCinematic()
    {
        isCinematicActive = false;

        if (notifDice != null) notifDice.SetActive(wasNotifActive);
        if (btnTapToRoll != null) btnTapToRoll.SetActive(wasTapToRollActive);
        if (HUDManager.Instance != null) HUDManager.Instance.ToggleHUD(true);

        if (PassiveCardManager.Instance != null && PassiveCardManager.Instance.playerHand != null)
        {
            PassiveCardManager.Instance.playerHand.gameObject.SetActive(true);
        }

        if (currentPhaseMemory == TurnManager.TurnPhase.FirstRoll || currentPhaseMemory == TurnManager.TurnPhase.RerollPhase)
        {
            if (panelDiceScreen != null) panelDiceScreen.SetActive(true);
            SetPanelVisible(panelDiceScreen, true);
            SetPanelVisible(panelMainBattle, false);
            
            if (EnemyCardContainer.Instance != null) EnemyCardContainer.Instance.SetHandVisible(false);
            
            bool isPlayerTurn = TurnManager.Instance != null && TurnManager.Instance.CurrentPlayerIndex == 0;
            if (actionButtons != null) actionButtons.SetActive(isPlayerTurn);
        }
        else
        {
            SetPanelVisible(panelDiceScreen, false);
            SetPanelVisible(panelMainBattle, true);
            if (EnemyCardContainer.Instance != null) EnemyCardContainer.Instance.SetHandVisible(true);
        }
    }

    public void HideMarketIndicator()
    {
        if (marketClickIndicator != null)
        {
            marketClickIndicator.SetActive(false);
        }
    }
}