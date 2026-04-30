using UnityEngine;

public class CardShopTrigger : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.CardDrafting &&
            TurnManager.Instance.CurrentPlayerIndex == 0)
        {
            DraftingManager.Instance.OpenMarketUI();
        }
    }
}
