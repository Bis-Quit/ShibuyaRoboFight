using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftingManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private List<CardData> masterCardDatabase;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<Transform> draftSlots;
    [SerializeField] private Transform deckPosition;

    [Header("UI Panels & Buttons")]
    [SerializeField] private GameObject draftingUIPanel;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button resetButton;
    private bool isProcessing = false;
    private List<CardData> currentCardPool = new List<CardData>();
    private GameObject[] activeCardsOnBoard = new GameObject[3];

    [Header("Animation Settings")]
    [SerializeField] private float slideSpeed = 8f;

    [Header("Player References")]
    public RobotStats playerStats;
    public PlayerHand playerHand;

    private void Awake()
    {
        InitializeDraftPool();
    }

    private void OnEnable()
    {
        TurnManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        TurnManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(TurnManager.TurnPhase phase)
    {
        if (phase == TurnManager.TurnPhase.CardDrafting)
        {
            if (draftingUIPanel != null) draftingUIPanel.SetActive(true);

            bool isPlayeTurn = (TurnManager.Instance.CurrentPlayerIndex == 0);

            SetDraftingUIInteractable(isPlayeTurn);

            bool isMarketEmpty = true;
            foreach (var card in activeCardsOnBoard)
            {
                if (card != null) isMarketEmpty = false;
            }

            if (isMarketEmpty)
            {
                Debug.Log("DraftingManager: Memulai fase drafting. Menyiapkan kartu untuk Open Market...");
                GenerateDraft();
            }
        }
        else
        {
            if (draftingUIPanel != null) draftingUIPanel.SetActive(false);
        }
    }

    private void SetDraftingUIInteractable(bool isPlayerTurn)
    {
        if (skipButton != null) skipButton.interactable = isPlayerTurn;
        if (resetButton != null) resetButton.interactable = isPlayerTurn;

        isProcessing = !isPlayerTurn;
    }

    public void InitializeDraftPool()
    {
        currentCardPool = new List<CardData>(masterCardDatabase);
        ShuffleDeck(currentCardPool);
    }

    private void ShuffleDeck(List<CardData> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void GenerateDraft()
    {
        ClearCurrentDraft();
        for (int i = 0; i < 3; i++)
        {
            SpawnCardAtSlot(i);
        }
    }

    private void SpawnCardAtSlot(int slotIndex)
    {
        if (currentCardPool.Count == 0) InitializeDraftPool();

        CardData drawnCard = currentCardPool[0];
        currentCardPool.RemoveAt(0);

        GameObject newCard = Instantiate(cardPrefab, deckPosition.position, deckPosition.rotation);
        activeCardsOnBoard[slotIndex] = newCard;

        if (newCard.TryGetComponent(out CardDisplay cardDisplay))
        {
            cardDisplay.SetupCard(drawnCard, slotIndex);
        }

        StartCoroutine(AnimateCardSlide(newCard.transform, draftSlots[slotIndex]));
    }

    public void OnCardClicked(int slotIndex, GameObject cardObject, CardData data)
    {
        if (isProcessing) return;

        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) return;
        if (TurnManager.Instance.CurrentPlayerIndex != 0) return;

        StartCoroutine(DraftingSequence(slotIndex, cardObject, data));
    }

    IEnumerator DraftingSequence(int slotIndex, GameObject cardObject, CardData data)
    {
        isProcessing = true;

        if (playerStats.SpendEnergy(data.abilityPointCost))
        {
            if (playerHand != null)
            {
                playerHand.AddCard(data);
                Debug.Log($"<color=green>Berhasil beli kartu: {data.cardName}</color>");
            }
            else
            {
                Debug.LogWarning($"<color=red>[{gameObject.name}] PlayerHand belum dimasukan!</color>");
            }

            Destroy(cardObject);
            activeCardsOnBoard[slotIndex] = null;

            yield return new WaitForSeconds(0.3f);

            isProcessing = false;

            TurnManager.Instance.ProcessedToTurnEnd();
        }
        else
        {
            Debug.Log("<color=orange>DraftingManager: Energy tidak cukup untuk beli kartu ini!</color>");
            isProcessing = false;
        }
    }

    private void SkipBuyCard()
    {
        if (isProcessing || TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) return;

        Debug.Log("DraftingManager: Player memilih untuk Skip beli kartu.");
        TurnManager.Instance.ProcessedToTurnEnd();
    }

    public void ResetPool()
    {
        if (isProcessing || TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) return;

        Debug.Log("Player mereset pool kartu!");
        GenerateDraft();
    }

    private IEnumerator EnemyAutoSkip()
    {
        Debug.Log("DraftingManager: Enemy Memikirkan langkahnya...");
        yield return new WaitForSeconds(1.5f);
        Debug.Log("DraftingManager: Enemy Memilih untuk Skip beli kartu.");
        TurnManager.Instance.ProcessedToTurnEnd();
    } 

    public void ClearCurrentDraft()
    {
        for (int i = 0; i < activeCardsOnBoard.Length; i++)
        {
            if (activeCardsOnBoard[i] != null) Destroy(activeCardsOnBoard[i]);
            activeCardsOnBoard[i] = null;
        }
    }

    private IEnumerator AnimateCardSlide(Transform cardTransform, Transform targetSlot)
    {
        Vector3 startPos = cardTransform.position;
        Quaternion startRot = cardTransform.rotation;
        
        float duration = 0.4f;
        float timeElapsed = 0f;
        float hopHeight = 0.6f;
        
        while (timeElapsed < duration)
        {
            if (cardTransform == null) break;
            
            float t = timeElapsed / duration;
            float easeT = t * (2f - t); 
            
            Vector3 currentPos = Vector3.Lerp(startPos, targetSlot.position, easeT);
            
            currentPos.y += Mathf.Sin(t * Mathf.PI) * hopHeight;
            
            cardTransform.position = currentPos;
            cardTransform.rotation = Quaternion.Lerp(startRot, targetSlot.rotation, easeT);
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        if (cardTransform != null) 
        {
            cardTransform.position = targetSlot.position;
            cardTransform.rotation = targetSlot.rotation; 
        }
    }
}