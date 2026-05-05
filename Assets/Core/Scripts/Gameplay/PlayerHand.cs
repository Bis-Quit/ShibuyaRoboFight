using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject handCardPrefab;
    [SerializeField] private Transform handContainer;

    [Header("Data Hand")]
    public List<CardData> cardsInHand = new List<CardData>();
    public void AddCard(CardData newCard)
{
    cardsInHand.Add(newCard);
    GameObject cardObj = Instantiate(handCardPrefab, handContainer);
    
    HandCardUI display = cardObj.GetComponent<HandCardUI>();
    if (display != null)
    {
        display.Setup(newCard);
        display.isHand = true;
    }
}
}