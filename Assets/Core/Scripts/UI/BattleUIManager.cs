using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class BattleUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Tarik Panel Main Battle dari Hierarchy ke sini!")]
    public GameObject panelMainBattle;

    [Tooltip("Tarik Panel Dice Screen dari Hierarchy ke sini!")]
    public GameObject panelDiceScreen;

    [Tooltip("Tarik btnTapToRoll dari Hierarchy ke sini!")]
    public GameObject btnTapToRoll;

    [Tooltip("Masukin object 'Action Buttons' dari Hierarchy ke sini!")]
    public GameObject actionButtons;

    [Header("Cinemachine Cameras")]
    public CinemachineCamera VCamArena;
    public CinemachineCamera VCamPlayer;
    public CinemachineCamera VCamEnemy;

    [Header("Priority Settings")]
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = 10;

    private void Awake()
    {
        ShowMainBattleScreen();
    }

    private void OnEnable()
    {
        TurnManager.OnPhaseChanged += HandlePhaseChange;
    }

    private void OnDisable()
    {
        TurnManager.OnPhaseChanged -= HandlePhaseChange;    
    }

    private void HandlePhaseChange(TurnManager.TurnPhase phase)
    {
        bool isPlayerTurn = (TurnManager.Instance.CurrentPlayerIndex == 0);

        switch(phase)
        {
            case TurnManager.TurnPhase.FirstRoll:
                ShowDiceScreen(isPlayerTurn);
                break;

            case TurnManager.TurnPhase.RerollPhase:
                if (panelDiceScreen != null) panelDiceScreen.SetActive(true);
                Debug.Log("<color=cyan>UIManager: Reroll Phase - Tampilkan UI Reroll!</color>");

                if (actionButtons != null) actionButtons.SetActive(isPlayerTurn);
                break;

            default:
                ShowMainBattleScreen();
                break;
        }
    }

    private void ShowMainBattleScreen()
    {
        if (panelMainBattle != null) panelMainBattle.SetActive(true);
        if (panelDiceScreen != null) panelDiceScreen.SetActive(false);

        SetCameraPriority(VCamArena, VCamPlayer, VCamEnemy);

        Debug.Log("<color=cyan>UIManager: Pindah ke Layar Main Battle!</color>");
    }

    private void ShowDiceScreen(bool isPlayerTurn)
    {
        if (panelMainBattle != null) panelMainBattle.SetActive(false);
        if (btnTapToRoll != null) btnTapToRoll.SetActive(isPlayerTurn);
        if (actionButtons != null) actionButtons.SetActive(isPlayerTurn);

        Debug.Log("<color=cyan>UIManager: Pindah ke Layar Lempar Dadu!</color>");

        if (isPlayerTurn)
        {
            SetCameraPriority(VCamPlayer, VCamArena, VCamEnemy);
        }
        else
        {
            SetCameraPriority(VCamEnemy, VCamArena, VCamPlayer);
        }

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
}
