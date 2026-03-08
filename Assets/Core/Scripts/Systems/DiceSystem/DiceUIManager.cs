using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceUIManager : MonoBehaviour
{
    public static DiceUIManager Instance { get; private set; }

    [Header("Wadah dan Cetakan UI")]
    [Tooltip("Masukkan Panel Locked Dice Area ke sini...")]
    [SerializeField] private Transform lockedDiceArea;

    [Tooltip("Masukkan Prefab UI Dice Item ke sini...")]
    [SerializeField] private GameObject uiDicePrefab;
    
    /*[Header("Katalog Ikon Dadu")]
    [Tooltip("Masukkan gambar icon 2D dadu ke sini...")]
    [SerializeField] private Sprite[] diceFaceIcon;
        */

    [Header("Katalog Placeholder (Testing)")]
    [SerializeField] private Color[] diceFaceColors;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddLockedDiceUI(Dice diceRef)
    {
        GameObject newDiceUI = Instantiate(uiDicePrefab, lockedDiceArea);

        Image diceImage = newDiceUI.GetComponent<Image>();
        TextMeshProUGUI textLabel = newDiceUI.GetComponentInChildren<TextMeshProUGUI>();
        int faceIndex = (int)diceRef.CurrentFace;

        if (diceImage != null && faceIndex >= 0 && faceIndex < diceFaceColors.Length)
            diceImage.color = diceFaceColors[faceIndex];

        if (textLabel != null)
            textLabel.text = diceRef.CurrentFace.ToString();

        Button btn = newDiceUI.GetComponent<Button>();
        if (btn == null) btn = newDiceUI.AddComponent<Button>();

        btn.onClick.AddListener(() =>
        {
            DiceManager.Instance.UnlockDice(diceRef);
            Destroy(newDiceUI);
        });
    }

    public void ClearLockedDice()
    {
        foreach (Transform child in lockedDiceArea)  Destroy(child.gameObject);
    }
}
