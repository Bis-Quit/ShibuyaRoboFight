using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftingManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private List<CardData> masterCardDatabase;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<Transform> draftSlots;
    [SerializeField] private Transform deckPosition;
    private bool isDrafting = false;

    [Header("Animation Settings")]
    [SerializeField] private float slideSpeed = 8f;

    // State management
    private List<CardData> currentCardPool = new List<CardData>();
    private GameObject[] activeCardsOnBoard = new GameObject[3];

    private void Awake()
    {
        InitializeDraftPool();
    }

    public void InitializeDraftPool()
    {
        currentCardPool = new List<CardData>(masterCardDatabase);
        ShuffleDeck(currentCardPool);
    }

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

    public void GenerateDraft()
    {
        ClearCurrentDraft();
        for (int i = 0; i < 3; i++)
        {
            SpawnCardAtSlot(i);
        }
    }

    private void SpawnCardAtSlot(int slotIndex)
    {
        if (currentCardPool.Count == 0) return;

        CardData drawnCard = currentCardPool[0];
        currentCardPool.RemoveAt(0);

        GameObject newCard = Instantiate(cardPrefab, deckPosition.position, deckPosition.rotation);
        activeCardsOnBoard[slotIndex] = newCard;

        if (newCard.TryGetComponent(out CardDisplay cardDisplay))
        {
            cardDisplay.SetupCard(drawnCard, slotIndex);
        }

        StartCoroutine(AnimateCardSlide(newCard.transform, draftSlots[slotIndex]));
    }

    public void OnCardClicked(int slotIndex, GameObject cardObject, CardData data)
    {
        if (isDrafting) return;

        StartCoroutine(DraftingSequence(slotIndex, cardObject, data));
    }

    IEnumerator DraftingSequence(int slotIndex, GameObject cardObject, CardData data)
    {
        FindFirstObjectByType<PlayerHand>().AddCard(data);
        Destroy(cardObject);
        activeCardsOnBoard[slotIndex] = null;
        yield return null;

        for (int i = slotIndex; i < 2; i++)
        {
            if (activeCardsOnBoard[i + 1] != null)
            {
                activeCardsOnBoard[i] = activeCardsOnBoard[i + 1];
                activeCardsOnBoard[i].GetComponent<CardDisplay>().slotIndex = i;

                StartCoroutine(AnimateCardSlide(activeCardsOnBoard[i].transform, draftSlots[i]));
                activeCardsOnBoard[i + 1] = null;
            }
        }
        yield return new WaitForSeconds(0.3f);
        SpawnCardAtSlot(2);
        yield return new WaitForSeconds(0.5f);
        isDrafting = false;
    }

    private void ShiftCards(int emptySlotIndex)
    {
        for (int i = emptySlotIndex; i < 2; i++)
        {
            if (activeCardsOnBoard[i + 1] != null)
            {
                activeCardsOnBoard[i] = activeCardsOnBoard[i + 1];
                activeCardsOnBoard[i].GetComponent<CardDisplay>().slotIndex = i;
                
                StartCoroutine(AnimateCardSlide(activeCardsOnBoard[i].transform, draftSlots[i]));
                
                activeCardsOnBoard[i + 1] = null;
            }
        }

        SpawnCardAtSlot(2);
    }

    public void ClearCurrentDraft()
    {
        for (int i = 0; i < activeCardsOnBoard.Length; i++)
        {
            if (activeCardsOnBoard[i] != null) Destroy(activeCardsOnBoard[i]);
            activeCardsOnBoard[i] = null;
        }
    }

    private IEnumerator AnimateCardSlide(Transform cardTransform, Transform targetSlot)
    {
        Vector3 startPos = cardTransform.position;
        Quaternion startRot = cardTransform.rotation;
        
        float duration = 0.4f;
        float timeElapsed = 0f;
        float hopHeight = 0.6f;
        
        while (timeElapsed < duration)
        {
            if (cardTransform == null) break;
            
            float t = timeElapsed / duration;
            float easeT = t * (2f - t); 
            
            Vector3 currentPos = Vector3.Lerp(startPos, targetSlot.position, easeT);
            
            currentPos.y += Mathf.Sin(t * Mathf.PI) * hopHeight;
            
            cardTransform.position = currentPos;
            cardTransform.rotation = Quaternion.Lerp(startRot, targetSlot.rotation, easeT);
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        if (cardTransform != null) 
        {
            cardTransform.position = targetSlot.position;
            cardTransform.rotation = targetSlot.rotation; 
        }
    }

    public void SkipBuyCard()
    {
        if (isDrafting) return;

        Debug.Log("Player memilih untuk skip membeli kartu!");
        ClearCurrentDraft();
    }

    public void ResetPool()
    {
        if (isDrafting) return;

        Debug.Log("Player mereset pool kartu!");
        GenerateDraft();
    }
}