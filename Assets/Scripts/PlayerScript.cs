using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    int characterIndex;
    public GameObject spawnPoint;
    int[] otherPlayers;
    int index;
    private const string textFileName = "PlayerNames";

    [Header("UI")]
    public TurnIndicatorScript turnIndicator;

    [Header("Checkpoint System")]
    public CheckpointMovementScript checkpointMovement;

    [Header("Bot System")]
    public BotPlayerScript botPlayerScript;

    [Header("Game Manager")]
    public GameManagerScript gameManager;

    private GameObject mainPlayer;
    private List<GameObject> botPlayers = new List<GameObject>();

    void Start()
    {
        Debug.Log("[PlayerScript] ===== Start called =====");

        characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        mainPlayer = Instantiate(playerPrefabs[characterIndex], spawnPoint.transform.position, Quaternion.identity);
        mainPlayer.GetComponent<NameScript>().SetName(PlayerPrefs.GetString("PlayerName", "John Doe"));
        Debug.Log($"[PlayerScript] Main player created: {mainPlayer.name}");

        // Find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManagerScript>();
            Debug.Log($"[PlayerScript] GameManager found automatically: {gameManager != null}");
        }

        // Register main player with GameManager
        if (gameManager != null)
        {
            gameManager.RegisterMainPlayer(mainPlayer);
        }

        // Register the main character with the checkpoint system
        if (checkpointMovement != null)
        {
            Debug.Log("[PlayerScript] Registering main player with checkpoint system");
            checkpointMovement.SetPlayerCharacter(mainPlayer);

            // Subscribe to movement completion event
            checkpointMovement.OnMovementComplete += HandlePlayerMovementComplete;
            Debug.Log("[PlayerScript] Subscribed to OnMovementComplete event");
        }
        else
        {
            Debug.LogError("[PlayerScript] CheckpointMovementScript not assigned in PlayerScript!");
        }

        otherPlayers = new int[PlayerPrefs.GetInt("PlayerCount")];
        string[] nameArray = ReadLinesFromFile(textFileName);

        Debug.Log($"[PlayerScript] Creating {otherPlayers.Length - 1} bot players");

        // Spawn bot players
        for (int i = 0; i < otherPlayers.Length - 1; i++)
        {
            spawnPoint.transform.position += new Vector3(0.2f, 0, 0.08f);
            index = Random.Range(0, playerPrefabs.Length);
            GameObject otherPlayer = Instantiate(playerPrefabs[index], spawnPoint.transform.position, Quaternion.identity);
            otherPlayer.GetComponent<NameScript>().SetName(nameArray[Random.Range(0, nameArray.Length)]);

            botPlayers.Add(otherPlayer);
            Debug.Log($"[PlayerScript] Bot player #{i + 1} created: {otherPlayer.name}");
        }

        // Register all players with the bot system
        if (botPlayerScript != null)
        {
            Debug.Log("[PlayerScript] Registering all players with bot system");
            botPlayerScript.RegisterPlayers(mainPlayer, botPlayers);
            botPlayerScript.SetTurnIndicator(turnIndicator);
        }
        else
        {
            Debug.LogError("[PlayerScript] BotPlayerScript not assigned in PlayerScript!");
        }

        Debug.Log("[PlayerScript] ===== Start complete =====");

        // Show player turn
        if (turnIndicator != null && mainPlayer != null)
        {
            string playerName = mainPlayer.GetComponent<NameScript>().GetDisplayName();
            Debug.Log($"[PlayerScript] About to show turn for: {playerName}");
            turnIndicator.ShowPlayerTurn(playerName);
        }
        else
        {
            Debug.LogError("[PlayerScript] CANNOT SHOW TURN - turnIndicator or mainPlayer is NULL!");
        }
    }

    private void HandlePlayerMovementComplete()
    {
        Debug.Log("[PlayerScript] ===== HandlePlayerMovementComplete called =====");
        Debug.Log("[PlayerScript] Main player movement complete");

        // CHECK WIN CONDITION FIRST
        if (gameManager != null && botPlayerScript != null)
        {
            int playerCheckpoint = botPlayerScript.GetPlayerCheckpointIndex(mainPlayer);
            Debug.Log($"[PlayerScript] Checking win condition at checkpoint {playerCheckpoint}");
            gameManager.CheckPlayerWinCondition(playerCheckpoint);

            // If player won, don't continue to bot turns
            if (gameManager.IsGameEnded())
            {
                Debug.Log("[PlayerScript] Player won! Not triggering bot turns.");
                if (turnIndicator != null)
                {
                    turnIndicator.HidePlayerTurn();
                }
                return;
            }
        }

        // Hide turn indicator
        if (turnIndicator != null)
        {
            turnIndicator.HidePlayerTurn();
            Debug.Log("[PlayerScript] HIDED PLAYER TURN");
        }
        else
        {
            Debug.LogError("[PlayerScript] turnIndicator is NULL in HandlePlayerMovementComplete!");
        }

        // Trigger bot turns
        if (botPlayerScript != null)
        {
            Debug.Log("[PlayerScript] Calling botPlayerScript.OnPlayerTurnComplete()");
            botPlayerScript.OnPlayerTurnComplete();
        }
        else
        {
            Debug.LogError("[PlayerScript] BotPlayerScript is null, cannot trigger bot turns!");
        }
    }

    string[] ReadLinesFromFile(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);

        if (textAsset != null)
        {
            return textAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            Debug.LogWarning("File not found: " + fileName);
            return new string[0];
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (checkpointMovement != null)
        {
            checkpointMovement.OnMovementComplete -= HandlePlayerMovementComplete;
        }
    }
}