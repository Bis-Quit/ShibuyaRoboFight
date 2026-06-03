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
    private PlayerHand parentHand;
    public bool isHand = false;
    public int marketSlotIndex = -1;

    [Header("Audio SFX")]
    public AudioClip pickCardSFX;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        parentHand = GetComponentInParent<PlayerHand>();
    }

    public void Setup(CardData data)
    {
        if (data == null) return;
        cardData = data;
        if (cardImage != null) cardImage.sprite = data.cardIllustration;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (pickCardSFX != null) AudioManager.Instance.PlaySFX(pickCardSFX);

        if (cardData == null) return;

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

        if (isHand)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (HandInspectManager.Instance != null)
                {
                    HandInspectManager.Instance.OpenInspect(cardData, this.gameObject);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                InspectManager.Instance.ShowCardPopup(cardData);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isHand || parentHand == null) return; 

        if (parentHand == null) parentHand = GetComponentInParent<PlayerHand>();
        if (parentHand == null) return;

        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        int myIndex = parentHand.cardsInHand.IndexOf(rectTransform);
        parentHand.RearrangeHand(myIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHand || parentHand == null) return;
        parentHand.RearrangeHand(-1);
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
        if (hand != null)
        {
            hand.cardsInHand.Remove(rectTransform);
            hand.RearrangeHand();
        }
        Destroy(this.gameObject);
    }
}