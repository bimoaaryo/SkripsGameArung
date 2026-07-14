using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HasilAkhirPanel : MonoBehaviour
{

    [Header("Hasil Pemain")]
    public TMP_Text namaPemainText;
    public TMP_Text gelarText;
    public TMP_Text skorText;
    public TMP_Text peringkatText;

    [Header("Breakdown Gulden")]
    public TMP_Text guldenL1Text;
    public TMP_Text guldenL2Text;
    public TMP_Text guldenL3Text;
    public TMP_Text totalObstacleText;
    public TMP_Text totalAkhirText;

    [Header("Leaderboard")]
    public Transform      leaderboardContainer;
    public GameObject     leaderboardEntryPrefab;
    public TMP_Text       leaderboardKosongText;

    [Header("Tombol")]
    public Button tombolMainLagi;
    public Button tombolMenuUtama;

    void Start()
    {
        if (GuldenManager.Instance == null)
        {
            Debug.LogWarning("[HasilAkhirPanel] GuldenManager tidak ditemukan!");
            return;
        }

        string nama  = PlayerPrefs.GetString("PlayerName", "Pemain");
        int    skor  = GuldenManager.Instance.FinalScore;
        string gelar = GuldenManager.Instance.GetGelar();

        DatabaseManager.Instance?.SaveScore(nama, skor, gelar);

        AudioManager.Instance?.PlayGelarMuncul();

        TampilkanHasil(nama, skor, gelar);
        TampilkanLeaderboard();

        tombolMainLagi?.onClick.AddListener(MainLagi);
        tombolMenuUtama?.onClick.AddListener(KembaliMenu);


    }

    void TampilkanHasil(string nama, int skor, string gelar)
    {
        bool english = LocalizationManager.Instance?.IsEnglish ?? false;

        if (namaPemainText != null)
            namaPemainText.text = english ? $"Congratulations, {nama}" : $"Selamat, {nama}";

        if (gelarText != null) gelarText.text = gelar;
        if (skorText  != null) skorText.text  = $"{skor} Gulden";

        int rank = DatabaseManager.Instance?.GetPlayerRank(skor) ?? -1;
        if (peringkatText != null)
        {
            string labelPeringkat = english ? "Rank" : "Peringkat";
            peringkatText.text = rank > 0 ? $"{labelPeringkat} #{rank}" : $"{labelPeringkat} -";
        }

        int l1  = GuldenManager.Instance.GetGuldenByLevel(1);
        int l2  = GuldenManager.Instance.GetGuldenByLevel(2);
        int l3  = GuldenManager.Instance.GetGuldenByLevel(3);
        int obs = GuldenManager.Instance.TotalObstacleCost;

        if (guldenL1Text != null) guldenL1Text.text = $"+{l1}";
        if (guldenL2Text != null) guldenL2Text.text = $"+{l2}";
        if (guldenL3Text != null) guldenL3Text.text = $"+{l3}";
        if (totalObstacleText != null)
            totalObstacleText.text = obs > 0 ? $"-{obs}" : "0";
        if (totalAkhirText != null) totalAkhirText.text = $"{skor}";
    }

    void TampilkanLeaderboard()
    {
        if (leaderboardContainer == null) return;

        foreach (Transform child in leaderboardContainer)
            Destroy(child.gameObject);

        List<LeaderboardEntry> entries = DatabaseManager.Instance?.GetTopScores()
            ?? new List<LeaderboardEntry>();

        if (entries.Count == 0)
        {
            if (leaderboardKosongText != null)
                leaderboardKosongText.gameObject.SetActive(true);
            return;
        }

        if (leaderboardKosongText != null)
            leaderboardKosongText.gameObject.SetActive(false);

        string namaPemain = PlayerPrefs.GetString("PlayerName", "");
        int    skorPemain = GuldenManager.Instance?.FinalScore ?? 0;
        int    indexPemain = -1;

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].playerName == namaPemain && entries[i].score == skorPemain)
            { indexPemain = i; break; }
        }

        int tampilAtas  = indexPemain > 0 ? indexPemain - 1 : -1;
        int tampilBawah = indexPemain < entries.Count - 1 ? indexPemain + 1 : -1;

        List<int> indexDitampilkan = new List<int>();
        if (tampilAtas  >= 0) indexDitampilkan.Add(tampilAtas);
        if (indexPemain >= 0) indexDitampilkan.Add(indexPemain);
        if (tampilBawah >= 0) indexDitampilkan.Add(tampilBawah);

        if (indexPemain < 0)
        {
            for (int i = 0; i < Mathf.Min(3, entries.Count); i++)
                indexDitampilkan.Add(i);
        }

        foreach (int idx in indexDitampilkan)
        {
            if (leaderboardEntryPrefab == null) break;
            LeaderboardEntry entry = entries[idx];
            GameObject obj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            LeaderboardEntryUI ui = obj.GetComponent<LeaderboardEntryUI>();

            bool isPemain = idx == indexPemain;
            string gelarSesuaiBahasa = GuldenManager.Instance != null
                ? GuldenManager.Instance.GetGelarFromScore(entry.score)
                : entry.gelar;

            ui?.SetData(entry.rank, entry.playerName, entry.score, gelarSesuaiBahasa, isPemain);
        }
    }


    void MainLagi()
    {
        GuldenManager.Instance?.ResetAll();
        LevelTransition.Instance?.UlangiDariAwal();
    }

    void KembaliMenu()
    {
        GuldenManager.Instance?.ResetAll();
        SceneLoaderRunner.Instance?.LoadScene("MainMenu");
    }


}