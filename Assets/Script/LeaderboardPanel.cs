using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LeaderboardPanel : MonoBehaviour
{


    [Header("Tombol Buka Panel")]
    [Tooltip("Tombol di Main Menu untuk membuka panel leaderboard")]
    public Button leaderboardButton;



    [Header("UI Panel")]
    public Button   tombolTutup;
    public TMP_Text judulText;
    public TMP_Text kosongText;

    [Header("Leaderboard Container")]
    [Tooltip("Vertical Layout Group untuk menampung entri leaderboard")]
    public Transform  leaderboardContainer;
    public GameObject leaderboardEntryPrefab;

    [Header("Pengaturan")]
    [Tooltip("Jumlah entri yang ditampilkan")]
    [Min(3)] public int jumlahEntri = 10;


    void Start()
    {
        gameObject.SetActive(false);

        leaderboardButton?.onClick.AddListener(Buka);
        tombolTutup?.onClick.AddListener(Tutup);
    }

    public void Buka()
    {
        gameObject.SetActive(true);
        MuatLeaderboard();
        Debug.Log("[LeaderboardPanel] Panel dibuka.");
    }

    public void Tutup()
    {
        gameObject.SetActive(false);
        Debug.Log("[LeaderboardPanel] Panel ditutup.");
    }


    void MuatLeaderboard()
    {
        if (leaderboardContainer == null) return;

        foreach (Transform child in leaderboardContainer)
            Destroy(child.gameObject);

        List<LeaderboardEntry> entries = DatabaseManager.Instance?.GetTopScores()
            ?? new List<LeaderboardEntry>();

        if (kosongText != null)
            kosongText.gameObject.SetActive(entries.Count == 0);

        if (entries.Count == 0) return;

        int batas = Mathf.Min(entries.Count, jumlahEntri);

        for (int i = 0; i < batas; i++)
        {
            if (leaderboardEntryPrefab == null) break;

            LeaderboardEntry   entry = entries[i];
            GameObject         obj   = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            LeaderboardEntryUI ui    = obj.GetComponent<LeaderboardEntryUI>();

           
            string gelarSesuaiBahasa = GuldenManager.Instance != null
                ? GuldenManager.Instance.GetGelarFromScore(entry.score)
                : entry.gelar; 

            ui?.SetData(entry.rank, entry.playerName, entry.score, gelarSesuaiBahasa);
        }

        Debug.Log($"[LeaderboardPanel] {batas} entri dimuat.");
    }
}