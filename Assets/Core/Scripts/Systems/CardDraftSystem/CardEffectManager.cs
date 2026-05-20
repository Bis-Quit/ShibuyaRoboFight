using UnityEngine;
using System.Collections;

public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance;

    [Header("Battle References")]
    public RobotStats playerStats; 
    public RobotStats enemyStats;  
    public CharacterAnimator playerAnim; 
    public CharacterAnimator enemyAnim;

    private void Awake() { Instance = this; }

    public IEnumerator ApplyCardEffect(CardData card)
    {
        Debug.Log($"<color=cyan>MENGAKTIFKAN JURUS: {card.cardName}</color>");

        bool isPlayerTurn = (TurnManager.Instance.CurrentPlayerIndex == 0);
        RobotStats caster = isPlayerTurn ? playerStats : enemyStats;
        CharacterAnimator casterAnim = isPlayerTurn ? playerAnim : enemyAnim;
        RobotStats target = (card.effectTarget == CardData.TargetSubject.Self) ? playerStats : enemyStats;
        CharacterAnimator targetAnim = (card.effectTarget == CardData.TargetSubject.Self) ? playerAnim : enemyAnim;

        string casterSkillAnimName = string.IsNullOrEmpty(card.animationClipName) ? "Attack" : card.animationClipName;

        string stateTargetName = card.targetState.ToString();
        string effectTypeName = card.effectType.ToString();

        bool isAttackCard = (effectTypeName != "Add" && (stateTargetName == "HealthPoint" || stateTargetName == "AbilityPoint"));

        if (BattleUIManager.Instance != null)
        {
            string actionCam = isAttackCard ? "AttackAction" : "BuffAction";
            BattleUIManager.Instance.SwitchCinematicPOV(isPlayerTurn, actionCam);
        }

        yield return new WaitForSeconds(0.5f); 

        casterAnim.PlayAnim(casterSkillAnimName);

        float totalAnimDur = 1.5f; 
        if (casterAnim.anim != null)
        {
            casterAnim.anim.Update(0f); 
            totalAnimDur = casterAnim.anim.GetCurrentAnimatorStateInfo(0).length;
        }
        
        if (totalAnimDur < 1f) totalAnimDur = 1.5f; 
        Debug.Log($"<color=yellow>🎬 Durasi Animasi '{casterSkillAnimName}' Terdeteksi: {totalAnimDur} detik.</color>");

        float hitDelay = totalAnimDur * 0.5f;
        yield return new WaitForSeconds(hitDelay); 

        if (isAttackCard && BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.SwitchCinematicPOV(isPlayerTurn, "Reaction");
            yield return new WaitForSeconds(0.2f); 
        }

        int finalValue = card.effectValue;
        switch (stateTargetName)
        {
            case "HealthPoint":
                if (effectTypeName == "Add") 
                {
                    target.Heal(finalValue);
                }
                else 
                {
                    target.TakeDamage(finalValue);

                    if(target.currentHP > 0 && targetAnim != null) 
                    {
                        targetAnim.PlayAnim("got attacked");
                    }
                }
                break;
                
            case "AbilityPoint":
                if (effectTypeName == "Add") 
                {
                    target.AddEnergy(finalValue);
                }
                else
                {
                    target.LoseEnergy(finalValue);
                    if (targetAnim != null && target.currentHP > 0) 
                    {
                        targetAnim.PlayAnim("got attacked"); 
                    }
                }
                break;
                
            case "Dice":
            case "DicePool":
                target.AddbonusDice(finalValue);
                break;
        }

        float remainingTime = totalAnimDur - hitDelay;
        yield return new WaitForSeconds(remainingTime + 0.3f);
    }
}