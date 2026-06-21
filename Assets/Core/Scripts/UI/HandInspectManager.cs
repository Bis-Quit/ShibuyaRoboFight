using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class HandInspectManager : MonoBehaviour
{
    public static HandInspectManager Instance;

    [Header("UI Reference")]
    public GameObject inspectPanel;
    public HandCardUI giantCardUI;
    public Button useButton;
    public Button cancelButton;

    [Header("Cinematic Skill UI")]
    public CanvasGroup cinematicContainer;
    public RectTransform cinematicCardPosition;
    public HandCardUI cinematicCardUI;
    public Text cinematicSkillText;

    private CardData currentCardData;
    private GameObject originalCardObject;

    private void Awake()
    {
        Instance = this;
        cancelButton.onClick.AddListener(CloseInspect);
        useButton.onClick.AddListener(ConfirmUseCard);

        if (inspectPanel != null) inspectPanel.SetActive(false);
        if (cinematicContainer != null) cinematicContainer.gameObject.SetActive(false);
    }

    public void OpenInspect(CardData data, GameObject originalObj)
    {
        if (inspectPanel.activeSelf && originalCardObject != null)
        {
            originalCardObject.SetActive(true);
        }

        currentCardData = data;
        originalCardObject = originalObj;

        giantCardUI.Setup(data);
        giantCardUI.isHand = false;

        bool isMyTurn = TurnManager.Instance.CurrentPlayerIndex == 0;
        bool isInstant = data.cardCategory == CardData.CardCategory.Instant;

        useButton.gameObject.SetActive(isInstant); 
        
        useButton.interactable = isMyTurn; 

        if (BattleUIManager.Instance != null) BattleUIManager.Instance.HideMarketIndicator();

        inspectPanel.SetActive(true);
    }

    public void CloseInspect()
    {
        inspectPanel.SetActive(false);

        if (originalCardObject != null)
        {
            originalCardObject.SetActive(true);
        }

        if (BattleUIManager.Instance != null && TurnManager.Instance != null)
        {
            bool isDraftingPhase = TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.CardDrafting;
            bool isPlayerTurn = TurnManager.Instance.CurrentPlayerIndex == 0;

            if (BattleUIManager.Instance.marketClickIndicator != null)
            {
                BattleUIManager.Instance.marketClickIndicator.SetActive(isDraftingPhase && isPlayerTurn);
            }
        }
    }

    private void ConfirmUseCard()
    {
        if (CardEffectManager.Instance != null && CardEffectManager.Instance.isResolvingEffect) return;

        if (CardEffectManager.Instance != null) CardEffectManager.Instance.isResolvingEffect = true;

        inspectPanel.SetActive(false);

        if (originalCardObject != null)
        {
            originalCardObject.SetActive(false);

            PlayerHand hand = originalCardObject.GetComponentInParent<PlayerHand>();
            if (hand != null)
            {
                hand.cardsInHand.Remove(originalCardObject.GetComponent<RectTransform>());
                hand.RearrangeHand();
            }
        }

        StartCoroutine(CinematicCutInRoutine(currentCardData, originalCardObject));
    }

    private IEnumerator CinematicCutInRoutine(CardData currentCardData, GameObject originalCardObject)
    {
        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.HideUIForCinematic();

        yield return new WaitForSeconds(0.2f);

        // --- CARD SUMMON CINEMATIC ---
        if (cinematicContainer != null && cinematicCardUI != null)
        {
            cinematicCardUI.Setup(currentCardData);
            cinematicCardUI.isHand = false;
            if (cinematicSkillText != null) cinematicSkillText.text = currentCardData.cardName;

            cinematicCardPosition.localScale = Vector3.zero;
            cinematicCardPosition.localRotation = Quaternion.Euler(0, 0, -45f); 
            cinematicContainer.alpha = 0;
            cinematicContainer.gameObject.SetActive(true);

            cinematicContainer.DOFade(1, 0.2f);
            cinematicCardPosition.DOScale(0.8f, 0.4f).SetEase(Ease.OutBack);
            cinematicCardPosition.DOLocalRotate(Vector3.zero, 0.4f).SetEase(Ease.OutBack);
            cinematicCardPosition.DOShakeAnchorPos(0.2f, 15f);

            yield return new WaitForSeconds(1.5f);

            cinematicCardPosition.DOScale(0f, 0.3f).SetEase(Ease.InBack);
            cinematicContainer.DOFade(0, 0.3f);

            yield return new WaitForSeconds(0.4f); 

            cinematicContainer.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(1f);

        // --- CARD EFFECT ---
        if (CardEffectManager.Instance != null)
        {
            yield return StartCoroutine(CardEffectManager.Instance.ApplyCardEffect(currentCardData));
        }

        if (originalCardObject != null)
        {
            Destroy(originalCardObject);
        }

        // --- CAMERA & UI BACK TO NORMAL ---
        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.ResetCamera();

        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.RestoreUIAfterCinematic();
    }
}