using UnityEngine;
using System.Collections.Generic;


public class ObjectivePointer : MonoBehaviour
{
    public static ObjectivePointer Instance { get; private set; }

    [Header("Referensi Player")]
    [Tooltip("Drag GameObject Player — pointer mengikuti posisi ini")]
    public Transform playerTransform;

    [Tooltip("Offset posisi pointer dari pusat player (world unit). " +
             "Pointer muncul di posisi player + offset ini sebelum arah diatur.")]
    public Vector2 offsetDariPlayer = new Vector2(0f, 0.8f);

    [Header("Target Tujuan (urutan dari awal sampai akhir level)")]
    [Tooltip("Drag target secara berurutan: Pedagang1, Pedagang2 (jika ada), FinishZone, dsb.")]
    public List<Transform> targets = new List<Transform>();

    [Header("Pengaturan Visual")]
    [Tooltip("Offset sudut jika sprite panah tidak menghadap ke atas secara default. " +
             "0 = menghadap atas, 90 = menghadap kanan")]
    public float spriteAngleOffset = 0f;

    [Tooltip("Aktifkan untuk menyembunyikan pointer saat player sudah sangat dekat dengan target")]
    public bool sembunyikanJikaDekat = true;

    [Min(0.5f)] public float jarakDekat = 8f;

    private SpriteRenderer spriteRenderer;
    private int indexTarget = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (playerTransform == null)
        {
            // Auto-detect via tag jika belum di-assign
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else Debug.LogWarning("[ObjectivePointer] Player belum di-assign dan tidak ditemukan via tag!");
        }

        if (targets.Count == 0)
        {
            Debug.LogWarning("[ObjectivePointer] Tidak ada target yang diisi di Inspector!");
            SetVisibilitas(false);
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        transform.position = (Vector2)playerTransform.position + offsetDariPlayer;

        if (targets.Count == 0) return;

        while (indexTarget < targets.Count && !IsTargetAktif(targets[indexTarget]))
            indexTarget++;

        if (indexTarget >= targets.Count)
        {
            SetVisibilitas(false);
            return;
        }

        Transform targetSaatIni = targets[indexTarget];
        Vector2 arah = (Vector2)(targetSaatIni.position - playerTransform.position);
        float jarak  = arah.magnitude;

        if (sembunyikanJikaDekat && jarak <= jarakDekat)
        {
            SetVisibilitas(false);
            return;
        }

        SetVisibilitas(true);

        float sudut = Mathf.Atan2(arah.y, arah.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, sudut - 90f + spriteAngleOffset);
    }

    bool IsTargetAktif(Transform target)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        PedagangNPC npc = target.GetComponent<PedagangNPC>();
        if (npc != null)
        {
            if (npc.SudahSelesai) return false;

            SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
            if (sr != null && !sr.enabled) return false;
        }

        return true;
    }



    public void SetTargetSelesai()
    {
        indexTarget++;

        if (indexTarget >= targets.Count)
        {
            SetVisibilitas(false);
            Debug.Log("[ObjectivePointer] Semua target selesai.");
        }
        else
        {
            Debug.Log($"[ObjectivePointer] Target berikutnya: " +
                      $"{(targets[indexTarget] != null ? targets[indexTarget].name : "null")}");
        }
    }

    public void Reset()
    {
        indexTarget = 0;
        SetVisibilitas(targets.Count > 0);
    }

    void SetVisibilitas(bool tampil)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = tampil;
    }
}