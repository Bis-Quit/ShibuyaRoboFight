using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PassiveCardManager : MonoBehaviour
{
    public static PassiveCardManager Instance;

    [Header("Referensi UI")]
    public PlayerHand playerHand;
    public Transform activeTrapContainer;
    public GameObject trapIconPrefab;

    [Header("Daftar CCTV Aktif")]
    public List<CardData> activePlayerPassives = new List<CardData>();
    public List<CardData> activeEnemyPassives = new List<CardData>();
    
    private Dictionary<CardData, GameObject> trapUI_Dictionary = new Dictionary<CardData, GameObject>();

    private struct QueuedPassive
    {
        public CardData card;
        public bool skipBuzzTile;
        public HandCardUI handUIObject; 
        public bool isPlayerCard;
    }
    
    private Queue<QueuedPassive> passiveQueue = new Queue<QueuedPassive>();
    private bool isProcessingQueue = false;

    private void Awake() { Instance = this; }

    private void OnEnable()
    {
        RobotStats.OnAnyRobotHealed += HandleHeal;
        RobotStats.OnAnyRobotDamaged += HandleDamage;
        RobotStats.OnAnyRobotEnergyAdded += HandleEnergyAdd;
        RobotStats.OnAnyRobotEnergyLost += HandleEnergyLose;
        DraftingManager.OnAbilityCardBought += HandleCardBought;
        TurnManager.OnPlayerTurnChanged += HandleTurnChange;
    }

    private void OnDisable()
    {
        RobotStats.OnAnyRobotHealed -= HandleHeal;
        RobotStats.OnAnyRobotDamaged -= HandleDamage;
        RobotStats.OnAnyRobotEnergyAdded -= HandleEnergyAdd;
        RobotStats.OnAnyRobotEnergyLost -= HandleEnergyLose;
        DraftingManager.OnAbilityCardBought -= HandleCardBought;
        TurnManager.OnPlayerTurnChanged -= HandleTurnChange;
    }

    private void HandleHeal(RobotStats robot, int amount) => CheckPassives(CardData.GameState.HealthPoint, CardData.OperativeAction.Add, robot, amount);
    private void HandleDamage(RobotStats robot, int amount) => CheckPassives(CardData.GameState.HealthPoint, CardData.OperativeAction.Subtract, robot, amount);
    private void HandleEnergyAdd(RobotStats robot, int amount) => CheckPassives(CardData.GameState.AbilityPoint, CardData.OperativeAction.Add, robot, amount);
    private void HandleEnergyLose(RobotStats robot, int amount) => CheckPassives(CardData.GameState.AbilityPoint, CardData.OperativeAction.Subtract, robot, amount);
    private void HandleTurnChange(int playerIndex)
    {
        RobotStats subject = (playerIndex == 0) ? CardEffectManager.Instance.playerStats : CardEffectManager.Instance.enemyStats;
        CheckPassives(CardData.GameState.Turn, CardData.OperativeAction.Start, subject, 0);
    }
    private void HandleCardBought(int playerIdx)
    {
        RobotStats triggerRobot = (playerIdx == 0) ? CardEffectManager.Instance.playerStats : CardEffectManager.Instance.enemyStats;
        CheckPassives(CardData.GameState.BuyCard, CardData.OperativeAction.Add, triggerRobot, 1);
    }

    public void RegisterPassiveCard(CardData card, bool isPlayerOwner)
    {
        if (isPlayerOwner)
        {
            if (!activePlayerPassives.Contains(card))
            {
                activePlayerPassives.Add(card);
                Debug.Log($"<color=magenta>CCTV PLAYER DIPASANG DI ARENA: {card.cardName}</color>");

                if (activeTrapContainer != null && trapIconPrefab != null)
                {
                    GameObject newIcon = Instantiate(trapIconPrefab, activeTrapContainer);
                    Image iconImg = newIcon.GetComponent<Image>();
                    if (iconImg != null) iconImg.sprite = card.cardIllustration; 

                    trapUI_Dictionary.Add(card, newIcon);
                    newIcon.transform.localScale = Vector3.zero;
                    newIcon.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
                }
            }
        }
        else 
        {
            if (!activeEnemyPassives.Contains(card))
            {
                activeEnemyPassives.Add(card);
                Debug.Log($"<color=red>CCTV MUSUH DIPASANG DI ARENA: {card.cardName}</color>");
            }
        }
    }

    private bool IsConditionMet(CardData card, CardData.GameState triggerState, CardData.OperativeAction triggerAction, CardData.TargetSubject whoDidIt, int amountValue)
    {
        if (card.conditionState != triggerState || card.operativeCondition != triggerAction) return false;
        if (card.conditionSubject != whoDidIt) return false;

        if (card.conditionType == CardData.ConditionTrigger.If)
        {
            if (card.comprativeCondition == CardData.Comprative.Equals)
                return (amountValue == card.conditionValue) || (card.conditionState == CardData.GameState.Turn); 
            else if (card.comprativeCondition == CardData.Comprative.MoreThan)
                return (amountValue > card.conditionValue);
        }
        return false;
    }

    private CardData.TargetSubject GetRelativeSubject(bool isPlayerOwner, RobotStats triggerRobot)
    {
        if (isPlayerOwner)
            return (triggerRobot == CardEffectManager.Instance.playerStats) ? CardData.TargetSubject.Self : CardData.TargetSubject.Opponent;
        else
            return (triggerRobot == CardEffectManager.Instance.enemyStats) ? CardData.TargetSubject.Self : CardData.TargetSubject.Opponent;
    }

    private void CheckPassives(CardData.GameState triggerState, CardData.OperativeAction triggerAction, RobotStats triggerRobot, int amountValue)
    {
        if (CardEffectManager.Instance == null) return;

        // ================== SCAN PLAYER HAND (PERMANENT STAY DI TANGAN) ==================
        CardData.TargetSubject playerPOV = GetRelativeSubject(true, triggerRobot);
        
        if (playerHand != null && playerHand.cardsInHand.Count > 0)
        {
            for (int i = playerHand.cardsInHand.Count - 1; i >= 0; i--)
            {
                HandCardUI handUI = playerHand.cardsInHand[i].GetComponent<HandCardUI>();
                if (handUI == null || handUI.cardData == null) continue;

                if (handUI.cardData.cardCategory == CardData.CardCategory.Permanent && 
                    IsConditionMet(handUI.cardData, triggerState, triggerAction, playerPOV, amountValue))
                {
                    EnqueuePassive(handUI.cardData, false, handUI, true);
                }
            }
        }

        // ================== SCAN PLAYER CCTV ==================
        if (activePlayerPassives.Count > 0)
        {
            for (int i = activePlayerPassives.Count - 1; i >= 0; i--)
            {
                if (IsConditionMet(activePlayerPassives[i], triggerState, triggerAction, playerPOV, amountValue))
                {
                    EnqueuePassive(activePlayerPassives[i], true, null, true);
                }
            }
        }

        // ================== SCAN ENEMY HAND (PERMANENT STAY) ==================
        CardData.TargetSubject enemyPOV = GetRelativeSubject(false, triggerRobot);

        if (EnemyCardContainer.Instance != null && EnemyCardContainer.Instance.currentHand.Count > 0)
        {
            for (int i = EnemyCardContainer.Instance.currentHand.Count - 1; i >= 0; i--)
            {
                CardData enemyHandCard = EnemyCardContainer.Instance.currentHand[i];
                if (enemyHandCard.cardCategory == CardData.CardCategory.Permanent && 
                    IsConditionMet(enemyHandCard, triggerState, triggerAction, enemyPOV, amountValue))
                {
                    Debug.Log($"<color=red>ENEMY HAND TRAP TRIGGERED (STAY): {enemyHandCard.cardName}</color>");
                    EnqueuePassive(enemyHandCard, false, null, false);
                }
            }
        }

        if (activeEnemyPassives.Count > 0)
        {
            for (int i = activeEnemyPassives.Count - 1; i >= 0; i--)
            {
                if (IsConditionMet(activeEnemyPassives[i], triggerState, triggerAction, enemyPOV, amountValue))
                {
                    Debug.Log($"<color=red>ENEMY CCTV RE-ACTIVATED: {activeEnemyPassives[i].cardName}</color>");
                    EnqueuePassive(activeEnemyPassives[i], true, null, false);
                }
            }
        }
    }

    private void EnqueuePassive(CardData cardToPlay, bool skipTile, HandCardUI uiObject, bool isPlayerOwner)
    {
        passiveQueue.Enqueue(new QueuedPassive {
            card = cardToPlay,
            skipBuzzTile = skipTile,
            handUIObject = uiObject,
            isPlayerCard = isPlayerOwner
        });

        if (!isProcessingQueue) StartCoroutine(ProcessPassiveQueue());
    }

    private IEnumerator ProcessPassiveQueue()
    {
        isProcessingQueue = true;

        while (passiveQueue.Count > 0)
        {
            yield return new WaitUntil(() => !CardEffectManager.Instance.isResolvingEffect);

            QueuedPassive task = passiveQueue.Dequeue();

            if (task.handUIObject != null)
            {
                task.handUIObject.transform.DOKill(true);
                task.handUIObject.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), 0.5f, 5, 1f);
            }

            if (task.skipBuzzTile && trapUI_Dictionary.ContainsKey(task.card))
            {
                GameObject iconObj = trapUI_Dictionary[task.card];
                if (iconObj != null)
                {
                    iconObj.transform.DOKill(true);
                    iconObj.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), 0.5f, 5, 1f);
                }
            }

            int forcedOwnerID = task.isPlayerCard ? 0 : 1;
            
            yield return StartCoroutine(CardEffectManager.Instance.ApplyCardEffect(task.card, task.skipBuzzTile, forcedOwnerID));
        }

        isProcessingQueue = false;
    }
}