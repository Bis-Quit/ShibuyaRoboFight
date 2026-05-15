using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerHand : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject handCardPrefab;
    [SerializeField] private Transform handContainer;

    [Header("Fan Settings")]
    public float spacing = 120f;
    public float fanArc = 8f;
    public float heightCurve = 15f;
    public float animDuration = 0.43f;

    [Header("Hover Settings")]
    public float hoveredScale = 1.15f;
    public float hoveredFocusOffset = 40f;
    public float hoveredYOffset = 30f;

    [Header("Selected Setting")]
    public float selectedYOffset = 150f;
    public float selectedScale = 1.3f;
    public int selectedIndex = -1;

    public List<RectTransform> cardsInHand = new List<RectTransform>();

    public void AddCard(CardData newCardData)
    {
        GameObject newCardObj = Instantiate(handCardPrefab, handContainer);
        
        HandCardUI uiScript = newCardObj.GetComponent<HandCardUI>();
        if (uiScript != null)
        {
            uiScript.Setup(newCardData);
            uiScript.isHand = true; 
        }

        RectTransform cardRT = newCardObj.GetComponent<RectTransform>();

        cardRT.localPosition = Vector3.zero; 
        cardRT.localScale = Vector3.one * 0.5f; 
        cardsInHand.Add(cardRT);
        RearrangeHand();
    }

    [ContextMenu("Test Add Cart")]
    public void AddCardTest()
    {
        GameObject newCardObj = Instantiate(handCardPrefab, handContainer);
        RectTransform cardRT = newCardObj.GetComponent<RectTransform>();
        cardsInHand.Add(cardRT);
        RearrangeHand();
    }

    public void RearrangeHand(int hoveredIndex = -1)
    {
        cardsInHand.RemoveAll(card => card == null);
        int count = cardsInHand.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            if (cardsInHand == null) return;

            float centerOffset = (i - (count - 1) / 2f);
            float posX = centerOffset * spacing;
            float posY = Mathf.Abs(centerOffset) * -heightCurve;
            float rotZ = centerOffset * -fanArc;

            float targetScale = 1f;
            float targetYOffset = 0f;

            if (hoveredIndex != -1 && selectedIndex == -1)
            {
                if (i < hoveredIndex)
                {
                    posX -= hoveredFocusOffset;
                }
                else if (i > hoveredIndex)
                {
                    posX += hoveredFocusOffset;
                }
                else
                {
                    targetScale = hoveredScale;
                    targetYOffset = hoveredYOffset;
                }
            }

            if (i == selectedIndex)
            {
                targetYOffset = selectedYOffset;
                rotZ = 0f;
                targetScale = selectedScale;
                posX = 0f;
            }

            cardsInHand[i].DOKill();
            cardsInHand[i].DOAnchorPos(new Vector2(posX, posY + targetYOffset), animDuration).SetEase(Ease.OutSine);
            cardsInHand[i].DORotate(new Vector3(0,0,rotZ), animDuration).SetEase(Ease.OutSine);
            cardsInHand[i].DOScale(targetScale, animDuration).SetEase(Ease.OutSine);

            if (i == hoveredIndex || i == selectedIndex)
            {
                cardsInHand[i].SetAsLastSibling();
            }
        }
    }

    public void SelectedCardFromChild(RectTransform cardRT)
    {
        int newSelectedIndex = cardsInHand.IndexOf(cardRT);

        if (selectedIndex == newSelectedIndex)
        {
            selectedIndex = -1;
        }
        else
        {
            selectedIndex = newSelectedIndex;
        }

        RearrangeHand();
    }

    public void RemoveCardFromHand(GameObject cardObj)
    {
        RectTransform rt = cardObj.GetComponent<RectTransform>();

        if (cardsInHand.Remove(rt))
        {
            cardsInHand.Remove(rt);
            RearrangeHand();
        }
    }
}