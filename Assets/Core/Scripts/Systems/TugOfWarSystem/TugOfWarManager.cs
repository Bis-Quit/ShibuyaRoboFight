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
}
