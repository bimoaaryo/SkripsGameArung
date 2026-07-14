using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class PedagangLokalNPC : MonoBehaviour
{
    [Header("Tag")]
    public string playerTag = "Player";

    [Header("Kontrol")]
    public bool sekaliInteraksi = true;

    [Header("Interaction Indicator")]
    [Tooltip("Child GameObject dengan script InteractionIndicator")]
    public InteractionIndicator indicator;

    private bool sudahInteraksi = false;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (sekaliInteraksi && sudahInteraksi) return;

        sudahInteraksi = true;

        indicator?.SembunyikanLangsung();

        if (PedagangBeliPanel.Instance != null)
            PedagangBeliPanel.Instance.Tampilkan();
        else
            Debug.LogWarning("[PedagangLokalNPC] PedagangBeliPanel.Instance tidak ditemukan!");
    }

    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.2f);
        Bounds b = col.bounds;
        Gizmos.DrawCube(b.center, b.size);
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(b.center, b.size);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            b.center + Vector3.up * (b.extents.y + 0.3f),
            "Pedagang Lokal Malaka");
#endif
    }
}