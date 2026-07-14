using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageSwitcher : MonoBehaviour
{
    [Header("Tombol Bahasa")]
    public Button indonesianButton;
    public Button englishButton;

    [Header("Indikator Bahasa Aktif (opsional)")]
    public TMP_Text currentLanguageText;

    [Header("Warna Tombol")]
    public Color activeColor   = new Color(0.2f, 0.8f, 0.3f, 1f);
    public Color inactiveColor = Color.white;

    [Header("Audio (opsional)")]
    public AudioSource audioSource;
    public AudioClip   switchSound;

    private bool sudahSubscribe = false;
    private bool sudahDaftarTombol = false;

    void Start()
    {
        if (!sudahDaftarTombol)
        {
            indonesianButton?.onClick.AddListener(SwitchToIndonesian);
            englishButton?.onClick.AddListener(SwitchToEnglish);
            sudahDaftarTombol = true;
        }

        TrySubscribe();
        RefreshUI();
    }

    void OnEnable()
    {
        TrySubscribe();
        RefreshUI();
    }

    void TrySubscribe()
    {
        if (sudahSubscribe) return;
        if (LocalizationManager.Instance == null) return;

        LocalizationManager.Instance.OnLanguageChanged.AddListener(RefreshUI);
        sudahSubscribe = true;
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged.RemoveListener(RefreshUI);
        sudahSubscribe = false;
    }


    public void SwitchToIndonesian()
    {
        LocalizationManager.Instance?.SetIndonesian();
        PlaySound();
    }

    public void SwitchToEnglish()
    {
        LocalizationManager.Instance?.SetEnglish();
        PlaySound();
    }


    void RefreshUI()
    {
        bool isEnglish = LocalizationManager.Instance?.IsEnglish ?? false;

        if (indonesianButton != null)
        {
            var colors = indonesianButton.colors;
            colors.normalColor = !isEnglish ? activeColor : inactiveColor;
            indonesianButton.colors = colors;
        }

        if (englishButton != null)
        {
            var colors = englishButton.colors;
            colors.normalColor = isEnglish ? activeColor : inactiveColor;
            englishButton.colors = colors;
        }

        if (currentLanguageText != null)
            currentLanguageText.text = isEnglish ? "Language: English" : "Bahasa: Indonesia";
    }

    void PlaySound()
    {
        if (audioSource != null && switchSound != null)
            audioSource.PlayOneShot(switchSound);
    }
}