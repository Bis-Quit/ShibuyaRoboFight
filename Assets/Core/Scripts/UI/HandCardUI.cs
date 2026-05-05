using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HandCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Card Data")]
    public CardData cardData;
    [SerializeField] private Image cardImage;

    [Header("Hover Settings")]
    public float hoverScale = 1.15f;
    public float animDuration = 0.2f;
    public Vector2 hoverOffset = new Vector2(30f, 0f);

    private Canvas canvas;
    private RectTransform rectTransform;
    public bool isHand = false;
    public int marketSlotIndex = -1;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(CardData data)
    {
        cardData = data;
        if (cardImage != null) cardImage.sprite = data.cardIllustration;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerExit(null); 

        if (!isHand) 
        {
            Debug.Log($"<color=cyan>Market: Membuka Inspect untuk {cardData.cardName}</color>");

            if (DraftingManager.Instance != null)
            {
                DraftingManager.Instance.OpenInspectPanel(cardData, marketSlotIndex); 
            }
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            InspectManager.Instance.ShowCardPopup(cardData);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            PlayCard();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isHand) return; 

        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        rectTransform.DOScale(hoverScale, animDuration).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHand) return;

        canvas.overrideSorting = false;
        canvas.sortingOrder = 0;
        rectTransform.DOScale(1f, animDuration).SetEase(Ease.InBack);
    }

    private void PlayCard()
    {
        if (!isHand) return;

        if (TurnManager.Instance.CurrentPlayerIndex != 0) return;

        if (cardData.cardCategory != CardData.CardCategory.Instant) return;

        if (CardEffectManager.Instance != null)
        {
            CardEffectManager.Instance.ApplyCardEffect(cardData);
        }

        PlayerHand hand = GetComponentInParent<PlayerHand>();
        if (hand != null) hand.cardsInHand.Remove(cardData);

        Destroy(this.gameObject);
    }
}