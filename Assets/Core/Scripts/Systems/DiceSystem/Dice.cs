using UnityEngine;
using System.Collections;

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
    private bool isSettling = false;

    [Header("Pengaturan Lemparan")]
    public float throwForce = 10f;
    public float rollTorque = 50f;
    private int nudgeCount = 0;
    private float rollCooldown = 0f;

    [Header("Visual Dadu & Lock")]
    public SpriteRenderer lockIconRenderer;
    public Sprite[] faceSprites;

    private float maxRollTime = 5f;
    private float currentRollTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        originalScale = transform.localScale;
    }

    public void Roll()
    {
        nudgeCount = 0;

        isRolling = true;
        isSettling = false;
        rollCooldown = 0.5f;
        currentRollTime = 0f;

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 throwDirection = (Vector3.up + Random.insideUnitSphere * 0.05f).normalized;

        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * rollTorque, ForceMode.Impulse);
    }

    private void Update()
    {
        if (isRolling && !isSettling)
        {

            currentRollTime += Time.deltaTime;
            if (currentRollTime > maxRollTime)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                isRolling = false;
                DetermineTopFace();
                return;
            }

            if (rollCooldown > 0)
            {
                rollCooldown -= Time.deltaTime;
                return;
            }

            if (rb.linearVelocity.sqrMagnitude < 0.01f && rb.angularVelocity.sqrMagnitude < 0.01f)
            {
                isSettling = true;
                StartCoroutine(CheckAndSettleDice());
            }
        }
    }

    private IEnumerator CheckAndSettleDice()
    {
        yield return new WaitForSeconds(0.2f);

        if (rb.linearVelocity.sqrMagnitude > 0.05f || rb.angularVelocity.sqrMagnitude > 0.05f)
        {
            isSettling = false;
            yield break;
        }

        Vector3[] localDirections = new Vector3[]
        {
            transform.up,
            -transform.up,
            transform.right,
            -transform.right,
            transform.forward,
            -transform.forward
        };

        float maxDotProduct = -Mathf.Infinity;
        Vector3 topDirection = Vector3.up;

        for (int i = 0; i < localDirections.Length; i++)
        {
            float dotProduct = Vector3.Dot(localDirections[i], Vector3.up);
            if (dotProduct > maxDotProduct)
            {
                maxDotProduct = dotProduct;
                topDirection = localDirections[i];
            }
        }

        float tiltAngle = Vector3.Angle(topDirection, Vector3.up);
        bool isStacked = false;
        float checkDistance = (transform.localScale.y / 2f) + 0.1f;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, checkDistance);
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.GetComponent<Dice>() != null && hit.collider.gameObject != this.gameObject)
                {
                    isStacked = true;
                    Debug.Log($"<color=orange>{gameObject.name} Terdeteksi bertumpuk dengan {hit.collider.gameObject.name}!</color>");
                    break;
                }
            }
        }

        if ((tiltAngle > 15f || isStacked) && nudgeCount < 3)
        {
            nudgeCount++;
            string reason = isStacked ? "bertumpuk" : $"miring ({tiltAngle:F1} derajat)";
            Debug.Log($"<color=yellow>{gameObject.name} masih belum stabil karena {reason}. Memberi dorongan ke udara...</color>");

            isSettling = false;
            rollCooldown = 0.5f;

            Vector3 centerPos = DiceManager.Instance.spawnPoint.position;
            centerPos.y = transform.position.y;
            Vector3 directionToCenter = (centerPos - transform.position).normalized;

            float gentleForce = Random.Range(1.5f, 2.5f);

            rb.AddForce((Vector3.up + directionToCenter) * gentleForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
        }
        else
        {
            isRolling = false;
            DetermineTopFace();
        }
    }

    public void ToggleLock()
    {
        if (isRolling) return;

        if (TurnManager.Instance.CurrentPlayerIndex != 0)
        {
            Debug.Log($"<color=red>Hanya pemain utama yang bisa mengunci dadu!</color>");
            return;
        }

        DiceManager.Instance.LockDice(this);

        if (lockIconRenderer != null && faceSprites.Length > 0)
        {
            int faceIndex = (int)CurrentFace;

            if (faceIndex >= 0 && faceIndex < faceSprites.Length)
            {
                lockIconRenderer.sprite = faceSprites[faceIndex];
            }

            lockIconRenderer.gameObject.SetActive(this.isLocked);
        }
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
            DiceFace.SpecialPower,
            DiceFace.Energy,
            DiceFace.Smash,
            DiceFace.Heal,
            DiceFace.Fame,
            DiceFace.Destruction
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

        Debug.DrawRay(transform.position, localDirections[topFaceIndex] * 3f, Color.red, 10f);

        rb.isKinematic = true;
    }
}
