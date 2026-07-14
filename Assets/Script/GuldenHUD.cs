using UnityEngine;
using TMPro;
using System.Collections;

public class GuldenHUD : MonoBehaviour
{
    [Header("Gulden Dagang")]
    public TMP_Text guldenText;
    public string   guldenPrefix = "Gulden: ";

    [Header("Biaya Obstacle")]
    public TMP_Text obstacleCostText;
    public string   obstacleCostPrefix = "Biaya perjalanan: -";
    public bool     hideObstacleCostIfZero = true;

    [Header("Animasi (opsional)")]
    public bool  enablePunchEffect = true;
    [Min(0.1f)] public float punchScale    = 1.2f;
    [Min(0.1f)] public float punchDuration = 0.2f;

    private int   lastGulden       = -1;
    private int   lastObstacleCost = -1;
    private float punchTimer       = 0f;
    private bool  isPunching       = false;
    private Transform punchTarget;

    void Start()
    {
        StartCoroutine(InisialisasiHUD());
    }

    IEnumerator InisialisasiHUD()
    {
        yield return new WaitUntil(() => GuldenManager.Instance != null);

        GuldenManager.Instance.OnGuldenChanged.AddListener(OnGuldenChanged);
        GuldenManager.Instance.OnObstacleCostChanged.AddListener(OnObstacleCostChanged);

        RefreshAll();

        Debug.Log("[GuldenHUD] Inisialisasi selesai.");
    }

    void OnDestroy()
    {
        if (GuldenManager.Instance == null) return;
        GuldenManager.Instance.OnGuldenChanged.RemoveListener(OnGuldenChanged);
        GuldenManager.Instance.OnObstacleCostChanged.RemoveListener(OnObstacleCostChanged);
    }

    void Update()
    {
        if (isPunching) UpdatePunchAnimation();
    }

    void OnGuldenChanged(int total)
    {
        UpdateGuldenText(total);
        if (enablePunchEffect && total != lastGulden)
            StartPunch(guldenText?.transform);
        lastGulden = total;
    }

    void OnObstacleCostChanged(int total)
    {
        UpdateObstacleCostText(total);
        if (enablePunchEffect && total != lastObstacleCost)
            StartPunch(obstacleCostText?.transform);
        lastObstacleCost = total;
    }

    void RefreshAll()
    {
        int gulden   = GuldenManager.Instance.TotalGuldenDagang;
        int obstacle = GuldenManager.Instance.TotalObstacleCost;
        UpdateGuldenText(gulden);
        UpdateObstacleCostText(obstacle);
        lastGulden       = gulden;
        lastObstacleCost = obstacle;
    }

    void UpdateGuldenText(int value)
    {
        if (guldenText == null) return;
        guldenText.text = $"{guldenPrefix}{value}";
    }

    void UpdateObstacleCostText(int value)
    {
        if (obstacleCostText == null) return;
        if (hideObstacleCostIfZero && value == 0)
        {
            obstacleCostText.gameObject.SetActive(false);
            return;
        }
        obstacleCostText.gameObject.SetActive(true);
        obstacleCostText.text = $"-{obstacleCostPrefix}{value}";
    }

    void StartPunch(Transform target)
    {
        if (target == null) return;
        punchTarget = target;
        punchTimer  = 0f;
        isPunching  = true;
    }

    void UpdatePunchAnimation()
    {
        if (punchTarget == null) { isPunching = false; return; }
        punchTimer += Time.unscaledDeltaTime;
        float half = punchDuration / 2f;
        if (punchTimer < half)
            punchTarget.localScale = Vector3.Lerp(Vector3.one, Vector3.one * punchScale, punchTimer / half);
        else if (punchTimer < punchDuration)
            punchTarget.localScale = Vector3.Lerp(Vector3.one * punchScale, Vector3.one, (punchTimer - half) / half);
        else
        {
            punchTarget.localScale = Vector3.one;
            isPunching = false;
        }
    }
}