using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class RempahDetailPanel : MonoBehaviour
{
    public static RempahDetailPanel Instance { get; private set; }

    [Header("Tombol Tutup")]
    public Button tombolTutup;

    [Header("Slot Detail Rempah")]
    [Tooltip("Satu entry per jenis rempah — urutan bebas. " +
             "Slot otomatis tampil/sembunyi sesuai kepemilikan pemain.")]
    public SlotDetailRempah[] daftarSlot;

    private bool sedangTampil = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        gameObject.SetActive(false);
        tombolTutup?.onClick.AddListener(Sembunyikan);
    }


    public void TampilkanAtauSembunyikan()
    {
        if (sedangTampil) Sembunyikan();
        else Tampilkan();
    }

    public void Tampilkan()
    {
        RefreshSlot();
        gameObject.SetActive(true);
        sedangTampil = true;
        Time.timeScale = 0f;
        IdleTimeoutManager.Instance?.PauseTimeout();
    }

    public void Sembunyikan()
    {
        gameObject.SetActive(false);
        sedangTampil = false;
        Time.timeScale = 1f;
        IdleTimeoutManager.Instance?.ResumeTimeout();
    }


    void RefreshSlot()
    {
        if (daftarSlot == null) return;

        bool english = LocalizationManager.Instance?.IsEnglish ?? false;

        foreach (SlotDetailRempah slot in daftarSlot)
        {
            if (slot == null || slot.containerObject == null) continue;

            bool punya = RempahState.Instance != null &&
                         RempahState.Instance.PunyaRempah(slot.namaKunci);

            slot.containerObject.SetActive(punya);

            if (!punya) continue;

            if (slot.gambarRempah != null && slot.sprite != null)
                slot.gambarRempah.sprite = slot.sprite;

            if (slot.namaText != null)
                slot.namaText.text = english ? slot.nama_EN : slot.nama_ID;

            if (slot.deskripsiText != null)
                slot.deskripsiText.text = english ? slot.deskripsi_EN : slot.deskripsi_ID;
        }
    }
}

[System.Serializable]
public class SlotDetailRempah
{
    [Tooltip("Kunci pencarian di RempahState — harus sama persis: " +
             "'pala', 'cengkeh', 'beras', 'kain', 'keramik'")]
    public string namaKunci = "";

    [Tooltip("Container GameObject slot ini — diaktifkan/nonaktifkan " +
             "berdasarkan kepemilikan rempah pemain")]
    public GameObject containerObject;

    [Tooltip("Image untuk gambar rempah/komoditas")]
    public Image gambarRempah;

    [Tooltip("Sprite gambar rempah")]
    public Sprite sprite;

    [Tooltip("TMP_Text untuk nama rempah")]
    public TMP_Text namaText;

    [Tooltip("TMP_Text untuk deskripsi historis")]
    public TMP_Text deskripsiText;

    [Header("Teks")]
    public string nama_ID = "";
    public string nama_EN = "";

    [TextArea(3, 5)]
    public string deskripsi_ID = "";

    [TextArea(3, 5)]
    public string deskripsi_EN = "";
}