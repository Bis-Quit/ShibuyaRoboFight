using UnityEngine;
using DG.Tweening;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("HUD Elements")]
    public RectTransform playerHPBar;
    public RectTransform enemyHPBar;

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

    [HideInInspector] public bool isCinematicActive = false;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void ToggleHUD(bool isVisible)
    {
        isCinematicActive = !isVisible;

        if (hudCanvasGroup != null)
        {
            float targetAlpha = isVisible ? 1f : 0f;
            DOTween.To(() => hudCanvasGroup.alpha, x => hudCanvasGroup.alpha = x, targetAlpha, 0.3f);
        }
    }

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
        if (isCinematicActive) return;

        if (phase == TurnManager.TurnPhase.FirstRoll ||
            phase == TurnManager.TurnPhase.RerollPhase)
        {
            hudCanvasGroup.DOFade(1f, animDuration);
            SwitchToActionUI();
        }
        else if ( phase == TurnManager.TurnPhase.CardDrafting ||
                phase == TurnManager.TurnPhase.TurnStart ||
                phase == TurnManager.TurnPhase.Resolution || 
                phase == TurnManager.TurnPhase.TurnEnd)
        {
            Debug.Log($"<color=cyan>HUDManager: Fase {phase}, HUD balik ke atas!</color>");
            hudCanvasGroup.DOFade(1f, animDuration);
            SwitchToArenaUI();
        }
    }

    public void SwitchToArenaUI()
    {
        if (playerHPBar != null) playerHPBar.DOAnchorPos(playerHPArenaPos, animDuration).SetEase(Ease.InOutQuad);
        if (enemyHPBar != null) enemyHPBar.DOAnchorPos(enemyHPArenaPos,animDuration).SetEase(Ease.InOutQuad);
    }

    public void SwitchToActionUI()
    {
        if (playerHPBar != null) playerHPBar.DOAnchorPos(playerHPActionPos, animDuration).SetEase(Ease.OutBack);
        if (enemyHPBar != null) enemyHPBar.DOAnchorPos(enemyHPActionPos, animDuration).SetEase(Ease.OutBack);
    }
}