using UnityEngine;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    [Header("Settings")]
    public int characterIndex;
    public Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1.2f); 
    public Vector3 normalScale = Vector3.one; 
    public float lerpSpeed = 10f; 

    [Header("Movement Settings")]
    public float shiftDistance = 30f;
    private Vector2 originalPos;
    private RectTransform rect;

    [Header("Visual Swap (Versi Tim Desain)")]
    public Image targetImage; 
    public Sprite normalSprite; 
    public Sprite selectedSprite; 

    private bool isSelected = false;
    private CharacterSelectionManager manager;

    void Awake()
    {
        manager = UnityEngine.Object.FindObjectOfType<CharacterSelectionManager>();
        rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        originalPos = rect.anchoredPosition;
    }

    void Update()
    {
        Vector3 targetScale = isSelected ? selectedScale : normalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * lerpSpeed);

        Vector2 targetPos = originalPos;
        
        if (isSelected)
        {
            if (characterIndex % 2 == 0) 
            {
                targetPos.x = originalPos.x + shiftDistance;
            }
            else
            {
                targetPos.x = originalPos.x - shiftDistance;
            }
        }

        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * lerpSpeed);
    }

    public void SetSelected(bool state)
    {
        isSelected = state;
        
        if (targetImage != null && normalSprite != null && selectedSprite != null)
        {
            targetImage.sprite = state ? selectedSprite : normalSprite;
        }
    }

    public void OnClickButton()
    {
        manager.PreviewCharacter(characterIndex);
    }
}