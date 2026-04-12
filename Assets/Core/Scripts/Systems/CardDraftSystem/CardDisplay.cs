using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [Header("Card Data")]
    public CardData cardData;
    public int slotIndex;

    [Header("Component UI")]
    [SerializeField] private Image frontImage;
    [SerializeField] private Image backImage;

    public void SetupCard(CardData newData, int index)
    {
        cardData = newData;
        slotIndex = index;

        if (cardData.cardIllustration != null)
        {
            frontImage.sprite = cardData.cardIllustration;
        }
    }

    private void OnMouseDown()
    {
        DraftingManager draftManager = FindFirstObjectByType<DraftingManager>();

        if (draftManager != null)
        {
            draftManager.OnCardClicked(slotIndex, this.gameObject, cardData);
        }
    }
}