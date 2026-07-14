using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialSceneManager : MonoBehaviour
{
    [Header("Panel per Section (urutan harus sesuai)")]
    public GameObject[] sectionPanels;

    [Header("Navigasi")]
    public Button tombolLanjut;
    public Button tombolSebelumnya;
    public Button tombolMulaiBerlayar;

    [Header("Indikator Progress (opsional)")]
    public TMP_Text progressText;

    [Header("Lanjut ke Scene")]
    public string sceneLevel1 = "Level1_Maluku";

    private int sectionAktif = 0;

    void Start()
    {
        tombolLanjut?.onClick.AddListener(LanjutSection);
        tombolSebelumnya?.onClick.AddListener(SebelumnyaSection);
        tombolMulaiBerlayar?.onClick.AddListener(MulaiBerlayar);

        TampilkanSection(0);
    }

    void LanjutSection()
    {
        if (sectionAktif < sectionPanels.Length - 1)
            TampilkanSection(sectionAktif + 1);
    }

    void SebelumnyaSection()
    {
        if (sectionAktif > 0)
            TampilkanSection(sectionAktif - 1);
    }

    void TampilkanSection(int index)
    {
        for (int i = 0; i < sectionPanels.Length; i++)
            sectionPanels[i]?.SetActive(i == index);

        sectionAktif = index;

        bool isLastSection  = (index == sectionPanels.Length - 1);
        bool isFirstSection = (index == 0);

        if (tombolSebelumnya != null) tombolSebelumnya.gameObject.SetActive(!isFirstSection);
        if (tombolLanjut != null)     tombolLanjut.gameObject.SetActive(!isLastSection);
        if (tombolMulaiBerlayar != null) tombolMulaiBerlayar.gameObject.SetActive(isLastSection);

        if (progressText != null)
            progressText.text = $"{index + 1} / {sectionPanels.Length}";
    }

    void MulaiBerlayar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneLevel1);
    }
}