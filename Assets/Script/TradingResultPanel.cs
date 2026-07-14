using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TradingResultPanel : MonoBehaviour
{
    public static TradingResultPanel Instance { get; private set; }


    [Header("Judul Level")]
    public TMP_Text judulLevelText;

    [Header("Ringkasan Gulden")]
    public TMP_Text guldenDagangText;
    public TMP_Text biayaObstacleText;
    public TMP_Text totalLevelText;

    [Header("Fakta Historis")]
    public TMP_Text faktaText;

    [Header("Tombol")]
    public Button tombolLanjut;
    public TMP_Text labelTombolLanjut;

    [Header("Konten per Level")]
    public KontenLevel kontenLevel1;
    public KontenLevel kontenLevel2;
    public KontenLevel kontenLevel3;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        tombolLanjut?.onClick.AddListener(OnTombolLanjut);
        gameObject.SetActive(false);
    }


    public void Tampilkan()
    {
        int level        = GetCurrentLevel();
        KontenLevel data = GetKontenLevel(level);

        if (judulLevelText != null)
            judulLevelText.text = LocalizationManager.Pick(data.judulLevel_ID, data.judulLevel_EN);

        int gulden   = GuldenManager.Instance?.GetGuldenByLevel(level) ?? 0;
        int obstacle = GuldenManager.Instance?.GetObstacleCostByLevel(level) ?? 0;
        int total    = gulden - obstacle;

        if (guldenDagangText != null)
            guldenDagangText.text = $"{gulden}";

        if (biayaObstacleText != null)
            biayaObstacleText.text = obstacle > 0
                ? $"-{obstacle} "
                : "-";

        if (totalLevelText != null)
            totalLevelText.text = $"{total}";

        if (faktaText != null)
            faktaText.text = LocalizationManager.Pick(data.faktaHistoris_ID, data.faktaHistoris_EN);

        if (labelTombolLanjut != null)
        {
            bool english = LocalizationManager.Instance?.IsEnglish ?? false;
            if (level < 3)
                labelTombolLanjut.text = english ? "Continue Journey →" : "Lanjutkan Perjalanan →";
            else
                labelTombolLanjut.text = english ? "View Final Result →" : "Lihat Hasil Akhir →";
        }

        AudioManager.Instance?.PlayTradingPanel();

        gameObject.SetActive(true);
        Time.timeScale = 0f;

        Debug.Log($"[TradingResultPanel] Tampil untuk Level {level}");
    }

    void OnTombolLanjut()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);

        if (LevelTransition.Instance != null)
            LevelTransition.Instance.LanjutKeLevel();
        else
            Debug.LogWarning("[TradingResultPanel] LevelTransition.Instance tidak ditemukan!");
    }

    int GetCurrentLevel()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene.Contains("Level1") || scene.Contains("Maluku"))  return 1;
        if (scene.Contains("Level2") || scene.Contains("Batavia")) return 2;
        if (scene.Contains("Level3") || scene.Contains("Malaka"))  return 3;
        return 1;
    }

    KontenLevel GetKontenLevel(int level) => level switch
    {
        1 => kontenLevel1,
        2 => kontenLevel2,
        3 => kontenLevel3,
        _ => kontenLevel1
    };
}


[System.Serializable]
public class KontenLevel
{
    [Header("Judul Level")]
    [Tooltip("Judul dalam Bahasa Indonesia, misal 'Maluku ke Batavia'")]
    public string judulLevel_ID = "";

    [Tooltip("Judul dalam English, misal 'Maluku to Batavia'")]
    public string judulLevel_EN = "";

    [Header("Fakta Historis")]
    [Tooltip("Fakta historis dalam Bahasa Indonesia")]
    [TextArea(3, 6)]
    public string faktaHistoris_ID = "";

    [Tooltip("Fakta historis dalam English")]
    [TextArea(3, 6)]
    public string faktaHistoris_EN = "";
}