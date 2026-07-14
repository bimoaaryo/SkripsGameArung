using UnityEngine;


public class ProximityAudio : MonoBehaviour
{

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip   audioClip;

    [Range(0f, 1f)]
    public float maxVolume = 0.8f;

    [Header("Jarak")]
    [Min(0.1f)] public float hearRange   = 4f;
    [Min(0.1f)] public float silentRange = 8f;

    [Header("Smoothing")]
    [Range(1f, 20f)] public float fadeSpeed = 5f;

    [Header("Debug")]
    public bool showGizmo = true;

    private Transform playerTransform;
    private float     targetVolume  = 0f;
    private bool      wasPaused     = false;


    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning($"[ProximityAudio] AudioSource tidak ditemukan di {gameObject.name}!");
            enabled = false;
            return;
        }

        if (hearRange > silentRange)
            hearRange = silentRange * 0.5f;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning($"[ProximityAudio] Player tidak ditemukan!");

        audioSource.clip         = audioClip;
        audioSource.loop         = true;
        audioSource.volume       = 0f;
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 0f;

        if (audioClip != null)
            audioSource.Play();
    }

    void Update()
    {
        HandlePauseState();

        if (Time.timeScale == 0f) return;
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        targetVolume = CalculateVolume(distance);

        audioSource.volume = Mathf.Lerp(
            audioSource.volume,
            targetVolume,
            fadeSpeed * Time.deltaTime
        );
    }


    void HandlePauseState()
    {
        bool isPaused = Time.timeScale == 0f;

        if (isPaused && !wasPaused)
        {
            audioSource.volume = 0f;
            audioSource.Pause();
            wasPaused = true;
        }
        else if (!isPaused && wasPaused)
        {
            audioSource.UnPause();
            wasPaused = false;
        }
    }


    float CalculateVolume(float distance)
    {
        if (distance <= hearRange)  return maxVolume;
        if (distance >= silentRange) return 0f;

        float t = 1f - (distance - hearRange) / (silentRange - hearRange);
        return t * maxVolume;
    }


    public void StopAudio()
    {
        if (audioSource != null) audioSource.Stop();
    }

    public void StartAudio()
    {
        if (audioSource != null && audioClip != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, hearRange);

        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, silentRange);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (hearRange + 0.2f),
            $"Hear: {hearRange}u | Silent: {silentRange}u"
        );
#endif
    }
}