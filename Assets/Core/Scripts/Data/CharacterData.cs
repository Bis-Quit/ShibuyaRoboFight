using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "ShibuyaRoboFight/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterID;
    public string characterName;

    [Header("Base Stats")]
    public int maxHealth;
    public int startingEnergy = 0;

    [Header("Dice System")]
    public int activeDicePool = 6;
    public int lockedDicePool = 0;
    public int powerCardSlot = 0;

    [Header("Special Skill Mechanics")]
    [TextArea(2,4)]
    public string specialSkillDescription;

    public SpecialSkillType skillType;
}

public enum SpecialSkillType
{
    CharacterA_PullTokens,
    CharacterB_MultiplyEnergy,
    CharacterC_ExtraDamage,
    CharacterD_ExtraDice
}
