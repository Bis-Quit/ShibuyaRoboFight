using System;
using UnityEngine;

public class TrackerUIController : MonoBehaviour
{
    [Header("UI Element")]
    public GameObject trackerUI;

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
        if (phase == TurnManager.TurnPhase.FirstRoll ||
            phase == TurnManager.TurnPhase.RerollPhase ||
            phase == TurnManager.TurnPhase.CardDrafting)
        {
            trackerUI.SetActive(true);
        }
        else
        {
            trackerUI.SetActive(false);
        }
    }
}
