using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;


public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance { get; private set; }

    [Header("Nama Scene")]
    [Tooltip("Nama scene Level 1")]
    public string sceneLevel1 = "Level1_Maluku";

    [Tooltip("Nama scene Level 2")]
    public string sceneLevel2 = "Level2_Batavia";

    [Tooltip("Nama scene Level 3")]
    public string sceneLevel3 = "Level3_Malaka";

    [Tooltip("Nama scene hasil akhir / leaderboard")]
    public string sceneHasilAkhir = "HasilAkhir";

    [Header("Cutscene Antar Level")]
    [Tooltip("Cutscene yang diputar SETELAH Level 1 selesai, SEBELUM masuk Level 2. " +
             "Kosongkan jika tidak ingin ada cutscene di transisi ini.")]
    public string sceneCutscene1ke2 = "Cutscene1to2";

    [Tooltip("Cutscene yang diputar SETELAH Level 2 selesai, SEBELUM masuk Level 3. " +
             "Kosongkan jika tidak ingin ada cutscene di transisi ini.")]
    public string sceneCutscene2ke3 = "Cutscene2to3";

    [Header("Transisi")]
    [Tooltip("Durasi fade out sebelum pindah scene")]
    [Min(0.3f)] public float fadeDuration = 0.8f;

    [Header("Fade Overlay")]
    [Tooltip("CanvasGroup untuk efek fade layar hitam")]
    public CanvasGroup fadeOverlay;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (fadeOverlay != null)
            StartCoroutine(FadeIn());
    }

    public void LanjutKeLevel()
    {
        string sceneBerikutnya = GetSceneBerikutnya();
        Debug.Log($"[LevelTransition] Lanjut ke: {sceneBerikutnya}");
        StartCoroutine(TransisiScene(sceneBerikutnya));
    }


    public void KembaliKeMainMenu()
    {
        if (GuldenManager.Instance != null)
            GuldenManager.Instance.ResetAll();

        StartCoroutine(TransisiScene("MainMenu"));
    }

    public void UlangiDariAwal()
    {
        if (GuldenManager.Instance != null)
            GuldenManager.Instance.ResetAll();

        StartCoroutine(TransisiScene(sceneLevel1));
    }

    public void LanjutKeLevel2DariCutscene()
    {
        SceneLoaderRunner.Instance?.LoadScene(sceneLevel2);
    }

    public void LanjutKeLevel3DariCutscene()
    {
        SceneLoaderRunner.Instance?.LoadScene(sceneLevel3);
    }


    string GetSceneBerikutnya()
    {
        string current = SceneManager.GetActiveScene().name;

        Debug.Log($"[LevelTransition] Scene aktif: '{current}'");

        if (current == sceneLevel1 ||
            current.Contains("Level1") || current.Contains("Maluku"))
        {
            string tujuan = !string.IsNullOrEmpty(sceneCutscene1ke2) ? sceneCutscene1ke2 : sceneLevel2;
            Debug.Log($"[LevelTransition] → {tujuan}");
            return tujuan;
        }

        if (current == sceneLevel2 ||
            current.Contains("Level2") || current.Contains("Batavia"))
        {
            string tujuan = !string.IsNullOrEmpty(sceneCutscene2ke3) ? sceneCutscene2ke3 : sceneLevel3;
            Debug.Log($"[LevelTransition] → {tujuan}");
            return tujuan;
        }

        if (current == sceneLevel3 ||
            current.Contains("Level3") || current.Contains("Malaka"))
        {
            Debug.Log($"[LevelTransition] → {sceneHasilAkhir}");
            return sceneHasilAkhir;
        }

        Debug.LogWarning($"[LevelTransition] Scene '{current}' tidak cocok dengan " +
                         $"'{sceneLevel1}', '{sceneLevel2}', atau '{sceneLevel3}'. " +
                         $"Periksa nama scene di Inspector LevelTransition.");
        return sceneHasilAkhir;
    }

    IEnumerator TransisiScene(string namaScene)
    {
        AudioManager.Instance?.PlayLevelTransisi();

        PlayerDamageVisual.Instance?.ResetKeAwal();

        if (fadeOverlay != null)
            yield return StartCoroutine(FadeOut());
        else
            yield return new WaitForSeconds(fadeDuration);

        AsyncOperation op = SceneManager.LoadSceneAsync(namaScene);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;
    }

    IEnumerator FadeIn()
    {
        if (fadeOverlay == null) yield break;

        fadeOverlay.alpha = 1f;
        float elapsed     = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed           += Time.unscaledDeltaTime;
            fadeOverlay.alpha  = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        fadeOverlay.alpha = 0f;
    }

    IEnumerator FadeOut()
    {
        if (fadeOverlay == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed           += Time.unscaledDeltaTime;
            fadeOverlay.alpha  = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeOverlay.alpha = 1f;
    }
}