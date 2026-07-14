using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class KonfirmasiMenuPanel : MonoBehaviour
{

    [Header("Tombol")]
    public Button   tombolYa;
    public Button   tombolTidak;

    [Header("Teks")]
    public TMP_Text pesanText;
    public string   pesan = "Kembali ke menu utama?\nProgress level ini akan hilang.";

    [Header("Scene")]
    public string namaSceneMenu = "MainMenu";

    [Header("Cutscene (opsional)")]
    [Tooltip("Assign CutsceneVideoPlayer jika panel ini dipakai di scene Cutscene. " +
             "Video akan di-resume otomatis saat pemain memilih Tidak (lanjut tonton).")]
    public CutsceneVideoPlayer cutscenePlayer;

    void Awake()
    {
        tombolYa?.onClick.AddListener(KembaliKeMenu);
        tombolTidak?.onClick.AddListener(Tutup);

        gameObject.SetActive(false);
    }

    public void Buka()
    {
        if (pesanText != null)
            pesanText.text = pesan;

        gameObject.SetActive(true);
        Time.timeScale = 0f;
        IdleTimeoutManager.Instance?.PauseTimeout();

        Debug.Log("[KonfirmasiMenuPanel] Panel dibuka.");
    }

    public void Tutup()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        IdleTimeoutManager.Instance?.ResumeTimeout();

        cutscenePlayer?.ResumeVideo();

        Debug.Log("[KonfirmasiMenuPanel] Panel ditutup.");
    }


    void KembaliKeMenu()
    {
        Time.timeScale = 1f;
        GuldenManager.Instance?.ResetAll();

        Debug.Log("[KonfirmasiMenuPanel] Kembali ke Main Menu.");
        SceneLoaderRunner.Instance?.LoadScene(namaSceneMenu);
    }
}