using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("UI Notif Merah (Reroll Warning)")]
    public GameObject rerollWarningNotif;
    public float displayDuration = 3f;
    [Tooltip("Geser dari KIRI ke KANAN = X nya minus (-1000)")]
    public Vector2 rerollSlideOffset = new Vector2(-1000f, 0f);

    [Header("Global Animation Settings")]
    public bool useAnimation = true;
    public float animDuration = 0.3f;
    [Tooltip("Geser dari KANAN ke KIRI = X nya plus (1000)")]
    public Vector2 defaultSlideOffset = new Vector2(1000f, 0f);

    private Dictionary<GameObject, Vector2> originalPositions = new Dictionary<GameObject, Vector2>();
    private Coroutine autoHideCoroutine;

    private void Start()
    {
        if (rerollWarningNotif != null) rerollWarningNotif.SetActive(false);
    }

    private void OnEnable()
    {
        TurnManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        TurnManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(TurnManager.TurnPhase phase)
    {
        if (phase == TurnManager.TurnPhase.RerollPhase)
        {
            if (TurnManager.Instance.CurrentPlayerIndex == 0)
            {
                ShowNotification(rerollWarningNotif);
                
                if (autoHideCoroutine != null) StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = StartCoroutine(AutoHideRoutine(rerollWarningNotif, displayDuration));
            }
        }
        else if (phase == TurnManager.TurnPhase.Resolution || phase == TurnManager.TurnPhase.TurnEnd || phase == TurnManager.TurnPhase.FirstRoll)
        {
            HideNotification(rerollWarningNotif);
            
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
        }
    }

    private IEnumerator AutoHideRoutine(GameObject notifObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        HideNotification(notifObj);
    }

    public void ShowNotification(GameObject notifObj)
    {
        if (notifObj == null) return;

        if (useAnimation) StartCoroutine(SlideAnimation(notifObj, true));
        else notifObj.SetActive(true);
    }

    public void HideNotification(GameObject notifObj)
    {
        if (notifObj == null || !notifObj.activeSelf) return; 

        if (useAnimation) StartCoroutine(SlideAnimation(notifObj, false));
        else notifObj.SetActive(false);
    }

    private Vector2 GetOriginalPosition(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (!originalPositions.ContainsKey(obj))
        {
            originalPositions[obj] = rect.anchoredPosition;
        }
        return originalPositions[obj];
    }

    private IEnumerator SlideAnimation(GameObject obj, bool isShowing)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector2 visiblePos = GetOriginalPosition(obj);
        
        Vector2 offsetToUse = (obj == rerollWarningNotif) ? rerollSlideOffset : defaultSlideOffset;
        Vector2 hiddenPos = visiblePos + offsetToUse;

        Vector2 startPos = isShowing ? hiddenPos : visiblePos;
        Vector2 targetPos = isShowing ? visiblePos : hiddenPos;

        rect.anchoredPosition = startPos;
        if (isShowing) obj.SetActive(true); 

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); 

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        rect.anchoredPosition = targetPos;

        if (!isShowing) 
        {
            obj.SetActive(false);
            rect.anchoredPosition = visiblePos; 
        }
    }
}