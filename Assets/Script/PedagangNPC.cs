using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider2D))]
public class PedagangNPC : MonoBehaviour
{

    [Header("Identitas")]
    public string namaPedagang    = "Pedagang";
    public Sprite portraitSprite;

    [Tooltip("Kalimat pertanyaan/tawaran dari pedagang — Bahasa Indonesia")]
    [TextArea(2, 4)]
    public string dialogPedagang_ID  = "";

    [Tooltip("Kalimat pertanyaan/tawaran dari pedagang — English")]
    [TextArea(2, 4)]
    public string dialogPedagang_EN  = "";

    public string DialogPedagang => LocalizationManager.Pick(dialogPedagang_ID, dialogPedagang_EN);

    [Tooltip("Ekspresi portrait Arung yang ditampilkan saat panel pedagang ini dibuka")]
    public EkspresiArung ekspresiArungPanel = EkspresiArung.Curious;

    [Header("Tag")]
    public string playerTag = "Player";

    [Header("Mode NPC Pembeli Rempah (khusus Level 3)")]
    [Tooltip("AKTIFKAN HANYA untuk NPC pembeli di Level 3 (Bangsawan, Garnisun VOC, " +
             "Pedagang Lokal) yang opsinya bergantung pada stok rempah pemain. " +
             "Saat aktif: sprite NPC dan InteractionIndicator otomatis disembunyikan " +
             "ketika rempah yang dibutuhkan NPC ini habis dari inventaris pemain. " +
             "JANGAN aktifkan untuk pedagang Level 1/2 — mereka harus selalu terlihat " +
             "dan indicator-nya berjalan sesuai pengaturan standar (tampilDiAwal), " +
             "karena pemain MENJUAL ke mereka, bukan membeli rempah spesifik DARI mereka.")]
    public bool modeNpcPembeliRempah = false;



    [Header("Opsi Dagang")]
    [Tooltip("Opsi yang ditampilkan ke player — maksimal 3")]
    public OpsiDagang opsiA;
    public OpsiDagang opsiB;

    [Tooltip("Opsi ketiga (opsional — kosongkan label jika tidak dipakai)")]
    public OpsiDagang opsiC;

    public bool PunyaOpsiC => !string.IsNullOrEmpty(opsiC.labelOpsi);

    [Header("Dialog Kakek — Saat Mendekati")]
    [Tooltip("Dialog kakek saat player pertama mendekati pedagang ini — Bahasa Indonesia")]
    [TextArea(2, 4)]
    public string dialogKakekAwal_ID = "";

    [Tooltip("Dialog kakek saat player pertama mendekati pedagang ini — English")]
    [TextArea(2, 4)]
    public string dialogKakekAwal_EN = "";

    public string DialogArungAwal => LocalizationManager.Pick(dialogKakekAwal_ID, dialogKakekAwal_EN);

    [Tooltip("Ekspresi portrait Arung saat dialog awal (mendekati pedagang) ditampilkan")]
    public EkspresiArung ekspresiArungAwal = EkspresiArung.Flat;

    [Header("Dialog Kakek — Setelah Pilih Opsi")]
    [Tooltip("Dialog setelah Opsi A — Bahasa Indonesia")]
    [TextArea(2, 4)]
    public string dialogKakekSetelahOpsiA_ID = "";

    [Tooltip("Dialog setelah Opsi A — English")]
    [TextArea(2, 4)]
    public string dialogKakekSetelahOpsiA_EN = "";

    [Tooltip("Dialog setelah Opsi B — Bahasa Indonesia")]
    [TextArea(2, 4)]
    public string dialogKakekSetelahOpsiB_ID = "";

    [Tooltip("Dialog setelah Opsi B — English")]
    [TextArea(2, 4)]
    public string dialogKakekSetelahOpsiB_EN = "";

    [Tooltip("Dialog setelah Opsi C (jika ada) — Bahasa Indonesia")]
    [TextArea(2, 4)]
    public string dialogKakekSetelahOpsiC_ID = "";

    [Tooltip("Dialog setelah Opsi C (jika ada) — English")]
    [TextArea(2, 4)]
    public string dialogKakekSetelahOpsiC_EN = "";

    string DialogArungSetelahOpsiA => LocalizationManager.Pick(dialogKakekSetelahOpsiA_ID, dialogKakekSetelahOpsiA_EN);
    string DialogArungSetelahOpsiB => LocalizationManager.Pick(dialogKakekSetelahOpsiB_ID, dialogKakekSetelahOpsiB_EN);
    string DialogArungSetelahOpsiC => LocalizationManager.Pick(dialogKakekSetelahOpsiC_ID, dialogKakekSetelahOpsiC_EN);

    [Header("Ekspresi Arung — Setelah Pilih Opsi")]
    [Tooltip("Ekspresi portrait Arung setelah memilih Opsi A")]
    public EkspresiArung ekspresiArungSetelahOpsiA = EkspresiArung.Flat;

    [Tooltip("Ekspresi portrait Arung setelah memilih Opsi B")]
    public EkspresiArung ekspresiArungSetelahOpsiB = EkspresiArung.Flat;

    [Tooltip("Ekspresi portrait Arung setelah memilih Opsi C (jika ada)")]
    public EkspresiArung ekspresiArungSetelahOpsiC = EkspresiArung.Flat;



    [Header("Interaction Indicator")]
    [Tooltip("Child GameObject dengan script InteractionIndicator")]
    public InteractionIndicator indicator;

    [Header("Kontrol")]
    [Tooltip("Apakah interaksi hanya bisa dilakukan satu kali?")]
    public bool sekaliInteraksi = true;

    [Tooltip("Nonaktifkan sprite pedagang setelah interaksi selesai")]
    public bool sembunyikanSetelahInteraksi = true;

    [Tooltip("Izinkan interaksi berulang — untuk pembeli Level 3 yang menerima banyak komoditas")]
    public bool izinkanInteraksiUlang = false;

    
    [Header("Events")]
    public UnityEvent OnInteraksiSelesai;

 
    private bool sudahInteraksi = false;


    public bool SudahSelesai => sudahInteraksi;
    private bool playerDiDekat  = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D colliderSolid; 



    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();

        
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            if (!col.isTrigger)
            {
                colliderSolid = col;
                break;
            }
        }

        if (spriteRenderer == null)
            Debug.LogWarning($"[PedagangNPC] {namaPedagang}: SpriteRenderer tidak ditemukan " +
                              $"di GameObject ini — sprite tidak akan otomatis disembunyikan " +
                              $"saat rempah habis.");

        // 
        if (string.IsNullOrEmpty(opsiA.labelOpsi))
            Debug.LogWarning($"[PedagangNPC] {namaPedagang}: opsiA belum diisi!");
        if (string.IsNullOrEmpty(opsiB.labelOpsi))
            Debug.LogWarning($"[PedagangNPC] {namaPedagang}: opsiB belum diisi!");
    }

    void Update()
    {

        if (modeNpcPembeliRempah && spriteRenderer != null && !sudahInteraksi)
        {
            bool tidakAdaOpsi = TidakAdaOpsiTersedia();
            spriteRenderer.enabled = !tidakAdaOpsi;

       
            if (colliderSolid != null)
                colliderSolid.enabled = !tidakAdaOpsi;
        }

        UpdateIndicator();
    }

    void UpdateIndicator()
    {

        if (!modeNpcPembeliRempah) return;

        if (indicator == null) return;
        if (sudahInteraksi) return;

        bool tidakAdaOpsi = TidakAdaOpsiTersedia();

        if (tidakAdaOpsi)
        {
            if (indicator.gameObject.activeInHierarchy)
                indicator.Sembunyikan();
        }
        else
        {

            if (!indicator.gameObject.activeInHierarchy)
                indicator.gameObject.SetActive(true);

            indicator.Tampilkan();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (sekaliInteraksi && sudahInteraksi) return;

   
        if (izinkanInteraksiUlang && TidakAdaOpsiTersedia())
        {
            Debug.Log($"[PedagangNPC] {namaPedagang}: semua rempah habis — panel tidak dibuka.");
            return;
        }

        playerDiDekat = true;

        if (!string.IsNullOrEmpty(DialogArungAwal))
            DialogArung.Instance?.TampilkanDialog(DialogArungAwal, ekspresiArungAwal);

        PedagangPanel.Instance?.Tampilkan(this);

        Debug.Log($"[PedagangNPC] Player mendekati {namaPedagang}");
    }


    bool TidakAdaOpsiTersedia()
    {
        bool aAda = !string.IsNullOrEmpty(opsiA.rempahDibutuhkan) && opsiA.BisaDipilih();
        bool bAda = !string.IsNullOrEmpty(opsiB.rempahDibutuhkan) && opsiB.BisaDipilih();
        bool cAda = PunyaOpsiC && !string.IsNullOrEmpty(opsiC.rempahDibutuhkan) && opsiC.BisaDipilih();
        return !aAda && !bAda && !cAda;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerDiDekat = false;
    }

    public void PilihOpsi(int pilihIndex)
    {
        OpsiDagang opsiDipilih = pilihIndex switch
        {
            0 => opsiA,
            1 => opsiB,
            2 => opsiC,
            _ => opsiA
        };

        if (GuldenManager.Instance != null)
        {
            GuldenManager.Instance.AddGuldenCurrentLevel(opsiDipilih.nilaiGulden);
            Debug.Log($"[PedagangNPC] {namaPedagang}: pilih '{opsiDipilih.labelOpsi}'" +
                      $" → {opsiDipilih.nilaiGulden:+#;-#;0} gulden");
        }

        if (!string.IsNullOrEmpty(opsiDipilih.rempahDibutuhkan) && opsiDipilih.nilaiGulden > 0)
        {
            RempahState.Instance?.JualRempah(opsiDipilih.rempahDibutuhkan);
            AudioManager.Instance?.PlayJualRempah();
        }

        string dialogSetelah = pilihIndex switch
        {
            0 => DialogArungSetelahOpsiA,
            1 => DialogArungSetelahOpsiB,
            2 => DialogArungSetelahOpsiC,
            _ => ""
        };

        EkspresiArung ekspresiSetelah = pilihIndex switch
        {
            0 => ekspresiArungSetelahOpsiA,
            1 => ekspresiArungSetelahOpsiB,
            2 => ekspresiArungSetelahOpsiC,
            _ => EkspresiArung.Flat
        };

        if (!string.IsNullOrEmpty(dialogSetelah))
            DialogArung.Instance?.TampilkanDialog(dialogSetelah, ekspresiSetelah);

        if (izinkanInteraksiUlang)
        {
            sudahInteraksi = false;

            if (TidakAdaOpsiTersedia())
            {
                indicator?.SembunyikanLangsung();

                ObjectivePointer.Instance?.SetTargetSelesai();
            }
        }
        else
        {
            sudahInteraksi = true;

            ObjectivePointer.Instance?.SetTargetSelesai();

            indicator?.SembunyikanLangsung();
        }


        OnInteraksiSelesai?.Invoke();
    }

    public void PilihOpsi(bool pilihA) => PilihOpsi(pilihA ? 0 : 1);


    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.2f);
        Bounds b = col.bounds;
        Gizmos.DrawCube(b.center, b.size);

        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(b.center, b.size);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            b.center + Vector3.up * (b.extents.y + 0.3f),
            $"{namaPedagang}\n" +
            $"A: {opsiA.labelOpsi} ({opsiA.nilaiGulden:+#;-#;0})\n" +
            $"B: {opsiB.labelOpsi} ({opsiB.nilaiGulden:+#;-#;0})");
#endif
    }
}


[System.Serializable]
public class OpsiDagang
{
    [Tooltip("Label tombol — Bahasa Indonesia, misal 'Jual Cengkeh'")]
    public string labelOpsi_ID = "";

    [Tooltip("Label tombol — English, misal 'Sell Cloves'")]
    public string labelOpsi_EN = "";

    public string labelOpsi => LocalizationManager.Pick(labelOpsi_ID, labelOpsi_EN);

    [Tooltip("Deskripsi singkat — Bahasa Indonesia")]
    [TextArea(1, 2)]
    public string deskripsi_ID = "";

    [Tooltip("Deskripsi singkat — English")]
    [TextArea(1, 2)]
    public string deskripsi_EN = "";

    public string deskripsi => LocalizationManager.Pick(deskripsi_ID, deskripsi_EN);

    [Tooltip("Nilai gulden: positif = dapat gulden, negatif = bayar gulden")]
    public int nilaiGulden = 0;

    [Tooltip("Teks hasil setelah memilih opsi ini, misal '+200 Gulden!'")]
    public string teksHasil = "";

    [Tooltip("Nama rempah yang HARUS DIMILIKI untuk memilih opsi ini. " +
             "Kosongkan jika tidak ada syarat. " +
             "Jika rempah sudah terjual, opsi ini akan di-disable.")]
    public string rempahDibutuhkan = "";

    [Tooltip("Nama rempah yang HARUS SUDAH TERJUAL untuk memilih opsi ini. " +
             "Contoh: opsi 'Tidak punya Pala' hanya bisa dipilih jika Pala sudah terjual. " +
             "Kosongkan jika tidak ada syarat.")]
    public string rempahHarusTerjual = "";

    [Tooltip("Reaksi pedagang setelah dipilih — Bahasa Indonesia")]
    [TextArea(2, 3)]
    public string reaksiPedagang_ID = "";

    [Tooltip("Reaksi pedagang setelah dipilih — English")]
    [TextArea(2, 3)]
    public string reaksiPedagang_EN = "";

    public string reaksiPedagang => LocalizationManager.Pick(reaksiPedagang_ID, reaksiPedagang_EN);

    public bool BisaDipilih()
    {
        if (RempahState.Instance == null) return true;

        if (!string.IsNullOrEmpty(rempahDibutuhkan) &&
            !RempahState.Instance.PunyaRempah(rempahDibutuhkan))
            return false;

        if (!string.IsNullOrEmpty(rempahHarusTerjual) &&
            RempahState.Instance.PunyaRempah(rempahHarusTerjual))
            return false;

        return true;
    }
}