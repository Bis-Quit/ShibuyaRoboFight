using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class EnemyCardContainer : MonoBehaviour
{
    public static EnemyCardContainer Instance { get; private set; }

    [Header("Enemy Card Container")]
    public List<CardData> currentHand = new List<CardData>();
    public int maxHandSize = 5;

    [Header("Visual Options")]
    public CanvasGroup handCanvasGroup;
    public Transform enemyHandUIContainer;
    
    [Tooltip("Tarik Prefab 'UI Hand Card Small' lu ke sini!")]
    public GameObject cardUIPrefab;

    private List<GameObject> visualCards = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        if (handCanvasGroup != null)
        {
            SetHandVisible(false, 0f);
        }
    }

    public void SetHandVisible(bool isVisible, float duration = 0.2f)
    {
        if (handCanvasGroup != null)
        {
            handCanvasGroup.DOFade(isVisible ? 1f : 0f, duration);
            handCanvasGroup.interactable = isVisible;
            handCanvasGroup.blocksRaycasts = isVisible;
        }
    }

    public void ReceiveCard(CardData newCard)
    {
        if (currentHand.Count >= maxHandSize) return;

        currentHand.Add(newCard);
        Debug.Log($"<color=green>[EnemyCardContainer]</color> Tangkap! Visual kartu {newCard.cardName} sedang di-spawn!");

        if (cardUIPrefab != null && enemyHandUIContainer != null)
        {
            GameObject visualCard = Instantiate(cardUIPrefab, enemyHandUIContainer);
            visualCards.Add(visualCard);

            visualCard.transform.localScale = Vector3.one; 
            visualCard.transform.localPosition = new Vector3(visualCard.transform.localPosition.x, visualCard.transform.localPosition.y, 0f);

            HandCardUI cardUI = visualCard.GetComponent<HandCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(newCard);
                cardUI.enabled = false;
            }

            CanvasGroup cg = visualCard.GetComponent<CanvasGroup>();
            if (cg == null) cg = visualCard.AddComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;

            float randomRotation = Random.Range(-5f, 5f);
            visualCard.transform.localRotation = Quaternion.Euler(0, 0, randomRotation);

            SetHandVisible(true, 0.2f); 
        }
        else
        {
            Debug.LogError("<color=red>[EnemyCardContainer]</color> VISUAL GAGAL SPAWN! Kolom Prefab atau Container di Inspector MASIH KOSONG!");
        }
    }

    public void PlayCard(CardData cardToPlay)
    {
        if (currentHand.Contains(cardToPlay))
        {
            currentHand.Remove(cardToPlay);

            if (visualCards.Count > 0)
            {
                GameObject cardToDestroy = visualCards[visualCards.Count - 1];
                visualCards.Remove(cardToDestroy);
                Destroy(cardToDestroy);
            }
        }
        Debug.Log("<color=red>[Enemy AI]</color> Musuh menggunakan kartu: " + cardToPlay.cardName);

        StartCoroutine(CardEffectManager.Instance.ApplyCardEffect(cardToPlay));
    }

    public void ClearHand()
    {
        currentHand.Clear();
        foreach (var vCard in visualCards)
        {
            Destroy(vCard);
        }
        visualCards.Clear();
    }
}