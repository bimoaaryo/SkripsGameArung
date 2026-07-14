using UnityEngine;
using System.Collections;

public class InteractionIndicator : MonoBehaviour
{

    [Header("Animasi Bounce")]
    [Tooltip("Tinggi bounce naik turun")]
    [Min(0f)] public float bounceHeight    = 0.15f;
    [Tooltip("Kecepatan bounce")]
    [Min(0f)] public float bounceSpeed     = 2f;

    [Header("Animasi Scale Pulse")]
    [Tooltip("Aktifkan efek scale pulse")]
    public bool  enablePulse   = true;
    [Tooltip("Skala maksimal saat pulse")]
    [Min(1f)] public float pulseScale      = 1.15f;
    [Tooltip("Kecepatan pulse")]
    [Min(0f)] public float pulseSpeed      = 3f;

    [Header("Fade")]
    [Tooltip("Durasi fade in saat muncul")]
    [Min(0.1f)] public float fadeDuration  = 0.3f;

    [Header("Kontrol")]
    [Tooltip("Tampilkan otomatis saat scene dimulai")]
    public bool tampilDiAwal   = true;

    private SpriteRenderer  sr;
    private Vector3         posisiAwal;
    private bool            aktif         = false;
    private Coroutine       fadeRoutine;


    void Awake()
    {
        sr        = GetComponent<SpriteRenderer>();
        posisiAwal = transform.localPosition;

        if (sr != null) sr.color = new Color(1f, 1f, 1f, 0f);
    }

    void Start()
    {
        if (tampilDiAwal)
            Tampilkan();
        else
            Sembunyikan();
    }

    void Update()
    {
        if (!aktif) return;

        float bounce         = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        transform.localPosition = new Vector3(
            posisiAwal.x,
            posisiAwal.y + bounce,
            posisiAwal.z);

        if (enablePulse)
        {
            float scale              = 1f + (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f)
                                          * (pulseScale - 1f);
            transform.localScale     = Vector3.one * scale;
        }
    }

    public void Tampilkan()
    {
        if (aktif) return;
        aktif = true;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeIn());
    }

    public void Sembunyikan()
    {
        if (!aktif) return;
        aktif = false;

        transform.localScale    = Vector3.one;
        transform.localPosition = posisiAwal;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOut());
    }

    public void SembunyikanLangsung()
    {
        aktif = false;
        transform.localScale    = Vector3.one;
        transform.localPosition = posisiAwal;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        if (sr != null) sr.color = new Color(1f, 1f, 1f, 0f);

        gameObject.SetActive(false);
    }



    IEnumerator FadeIn()
    {
        gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed     += Time.deltaTime;
            float alpha  = Mathf.Clamp01(elapsed / fadeDuration);
            if (sr != null) sr.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        if (sr != null) sr.color = Color.white;
    }

    IEnumerator FadeOut()
    {
        float elapsed  = 0f;
        float startAlpha = sr != null ? sr.color.a : 1f;

        while (elapsed < fadeDuration)
        {
            elapsed     += Time.deltaTime;
            float alpha  = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            if (sr != null) sr.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        if (sr != null) sr.color = new Color(1f, 1f, 1f, 0f);
        gameObject.SetActive(false);
    }
}