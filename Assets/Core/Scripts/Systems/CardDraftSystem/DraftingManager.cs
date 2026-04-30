using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraftingManager : MonoBehaviour
{
    public static DraftingManager Instance;

    [Header("Dependencies 3D")]
    [SerializeField] private List<CardData> masterCardDatabase;
    [SerializeField] private GameObject card3DPrefab;
    [SerializeField] private List<Transform> draftSlots;
    [SerializeField] private Transform deckPosition;

    [Header("Dependencies 2D")]
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private Transform marketGridContainer;
    [SerializeField] private GameObject draftingUIPanel;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button resetButton;

    [Header("UI Panels")]
    [SerializeField] private GameObject inspectPanel;
    [SerializeField] private Image inspectImage;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeInspectButton;
    private bool isProcessing = false;
    private List<CardData> currentCardPool = new List<CardData>();

    private CardData[] activeCardData = new CardData[3];
    private GameObject[] activeCardsOnBoard = new GameObject[3];
    private CardData selectedCardData;
    private int selectedSlotIndex;

    [Header("Player References")]
    public RobotStats playerStats;
    public PlayerHand playerHand;

    private void Awake()
    {
        Instance = this;
        InitializeDraftPool();

        if (skipButton != null) skipButton.onClick.AddListener(SkipBuyCard);
        if (resetButton != null) resetButton.onClick.AddListener(ResetPool);
        if (closeInspectButton != null) closeInspectButton.onClick.AddListener(CloseInspectPanel);
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
            if (draftingUIPanel != null) draftingUIPanel.SetActive(false);

            bool isPlayeTurn = (TurnManager.Instance.CurrentPlayerIndex == 0);
            if (skipButton != null) skipButton.interactable = isPlayeTurn;
            if (resetButton != null) resetButton.interactable = isPlayeTurn;
            isProcessing = !isPlayeTurn;

            if (!isPlayeTurn)
            {
                StartCoroutine(EnemyAutoSkipDrafting());
                Debug.Log("<color=yellow>Musuh melewati Toko Kartu...</color>");
                return;
            }

            bool isMarketEmpty = true;
            for (int i = 0; i < activeCardsOnBoard.Length; i++)
            {
                if (activeCardsOnBoard[i] != null) isMarketEmpty = false;
            }

            if (isMarketEmpty) GenerateDraft3D();
        }
        else
        {
            CloseAllUI();
        }
    }

    private IEnumerator EnemyAutoSkipDrafting()
    {
        yield return new WaitForSeconds(1.5f);
        TurnManager.Instance.ProcessedToTurnEnd();
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

    public void GenerateDraft3D()
    {
        ClearCurrentDraft3D();
        for (int i = 0; i < draftSlots.Count; i++)
        {
            if (currentCardPool.Count == 0) InitializeDraftPool();

            CardData drawnCard = currentCardPool[0];
            currentCardPool.RemoveAt(0);

            activeCardData[i] = drawnCard;

            GameObject newCard3D = Instantiate(card3DPrefab, deckPosition.position, deckPosition.rotation);
            activeCardsOnBoard[i] = newCard3D;

            CardDisplay display = newCard3D.GetComponent<CardDisplay>();
            if (display != null) display.SetupCard(drawnCard, i);

            StartCoroutine(AnimateCardSlide(newCard3D.transform, draftSlots[i]));
        }
    }

    public void ClearCurrentDraft3D()
    {
        for (int i = 0; i < activeCardsOnBoard.Length; i++)
        {
            if (activeCardsOnBoard[i] != null) Destroy(activeCardsOnBoard[i]);
            activeCardsOnBoard[i] = null;
            activeCardData[i] = null;
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

    public void OpenMarketUI()
    {
        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) return;
        if (TurnManager.Instance.CurrentPlayerIndex != 0) return;

        if (draftingUIPanel != null) draftingUIPanel.SetActive(true);
        PopulateMarketUI();
    }

    private void PopulateMarketUI()
    {
        foreach (Transform child in marketGridContainer) Destroy(child.gameObject);
        
        for (int i = 0; i < activeCardData.Length; i++)
        {
            if (activeCardData[i] == null) continue;

            CardData data = activeCardData[i];
            int slotIndex = i;

            GameObject newUICard = Instantiate(cardUIPrefab, marketGridContainer);

            // 1. SETUP GAMBAR
            HandCardDisplay display = newUICard.GetComponent<HandCardDisplay>();
            if (display != null)
            {
                display.Setup(data); 
                Destroy(display); 
            }
            else
            {
                Image cardImg = newUICard.GetComponent<Image>();
                if (cardImg != null && data.cardIllustration != null) 
                {
                    cardImg.sprite = data.cardIllustration;
                }
            }

            Button cardBtn = newUICard.GetComponent<Button>();
            if (cardBtn != null) 
            {
                cardBtn.onClick.RemoveAllListeners(); 

                cardBtn.onClick.AddListener(() => OpenInspectPanel(data, slotIndex));
            }
        }
    }

    public void OpenInspectPanel(CardData data, int slotIndex)
    {
        if (isProcessing) return;

        selectedCardData = data;
        selectedSlotIndex = slotIndex;
        if (inspectPanel != null) inspectPanel.SetActive(true);

        if (inspectImage != null) inspectImage.sprite = data.cardIllustration;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(ConfirmPurchase);
    }

    private void CloseInspectPanel()
    {
        if (inspectPanel != null)inspectPanel.SetActive(false);
        selectedCardData = null;
    }

    private void ConfirmPurchase()
    {
        if (isProcessing || selectedCardData == null) return;
        StartCoroutine(DraftingSequence());
    }

    IEnumerator DraftingSequence()
    {
        isProcessing = true;

        if (playerStats == null)
        {
            Debug.LogError("<color=red>DraftingManager: PlayerStats Kosong!</color>");
            isProcessing = false;
            yield break;
        }

        if (playerStats.SpendEnergy(selectedCardData.abilityPointCost))
        {
            if (playerHand != null) playerHand.AddCard(selectedCardData);

            if (activeCardsOnBoard[selectedSlotIndex] != null)
            {
                Destroy(activeCardsOnBoard[selectedSlotIndex]);
                activeCardsOnBoard[selectedSlotIndex] = null;
                activeCardData[selectedSlotIndex] = null;
            }
            yield return new WaitForSeconds(0.3f);

            isProcessing = false;
            CloseAllUI();
            TurnManager.Instance.ProcessedToTurnEnd();
        }
        else
        {
            Debug.Log("<color=orange>Energy tidak cukup!</color>");
            isProcessing = false;
        }
    }

    private void SkipBuyCard()
    {
        if (isProcessing || TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) return;
        CloseAllUI();
        TurnManager.Instance.ProcessedToTurnEnd();
    }

    private void ResetPool()
    {
        if (isProcessing || TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) return;
        GenerateDraft3D();
        PopulateMarketUI();
    }

    public void CloseAllUI()
    {
        if (draftingUIPanel != null) draftingUIPanel.SetActive(false);
        if (inspectPanel != null) inspectPanel.SetActive(false);
    }
}