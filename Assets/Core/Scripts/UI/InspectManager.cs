using UnityEngine;
using DG.Tweening;

public class InspectManager : MonoBehaviour
{
    public static InspectManager Instance;

    [Header("UI References")]
    public CanvasGroup popupCanvasGroup; // Tarik 'Panel Inspect Card' (Root) ke sini
    public RectTransform popupPanel;     // Tarik 'CardContentContainer' ke sini

    private void Awake()
    {
        Instance = this;
        
        popupCanvasGroup.alpha = 0;
        popupCanvasGroup.blocksRaycasts = false;
        popupPanel.localScale = Vector3.zero;
    }

    public void ShowCardPopup(CardData data)
    {
        // Matiin animasi sebelumnya kalau masih jalan
        DOTween.Kill(popupCanvasGroup);
        DOTween.Kill(popupPanel);

        popupPanel.localScale = Vector3.zero;
        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.blocksRaycasts = true;

        popupCanvasGroup.DOFade(1, 0.3f);
        popupPanel.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }
    
    public void ClosePopup()
    {
        DOTween.Kill(popupCanvasGroup);
        DOTween.Kill(popupPanel);

        popupCanvasGroup.blocksRaycasts = false;
        popupCanvasGroup.DOFade(0, 0.2f);
        
        // Diganti ke 0f biar ngecil sampai hilang
        popupPanel.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => 
        {
            if (DraftingManager.Instance != null)
            {
                DraftingManager.Instance.CloseAllUI();
            }
        });
    }
}