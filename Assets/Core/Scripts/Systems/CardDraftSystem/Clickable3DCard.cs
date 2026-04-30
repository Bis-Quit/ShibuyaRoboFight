using UnityEngine;

public class Clickable3DCard : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.CardDrafting) 
        {
        return;
        }

        if (TurnManager.Instance.CurrentPlayerIndex != 0)
        {
            return;
        }

        DraftingManager.Instance.OpenMarketUI();
        Debug.Log("<color=green>Kartu 3D diklik! Membuka Layar Katalog Toko...</color>");    
    }
}
