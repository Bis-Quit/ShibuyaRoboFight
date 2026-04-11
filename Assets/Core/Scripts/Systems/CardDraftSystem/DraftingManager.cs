using System.Collections.Generic;
using UnityEngine;

public class DraftingManager : MonoBehaviour
{
    [Header("Dependency")]
    [SerializeField] private List<CardData> masterCardDatabase;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<Transform> draftSlots;

    private List<CardData> currentCardPool = new List<CardData>();
    private List<GameObject> activeDraftCards = new List<GameObject>();

    private void Awake()
    {
        InitializeDraftPool();
    }

    /// <summary>
    /// Copy data dari master database dan mengacaknya untuk draft.
    /// </summary>

    private void InitializeDraftPool()
    {
        if (masterCardDatabase == null || masterCardDatabase.Count == 0)
        {
            Debug.LogError("[CardDraftingManager] master Card Database is empty!");
            return;
        }

        currentCardPool = new List<CardData>(masterCardDatabase);
        ShuffleDeck(currentCardPool);
    }

    /// <summary>
    /// Fisher-Yates shuffle algorithm untuk mengacak kartu.
    /// </summary>

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

    /// <summary>
    /// Memunculkan kartu di atas arena sesuai jumlah slot yang tersedia.
    /// </summary>

    public void GenerateDraft()
    {
        ClearCurrentDraft();

        int cardToDraw = Mathf.Min(draftSlots.Count, currentCardPool.Count);

        if (cardToDraw == 0)
        {
            Debug.LogWarning("[CardDraftingManager] No cards left in the draft pool!");
            return;
        }
        for (int i = 0; i < cardToDraw; i++)
        {
            CardData drawCard = currentCardPool[0];
            currentCardPool.RemoveAt(0);

            GameObject newCard = Instantiate(cardPrefab, draftSlots[i].position, cardPrefab.transform.rotation);
            activeDraftCards.Add(newCard);

            if (newCard.TryGetComponent(out CardDisplay cardDisplay))
            {
                cardDisplay.SetupCard(drawCard);
            }
            else
            {
                Debug.LogError("[CardDraftingManager] Card prefab is missing CardDisplay component!");
            }
        }
    }
        /// <summary>
    /// Membersihkan kartu yang sedang ditampilkan sebelum memunculkan draft baru.
    /// </summary>

    public void ClearCurrentDraft()
    {
        foreach (GameObject card in activeDraftCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        activeDraftCards.Clear();
    }
}

