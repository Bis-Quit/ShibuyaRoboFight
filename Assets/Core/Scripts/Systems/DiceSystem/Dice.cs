using UnityEngine;

    public enum DiceFace
    {
        Smash,
        Heal,
        Energy,
        Fame,
        Destruction,
        SpecialPower
    }

[RequireComponent(typeof(Rigidbody))]
public class Dice : MonoBehaviour
{
    public DiceFace CurrentFace { get; private set; }

    public bool isLocked { get; private set; }
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private Rigidbody rb;
    
    public bool isRolling { get; private set; }

    [Header("Pengaturan Lemparan")]
    public float throwForce = 10f;
    public float rollTorque = 50f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        originalScale = transform.localScale;
    }

    public void Roll()
    {
        isRolling = true;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 throwDirection = (Vector3.up + Random.insideUnitSphere * 0.05f).normalized;

        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * rollTorque, ForceMode.Impulse);
    }

    private void Update()
    {
        if (isRolling)
        {
            if (rb.linearVelocity.sqrMagnitude < 0.01f && rb.angularVelocity.sqrMagnitude < 0.01f)
            {
                isRolling = false;
                DetermineTopFace();
            }
        }
    }

    public void ToggleLock()
    {
        if (isRolling) return;

        DiceManager.Instance.LockDice(this);
    }

    private void DetermineTopFace()
    {
        Vector3[] localDirections = new Vector3[]
        {
            transform.up,
            -transform.up,
            transform.right,
            -transform.right,
            transform.forward,
            -transform.forward
        };

        DiceFace[] faceValues = new DiceFace[]
        {
            DiceFace.Smash,
            DiceFace.Heal,
            DiceFace.Energy,
            DiceFace.Fame,
            DiceFace.Destruction,
            DiceFace.SpecialPower
        };

        float maxDotProduct = -Mathf.Infinity;
        int topFaceIndex = 0;

        for (int i = 0; i < localDirections.Length; i++)
        {
            float dotProduct = Vector3.Dot(localDirections[i], Vector3.up);

            if (dotProduct > maxDotProduct)
            {
                maxDotProduct = dotProduct;
                topFaceIndex = i;
            }
        }

        CurrentFace = faceValues[topFaceIndex];

        Debug.Log($"<color=cyan>{gameObject.name} berhenti!</color> Hasilnya <b>{CurrentFace}</b>");
    }
}
