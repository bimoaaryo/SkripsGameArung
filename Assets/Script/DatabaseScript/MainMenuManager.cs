using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject tapToStartPanel;
    public GameObject inputNamePanel;

    [Header("Tap To Start")]
    public Button tapToStartButton;

    [Header("Input Nama")]
    public TMP_InputField nameInputField;
    public Button         startButton;
    public Button         closeInputNameButton;
    public TMP_Text       warningText;

    [Header("Leaderboard")]
    public LeaderboardPanel leaderboardPanel;
    public Button           leaderboardButton;

    [Header("Quit")]
    public Button quitButton;

    [Header("Tutorial")]
    [Tooltip("Panel tutorial (5 section) di scene MainMenu yang ditampilkan " +
             "setelah nama valid, sebelum masuk Level 1")]
    public TutorialPanel tutorialPanel;

    [Header("Audio (opsional)")]
    public AudioSource audioSource;
    public AudioClip   tapSound;
    public AudioClip   buttonSound;
    public AudioClip   errorSound;

    void Start()
    {
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.Save();

        SetPanel(tapToStartPanel, true);
        SetPanel(inputNamePanel,  false);

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        tapToStartButton?.onClick.AddListener(OnTapToStart);
        startButton?.onClick.AddListener(OnStartPressed);
        quitButton?.onClick.AddListener(OnQuitGame);
        nameInputField?.onSubmit.AddListener(_ => OnStartPressed());
        closeInputNameButton?.onClick.AddListener(OnCloseInputName);
        leaderboardButton?.onClick.AddListener(() => leaderboardPanel?.Buka());
    }

    void OnTapToStart()
    {
        PlaySound(tapSound);
        SetPanel(tapToStartPanel, false);
        SetPanel(inputNamePanel,  true);

        if (nameInputField != null)
        {
            nameInputField.text = "";
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        Debug.Log("[MainMenu] Form input nama ditampilkan.");
    }

    void OnCloseInputName()
    {
        PlaySound(tapSound);
        SetPanel(inputNamePanel,  false);
        SetPanel(tapToStartPanel, true);

        if (nameInputField != null)
            nameInputField.text = "";

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        Debug.Log("[MainMenu] Input nama ditutup — kembali ke TapToStart.");
    }

    public void KembaliKeTapToStart()
    {
        SetPanel(inputNamePanel,  false);
        SetPanel(tapToStartPanel, true);

        if (nameInputField != null)
            nameInputField.text = "";

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        Debug.Log("[MainMenu] Kembali dari Tutorial ke TapToStart.");
    }

    void OnStartPressed()
    {
        string playerName = nameInputField != null
            ? nameInputField.text.Trim()
            : "";

        bool english = LocalizationManager.Instance?.IsEnglish ?? false;

        if (string.IsNullOrEmpty(playerName))
        {
            ShowWarning(english ? "Please enter your name first!" : "Masukkan nama kamu dulu!");
            PlayErrorSound();
            return;
        }

        if (playerName.Length > 20)
        {
            ShowWarning(english ? "Name too long (max 20 characters)" : "Nama terlalu panjang (maks 20 karakter)");
            PlayErrorSound();
            return;
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        PlaySound(buttonSound);

        GuldenManager.Instance?.ResetAll();

        Debug.Log($"[MainMenu] Nama: {playerName} → reset data → Tutorial");

        SetPanel(inputNamePanel, false);

        if (tutorialPanel != null)
            tutorialPanel.Tampilkan();
        else
            Debug.LogWarning("[MainMenu] TutorialPanel belum di-assign! " +
                              "Tidak bisa lanjut ke Level 1.");
    }

    void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    void ShowWarning(string msg)
    {
        if (warningText == null) return;
        warningText.text = msg;
        warningText.gameObject.SetActive(true);
        Invoke(nameof(HideWarning), 2.5f);
    }

    void HideWarning()
    {
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
            return;
        }

        AudioManager.Instance?.PlayTombolKlik();
    }

    void PlayErrorSound()
    {
        if (audioSource != null && errorSound != null)
        {
            audioSource.PlayOneShot(errorSound);
            return;
        }

        AudioManager.Instance?.PlayInputError();
    }

    void OnQuitGame()
    {
        PlaySound(buttonSound);
        Debug.Log("[MainMenu] Quit game.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}