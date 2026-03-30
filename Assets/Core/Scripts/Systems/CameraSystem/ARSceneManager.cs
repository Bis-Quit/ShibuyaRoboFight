using UnityEngine;
using Vuforia; 

public class ARSceneManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Menyalakan mesin Vuforia...");
        VuforiaApplication.Instance.Initialize();
    }

    void OnDestroy()
    {
        Debug.Log("Mematikan kamera Vuforia...");
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.enabled = false; 
        }
        
        VuforiaApplication.Instance.Deinit(); 
    }
}