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
    [SerializeField] private RectTransform handTargetUI; // Tadi lu nulisnya ReactTransform

    [Header("UI Panels")]
    [SerializeField] private GameObject inspectPanel;
    [SerializeField] private Image inspectImage;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeInspectButton;
    
    private bool isProcessing = false;
    private List<CardData> currentCardPool = new List<CardData>();

    private CardData[] activeCardData = new CardData[3];
    private GameObject[] activeCardsOnBoard = new GameObject[3];
    private List<GameObject> activeUICards = new List<GameObject>();

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

        if (draftingUIPanel != null && draftingUIPanel.activeSelf) return;
        if (isProcessing) return; 

        if (draftingUIPanel != null) draftingUIPanel.SetActive(true);
        StartCoroutine(PopulateMarketWithAnimation());
    }

    private IEnumerator PopulateMarketWithAnimation()
    {
        isProcessing = true;
        
        foreach (Transform child in marketGridContainer)
        {
            Destroy(child.gameObject);
        }
        activeUICards.Clear();

        for (int i = 0; i < activeCardData.Length; i++)
        {
            if (activeCardData[i] == null) continue;

            CardData data = activeCardData[i];
            int slotIndex = i;

            GameObject newUICard = Instantiate(cardUIPrefab, marketGridContainer);
            activeUICards.Add(newUICard);

            HandCardDisplay display = newUICard.GetComponent<HandCardDisplay>();
            if (display != null)
            {
                display.Setup(data); 
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

            StartCoroutine(AnimateCardEntrance(newUICard));
            yield return new WaitForSeconds(0.1f); 
        }

        isProcessing = false;
    }

    private IEnumerator AnimateCardEntrance(GameObject card)
    {
        if (card == null) yield break; 

        CanvasGroup group = card.GetComponent<CanvasGroup>();
        if (group == null) group = card.AddComponent<CanvasGroup>();

        RectTransform rect = card.GetComponent<RectTransform>();
        
        rect.localScale = Vector3.zero; 
        group.alpha = 0;

        float elapsed = 0;
        float duration = 0.3f; 
        
        while (elapsed < duration)
        {
            if (card == null || rect == null) yield break; 

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easeOutBack = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            rect.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easeOutBack);
            group.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        
        if (card != null && rect != null) 
        {
            rect.localScale = Vector3.one;
            group.alpha = 1;
        }
    }

    private void ResetPool()
    {
        if (isProcessing || TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) return;
        StartCoroutine(AnimateResetAndRefresh());
    }

    private IEnumerator AnimateResetAndRefresh()
    {
        isProcessing = true;

        List<GameObject> cardsToDestroy = new List<GameObject>(activeUICards);
        
        activeUICards.Clear(); 

        foreach (GameObject uiCard in cardsToDestroy)
        {
            if (uiCard != null) StartCoroutine(AnimateCardExit(uiCard));
        }

        yield return new WaitForSeconds(0.3f);
        
        GenerateDraft3D(); 
        yield return StartCoroutine(PopulateMarketWithAnimation());
        isProcessing = false;
    }

    private IEnumerator AnimateCardExit(GameObject card)
    {
        if (card == null) yield break;
        CanvasGroup group = card.GetComponent<CanvasGroup>();
        if (group == null) group = card.AddComponent<CanvasGroup>();

        RectTransform rect = card.GetComponent<RectTransform>();
        
        card.transform.SetParent(draftingUIPanel.transform); 

        float elapsed = 0;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            if (card == null) yield break;

            elapsed += Time.deltaTime;
            rect.localPosition += new Vector3(1500 * Time.deltaTime, 0, 0); 
            group.alpha -= Time.deltaTime * 3;
            yield return null;
        }
        
        if (card != null) Destroy(card);
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
            if(inspectPanel != null) inspectPanel.SetActive(false);

            if (activeUICards.Count > selectedSlotIndex && activeUICards[selectedSlotIndex] != null)
            {
                yield return StartCoroutine(AnimatePurchaseFly(activeUICards[selectedSlotIndex]));
            }

            if (playerHand != null) playerHand.AddCard(selectedCardData);

            if (activeCardsOnBoard[selectedSlotIndex] != null)
            {
                Destroy(activeCardsOnBoard[selectedSlotIndex]);
                activeCardsOnBoard[selectedSlotIndex] = null;
                activeCardData[selectedSlotIndex] = null;
            }

            isProcessing = false;
            CloseAllUI();
            TurnManager.Instance.ProcessedToTurnEnd();
        }
        else
        {
            Debug.Log("<color=orange>Energy tidak cukup!</color>");
            StartCoroutine(ShakeUI(inspectPanel.transform));
            isProcessing = false;
        }
    }

    private IEnumerator AnimatePurchaseFly(GameObject cardToFly)
    {
        cardToFly.transform.SetParent(draftingUIPanel.transform); 
        
        RectTransform rect = cardToFly.GetComponent<RectTransform>();
        Vector3 startPos = rect.position;
        Vector3 targetPos = handTargetUI != null ? handTargetUI.position : startPos;

        float elapsed = 0;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.position = Vector3.Lerp(startPos, targetPos, t);
            rect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.4f, t);
            yield return null;
        }
        
        Destroy(cardToFly);
    }

    private IEnumerator ShakeUI(Transform ui)
    {
        Vector3 originalPos = ui.localPosition;
        for (int i = 0; i < 5; i++)
        {
            ui.localPosition += new Vector3(Random.Range(-15, 15), 0, 0);
            yield return new WaitForSeconds(0.04f);
            ui.localPosition = originalPos;
        }
    }

    private void SkipBuyCard()
    {
        if (isProcessing || TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) return;
        CloseAllUI();
        TurnManager.Instance.ProcessedToTurnEnd();
    }

    public void CloseAllUI()
    {
        if (draftingUIPanel != null) draftingUIPanel.SetActive(false);
        if (inspectPanel != null) inspectPanel.SetActive(false);
    }
}