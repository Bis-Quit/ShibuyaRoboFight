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

        string casterSkillAnimName = string.IsNullOrEmpty(card.animationClipName) ? "attack" : card.animationClipName;

        float totalAnimDur = casterAnim.GetAnimDuration(casterSkillAnimName);
        if (totalAnimDur < 1f) totalAnimDur = 1.5f;

        string stateTargetName = card.targetState.ToString();
        bool isAttackCard = (stateTargetName == "HealthPoint" || stateTargetName == "AbilityPoint");

        if (BattleUIManager.Instance != null)
        {
            string actionCam = isAttackCard ? "AttackAction" : "BuffAction";
            BattleUIManager.Instance.SwitchCinematicPOV(isPlayerTurn, actionCam);
        }

        yield return new WaitForSeconds(0.5f); 

        casterAnim.PlayAnim(casterSkillAnimName);

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
                target.TakeDamage(finalValue);
                if(target.currentHP <= 0 && targetAnim != null) targetAnim.PlayAnim("defeat");
                else if (targetAnim != null) targetAnim.PlayAnim("got attacked");
                break;
                
            case "AbilityPoint":
                target.TakeDamage(finalValue); 
                if (targetAnim != null && target.currentHP > 0) targetAnim.PlayAnim("got attacked");
                break;
                
            case "Dice":
            case "DicePool":
                target.AddbonusDice(finalValue);
                Debug.Log($"Dapet {finalValue} dadu tambahan!");
                break;
        }

        float remainingTime = totalAnimDur - hitDelay;
        yield return new WaitForSeconds(remainingTime + 0.5f);
    }
}