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
    [SerializeField] private RectTransform handTargetUI;

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

    private void OnEnable() { TurnManager.OnPhaseChanged += HandlePhaseChanged; }
    private void OnDisable() { TurnManager.OnPhaseChanged -= HandlePhaseChanged; }

    private void HandlePhaseChanged(TurnManager.TurnPhase phase)
    {
        if (phase == TurnManager.TurnPhase.CardDrafting)
        {
            if (draftingUIPanel != null) draftingUIPanel.SetActive(false);
            bool isPlayeTurn = (TurnManager.Instance.CurrentPlayerIndex == 0);
            if (skipButton != null) skipButton.interactable = isPlayeTurn;
            if (resetButton != null) resetButton.interactable = isPlayeTurn;
            isProcessing = !isPlayeTurn;

            if (!isPlayeTurn) return;

            bool isMarketEmpty = true;
            for (int i = 0; i < activeCardsOnBoard.Length; i++)
            {
                if (activeCardsOnBoard[i] != null) isMarketEmpty = false;
            }
            if (isMarketEmpty) GenerateDraft3D();
        }
        else { CloseAllUI(); }
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
        float duration = 0.4f, timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            if (cardTransform == null) break;
            float t = timeElapsed / duration;
            cardTransform.position = Vector3.Lerp(startPos, targetSlot.position, t * (2f - t)) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.6f;
            cardTransform.rotation = Quaternion.Lerp(startRot, targetSlot.rotation, t * (2f - t));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        if (cardTransform != null) { cardTransform.position = targetSlot.position; cardTransform.rotation = targetSlot.rotation; }
    }

    public void OpenMarketUI()
    {
        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) return;
        if (TurnManager.Instance.CurrentPlayerIndex != 0 || draftingUIPanel.activeSelf || isProcessing) return;
        draftingUIPanel.SetActive(true);
        StartCoroutine(PopulateMarketWithAnimation());
    }

    private IEnumerator PopulateMarketWithAnimation()
    {
        isProcessing = true;
        foreach (Transform child in marketGridContainer) Destroy(child.gameObject);
        activeUICards.Clear();

        for (int i = 0; i < activeCardData.Length; i++)
        {
            if (activeCardData[i] == null) continue;
            CardData data = activeCardData[i];

            GameObject newUICard = Instantiate(cardUIPrefab, marketGridContainer);
            activeUICards.Add(newUICard);

            HandCardUI display = newUICard.GetComponent<HandCardUI>();
            if (display != null)
            {
                display.Setup(data);
                display.isHand = false;
                display.marketSlotIndex = i;
            }

            int index = i;
            Button cardBtn = newUICard.GetComponent<Button>();
            if (cardBtn != null) cardBtn.onClick.AddListener(() => OpenInspectPanel(data, index));

            StartCoroutine(AnimateCardEntrance(newUICard));
            yield return new WaitForSeconds(0.1f);
        }
        isProcessing = false;
    }

    private IEnumerator AnimateCardEntrance(GameObject card)
    {
        CanvasGroup group = card.GetComponent<CanvasGroup>() ?? card.AddComponent<CanvasGroup>();
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero; group.alpha = 0;
        float elapsed = 0, duration = 0.3f;
        while (elapsed < duration)
        {
            if (card == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeOutBack = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
            rect.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easeOutBack);
            group.alpha = t;
            yield return null;
        }
        rect.localScale = Vector3.one; group.alpha = 1;
    }

    public void OpenInspectPanel(CardData data, int slotIndex)
    {
        if (isProcessing) isProcessing = false;
        selectedCardData = data;
        selectedSlotIndex = slotIndex;
        if (inspectPanel != null) inspectPanel.SetActive(true);
        if (inspectImage != null) inspectImage.sprite = data.cardIllustration;
        if (InspectManager.Instance != null) InspectManager.Instance.ShowCardPopup(data);
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(ConfirmPurchase);
    }

    private void CloseInspectPanel() { if (inspectPanel != null) inspectPanel.SetActive(false); selectedCardData = null; }

    private void ConfirmPurchase() { if (!isProcessing && selectedCardData != null) StartCoroutine(DraftingSequence()); }

    private IEnumerator AnimateRefillCard(Transform cardTransform, Transform targetSlot)
    {
        Vector3 startPos = deckPosition.position;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (cardTransform == null) break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            cardTransform.position = Vector3.Lerp(startPos, targetSlot.position, t * t * (3f - 2f * t));
            cardTransform.rotation = Quaternion.Lerp(cardTransform.rotation, targetSlot.rotation, t);
            yield return null;
        }
    }

    IEnumerator DraftingSequence()
    {
        isProcessing = true;

        if (playerStats.SpendEnergy(selectedCardData.abilityPointCost))
        {
            if (inspectPanel != null) inspectPanel.SetActive(false);

            if (activeUICards.Count > selectedSlotIndex && activeUICards[selectedSlotIndex] != null)
            {
                GameObject boughtUICard = activeUICards[selectedSlotIndex];
                boughtUICard.transform.SetParent(draftingUIPanel.transform, true); 
                StartCoroutine(AnimatePurchaseFly(boughtUICard, selectedCardData));
            }

            if (activeCardsOnBoard[selectedSlotIndex] != null) 
                Destroy(activeCardsOnBoard[selectedSlotIndex]);

            for (int i = selectedSlotIndex; i > 0; i--)
            {
                activeCardData[i] = activeCardData[i - 1];
                activeCardsOnBoard[i] = activeCardsOnBoard[i - 1];
                
                if (activeCardsOnBoard[i] != null)
                {
                    CardDisplay disp = activeCardsOnBoard[i].GetComponent<CardDisplay>();
                    if (disp != null) disp.slotIndex = i;
                    StartCoroutine(AnimateCardSlide(activeCardsOnBoard[i].transform, draftSlots[i]));
                }

                activeUICards[i] = activeUICards[i - 1];
                if (activeUICards[i] != null)
                {
                    HandCardUI uiScript = activeUICards[i].GetComponent<HandCardUI>();
                    if (uiScript != null) uiScript.marketSlotIndex = i;

                    Button btn = activeUICards[i].GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        int capturedIndex = i;
                        CardData capturedData = activeCardData[i];
                        btn.onClick.AddListener(() => OpenInspectPanel(capturedData, capturedIndex));
                    }
                }
            }

            if (currentCardPool.Count == 0) InitializeDraftPool();
            CardData newCard = currentCardPool[0];
            currentCardPool.RemoveAt(0);
            activeCardData[0] = newCard;

            GameObject n3D = Instantiate(card3DPrefab, deckPosition.position, deckPosition.rotation);
            activeCardsOnBoard[0] = n3D;
            n3D.GetComponent<CardDisplay>().SetupCard(newCard, 0);
            StartCoroutine(AnimateRefillCard(n3D.transform, draftSlots[0]));

            GameObject newUICard = Instantiate(cardUIPrefab, marketGridContainer);

            newUICard.transform.SetSiblingIndex(0); 
            
            activeUICards[0] = newUICard;

            HandCardUI newUIScript = newUICard.GetComponent<HandCardUI>();
            if (newUIScript != null)
            {
                newUIScript.Setup(newCard);
                newUIScript.isHand = false;
                newUIScript.marketSlotIndex = 0;
            }

            Button newBtn = newUICard.GetComponent<Button>();
            if (newBtn != null) newBtn.onClick.AddListener(() => OpenInspectPanel(newCard, 0));

            StartCoroutine(AnimateCardEntrance(newUICard));

            yield return new WaitForSeconds(0.6f); 

            isProcessing = false;
            CloseAllUI();
            
            TurnManager.Instance.ProcessedToTurnEnd();
        }
        else
        {
            StartCoroutine(ShakeUI(inspectPanel.transform));
            isProcessing = false;
        }
    }

    private IEnumerator AnimatePurchaseFly(GameObject card, CardData dataToAdd)
    {
        RectTransform rect = null;
        Vector3 start = Vector3.zero;
        Vector3 target = handTargetUI != null ? handTargetUI.position : Vector3.zero;

        if (card != null)
        {
            rect = card.GetComponent<RectTransform>();
            start = rect.position;
        }

        float elapsed = 0, duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (card != null && rect != null)
            {
                rect.position = Vector3.Lerp(start, target, elapsed / duration);
                rect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.4f, elapsed / duration);
            }

            yield return null;
        }

        if (playerHand != null && dataToAdd != null) 
        {
            Debug.Log($"<color=cyan>Sukses mendarat! Menambahkan {dataToAdd.cardName} ke tangan.</color>");
            playerHand.AddCard(dataToAdd);
        }
        else
        {
            Debug.LogError("Gagal nambah kartu! PlayerHand atau dataToAdd kosong!");
        }
        if (card != null) Destroy(card); 
    }

    private IEnumerator ShakeUI(Transform ui)
    {
        Vector3 orig = ui.localPosition;
        for (int i = 0; i < 5; i++) {
            ui.localPosition += new Vector3(Random.Range(-15, 15), 0, 0);
            yield return new WaitForSeconds(0.04f); ui.localPosition = orig;
        }
    }

    private void SkipBuyCard() { if (!isProcessing) { CloseAllUI(); TurnManager.Instance.ProcessedToTurnEnd(); } }
    private void ResetPool() { if (!isProcessing) StartCoroutine(AnimateResetAndRefresh()); }

    private IEnumerator AnimateResetAndRefresh()
    {
        isProcessing = true;
        foreach (GameObject c in activeUICards) if (c != null) StartCoroutine(AnimateCardExit(c));
        yield return new WaitForSeconds(0.3f);
        GenerateDraft3D(); yield return StartCoroutine(PopulateMarketWithAnimation());
        isProcessing = false;
    }

    private IEnumerator AnimateCardExit(GameObject c)
    {
        float elapsed = 0, duration = 0.3f;
        while (elapsed < duration) {
            if (c == null) yield break;
            elapsed += Time.deltaTime;
            c.GetComponent<RectTransform>().localPosition += Vector3.right * 1500 * Time.deltaTime;
            yield return null;
        }
        Destroy(c);
    }

    public void CloseAllUI() { if (draftingUIPanel != null) draftingUIPanel.SetActive(false); if (inspectPanel != null) inspectPanel.SetActive(false); }

    public bool EnemyTryBuyCard(RobotStats enemyStats)
    {
        int best = -1, high = -1;
        for (int i = 0; i < activeCardData.Length; i++) {
            if (activeCardData[i] != null && enemyStats.currentEnergy >= activeCardData[i].abilityPointCost && activeCardData[i].abilityPointCost > high) {
                high = activeCardData[i].abilityPointCost; best = i;
            }
        }
        if (best != -1) { StartCoroutine(EnemyDraftingSequence(enemyStats, best)); return true; }
        return false;
    }

    private IEnumerator EnemyDraftingSequence(RobotStats enemyStats, int slotIndex)
    {
        isProcessing = true;
        if (enemyStats.SpendEnergy(activeCardData[slotIndex].abilityPointCost)) 
        {
            CardData boughtCard = activeCardData[slotIndex];
            if (EnemyCardContainer.Instance != null) 
            {
                EnemyCardContainer.Instance.ReceiveCard(boughtCard);
            }

            if (activeCardsOnBoard[slotIndex] != null) Destroy(activeCardsOnBoard[slotIndex]);
            for (int i = slotIndex; i > 0; i--) {
                activeCardData[i] = activeCardData[i - 1]; activeCardsOnBoard[i] = activeCardsOnBoard[i - 1];
                if (activeCardsOnBoard[i] != null) {
                    activeCardsOnBoard[i].GetComponent<CardDisplay>().slotIndex = i;
                    StartCoroutine(AnimateCardSlide(activeCardsOnBoard[i].transform, draftSlots[i]));
                }
            }
            if (currentCardPool.Count == 0) InitializeDraftPool();
            CardData n = currentCardPool[0]; currentCardPool.RemoveAt(0);
            activeCardData[0] = n;
            GameObject n3 = Instantiate(card3DPrefab, deckPosition.position, deckPosition.rotation);
            activeCardsOnBoard[0] = n3; n3.GetComponent<CardDisplay>().SetupCard(n, 0);
            StartCoroutine(AnimateCardSlide(n3.transform, draftSlots[0]));
            yield return new WaitForSeconds(1.5f);
        }
        if (EnemyAIManager.Instance != null) 
        {
        EnemyAIManager.Instance.StartCoroutine(EnemyAIManager.Instance.EnemyActionRoutine());
        }
    }

    public IEnumerator EnemyTryResetAndBuy(RobotStats enemyStats)
    {
        isProcessing = true; ClearCurrentDraft3D(); yield return new WaitForSeconds(0.3f);
        GenerateDraft3D(); yield return new WaitForSeconds(0.8f); isProcessing = false;
        if (!EnemyTryBuyCard(enemyStats)) { yield return new WaitForSeconds(1.5f); TurnManager.Instance.ProcessedToTurnEnd(); }
    }
}