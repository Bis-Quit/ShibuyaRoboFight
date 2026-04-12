using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "ShibuyaRoboFight/Card Data")]

public class CardData : ScriptableObject
{
    [Header("Card Idetity")]
    [SerializeField] private string cardID;
    public string cardName;

    [TextArea(2, 4)]
    [SerializeField] private string description;
    public Sprite cardIllustration;

    [Header("Attribute")]
    [SerializeField] private int abilityPointCost;
    [SerializeField] private CardCategory cardCategory;

    [Header("Buzz Tile System")]
    [SerializeField] private bool produceBuzzTile;
    [SerializeField] private string buzzTileID;

    [Header("Player Condition")]
    [SerializeField] private TargetSubject conditionSubject;
    [SerializeField] private ConditionTrigger conditionType;
    [SerializeField] private OperativeAction operativeCondition;
    [SerializeField] private GameState conditionState;
    [SerializeField] private Comprative comprativeCondition;
    [SerializeField] private int conditionValue;

    [Header("Effect")]
    [SerializeField] private TargetSubject effectTarget;
    [SerializeField] private EffectAction effectType;
    [SerializeField] private GameState targetState;
    [SerializeField] private int effectValue;

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
