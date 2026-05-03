using System;
using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public enum TurnPhase
    {
        None,
        TurnStart,
        FirstRoll,
        RerollPhase,
        Resolution,
        TugOfWarUpdate,
        CardDrafting,
        TurnEnd
    }

    public TurnPhase CurrentPhase { get; private set; }

    public int CurrentPlayerIndex { get; private set; }

    private int currentRerollCount = 0;
    private const int MAX_REROLLS = 0;

    public static event Action<TurnPhase> OnPhaseChanged;
    public static event Action<int> OnPlayerTurnChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;

        DiceManager.OnAllDiceStopped += HandleAllDiceStopped;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;

        DiceManager.OnAllDiceStopped -= HandleAllDiceStopped;

    }

    private void HandleGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Gameplay)
        {
            StartNewGameLoop();
        }
    }

    private void StartNewGameLoop()
    {
        StartCoroutine(OpeningSceneRoutine());
    }

    private IEnumerator OpeningSceneRoutine()
    {
        Debug.Log("<color=yellow>TurnManager: Memulai Opening Scene...</color>");

        yield return new WaitForSeconds(3f);

        Debug.Log("<color=yellow>TurnManager: Opening Scene selesai, memulai battle!</color>");
        CurrentPlayerIndex = 0;
        OnPlayerTurnChanged?.Invoke(CurrentPlayerIndex);
        ChangePhase(TurnPhase.TurnStart);
    }

    private void ChangePhase(TurnPhase newPhase)
    {
        if (CurrentPhase == newPhase) return;
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);

        switch(newPhase)
        {
            case TurnPhase.TurnStart:
                HandleTurnStart();
                break;
            case TurnPhase.CardDrafting:
                HandleCardDrafting();
                break;
            case TurnPhase.FirstRoll:
                HandleFirstRoll();
                break;
            case TurnPhase.RerollPhase:
                HandleRerollPhase();
                break;
            case TurnPhase.Resolution:
                HandleResolution();
                break;
            case TurnPhase.TurnEnd:
                HandleTurnEnd();
                break;
        }
    }

    private void HandleTurnStart()
    {
        Debug.Log($"Giliran Pemain {CurrentPlayerIndex} Dimulai");
        currentRerollCount = 0;

        ChangePhase(TurnPhase.FirstRoll);
    }

    private void HandleFirstRoll()
    {
        Debug.Log("Fase First Roll: Melempar semua dadu!");
    }

    private void HandleRerollPhase()
    {
        Debug.Log($"Fase Reroll ({currentRerollCount}/{MAX_REROLLS}): Menunggu pemain nge-lock dadu...");
    }

public void ProcessedToResolution()
    {
        ChangePhase(TurnPhase.Resolution);
    }

    private void HandleResolution()
    {
        Debug.Log("<color=cyan>Fase Resolution: ResolutionManager mengambil alih! Sutradara istirahat bentar.</color>");
    }

    public void ProcessedToDrafting()
    {
        ChangePhase(TurnPhase.CardDrafting);
    }

    private void HandleCardDrafting()
    {
        Debug.Log("Fase Card Drafting: Menunggu pemain memilih kartu dari Open Market...");
    }

    public void ProcessedToTurnEnd()
    {
        ChangePhase(TurnPhase.TurnEnd);
    }

    private void HandleTurnEnd()
    {
        Debug.Log("Giliran Selesai. Mengecek Pemenang...");

        DiceManager.Instance.CleanUpDiceForNextTurn();

        CurrentPlayerIndex = (CurrentPlayerIndex == 0) ? 1 : 0;
        OnPlayerTurnChanged?.Invoke(CurrentPlayerIndex);

        StartCoroutine(EndTurnDelayRoutine());
    }

    private IEnumerator EndTurnDelayRoutine()
    {
        yield return new WaitForSeconds(2f);
        ChangePhase(TurnPhase.TurnStart);
    }

    private void HandleAllDiceStopped()
    {
        if (CurrentPhase == TurnPhase.FirstRoll)
        {
            Debug.Log("TurnManager: Mendapat laporan semua dadu sudah berhenti. Melanjutkan ke fase ReRoll!");
            ChangePhase(TurnPhase.RerollPhase);
        }
        else if (CurrentPhase == TurnPhase.RerollPhase)
        {
            Debug.Log("TurnManager: Re-Roll Selesai! Silakan Lock dadu lagi atau lanjut ke fase Resolve!");
        }
    }
}
