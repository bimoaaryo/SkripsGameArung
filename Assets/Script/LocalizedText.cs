using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [Header("Teks per Bahasa")]
    [TextArea(1, 3)]
    [Tooltip("Teks dalam Bahasa Indonesia")]
    public string textID = "";

    [TextArea(1, 3)]
    [Tooltip("Teks dalam English")]
    public string textEN = "";

    private TMP_Text tmpText;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    private bool sudahSubscribe = false;

    void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    void Start()
    {
        TrySubscribe();
        Refresh();
    }

    void TrySubscribe()
    {
        if (sudahSubscribe) return;
        if (LocalizationManager.Instance == null) return;

        LocalizationManager.Instance.OnLanguageChanged.AddListener(Refresh);
        sudahSubscribe = true;
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged.RemoveListener(Refresh);
        sudahSubscribe = false;
    }

    public void Refresh()
    {
        if (tmpText == null) tmpText = GetComponent<TMP_Text>();
        if (tmpText == null) return;

        tmpText.text = LocalizationManager.Pick(textID, textEN);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        TMP_Text t = GetComponent<TMP_Text>();
        if (t != null && !string.IsNullOrEmpty(textID))
            t.text = textID;
    }
#endif
}