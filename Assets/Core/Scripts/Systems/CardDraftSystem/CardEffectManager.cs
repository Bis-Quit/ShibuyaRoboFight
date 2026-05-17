using UnityEngine;
using System.Collections;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance;

    [Header("Battle References (Stats)")]
    [Tooltip("Otomatis diisi oleh ArenaManager")]
    public RobotStats playerStats; 
    public RobotStats enemyStats;  

    [Header("Battle References (Animations)")]
    [Tooltip("Otomatis diisi oleh ArenaManager")]
    public CharacterAnimator playerAnim; 
    public CharacterAnimator enemyAnim;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ApplyCardEffect(CardData card)
    {
        Debug.Log($"<color=cyan>MENGAKTIFKAN JURUS: {card.cardName}</color>");

        RobotStats caster = playerStats; 
        CharacterAnimator casterAnim = playerAnim;
        
        RobotStats target = (card.effectTarget == CardData.TargetSubject.Self) ? playerStats : enemyStats;
        CharacterAnimator targetAnim = (card.effectTarget == CardData.TargetSubject.Self) ? playerAnim : enemyAnim;
        
        RobotStats conditionSubjectRobot = (card.conditionSubject == CardData.TargetSubject.Self) ? playerStats : enemyStats;

        if (target == null || caster == null || conditionSubjectRobot == null || casterAnim == null || targetAnim == null)
        {
            Debug.LogError("Waduh, Robot Stats / Animator belum dicolokin ke CardEffectManager!");
            yield break;
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
                {
                    target.Heal(finalValue); 
                    casterAnim.PlayAnim("Restore");
                }
                else if (card.effectType == CardData.EffectAction.Subtract)
                {
                    target.TakeDamage(finalValue); 
                    casterAnim.PlayAnim("Attack");
                    targetAnim.PlayAnim("GotAttacked");
                }
                break;

            case CardData.GameState.AbilityPoint:
                if (card.effectType == CardData.EffectAction.Add)
                {
                    target.AddEnergy(finalValue); 
                    casterAnim.PlayAnim("GetPower");
                }
                else if (card.effectType == CardData.EffectAction.Subtract)
                {
                    target.LoseEnergy(finalValue); 
                    casterAnim.PlayAnim("Attack");
                    targetAnim.PlayAnim("GotAttacked");
                }
                break;

            case CardData.GameState.Fame:
                if (TugOfWarManager.Instance != null) 
                {
                    int targetIndex = (target == playerStats) ? 0 : 1;
                    TugOfWarManager.Instance.MoveFame(finalValue, targetIndex);
                    
                    casterAnim.PlayAnim("Bragging");
                    Debug.Log($"Tarik token FAME sejauh {finalValue} langkah!");
                }
                break;

            case CardData.GameState.Destruction:
                if (TugOfWarManager.Instance != null) 
                {
                    int targetIndex = (target == playerStats) ? 0 : 1;
                    TugOfWarManager.Instance.MoveDestruction(finalValue, targetIndex);
                    
                    casterAnim.PlayAnim("Destruct");
                    
                    if (targetAnim != casterAnim) 
                        targetAnim.PlayAnim("GetDestructed");
                    
                    Debug.Log($"Tarik token DESTRUCTION sejauh {finalValue} langkah!");
                }
                break;

            case CardData.GameState.Dice:
                if (card.effectType == CardData.EffectAction.Add)
                {
                    target.AddbonusDice(finalValue);
                    
                    casterAnim.PlayAnim("Bragging");
                    Debug.Log($"Dapet {finalValue} dadu tambahan buat nge-roll berikutnya!");
                }
                break;

            default:
                Debug.LogWarning($"Efek untuk {card.targetState} belum ada logikanya bro!");
                casterAnim.PlayAnim("Attack");
                break;
        }

        yield return null;

        float animDuration = 1f;
        if (casterAnim != null)
        {
            animDuration = casterAnim.GetAnimDuration();
        }
        Debug.Log($"<color=magenta>Kamera standby menunggu animasi selesai selama {animDuration} detik...</color>");
        yield return new WaitForSeconds(animDuration + 0.5f);
    }
}