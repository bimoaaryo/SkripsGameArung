using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PedagangPanel : MonoBehaviour
{
    public static PedagangPanel Instance { get; private set; }

    [Header("Header")]
    public TMP_Text namaPedagangText;
    public TMP_Text dialogPedagangText;
    public Image    portraitImage;

    [Header("Opsi A")]
    public Button   tombolA;
    public TMP_Text labelA;
    public TMP_Text deskripsiA;
    public TMP_Text nilaiGuldenA;

    [Header("Opsi B")]
    public Button   tombolB;
    public TMP_Text labelB;
    public TMP_Text deskripsiB;
    public TMP_Text nilaiGuldenB;

    [Header("Opsi C (opsional)")]
    public GameObject containerC;
    public Button     tombolC;
    public TMP_Text   labelC;
    public TMP_Text   deskripsiC;
    public TMP_Text   nilaiGuldenC;

    [Header("Tombol Tutup")]
    [Tooltip("Tombol untuk menutup panel tanpa memilih — agar player bisa bandingkan harga")]
    public Button tombolTutup;

    [Header("Opsi Tidak Tersedia")]
    [Range(0.1f, 0.5f)] public float alphaDisable   = 0.35f;
    public string teksRempahHabis_ID = "Rempah sudah terjual";
    public string teksRempahHabis_EN = "Already sold";

    string TeksRempahHabis => LocalizationManager.Pick(teksRempahHabis_ID, teksRempahHabis_EN);

    private PedagangNPC pedagangAktif;
    private bool        sudahMemilih = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        tombolA?.onClick.AddListener(() => PilihOpsi(0));
        tombolB?.onClick.AddListener(() => PilihOpsi(1));
        tombolC?.onClick.AddListener(() => PilihOpsi(2));

        // Tombol tutup — tutup panel tanpa aksi
        tombolTutup?.onClick.AddListener(TutupTanpaMemilih);

        gameObject.SetActive(false);
    }


    public void Tampilkan(PedagangNPC pedagang)
    {
        if (pedagang == null) return;

        pedagangAktif = pedagang;
        sudahMemilih  = false;

        if (namaPedagangText   != null) namaPedagangText.text   = pedagang.namaPedagang;
        if (dialogPedagangText != null) dialogPedagangText.text = pedagang.DialogPedagang;
        if (portraitImage != null && pedagang.portraitSprite != null)
            portraitImage.sprite = pedagang.portraitSprite;

        SetSemuaTombolInteractable(true);
        ResetSemuaAlpha();

        SetOpsiUI(labelA, deskripsiA, nilaiGuldenA, pedagang.opsiA, tombolA);
        SetOpsiUI(labelB, deskripsiB, nilaiGuldenB, pedagang.opsiB, tombolB);

        bool punyaC = pedagang.PunyaOpsiC;
        if (containerC != null) containerC.SetActive(punyaC);
        if (punyaC)
            SetOpsiUI(labelC, deskripsiC, nilaiGuldenC, pedagang.opsiC, tombolC);

        gameObject.SetActive(true);
        Time.timeScale = 0f;

        AudioManager.Instance?.PlayDialogMuncul();

        Debug.Log($"[PedagangPanel] Tampil: {pedagang.namaPedagang} | OpsiC: {punyaC}");
    }

    void PilihOpsi(int index)
    {
        if (sudahMemilih || pedagangAktif == null) return;
        sudahMemilih = true;
        pedagangAktif.PilihOpsi(index);
        RefreshOpsi();
        sudahMemilih = false;
        Tutup();
    }


    void TutupTanpaMemilih()
    {
        Debug.Log("[PedagangPanel] Panel ditutup tanpa memilih.");
        Tutup();
    }

    void Tutup()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        pedagangAktif = null;
    }


    void RefreshOpsi()
    {
        if (pedagangAktif == null) return;

        SetOpsiUI(labelA, deskripsiA, nilaiGuldenA, pedagangAktif.opsiA, tombolA);
        SetOpsiUI(labelB, deskripsiB, nilaiGuldenB, pedagangAktif.opsiB, tombolB);

        if (pedagangAktif.PunyaOpsiC)
            SetOpsiUI(labelC, deskripsiC, nilaiGuldenC, pedagangAktif.opsiC, tombolC);
    }


    void SetOpsiUI(TMP_Text label, TMP_Text deskripsi, TMP_Text nilaiGulden,
                   OpsiDagang opsi, Button tombol)
    {
        if (label != null) label.text = opsi.labelOpsi;

        bool bisaDipilih = opsi.BisaDipilih();

        if (deskripsi != null)
            deskripsi.text = bisaDipilih ? opsi.deskripsi : TeksRempahHabis;

        if (nilaiGulden != null)
            nilaiGulden.text = bisaDipilih ? FormatNilaiGulden(opsi.nilaiGulden) : "-";

        if (tombol != null)
        {
            tombol.interactable = bisaDipilih;
            CanvasGroup cg = tombol.GetComponent<CanvasGroup>();
            if (cg == null) cg = tombol.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = bisaDipilih ? 1f : alphaDisable;
        }
    }

    string FormatNilaiGulden(int nilai)
    {
        if (nilai > 0) return $"+{nilai} Gulden";
        if (nilai < 0) return $"{nilai} Gulden";
        return "0 Gulden";
    }

    void SetSemuaTombolInteractable(bool state)
    {
        if (tombolA != null) tombolA.interactable = state;
        if (tombolB != null) tombolB.interactable = state;
        if (tombolC != null) tombolC.interactable = state;
    }

    void ResetSemuaAlpha()
    {
        SetAlphaTombol(tombolA, 1f);
        SetAlphaTombol(tombolB, 1f);
        SetAlphaTombol(tombolC, 1f);
    }

    void SetAlphaTombol(Button btn, float alpha)
    {
        if (btn == null) return;
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
    }
}