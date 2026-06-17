using UnityEngine;

public class ArenaTile : MonoBehaviour
{
    [Header("Buzz Tile Settings")]
    public bool isBuzzTileSlot = true; 
    public string activeBuzzEffectID = ""; 
    
    [Tooltip("Masukin partikel nyala di sini (Opsional)")]
    public GameObject trapVFX; 

    public void SetBuzzTrap(string buzzID)
    {
        activeBuzzEffectID = buzzID;
        if (trapVFX != null) trapVFX.SetActive(true);
    }

    public void ClearBuzzTrap()
    {
        activeBuzzEffectID = "";
        if (trapVFX != null) trapVFX.SetActive(false);
    }
}