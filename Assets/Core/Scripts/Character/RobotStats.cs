using System;
using UnityEngine;

public class RobotStats : MonoBehaviour
{
    [Header("Data Character")]
    public CharacterData baseData;

    [Header("Current State")]
    public int currentHP;
    public int currentEnergy;

    [Header("Combat Anchors (Pintu Toko)")]
    public Transform attackMuzzle_1;
    public Transform attackMuzzle_2;
    public Transform attackMuzzle_3;
    
    public Transform destructionMuzzle_1;
    public Transform destructionMuzzle_2;
    public Transform destructionMuzzle_3;

    public Transform healPoint;
    public Transform braggingPoint;
    public Transform gotPowerPoint;
    public Transform defeatPoint;
    public Transform hitPoint;

    [Header("Personal VFX (Kurir Spesial)")]
    public GameObject attackVFX_1;
    public GameObject attackVFX_2;
    public GameObject attackVFX_3;
    
    public GameObject destructionVFX_1;
    public GameObject destructionVFX_2;
    public GameObject destructionVFX_3;
    
    public GameObject healVFX;
    public GameObject braggingVFX;
    public GameObject gotPowerVFX;
    public GameObject defeatVFX;

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

        Debug.Log($"<color=cyan>[{gameObject.name}] {baseData.characterName} SIAP! HP: {currentHP}/{baseData.maxHealth}</color>");
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        OnAnyRobotDamaged?.Invoke(this, amount);
        Debug.Log($"[{baseData.characterName}] Kena {amount} Damage! Sisa HP: {currentHP}/{baseData.maxHealth}");
        OnHPChanged?.Invoke(currentHP, baseData.maxHealth);

        if (currentHP <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > baseData.maxHealth) currentHP = baseData.maxHealth;

        Debug.Log($"[{baseData.characterName}] Di-heal {amount}! HP Sekarang: {currentHP}/{baseData.maxHealth}");
        OnHPChanged?.Invoke(currentHP, baseData.maxHealth);
        OnAnyRobotHealed?.Invoke(this, amount);
    }

    public void AddEnergy(int amount)
    {
        currentEnergy += amount;
        Debug.Log($"[{baseData.characterName}] Nambah {amount} Energy! Total Energy: {currentEnergy}");
        OnAnyRobotEnergyAdded?.Invoke(this, amount);
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    public void Die()
    {
        Debug.Log($"[{baseData.characterName}] HANCUR BERANTAKAN!");

        CharacterAnimator anim = GetComponent<CharacterAnimator>();
        if (anim != null)
        {
            anim.PlayAnim("defeat");
        }

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
            Debug.Log($"[{baseData.characterName}] Berhasil beli kartu seharga {costAmount} Energy! Sisa Energy: {currentEnergy}");
            OnEnergyChanged?.Invoke(currentEnergy);
            return true;
        }
        else
        {
            Debug.Log($"[{baseData.characterName}] Transaksi Gagal! Energy {currentEnergy} tidak cukup!");
        }
        return false;
    }

    public void AddSkillPower(int amount)
    {
        if (amount <= 0) return;
        currentSkillPower += amount;
        Debug.Log($"[{baseData.characterName}] Mendapat {amount} Skill Point! Total Skill Power: {currentSkillPower}");
        OnSkillPowerChanged?.Invoke(currentSkillPower);
    }

    public void LoseEnergy(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        Debug.Log($"[{baseData.characterName}] Kehilangan {amount} Energy! Sisa Energy: {currentEnergy}");
        OnAnyRobotEnergyLost?.Invoke(this, amount);
        OnEnergyChanged?.Invoke(currentEnergy); 
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
                    Debug.Log($"<color=yellow>{baseData.characterName} ULTIMATE: Fame +1, Destruction +1!</color>");
                    currentSkillPower -= 3;
                    OnSkillPowerChanged?.Invoke(currentSkillPower);
                }
            break;

            case SpecialSkillType.CharacterB_MultiplyEnergy:
            if (currentSkillPower > 0 && energyDiceRolled > 0)
            {
                int bonusEnergy = currentSkillPower *energyDiceRolled;
                AddEnergy(bonusEnergy);
                Debug.Log($"<color=yellow>{baseData.characterName} ULTIMATE: Bonus Energy +{bonusEnergy}!</color>");
                currentSkillPower = 0;
                OnSkillPowerChanged?.Invoke(currentSkillPower);
            }
            break;

            case SpecialSkillType.CharacterC_ExtraDamage:
                if (currentSkillPower >= 2)
                {
                    targetRobot.TakeDamage(3);
                    Debug.Log($"<color=yellow>{baseData.characterName} ULTIMATE: Serangan spesial! Target kena 3 damage!</color>");
                    currentSkillPower -= 2;
                    OnSkillPowerChanged?.Invoke(currentSkillPower);
                }
            break;

            case SpecialSkillType.CharacterD_ExtraDice:
                if (currentSkillPower >= 3)
                {
                    Debug.Log($"<color=yellow>{baseData.characterName} ULTIMATE: Extract Dadu untuk gilingan berikutnya!</color>");
                    AddbonusDice(1);
                    currentSkillPower -= 3;
                    OnSkillPowerChanged?.Invoke(currentSkillPower);
                }
            break;
        }
    }

    public void AddbonusDice(int amount)
    {
        bonusDice += amount;
        Debug.Log($"<color=cyan> [{baseData.characterName}] dapat tabungan {amount} dadu tambahan! Total : {bonusDice}</color>");
    }
}