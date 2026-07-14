using UnityEngine;


public class TornadoAnimator : MonoBehaviour
{

    [Header("Rotasi Sprite")]
    [Tooltip("Kecepatan rotasi derajat per detik. Negatif = berlawanan arah jarum jam")]
    public float rotationSpeed = -180f;

    [Tooltip("Aktifkan rotasi visual")]
    public bool enableRotation = true;

    [Header("Pulsasi Ukuran")]
    [Tooltip("Aktifkan efek pulsasi (membesar-mengecil)")]
    public bool enablePulse = true;

    [Tooltip("Seberapa besar pulsasi (0.1 = plus minus 10% dari ukuran asli)")]
    [Range(0f, 0.5f)]
    public float pulseAmount = 0.08f;

    [Tooltip("Seberapa cepat pulsasi")]
    [Range(0.5f, 10f)]
    public float pulseSpeed = 3f;


    [Header("Animator (opsional)")]
    [Tooltip("Kosongkan jika tidak pakai Animator Controller")]
    public Animator tornadoAnimator;

    [Tooltip("Nama Float parameter kecepatan gerak di Animator")]
    public string speedParamName = "MoveSpeed";

    [Tooltip("Nama Trigger saat mengenai player (opsional)")]
    public string hitTriggerName = "Hit";

    [Tooltip("Nama Trigger saat tornado kembali ke origin (opsional)")]
    public string returnTriggerName = "Return";


    private Vector3         baseScale;
    private TornadoChaser chaser;
    private Rigidbody2D     rb;
    private TornadoChaser.TornadoState lastState;

    void Awake()
    {
        baseScale = transform.localScale;
        chaser    = GetComponent<TornadoChaser>();
        rb        = GetComponent<Rigidbody2D>();

        if (tornadoAnimator == null)
            tornadoAnimator = GetComponent<Animator>();

        if (chaser == null)
            Debug.LogWarning("[TornadoAnimator] TornadoChaser tidak ditemukan di GameObject yang sama!");
    }

    void Start()
    {
        if (chaser != null)
            lastState = chaser.CurrentState;
    }

    void Update()
    {
        if (enableRotation)
            ApplyRotation();

        if (enablePulse)
            ApplyPulse();

        if (chaser != null)
        {
            UpdateAnimatorParams();
            DetectStateChange();
        }
    }


    void ApplyRotation()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void ApplyPulse()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;
    }

    void UpdateAnimatorParams()
    {
        if (tornadoAnimator == null) return;
        if (string.IsNullOrEmpty(speedParamName)) return;

        float speed = rb != null ? rb.velocity.magnitude : 0f;
        tornadoAnimator.SetFloat(speedParamName, speed);
    }

    void DetectStateChange()
    {
        if (tornadoAnimator == null) return;
        if (chaser.CurrentState == lastState) return;

        switch (chaser.CurrentState)
        {
            case TornadoChaser.TornadoState.Returning:
                if (!string.IsNullOrEmpty(returnTriggerName))
                    tornadoAnimator.SetTrigger(returnTriggerName);
                break;
        }

        lastState = chaser.CurrentState;
    }


    public void PlayHitAnimation()
    {
        if (tornadoAnimator != null && !string.IsNullOrEmpty(hitTriggerName))
            tornadoAnimator.SetTrigger(hitTriggerName);
    }
}