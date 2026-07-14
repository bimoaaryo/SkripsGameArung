using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdminResetPanel : MonoBehaviour
{


    [Header("Info Leaderboard")]
    public TMP_Text infoText;

    [Header("Reset Leaderboard")]
    public Button tombolReset;
    public Button tombolBatal;

    [Header("PIN Reset (opsional)")]
    public bool           gunakaPIN  = false;
    public string         pinRahasia = "1234";
    public TMP_InputField inputPIN;
    public TMP_Text       pesanPIN;


    [Header("Tutup Aplikasi")]
    public Button         tombolTutupAplikasi;
    public GameObject     panelPasswordTutup;
    public TMP_InputField inputPasswordTutup;
    public TMP_Text       pesanPasswordTutup;
    public Button         tombolKonfirmasiTutup;
    public Button         tombolBatalTutup;
    public string         passwordTutup = "admin123";


    [Header("Tap Logo")]
    [Min(3)] public int   jumlahTap   = 5;
    [Min(1f)] public float tapInterval = 2f;

    private int   tapCount    = 0;
    private float lastTapTime = 0f;


    void Start()
    {
        gameObject.SetActive(false);

        tombolReset?.onClick.AddListener(OnTombolReset);
        tombolBatal?.onClick.AddListener(TutupPanel);

        tombolTutupAplikasi?.onClick.AddListener(BukaPanelPasswordTutup);
        tombolKonfirmasiTutup?.onClick.AddListener(OnKonfirmasiTutup);
        tombolBatalTutup?.onClick.AddListener(TutupPanelPasswordTutup);

        if (panelPasswordTutup != null)
            panelPasswordTutup.SetActive(false);
    }



    public void OnLogoTap()
    {
        if (Time.time - lastTapTime > tapInterval)
            tapCount = 0;

        lastTapTime = Time.time;
        tapCount++;

        Debug.Log($"[AdminReset] Tap {tapCount}/{jumlahTap}");

        if (tapCount >= jumlahTap)
        {
            tapCount = 0;
            BukaPanel();
        }
    }


    void BukaPanel()
    {
        gameObject.SetActive(true);

        int totalEntri = DatabaseManager.Instance?.GetTopScores().Count ?? 0;

        if (infoText != null)
            infoText.text = totalEntri > 0
                ? $"Leaderboard memiliki {totalEntri} entri.\nApakah yakin ingin mereset?"
                : "Leaderboard sudah kosong.";

        if (inputPIN != null)
        {
            inputPIN.text = "";
            inputPIN.gameObject.SetActive(gunakaPIN);
        }

        if (pesanPIN    != null) pesanPIN.text = "";

        Debug.Log("[AdminReset] Panel admin terbuka.");
    }

    void TutupPanel()
    {
        if (tombolReset != null)
            tombolReset.interactable = true;

        if (panelPasswordTutup != null)
            panelPasswordTutup.SetActive(false);

        gameObject.SetActive(false);
    }


    void OnTombolReset()
    {
        if (gunakaPIN)
        {
            if (inputPIN == null || inputPIN.text != pinRahasia)
            {
                if (pesanPIN != null) pesanPIN.text = "PIN salah!";
                Debug.LogWarning("[AdminReset] PIN salah.");
                return;
            }
        }

        DatabaseManager.Instance?.ResetLeaderboard("Reset manual oleh petugas museum");

        if (infoText != null)
            infoText.text = "Leaderboard berhasil direset.";

        if (tombolReset != null)
            tombolReset.interactable = false;

        Invoke(nameof(TutupPanel), 2f);

        Debug.Log("[AdminReset] Leaderboard direset.");
    }


    void BukaPanelPasswordTutup()
    {
        if (panelPasswordTutup == null) return;

        panelPasswordTutup.SetActive(true);

        if (inputPasswordTutup != null)
        {
            inputPasswordTutup.text        = "";
            inputPasswordTutup.contentType = TMP_InputField.ContentType.Password;
            inputPasswordTutup.Select();
            inputPasswordTutup.ActivateInputField();
        }

        if (pesanPasswordTutup != null)
            pesanPasswordTutup.text = "";

        Debug.Log("[AdminReset] Panel password tutup dibuka.");
    }

    void TutupPanelPasswordTutup()
    {
        if (panelPasswordTutup != null)
            panelPasswordTutup.SetActive(false);

        if (inputPasswordTutup != null)
            inputPasswordTutup.text = "";

        if (pesanPasswordTutup != null)
            pesanPasswordTutup.text = "";
    }

    void OnKonfirmasiTutup()
    {
        string input = inputPasswordTutup != null
            ? inputPasswordTutup.text
            : "";

        if (input != passwordTutup)
        {
            if (pesanPasswordTutup != null)
                pesanPasswordTutup.text = "PIN salah!";

            if (inputPasswordTutup != null)
                inputPasswordTutup.text = "";

            Debug.LogWarning("[AdminReset] PIN tutup salah.");
            return;
        }

        Debug.Log("[AdminReset] PIN benar — menutup aplikasi.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}