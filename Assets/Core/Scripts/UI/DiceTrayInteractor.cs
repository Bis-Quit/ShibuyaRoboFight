using UnityEngine;
using UnityEngine.EventSystems;

public class DiceTrayInteractor : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Camera _diceCamera;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("1. Layar TV Nampan berhasil diklik!");

        if (_diceCamera == null) return;

        RectTransform rectTransform = GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Rect rect = rectTransform.rect;
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;
            Vector2 viewportPoint = new Vector2(normalizedX, normalizedY);

            Ray ray = _diceCamera.ViewportPointToRay(viewportPoint);
            
            // Kita gambar lasernya di Scene view biar kelihatan (warna merah)
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"2. Laser kena objek: {hit.collider.gameObject.name}");

                Dice clickedDice = hit.collider.GetComponent<Dice>();
                if (clickedDice != null)
                {
                    Debug.Log("3. Dadu berhasil di-lock dari UI!");
                    clickedDice.ToggleLock();
                }
            }
            else
            {
                Debug.LogWarning("2. Laser meleset, gak kena apa-apa di dalam nampan.");
            }
        }
    }
}