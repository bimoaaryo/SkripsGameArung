using UnityEngine;

public class PlayerDamageVisual : MonoBehaviour
{
    public static PlayerDamageVisual Instance { get; private set; }

    [Header("Sprite")]
    [Tooltip("Sprite kapal dalam kondisi normal/utuh")]
    public Sprite spriteNormal;

    [Tooltip("Daftar sprite kerusakan secara berurutan dari ringan ke parah. " +
             "Contoh: [0] retak ringan, [1] retak sedang, [2] retak parah")]
    public Sprite[] daftarSpriteKerusakan;

    [Header("Pengaturan")]
    [Tooltip("Jumlah hit per tingkat kerusakan. " +
             "Misal 3 dengan 3 sprite: tingkat 1 di hit ke-3, tingkat 2 di hit ke-6, tingkat 3 di hit ke-9")]
    [Min(1)] public int thresholdPerTingkat = 3;

    [Header("Referensi")]
    public SpriteRenderer spriteRenderer;

    private int hitCount        = 0;
    private int indexKerusakan  = -1; 
    private Vector3 scaleAwal;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

      
        scaleAwal = transform.localScale;
    }


    public void TambahHit()
    {
        hitCount++;

        int indexBaru = (hitCount / thresholdPerTingkat) - 1;

        if (daftarSpriteKerusakan != null && daftarSpriteKerusakan.Length > 0)
            indexBaru = Mathf.Clamp(indexBaru, -1, daftarSpriteKerusakan.Length - 1);

        if (indexBaru != indexKerusakan)
        {
            indexKerusakan = indexBaru;
            TerapkanSpriteSesuaiTingkat();
            Debug.Log($"[PlayerDamageVisual] Hit ke-{hitCount} — " +
                      $"tingkat kerusakan: {indexKerusakan + 1}/{daftarSpriteKerusakan?.Length}");
        }
    }

    public void ResetKeAwal()
    {
        hitCount       = 0;
        indexKerusakan = -1;
        TerapkanSprite(spriteNormal);
        Debug.Log("[PlayerDamageVisual] Sprite kapal direset ke kondisi normal.");
    }

    void TerapkanSpriteSesuaiTingkat()
    {
        if (indexKerusakan < 0)
        {
            TerapkanSprite(spriteNormal);
            return;
        }

        if (daftarSpriteKerusakan == null || daftarSpriteKerusakan.Length == 0)
        {
            Debug.LogWarning("[PlayerDamageVisual] daftarSpriteKerusakan kosong!");
            return;
        }

        TerapkanSprite(daftarSpriteKerusakan[indexKerusakan]);
    }

    void TerapkanSprite(Sprite sprite)
    {
        if (spriteRenderer == null || sprite == null) return;

        Debug.Log($"[PlayerDamageVisual] Sebelum ganti sprite — " +
                  $"localScale={transform.localScale}, " +
                  $"sprite lama={spriteRenderer.sprite?.name}, " +
                  $"sprite baru={sprite.name}, " +
                  $"bounds lama={spriteRenderer.bounds.size}");

        spriteRenderer.sprite = sprite;

        Debug.Log($"[PlayerDamageVisual] Setelah ganti sprite — " +
                  $"localScale={transform.localScale}, " +
                  $"bounds baru={spriteRenderer.bounds.size}");
    }
}