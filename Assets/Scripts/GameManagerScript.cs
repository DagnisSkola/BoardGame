using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class WinRecord
{
    public string playerName;
    public int turns;
    public string time;
    public string date;
}

[System.Serializable]
public class WinRecordList
{
    public List<WinRecord> records = new List<WinRecord>();
}

public class GameManagerScript : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Win Panel Elements")]
    [SerializeField] private TextMeshProUGUI winTitleText;
    [SerializeField] private TextMeshProUGUI winTimeText;
    [SerializeField] private TextMeshProUGUI winTurnsText;

    [Header("Lose Panel Elements")]
    [SerializeField] private TextMeshProUGUI loseTitleText;
    [SerializeField] private TextMeshProUGUI loseTimeText;
    [SerializeField] private TextMeshProUGUI loseTurnsText;
    [SerializeField] private TextMeshProUGUI loseWinnerText;

    [Header("References")]
    [SerializeField] private CheckpointMovementScript checkpointMovement;
    [SerializeField] private BotPlayerScript botPlayerScript;
    [SerializeField] private SceneChanger sceneChanger;

    [Header("Settings")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private int finalCheckpointIndex = 119;
    [SerializeField] private string defaultPlayerName = "Player"; // Default name if none set

    // Game stats tracking
    private float gameStartTime;
    private int playerTurnCount = 0;
    private int totalTurnCount = 0;
    private bool gameEnded = false;

    private GameObject mainPlayer;
    private string saveFilePath;

    void Start()
    {
        // Set up save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, "win_records.json");
        Debug.Log($"[GameManager] Save file path: {saveFilePath}");

        // Hide panels at start
        if (winPanel != null)
        {
            winPanel.SetActive(false);
            Debug.Log("[GameManager] Win panel hidden at start");
        }
        else
        {
            Debug.LogError("[GameManager] WIN PANEL NOT ASSIGNED!");
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
            Debug.Log("[GameManager] Lose panel hidden at start");
        }
        else
        {
            Debug.LogError("[GameManager] LOSE PANEL NOT ASSIGNED!");
        }

        // Record game start time
        gameStartTime = Time.time;

        Debug.Log($"[GameManager] Game started - Final checkpoint index: {finalCheckpointIndex}");
        Debug.Log($"[GameManager] Menu scene name: {menuSceneName}");
    }

    public void RegisterMainPlayer(GameObject player)
    {
        mainPlayer = player;
        Debug.Log($"[GameManager] Main player registered: {player.name}");
    }

    public void OnPlayerTurnComplete()
    {
        if (gameEnded) return;

        playerTurnCount++;
        totalTurnCount++;
        Debug.Log($"[GameManager] Player turn complete. Player turns: {playerTurnCount}, Total turns: {totalTurnCount}");
    }

    public void OnBotTurnComplete()
    {
        if (gameEnded) return;

        totalTurnCount++;
        Debug.Log($"[GameManager] Bot turn complete. Total turns: {totalTurnCount}");
    }

    public void CheckPlayerWinCondition(int currentCheckpoint)
    {
        Debug.Log($"[GameManager] CheckPlayerWinCondition called - Current: {currentCheckpoint}, Final: {finalCheckpointIndex}, GameEnded: {gameEnded}");

        if (gameEnded)
        {
            Debug.Log("[GameManager] Game already ended, ignoring win check");
            return;
        }

        if (currentCheckpoint >= finalCheckpointIndex)
        {
            Debug.Log("[GameManager] *** PLAYER WINS! ***");
            PlayerWins();
        }
        else
        {
            Debug.Log($"[GameManager] Player not at final checkpoint yet ({currentCheckpoint}/{finalCheckpointIndex})");
        }
    }

    public void CheckBotWinCondition(GameObject bot, int currentCheckpoint)
    {
        Debug.Log($"[GameManager] CheckBotWinCondition called - Bot: {bot.name}, Current: {currentCheckpoint}, Final: {finalCheckpointIndex}, GameEnded: {gameEnded}");

        if (gameEnded)
        {
            Debug.Log("[GameManager] Game already ended, ignoring bot win check");
            return;
        }

        if (currentCheckpoint >= finalCheckpointIndex)
        {
            Debug.Log($"[GameManager] *** BOT WINS: {bot.name} ***");
            string botName = bot.GetComponent<NameScript>()?.GetDisplayName() ?? "Bot";
            PlayerLoses(botName);
        }
        else
        {
            Debug.Log($"[GameManager] Bot not at final checkpoint yet ({currentCheckpoint}/{finalCheckpointIndex})");
        }
    }

    private void PlayerWins()
    {
        gameEnded = true;
        float gameDuration = Time.time - gameStartTime;

        // Format time
        string timeString = FormatTime(gameDuration);

        // Get player name
        string playerName = defaultPlayerName;
        if (mainPlayer != null)
        {
            NameScript nameScript = mainPlayer.GetComponent<NameScript>();
            if (nameScript != null)
            {
                playerName = nameScript.GetDisplayName();
            }
        }

        // Save the win record
        SaveWinRecord(playerName, playerTurnCount, timeString);

        // Update win panel
        if (winPanel != null)
        {
            if (winTitleText != null)
                winTitleText.text = "VICTORY!";

            if (winTimeText != null)
                winTimeText.text = $"Time: {timeString}";

            if (winTurnsText != null)
                winTurnsText.text = $"Turns: {playerTurnCount}";

            winPanel.SetActive(true);
            Debug.Log($"[GameManager] Win panel shown - Time: {timeString}, Turns: {playerTurnCount}");
        }
        else
        {
            Debug.LogError("[GameManager] Win panel not assigned!");
        }

        // Pause the game
        Time.timeScale = 0f;
    }

    private void SaveWinRecord(string playerName, int turns, string time)
    {
        try
        {
            // Load existing records or create new list
            WinRecordList recordList;

            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                recordList = JsonUtility.FromJson<WinRecordList>(json);
                if (recordList == null)
                {
                    recordList = new WinRecordList();
                }
            }
            else
            {
                recordList = new WinRecordList();
            }

            // Create new record
            WinRecord newRecord = new WinRecord
            {
                playerName = playerName,
                turns = turns,
                time = time,
                date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Add to list
            recordList.records.Add(newRecord);

            // Save to file
            string jsonToSave = JsonUtility.ToJson(recordList, true);
            File.WriteAllText(saveFilePath, jsonToSave);

            Debug.Log($"[GameManager] Win record saved! Player: {playerName}, Turns: {turns}, Time: {time}");
            Debug.Log($"[GameManager] Total records: {recordList.records.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Failed to save win record: {e.Message}");
        }
    }

    private void PlayerLoses(string winnerName)
    {
        gameEnded = true;
        float gameDuration = Time.time - gameStartTime;

        // Format time
        string timeString = FormatTime(gameDuration);

        // Update lose panel
        if (losePanel != null)
        {
            if (loseTitleText != null)
                loseTitleText.text = "DEFEAT!";

            if (loseWinnerText != null)
                loseWinnerText.text = $"{winnerName} Won!";

            if (loseTimeText != null)
                loseTimeText.text = $"Game Duration: {timeString}";

            if (loseTurnsText != null)
                loseTurnsText.text = $"Total Turns: {totalTurnCount}";

            losePanel.SetActive(true);
            Debug.Log($"[GameManager] Lose panel shown - Winner: {winnerName}, Time: {timeString}, Turns: {totalTurnCount}");
        }
        else
        {
            Debug.LogError("[GameManager] Lose panel not assigned!");
        }

        // Pause the game
        Time.timeScale = 0f;
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public void GoToMenu()
    {
        Debug.Log("[GameManager] Going to menu...");

        // Resume time before loading scene
        Time.timeScale = 1f;

        // Use SceneChanger if available for fade effect
        if (sceneChanger != null)
        {
            sceneChanger.GoToMenu();
        }
        else
        {
            // Fallback to direct scene loading
            SceneManager.LoadScene(menuSceneName);
        }
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] Restarting game...");

        // Resume time before reloading
        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Public getters for other scripts
    public int GetPlayerTurnCount() => playerTurnCount;
    public int GetTotalTurnCount() => totalTurnCount;
    public float GetGameDuration() => Time.time - gameStartTime;
    public bool IsGameEnded() => gameEnded;

    /// <summary>
    /// Load all win records from file
    /// </summary>
    public WinRecordList LoadWinRecords()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                return JsonUtility.FromJson<WinRecordList>(json);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Failed to load win records: {e.Message}");
        }

        return new WinRecordList();
    }

    /// <summary>
    /// Clear all win records
    /// </summary>
    public void ClearWinRecords()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.Log("[GameManager] Win records cleared!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Failed to clear win records: {e.Message}");
        }
    }

    public void TestWinScreen()
    {
        Debug.Log("[GameManager] TESTING WIN SCREEN");
        PlayerWins();
    }

    public void TestLoseScreen()
    {
        Debug.Log("[GameManager] TESTING LOSE SCREEN");
        PlayerLoses("Test Bot");
    }

    public void DebugCheckSetup()
    {
        Debug.Log("===== GAMEMANAGER DEBUG INFO =====");
        Debug.Log($"Win Panel assigned: {winPanel != null}");
        Debug.Log($"Lose Panel assigned: {losePanel != null}");
        Debug.Log($"Win Title Text assigned: {winTitleText != null}");
        Debug.Log($"Win Time Text assigned: {winTimeText != null}");
        Debug.Log($"Win Turns Text assigned: {winTurnsText != null}");
        Debug.Log($"Lose Title Text assigned: {loseTitleText != null}");
        Debug.Log($"Lose Time Text assigned: {loseTimeText != null}");
        Debug.Log($"Lose Turns Text assigned: {loseTurnsText != null}");
        Debug.Log($"Lose Winner Text assigned: {loseWinnerText != null}");
        Debug.Log($"CheckpointMovement assigned: {checkpointMovement != null}");
        Debug.Log($"BotPlayerScript assigned: {botPlayerScript != null}");
        Debug.Log($"Final Checkpoint Index: {finalCheckpointIndex}");
        Debug.Log($"Menu Scene Name: {menuSceneName}");
        Debug.Log($"Game Ended: {gameEnded}");
        Debug.Log($"Save File Path: {saveFilePath}");
        Debug.Log("==================================");
    }
}