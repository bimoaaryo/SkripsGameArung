using UnityEngine;
using UnityEngine.UI;


public class TutorialPanel : MonoBehaviour
{
    [Header("Tombol")]
    public Button tombolLanjut;

    [Tooltip("Tombol kembali ke menu utama — membatalkan sesi sebelum dimulai")]
    public Button tombolKembali;

    [Header("Lanjut ke Scene")]
    public string sceneCutscene = "Cutscene";

    [Header("Referensi MainMenuManager")]
    [Tooltip("Drag GameObject MainMenuManager dari Hierarchy — " +
             "dibutuhkan untuk mengembalikan tampilan ke TapToStartPanel")]
    public MainMenuManager mainMenuManager;

    void Start()
    {
        tombolLanjut?.onClick.AddListener(LanjutKeCutscene);
        tombolKembali?.onClick.AddListener(KembaliKeMenu);
    }

    public void Tampilkan()
    {
        gameObject.SetActive(true);
    }

    
    void LanjutKeCutscene()
    {
        AudioManager.Instance?.PlayTombolKlik();

        Debug.Log($"[TutorialPanel] Lanjut ke scene: '{sceneCutscene}' — " +
                  $"SceneLoaderRunner.Instance null? {SceneLoaderRunner.Instance == null}");

        SceneLoaderRunner.Instance?.LoadScene(sceneCutscene);
        gameObject.SetActive(false);
    }

 

    void KembaliKeMenu()
    {
        AudioManager.Instance?.PlayTombolKlik();

        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.Save();

        GuldenManager.Instance?.ResetAll();

        gameObject.SetActive(false);

        if (mainMenuManager != null)
            mainMenuManager.KembaliKeTapToStart();
        else
            Debug.LogWarning("[TutorialPanel] MainMenuManager belum di-assign! " +
                             "TapToStartPanel tidak dapat ditampilkan kembali.");
    }
}