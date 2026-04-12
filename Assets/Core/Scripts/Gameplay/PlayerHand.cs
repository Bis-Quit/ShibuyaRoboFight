using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerHand : MonoBehaviour
{
    [Header("Card in Player Hand")]
    public List<CardData> myCard = new List<CardData>();

    [Header("UI Setup")]
    public Transform handPanel;
    public GameObject cardUIPrefab;

    public void AddCard(CardData selectedCard)
    {
        myCard.Add(selectedCard);

        GameObject newUICard = Instantiate(cardUIPrefab, handPanel);
        
        newUICard.transform.localScale = Vector3.one; 

        Image cardImage = newUICard.GetComponent<Image>();
        if (cardImage != null && selectedCard.cardIllustration != null)
        {
            cardImage.sprite = selectedCard.cardIllustration;
        }

        Debug.Log(" Visual kartu berhasil dipajang di layar: " + selectedCard.cardName);
    }
}