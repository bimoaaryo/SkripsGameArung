using UnityEngine;
using System.Collections;

/// <summary>
/// Attach ke GameObject Gate.
///
/// Gerbang tertutup secara default — Collider2D aktif, player tidak bisa lewat.
/// Saat OpenGate() dipanggil oleh GateButton:
/// - Animasi buka diputar
/// - Collider dinonaktifkan setelah animasi selesai
/// - Sprite berubah (opsional)
///
/// SETUP:
/// 1. Attach ke Gate GameObject.
/// 2. Collider2D → Is Trigger = OFF (solid blocker).
/// 3. Rigidbody2D → Body Type = Static.
/// 4. Animator dengan Trigger parameter "Open".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GateController : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector
    // -------------------------------------------------------

    [Header("Animasi")]
    [Tooltip("Animator pada Gate (isi otomatis)")]
    public Animator gateAnimator;

    [Tooltip("Nama Trigger di Animator saat gerbang terbuka")]
    public string openTrigger  = "Open";

    [Tooltip("Nama Trigger di Animator saat gerbang tertutup (opsional)")]
    public string closeTrigger = "Close";

    [Header("Timing")]
    [Tooltip("Delay sebelum collider dinonaktifkan (beri waktu animasi mulai)")]
    [Min(0f)] public float colliderDisableDelay = 0.3f;

    [Header("Audio (opsional)")]
    public AudioSource audioSource;
    public AudioClip   openSound;
    public AudioClip   closeSound;

    [Header("Visual (opsional)")]
    [Tooltip("Sprite saat gerbang terbuka. Kosongkan = tidak ganti sprite")]
    public Sprite openSprite;

    [Tooltip("Sprite saat gerbang tertutup")]
    public Sprite closedSprite;

    // -------------------------------------------------------
    // State
    // -------------------------------------------------------
    private Collider2D     col;
    private SpriteRenderer sr;
    private bool           isOpen = false;

    public bool IsOpen => isOpen;

    // -------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr  = GetComponent<SpriteRenderer>();

        if (gateAnimator == null)
            gateAnimator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Simpan sprite awal sebagai closedSprite
        if (closedSprite == null && sr != null)
            closedSprite = sr.sprite;
    }

    // -------------------------------------------------------
    // Public API — dipanggil oleh GateButton
    // -------------------------------------------------------

    /// <summary>
    /// Buka gerbang: mainkan animasi dan nonaktifkan collider.
    /// </summary>
    public void OpenGate()
    {
        if (isOpen) return;
        isOpen = true;

        Debug.Log($"[GateController] {gameObject.name} dibuka!");

        // Animasi
        if (gateAnimator != null && !string.IsNullOrEmpty(openTrigger))
            gateAnimator.SetTrigger(openTrigger);

        // Audio
        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

        // Nonaktifkan collider setelah delay
        StartCoroutine(DisableColliderAfterDelay(colliderDisableDelay));

        // Ganti sprite
        if (sr != null && openSprite != null)
            sr.sprite = openSprite;
    }

    /// <summary>
    /// Tutup gerbang kembali (opsional, bisa dipanggil dari script lain).
    /// </summary>
    public void CloseGate()
    {
        if (!isOpen) return;
        isOpen = false;

        Debug.Log($"[GateController] {gameObject.name} ditutup!");

        col.enabled = true;

        if (gateAnimator != null && !string.IsNullOrEmpty(closeTrigger))
            gateAnimator.SetTrigger(closeTrigger);

        if (audioSource != null && closeSound != null)
            audioSource.PlayOneShot(closeSound);

        if (sr != null && closedSprite != null)
            sr.sprite = closedSprite;
    }

    // -------------------------------------------------------
    // Coroutine
    // -------------------------------------------------------

    IEnumerator DisableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        col.enabled = false;
        Debug.Log($"[GateController] Collider {gameObject.name} dinonaktifkan.");
    }

    // -------------------------------------------------------
    // Gizmo
    // -------------------------------------------------------
    void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<Collider2D>();
        Bounds b = col != null
            ? col.bounds
            : new Bounds(transform.position, Vector3.one);

        Gizmos.color = isOpen
            ? new Color(0.2f, 0.9f, 0.3f, 0.2f)
            : new Color(0.9f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawCube(b.center, b.size);

        Gizmos.color = isOpen ? Color.green : Color.red;
        Gizmos.DrawWireCube(b.center, b.size);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            isOpen ? "GATE OPEN" : "GATE CLOSED");
#endif
    }
}