using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }

    [Header("Dice Setting")]
    public GameObject dicePrefab;
    public Transform spawnPoint;
    public int numberOfDice = 6;

    [Header("Player Reference")]
    public RobotStats playerStats;
    public RobotStats enemyStats;

    public List<Dice> activeDice = new List<Dice>();
    public List<Dice> lockedDice = new List<Dice>();

    [Header("Roll Setting")]
    public int maxRolls = 3;
    public int currentRollCount = 0;
    private bool isCheckingRollStatus = false;

    [Header("Audio SFX")]
    public AudioClip rollSFX;

    public static event Action OnAllDiceStopped;

    private void Awake()
    {
        TurnManager.OnPhaseChanged += HandleChanged;

        if (Instance == null) Instance = this;
        else Destroy (gameObject);
    }

    private void OnDestroy()
    {
        TurnManager.OnPhaseChanged -= HandleChanged; 
    }

    private void HandleChanged(TurnManager.TurnPhase phase)
    {
        if (phase == TurnManager.TurnPhase.FirstRoll)
        {
            currentRollCount = 1;

            Debug.Log("DiceManager: Dadu disiapkan. Menunggu lemparan pertama...");
        }
    }

    private void SpawnDice()
    {
        bool isPlayerTurn = (TurnManager.Instance.CurrentPlayerIndex == 0);
        RobotStats currentPlayer = isPlayerTurn ? playerStats : enemyStats;

        if (currentPlayer == null || currentPlayer.gameObject.scene.name == null)
        {
            Debug.LogWarning(" DiceManager kehilangan jejak robot! Memaksa sinkronisasi dengan CardEffectManager...");
            
            if (CardEffectManager.Instance != null)
            {
                currentPlayer = isPlayerTurn ? CardEffectManager.Instance.playerStats : CardEffectManager.Instance.enemyStats;
                
                if (isPlayerTurn) playerStats = currentPlayer;
                else enemyStats = currentPlayer;
            }
        }

        int totalDiceToSpawn = 6;
        if (currentPlayer != null && currentPlayer.baseData != null)
        {
            totalDiceToSpawn = currentPlayer.baseData.activeDicePool;
        }

        if (currentPlayer != null && currentPlayer.bonusDice > 0)
        {
            Debug.Log($"<color=yellow>MENGELUARKAN {currentPlayer.bonusDice} DADU TAMBAHAN DARI TABUNGAN!</color>");
            totalDiceToSpawn += currentPlayer.bonusDice;
            
            currentPlayer.bonusDice = 0; 
        }

        Debug.Log($"DiceManager: Memunculkan {totalDiceToSpawn} dadu di udara Shibuya...");

        for(int i = 0; i < totalDiceToSpawn; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 2f;
            randomOffset.y = MathF.Abs(randomOffset.y);

            Vector3 finalSpawnPos = spawnPoint.position + randomOffset;

            GameObject newDiceObj = Instantiate(dicePrefab, finalSpawnPos, Random.rotation);
            newDiceObj.name = "Dadu_" + (i+1);

            Dice diceScript = newDiceObj.GetComponent<Dice>();
            if (diceScript != null)
            {
                activeDice.Add(diceScript);
            }
        }
    }

    public void LockDice(Dice dice)
    {
        if (activeDice.Contains(dice))
        {
            activeDice.Remove(dice);

            lockedDice.Add(dice);

            dice.gameObject.SetActive(false);
            DiceUIManager.Instance.AddLockedDiceUI(dice);
            Debug.Log($"<color=yellow>DiceManager: {dice.name} ({dice.CurrentFace}) di-LOCK!</color>");
        }
    }

    public void UnlockDice(Dice dice)
    {
        if (lockedDice.Contains(dice))
        {
            lockedDice.Remove(dice);
            activeDice.Add(dice);
            
            dice.gameObject.SetActive(true);
            Debug.Log($"<color=white>DiceManager: {dice.name} di-UNLOCK dan kembali ke tray!</color>");
        }
    }

    public void RollAllDice()
    {
        if (rollSFX != null)
        {
            AudioManager.Instance.PlaySFX(rollSFX);
        }
        Debug.Log("DiceManager: Melempar SEMUA dadu!...");


        if (activeDice.Count == 0 && lockedDice.Count == 0)
        {
            SpawnDice();
        }

        foreach(Dice dice in activeDice)
        {
            dice.Roll();
        }

        isCheckingRollStatus = true;
    }

    public void ReRollActiveDice()
    {
        if (rollSFX != null && activeDice.Count < 3)
        {
            AudioManager.Instance.PlaySFX(rollSFX);
        }

        if (isCheckingRollStatus) return;

        if (activeDice.Count == 0)
        {
            Debug.Log("<color=orange>DiceManager: Semua dadu sudah di-LOCK, tidak bisa Re-Roll!</color>");
            return;
        }

        if (currentRollCount < maxRolls)
        {
            currentRollCount++;
            Debug.Log($"<color=green>DiceManager: Re-Roll! ini lemparan ke-{currentRollCount} dari {maxRolls}. Roll ulang {activeDice.Count} dadu di tray...</color>");

            RollAllDice();
        }
        else
        {
            Debug.Log("<color=red>DiceManager: Jatah Re-Roll sudah habis (Max 3 lemparan)!");
        }
    }

    private void Update()
    {
        if (isCheckingRollStatus)
        {
            bool allStopped = true;

            for (int i = activeDice.Count - 1; i >= 0; i--)
            {
                Dice dice = activeDice[i];

                if (dice == null)
                {
                    activeDice.RemoveAt(i);
                    continue;
                }

                if (dice.isRolling)
                {
                    allStopped = false;
                }
            }

            if (allStopped)
            {
                isCheckingRollStatus = false;
                Debug.Log("DiceManager: Semua dadu sudah berhenti!");

                OnAllDiceStopped?.Invoke();
            }
        }
    }

    public void ClearActiveDice()
    {
        activeDice.Clear();
    }

    public void CleanUpDiceForNextTurn()
    {
        Debug.Log("DiceManager: Membersihkan tray untuk gilliran berikutnya...");

        foreach (Dice dice in activeDice) { if (dice != null) Destroy(dice.gameObject); }
        foreach (Dice dice in lockedDice) { if (dice != null) Destroy(dice.gameObject); }

        activeDice.Clear();
        lockedDice.Clear();
        currentRollCount = 0;

        if (DiceUIManager.Instance != null)
        {
            DiceUIManager.Instance.ClearLockedDice();
        }
    }
}
