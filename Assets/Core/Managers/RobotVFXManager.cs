using System.Collections;
using UnityEngine;

public class RobotVFXManager : MonoBehaviour
{
    [Header("Combat Anchors (Titik Tembak & Target)")]
    public Transform hitPoint;
    public Transform attackMuzzle_1;
    public Transform attackMuzzle_2;
    public Transform attackMuzzle_3;
    public Transform destructionMuzzle_1;
    public Transform destructionMuzzle_2;
    public Transform destructionMuzzle_3;

    [Header("Combat VFX (Tarik Prefab Peluru dari Folder)")]
    public GameObject attackVFX_1; 
    public GameObject attackVFX_2; 
    public GameObject attackVFX_3; 
    public GameObject destructionVFX_1; 
    public GameObject destructionVFX_2; 
    public GameObject destructionVFX_3; 
    
    [Header("Personal VFX (Aura Nempel di Badan)")]
    public GameObject healVFX;       
    public GameObject braggingVFX;   
    public GameObject gotPowerVFX;   

    public void PlayHealVFX(float animDuration)
    {
        if (healVFX != null) { healVFX.SetActive(true); StartCoroutine(TurnOffVFX(healVFX, animDuration)); }
    }

    public void PlayGotPowerVFX(float animDuration)
    {
        if (gotPowerVFX != null) { gotPowerVFX.SetActive(true); StartCoroutine(TurnOffVFX(gotPowerVFX, animDuration)); }
    }

    public void PlayBraggingVFX(float animDuration)
    {
        if (braggingVFX != null) { braggingVFX.SetActive(true); StartCoroutine(TurnOffVFX(braggingVFX, animDuration)); }
    }

    private IEnumerator TurnOffVFX(GameObject vfx, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (vfx != null) vfx.SetActive(false);
    }
}