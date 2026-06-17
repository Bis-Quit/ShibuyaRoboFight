using UnityEngine;
using System.Collections;
public class TugOfWarManager : MonoBehaviour
{
    public static TugOfWarManager Instance { get; private set; }

    [Header("Track")]
    public Transform[] fameTiles;
    public Transform[] destructTiles;

    [Header("Token")]
    public Transform fameToken;
    public Transform destructToken;

    [Header("Buzz Tile Slots (Kotak Ungu)")]
    public ArenaTile playerDestructionBuzzTile;
    public ArenaTile playerFameBuzzTile;
    public ArenaTile enemyDestructionBuzzTile;
    public ArenaTile enemyFameBuzzTile;

    [Header("Animation")]
    public float yOffset = 0.5f;
    public float moveDuration = 0.5f;

    private int currentFameIndex;
    private int currentDestructionIndex;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentFameIndex = fameTiles.Length / 2;
        currentDestructionIndex = destructTiles.Length / 2;

        if (fameTiles.Length > 0 && fameToken != null)
            fameToken.position = fameTiles[currentFameIndex].position + new Vector3(0, yOffset, 0);
            fameToken.rotation = fameTiles[currentFameIndex].rotation;

        if (destructTiles.Length > 0 && destructToken != null)
            destructToken.position = destructTiles[currentDestructionIndex].position + new Vector3(0, yOffset, 0);
            destructToken.rotation = destructTiles[currentFameIndex].rotation;
    }

    public void MoveFame(int points, int playerIndex)
    {
        if (playerIndex == 0) currentFameIndex -= points;
        else currentFameIndex += points;

        currentFameIndex = Mathf.Clamp(currentFameIndex, 0, fameTiles.Length - 1);

        StartCoroutine(AnimateToken(fameToken, fameTiles, currentFameIndex));
        CheckWinCondition(currentFameIndex, fameTiles.Length, "Fame");
    }

    public void MoveDestruction(int points, int playerIndex)
    {
        if (playerIndex == 0) currentDestructionIndex -= points;
        else currentDestructionIndex += points;

        currentDestructionIndex = Mathf.Clamp(currentDestructionIndex, 0, destructTiles.Length - 1);

        StartCoroutine(AnimateToken(destructToken, destructTiles, currentDestructionIndex));
        CheckWinCondition(currentDestructionIndex, destructTiles.Length, "Destruction");
    }

    public IEnumerator AnimateToken(Transform token, Transform[] track, int targetIndex)
    {
        Vector3 startPos = token.position;
        Vector3 targetPos = track[targetIndex].position + new Vector3(0, yOffset, 0);

        Quaternion startRot = token.rotation;
        Quaternion targetRot = track[targetIndex].rotation;

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            token.position = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            token.rotation = Quaternion.Lerp(startRot, targetRot, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        token.position = targetPos;
        token.rotation = targetRot;

        ArenaTile landedTile = track[targetIndex].GetComponent<ArenaTile>();
        
        if (landedTile != null && !string.IsNullOrEmpty(landedTile.activeBuzzEffectID))
        {
            Debug.Log($"<color=red>💥 BOOM! Bidak menginjak ranjau: {landedTile.activeBuzzEffectID}</color>");
            
            ExecuteBuzzEffect(landedTile.activeBuzzEffectID);

            landedTile.ClearBuzzTrap();
        }
    }

    public void CheckWinCondition(int index, int trackLength, string trackType)
    {
        if (index == 0)
        {
            Debug.Log($"<color=green> Player 0 menarik full jalur {trackType.ToUpper()}! </color>");

            GameOverManager.Instance.TriggerGameOver(true);
        }
        else if (index == trackLength - 1)
        {
            Debug.Log($"<color=green> Player 1 menarik full jalur {trackType.ToUpper()}! </color>");

            GameOverManager.Instance.TriggerGameOver(false);
        }
    }

    private void ExecuteBuzzEffect(string buzzID)
    {
        bool isPlayerTurn = TurnManager.Instance.CurrentPlayerIndex == 0;
        RobotStats victimStats = isPlayerTurn ? CardEffectManager.Instance.playerStats : CardEffectManager.Instance.enemyStats;
        CharacterAnimator victimAnim = isPlayerTurn ? CardEffectManager.Instance.playerAnim : CardEffectManager.Instance.enemyAnim;

        switch (buzzID)
        {
            // ================= TILE COUNT: 1 =================
            case "BT001":
            case "BT002":
            case "BT003":
                Debug.Log("Hukuman: -1 Health Point!");
                victimStats.TakeDamage(1);
                if (victimAnim != null) victimAnim.PlayAnim("got attacked");
                break;

            case "BT004":
            case "BT005":
                Debug.Log("Hukuman: -1 Ability Point!");
                victimStats.LoseEnergy(1);
                break;

            // ================= TILE COUNT: 2 =================
            case "BT006":
                Debug.Log("✨ Efek Buzz Tile: -1 Health Point & +1 EXTRA DICE!");
                victimStats.TakeDamage(1);
                if (victimAnim != null) victimAnim.PlayAnim("got attacked");             // Tambah 1 Dadu ke korban yang nginjek!
                victimStats.AddbonusDice(1);
                break;

            case "BT007":
                Debug.Log("✨ Efek Buzz Tile: -1 Ability Point & +1 EXTRA DICE!");
                victimStats.LoseEnergy(1);
                victimStats.AddbonusDice(1);
                break;

            // ================= TILE COUNT: 3 =================
            case "BT008":
                Debug.Log("✨ Efek SUPER BUZZ: -1 HP, -1 AP, & +1 EXTRA DICE!");
                victimStats.TakeDamage(1);
                victimStats.LoseEnergy(1);
                if (victimAnim != null) victimAnim.PlayAnim("got attacked");
                victimStats.AddbonusDice(1);
                break;

            default:
                Debug.LogWarning($"Efek untuk {buzzID} belum diatur!");
                break;
        }
    }
}
