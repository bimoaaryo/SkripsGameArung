using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    public enum FitMode
    {
        Fill,
        Fit
    }

    [Header("Pengaturan")]
    public FitMode fitMode = FitMode.Fill;

    [Tooltip("Kamera yang dijadikan referensi. Kosongkan = pakai Camera.main")]
    public Camera targetCamera;

    void Start()
    {
        SesuaikanSkala();
    }

    void SesuaikanSkala()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        float tinggiKamera = cam.orthographicSize * 2f;
        float lebarKamera  = tinggiKamera * cam.aspect;

        float lebarSprite  = sr.sprite.bounds.size.x;
        float tinggiSprite = sr.sprite.bounds.size.y;

        float skalaX = lebarKamera  / lebarSprite;
        float skalaY = tinggiKamera / tinggiSprite;

        float skalaAkhir = fitMode == FitMode.Fill
            ? Mathf.Max(skalaX, skalaY)  
            : Mathf.Min(skalaX, skalaY);

        transform.localScale = new Vector3(skalaAkhir, skalaAkhir, 1f);

        Debug.Log($"[BackgroundFitter] Skala: {skalaAkhir:F2} | " +
                  $"Layar: {lebarKamera:F1}x{tinggiKamera:F1} | " +
                  $"Sprite: {lebarSprite:F1}x{tinggiSprite:F1}");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) SesuaikanSkala();
    }
#endif
}