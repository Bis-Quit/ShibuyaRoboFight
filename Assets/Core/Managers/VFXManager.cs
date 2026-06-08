using UnityEngine;
using System.Collections;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void TriggerHitStop(float duration)
    {
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f; 
    }
}