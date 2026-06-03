using UnityEngine;

public class DiceInteractableVisual : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseSpeed = 4f;      
    public float pulseAmount = 0.03f;  
    
    [Header("Pengaturan Waktu")]
    public float delayBeforePulse = 1.5f;

    private Vector3 originalScale;
    private Dice diceScript; 
    
    private float currentTimer = 0f; 

    private void Start()
    {
        originalScale = transform.localScale;
        diceScript = GetComponent<Dice>(); 
    }

    private void Update()
    {
        if (TurnManager.Instance == null || diceScript == null) return;

        bool isPlayerTurn = TurnManager.Instance.CurrentPlayerIndex == 0;
        bool isRerollPhase = TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.RerollPhase;

        bool canBeClicked = isPlayerTurn && isRerollPhase && !diceScript.isRolling && !diceScript.isLocked;

        if (canBeClicked)
        {
            currentTimer += Time.deltaTime;

            if (currentTimer >= delayBeforePulse)
            {
                float pulseTime = currentTimer - delayBeforePulse; 
                float scaleOffset = Mathf.Sin(pulseTime * pulseSpeed) * pulseAmount;
                
                transform.localScale = originalScale + new Vector3(scaleOffset, scaleOffset, scaleOffset);
            }
        }
        else
        {
            currentTimer = 0f;
            transform.localScale = originalScale;
        }
    }
}