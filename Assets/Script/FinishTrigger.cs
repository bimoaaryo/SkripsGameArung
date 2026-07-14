using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class FinishTrigger : MonoBehaviour
{
    [Header("Tag")]
    public string playerTag = "Player";

    [Header("Gizmo")]
    public Color gizmoColor = new Color(0.2f, 0.9f, 0.4f, 0.25f);

    private bool sudahTrigger = false;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (sudahTrigger) return;
        if (!other.CompareTag(playerTag)) return;

        sudahTrigger = true;

        ObjectivePointer.Instance?.SetTargetSelesai();

        GetComponentInChildren<FinishZoneIndicator>()?.Sembunyikan();

        Debug.Log("[FinishTrigger] Player mencapai finish — tampilkan Trading Panel.");

        if (TradingResultPanel.Instance != null)
            TradingResultPanel.Instance.Tampilkan();
        else
            Debug.LogWarning("[FinishTrigger] TradingResultPanel.Instance tidak ditemukan!");
    }

    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = gizmoColor;
        Bounds b = col.bounds;
        Gizmos.DrawCube(b.center, b.size);
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.8f);
        Gizmos.DrawWireCube(b.center, b.size);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            b.center + Vector3.up * (b.extents.y + 0.3f), "Finish Zone");
#endif
    }
}