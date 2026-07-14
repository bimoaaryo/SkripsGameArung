using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogArung : MonoBehaviour
{
    public static DialogArung Instance     { get; private set; }

    public static DialogArung InstanceAwal { get; private set; }

    [Header("Pengaturan Instance")]
    [Tooltip("Aktifkan HANYA pada GameObject DialogArung yang dipakai untuk dialog awal level " +
             "(tengah layar, tunggu sentuhan). Nonaktifkan untuk dialog reaksi obstacle/pedagang " +
             "(pojok kiri bawah, auto fade).")]
    public bool adalahDialogAwal = false;

    [Header("UI")]
    public Image    portraitImage;
    public TMP_Text dialogText;
    public TMP_Text namaText;

    [Header("Pengaturan")]
    public string namaArung    = "Arung";
    [Min(1f)]   public float durasi       = 4f;
    [Min(0.1f)] public float fadeDuration = 0.3f;

    [Header("Mode Tunggu Sentuhan")]
    [Tooltip("Teks petunjuk yang muncul di bawah dialog — misal 'Ketuk untuk melanjutkan'")]
    public TMP_Text teksKetuk;

    [Tooltip("Aktifkan agar panel dialog otomatis diposisikan di tengah layar " +
             "saat mode Tunggu Sentuhan dipakai. Bisa juga diatur manual via " +
             "RectTransform Inspector (Anchor: Middle Center, Pos X/Y: 0).")]
    public bool posisiTengah = true;

    [Header("Ekspresi Portrait")]
    [Tooltip("Daftar sprite untuk setiap ekspresi.")]
    public EkspresiSprite[] daftarEkspresi;

    private CanvasGroup   canvasGroup;
    private RectTransform rectTransform;
    private Coroutine     activeRoutine;
    private Vector2       posisiAwal;
    private Vector2       anchorMinAwal;
    private Vector2       anchorMaxAwal;
    private Vector2       pivotAwal;
    private bool          menungguSentuhan = false;

    public bool IsShowing => canvasGroup != null && canvasGroup.alpha > 0.01f;

    void Awake()
    {
        if (adalahDialogAwal)
        {
            if (InstanceAwal == null) InstanceAwal = this;
            else { Destroy(gameObject); return; }
        }
        else
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        canvasGroup   = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (namaText != null)
            namaText.text = namaArung;

        canvasGroup.alpha          = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(true);

        if (rectTransform != null)
        {
            posisiAwal    = rectTransform.anchoredPosition;
            anchorMinAwal = rectTransform.anchorMin;
            anchorMaxAwal = rectTransform.anchorMax;
            pivotAwal     = rectTransform.pivot;
        }

        if (teksKetuk != null)
            teksKetuk.gameObject.SetActive(false);
    }

    void Update()
    {
        // Deteksi sentuhan/klik saat mode tunggu sentuhan aktif
        if (!menungguSentuhan) return;

        bool disentuh = Input.touchCount > 0 || Input.GetMouseButtonDown(0);
        if (disentuh)
        {
            menungguSentuhan = false;
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(SelesaiDialogDanResumeGame());
        }
    }


    public void TampilkanDialog(string teks, EkspresiArung ekspresi = EkspresiArung.Flat)
    {
        if (string.IsNullOrEmpty(teks)) return;
        if (dialogText != null) dialogText.text = teks;

        SetEkspresi(ekspresi);
        SetPosisiNormal();

        if (teksKetuk != null) teksKetuk.gameObject.SetActive(false);

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(DialogRoutine());

        Debug.Log($"[DialogArung] Auto ({ekspresi}): \"{teks}\"");
    }


    public void TampilkanDialogManual(string teks, EkspresiArung ekspresi = EkspresiArung.Flat)
    {
        if (string.IsNullOrEmpty(teks)) return;
        if (dialogText != null) dialogText.text = teks;

        SetEkspresi(ekspresi);
        SetPosisiNormal();

        if (teksKetuk != null) teksKetuk.gameObject.SetActive(false);

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(FadeInSaja());
    }


    public void TampilkanDialogTungguSentuhan(string teks, EkspresiArung ekspresi = EkspresiArung.Flat)
    {
        if (string.IsNullOrEmpty(teks)) return;
        if (dialogText != null) dialogText.text = teks;

        SetEkspresi(ekspresi);

        if (teksKetuk != null)
            teksKetuk.gameObject.SetActive(true);

        Time.timeScale = 0f;
        menungguSentuhan = false; 

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(FadeInLaluTunggu());

        Debug.Log($"[DialogArung] TungguSentuhan ({ekspresi}): \"{teks}\"");
    }

    public void SembunyikanDialog()
    {
        menungguSentuhan = false;
        if (teksKetuk != null) teksKetuk.gameObject.SetActive(false);
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(FadeOut());
    }

 

    void SetEkspresi(EkspresiArung ekspresi)
    {
        if (portraitImage == null) return;
        Sprite sprite = GetSprite(ekspresi);
        if (sprite != null) portraitImage.sprite = sprite;
    }

    public Sprite GetSprite(EkspresiArung ekspresi)
    {
        if (daftarEkspresi == null) return null;

        foreach (var entry in daftarEkspresi)
        {
            if (entry.ekspresi == ekspresi && entry.sprite != null)
                return entry.sprite;
        }

        Debug.LogWarning($"[DialogArung] Sprite untuk ekspresi '{ekspresi}' belum diisi di Daftar Ekspresi.");
        return null;
    }

    void SetPosisiNormal()
    {
        if (rectTransform == null) return;

        rectTransform.anchorMin        = anchorMinAwal;
        rectTransform.anchorMax        = anchorMaxAwal;
        rectTransform.pivot            = pivotAwal;
        rectTransform.anchoredPosition = posisiAwal;
    }


    IEnumerator DialogRoutine()
    {
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSecondsRealtime(durasi);
        yield return StartCoroutine(FadeOut());
    }

    IEnumerator FadeInSaja()
    {
        canvasGroup.blocksRaycasts = false;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed           += Time.unscaledDeltaTime;
            canvasGroup.alpha  = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha          = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    IEnumerator FadeInLaluTunggu()
    {
        canvasGroup.blocksRaycasts = false;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed           += Time.unscaledDeltaTime;
            canvasGroup.alpha  = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha          = 1f;
        canvasGroup.blocksRaycasts = true;

   
        yield return null;

        menungguSentuhan = true;
    }

    IEnumerator SelesaiDialogDanResumeGame()
    {
        if (teksKetuk != null) teksKetuk.gameObject.SetActive(false);

        yield return StartCoroutine(FadeOut());

        SetPosisiNormal();

        Time.timeScale = 1f;

        Debug.Log("[DialogArung] Dialog selesai — game di-resume.");
    }

    IEnumerator FadeIn()
    {
        canvasGroup.blocksRaycasts = false;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed           += Time.unscaledDeltaTime;
            canvasGroup.alpha  = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha          = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float start   = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed           += Time.unscaledDeltaTime;
            canvasGroup.alpha  = Mathf.Lerp(start, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha          = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}



[System.Serializable]
public class EkspresiSprite
{
    public EkspresiArung ekspresi;
    public Sprite        sprite;
}