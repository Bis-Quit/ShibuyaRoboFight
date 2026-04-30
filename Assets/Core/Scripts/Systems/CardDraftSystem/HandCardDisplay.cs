using UnityEngine;
using UnityEngine.UI;

// Tambahin komponen Button otomatis kalau lu lupa pasang di Unity
[RequireComponent(typeof(Button))] 
public class HandCardDisplay : MonoBehaviour
{
    public CardData cardData;
    [SerializeField] private Image cardImage;
    private Button cardButton;

    private void Awake()
    {
        cardButton = GetComponent<Button>();
        // Daftarin fungsi klik ke tombol ini
        cardButton.onClick.AddListener(OnHandCardClicked); 
    }

    public void Setup(CardData data)
    {
        cardData = data;
        if (cardImage != null) cardImage.sprite = data.cardIllustration;
    }

    // FUNGSI INI JALAN PAS KARTU DI TANGAN DIKLIK
    private void OnHandCardClicked()
    {
        // 1. CEK KEAMANAN SUTRADARA: Apakah ini giliran Player 1?
        if (TurnManager.Instance.CurrentPlayerIndex != 0)
        {
            Debug.Log("<color=orange>Sabar bro, ini lagi giliran musuh! Nggak bisa pake kartu!</color>");
            return; // Berhenti di sini, kartu batal dipake
        }

        // 2. CEK KEAMANAN TIPE KARTU: Pastikan ini kartu Instant
        if (cardData.cardCategory != CardData.CardCategory.Instant)
        {
            Debug.Log("<color=orange>Ini kartu Permanent, nggak perlu diklik!</color>");
            return;
        }

        // 3. EKSEKUSI JURUSNYA! (Kirim data ke CardEffectManager)
        if (CardEffectManager.Instance != null)
        {
            CardEffectManager.Instance.ApplyCardEffect(cardData);
        }

        // 4. HANCURKAN KARTU DARI UI (Karena udah dipake)
        Debug.Log($"<color=red>Kartu {cardData.cardName} dipakai dan hangus!</color>");
        
        // (Opsional) Lapor ke PlayerHand buat dihapus dari database list
        PlayerHand hand = GetComponentInParent<PlayerHand>();
        if (hand != null) hand.cardsInHand.Remove(cardData);

        Destroy(this.gameObject); // Hilang dari layar
    }
}