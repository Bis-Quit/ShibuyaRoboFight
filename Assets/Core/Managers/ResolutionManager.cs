using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    [Header("Robot References")]
    public RobotStats playerStats;
    public RobotStats enemyStats;

    private void OnEnable()
    {
        TurnManager.OnPhaseChanged += HandlePhaseChange;
    }

    private void OnDisable ()
    {
        TurnManager.OnPhaseChanged -= HandlePhaseChange;
    }

    private void HandlePhaseChange(TurnManager.TurnPhase phase)
    {
        if (phase == TurnManager.TurnPhase.Resolution)
        {
            StartCoroutine(ResolveDiceRoutine());
        }
    }

    private IEnumerator ResolveDiceRoutine()
    {
        yield return new WaitForSeconds(1f);

        List<Dice> finalDicePool = new List<Dice>();
        finalDicePool.AddRange(DiceManager.Instance.activeDice);
        finalDicePool.AddRange(DiceManager.Instance.lockedDice);

        if (finalDicePool.Count == 0)
        {
            Debug.Log("<color=yellow>ResolutionManager: Tidak ada dadu untuk di-resolve!</color>");
            TurnManager.Instance.ProcessedToDrafting();
            yield break;
        }

        Debug.Log($"<color=magenta>--- MEMULAI RESOLVE PHASE ({finalDicePool.Count} Dadu) ---</color>");

        RobotStats currentAttacker;
        RobotStats currentDefender;

        if (TurnManager.Instance.CurrentPlayerIndex == 0)
        {
            currentAttacker = playerStats;
            currentDefender = enemyStats;
        }
        else
        {
            currentAttacker = enemyStats;
            currentDefender = playerStats;
        }

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

        // -- SMASH --
        if (diceCounts[DiceFace.Smash] > 0)
        {
            int count = diceCounts[DiceFace.Smash];
            Debug.Log($"[3] ATTACK: Menyerang musuh dengan {count} Damage!");
            if (currentDefender != null) currentDefender.TakeDamage(count);
            yield return new WaitForSeconds(0.5f);
        }

        // -- HEAL --
        if (diceCounts[DiceFace.Heal] > 0)
        {
            int count = diceCounts[DiceFace.Heal];
            Debug.Log($"[2] HEAL: Player dipulihkan sebanyak {count * 2} HP.");
            if (currentAttacker != null) currentAttacker.Heal(count * 2);
            yield return new WaitForSeconds(0.5f);
        }

        // -- ENERGY --
        if (diceCounts[DiceFace.Energy] > 0)
        {
            int count = diceCounts[DiceFace.Energy];
            Debug.Log($"[4] ENERGY: Menambah {count} Energy!");
            if (currentAttacker != null) currentAttacker.AddEnergy(count);
            yield return new WaitForSeconds(0.5f);
        }

        // -- DESTRUCTION --
        int destructCount = diceCounts[DiceFace.Destruction];
        if (destructCount > 0)
        {
            if (destructCount >= 3)
            {
                int destructPoints = 1 + (destructCount - 3); // RUMUS PATEN

                Debug.Log($"[5] DESTRUCT: KOMBO AKTIF! Menarik Destruct Token sebanyak {destructPoints} poin.");
                TugOfWarManager.Instance.MoveDestruction(destructPoints, TurnManager.Instance.CurrentPlayerIndex);
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.Log($"[5] DESTRUCT: Gagal kombo. (Dapat {destructCount} dadu, butuh minimal 3).");
            }
        }

        // -- FAME --
        int fameCount = diceCounts[DiceFace.Fame];
        if (fameCount > 0)
        {
            if (fameCount >= 3)
            {
                int famePoints = 1 + (fameCount - 3);

                Debug.Log($"[6] FAME: KOMBO AKTIF! Menarik Fame Token sebanyak {famePoints} poin.");
                TugOfWarManager.Instance.MoveFame(famePoints, TurnManager.Instance.CurrentPlayerIndex);
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.Log($"[6] FAME: Gagal kombo. (Dapat {fameCount} dadu, butuh minimal 3).");
            }
        }

        // -- SPECIAL POWER --
        int skillCount = diceCounts[DiceFace.SpecialPower];
        int energyCount = diceCounts[DiceFace.Energy];

        if (skillCount > 0)
        {
            currentAttacker.AddSkillPower(skillCount);
            yield return new WaitForSeconds(0.5f);
        }

        if (currentAttacker.currentSkillPower > 0 || energyCount > 0)
        {
            currentAttacker.CheckAndExecuteSkill(energyCount, currentDefender);
            Debug.Log($"[1] SPECIAL SKILL: Cek aktivasi Ultimate Character...");
            yield return new WaitForSeconds(1f);
        }

        TurnManager.Instance.ProcessedToDrafting();
    }
}