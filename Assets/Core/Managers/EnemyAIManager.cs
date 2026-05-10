using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAIManager : MonoBehaviour
{
    private int aiDifficultyLevel; 

    [Header("AI Context (Wajib Isi)")]
    public RobotStats aiStats;
    public RobotStats playerStats;

    private void Start() 
    {
        aiDifficultyLevel = PlayerPrefs.GetInt("EnemyDifficulty", 3);
        Debug.Log($"EnemyAIManager: Enemy difficulty set to level {aiDifficultyLevel}.");
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
        if (TurnManager.Instance.CurrentPlayerIndex == 1)
        {
            if (phase == TurnManager.TurnPhase.FirstRoll)
            {
                StartCoroutine(AutoRollRoutine());
            }
            else if (phase == TurnManager.TurnPhase.RerollPhase)
            {
                StartCoroutine(ThinkAndReRollRoutine());
            }
            else if (phase == TurnManager.TurnPhase.CardDrafting)
            {
                StartCoroutine(EnemyDraftingRoutine());
            }
        }
    }

    private IEnumerator AutoRollRoutine()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("<color=magenta>EnemyAIManager: Enemy rolls dice!</color>");
        DiceManager.Instance.RollAllDice();
    }

    private IEnumerator ThinkAndReRollRoutine()
    {
        bool keepThinking = true;

        while (keepThinking)
        {
            Debug.Log($"<color=magenta>EnemyAIManager: Enemy is thinking... (Diff: {aiDifficultyLevel})</color>");
            yield return new WaitForSeconds(2f);

            List<Dice> diceOnTray = new List<Dice>(DiceManager.Instance.activeDice);

            float aiHpPercent = 1f;
            float playerHpPercent = 1f;

            if (aiStats != null && aiStats.baseData != null)
                aiHpPercent = (float)aiStats.currentHP / aiStats.baseData.maxHealth;
            
            if (playerStats != null && playerStats.baseData != null)
                playerHpPercent = (float)playerStats.currentHP / playerStats.baseData.maxHealth;

            foreach (Dice dice in diceOnTray)
            {
                bool shouldLock = false;

                if (aiDifficultyLevel == 0) 
                {
                    if (Random.Range(0, 100) < 50) 
                    {
                        shouldLock = Random.value > 0.5f;
                    }
                    else 
                    {
                        if (dice.CurrentFace == DiceFace.Energy || dice.CurrentFace == DiceFace.Heal) 
                        {
                            shouldLock = true;
                        }
                    }
                }
                else
                {
                    if (aiHpPercent <= 0.3f && dice.CurrentFace == DiceFace.Heal)
                    {
                        shouldLock = true;
                    }
                    else if (playerHpPercent <= 0.3f && dice.CurrentFace == DiceFace.Smash)
                    {
                        shouldLock = true;
                    }
                    else if (aiStats != null && aiStats.currentEnergy < 2 && dice.CurrentFace == DiceFace.Energy)
                    {
                        shouldLock = true;
                    }
                    else if (dice.CurrentFace == DiceFace.Smash || dice.CurrentFace == DiceFace.Destruction)
                    {
                        shouldLock = true;
                    }
                }

                if (shouldLock)
                {
                    DiceManager.Instance.LockDice(dice);
                    yield return new WaitForSeconds(0.5f);
                }
            }

            yield return new WaitForSeconds(1f);
            if (DiceManager.Instance.activeDice.Count > 0 && DiceManager.Instance.currentRollCount < DiceManager.Instance.maxRolls)
            {
                Debug.Log("<color=magenta>EnemyAIManager: Enemy memutuskan untuk re-roll!</color>");
                DiceManager.Instance.ReRollActiveDice();
                yield return new WaitUntil(() => AllDiceStopped());
            }
            else
            {
                Debug.Log("<color=magenta>EnemyAIManager: Enemy selesai mikir -> Resolve!</color>");
                TurnManager.Instance.ProcessedToResolution();
                keepThinking = false;
            }
        }
    }

    private IEnumerator EnemyDraftingRoutine()
    {
        Debug.Log("<color=magenta>EnemyAIManager: Enemy masuk ke Market...</color>");
        yield return new WaitForSeconds(1.5f);

        if (DraftingManager.Instance != null && aiStats != null)
        {
            bool isBuying = DraftingManager.Instance.EnemyTryBuyCard(aiStats);
            
            if (isBuying)
            {
                yield break; 
            }
            else 
            {
                if (aiStats.currentEnergy > 0) 
                {
                    DraftingManager.Instance.StartCoroutine(DraftingManager.Instance.EnemyTryResetAndBuy(aiStats));
                    yield break; 
                }
            }
        }

        Debug.Log("<color=magenta>EnemyAIManager: Uang benar-benar habis. Skip belanja.</color>");
        yield return new WaitForSeconds(1.5f);
        TurnManager.Instance.ProcessedToTurnEnd();
    }

    private bool AllDiceStopped()
    {
        foreach (Dice dice in DiceManager.Instance.activeDice)
        {
            if (dice != null && dice.isRolling) return false;
        }
        return true;
    }
}