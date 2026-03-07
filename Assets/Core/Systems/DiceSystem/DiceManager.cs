using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }

    [Header("Dice Setting")]
    public GameObject dicePrefab;
    public Transform spawnPoint;
    public int numberOfDice = 6;

    private List<Dice> activeDice = new List<Dice>();
    private bool isCheckingRollStatus = false;

    public static event Action OnAllDiceStopped;

    private void Awake()
    {
        TurnManager.OnPhaseChanged += HandleChanged; 

        if (Instance == null) Instance = this;
        else Destroy (gameObject);
    }

    /*private void Start()
    {
        TurnManager.OnPhaseChanged += HandleChanged; 
    }
    */

    private void OnDestroy()
    {
        TurnManager.OnPhaseChanged -= HandleChanged; 
    }

    private void HandleChanged(TurnManager.TurnPhase phase)
    {
        if (phase == TurnManager.TurnPhase.FirstRoll)
        {
            if (activeDice.Count == 0)
            {
                SpawnDice();
            }

            RollAllDice();
        }
    }

    private void SpawnDice()
    {
        Debug.Log($"DiceManager: Memunculkan {numberOfDice} dadu di udara Shibuya...");

        for(int i = 0; i < numberOfDice; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 2f;
            randomOffset.y = MathF.Abs(randomOffset.y);

            Vector3 finalSpawnPos = spawnPoint.position + randomOffset;

            GameObject newDiceObj = Instantiate(dicePrefab, finalSpawnPos, Random.rotation);

            Dice diceScript = newDiceObj.GetComponent<Dice>();
            if (diceScript != null)
            {
                activeDice.Add(diceScript);
            }
        }
    }

    private void RollAllDice()
    {
        Debug.Log("DiceManager: Melempar SEMUA dadu!...");

        foreach(Dice dice in activeDice)
        {
            dice.Roll();
        }

        isCheckingRollStatus = true;
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
}
