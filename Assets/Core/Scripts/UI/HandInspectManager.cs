using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HandInspectManager : MonoBehaviour
{
    public static HandInspectManager Instance;

    [Header("UI Reference")]
    public GameObject inspectPanel;
    public HandCardUI giantCardUI;
    public Button useButton;
    public Button cancelButton;

    private CardData currentCardData;
    private GameObject originalCardObject;

    private void Awake()
    {
        Instance = this;
        cancelButton.onClick.AddListener(CloseInspect);
        useButton.onClick.AddListener(ConfirmUseCard);

        if (inspectPanel != null) inspectPanel.SetActive(false);
    }

    public void OpenInspect(CardData data, GameObject originalObj)
    {
        if (inspectPanel.activeSelf && originalCardObject != null)
        {
            originalCardObject.SetActive(true);
        }

        currentCardData = data;
        originalCardObject = originalObj;

        giantCardUI.Setup(data);
        giantCardUI.isHand = false;

        bool isMyTurn = TurnManager.Instance.CurrentPlayerIndex == 0;
        bool isInstant = data.cardCategory == CardData.CardCategory.Instant;

        useButton.interactable = (isMyTurn && isInstant);

        inspectPanel.SetActive(true);
        // originalCardObject.SetActive(false);
    }

    public void CloseInspect()
    {
        inspectPanel.SetActive(false);

        if (originalCardObject != null)
        {
            originalCardObject.SetActive(true);
        }
    }

    private void ConfirmUseCard()
    {
        if (CardEffectManager.Instance != null)
        {
            CardEffectManager.Instance.ApplyCardEffect(currentCardData);
        }

        if (originalCardObject != null)
        {
            PlayerHand hand = originalCardObject.GetComponentInParent<PlayerHand>();
            if (hand != null)
            {
                hand.RemoveCardFromHand(originalCardObject);
            }
            Destroy(originalCardObject);
        }
        inspectPanel.SetActive(false);
    }
}