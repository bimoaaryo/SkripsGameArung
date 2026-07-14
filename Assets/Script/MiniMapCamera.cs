using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Kosongkan = auto-cari via tag Player")]
    public Transform target;

    [Header("Zoom")]
    [Tooltip("Makin besar = makin zoom out (area lebih luas terlihat)")]
    [Range(2f, 100f)] public float zoomSize = 8f;

    [Header("Clamp Batas Peta")]
    [Tooltip("Aktifkan agar minimap tidak keluar dari tepi peta")]
    public bool clampToBounds = false;

    [Tooltip("Batas kiri-bawah peta (world position). " +
             "Isi nilai yang sama dengan CameraFollow.boundsMin di scene yang sama.")]
    public Vector2 boundsMin = new Vector2(-8f, -4.5f);

    [Tooltip("Batas kanan-atas peta (world position). " +
             "Isi nilai yang sama dengan CameraFollow.boundsMax di scene yang sama.")]
    public Vector2 boundsMax = new Vector2( 8f,  4.5f);

    private Camera minimapCam;

    void Awake()
    {
        minimapCam = GetComponent<Camera>();

        if (minimapCam != null)
        {
            minimapCam.orthographic     = true;
            minimapCam.orthographicSize = zoomSize;
        }

        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float x = target.position.x;
        float y = target.position.y;


        if (clampToBounds)
        {
            x = Mathf.Clamp(x, boundsMin.x, boundsMax.x);
            y = Mathf.Clamp(y, boundsMin.y, boundsMax.y);
        }

        transform.position = new Vector3(x, y, -10f);
    }

    public void SetZoom(float size)
    {
        zoomSize = Mathf.Clamp(size, 2f, 100f);
        if (minimapCam != null)
            minimapCam.orthographicSize = zoomSize;
    }
}