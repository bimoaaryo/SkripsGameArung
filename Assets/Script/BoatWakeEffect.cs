using UnityEngine;


[RequireComponent(typeof(ParticleSystem))]
public class BoatWakeEffect : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Rigidbody2D kapal/player. Kosongkan jika kapal TIDAK menggunakan Rigidbody2D " +
             "(misal PatrolMover yang gerak via transform.position langsung) — kecepatan " +
             "akan dihitung otomatis dari perubahan posisi tiap frame sebagai fallback.")]
    public Rigidbody2D playerRigidbody;

    [Header("Pengaturan Deteksi Gerak")]
    [Tooltip("Kecepatan minimum agar wake effect aktif")]
    [Min(0.01f)] public float minSpeedToEmit = 0.1f;

    [Tooltip("Emission rate saat kapal bergerak")]
    [Min(0f)] public float emissionRateMoving = 20f;

    [Header("Pengaturan V — Opsional")]
    [Tooltip("Sesuaikan sudut V berdasarkan kecepatan kapal. " +
             "Semakin cepat, V semakin sempit/memanjang.")]
    public bool scaleConeAngleWithSpeed = false;

    [Tooltip("Sudut cone minimum (kapal cepat) dan maksimum (kapal lambat)")]
    public float coneAngleMin = 15f;
    public float coneAngleMax = 30f;

    [Tooltip("Kecepatan referensi untuk normalisasi (sesuaikan dengan speed kapal)")]
    public float referenceMaxSpeed = 5f;

    private ParticleSystem            ps;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.ShapeModule   shape;
    private Vector3 posSebelumnya;
    private Transform kapalTransform;

    void Awake()
    {
        ps       = GetComponent<ParticleSystem>();
        emission = ps.emission;
        shape    = ps.shape;

        if (playerRigidbody == null)
            playerRigidbody = GetComponentInParent<Rigidbody2D>();

        kapalTransform = playerRigidbody != null
            ? playerRigidbody.transform
            : transform.parent;

        if (kapalTransform != null)
            posSebelumnya = kapalTransform.position;

        if (playerRigidbody == null && kapalTransform == null)
            Debug.LogWarning("[BoatWakeEffect] Tidak ada Rigidbody2D maupun parent Transform! " +
                              "Wake effect tidak akan aktif.");
    }

    void Update()
    {
        float speed = HitungKecepatan();

        if (speed > minSpeedToEmit)
        {
            var rate = emission.rateOverTime;
            if (rate.constant != emissionRateMoving)
            {
                rate.constant = emissionRateMoving;
                emission.rateOverTime = rate;
            }

            if (!ps.isEmitting) ps.Play();

            if (scaleConeAngleWithSpeed)
            {
                float t      = Mathf.Clamp01(speed / referenceMaxSpeed);
                float angle  = Mathf.Lerp(coneAngleMax, coneAngleMin, t);
                shape.angle  = angle;
            }
        }
        else
        {
            if (ps.isEmitting) ps.Stop();
        }
    }


    float HitungKecepatan()
    {
        if (playerRigidbody != null)
            return playerRigidbody.velocity.magnitude;

        if (kapalTransform == null) return 0f;

        Vector3 posSekarang = kapalTransform.position;
        float   jarak       = Vector3.Distance(posSekarang, posSebelumnya);
        float   speedFallback = Time.deltaTime > 0f ? jarak / Time.deltaTime : 0f;

        posSebelumnya = posSekarang;
        return speedFallback;
    }
}