using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] private CardData cardData;

    [Header("Component UI")]
    [SerializeField] private Image frontImage;
    [SerializeField] private Image backImage;

    public void SetupCard(CardData newData)
    {
        cardData = newData;

        if (cardData.cardIllustration != null)
        {
            frontImage.sprite = cardData.cardIllustration;
        }
    }
}