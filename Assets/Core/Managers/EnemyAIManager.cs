using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAIManager : MonoBehaviour
{
    private int aiDifficultyLevel; 

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
                StartCoroutine(SkipDraftingRoutine());
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
            Debug.Log($"<color=magenta>EnemyAIManager: Enemy is thinking... (Difficulty Level: {aiDifficultyLevel})</color>");
            yield return new WaitForSeconds(2f);

            List<Dice> diceOnTray = new List<Dice>(DiceManager.Instance.activeDice);

            foreach (Dice dice in diceOnTray)
            {
                bool shouldLock = false;

                if (aiDifficultyLevel == 0)
                {
                    if (dice.CurrentFace == DiceFace.Energy) shouldLock = true;
                }
                else if (aiDifficultyLevel == 1)
                {
                    if (dice.CurrentFace == DiceFace.Smash || dice.CurrentFace == DiceFace.Heal) shouldLock = true;
                }
                else if (aiDifficultyLevel >= 2)
                {
                    if (dice.CurrentFace == DiceFace.Smash || dice.CurrentFace == DiceFace.Destruction)
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
                Debug.Log("<color=magenta>EnemyAIManager: Enemy memutuskan untuk re-roll beberapa dadu!</color>");
                DiceManager.Instance.ReRollActiveDice();

                yield return new WaitUntil(() => AllDiceStopped());
            }
            else
            {
                Debug.Log("<color=magenta>EnemyAIManager: Enemy memutuskan untuk Resolve...</color>");
                TurnManager.Instance.ProcessedToResolution();

                keepThinking = false;
            }
        }
    }

    private IEnumerator SkipDraftingRoutine()
    {
        Debug.Log("<color=magenta>EnemyAIManager: Enemy memutuskan untuk Skip beli kartu!</color>");
        yield return new WaitForSeconds(1.5f);
        TurnManager.Instance.ProcessedToTurnEnd();
    }

    private bool AllDiceStopped()
    {
        foreach (Dice dice in DiceManager.Instance.activeDice)
        {
            if (dice != null && dice.isRolling)
            {
                return false;
            }
        }
        return true;
    }
}

