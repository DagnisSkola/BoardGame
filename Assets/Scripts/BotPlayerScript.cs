using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotPlayerScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DiceRollScript diceRollScript;
    [SerializeField] private CameraFollowScript cameraFollow;
    [SerializeField] private Transform checkpointParent;
    [SerializeField] private CheckpointMovementScript checkpointMovement; // Use existing rules

    [Header("Bot Settings")]
    [SerializeField] private float botTurnDelay = 2f; // Delay before bot takes their turn
    [SerializeField] private float diceWaitTime = 3f; // Time to wait for dice to settle
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float heightOffset = 0.5f;

    private List<GameObject> allPlayers = new List<GameObject>();
    private int currentPlayerIndex = 0;
    private Transform[] checkpoints;
    private Dictionary<GameObject, int> playerCheckpointIndices = new Dictionary<GameObject, int>();
    private bool isProcessingTurn = false;

    void Start()
    {
        // Find references if not assigned
        if (diceRollScript == null)
        {
            diceRollScript = FindFirstObjectByType<DiceRollScript>();
        }

        if (cameraFollow == null)
        {
            cameraFollow = FindFirstObjectByType<CameraFollowScript>();
        }

        if (checkpointMovement == null)
        {
            checkpointMovement = FindFirstObjectByType<CheckpointMovementScript>();
            Debug.Log("[BotPlayer] Found CheckpointMovementScript automatically");
        }

        // Load checkpoints
        if (checkpointParent != null)
        {
            PopulateCheckpoints();
        }
        else
        {
            Debug.LogWarning("[BotPlayer] Checkpoint Parent not assigned!");
        }
    }

    private void PopulateCheckpoints()
    {
        List<Transform> checkpointList = new List<Transform>();

        foreach (Transform child in checkpointParent)
        {
            checkpointList.Add(child);
        }

        checkpointList.Sort((a, b) =>
        {
            int numA = ExtractNumberFromName(a.name);
            int numB = ExtractNumberFromName(b.name);
            return numA.CompareTo(numB);
        });

        checkpoints = checkpointList.ToArray();
        Debug.Log($"[BotPlayer] Loaded {checkpoints.Length} checkpoints");
    }

    private int ExtractNumberFromName(string name)
    {
        string numberPart = "";
        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsDigit(name[i]))
            {
                numberPart += name[i];
            }
        }
        return int.TryParse(numberPart, out int number) ? number : 0;
    }

    // Call this from PlayerScript to register all players
    public void RegisterPlayers(GameObject mainPlayer, List<GameObject> botPlayers)
    {
        Debug.Log("[BotPlayer] ===== RegisterPlayers called =====");
        allPlayers.Clear();
        playerCheckpointIndices.Clear();

        // Add main player first
        allPlayers.Add(mainPlayer);
        playerCheckpointIndices[mainPlayer] = 0;
        Debug.Log($"[BotPlayer] Registered main player: {mainPlayer.name} at index 0");

        // Add bot players
        for (int i = 0; i < botPlayers.Count; i++)
        {
            GameObject bot = botPlayers[i];
            allPlayers.Add(bot);
            playerCheckpointIndices[bot] = 0;
            Debug.Log($"[BotPlayer] Registered bot #{i + 1}: {bot.name} at index {i + 1}");
        }

        Debug.Log($"[BotPlayer] ===== Total players registered: {allPlayers.Count} =====");
        Debug.Log($"[BotPlayer] Player list: {string.Join(", ", allPlayers.ConvertAll(p => p.name))}");
    }

    // Call this when the main player finishes their turn
    public void OnPlayerTurnComplete()
    {
        Debug.Log($"[BotPlayer] OnPlayerTurnComplete called. isProcessingTurn: {isProcessingTurn}, allPlayers.Count: {allPlayers.Count}");

        if (!isProcessingTurn && allPlayers.Count > 1)
        {
            Debug.Log("[BotPlayer] Player turn complete, starting bot turns");
            StartCoroutine(ProcessBotTurns());
        }
        else if (isProcessingTurn)
        {
            Debug.LogWarning("[BotPlayer] Already processing a turn!");
        }
        else if (allPlayers.Count <= 1)
        {
            Debug.LogWarning("[BotPlayer] No bot players registered!");
        }
    }

    private IEnumerator ProcessBotTurns()
    {
        Debug.Log($"[BotPlayer] ===== ProcessBotTurns started =====");
        Debug.Log($"[BotPlayer] Total players: {allPlayers.Count}");
        isProcessingTurn = true;

        // Process all bot players (skip index 0 which is the main player)
        for (int i = 1; i < allPlayers.Count; i++)
        {
            currentPlayerIndex = i;
            GameObject currentBot = allPlayers[i];

            Debug.Log($"[BotPlayer] ===== Processing bot at index {i}: {currentBot.name} =====");

            // Wait before bot's turn
            Debug.Log($"[BotPlayer] Waiting {botTurnDelay} seconds before {currentBot.name}'s turn");
            yield return new WaitForSeconds(botTurnDelay);

            // Roll the physical dice
            int diceRoll = 0;
            if (diceRollScript != null)
            {
                Debug.Log($"[BotPlayer] {currentBot.name} is rolling the dice...");

                // Reset and roll the dice
                diceRollScript.ResetDice();
                yield return new WaitForSeconds(0.2f); // Small delay after reset

                // Trigger the dice roll (simulate the roll)
                diceRollScript.GetComponent<Rigidbody>().isKinematic = false;
                float forceX = Random.Range(0, 500);
                float forceY = Random.Range(0, 500);
                float forceZ = Random.Range(0, 500);
                diceRollScript.GetComponent<Rigidbody>().AddForce(Vector3.up * Random.Range(800, 1200));
                diceRollScript.GetComponent<Rigidbody>().AddTorque(forceX, forceY, forceZ);

                // Wait for dice to land
                Debug.Log($"[BotPlayer] Waiting for dice to settle...");
                float elapsed = 0f;
                while (elapsed < diceWaitTime && !diceRollScript.isLanded)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Get the result
                if (diceRollScript.isLanded)
                {
                    if (int.TryParse(diceRollScript.diceFaceNum, out diceRoll))
                    {
                        Debug.Log($"[BotPlayer] {currentBot.name} rolled: {diceRoll}");
                    }
                    else
                    {
                        Debug.LogWarning($"[BotPlayer] Could not parse dice result: {diceRollScript.diceFaceNum}, using random");
                        diceRoll = Random.Range(1, 7);
                    }
                }
                else
                {
                    Debug.LogWarning($"[BotPlayer] Dice didn't land in time, using random number");
                    diceRoll = Random.Range(1, 7);
                }
            }
            else
            {
                Debug.LogWarning("[BotPlayer] DiceRollScript is null, using random number");
                diceRoll = Random.Range(1, 7);
            }

            // Tell camera to follow this bot
            if (cameraFollow != null)
            {
                Debug.Log($"[BotPlayer] Setting camera to follow {currentBot.name}");
                cameraFollow.SetPlayerToFollow(currentBot);
                cameraFollow.StartFollowing();
            }
            else
            {
                Debug.LogWarning("[BotPlayer] Camera follow is null!");
            }

            // Move the bot
            Debug.Log($"[BotPlayer] Starting movement for {currentBot.name}");
            yield return StartCoroutine(MoveBot(currentBot, diceRoll));
            Debug.Log($"[BotPlayer] Movement complete for {currentBot.name}");

            // Check for special rules
            Debug.Log($"[BotPlayer] Checking special rules for {currentBot.name}");
            yield return StartCoroutine(CheckSpecialRules(currentBot));

            // Tell camera to stop following
            if (cameraFollow != null)
            {
                Debug.Log($"[BotPlayer] Camera stop following {currentBot.name}");
                cameraFollow.StopFollowing();
            }

            // Small delay between bot turns
            Debug.Log($"[BotPlayer] Turn complete for {currentBot.name}, waiting 0.5s");
            yield return new WaitForSeconds(0.5f);
        }

        isProcessingTurn = false;
        Debug.Log("[BotPlayer] ===== All bot turns complete =====");

        // Reset to main player
        if (allPlayers.Count > 0 && cameraFollow != null)
        {
            Debug.Log($"[BotPlayer] Resetting camera to main player: {allPlayers[0].name}");
            cameraFollow.SetPlayerToFollow(allPlayers[0]);
        }
    }

    private IEnumerator MoveBot(GameObject bot, int steps)
    {
        Debug.Log($"[BotPlayer] MoveBot called for {bot.name} with {steps} steps");

        if (!playerCheckpointIndices.ContainsKey(bot))
        {
            Debug.LogError($"[BotPlayer] Bot {bot.name} not found in checkpoint indices!");
            yield break;
        }

        int currentCheckpointIndex = playerCheckpointIndices[bot];
        Debug.Log($"[BotPlayer] {bot.name} starting at checkpoint {currentCheckpointIndex}");

        for (int i = 0; i < steps; i++)
        {
            int targetIndex = currentCheckpointIndex + 1;
            Debug.Log($"[BotPlayer] {bot.name} moving step {i + 1}/{steps} to checkpoint {targetIndex}");

            if (targetIndex >= checkpoints.Length)
            {
                Debug.Log($"[BotPlayer] {bot.name} reached the final checkpoint!");
                break;
            }

            yield return StartCoroutine(MoveToCheckpoint(bot, checkpoints[targetIndex]));
            currentCheckpointIndex = targetIndex;
            playerCheckpointIndices[bot] = currentCheckpointIndex;
            Debug.Log($"[BotPlayer] {bot.name} now at checkpoint {currentCheckpointIndex}");

            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log($"[BotPlayer] MoveBot complete for {bot.name}");
    }

    private IEnumerator MoveToCheckpoint(GameObject bot, Transform targetCheckpoint)
    {
        Vector3 startPos = bot.transform.position;
        Vector3 endPos = targetCheckpoint.position;
        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smooth step

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            float arc = heightOffset * Mathf.Sin(t * Mathf.PI);
            currentPos.y += arc;

            bot.transform.position = currentPos;

            // Rotate to face movement direction
            if (endPos != startPos)
            {
                Vector3 direction = (endPos - startPos).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                bot.transform.rotation = Quaternion.Slerp(
                    bot.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        bot.transform.position = endPos;
    }

    private IEnumerator CheckSpecialRules(GameObject bot)
    {
        if (!playerCheckpointIndices.ContainsKey(bot))
            yield break;

        int currentCheckpointIndex = playerCheckpointIndices[bot];

        // Use the special rules from CheckpointMovementScript if available
        if (checkpointMovement != null)
        {
            var specialRules = checkpointMovement.GetSpecialRules();
            var applicableRule = specialRules.Find(rule => rule.fromCheckpoint == currentCheckpointIndex);

            if (applicableRule != null)
            {
                if (!string.IsNullOrEmpty(applicableRule.message))
                {
                    Debug.Log($"[BotPlayer] {bot.name}: {applicableRule.message}");
                }

                yield return new WaitForSeconds(0.5f);

                if (applicableRule.toCheckpoint >= 0 && applicableRule.toCheckpoint < checkpoints.Length)
                {
                    if (applicableRule.movementType == CheckpointMovementScript.MovementType.Teleport)
                    {
                        bot.transform.position = checkpoints[applicableRule.toCheckpoint].position;
                        playerCheckpointIndices[bot] = applicableRule.toCheckpoint;
                        Debug.Log($"[BotPlayer] {bot.name} teleported to checkpoint {applicableRule.toCheckpoint}");
                    }
                    else if (applicableRule.movementType == CheckpointMovementScript.MovementType.SmoothMove)
                    {
                        yield return StartCoroutine(MoveToCheckpoint(bot, checkpoints[applicableRule.toCheckpoint]));
                        playerCheckpointIndices[bot] = applicableRule.toCheckpoint;
                        Debug.Log($"[BotPlayer] {bot.name} moved to checkpoint {applicableRule.toCheckpoint}");
                    }
                }
            }
        }
    }

    // Public method to get current checkpoint index for a player
    public int GetPlayerCheckpointIndex(GameObject player)
    {
        if (playerCheckpointIndices.ContainsKey(player))
        {
            return playerCheckpointIndices[player];
        }
        return 0;
    }

    // Public method to manually trigger a bot turn (for testing)
    public void TriggerBotTurns()
    {
        if (!isProcessingTurn)
        {
            StartCoroutine(ProcessBotTurns());
        }
    }
}