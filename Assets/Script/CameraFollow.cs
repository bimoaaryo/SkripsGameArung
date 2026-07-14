using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [Header("Target")]
    [Tooltip("GameObject yang diikuti kamera (Player)")]
    public Transform target;

    [Header("Smooth")]
    [Tooltip("Kecepatan kamera mengikuti player. Makin besar makin cepat.")]
    [Range(1f, 20f)]
    public float smoothSpeed = 8f;

    [Header("Offset")]
    [Tooltip("Geser posisi kamera relatif terhadap player (biasanya biarkan 0,0)")]
    public Vector2 offset = Vector2.zero;

    [Header("Zoom")]
    [Tooltip("Ukuran orthographic kamera — atur sekali dari Inspector, tidak bisa diubah saat runtime")]
    [Range(1f, 20f)]
    public float cameraSize = 5f;

    [Header("Batas Area Kamera")]
    [Tooltip("Aktifkan agar kamera berhenti di tepi peta, tidak mengikuti player " +
             "sampai menampilkan area di luar peta")]
    public bool clampToBounds = false;

    [Tooltip("Batas posisi PUSAT kamera. Nilai ini harus sudah dikurangi setengah " +
             "lebar/tinggi kamera dari batas peta mentah — lihat tombol " +
             "'Hitung Half-Extent Kamera' di Inspector untuk bantuan kalkulasi.")]
    public Vector2 boundsMin = new Vector2(-8f, -4.5f);
    public Vector2 boundsMax = new Vector2( 8f,  4.5f);

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam != null)
        {
            cam.orthographic     = true;
            cam.orthographicSize = cameraSize;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                Debug.LogWarning("[CameraFollow] Target tidak ditemukan! Assign Player di Inspector.");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        FollowTarget();
    }


    void FollowTarget()
    {
        Vector3 desiredPos = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        if (clampToBounds)
        {
            desiredPos.x = Mathf.Clamp(desiredPos.x, boundsMin.x, boundsMax.x);
            desiredPos.y = Mathf.Clamp(desiredPos.y, boundsMin.y, boundsMax.y);
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );
    }


    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 pos = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        if (clampToBounds)
        {
            pos.x = Mathf.Clamp(pos.x, boundsMin.x, boundsMax.x);
            pos.y = Mathf.Clamp(pos.y, boundsMin.y, boundsMax.y);
        }

        transform.position = pos;
    }


    [ContextMenu("Hitung Half-Extent Kamera")]
    void HitungHalfExtent()
    {
        if (cam == null) cam = GetComponent<Camera>();

        float halfHeight = cam.orthographicSize;
        float halfWidth  = halfHeight * cam.aspect;

        Debug.Log($"[CameraFollow] Half Width: {halfWidth:F2} | Half Height: {halfHeight:F2}\n" +
                  $"boundsMin/boundsMax = batas tepi peta MENTAH dikurangi/ditambah nilai ini.");
    }


    void OnDrawGizmosSelected()
    {
        if (!clampToBounds) return;

        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            (boundsMin.x + boundsMax.x) / 2f,
            (boundsMin.y + boundsMax.y) / 2f, 0f);
        Vector3 size = new Vector3(
            boundsMax.x - boundsMin.x,
            boundsMax.y - boundsMin.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}