using UnityEngine;

public class ObstacleSightZone : MonoBehaviour
{
    [Header("Tag")]
    public string playerTag = "Player";

    [Header("Dialog Arung — Melihat Obstacle")]
    [Tooltip("Tampil sekali per level saat player pertama masuk area ini. " +
             "Jika dialog Arung lain sedang tampil, sistem menunggu sampai selesai.")]
    [TextArea(2, 4)] public string dialogLihatObstacle_ID = "";
    [TextArea(2, 4)] public string dialogLihatObstacle_EN = "";
    public EkspresiArung ekspresiLihatObstacle = EkspresiArung.Startled;

    [Tooltip("Kunci unik untuk grup obstacle ini (misal 'VOC_L2', 'Bajak_L3', 'Karang_L1'). " +
             "Semua SightZone yang ingin dialognya tampil 1x per level pakai kunci yang sama.")]
             
    public string sightDialogKey = "default";

    string DialogLihatObstacle => LocalizationManager.Pick(dialogLihatObstacle_ID, dialogLihatObstacle_EN);

    private static System.Collections.Generic.HashSet<string> sudahTampilkanDialogLihat
        = new System.Collections.Generic.HashSet<string>();

    private bool sudahLihat = false;

    void Awake()
    {
        Collider2D[] cols = GetComponents<Collider2D>();
        if (cols.Length == 0)
        {
            Debug.LogWarning($"[ObstacleSightZone] {gameObject.name} tidak punya Collider2D!");
            return;
        }

        foreach (var col in cols)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (sudahLihat) return;
        if (sudahTampilkanDialogLihat.Contains(sightDialogKey)) return;
        if (string.IsNullOrEmpty(DialogLihatObstacle)) return;

        sudahLihat = true;
        sudahTampilkanDialogLihat.Add(sightDialogKey);

        StartCoroutine(TampilkanDialogTunggu(DialogLihatObstacle, ekspresiLihatObstacle));
    }

    System.Collections.IEnumerator TampilkanDialogTunggu(string teks, EkspresiArung ekspresi)
    {
        if (DialogArung.Instance == null) yield break;

        while (DialogArung.Instance.IsShowing)
            yield return null;

        DialogArung.Instance.TampilkanDialog(teks, ekspresi);
    }

    public static void ResetDialogFlag(string key) => sudahTampilkanDialogLihat.Remove(key);

    public static void ResetAllDialogFlags() => sudahTampilkanDialogLihat.Clear();
}