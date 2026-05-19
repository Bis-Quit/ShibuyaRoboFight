using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "ShibuyaRoboFight/Card Data")]

public class CardData : ScriptableObject
{
    [Header("Card Idetity")]
    public string cardID;
    public string cardName;

    [TextArea(2, 4)]
    public string description;
    public Sprite cardIllustration;

    [Header("Attribute")]
    public int abilityPointCost;
    public CardCategory cardCategory;

    [Header("Buzz Tile System")]
    public bool produceBuzzTile;
    public string buzzTileID;

    [Header("Player Condition")]
    public TargetSubject conditionSubject;
    public ConditionTrigger conditionType;
    public OperativeAction operativeCondition;
    public GameState conditionState;
    public Comprative comprativeCondition;
    public int conditionValue;

    [Header("Effect")]
    public TargetSubject effectTarget;
    public EffectAction effectType;
    public GameState targetState;
    public int effectValue;

    [Tooltip("Nama animasi yang mau di-play (misal: attack, healing, destruct)")]
    public string animationClipName;

    public enum CardCategory { Instant, Permanent }
    public enum TargetSubject { Self, Opponent }
    public enum ConditionTrigger { Immediately, ForEach, If }
    public enum OperativeAction { Has, Add, Subtract, Start }
    public enum Comprative { Equals, MoreThan }
    public enum EffectAction { Add, Subtract }

    public enum GameState
    {
        AbilityCard,
        AbilityPoint,
        HealthPoint,
        Fame,
        Destruction,
        Turn,
        Dice
    }
}
