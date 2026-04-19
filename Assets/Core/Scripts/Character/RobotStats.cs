using System;
using UnityEngine;

public class RobotStats : MonoBehaviour
{
    [Header("Data Character")]
    [Tooltip("Tarik ScriptableObject dari Folder Data")]
    public CharacterData baseData;

    [Header("Current State")]
    public int currentHP;
    public int currentEnergy;

    public event Action<int, int> OnHPChanged;
    public event Action<int> OnEnergyChanged;

    private void Start()
    {
        if (baseData != null)
        {
            currentHP = baseData.maxHealth;
            currentEnergy = baseData.startingEnergy;

            Debug.Log($"<color=cyan>[{gameObject}] {baseData.characterName} siap bertempur! HP: {currentHP}, Energy {currentEnergy}</color>");

            OnHPChanged?.Invoke(currentHP, baseData.maxHealth);
            OnEnergyChanged?.Invoke(currentEnergy);
        }
        else
        {
            Debug.LogWarning($"<color=red>[{gameObject.name}] CharacerData belum dimasukan!</color>");
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

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
    }

    public void AddEnergy(int amount)
    {
        currentEnergy += amount;
        Debug.Log($"[{baseData.characterName}] Nambah {amount} Energy! Total Energy: {currentEnergy}");

        OnEnergyChanged?.Invoke(currentEnergy);
    }

    public void Die()
    {
        Debug.Log($"[{baseData.characterName}] HANCUR BERANTAKAN!");
    }

    public bool SpendEnergy(int costAmount)
    {
        if (currentEnergy >= costAmount)
        {
            Debug.Log($"[{baseData.characterName}] Berhasil beli kartu seharga {costAmount} Energy! Sisa Energy: {currentEnergy}");

            OnEnergyChanged?.Invoke(currentEnergy);
            return true;
        }
        else
        {
            Debug.Log($"[{baseData.characterName}] Transaksi Gagal! Energy {currentEnergy} tidak cukup untuk beli kartu seharga {costAmount} Energy!");
        }
        return false;
    }
}
