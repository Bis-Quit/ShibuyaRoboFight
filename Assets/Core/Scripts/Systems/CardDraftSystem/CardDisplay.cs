using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("Card Data")]
    public CardData cardData;
    public int slotIndex;

    [Header("Component 3D")]
    [SerializeField] private SpriteRenderer frontImage; 
    [SerializeField] private SpriteRenderer backImage;

    public void SetupCard(CardData newData, int index)
    {
        cardData = newData;
        slotIndex = index;

        if (cardData != null && cardData.cardIllustration != null && frontImage != null)
        {
            frontImage.sprite = cardData.cardIllustration;
        }
    }

    private void OnMouseDown()
    {
        if (DraftingManager.Instance != null)
        {
            DraftingManager.Instance.OpenMarketUI();
        }
    }
}