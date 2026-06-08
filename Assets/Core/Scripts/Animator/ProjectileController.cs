using UnityEngine;
using DG.Tweening;
using System;

public class ProjectileController : MonoBehaviour
{
    [Header("VFX Settings")]
    public GameObject impactParticlePrefab; 
    public float travelDuration = 0.3f;     
    
    [Header("Juice Settings")]
    public bool addHitStop = true;          

    public void FireProjectile(Transform targetTransform, Action onHitCallback)
    {
        transform.LookAt(targetTransform.position);

        transform.DOMove(targetTransform.position, travelDuration)
            .SetEase(Ease.Linear) 
            .OnComplete(() => 
            {
                if (impactParticlePrefab != null)
                {
                    Instantiate(impactParticlePrefab, targetTransform.position, Quaternion.identity);
                }

                onHitCallback?.Invoke();

                if (addHitStop && VFXManager.Instance != null)
                {
                    VFXManager.Instance.TriggerHitStop(0.05f); 
                }

                ParticleSystem[] pSystems = GetComponentsInChildren<ParticleSystem>();
                foreach(var ps in pSystems) 
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                }

                MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
                foreach(var mesh in meshes)
                {
                    mesh.enabled = false;
                }

                Destroy(gameObject, 2f);
            });
    }
}