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

    private List<Dice> activeDice = new List<Dice>();
    private List<Dice> lockedDice = new List<Dice>();

    [Header("Robot Reference")]
    public RobotStats playerStats;
    public RobotStats enemyStats;

    [Header("Roll Setting")]
    public int maxRolls = 3;
    public int currentRollCount = 0;
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
            currentRollCount = 1;

            if (activeDice.Count == 0 && lockedDice.Count == 0)
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

    private void RollAllDice()
    {
        Debug.Log("DiceManager: Melempar SEMUA dadu!...");

        foreach(Dice dice in activeDice)
        {
            dice.Roll();
        }

        isCheckingRollStatus = true;
    }

    public void ReRollActiveDice()
    {
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

    public void EndTurnAndResolve()
    {
        if (activeDice.Count > 0 && isCheckingRollStatus)
        {
            Debug.Log("<color=yellow>Tunggu dadu berhenti dahulu!</color>");
            return;
        }

        List<Dice> finalDicePool = new List<Dice>();
        finalDicePool.AddRange(lockedDice);
        finalDicePool.AddRange(activeDice);

        if (finalDicePool.Count == 0)
        {
            Debug.Log("<color=red>DiceManager: Tidak ada dadu untuk di-Resolve!</color>");
            return;
        }

        Debug.Log($"<color=magenta>--- MEMULAI RESOLVE PHASE ({finalDicePool.Count} Dadu) ---</color>");

        Dictionary<DiceFace, int> diceCounts = new Dictionary<DiceFace, int>();
        foreach (DiceFace face in Enum.GetValues(typeof(DiceFace)))
        {
            diceCounts[face] = 0;
        }

        foreach (Dice dice in finalDicePool)
        {
            if (dice != null)
            {
                diceCounts[dice.CurrentFace]++;
            }
        }

        if (diceCounts[DiceFace.SpecialPower] > 0)
        {
            int count = diceCounts[DiceFace.SpecialPower];
            Debug.Log($"[1] SPECIAL SKILL: Menambah {count} Skill Point. Menunggu cek aktivasi...");
        }

        if (diceCounts[DiceFace.Heal] > 0)
        {
            int count = diceCounts[DiceFace.Heal];
            Debug.Log($"[2] HEAL: Player dipulihkan sebanyak {count * 2} HP.");

            if (playerStats != null) playerStats.Heal(count * 2);
        }

        if (diceCounts[DiceFace.Smash] > 0)
        {
            int count = diceCounts[DiceFace.Smash];
            Debug.Log($"[3] ATTACK: Menyerang musuh dengan {count} Damage!");

            if (enemyStats != null) enemyStats.TakeDamage(count);
        }

        if (diceCounts[DiceFace.Energy] > 0)
        {
            int count = diceCounts[DiceFace.Energy];
            Debug.Log($"[4] ENERGY: Menambah {count} Ability Point ke Player.");

            if (playerStats != null) playerStats.AddEnergy(count);
        }

        int destructCount = diceCounts[DiceFace.Destruction];
        if (destructCount > 0)
        {
            int destructPoints = 0;
            if (destructCount >= 3)
            {
                destructPoints += 1;

                if (destructCount % 3 > 0)
                {
                    destructPoints += (destructCount % 3);
                }

                Debug.Log($"[5] DESTRUCT: KOMBO AKTIF! Menarik Destruct Token sebanyak {destructPoints} poin.");
            }
            else
            {
                Debug.Log($"[5] DESTRUCT: Gagal kombo. (Dapat {destructCount} dadu, butuh minimal 3).");
            }
        }

        int fameCount = diceCounts[DiceFace.Fame];
        if (fameCount > 0)
        {
            int famePoints = 0;
            if (fameCount >= 3)
            {
                famePoints += 1;

                if (fameCount % 3 > 0)  famePoints += (fameCount % 3);

                Debug.Log($"[6] FAME: KOMBO AKTIF! Menarik Fame Token sebanyak {famePoints} poin.");
            }
            else
            {
                Debug.Log($"[6] FAME: Gagal kombo. (Dapat {fameCount} dadu, butuh minimal 3).");
            }
        }

        Debug.Log("<color=magenta>--- RESOLVE PHASE SELESAI ---</color>");

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
