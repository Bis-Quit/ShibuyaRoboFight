using System;
using UnityEngine;

public class RobotStats : MonoBehaviour
{
    [Header("Data Character")]
    public CharacterData baseData;

    [Header("Current State")]
    public int currentHP;
    public int currentEnergy;

    public event Action<int, int> OnHPChanged;
    public event Action<int> OnEnergyChanged;
    public event Action<int> OnSkillPowerChanged;

    public static event Action<RobotStats, int> OnAnyRobotHealed;
    public static event Action<RobotStats, int> OnAnyRobotDamaged;
    public static event Action<RobotStats, int> OnAnyRobotEnergyAdded;
    public static event Action<RobotStats, int> OnAnyRobotEnergyLost;

    [Header("Skill System")]
    public int currentSkillPower = 0;

    [Header("Buff Status")]
    public int bonusDice = 0;

    public void InitializeHP()
    {
        if (baseData == null) return;
        currentHP = baseData.maxHealth;
        currentEnergy = baseData.startingEnergy;
        currentSkillPower = 0;
        bonusDice = 0;

        OnHPChanged?.Invoke(currentHP, baseData.maxHealth);
        OnEnergyChanged?.Invoke(currentEnergy);
        OnSkillPowerChanged?.Invoke(currentSkillPower);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;
        OnAnyRobotDamaged?.Invoke(this, amount);
        OnHPChanged?.Invoke(currentHP, baseData.maxHealth);
        if (currentHP <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > baseData.maxHealth) currentHP = baseData.maxHealth;
        OnHPChanged?.Invoke(currentHP, baseData.maxHealth);
        OnAnyRobotHealed?.Invoke(this, amount);
    }

    public void AddEnergy(int amount)
    {
        currentEnergy += amount;
        OnAnyRobotEnergyAdded?.Invoke(this, amount);
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    public void LoseEnergy(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        OnAnyRobotEnergyLost?.Invoke(this, amount);
        OnEnergyChanged?.Invoke(currentEnergy); 
    }

    public void Die()
    {
        CharacterAnimator anim = GetComponent<CharacterAnimator>();
        if (anim != null) anim.PlayAnim("defeat");
        if (GameOverManager.Instance != null)
        {
            bool isPlayerWin = (this != GameOverManager.Instance.playerRobot);
            GameOverManager.Instance.TriggerGameOver(isPlayerWin);
        }
    }

    public bool SpendEnergy(int costAmount)
    {
        if (currentEnergy >= costAmount)
        {
            currentEnergy -= costAmount;
            OnEnergyChanged?.Invoke(currentEnergy);
            return true;
        }
        return false;
    }

    public void AddSkillPower(int amount)
    {
        if (amount <= 0) return;
        currentSkillPower += amount;
        OnSkillPowerChanged?.Invoke(currentSkillPower);
    }

    public void AddbonusDice(int amount)
    {
        bonusDice += amount;
    }

    public void CheckAndExecuteSkill(int energyDiceRolled, RobotStats targetRobot)
    {
        switch (baseData.skillType)
        {
            case SpecialSkillType.CharacterA_PullTokens:
                if (currentSkillPower >= 3)
                {
                    TugOfWarManager.Instance.MoveFame(1, TurnManager.Instance.CurrentPlayerIndex);
                    TugOfWarManager.Instance.MoveDestruction(1, TurnManager.Instance.CurrentPlayerIndex);
                    currentSkillPower -= 3;
                    OnSkillPowerChanged?.Invoke(currentSkillPower);
                }
            break;

            case SpecialSkillType.CharacterB_MultiplyEnergy:
            if (currentSkillPower > 0 && energyDiceRolled > 0)
            {
                int bonusEnergy = currentSkillPower * energyDiceRolled;
                AddEnergy(bonusEnergy);
                currentSkillPower = 0;
                OnSkillPowerChanged?.Invoke(currentSkillPower);
            }
            break;

            case SpecialSkillType.CharacterC_ExtraDamage:
                if (currentSkillPower >= 2)
                {
                    targetRobot.TakeDamage(3);
                    currentSkillPower -= 2;
                    OnSkillPowerChanged?.Invoke(currentSkillPower);
                }
            break;

            case SpecialSkillType.CharacterD_ExtraDice:
                if (currentSkillPower >= 3)
                {
                    AddbonusDice(1);
                    currentSkillPower -= 3;
                    OnSkillPowerChanged?.Invoke(currentSkillPower);
                }
            break;
        }
    }
}