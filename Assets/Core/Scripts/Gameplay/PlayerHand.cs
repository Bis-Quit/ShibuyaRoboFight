using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject handCardPrefab; // Prefab kartu kecil
    [SerializeField] private Transform handContainer;   // Objek Player_Hand_Container

    [Header("Data Hand")]
    // INI DIA VARIABEL YANG DICARI SAMA UNITY (cardsInHand)
    public List<CardData> cardsInHand = new List<CardData>();

    // INI DIA FUNGSI YANG DICARI SAMA DRAFTING MANAGER (AddCard)
    public void AddCard(CardData newCard)
    {
        // 1. Masukin datanya ke list tangan kita
        cardsInHand.Add(newCard);

        // 2. Spawn UI visualnya ke layar
        GameObject cardObj = Instantiate(handCardPrefab, handContainer);
        
        // 3. Setup gambar dan tombolnya
        HandCardDisplay display = cardObj.GetComponent<HandCardDisplay>();
        if (display != null)
        {
            display.Setup(newCard);
        }

        Debug.Log($"<color=green>Hand: Berhasil masukin kartu {newCard.cardName} ke tangan!</color>");
    }
}