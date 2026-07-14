using UnityEngine;
using UnityEngine.Events;


public class GuldenManager : MonoBehaviour
{
    public static GuldenManager Instance { get; private set; }

    public UnityEvent<int> OnGuldenChanged;
    public UnityEvent<int> OnObstacleCostChanged;

    private int guldenL1 = 0;
    private int guldenL2 = 0;
    private int guldenL3 = 0;
    private int obstacleCostL1 = 0;
    private int obstacleCostL2 = 0;
    private int obstacleCostL3 = 0;

    [Header("Biaya per hit obstacle")]
    [Min(0)] public int costPerObstacleHit = 10;

    public int TotalGuldenDagang  => guldenL1 + guldenL2 + guldenL3;
    public int TotalObstacleCost  => obstacleCostL1 + obstacleCostL2 + obstacleCostL3;
    public int FinalScore         => Mathf.Max(0, TotalGuldenDagang - TotalObstacleCost);

    public int CurrentLevelGulden
    {
        get => GetCurrentLevel() switch { 1 => guldenL1, 2 => guldenL2, 3 => guldenL3, _ => 0 };
    }

    public int CurrentLevelObstacleCost
    {
        get => GetCurrentLevel() switch { 1 => obstacleCostL1, 2 => obstacleCostL2, 3 => obstacleCostL3, _ => 0 };
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[GuldenManager] Instance dibuat — data mulai dari 0.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddGulden(int amount, int level)
    {
        switch (level)
        {
            case 1: guldenL1 += amount; break;
            case 2: guldenL2 += amount; break;
            case 3: guldenL3 += amount; break;
            default:
                Debug.LogWarning($"[GuldenManager] Level {level} tidak valid!");
                return;
        }

        OnGuldenChanged?.Invoke(TotalGuldenDagang);
        Debug.Log($"[GuldenManager] L{level} +{amount} | Total: {TotalGuldenDagang}");
    }

    public void AddGuldenCurrentLevel(int amount)
        => AddGulden(amount, GetCurrentLevel());

    public void RegisterObstacleHit(int level)
    {
        switch (level)
        {
            case 1: obstacleCostL1 += costPerObstacleHit; break;
            case 2: obstacleCostL2 += costPerObstacleHit; break;
            case 3: obstacleCostL3 += costPerObstacleHit; break;
            default:
                Debug.LogWarning($"[GuldenManager] Level {level} tidak valid!");
                return;
        }

        OnObstacleCostChanged?.Invoke(TotalObstacleCost);
        Debug.Log($"[GuldenManager] Obstacle L{level} | Total biaya: -{TotalObstacleCost}");
    }

    public void RegisterObstacleHitCurrentLevel()
        => RegisterObstacleHit(GetCurrentLevel());

    public int GetGuldenByLevel(int level) => level switch
    {
        1 => guldenL1, 2 => guldenL2, 3 => guldenL3, _ => 0
    };

    public int GetObstacleCostByLevel(int level) => level switch
    {
        1 => obstacleCostL1, 2 => obstacleCostL2, 3 => obstacleCostL3, _ => 0
    };

    public string GetGelar() => GetGelarFromScore(FinalScore);


    public string GetGelarFromScore(int score)
    {
        bool english = LocalizationManager.Instance?.IsEnglish ?? false;
        return english ? GetGelarEN(score) : GetGelarID(score);
    }

    string GetGelarID(int score)
    {
        if (score >= 451) return "Penguasa Jalur Rempah";
        if (score >= 301) return "Raja Rempah";
        if (score >= 151) return "Saudagar Nusantara";
        return "Pedagang Kecil";
    }

    string GetGelarEN(int score)
    {
        if (score >= 451) return "Master of the Spice Route";
        if (score >= 301) return "Spice King";
        if (score >= 151) return "Nusantara Merchant";
        return "Petty Trader";
    }

    public void ResetAll()
    {
        guldenL1 = guldenL2 = guldenL3 = 0;
        obstacleCostL1 = obstacleCostL2 = obstacleCostL3 = 0;

        OnGuldenChanged?.Invoke(0);
        OnObstacleCostChanged?.Invoke(0);

        RempahState.Instance?.ResetAll();

        TornadoChaser.ResetDialogFlag();
        ObstacleSightZone.ResetAllDialogFlags();

        PlayerDamageVisual.Instance?.ResetKeAwal();

        Debug.Log("[GuldenManager] Reset selesai — semua data = 0.");
    }

    int GetCurrentLevel()
    {
        string s = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (s.Contains("Level1") || s.Contains("Maluku"))  return 1;
        if (s.Contains("Level2") || s.Contains("Batavia")) return 2;
        if (s.Contains("Level3") || s.Contains("Malaka"))  return 3;
        Debug.LogWarning($"[GuldenManager] Scene tidak dikenali: {s}");
        return 1;
    }

#if UNITY_EDITOR
    [Header("Debug — Read Only")]
    [SerializeField] private int _debug_guldenL1;
    [SerializeField] private int _debug_guldenL2;
    [SerializeField] private int _debug_guldenL3;
    [SerializeField] private int _debug_totalDagang;
    [SerializeField] private int _debug_totalObstacle;
    [SerializeField] private int _debug_finalScore;
    [SerializeField] private string _debug_gelar;

    void OnValidate()
    {
        _debug_guldenL1      = guldenL1;
        _debug_guldenL2      = guldenL2;
        _debug_guldenL3      = guldenL3;
        _debug_totalDagang   = TotalGuldenDagang;
        _debug_totalObstacle = TotalObstacleCost;
        _debug_finalScore    = FinalScore;
        _debug_gelar         = GetGelar();
    }
#endif
}