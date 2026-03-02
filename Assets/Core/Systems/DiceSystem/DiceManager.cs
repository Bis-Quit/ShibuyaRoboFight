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
        Debug.Log("DiceManager: Memunculkan 6 dadu di udara Shibuya...");

        for(int i = 0; i < numberOfDice; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 1.5f;
            randomOffset.y = 0;

            Vector3 finalSpawnPos = spawnPoint.position + randomOffset;

            GameObject newDiceObj = Instantiate(dicePrefab, finalSpawnPos, Random.rotation);
            Dice diceScript = newDiceObj.GetComponent<Dice>();

            activeDice.Add(diceScript);
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
        Vector3 interactionPosition = Vector3.zero;
        bool isInteracting = false;

        if (Input.GetMouseButtonDown(0))
        {
            interactionPosition = Input.mousePosition;
            isInteracting = true;
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            interactionPosition = Input.GetTouch(0).position;
            isInteracting = true;
        }

        if (isInteracting)
        {
            Ray ray = Camera.main.ScreenPointToRay(interactionPosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Dice clickedDice = hit.collider.GetComponent<Dice>();
                if (clickedDice != null)
                {
                    clickedDice.ToggleLock();
                }
            }
        }

        if (isCheckingRollStatus)
        {
            bool allStopped = true;

            foreach (Dice dice in activeDice)
            {
                if (dice.isRolling)
                {
                    allStopped = false;
                    break;
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
}
