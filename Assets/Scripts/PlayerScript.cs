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

    [Header("Checkpoint System")]
    public CheckpointMovementScript checkpointMovement;

    [Header("Bot System")]
    public BotPlayerScript botPlayerScript;

    private GameObject mainPlayer;
    private List<GameObject> botPlayers = new List<GameObject>();

    void Start()
    {
        Debug.Log("[PlayerScript] ===== Start called =====");

        characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        mainPlayer = Instantiate(playerPrefabs[characterIndex], spawnPoint.transform.position, Quaternion.identity);
        mainPlayer.GetComponent<NameScript>().SetName(PlayerPrefs.GetString("PlayerName", "John Doe"));
        Debug.Log($"[PlayerScript] Main player created: {mainPlayer.name}");

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
        }
        else
        {
            Debug.LogError("[PlayerScript] BotPlayerScript not assigned in PlayerScript!");
        }

        Debug.Log("[PlayerScript] ===== Start complete =====");
    }

    private void HandlePlayerMovementComplete()
    {
        Debug.Log("[PlayerScript] ===== HandlePlayerMovementComplete called =====");
        Debug.Log("[PlayerScript] Main player movement complete, triggering bot turns");

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