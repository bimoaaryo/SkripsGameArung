using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ObstacleHit : MonoBehaviour
{
    [Header("Tag")]
    public string playerTag = "Player";

    [Header("Efek Visual Player")]
    [Min(0.5f)]  public float blinkDuration  = 0.5f;
    [Min(0.05f)] public float blinkInterval  = 0.15f;

    [Header("Audio (opsional)")]
    public AudioSource audioSource;
    public AudioClip   hitSound;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Dialog Arung — Terkena Obstacle")]
    [TextArea(2, 4)] public string dialogKenaObstacle_ID = "";
    [TextArea(2, 4)] public string dialogKenaObstacle_EN = "";
    public EkspresiArung ekspresiKenaObstacle = EkspresiArung.Sad;

    string DialogKenaObstacle => LocalizationManager.Pick(dialogKenaObstacle_ID, dialogKenaObstacle_EN);

    private bool sudahKena = false;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (sudahKena) return;

        PlayerBlink blink = other.GetComponent<PlayerBlink>();
        if (blink != null && blink.SedangBlink) return;

        sudahKena = true;

        if (GuldenManager.Instance != null)
        {
            GuldenManager.Instance.RegisterObstacleHitCurrentLevel();
            Debug.Log($"[ObstacleHit] {gameObject.name} mengenai player " +
                      $"— -{GuldenManager.Instance.costPerObstacleHit} gulden dicatat");
        }

        PlayerDamageVisual.Instance?.TambahHit();

        AudioManager.Instance?.PlayObstacleHit();

        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound, volume);

        if (blink == null) blink = other.gameObject.AddComponent<PlayerBlink>();
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
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.2f);
        Bounds b = col.bounds;
        Gizmos.DrawCube(b.center, b.size);
        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(b.center, b.size);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            b.center + Vector3.up * (b.extents.y + 0.2f),
            $"Obstacle\n-{(GuldenManager.Instance?.costPerObstacleHit ?? 10)} gulden");
#endif
    }
}