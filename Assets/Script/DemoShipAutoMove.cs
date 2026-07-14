using UnityEngine;


public class DemoShipAutoMove : MonoBehaviour
{
    [Header("Path Gerak")]
    [Tooltip("Titik kiri dan kanan pergerakan demo (local position relatif terhadap parent)")]
    public Vector2 titikKiri  = new Vector2(-2.5f, 0f);
    public Vector2 titikKanan = new Vector2( 2.5f, 0f);

    [Tooltip("Kecepatan gerak bolak-balik")]
    [Range(0.2f, 3f)] public float speed = 0.8f;

    [Header("Rotasi")]
    [Tooltip("Aktifkan agar sprite kapal sedikit miring sesuai arah gerak, " +
             "meniru rotasi kapal asli di PlayerMovement")]
    public bool enableTilt = true;
    [Range(0f, 30f)] public float tiltAngle = 12f;

    private Vector3 startLocalPos;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f);
        Vector2 target = Vector2.Lerp(titikKiri, titikKanan, t);

        Vector3 posSebelum = transform.localPosition;
        transform.localPosition = new Vector3(target.x, target.y, transform.localPosition.z);

        if (enableTilt)
        {
            float arah = transform.localPosition.x - posSebelum.x;
            float targetTilt = arah > 0 ? -tiltAngle : (arah < 0 ? tiltAngle : 0f);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, targetTilt),
                5f * Time.deltaTime);
        }
    }
}