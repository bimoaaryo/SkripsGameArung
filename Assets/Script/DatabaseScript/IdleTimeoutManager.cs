using UnityEngine;
using UnityEngine.SceneManagement;

public class IdleTimeoutManager : MonoBehaviour
{
    public static IdleTimeoutManager Instance { get; private set; }

    [Header("Timeout")]
    [Tooltip("Durasi idle sebelum reset ke Main Menu (detik)")]
    [Min(30f)] public float timeoutDuration = 180f; // 3 menit

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Scene yang dikecualikan dari timeout (gameplay aktif, cutscene)")]
    public string[] excludedScenes = {
        "Level1_Maluku", "Level2_Batavia", "Level3_Malaka",
        "Cutscene", "Cutscene1to2", "Cutscene2to3"
    };


    private float idleTimer = 0f;
    private bool  isPaused  = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (isPaused) return;

        if (IsExcludedScene()) return;

        if (Time.timeScale == 0f) return;

        if (AnyInputDetected())
        {
            ResetTimer();
            return;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= timeoutDuration)
            TriggerTimeout();
    }

    bool AnyInputDetected()
    {
        if (Input.touchCount > 0) return true;

        if (Input.GetMouseButtonDown(0) ||
            Input.GetMouseButtonDown(1) ||
            Input.anyKeyDown)
            return true;

        return false;
    }

    void TriggerTimeout()
    {
        Debug.Log("[IdleTimeout] Timeout! Kembali ke Main Menu.");

        ResetTimer();

        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        GuldenManager.Instance?.ResetAll();

        SceneLoaderRunner.Instance?.LoadScene(mainMenuSceneName);
    }


    void ResetTimer() => idleTimer = 0f;

    bool IsExcludedScene()
    {
        string current = SceneManager.GetActiveScene().name;
        foreach (string s in excludedScenes)
            if (current == s) return true;
        return false;
    }

    public void PauseTimeout()
    {
        isPaused = true;
        ResetTimer();
    }

    public void ResumeTimeout()
    {
        isPaused = false;
        ResetTimer();
    }

    public void ResetIdleTimer() => ResetTimer();
}