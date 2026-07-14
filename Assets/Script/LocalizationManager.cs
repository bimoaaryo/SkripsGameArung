using UnityEngine;
using UnityEngine.Events;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public UnityEvent OnLanguageChanged;

    private const string PREF_KEY = "GameLanguage";

    public bool IsEnglish { get; private set; } = false;

    public string CurrentCode => IsEnglish ? "en" : "id";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetIndonesian() => SetLanguage(false);

    public void SetEnglish() => SetLanguage(true);

    public void ToggleLanguage() => SetLanguage(!IsEnglish);

    void SetLanguage(bool english)
    {
        IsEnglish = english;
        PlayerPrefs.SetString(PREF_KEY, english ? "en" : "id");
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
        Debug.Log($"[LocalizationManager] Bahasa: {(english ? "English" : "Indonesia")}");
    }

    void LoadLanguage()
    {
        string code = PlayerPrefs.GetString(PREF_KEY, "id");
        IsEnglish   = code == "en";
        Debug.Log($"[LocalizationManager] Bahasa dimuat: {(IsEnglish ? "English" : "Indonesia")}");
    }


    public static string Pick(string textID, string textEN)
    {
        bool english = Instance != null && Instance.IsEnglish;
        string result = english ? textEN : textID;
        return string.IsNullOrEmpty(result) ? textID : result;
    }
}