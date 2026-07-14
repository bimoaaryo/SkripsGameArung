using UnityEngine;
using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;


public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [Header("Reset Otomatis")]
    [Min(0)] public int resetIntervalDays = 30;

    [Header("Batas Leaderboard")]
    [Min(5)] public int maxEntries = 10;

    private string           dbPath;
    private SqliteConnection dbConnection;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitDatabase();
            CheckAutoReset();
        }
        else Destroy(gameObject);
    }

    void OnDestroy() => dbConnection?.Close();


    void InitDatabase()
    {
        dbPath = $"URI=file:{Application.persistentDataPath}/spicerun.db";
        try
        {
            dbConnection = new SqliteConnection(dbPath);
            dbConnection.Open();
            CreateTables();
            Debug.Log($"[DB] Terbuka: {Application.persistentDataPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DB] Gagal: {e.Message}");
        }
    }

    void CreateTables()
    {
        using var cmd = dbConnection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS leaderboard (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                player_name TEXT    NOT NULL,
                score       INTEGER NOT NULL,
                gelar       TEXT    NOT NULL,
                created_at  TEXT    NOT NULL
            );
            CREATE TABLE IF NOT EXISTS reset_log (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                reset_date  TEXT NOT NULL,
                reset_type  TEXT NOT NULL,
                keterangan  TEXT
            );";
        cmd.ExecuteNonQuery();
    }


    public void SaveScore(string playerName, int score, string gelar)
    {
        if (dbConnection == null) return;
        try
        {
            using var cmd = dbConnection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO leaderboard (player_name, score, gelar, created_at)
                VALUES (@name, @score, @gelar, @date);";
            cmd.Parameters.AddWithValue("@name",  playerName);
            cmd.Parameters.AddWithValue("@score", score);
            cmd.Parameters.AddWithValue("@gelar", gelar);
            cmd.Parameters.AddWithValue("@date",  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.ExecuteNonQuery();
            TrimLeaderboard();
            Debug.Log($"[DB] Simpan: {playerName} | {score} | {gelar}");
        }
        catch (Exception e) { Debug.LogError($"[DB] Gagal simpan: {e.Message}"); }
    }

    void TrimLeaderboard()
    {
        using var cmd = dbConnection.CreateCommand();
        cmd.CommandText = @"
            DELETE FROM leaderboard
            WHERE id NOT IN (
                SELECT id FROM leaderboard
                ORDER BY score DESC LIMIT @max);";
        cmd.Parameters.AddWithValue("@max", maxEntries);
        cmd.ExecuteNonQuery();
    }


    public List<LeaderboardEntry> GetTopScores()
    {
        var list = new List<LeaderboardEntry>();
        if (dbConnection == null) return list;
        try
        {
            using var cmd = dbConnection.CreateCommand();
            cmd.CommandText = @"
                SELECT player_name, score, gelar, created_at
                FROM leaderboard
                ORDER BY score DESC LIMIT @max;";
            cmd.Parameters.AddWithValue("@max", maxEntries);
            using var reader = cmd.ExecuteReader();
            int rank = 1;
            while (reader.Read())
                list.Add(new LeaderboardEntry
                {
                    rank       = rank++,
                    playerName = reader.GetString(0),
                    score      = reader.GetInt32(1),
                    gelar      = reader.GetString(2),
                    createdAt  = reader.GetString(3)
                });
        }
        catch (Exception e) { Debug.LogError($"[DB] Gagal ambil: {e.Message}"); }
        return list;
    }

    public int GetPlayerRank(int score)
    {
        if (dbConnection == null) return -1;
        try
        {
            using var cmd = dbConnection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) + 1 FROM leaderboard WHERE score > @score;";
            cmd.Parameters.AddWithValue("@score", score);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch (Exception e) { Debug.LogError($"[DB] Gagal rank: {e.Message}"); return -1; }
    }



    public void ResetLeaderboard(string keterangan = "Reset manual oleh petugas")
    {
        if (dbConnection == null) return;
        try
        {
            using var cmd = dbConnection.CreateCommand();
            cmd.CommandText = "DELETE FROM leaderboard;";
            cmd.ExecuteNonQuery();
            LogReset("manual", keterangan);
            PlayerPrefs.SetString("LastResetDate", DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
            Debug.Log("[DB] Reset manual selesai.");
        }
        catch (Exception e) { Debug.LogError($"[DB] Gagal reset: {e.Message}"); }
    }

    void CheckAutoReset()
    {
        if (resetIntervalDays <= 0) return;
        string last = PlayerPrefs.GetString("LastResetDate", "");
        if (string.IsNullOrEmpty(last))
        {
            PlayerPrefs.SetString("LastResetDate", DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
            return;
        }
        int days = (DateTime.Now - DateTime.Parse(last)).Days;
        if (days < resetIntervalDays) return;

        using var cmd = dbConnection.CreateCommand();
        cmd.CommandText = "DELETE FROM leaderboard;";
        cmd.ExecuteNonQuery();
        PlayerPrefs.SetString("LastResetDate", DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
        LogReset("auto", $"Auto reset setelah {days} hari");
        Debug.Log($"[DB] Auto reset setelah {days} hari.");
    }

    void LogReset(string type, string keterangan)
    {
        try
        {
            using var cmd = dbConnection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO reset_log (reset_date, reset_type, keterangan)
                VALUES (@date, @type, @ket);";
            cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@ket",  keterangan);
            cmd.ExecuteNonQuery();
        }
        catch (Exception e) { Debug.LogError($"[DB] Gagal log: {e.Message}"); }
    }
}

[System.Serializable]
public class LeaderboardEntry
{
    public int    rank;
    public string playerName;
    public int    score;
    public string gelar;
    public string createdAt;
}