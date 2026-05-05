using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance;

    [Header("Battle References")]
    [Tooltip("Tarik Robot Player ke sini")]
    public RobotStats playerStats; 
    
    [Tooltip("Tarik Robot Enemy ke sini")]
    public RobotStats enemyStats;  

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyCardEffect(CardData card)
    {
        Debug.Log($"<color=cyan>🔥 MENGAKTIFKAN JURUS: {card.cardName} 🔥</color>");

        RobotStats caster = playerStats; 
        
        RobotStats target = (card.effectTarget == CardData.TargetSubject.Self) ? playerStats : enemyStats;
        
        RobotStats conditionSubjectRobot = (card.conditionSubject == CardData.TargetSubject.Self) ? playerStats : enemyStats;

        if (target == null || caster == null || conditionSubjectRobot == null)
        {
            Debug.LogError("Waduh, Robot Stats belum dicolokin di Inspector CardEffectManager!");
            return;
        }

        int finalValue = card.effectValue;

        if (card.conditionType == CardData.ConditionTrigger.ForEach)
        {
            int multiplier = 0;

            if (card.conditionState == CardData.GameState.AbilityPoint)
            {
                multiplier = conditionSubjectRobot.currentEnergy;
                Debug.Log($"<color=yellow>⚡ Menghitung Ability Point dari {card.conditionSubject}: {multiplier}</color>");
            }
            else if (card.conditionState == CardData.GameState.AbilityCard)
            {
                Debug.LogWarning("Penghitungan ForEach untuk AbilityCard belum jalan bro!");
                multiplier = 1; 
            }
            
            finalValue = card.effectValue * multiplier;
            Debug.Log($"<color=yellow>⚡ MULTIPLIER AKTIF! Efek Asli ({card.effectValue}) x Kondisi ({multiplier}) = {finalValue}</color>");
        }

        switch (card.targetState)
        {
            case CardData.GameState.HealthPoint:
                if (card.effectType == CardData.EffectAction.Add)
                    target.Heal(finalValue); 
                else if (card.effectType == CardData.EffectAction.Subtract)
                    target.TakeDamage(finalValue); 
                break;

            case CardData.GameState.AbilityPoint:
                if (card.effectType == CardData.EffectAction.Add)
                    target.AddEnergy(finalValue); 
                else if (card.effectType == CardData.EffectAction.Subtract)
                    target.LoseEnergy(finalValue); 
                break;

            case CardData.GameState.Fame:
                if (TugOfWarManager.Instance != null) 
                {
                    int targetIndex = (target == playerStats) ? 0 : 1;
                    TugOfWarManager.Instance.MoveFame(finalValue, targetIndex);
                    Debug.Log($"✨ Tarik token FAME sejauh {finalValue} langkah!");
                }
                break;

            case CardData.GameState.Destruction:
                if (TugOfWarManager.Instance != null) 
                {
                    int targetIndex = (target == playerStats) ? 0 : 1;
                    TugOfWarManager.Instance.MoveDestruction(finalValue, targetIndex);
                    Debug.Log($"☠️ Tarik token DESTRUCTION sejauh {finalValue} langkah!");
                }
                break;

            case CardData.GameState.Dice:
                if (card.effectType == CardData.EffectAction.Add)
                {
                    target.AddbonusDice(finalValue);
                    Debug.Log($"🎲 (WIP) Dapet {finalValue} dadu tambahan buat nge-roll berikutnya!");
                }
                break;

            default:
                Debug.LogWarning($"Efek untuk {card.targetState} belum ada logikanya bro!");
                break;
        }
    }
}