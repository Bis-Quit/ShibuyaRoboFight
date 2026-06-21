using UnityEngine;
using System;

public class ArenaTile : MonoBehaviour
{
    [Header("Buzz Tile Settings")]
    public bool isBuzzTileSlot = true; 
    public string activeBuzzEffectID = ""; 
    public GameObject trapVFX; 

    [Header("Manual Selection UI")]
    public GameObject highlightIndicator; 
    
    public static event Action<ArenaTile> OnTileClicked; 
    
    private bool isClickable = false;
    private Vector3 originalHighlightScale;
    private bool isHovered = false;

    private void Start()
    {
        if (highlightIndicator != null)
        {
            originalHighlightScale = highlightIndicator.transform.localScale;
        }
    }

    private void Update()
    {
        if (isClickable && highlightIndicator != null && !isHovered)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.05f; 
            highlightIndicator.transform.localScale = originalHighlightScale * pulse;
        }
    }

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

    public void SetClickable(bool state)
    {
        isClickable = state;
        isHovered = false;

        if (highlightIndicator != null) 
        {
            highlightIndicator.SetActive(state);
            if (!state) highlightIndicator.transform.localScale = originalHighlightScale;
        }
    }

    private void OnMouseEnter()
    {
        if (isClickable && highlightIndicator != null)
        {
            isHovered = true;
            highlightIndicator.transform.localScale = originalHighlightScale * 1.15f;
        }
    }

    private void OnMouseExit()
    {
        if (isClickable && highlightIndicator != null)
        {
            isHovered = false;
        }
    }

    private void OnMouseDown()
    {
        if (isClickable)
        {
            Debug.Log($"<color=yellow>Tile {gameObject.name} Diklik!</color>");
            if (highlightIndicator != null)
            {
                highlightIndicator.transform.localScale = originalHighlightScale * 0.85f;
            }

            OnTileClicked?.Invoke(this);
        }
    }
}