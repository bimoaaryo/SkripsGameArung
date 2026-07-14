using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// Memutar video cutscene satu kali (tidak looping), lalu otomatis pindah ke
/// scene tujuan setelah video selesai. Script generik — dipakai di TIGA scene
/// cutscene berbeda dengan sceneTujuan yang berbeda-beda:
///
///   Scene "Cutscene" (Tutorial → Level 1)      → sceneTujuan = "Level1_Maluku"
///   Scene "Cutscene1to2" (Level 1 → Level 2)   → sceneTujuan = "Level2_Batavia"
///   Scene "Cutscene2to3" (Level 2 → Level 3)   → sceneTujuan = "Level3_Malaka"
///
/// SETUP DWIBAHASA:
/// Assign dua VideoClip berbeda di Inspector:
///   videoClip_ID → video versi Bahasa Indonesia
///   videoClip_EN → video versi Bahasa Inggris
/// Script otomatis memilih clip sesuai bahasa aktif saat Start().
///
/// SETUP (ulangi untuk masing-masing scene cutscene di atas):
/// 1. Buat scene baru sesuai nama di atas
/// 2. Buat GameObject "VideoPlayer" → Add Component → Video Player
///    └── Source: Video Clip (akan di-assign via script, tidak perlu di-set manual)
///    └── Play On Awake: ❌ OFF (dikontrol script ini)
///    └── Loop: ❌ OFF
///    └── Camera: drag Main Camera jika pakai Render Mode Camera Far/Near Plane
/// 3. Attach script ini, assign videoPlayer, videoClip_ID, videoClip_EN,
///    sceneTujuan, dan tombolMenu di Inspector
/// </summary>
public class CutsceneVideoPlayer : MonoBehaviour
{
    [Header("Referensi")]
    public VideoPlayer videoPlayer;

    [Header("Video Clip Dwibahasa")]
    [Tooltip("Video cutscene versi Bahasa Indonesia")]
    public VideoClip videoClip_ID;

    [Tooltip("Video cutscene versi Bahasa Inggris")]
    public VideoClip videoClip_EN;

    [Header("Tombol")]
    [Tooltip("Tombol skip — langsung lanjut ke scene tujuan tanpa menunggu video selesai")]
    public Button tombolSkip;

    [Tooltip("Tombol kembali ke menu — membuka KonfirmasiMenuPanel dan mem-pause video")]
    public Button tombolMenu;

    [Header("Panel Konfirmasi Menu (opsional)")]
    public KonfirmasiMenuPanel konfirmasiMenuPanel;

    [Header("Lanjut ke Scene")]
    public string sceneTujuan = "Level1_Maluku";

    private bool sudahPindah = false;

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("[CutsceneVideoPlayer] VideoPlayer belum di-assign!");
            return;
        }

        // Pilih clip sesuai bahasa aktif
        bool english = LocalizationManager.Instance?.IsEnglish ?? false;
        VideoClip clipDipilih = english ? videoClip_EN : videoClip_ID;

        if (clipDipilih == null)
        {
            // Fallback ke clip yang tersedia kalau salah satu kosong
            clipDipilih = videoClip_ID ?? videoClip_EN;
            Debug.LogWarning($"[CutsceneVideoPlayer] VideoClip_{(english ? "EN" : "ID")} " +
                             $"belum di-assign — fallback ke clip yang tersedia.");
        }

        videoPlayer.clip      = clipDipilih;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoSelesai;
        videoPlayer.Play();

        tombolSkip?.onClick.AddListener(LanjutKeTujuan);
        tombolMenu?.onClick.AddListener(BukaKonfirmasiMenu);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoSelesai;
    }

    // -------------------------------------------------------
    // Video Control
    // -------------------------------------------------------

    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Pause();
    }

    public void ResumeVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying && !sudahPindah)
            videoPlayer.Play();
    }

    // -------------------------------------------------------
    // Menu
    // -------------------------------------------------------

    void BukaKonfirmasiMenu()
    {
        PauseVideo();

        if (konfirmasiMenuPanel != null)
            konfirmasiMenuPanel.Buka();
        else
            Debug.LogWarning("[CutsceneVideoPlayer] KonfirmasiMenuPanel belum di-assign!");
    }

    // -------------------------------------------------------
    // Transisi
    // -------------------------------------------------------

    void OnVideoSelesai(VideoPlayer vp)
    {
        LanjutKeTujuan();
    }

    void LanjutKeTujuan()
    {
        if (sudahPindah) return;
        sudahPindah = true;

        SceneLoaderRunner.Instance?.LoadScene(sceneTujuan);
    }
}