using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class LeaderboardScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI leaderboardText;

    [Header("Settings")]
    [SerializeField] private int maxEntries = 10;
    [SerializeField] private bool updateOnStart = true;
    [SerializeField] private bool updateOnEnable = true;

    private string saveFilePath;

    void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "win_records.json");
        Debug.Log($"[Leaderboard] Save file path: {saveFilePath}");

        if (updateOnStart)
        {
            UpdateLeaderboard();
        }
    }

    void OnEnable()
    {
        if (updateOnEnable && saveFilePath != null)
        {
            UpdateLeaderboard();
        }
    }

    /// <summary>
    /// Updates the leaderboard display with top players
    /// </summary>
    public void UpdateLeaderboard()
    {
        if (leaderboardText == null)
        {
            Debug.LogError("[Leaderboard] TextMeshProUGUI not assigned!");
            return;
        }

        // Load records
        WinRecordList recordList = LoadWinRecords();

        if (recordList == null || recordList.records.Count == 0)
        {
            leaderboardText.text = "No records yet!\n\nBe the first to complete the game!";
            Debug.Log("[Leaderboard] No records found");
            return;
        }

        // Sort by turns (ascending - fewer turns is better)
        List<WinRecord> sortedRecords = recordList.records
            .OrderBy(record => record.turns)
            .ThenBy(record => record.time) // Secondary sort by time if turns are equal
            .Take(maxEntries)
            .ToList();

        // Build leaderboard text
        string leaderboardContent = BuildLeaderboardText(sortedRecords);
        leaderboardText.text = leaderboardContent;

        Debug.Log($"[Leaderboard] Displayed {sortedRecords.Count} records");
    }

    /// <summary>
    /// Builds the formatted leaderboard text
    /// </summary>
    private string BuildLeaderboardText(List<WinRecord> records)
    {
        string text = "<size=130%><b>TOP PLAYERS</b></size>\n";

        for (int i = 0; i < records.Count; i++)
        {
            WinRecord record = records[i];

            // Medal/rank prefix
            string rankPrefix = GetRankPrefix(i);

            // Format line
            string line = $"{rankPrefix} <b>{record.playerName}</b>\n";
            line += $"    <color=#AAAAAA>Turns: {record.turns} | Time: {record.time}</color>\n";

            text += line;

            // Add spacing between entries
            if (i < records.Count - 1)
            {
                text += "";
            }
        }

        return text;
    }

    /// <summary>
    /// Gets the rank prefix with medals for top 3
    /// </summary>
    private string GetRankPrefix(int index)
    {
        switch (index)
        {
            case 0:
                return "<color=#FFD700>1st</color>"; // Gold
            case 1:
                return "<color=#C0C0C0>2nd</color>"; // Silver
            case 2:
                return "<color=#CD7F32>3rd</color>"; // Bronze
            default:
                return $"<color=#888888>{index + 1}.</color>";
        }
    }

    /// <summary>
    /// Load win records from JSON file
    /// </summary>
    private WinRecordList LoadWinRecords()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                WinRecordList recordList = JsonUtility.FromJson<WinRecordList>(json);

                if (recordList != null)
                {
                    Debug.Log($"[Leaderboard] Loaded {recordList.records.Count} records");
                    return recordList;
                }
            }
            else
            {
                Debug.Log($"[Leaderboard] No save file found at: {saveFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Leaderboard] Failed to load records: {e.Message}");
        }

        return new WinRecordList();
    }

    /// <summary>
    /// Clear the leaderboard display
    /// </summary>
    public void ClearDisplay()
    {
        if (leaderboardText != null)
        {
            leaderboardText.text = "";
        }
    }

    /// <summary>
    /// Manually refresh the leaderboard (call from a button)
    /// </summary>
    public void RefreshLeaderboard()
    {
        Debug.Log("[Leaderboard] Manual refresh requested");
        UpdateLeaderboard();
    }

    /// <summary>
    /// Get specific rank information
    /// </summary>
    public WinRecord GetRankRecord(int rank)
    {
        WinRecordList recordList = LoadWinRecords();

        if (recordList != null && recordList.records.Count > 0)
        {
            var sortedRecords = recordList.records
                .OrderBy(record => record.turns)
                .ThenBy(record => record.time)
                .ToList();

            if (rank >= 0 && rank < sortedRecords.Count)
            {
                return sortedRecords[rank];
            }
        }

        return null;
    }

    /// <summary>
    /// Get total number of wins recorded
    /// </summary>
    public int GetTotalWins()
    {
        WinRecordList recordList = LoadWinRecords();
        return recordList?.records.Count ?? 0;
    }

    /// <summary>
    /// Debug method to check setup
    /// </summary>
    public void DebugCheckSetup()
    {
        Debug.Log("===== LEADERBOARD DEBUG INFO =====");
        Debug.Log($"TextMeshProUGUI assigned: {leaderboardText != null}");
        Debug.Log($"Max entries: {maxEntries}");
        Debug.Log($"Update on start: {updateOnStart}");
        Debug.Log($"Update on enable: {updateOnEnable}");
        Debug.Log($"Save file path: {saveFilePath}");
        Debug.Log($"Save file exists: {File.Exists(saveFilePath)}");
        Debug.Log($"Total wins recorded: {GetTotalWins()}");
        Debug.Log("==================================");
    }
}