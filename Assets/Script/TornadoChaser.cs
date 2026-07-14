using UnityEngine;
using System.Collections;

// SETUP: tambahkan child GameObject dengan SpriteRenderer (lingkaran merah)
// + HitAreaPulse.cs di posisi collider hit untuk animasi pulse merah.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class TornadoChaser : MonoBehaviour
{
    public enum TornadoState { Idle, Chasing, Returning }

    [Header("Target")]
    public string playerTag = "Player";

    [Header("Movement")]
    public float chaseSpeed        = 3f;
    public float returnSpeed       = 5f;

    [Header("Area")]
    public float minX = -8f;
    public float maxX =  8f;
    public float minY = -4f;
    public float maxY =  4f;

    [Header("Distance")]
    public float chaseStopDistance  = 0.1f;
    public float returnStopDistance = 0.1f;

    [Header("Delay")]
    public float startDelay = 1f;

    [Header("Efek Visual Player")]
    [Min(0.5f)]  public float blinkDuration  = 0.5f;
    [Min(0.05f)] public float blinkInterval  = 0.15f;

    [Header("Dialog Arung — Melihat Obstacle")]
    [Tooltip("Tampil sekali saat player pertama kali memasuki area kejar (sebelum kena). " +
             "Jika dialog Arung lain sedang tampil, sistem menunggu sampai selesai.")]
    [TextArea(2, 4)] public string dialogLihatObstacle_ID = "";
    [TextArea(2, 4)] public string dialogLihatObstacle_EN = "";
    public EkspresiArung ekspresiLihatObstacle = EkspresiArung.Startled;

    [Header("Dialog Arung — Terkena Obstacle")]
    [TextArea(2, 4)] public string dialogKenaObstacle_ID = "";
    [TextArea(2, 4)] public string dialogKenaObstacle_EN = "";
    public EkspresiArung ekspresiKenaObstacle = EkspresiArung.Sad;

    string DialogLihatObstacle => LocalizationManager.Pick(dialogLihatObstacle_ID, dialogLihatObstacle_EN);
    string DialogKenaObstacle  => LocalizationManager.Pick(dialogKenaObstacle_ID,  dialogKenaObstacle_EN);

  
    private static bool sudahTampilkanDialogLihat = false;

    private Rigidbody2D  rb;
    private Transform    playerTransform;
    private Vector2      originPosition;
    private TornadoState currentState = TornadoState.Idle;
    private bool         sudahHit     = false;

    public TornadoState CurrentState => currentState;

    void Awake()
    {
        rb                = GetComponent<Rigidbody2D>();
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;
        originPosition    = transform.position;
    }

    void OnEnable()
    {

    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj == null)
        {
            Debug.LogError("[TornadoChaser] Player tidak ditemukan!");
            enabled = false;
            return;
        }
        playerTransform = playerObj.transform;
        Invoke(nameof(StartChasing), startDelay);
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case TornadoState.Idle:      UpdateIdle();      break;
            case TornadoState.Chasing:   UpdateChasing();   break;
            case TornadoState.Returning: UpdateReturning(); break;
        }
    }

    void UpdateIdle()
    {
        rb.velocity = Vector2.zero;
        if (playerTransform != null && IsPlayerInsideArea() && !sudahHit)
            SetState(TornadoState.Chasing);
    }

    void UpdateChasing()
    {
        if (playerTransform == null) return;
        if (!IsPlayerInsideArea()) { SetState(TornadoState.Returning); return; }
        MoveTo(playerTransform.position, chaseSpeed, chaseStopDistance);
    }

    void UpdateReturning()
    {
        float distance = Vector2.Distance(rb.position, originPosition);
        if (distance <= returnStopDistance)
        {
            rb.velocity = Vector2.zero;
            rb.MovePosition(originPosition);
            SetState(TornadoState.Idle);
            return;
        }
        MoveTo(originPosition, returnSpeed, returnStopDistance);
    }

    void MoveTo(Vector2 target, float speed, float stopDistance)
    {
        float distance = Vector2.Distance(rb.position, target);
        if (distance <= stopDistance) { rb.velocity = Vector2.zero; return; }
        rb.velocity = (target - rb.position).normalized * speed;
    }

    bool IsPlayerInsideArea()
    {
        Vector2 pos = playerTransform.position;
        return pos.x >= minX && pos.x <= maxX &&
               pos.y >= minY && pos.y <= maxY;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        TryHitPlayer(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;
        TryHitPlayer(collision.gameObject);
    }

    void TryHitPlayer(GameObject player)
    {
        if (sudahHit) return;

        PlayerBlink blink = player.GetComponent<PlayerBlink>();
        if (blink != null && blink.SedangBlink) return;

        sudahHit = true;

        GuldenManager.Instance?.RegisterObstacleHitCurrentLevel();
        Debug.Log("[TornadoChaser] Player terkena! -10 gulden dicatat.");

        PlayerDamageVisual.Instance?.TambahHit();

        AudioManager.Instance?.PlayObstacleHit();

        if (blink == null) blink = player.AddComponent<PlayerBlink>();
        blink.MulaiBlink(blinkDuration, blinkInterval);

        if (!string.IsNullOrEmpty(DialogKenaObstacle))
            StartCoroutine(TampilkanDialogTunggu(DialogKenaObstacle, ekspresiKenaObstacle));

        gameObject.SetActive(false);
    }

    void SetState(TornadoState newState)
    {
        bool barusanMulaiChasing = (newState == TornadoState.Chasing &&
                                     currentState != TornadoState.Chasing);

        if (currentState == newState) return;
        currentState = newState;
        rb.velocity  = Vector2.zero;

        if (barusanMulaiChasing && !sudahTampilkanDialogLihat &&
            !string.IsNullOrEmpty(DialogLihatObstacle))
        {
            sudahTampilkanDialogLihat = true;
            StartCoroutine(TampilkanDialogTunggu(DialogLihatObstacle, ekspresiLihatObstacle));
        }
    }


    IEnumerator TampilkanDialogTunggu(string teks, EkspresiArung ekspresi)
    {
        if (DialogArung.Instance == null) yield break;

        while (DialogArung.Instance.IsShowing)
            yield return null;

        DialogArung.Instance.TampilkanDialog(teks, ekspresi);
    }

    void StartChasing()
    {
        if (currentState == TornadoState.Idle)
            SetState(TornadoState.Chasing);
    }

    public void ForceReturn() => SetState(TornadoState.Returning);
    public void StopTornado() { SetState(TornadoState.Idle); rb.velocity = Vector2.zero; }

    public static void ResetDialogFlag() => sudahTampilkanDialogLihat = false;

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.12f);
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size   = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireCube(center, size);

        Vector3 origin = Application.isPlaying
            ? new Vector3(originPosition.x, originPosition.y, 0f)
            : transform.position;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(origin, 0.2f);

        if (Application.isPlaying && playerTransform != null)
        {
            if (currentState == TornadoState.Chasing)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }
            else if (currentState == TornadoState.Returning)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, origin);
            }
        }

#if UNITY_EDITOR
        if (Application.isPlaying)
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f, $"{currentState}");
#endif
    }
}