using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

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
    [SerializeField] private string menuSceneName = "MainMenu"; // Name of your menu scene
    [SerializeField] private int finalCheckpointIndex = 119; // Set this to your last checkpoint

    // Game stats tracking
    private float gameStartTime;
    private int playerTurnCount = 0;
    private int totalTurnCount = 0;
    private bool gameEnded = false;

    private GameObject mainPlayer;

    void Start()
    {
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

    /// <summary>
    /// Call this when the player completes their turn
    /// </summary>
    public void OnPlayerTurnComplete()
    {
        if (gameEnded) return;

        playerTurnCount++;
        totalTurnCount++;
        Debug.Log($"[GameManager] Player turn complete. Player turns: {playerTurnCount}, Total turns: {totalTurnCount}");
    }

    /// <summary>
    /// Call this when a bot completes their turn
    /// </summary>
    public void OnBotTurnComplete()
    {
        if (gameEnded) return;

        totalTurnCount++;
        Debug.Log($"[GameManager] Bot turn complete. Total turns: {totalTurnCount}");
    }

    /// <summary>
    /// Check if player has reached the final checkpoint
    /// </summary>
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

    /// <summary>
    /// Check if a bot has reached the final checkpoint
    /// </summary>
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

    // Update the GoToMenu method
    /// <summary>
    /// Call this from the menu button
    /// </summary>
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

    /// <summary>
    /// Restart the current game
    /// </summary>
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
    /// Manual test method - call this to test win screen
    /// </summary>
    public void TestWinScreen()
    {
        Debug.Log("[GameManager] TESTING WIN SCREEN");
        PlayerWins();
    }

    /// <summary>
    /// Manual test method - call this to test lose screen
    /// </summary>
    public void TestLoseScreen()
    {
        Debug.Log("[GameManager] TESTING LOSE SCREEN");
        PlayerLoses("Test Bot");
    }

    /// <summary>
    /// Debug method to check current setup
    /// </summary>
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
        Debug.Log("==================================");
    }
}