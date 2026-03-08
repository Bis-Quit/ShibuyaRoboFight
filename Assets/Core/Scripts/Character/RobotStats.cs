using UnityEngine;

public class RobotStats : MonoBehaviour
{
    [Header("Data Character")]
    [Tooltip("Tarik ScriptableObject dari Folder Data")]
    public CharacterData baseData;

    [Header("Current State")]
    public int currentHP;
    public int currentEnergy;

    private void Start()
    {
        if (baseData != null)
        {
            currentHP = baseData.maxHealth;
            currentEnergy = baseData.startingEnergy;

            Debug.Log($"<color=cyan>[{gameObject}] {baseData.characterName} siap bertempur! HP: {currentHP}, Energy {currentEnergy}</color>");
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

        if (currentHP <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > baseData.maxHealth) currentHP = baseData.maxHealth;

        Debug.Log($"[{baseData.characterName}] Di-heal {amount}! HP Sekarang: {currentHP}/{baseData.maxHealth}");
    }

    public void AddEnergy(int amount)
    {
        currentEnergy += amount;
        Debug.Log($"[{baseData.characterName}] Nambah {amount} Energy! Total Energy: {currentEnergy}");
    }

    public void Die()
    {
        Debug.Log($"[{baseData.characterName}] HANCUR BERANTAKAN!");
    }
}
