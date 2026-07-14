using UnityEngine;
using UnityEngine.UI;


public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance { get; private set; }


    [Header("Panel Audio")]
    [Tooltip("Suara saat FailPanel muncul")]
    public AudioClip failPanelSound;

    [Tooltip("Suara saat WinPanel muncul")]
    public AudioClip winPanelSound;

    [Header("Button Audio")]
    [Tooltip("Suara saat button UI diklik (default untuk semua button)")]
    public AudioClip buttonClickSound;

    [Tooltip("Suara khusus tombol Retry (kosongkan = pakai buttonClickSound)")]
    public AudioClip retryButtonSound;

    [Tooltip("Suara khusus tombol Map (kosongkan = pakai buttonClickSound)")]
    public AudioClip mapButtonSound;

    [Tooltip("Suara khusus tombol Replay (kosongkan = pakai buttonClickSound)")]
    public AudioClip replayButtonSound;

    [Header("Volume")]
    [Range(0f, 1f)] public float panelVolume  = 1f;
    [Range(0f, 1f)] public float buttonVolume = 0.8f;

    [Header("Auto Register Buttons (opsional)")]
    [Tooltip("Drag semua Button UI ke sini untuk auto-assign suara klik")]
    public Button[] autoRegisterButtons;

    private AudioSource audioSource;

   
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (autoRegisterButtons != null)
        {
            foreach (Button btn in autoRegisterButtons)
            {
                if (btn != null)
                    btn.onClick.AddListener(PlayButtonClick);
            }
        }
    }


    public void PlayFailSound()
    {
        Play(failPanelSound, panelVolume);
    }

    public void PlayWinSound()
    {
        Play(winPanelSound, panelVolume);
    }


    public void PlayButtonClick()
    {
        Play(buttonClickSound, buttonVolume);
    }

    public void PlayRetryClick()
    {
        Play(retryButtonSound != null ? retryButtonSound : buttonClickSound, buttonVolume);
    }

    public void PlayMapClick()
    {
        Play(mapButtonSound != null ? mapButtonSound : buttonClickSound, buttonVolume);
    }

    public void PlayReplayClick()
    {
        Play(replayButtonSound != null ? replayButtonSound : buttonClickSound, buttonVolume);
    }

    void Play(AudioClip clip, float volume)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }
}