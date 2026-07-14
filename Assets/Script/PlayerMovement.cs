using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Kecepatan")]
    [Range(1f, 20f)]
    [Tooltip("Kecepatan gerak maksimum kapal")]
    public float moveSpeed = 5f;

    [Header("Input")]
    [Tooltip("Aktifkan input WASD / Arrow Keys — untuk testing di laptop. " +
             "Joystick tetap berfungsi meski ini aktif; salah satu yang " +
             "memberi input non-zero akan dipakai.")]
    public bool enableKeyboardInput = true;

    [Header("Rotasi Kapal")]
    [Tooltip("Aktifkan rotasi sprite mengikuti arah pergerakan")]
    public bool enableRotation = true;

    [Tooltip("Kecepatan rotasi sprite mengikuti arah (0 = langsung snap)")]
    [Range(0f, 20f)]
    public float rotationSpeed = 10f;

    [Tooltip("Offset sudut jika sprite kapal tidak menghadap ke atas secara default. " +
             "0 = sprite menghadap atas, 90 = kanan, -90 = kiri")]
    public float spriteAngleOffset = 0f;

    [Header("Batas Area Gerak (opsional)")]
    public bool    clampToArea = false;
    public Vector2 areaMin     = new Vector2(-8f, -4.5f);
    public Vector2 areaMax     = new Vector2( 8f,  4.5f);

    private Rigidbody2D rb;
    private Vector2     moveInput   = Vector2.zero;
    private float       targetAngle = 0f;

    public bool IsMoving => moveInput.magnitude > 0.01f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        moveInput = ReadInput();

        if (enableRotation)
            UpdateTargetAngle();

        UpdateRotation();
    }

    void FixedUpdate()
    {
        Vector2 velocity = moveInput * moveSpeed;

        if (clampToArea)
        {
            Vector2 pos = rb.position;

            if (pos.x <= areaMin.x && velocity.x < 0f) velocity.x = 0f;
            if (pos.x >= areaMax.x && velocity.x > 0f) velocity.x = 0f;
            if (pos.y <= areaMin.y && velocity.y < 0f) velocity.y = 0f;
            if (pos.y >= areaMax.y && velocity.y > 0f) velocity.y = 0f;
        }

        rb.velocity = velocity;
    }

    Vector2 ReadInput()
    {
        if (VirtualJoystick.Instance != null && VirtualJoystick.Instance.IsActive)
            return VirtualJoystick.Instance.Direction * VirtualJoystick.Instance.Magnitude;

        if (enableKeyboardInput)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 kb = new Vector2(h, v);
            return kb.magnitude > 1f ? kb.normalized : kb;
        }

        return Vector2.zero;
    }

    void UpdateTargetAngle()
    {
        if (moveInput.magnitude < 0.01f) return;

        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
        targetAngle = angle - 90f + spriteAngleOffset;
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

    public void StopMovement()
    {
        moveInput   = Vector2.zero;
        rb.velocity = Vector2.zero;
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