using UnityEngine;
using UnityEngine.UI;

public class DiceUIManager : MonoBehaviour
{
    [Header("Wadah dan Cetakan UI")]
    [Tooltip("Masukkan Panel Locked Dice Area ke sini...")]
    [SerializeField] private Transform lockedDiceArea;

    [Tooltip("Masukkan Prefab UI Dice Item ke sini...")]
    [SerializeField] private GameObject uiDicePrefab;
    
    [Header("Katalog Ikon Dadu")]
    [Tooltip("Masukkan gambar icon 2D dadu ke sini...")]
    [SerializeField] private Sprite[] diceFaceIcon;

    /// <summary>
    /// </summary>

    public void AddLockedDiceUI(int faceIndex)
    {
        GameObject newDiceUI = Instantiate(uiDicePrefab, lockedDiceArea);
        Image diceImage = newDiceUI.GetComponent<Image>();

        if (diceImage != null && faceIndex >= 0 && faceIndex < diceFaceIcon.Length)
        {
            diceImage.sprite = diceFaceIcon[faceIndex];
        }
        else
        {
            Debug.Log("Error UI: Gambar icon belum dimasukkan atau index melebihi batas!");
        }
    }

        /// <summary>
        /// </summary>>

    public void ClearLockedDice()
    {
        foreach (Transform child in lockedDiceArea)
        {
            Destroy(child.gameObject);
        }
    }
}
