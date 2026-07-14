using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static IEnumerator LoadAsync(string namaScene)
    {
        SceneLoaderRunner.Instance?.TampilkanLoading();
        Debug.Log($"[SceneLoader] Mulai load: {namaScene}");

        yield return new WaitForEndOfFrame();

        AsyncOperation op = SceneManager.LoadSceneAsync(namaScene);
        if (op == null)
        {
            SceneLoaderRunner.Instance?.SembunyikanLoading();
            yield break;
        }

        while (!op.isDone)
        {
            SceneLoaderRunner.Instance?.UpdateProgress(op.progress);
            yield return null;
        }

        SceneLoaderRunner.Instance?.SembunyikanLoading();
    }
}

public class SceneLoaderRunner : MonoBehaviour
{
    public static SceneLoaderRunner Instance { get; private set; }

    [Header("Loading Screen")]
    public GameObject loadingScreen;

    [Header("UI (opsional)")]
    public TMP_Text loadingText;
    public Image progressBar;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string namaScene)
    {
        Debug.Log($"[SceneLoaderRunner] LoadScene({namaScene}) dipanggil.");
        StartCoroutine(SceneLoader.LoadAsync(namaScene));
    }

    public void TampilkanLoading()
    {
        Debug.Log("[SceneLoaderRunner] TampilkanLoading dipanggil.");

        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        UpdateProgress(0f);

        bool english = LocalizationManager.Instance?.IsEnglish ?? false;
        if (loadingText != null)
            loadingText.text = english ? "Loading..." : "Memuat...";
    }

    public void SembunyikanLoading()
    {
        Debug.Log($"[SceneLoaderRunner] SembunyikanLoading — " +
                  $"loadingScreen null? {loadingScreen == null}" +
                  $"{(loadingScreen != null ? $", nama: {loadingScreen.name}, aktif: {loadingScreen.activeSelf}" : "")}");

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            Debug.Log($"[SceneLoaderRunner] SetActive(false) selesai — aktif sekarang: {loadingScreen.activeSelf}");
        }
    }

    public void UpdateProgress(float value)
    {
        if (progressBar != null)
            progressBar.fillAmount = Mathf.Clamp01(value);
    }
}