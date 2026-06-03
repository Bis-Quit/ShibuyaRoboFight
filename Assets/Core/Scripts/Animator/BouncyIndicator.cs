using UnityEngine;

public class BouncyIndicator : MonoBehaviour
{
    [Header("Bouncy Settings")]
    public float bounceSpeed = 5f;
    public float bounceHeight = 30f;

    private Vector3 starPos;

    private void Start()
    {
        starPos = transform.localPosition;
    }

    private void Update()
    {
        float newY = starPos.y + (Mathf.Sin(Time.time * bounceSpeed) * bounceHeight);
        transform.localPosition = new Vector3(starPos.x, newY, starPos.z);
    }
}
