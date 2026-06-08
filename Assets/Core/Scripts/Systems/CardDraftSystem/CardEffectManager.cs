using UnityEngine;
using System.Collections;
using System;

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
        RobotStats target = (card.effectTarget == CardData.TargetSubject.Self) ? caster : (isPlayerTurn ? enemyStats : playerStats);
        CharacterAnimator targetAnim = (card.effectTarget == CardData.TargetSubject.Self) ? casterAnim : (isPlayerTurn ? enemyAnim : playerAnim);

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

        Action deliverPackage = () => 
        {
            switch (stateTargetName)
            {
                case "HealthPoint":
                    if (effectTypeName == "Add") { target.Heal(finalValue); }
                    else 
                    { 
                        target.TakeDamage(finalValue);
                        if(target.currentHP > 0 && targetAnim != null) targetAnim.PlayAnim("got attacked");
                    }
                    break;
                    
                case "AbilityPoint":
                    if (effectTypeName == "Add") { target.AddEnergy(finalValue); }
                    else
                    {
                        target.LoseEnergy(finalValue);
                        if (targetAnim != null && target.currentHP > 0) targetAnim.PlayAnim("got attacked"); 
                    }
                    break;
                    
                case "Dice":
                case "DicePool":
                    target.AddbonusDice(finalValue);
                    break;
            }
        };

        if (isAttackCard)
        {
            Transform selectedMuzzle = null;
            GameObject selectedProjPrefab = null;

            switch (card.weaponType)
            {
                case CardData.WeaponType.Attack_1:
                    selectedMuzzle = caster.attackMuzzle_1; selectedProjPrefab = caster.attackVFX_1; break;
                case CardData.WeaponType.Attack_2:
                    selectedMuzzle = caster.attackMuzzle_2; selectedProjPrefab = caster.attackVFX_2; break;
                case CardData.WeaponType.Attack_3:
                    selectedMuzzle = caster.attackMuzzle_3; selectedProjPrefab = caster.attackVFX_3; break;
                case CardData.WeaponType.Destruction_1:
                    selectedMuzzle = caster.destructionMuzzle_1; selectedProjPrefab = caster.destructionVFX_1; break;
                case CardData.WeaponType.Destruction_2:
                    selectedMuzzle = caster.destructionMuzzle_2; selectedProjPrefab = caster.destructionVFX_2; break;
                case CardData.WeaponType.Destruction_3:
                    selectedMuzzle = caster.destructionMuzzle_3; selectedProjPrefab = caster.destructionVFX_3; break;
            }

            if (selectedProjPrefab != null && selectedMuzzle != null && target.hitPoint != null) 
            {
                GameObject projObj = Instantiate(selectedProjPrefab, selectedMuzzle.position, selectedMuzzle.rotation);
                ProjectileController proj = projObj.GetComponent<ProjectileController>();

                bool isLanded = false; 

                proj.FireProjectile(target.hitPoint, () => 
                {
                    deliverPackage.Invoke(); 
                    isLanded = true;         

                    if (target.currentHP <= 0 && target.defeatVFX != null && target.defeatPoint != null)
                    {
                        Instantiate(target.defeatVFX, target.defeatPoint.position, target.defeatPoint.rotation);
                    }
                });

                yield return new WaitUntil(() => isLanded == true);
            }
            else
            {
                Debug.LogWarning($"<color=red>VFX {card.weaponType} ga ada di {caster.gameObject.name}! Damage instan.</color>");
                deliverPackage.Invoke();
            }
        }
        else
        {
            deliverPackage.Invoke();

            GameObject vfxToPlay = null;
            Transform spawnSpot = null;

            if (effectTypeName == "Add" && stateTargetName == "HealthPoint") 
            {
                vfxToPlay = target.healVFX;
                spawnSpot = target.healPoint;
            }
            else if (stateTargetName == "AbilityPoint" || stateTargetName == "Dice") 
            {
                vfxToPlay = target.gotPowerVFX;
                spawnSpot = target.gotPowerPoint;
            }
            else if (stateTargetName == "Fame")
            {
                vfxToPlay = target.braggingVFX;
                spawnSpot = target.braggingPoint;
            }

            if (vfxToPlay != null && spawnSpot != null)
            {
                GameObject spawnedVFX = Instantiate(vfxToPlay, spawnSpot.position, Quaternion.identity);
                spawnedVFX.transform.SetParent(spawnSpot);
                Destroy(spawnedVFX, 3f);
            }
        }

        float remainingTime = totalAnimDur - hitDelay;
        yield return new WaitForSeconds(remainingTime + 0.3f);

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ResetCamera();
        }
        Debug.Log($"<color=green> Efek '{card.cardName}' selesai diterapkan, Kamera kembali ke Arena.</color>");
    }
}