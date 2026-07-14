using UnityEngine;
using UnityEngine.UI;

public class FinishZoneIndicator : MonoBehaviour
{

    [Header("Referensi")]
    [Tooltip("SpriteRenderer ikon di atas (anchor, bendera, bintang, dll)")]
    public SpriteRenderer iconRenderer;

    [Tooltip("SpriteRenderer lingkaran pulse di bawah ikon")]
    public SpriteRenderer pulseRenderer;

    [Header("Bounce Ikon")]
    [Tooltip("Tinggi pantulan ikon dari posisi asal (dalam unit Unity)")]
    [Range(0.05f, 1f)] public float bounceHeight = 0.25f;

    [Tooltip("Kecepatan naik-turun ikon")]
    [Range(0.5f, 5f)]  public float bounceSpeed  = 2f;


    [Header("Pulse Ring")]
    [Tooltip("Ukuran minimum lingkaran pulse")]
    [Range(0.1f, 2f)]  public float pulseScaleMin = 0.6f;

    [Tooltip("Ukuran maksimum lingkaran pulse saat mengembang")]
    [Range(0.5f, 4f)]  public float pulseScaleMax = 2f;

    [Tooltip("Alpha lingkaran saat ukuran minimum (paling solid)")]
    [Range(0f, 1f)]    public float pulseAlphaMax = 0.5f;

    [Tooltip("Kecepatan pulse mengembang")]
    [Range(0.3f, 4f)]  public float pulseSpeed    = 1.2f;

    [Header("Tampilan")]
    [Tooltip("Warna ikon — sesuaikan dengan palet UI game")]
    public Color iconColor  = Color.white;

    [Tooltip("Warna pulse ring")]
    public Color pulseColor = new Color(1f, 0.85f, 0.2f, 1f); // kuning

    [Tooltip("Nonaktifkan untuk menyembunyikan indikator (misal saat level selesai)")]
    public bool  visible    = true;

    private Vector3 iconOrigin;


    void Start()
    {
        if (iconRenderer  != null) iconRenderer.color  = iconColor;
        if (pulseRenderer != null)
        {
            Color c = pulseColor;
            c.a              = 0f;
            pulseRenderer.color = c;
        }

        iconOrigin = iconRenderer != null
            ? iconRenderer.transform.localPosition
            : Vector3.zero;
    }

    void Update()
    {
        if (!visible)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        UpdateBounce();
        UpdatePulse();
    }


    void UpdateBounce()
    {
        if (iconRenderer == null) return;

        float offsetY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        iconRenderer.transform.localPosition = iconOrigin + new Vector3(0f, offsetY, 0f);
    }


    void UpdatePulse()
    {
        if (pulseRenderer == null) return;

        float t     = Mathf.Repeat(Time.time * pulseSpeed, 1f);
        float scale = Mathf.Lerp(pulseScaleMin, pulseScaleMax, t);
        float alpha = Mathf.Lerp(pulseAlphaMax, 0f, t);

        pulseRenderer.transform.localScale = Vector3.one * scale;

        Color c = pulseColor;
        c.a              = alpha;
        pulseRenderer.color = c;
    }


    public void Tampilkan()
    {
        visible = true;
    }

    public void Sembunyikan()
    {
        visible = false;
    }


    void SetVisible(bool state)
    {
        if (iconRenderer  != null) iconRenderer.enabled  = state;
        if (pulseRenderer != null) pulseRenderer.enabled = state;
    }
}