using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PedagangBeliPanel : MonoBehaviour
{
    public static PedagangBeliPanel Instance { get; private set; }


    [Header("Header")]
    public TMP_Text namaPedagangText;
    public TMP_Text dialogPedagangText;
    public Image    portraitImage;

    [Header("Avatar Arung")]
    [Tooltip("Image untuk menampilkan portrait Arung dengan ekspresi sesuai konteks panel ini")]
    public Image    portraitArungImage;

    [Header("Slot Komoditas")]
    public SlotKomoditas slotBeras;
    public SlotKomoditas slotKain;
    public SlotKomoditas slotKeramik;

    [Header("Footer")]
    public TMP_Text totalBeliText;
    public TMP_Text uangSaatIniText;
    public Button   tombolKonfirmasi;
    public Button   tombolLewati;
    public TMP_Text pesanMinimalText;

    [Header("Harga Beli")]
    public int hargaBeras   = 20;
    public int hargaKain    = 50;
    public int hargaKeramik = 80;


    [Header("Pedagang — Bahasa Indonesia")]
    public string namaPedagang_ID    = "Pedagang Lokal Malaka";
    [TextArea(2, 3)]
    public string dialogPedagang_ID  = "Aku punya beras, kain patola, dan keramik tiongkok. Pilih yang kau mau!";

    [Header("Pedagang — English")]
    public string namaPedagang_EN    = "Malacca Local Trader";
    [TextArea(2, 3)]
    public string dialogPedagang_EN  = "I have rice, patola cloth, and Chinese porcelain. Pick what you want!";

    public Sprite portraitSprite;

    [Header("Dialog Kakek — Bahasa Indonesia")]
    [TextArea(2, 3)] public string dialogKakekAwal_ID   = "Lihat baik-baik apa yang mereka tawarkan. Rempah yang murah di sini bisa bernilai berlipat di tempat lain.";
    [TextArea(2, 3)] public string dialogKakekSelesai_ID = "Pilihan yang bijak. Sekarang mari kita cari pembeli yang tepat di pelabuhan.";

    [Header("Dialog Kakek — English")]
    [TextArea(2, 3)] public string dialogKakekAwal_EN   = "Look carefully at what they're offering. Goods that are cheap here can be worth much more elsewhere.";
    [TextArea(2, 3)] public string dialogKakekSelesai_EN = "A wise choice. Now let's find the right buyers at the harbor.";

    string NamaPedagang        => LocalizationManager.Pick(namaPedagang_ID, namaPedagang_EN);
    string DialogPedagang      => LocalizationManager.Pick(dialogPedagang_ID, dialogPedagang_EN);
    string DialogArungAwal     => LocalizationManager.Pick(dialogKakekAwal_ID, dialogKakekAwal_EN);
    string DialogArungSelesai  => LocalizationManager.Pick(dialogKakekSelesai_ID, dialogKakekSelesai_EN);

    [Header("Ekspresi Arung")]
    [Tooltip("Ekspresi portrait Arung saat dialog awal (panel terbuka) ditampilkan")]
    public EkspresiArung ekspresiArungAwal = EkspresiArung.Curious;

    [Tooltip("Ekspresi portrait Arung saat dialog selesai (setelah konfirmasi beli) ditampilkan")]
    public EkspresiArung ekspresiArungSelesai = EkspresiArung.Flat;

    private bool pilihBeras   = false;
    private bool pilihKain    = false;
    private bool pilihKeramik = false;

  
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        slotBeras?.toggleButton?.onClick.AddListener(()   => ToggleKomoditas("beras"));
        slotKain?.toggleButton?.onClick.AddListener(()    => ToggleKomoditas("kain"));
        slotKeramik?.toggleButton?.onClick.AddListener(() => ToggleKomoditas("keramik"));

        tombolKonfirmasi?.onClick.AddListener(OnKonfirmasi);
        tombolLewati?.onClick.AddListener(OnLewati);

        gameObject.SetActive(false);
    }


    public void Tampilkan()
    {
        pilihBeras = pilihKain = pilihKeramik = false;

        if (namaPedagangText   != null) namaPedagangText.text   = NamaPedagang;
        if (dialogPedagangText != null) dialogPedagangText.text = DialogPedagang;
        if (portraitImage != null && portraitSprite != null)
            portraitImage.sprite = portraitSprite;

       
        if (portraitArungImage != null)
        {
            Sprite ekspresiSprite = DialogArung.Instance?.GetSprite(ekspresiArungAwal);
            if (ekspresiSprite != null)
                portraitArungImage.sprite = ekspresiSprite;
        }

        bool english = LocalizationManager.Instance?.IsEnglish ?? false;
        SetupSlot(slotBeras,   english ? "Rice"            : "Beras",           hargaBeras,   "beras");
        SetupSlot(slotKain,    english ? "Patola Cloth"    : "Kain Patola",      hargaKain,    "kain");
        SetupSlot(slotKeramik, english ? "Chinese Porcelain" : "Keramik Tiongkok", hargaKeramik, "keramik");

        if (pesanMinimalText != null) pesanMinimalText.gameObject.SetActive(false);
        UpdateUangSaatIni();
        UpdateTotalBeli();
        UpdateKonfirmasiButton();

        gameObject.SetActive(true);
        Time.timeScale = 0f;
        IdleTimeoutManager.Instance?.PauseTimeout();

        AudioManager.Instance?.PlayDialogMuncul();

        if (!string.IsNullOrEmpty(DialogArungAwal))
            DialogArung.Instance?.TampilkanDialog(DialogArungAwal, ekspresiArungAwal);

        Debug.Log("[PedagangBeliPanel] Panel terbuka.");
    }

    void ToggleKomoditas(string nama)
    {
        switch (nama)
        {
            case "beras":
                pilihBeras = !pilihBeras;
                UpdateSlotVisual(slotBeras, pilihBeras);
                break;
            case "kain":
                pilihKain = !pilihKain;
                UpdateSlotVisual(slotKain, pilihKain);
                break;
            case "keramik":
                pilihKeramik = !pilihKeramik;
                UpdateSlotVisual(slotKeramik, pilihKeramik);
                break;
        }

        UpdateTotalBeli();
        UpdateKonfirmasiButton();

        if (pesanMinimalText != null)
            pesanMinimalText.gameObject.SetActive(false);
    }

    void OnKonfirmasi()
    {
        if (!pilihBeras && !pilihKain && !pilihKeramik)
        {
            if (pesanMinimalText != null)
            {
                bool english = LocalizationManager.Instance?.IsEnglish ?? false;
                pesanMinimalText.text = english
                    ? "Select at least 1 item!"
                    : "Pilih minimal 1 komoditas!";
                pesanMinimalText.gameObject.SetActive(true);
            }
            return;
        }

        int totalBeli = 0;

        if (pilihBeras)
        {
            GuldenManager.Instance?.AddGuldenCurrentLevel(-hargaBeras);
            RempahState.Instance?.BeliRempah("beras");
            totalBeli += hargaBeras;
        }
        if (pilihKain)
        {
            GuldenManager.Instance?.AddGuldenCurrentLevel(-hargaKain);
            RempahState.Instance?.BeliRempah("kain");
            totalBeli += hargaKain;
        }
        if (pilihKeramik)
        {
            GuldenManager.Instance?.AddGuldenCurrentLevel(-hargaKeramik);
            RempahState.Instance?.BeliRempah("keramik");
            totalBeli += hargaKeramik;
        }

        AudioManager.Instance?.PlayBeliKomoditas();
        Debug.Log($"[PedagangBeliPanel] Beli: Beras={pilihBeras} Kain={pilihKain} " +
                  $"Keramik={pilihKeramik} | Total: -{totalBeli}");

        Tutup();

        ObjectivePointer.Instance?.SetTargetSelesai();

        if (!string.IsNullOrEmpty(DialogArungSelesai))
            DialogArung.Instance?.TampilkanDialog(DialogArungSelesai, ekspresiArungSelesai);
    }

    void OnLewati()
    {
        Debug.Log("[PedagangBeliPanel] Player melewati pedagang.");
        Tutup();
    }

    void Tutup()
    {
        Time.timeScale = 1f;
        IdleTimeoutManager.Instance?.ResumeTimeout();
        gameObject.SetActive(false);
    }


    void SetupSlot(SlotKomoditas slot, string nama, int harga, string key)
    {
        if (slot == null) return;
        if (slot.namaText  != null) slot.namaText.text  = nama;
        if (slot.hargaText != null) slot.hargaText.text = $"-{harga}";

        UpdateSlotVisual(slot, false);
    }

    void UpdateSlotVisual(SlotKomoditas slot, bool dipilih)
    {
        if (slot == null) return;

        if (slot.backgroundImage != null)
            slot.backgroundImage.color = dipilih
                ? new Color(0.2f, 0.8f, 0.4f, 0.3f)
                : new Color(1f, 1f, 1f, 0.1f);

        if (slot.checkText != null)
            slot.checkText.text = dipilih ? "✓" : "";

        if (slot.toggleButton != null)
        {
            TMP_Text btnLabel = slot.toggleButton.GetComponentInChildren<TMP_Text>();
            if (btnLabel != null)
            {
                bool english = LocalizationManager.Instance?.IsEnglish ?? false;
                btnLabel.text = dipilih
                    ? (english ? "Cancel" : "Batal")
                    : (english ? "Select" : "Pilih");
            }
        }
    }

    void UpdateTotalBeli()
    {
        int total = 0;
        if (pilihBeras)   total += hargaBeras;
        if (pilihKain)    total += hargaKain;
        if (pilihKeramik) total += hargaKeramik;

        if (totalBeliText != null)
            totalBeliText.text = $"-{total}";
    }

    void UpdateUangSaatIni()
    {
        if (uangSaatIniText == null) return;
        int gulden           = GuldenManager.Instance?.TotalGuldenDagang ?? 0;
        uangSaatIniText.text = $"{gulden}";
    }

    void UpdateKonfirmasiButton()
    {
        bool adaPilihan = pilihBeras || pilihKain || pilihKeramik;
        if (tombolKonfirmasi != null)
            tombolKonfirmasi.interactable = adaPilihan;

        CanvasGroup cg = tombolKonfirmasi?.GetComponent<CanvasGroup>();
        if (cg == null && tombolKonfirmasi != null)
            cg = tombolKonfirmasi.gameObject.AddComponent<CanvasGroup>();
        if (cg != null) cg.alpha = adaPilihan ? 1f : 0.4f;
    }
}


[System.Serializable]
public class SlotKomoditas
{
    public TMP_Text namaText;
    public TMP_Text hargaText;
    public TMP_Text checkText;
    public Image    backgroundImage;
    public Button   toggleButton;
}