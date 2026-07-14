using UnityEngine;


public class DragController : MonoBehaviour
{

    [Header("Batas Area Gerak (opsional)")]
    public bool    clampToArea = false;
    public Vector2 areaMin     = new Vector2(-8f, -4.5f);
    public Vector2 areaMax     = new Vector2( 8f,  4.5f);

    [Header("Smoothing")]
    [Tooltip("0 = langsung snap ke jari")]
    [Range(0f, 20f)]
    public float followSpeed = 0f;

    [Header("Visual saat di-drag")]
    public float dragScale = 1.1f;
    [Range(0f, 1f)]
    public float dragAlpha = 0.8f;


    [Header("Rotasi Kapal")]
    [Tooltip("Aktifkan rotasi sprite mengikuti arah pergerakan")]
    public bool  enableRotation   = true;

    [Tooltip("Kecepatan rotasi sprite mengikuti arah (0 = langsung)")]
    [Range(0f, 20f)]
    public float rotationSpeed    = 10f;

    [Tooltip("Jarak minimum pergerakan sebelum rotasi diupdate (mencegah jitter)")]
    [Min(0.01f)]
    public float rotationThreshold = 0.05f;

    [Tooltip("Offset sudut jika sprite kapal tidak menghadap ke atas secara default. " +
             "0 = sprite menghadap atas, 90 = sprite menghadap kanan, -90 = kiri")]
    public float spriteAngleOffset = 0f;

    private bool           isDragging     = false;
    private Vector3        offset         = Vector3.zero;
    private Camera         mainCam;
    private Rigidbody2D    rb;
    private SpriteRenderer sr;
    private Collider2D     col;
    private Vector3        originalScale;
    private Color          originalColor;
    private int            activeTouchId  = -1;

    private Vector3        lastPosition;
    private float          targetAngle    = 0f;

    public bool IsDragging => isDragging;

    void Awake()
    {
        mainCam       = Camera.main;
        rb            = GetComponent<Rigidbody2D>();
        sr            = GetComponent<SpriteRenderer>();
        col           = GetComponent<Collider2D>();
        originalScale = transform.localScale;
        lastPosition  = transform.position;

        if (sr != null)
            originalColor = sr.color;

        if (rb != null)
        {
            rb.gravityScale   = 0f;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        if (Input.touchCount > 0)
            HandleTouch();
        else
            HandleMouse();

        UpdateRotation();
    }

    void HandleTouch()
    {
        foreach (Touch touch in Input.touches)
        {
            Vector3 worldPos = ScreenToWorld(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (activeTouchId == -1 && IsTouchingPlayer(worldPos))
                    {
                        activeTouchId = touch.fingerId;
                        StartDrag(worldPos);
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isDragging && touch.fingerId == activeTouchId)
                        DragTo(worldPos);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (touch.fingerId == activeTouchId)
                    {
                        EndDrag();
                        activeTouchId = -1;
                    }
                    break;
            }
        }
    }


    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = ScreenToWorld(Input.mousePosition);
            if (IsTouchingPlayer(worldPos))
                StartDrag(worldPos);
        }

        if (isDragging && Input.GetMouseButton(0))
            DragTo(ScreenToWorld(Input.mousePosition));

        if (Input.GetMouseButtonUp(0))
            EndDrag();
    }


    void StartDrag(Vector3 worldPos)
    {
        isDragging   = true;
        offset       = transform.position - worldPos;
        lastPosition = transform.position;

        transform.localScale = originalScale * dragScale;

        if (sr != null)
        {
            Color c = originalColor;
            c.a      = dragAlpha;
            sr.color = c;
        }

        if (rb != null) rb.velocity = Vector2.zero;
    }

    void DragTo(Vector3 worldPos)
    {
        Vector3 targetPos = worldPos + offset;
        targetPos.z = 0f;

        if (clampToArea)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, areaMin.x, areaMax.x);
            targetPos.y = Mathf.Clamp(targetPos.y, areaMin.y, areaMax.y);
        }

        if (followSpeed <= 0f)
            transform.position = targetPos;
        else
            transform.position = Vector3.Lerp(
                transform.position, targetPos, followSpeed * Time.deltaTime);

        UpdateTargetAngle();
    }

    void EndDrag()
    {
        isDragging = false;

        transform.localScale = originalScale;

        if (sr != null) sr.color = originalColor;
        if (rb != null) rb.velocity = Vector2.zero;
    }


    void UpdateTargetAngle()
    {
        if (!enableRotation) return;

        Vector3 delta = transform.position - lastPosition;

        if (delta.magnitude >= rotationThreshold)
        {
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            targetAngle = angle - 90f + spriteAngleOffset;
        }

        lastPosition = transform.position;
    }

    void UpdateRotation()
    {
        if (!enableRotation) return;

        if (rotationSpeed <= 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, targetAngle),
                rotationSpeed * Time.deltaTime);
        }
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 pos = mainCam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y,
                Mathf.Abs(mainCam.transform.position.z)));
        pos.z = 0f;
        return pos;
    }

    bool IsTouchingPlayer(Vector3 worldPos)
    {
        if (col == null) return false;
        return col.OverlapPoint(worldPos);
    }

    public void ForceDrop()
    {
        EndDrag();
        activeTouchId = -1;
    }

    void OnDrawGizmos()
    {
        if (!clampToArea) return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Vector3 center = new Vector3(
            (areaMin.x + areaMax.x) / 2f,
            (areaMin.y + areaMax.y) / 2f, 0f);
        Vector3 size = new Vector3(
            areaMax.x - areaMin.x,
            areaMax.y - areaMin.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}