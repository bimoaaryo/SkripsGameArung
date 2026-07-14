using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PatrolMover : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Kecepatan & Jarak Berhenti")]
    [Range(0.5f, 20f)]   public float speed            = 4f;
    [Range(0.02f, 0.5f)] public float stoppingDistance = 0.1f;

    [Header("Rotasi Sprite")]
    [Tooltip("Aktifkan agar sprite kapal berotasi mengikuti arah pergerakan")]
    public bool enableRotation = true;

    [Tooltip("Kecepatan rotasi mengikuti arah (0 = langsung snap)")]
    [Range(0f, 20f)] public float rotationSpeed = 10f;

    [Tooltip("Offset sudut jika sprite tidak menghadap ke atas secara default. " +
             "0 = sprite menghadap atas, 90 = kanan, -90 = kiri")]
    public float spriteAngleOffset = 0f;

    [Header("Mode Patrol")]
    public PatrolMode patrolMode = PatrolMode.Loop;

    [Header("Tabrakan")]
    public string playerTag = "Player";

    [Header("Efek Visual Player")]
    [Min(0.5f)]  public float blinkDuration  = 0.5f;
    [Min(0.05f)] public float blinkInterval  = 0.15f;

    [Header("Dialog Arung — Terkena Obstacle")]
    [TextArea(2, 4)] public string dialogKenaObstacle_ID = "";
    [TextArea(2, 4)] public string dialogKenaObstacle_EN = "";
    public EkspresiArung ekspresiKenaObstacle = EkspresiArung.Sad;

    string DialogKenaObstacle => LocalizationManager.Pick(dialogKenaObstacle_ID, dialogKenaObstacle_EN);

    [Header("Visual")]
    public bool  showGizmos      = true;
    public Color waypointColor   = new Color(0.2f, 0.8f, 1f, 1f);
    public Color activeLineColor = Color.yellow;

    public enum PatrolMode { Loop, PingPong }

    private int  currentIndex  = 0;
    private bool movingForward = true;
    private bool sudahHit      = false;

    void Start()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("[PatrolMover] Tidak ada waypoint!");
            enabled = false;
            return;
        }
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (waypoints.Count == 0) return;
        MoveToCurrentWaypoint();
    }

    void MoveToCurrentWaypoint()
    {
        if (waypoints[currentIndex] == null) return;

        Vector2 target   = waypoints[currentIndex].position;
        float   distance = Vector2.Distance(transform.position, target);

        if (distance > stoppingDistance)
        {
            Vector3 posSebelum = transform.position;

            transform.position = Vector2.MoveTowards(
                transform.position, target, speed * Time.deltaTime);

            UpdateRotation(transform.position - posSebelum);
        }
        else
        {
            transform.position = new Vector3(target.x, target.y, 0f);
            AdvanceWaypoint();
        }
    }

    void UpdateRotation(Vector3 deltaGerak)
    {
        if (!enableRotation) return;
        if (deltaGerak.sqrMagnitude < 0.0001f) return; // diam, pertahankan sudut terakhir

        float angle       = Mathf.Atan2(deltaGerak.y, deltaGerak.x) * Mathf.Rad2Deg;
        float targetAngle = angle - 90f + spriteAngleOffset;

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

    void AdvanceWaypoint()
    {
        switch (patrolMode)
        {
            case PatrolMode.Loop:
                currentIndex = (currentIndex + 1) % waypoints.Count;
                break;
            case PatrolMode.PingPong:
                if (movingForward)
                {
                    currentIndex++;
                    if (currentIndex >= waypoints.Count)
                    { currentIndex = waypoints.Count - 2; movingForward = false; }
                }
                else
                {
                    currentIndex--;
                    if (currentIndex < 0)
                    { currentIndex = 1; movingForward = true; }
                }
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        HandleHit(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;
        HandleHit(collision.gameObject);
    }

    void HandleHit(GameObject player)
    {
        if (sudahHit) return;
        sudahHit = true;

        GuldenManager.Instance?.RegisterObstacleHitCurrentLevel();
        Debug.Log($"[PatrolMover] {gameObject.name} mengenai player! -10 gulden dicatat.");

        PlayerDamageVisual.Instance?.TambahHit();

        AudioManager.Instance?.PlayObstacleHit();

        PlayerBlink blink = player.GetComponent<PlayerBlink>();
        if (blink == null) blink = player.AddComponent<PlayerBlink>();
        blink.MulaiBlink(blinkDuration, blinkInterval);

        if (!string.IsNullOrEmpty(DialogKenaObstacle))
            StartCoroutine(TampilkanDialogTunggu(DialogKenaObstacle, ekspresiKenaObstacle));

        gameObject.SetActive(false);
    }

    IEnumerator TampilkanDialogTunggu(string teks, EkspresiArung ekspresi)
    {
        if (DialogArung.Instance == null) yield break;

        while (DialogArung.Instance.IsShowing)
            yield return null;

        DialogArung.Instance.TampilkanDialog(teks, ekspresi);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || waypoints == null || waypoints.Count == 0) return;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            bool isActive = Application.isPlaying && (i == currentIndex);
            Gizmos.color  = isActive ? activeLineColor : waypointColor;
            Gizmos.DrawWireSphere(waypoints[i].position, isActive ? 0.22f : 0.16f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                waypoints[i].position + Vector3.up * 0.3f, $"WP {i + 1}");
#endif
        }

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            int next = (i + 1) % waypoints.Count;
            if (patrolMode == PatrolMode.PingPong && i == waypoints.Count - 1) break;
            if (waypoints[next] == null) continue;
            Gizmos.color = new Color(waypointColor.r, waypointColor.g, waypointColor.b, 0.4f);
            Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }

        if (Application.isPlaying && waypoints.Count > 0 && waypoints[currentIndex] != null)
        {
            Gizmos.color = activeLineColor;
            Gizmos.DrawLine(transform.position, waypoints[currentIndex].position);
        }
    }
}