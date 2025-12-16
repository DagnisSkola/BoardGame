using UnityEngine;
using TMPro;

public class TurnIndicatorScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerTurnText; // For player turn (no panel)
    [SerializeField] private GameObject botTurnPanel; // Panel that appears for bot turns
    [SerializeField] private TextMeshProUGUI botTurnText; // Text inside the bot panel

    [Header("Display Settings")]
    [SerializeField] private bool debugMode = true;

    private void Start()
    {
        // Check if references are assigned
        if (debugMode)
        {
            Debug.Log("[TurnIndicator] === Checking References ===");
            Debug.Log($"[TurnIndicator] playerTurnText assigned: {playerTurnText != null}");
            Debug.Log($"[TurnIndicator] botTurnPanel assigned: {botTurnPanel != null}");
            Debug.Log($"[TurnIndicator] botTurnText assigned: {botTurnText != null}");
        }

        // Initialize - hide everything at start
        if (playerTurnText != null)
        {
            playerTurnText.gameObject.SetActive(false);
            if (debugMode) Debug.Log("[TurnIndicator] Player turn text hidden at start");
        }

        if (botTurnPanel != null)
        {
            botTurnPanel.SetActive(false);
            if (debugMode) Debug.Log("[TurnIndicator] Bot turn panel hidden at start");
        }
    }

    /// <summary>
    /// Show the player's turn indicator
    /// </summary>
    public void ShowPlayerTurn(string playerName)
    {
        if (debugMode)
            Debug.Log($"[TurnIndicator] ===== ShowPlayerTurn called with name: '{playerName}' =====");

        // Hide bot panel
        if (botTurnPanel != null)
        {
            botTurnPanel.SetActive(false);
            if (debugMode) Debug.Log("[TurnIndicator] Bot panel hidden");
        }
        else if (debugMode)
        {
            Debug.LogWarning("[TurnIndicator] botTurnPanel is NULL!");
        }

        // Show player turn text
        if (playerTurnText != null)
        {
            playerTurnText.text = $"{playerName} Turn";
            playerTurnText.gameObject.SetActive(true);

            if (debugMode)
            {
                Debug.Log($"[TurnIndicator] Player turn text set to: '{playerTurnText.text}'");
                Debug.Log($"[TurnIndicator] Player turn text active: {playerTurnText.gameObject.activeSelf}");
                Debug.Log($"[TurnIndicator] Player turn text enabled: {playerTurnText.enabled}");
            }

            // Cancel any pending auto-hide (keep it visible)
            CancelInvoke(nameof(HidePlayerTurn));
        }
        else if (debugMode)
        {
            Debug.LogWarning("[TurnIndicator] playerTurnText is NULL!");
        }
    }

    /// <summary>
    /// Show the bot's turn indicator with panel
    /// </summary>
    public void ShowBotTurn(string botName)
    {
        if (debugMode)
            Debug.Log($"[TurnIndicator] ===== ShowBotTurn called with name: '{botName}' =====");

        // Hide player turn text
        if (playerTurnText != null)
        {
            playerTurnText.gameObject.SetActive(false);
            if (debugMode) Debug.Log("[TurnIndicator] Player turn text hidden");
        }
        else if (debugMode)
        {
            Debug.LogWarning("[TurnIndicator] playerTurnText is NULL!");
        }

        // Show bot panel and text
        if (botTurnPanel != null && botTurnText != null)
        {
            botTurnText.text = $"{botName} Turn";
            botTurnPanel.SetActive(true);

            if (debugMode)
            {
                Debug.Log($"[TurnIndicator] Bot turn text set to: '{botTurnText.text}'");
                Debug.Log($"[TurnIndicator] Bot panel active: {botTurnPanel.activeSelf}");
                Debug.Log($"[TurnIndicator] Bot text enabled: {botTurnText.enabled}");
            }

            // Keep bot turn visible (don't auto-hide)
            CancelInvoke(nameof(HideBotTurn));
        }
        else if (debugMode)
        {
            if (botTurnPanel == null) Debug.LogWarning("[TurnIndicator] botTurnPanel is NULL!");
            if (botTurnText == null) Debug.LogWarning("[TurnIndicator] botTurnText is NULL!");
        }
    }

    /// <summary>
    /// Hide player turn indicator
    /// </summary>
    public void HidePlayerTurn()
    {
        if (debugMode) Debug.Log("[TurnIndicator] HidePlayerTurn called");

        if (playerTurnText != null)
        {
            playerTurnText.gameObject.SetActive(false);
            if (debugMode) Debug.Log("[TurnIndicator] Player turn text hidden");
        }
    }

    /// <summary>
    /// Hide bot turn indicator
    /// </summary>
    public void HideBotTurn()
    {
        if (debugMode) Debug.Log("[TurnIndicator] HideBotTurn called");

        if (botTurnPanel != null)
        {
            botTurnPanel.SetActive(false);
            if (debugMode) Debug.Log("[TurnIndicator] Bot panel hidden");
        }
    }

    /// <summary>
    /// Hide all turn indicators
    /// </summary>
    public void HideAllIndicators()
    {
        if (debugMode) Debug.Log("[TurnIndicator] HideAllIndicators called");
        HidePlayerTurn();
        HideBotTurn();
    }

    // Test methods you can call from inspector or other scripts
    public void TestShowPlayer()
    {
        ShowPlayerTurn("Test Player");
    }

    public void TestShowBot()
    {
        ShowBotTurn("Test Bot");
    }

    private void OnDestroy()
    {
        // Clean up any pending invokes
        CancelInvoke();
    }
}