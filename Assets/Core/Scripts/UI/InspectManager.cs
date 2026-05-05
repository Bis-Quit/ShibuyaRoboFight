using UnityEngine;
using DG.Tweening;

public class InspectManager : MonoBehaviour
{
    public static InspectManager Instance;

    [Header("UI References")]
    public CanvasGroup popupCanvasGroup;
    public RectTransform popupPanel;

    private void Awake()
    {
        Instance = this;
        
        popupCanvasGroup.alpha = 0;
        popupCanvasGroup.blocksRaycasts = false;
        popupPanel.localScale = Vector3.zero;
    }

    public void ShowCardPopup(CardData data)
    {
        popupCanvasGroup.blocksRaycasts = true;
        
        popupCanvasGroup.DOFade(1, 0.3f);
        popupPanel.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }
    public void ClosePopup()
    {
        popupCanvasGroup.blocksRaycasts = false;
        popupCanvasGroup.DOFade(0, 0.2f);
        popupPanel.DOScale(0.5f, 0.2f).SetEase(Ease.InBack).OnComplete(() => 
        {
            if (DraftingManager.Instance != null)
            {
                DraftingManager.Instance.CloseAllUI();
            }
        });
    }
}