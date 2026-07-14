using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM per Scene")]
    [Tooltip("BGM untuk Main Menu")]
    public AudioClip bgmMainMenu;

    [Tooltip("BGM untuk Level 1 — Maluku ke Batavia")]
    public AudioClip bgmLevel1;

    [Tooltip("BGM untuk Level 2 — Batavia ke Malaka")]
    public AudioClip bgmLevel2;

    [Tooltip("BGM untuk Level 3 — Malaka ke Maluku")]
    public AudioClip bgmLevel3;

    [Tooltip("BGM untuk Hasil Akhir")]
    public AudioClip bgmHasilAkhir;

    [Header("BGM Settings")]
    [Range(0f, 1f)] public float bgmVolume       = 0.5f;
    [Tooltip("Durasi fade in/out saat ganti BGM")]
    [Min(0.1f)]     public float bgmFadeDuration = 1f;
    public bool loopBGM = true;

    [Header("SFX — Obstacle")]
    [Tooltip("Suara saat player kena obstacle (Tornado, Karang, Kapal Patroli)")]
    public AudioClip sfxObstacleHit;

    [Header("SFX — Transaksi")]
    [Tooltip("Suara saat player berhasil menjual rempah")]
    public AudioClip sfxJualRempah;

    [Tooltip("Suara saat player membeli komoditas")]
    public AudioClip sfxBeliKomoditas;

    [Tooltip("Suara saat opsi tidak tersedia / rempah habis")]
    public AudioClip sfxOpsiTidakTersedia;

    [Header("SFX — Dialog")]
    [Tooltip("Suara saat dialog kakek muncul")]
    public AudioClip sfxDialogMuncul;

    [Header("SFX — Transisi")]
    [Tooltip("Suara saat trading panel muncul")]
    public AudioClip sfxTradingPanel;

    [Tooltip("Suara saat pindah ke level berikutnya")]
    public AudioClip sfxLevelTransisi;

    [Header("SFX — Hasil Akhir")]
    [Tooltip("Suara saat gelar muncul di hasil akhir")]
    public AudioClip sfxGelarMuncul;

    [Header("SFX — UI / Tombol")]
    [Tooltip("Suara klik tombol umum (Mulai, Lanjut, Tutup, dsb)")]
    public AudioClip sfxTombolKlik;

    [Tooltip("Suara saat input nama tidak valid (kosong/terlalu panjang)")]
    public AudioClip sfxInputError;

    [Header("SFX Volume")]
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private Coroutine   fadeRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        PlayBGMSesuaiScene();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
                       UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        PlayBGMSesuaiScene();
    }


    void SetupAudioSources()
    {
        bgmSource             = gameObject.AddComponent<AudioSource>();
        bgmSource.loop        = loopBGM;
        bgmSource.volume      = bgmVolume;
        bgmSource.playOnAwake = false;

        sfxSource             = gameObject.AddComponent<AudioSource>();
        sfxSource.loop        = false;
        sfxSource.volume      = sfxVolume;
        sfxSource.playOnAwake = false;
    }


    void PlayBGMSesuaiScene()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (scene.Contains("Cutscene"))
        {
            if (bgmSource.isPlaying)
                StartCoroutine(FadeOutBGM());
            return;
        }

        AudioClip target = null;

        if (scene.Contains("MainMenu") || scene.Contains("Menu"))
            target = bgmMainMenu;
        else if (scene.Contains("Level1") || scene.Contains("Maluku"))
            target = bgmLevel1;
        else if (scene.Contains("Level2") || scene.Contains("Batavia"))
            target = bgmLevel2;
        else if (scene.Contains("Level3") || scene.Contains("Malaka"))
            target = bgmLevel3;
        else if (scene.Contains("HasilAkhir") || scene.Contains("Hasil"))
            target = bgmHasilAkhir;

        if (target == null) return;

        if (bgmSource.clip == target && bgmSource.isPlaying) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(GantiBGM(target));
    }

    IEnumerator GantiBGM(AudioClip clipBaru)
    {
        if (bgmSource.isPlaying)
        {
            float startVol = bgmSource.volume;
            float elapsed  = 0f;

            while (elapsed < bgmFadeDuration)
            {
                elapsed          += Time.unscaledDeltaTime;
                bgmSource.volume  = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
                yield return null;
            }

            bgmSource.Stop();
        }
        bgmSource.clip   = clipBaru;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float elapsed2 = 0f;
        while (elapsed2 < bgmFadeDuration)
        {
            elapsed2         += Time.unscaledDeltaTime;
            bgmSource.volume  = Mathf.Lerp(0f, bgmVolume, elapsed2 / bgmFadeDuration);
            yield return null;
        }

        bgmSource.volume = bgmVolume;

        Debug.Log($"[AudioManager] BGM: {clipBaru.name}");
    }

    IEnumerator FadeOutBGM()
    {
        float startVol = bgmSource.volume;
        float elapsed   = 0f;

        while (elapsed < bgmFadeDuration)
        {
            elapsed          += Time.unscaledDeltaTime;
            bgmSource.volume  = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = bgmVolume; 

        Debug.Log("[AudioManager] BGM dihentikan untuk Cutscene.");
    }


    public void SetBGMVolume(float volume)
    {
        bgmVolume        = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void PauseBGM()  => bgmSource.Pause();
    public void ResumeBGM() => bgmSource.UnPause();
    public void StopBGM()   => bgmSource.Stop();

    public void PlayObstacleHit()       => PlaySFX(sfxObstacleHit);
    public void PlayJualRempah()        => PlaySFX(sfxJualRempah);
    public void PlayBeliKomoditas()     => PlaySFX(sfxBeliKomoditas);
    public void PlayOpsiTidakTersedia() => PlaySFX(sfxOpsiTidakTersedia);
    public void PlayDialogMuncul()      => PlaySFX(sfxDialogMuncul);
    public void PlayTradingPanel()      => PlaySFX(sfxTradingPanel);
    public void PlayLevelTransisi()     => PlaySFX(sfxLevelTransisi);
    public void PlayGelarMuncul()       => PlaySFX(sfxGelarMuncul);
    public void PlayTombolKlik()        => PlaySFX(sfxTombolKlik);
    public void PlayInputError()        => PlaySFX(sfxInputError);

    public void SetSFXVolume(float volume)
    {
        sfxVolume        = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}