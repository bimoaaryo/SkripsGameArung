using UnityEngine;
using System.Collections;


public class DialogAwalLevel : MonoBehaviour
{
    [Header("Dialog — Bahasa Indonesia")]
    [TextArea(2, 4)] public string dialogPertama_ID = "";
    [TextArea(2, 4)] public string dialogKedua_ID   = "";

    [Header("Dialog — English")]
    [TextArea(2, 4)] public string dialogPertama_EN = "";
    [TextArea(2, 4)] public string dialogKedua_EN   = "";

    string DialogPertama => LocalizationManager.Pick(dialogPertama_ID, dialogPertama_EN);
    string DialogKedua   => LocalizationManager.Pick(dialogKedua_ID,   dialogKedua_EN);

    [Header("Ekspresi Arung")]
    [Tooltip("Ekspresi portrait Arung saat dialog pertama ditampilkan")]
    public EkspresiArung ekspresiPertama = EkspresiArung.Flat;

    [Tooltip("Ekspresi portrait Arung saat dialog kedua ditampilkan")]
    public EkspresiArung ekspresiKedua = EkspresiArung.Flat;

    [Header("Pengaturan")]
    [Tooltip("Jeda sebelum dialog pertama muncul (detik, unscaled)")]
    [Min(0f)] public float delayMulai = 0.5f;


    void Start()
    {
        if (string.IsNullOrEmpty(dialogPertama_ID) && string.IsNullOrEmpty(dialogPertama_EN))
        {
            Debug.LogWarning("[DialogAwalLevel] Tidak ada dialog yang diisi.");
            return;
        }

        StartCoroutine(MulaiDialogRoutine());
    }


    IEnumerator MulaiDialogRoutine()
    {
        yield return new WaitForSecondsRealtime(delayMulai);

        yield return StartCoroutine(TampilkanDialog(DialogPertama, ekspresiPertama));

        if (!string.IsNullOrEmpty(DialogKedua))
            yield return StartCoroutine(TampilkanDialog(DialogKedua, ekspresiKedua));

        Debug.Log("[DialogAwalLevel] Semua dialog selesai — game sudah di-resume oleh DialogArung.");
    }

    IEnumerator TampilkanDialog(string teks, EkspresiArung ekspresi = EkspresiArung.Flat)
    {
        if (string.IsNullOrEmpty(teks)) yield break;

        DialogArung.InstanceAwal?.TampilkanDialogTungguSentuhan(teks, ekspresi);

        yield return new WaitForSecondsRealtime(0.5f);
        yield return new WaitUntil(() =>
            DialogArung.InstanceAwal == null || !DialogArung.InstanceAwal.IsShowing);
    }
}