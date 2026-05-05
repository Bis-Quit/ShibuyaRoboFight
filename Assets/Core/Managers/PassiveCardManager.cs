using System.Collections.Generic;
using UnityEngine;

public class PassiveCardManager : MonoBehaviour
{
    public static PassiveCardManager Instance;

    [Header("Daftar CCTV Aktif (Kartu Permanent Player)")]
    public List<CardData> activePlayerPassives = new List<CardData>();

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("<color=red>⚠️ [CHEAT AKTIF] Maksa musuh nge-Heal 4 HP buat ngetes CCTV!</color>");
            
            CardEffectManager.Instance.enemyStats.Heal(4); 
        }
    }
    public void RegisterPassiveCard(CardData card)
    {
        if (!activePlayerPassives.Contains(card))
        {
            activePlayerPassives.Add(card);
            Debug.Log($"<color=magenta>👁️ CCTV PASIF DIPASANG: {card.cardName}</color>");
        }
    }
    private void OnEnable()
    {
        RobotStats.OnAnyRobotHealed += (robot, amount) => CheckPassives(CardData.GameState.HealthPoint, CardData.OperativeAction.Add, robot, amount);
        RobotStats.OnAnyRobotDamaged += (robot, amount) => CheckPassives(CardData.GameState.HealthPoint, CardData.OperativeAction.Subtract, robot, amount);
        RobotStats.OnAnyRobotEnergyAdded += (robot, amount) => CheckPassives(CardData.GameState.AbilityPoint, CardData.OperativeAction.Add, robot, amount);
        RobotStats.OnAnyRobotEnergyLost += (robot, amount) => CheckPassives(CardData.GameState.AbilityPoint, CardData.OperativeAction.Subtract, robot, amount);
        
        TurnManager.OnPlayerTurnChanged += (playerIndex) => 
        {
            RobotStats subject = (playerIndex == 0) ? CardEffectManager.Instance.playerStats : CardEffectManager.Instance.enemyStats;
            CheckPassives(CardData.GameState.Turn, CardData.OperativeAction.Start, subject, 0);
        };
    }

    private void OnDisable()
    {
        
    }

    private void CheckPassives(CardData.GameState triggerState, CardData.OperativeAction triggerAction, RobotStats triggerRobot, int amountValue)
    {
        if (activePlayerPassives.Count == 0) return;

        CardData.TargetSubject whoDidIt = (triggerRobot == CardEffectManager.Instance.playerStats) ? CardData.TargetSubject.Self : CardData.TargetSubject.Opponent;

        for (int i = activePlayerPassives.Count - 1; i >= 0; i--)
        {
            CardData card = activePlayerPassives[i];

            if (card.conditionState != triggerState || card.operativeCondition != triggerAction) continue;

            if (card.conditionSubject != whoDidIt) continue;

            bool isConditionMet = false;
            
            if (card.conditionType == CardData.ConditionTrigger.If)
            {
                if (card.comprativeCondition == CardData.Comprative.Equals)
                    isConditionMet = (amountValue == card.conditionValue) || (card.conditionState == CardData.GameState.Turn); 
                else if (card.comprativeCondition == CardData.Comprative.MoreThan)
                    isConditionMet = (amountValue > card.conditionValue);
            }

            if (isConditionMet)
            {
                Debug.Log($"<color=magenta>✨ TRAP CARD AKTIF! Syarat {card.cardName} terpenuhi! Mengeksekusi efek...</color>");
                CardEffectManager.Instance.ApplyCardEffect(card);
            }
        }
    }
}