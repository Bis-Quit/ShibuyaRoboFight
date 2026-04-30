using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance;

    [Header("Battle References")]
    [Tooltip("Tarik Robot Player ke sini")]
    public RobotStats playerStats; 
    
    [Tooltip("Tarik Robot Musuh (AI) ke sini")]
    public RobotStats enemyStats;  

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyCardEffect(CardData card)
    {
        Debug.Log($"<color=cyan>🔥 MENGAKTIFKAN JURUS: {card.cardName} 🔥</color>");

        // 1. TENTUKAN TARGET
        RobotStats target = (card.effectTarget == CardData.TargetSubject.Self) ? playerStats : enemyStats;

        if (target == null)
        {
            Debug.LogError("Waduh, Target Stats belum dicolokin di Inspector CardEffectManager!");
            return;
        }

        // 2. TERJEMAHKAN EFEKNYA
        switch (card.targetState)
        {
            case CardData.GameState.HealthPoint:
                if (card.effectType == CardData.EffectAction.Add)
                {
                    target.Heal(card.effectValue); // 👈 Langsung panggil fungsi Heal lu!
                }
                else if (card.effectType == CardData.EffectAction.Subtract)
                {
                    target.TakeDamage(card.effectValue); // 👈 Langsung panggil TakeDamage lu!
                }
                break;

            case CardData.GameState.AbilityPoint:
                if (card.effectType == CardData.EffectAction.Add)
                {
                    target.AddEnergy(card.effectValue); // 👈 Langsung panggil AddEnergy lu!
                }
                else if (card.effectType == CardData.EffectAction.Subtract)
                {
                    target.LoseEnergy(card.effectValue); // 👈 Kita butuh nambahin fungsi ini dikit di RobotStats
                }
                break;

            case CardData.GameState.Fame:
                // Asumsi lu punya TugOfWarManager seperti di kodingan Skill lu
                if (TugOfWarManager.Instance != null) 
                {
                    int playerIndex = (target == playerStats) ? 0 : 1;
                    TugOfWarManager.Instance.MoveFame(card.effectValue, playerIndex);
                    Debug.Log($"✨ Tarik token FAME sejauh {card.effectValue} langkah!");
                }
                break;

            case CardData.GameState.Destruction:
                if (TugOfWarManager.Instance != null) 
                {
                    int playerIndex = (target == playerStats) ? 0 : 1;
                    TugOfWarManager.Instance.MoveDestruction(card.effectValue, playerIndex);
                    Debug.Log($"☠️ Tarik token DESTRUCTION sejauh {card.effectValue} langkah!");
                }
                break;

            case CardData.GameState.Dice:
                if (card.effectType == CardData.EffectAction.Add)
                {
                    Debug.Log($"🎲 (WIP) Dapet {card.effectValue} dadu tambahan buat nge-roll!");
                    // TODO: Tambah logika dadu ke DiceManager lu
                }
                break;

            default:
                Debug.LogWarning($"Efek untuk {card.targetState} belum ada logikanya bro!");
                break;
        }
    }
}