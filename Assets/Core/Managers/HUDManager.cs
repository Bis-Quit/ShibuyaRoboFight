using UnityEngine;
using DG.Tweening;

public class HUDManager : MonoBehaviour
{
    [Header("HUD Elements")]
    public RectTransform playerHPBar;
    public RectTransform enemyHPBar;
    public RectTransform trackerTile;

    [Header("Arena Mode (VCam Arena)")]
    public Vector2 playerHPArenaPos;
    public Vector2 enemyHPArenaPos;
    public Vector2 trackerArenaPos;

    [Header("Action Mode")]
    public Vector2 playerHPActionPos;
    public Vector2 enemyHPActionPos;
    public Vector2 trackerActionPos;

    [Header("Animation")]
    public float animDuration = 0.6f;

    [Header("Transparency")]
    public CanvasGroup hudCanvasGroup;

    private void OnEnable()
    {
        TurnManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        TurnManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(TurnManager.TurnPhase phase)
    {
        if (phase == TurnManager.TurnPhase.FirstRoll || phase == TurnManager.TurnPhase.RerollPhase)
        {
            hudCanvasGroup.DOFade(1f, animDuration);
            SwitchToActionUI();
        }
        else if (phase == TurnManager.TurnPhase.CardDrafting)
        {
            hudCanvasGroup.DOFade(0.2f, animDuration);
            SwitchToArenaUI();
        }
        else if (phase == TurnManager.TurnPhase.TurnStart || phase == TurnManager.TurnPhase.Resolution || 
                phase == TurnManager.TurnPhase.TurnEnd)
        {
            Debug.Log($"<color=cyan>HUDManager: Fase {phase}, HUD balik ke atas!</color>");
            hudCanvasGroup.DOFade(1f, animDuration);
            SwitchToArenaUI();
        }
    }

    public void SwitchToArenaUI()
    {
        playerHPBar.DOAnchorPos(playerHPArenaPos, animDuration).SetEase(Ease.InOutQuad);
        enemyHPBar.DOAnchorPos(enemyHPArenaPos,animDuration).SetEase(Ease.InOutQuad);
        trackerTile.DOAnchorPos(trackerArenaPos, animDuration).SetEase(Ease.InBack);
    }

    public void SwitchToActionUI()
    {
        playerHPBar.DOAnchorPos(playerHPActionPos, animDuration).SetEase(Ease.OutBack);
        enemyHPBar.DOAnchorPos(enemyHPActionPos, animDuration).SetEase(Ease.OutBack);
        trackerTile.DOAnchorPos(trackerActionPos, animDuration).SetEase(Ease.OutBack);
    }
}
