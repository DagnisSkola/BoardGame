using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotPlayerScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DiceRollScript diceRollScript;
    [SerializeField] private CameraFollowScript cameraFollow;
    [SerializeField] private Transform checkpointParent;
    [SerializeField] private CheckpointMovementScript checkpointMovement;
    [SerializeField] private GameManagerScript gameManager;

    [Header("Bot Settings")]
    [SerializeField] private float botTurnDelay = 2f;
    [SerializeField] private float diceWaitTime = 4f;
    [SerializeField] private float minRollTime = 0.5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private bool requireExactLanding = true; // NEW: Enable bounce-back for bots

    private List<GameObject> allPlayers = new List<GameObject>();
    private int currentPlayerIndex = 0;
    private Transform[] checkpoints;
    private Dictionary<GameObject, int> playerCheckpointIndices = new Dictionary<GameObject, int>();
    private bool isProcessingTurn = false;

    private TurnIndicatorScript turnIndicator;

    void Start()
    {
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

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManagerScript>();
            Debug.Log("[BotPlayer] Found GameManagerScript automatically");
        }

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

    public void RegisterPlayers(GameObject mainPlayer, List<GameObject> botPlayers)
    {
        Debug.Log("[BotPlayer] ===== RegisterPlayers called =====");
        allPlayers.Clear();
        playerCheckpointIndices.Clear();

        allPlayers.Add(mainPlayer);
        playerCheckpointIndices[mainPlayer] = 0;
        Debug.Log($"[BotPlayer] Registered main player: {mainPlayer.name} at index 0");

        for (int i = 0; i < botPlayers.Count; i++)
        {
            GameObject bot = botPlayers[i];
            allPlayers.Add(bot);
            playerCheckpointIndices[bot] = 0;
            Debug.Log($"[BotPlayer] Registered bot #{i + 1}: {bot.name} at index {i + 1}");
        }

        Debug.Log($"[BotPlayer] ===== Total players registered: {allPlayers.Count} =====");
    }

    public void OnPlayerTurnComplete()
    {
        Debug.Log($"[BotPlayer] OnPlayerTurnComplete called. isProcessingTurn: {isProcessingTurn}, allPlayers.Count: {allPlayers.Count}");

        // Check if game has ended
        if (gameManager != null && gameManager.IsGameEnded())
        {
            Debug.Log("[BotPlayer] Game has ended, skipping bot turns");
            return;
        }

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
        isProcessingTurn = true;

        if (checkpointMovement != null)
        {
            checkpointMovement.SetPlayerControlEnabled(false);
        }

        for (int i = 1; i < allPlayers.Count; i++)
        {
            // Check if game ended before processing this bot
            if (gameManager != null && gameManager.IsGameEnded())
            {
                Debug.Log("[BotPlayer] Game ended during bot turns, stopping");
                break;
            }

            currentPlayerIndex = i;
            GameObject currentBot = allPlayers[i];

            if (turnIndicator != null)
            {
                string botName = currentBot.GetComponent<NameScript>().GetDisplayName();
                turnIndicator.ShowBotTurn(botName);
            }

            Debug.Log($"[BotPlayer] ===== Processing bot at index {i}: {currentBot.name} =====");

            yield return new WaitForSeconds(botTurnDelay);

            int diceRoll = 0;
            if (diceRollScript != null)
            {
                Debug.Log($"[BotPlayer] {currentBot.name} is rolling the dice...");

                if (checkpointMovement != null)
                {
                    checkpointMovement.IgnoreNextDiceLanding();
                }

                diceRollScript.ResetDice();
                yield return new WaitForSeconds(0.3f);

                Rigidbody diceRB = diceRollScript.GetComponent<Rigidbody>();
                diceRB.isKinematic = false;

                diceRollScript.transform.rotation = new Quaternion(
                    Random.Range(0, 360),
                    Random.Range(0, 360),
                    Random.Range(0, 360),
                    0
                );

                float forceX = Random.Range(0, 500);
                float forceY = Random.Range(0, 500);
                float forceZ = Random.Range(0, 500);
                diceRB.AddForce(Vector3.up * Random.Range(800, 1200));
                diceRB.AddTorque(forceX, forceY, forceZ);

                yield return new WaitForSeconds(minRollTime);

                float elapsed = minRollTime;
                bool diceHasLanded = false;

                while (elapsed < diceWaitTime)
                {
                    if (diceRollScript.isLanded)
                    {
                        diceHasLanded = true;
                        break;
                    }
                    yield return null;
                    elapsed += Time.deltaTime;
                }

                if (diceHasLanded && diceRollScript.isLanded)
                {
                    if (int.TryParse(diceRollScript.diceFaceNum, out diceRoll))
                    {
                        Debug.Log($"[BotPlayer] *** {currentBot.name} rolled: {diceRoll} ***");
                    }
                    else
                    {
                        diceRoll = Random.Range(1, 7);
                        Debug.Log($"[BotPlayer] *** {currentBot.name} random roll: {diceRoll} ***");
                    }
                }
                else
                {
                    diceRoll = Random.Range(1, 7);
                    Debug.Log($"[BotPlayer] *** {currentBot.name} random roll: {diceRoll} ***");
                }

                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                diceRoll = Random.Range(1, 7);
                Debug.Log($"[BotPlayer] *** {currentBot.name} random roll: {diceRoll} ***");
            }

            if (diceRoll <= 0)
            {
                diceRoll = 1;
            }

            if (cameraFollow != null)
            {
                cameraFollow.SetPlayerToFollow(currentBot);
                cameraFollow.StartFollowing();
            }

            yield return StartCoroutine(MoveBot(currentBot, diceRoll));

            // Check win condition after bot movement
            if (gameManager != null && playerCheckpointIndices.ContainsKey(currentBot))
            {
                int botCheckpoint = playerCheckpointIndices[currentBot];
                gameManager.CheckBotWinCondition(currentBot, botCheckpoint);

                // If bot won, break the loop
                if (gameManager.IsGameEnded())
                {
                    Debug.Log("[BotPlayer] Bot won the game!");
                    break;
                }
            }

            // Notify GameManager that bot turn is complete
            if (gameManager != null)
            {
                gameManager.OnBotTurnComplete();
            }

            yield return StartCoroutine(CheckSpecialRules(currentBot));

            if (cameraFollow != null)
            {
                cameraFollow.StopFollowing();
            }

            yield return new WaitForSeconds(0.5f);
        }

        if (checkpointMovement != null)
        {
            checkpointMovement.SetPlayerControlEnabled(true);
        }

        isProcessingTurn = false;
        Debug.Log("[BotPlayer] ===== All bot turns complete =====");

        if (allPlayers.Count > 0 && cameraFollow != null)
        {
            cameraFollow.SetPlayerToFollow(allPlayers[0]);
        }

        // Only show player turn again if game hasn't ended
        if (gameManager != null && !gameManager.IsGameEnded())
        {
            if (turnIndicator != null && allPlayers.Count > 0)
            {
                GameObject mainPlayer = allPlayers[0];
                string playerName = mainPlayer.GetComponent<NameScript>().GetDisplayName();
                turnIndicator.ShowPlayerTurn(playerName);
            }
        }
    }

    // Replace the MoveBot method with this updated version
    private IEnumerator MoveBot(GameObject bot, int steps)
    {
        if (bot == null || checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogError("[BotPlayer] Invalid bot or checkpoints!");
            yield break;
        }

        if (!playerCheckpointIndices.ContainsKey(bot))
        {
            Debug.LogError($"[BotPlayer] Bot {bot.name} not found in checkpoint indices!");
            yield break;
        }

        int currentCheckpointIndex = playerCheckpointIndices[bot];
        int remainingSteps = steps;
        int stepsToFinal = checkpoints.Length - 1 - currentCheckpointIndex;

        // Check if we need exact landing
        if (requireExactLanding && remainingSteps > stepsToFinal && stepsToFinal > 0)
        {
            Debug.Log($"[BotPlayer] {bot.name} needs exact landing! Rolled {remainingSteps}, need {stepsToFinal}");

            // Move forward to the final checkpoint
            for (int i = 0; i < stepsToFinal; i++)
            {
                int targetIndex = currentCheckpointIndex + 1;

                if (targetIndex >= checkpoints.Length)
                {
                    break;
                }

                if (checkpoints[targetIndex] == null)
                {
                    Debug.LogError($"[BotPlayer] Checkpoint at index {targetIndex} is NULL!");
                    break;
                }

                yield return StartCoroutine(MoveToCheckpoint(bot, checkpoints[targetIndex]));
                currentCheckpointIndex = targetIndex;
                playerCheckpointIndices[bot] = currentCheckpointIndex;

                // Check win condition after each step
                if (gameManager != null)
                {
                    gameManager.CheckBotWinCondition(bot, currentCheckpointIndex);
                    if (gameManager.IsGameEnded())
                    {
                        Debug.Log($"[BotPlayer] {bot.name} won! Stopping movement.");
                        yield break;
                    }
                }

                yield return new WaitForSeconds(0.2f);
            }

            // Calculate bounce-back steps
            int bounceBackSteps = remainingSteps - stepsToFinal;
            Debug.Log($"[BotPlayer] {bot.name} reached final checkpoint! Bouncing back {bounceBackSteps} steps");

            yield return new WaitForSeconds(0.3f); // Small pause at the final checkpoint

            // Bounce back
            for (int i = 0; i < bounceBackSteps; i++)
            {
                int targetIndex = currentCheckpointIndex - 1;

                if (targetIndex < 0)
                {
                    Debug.Log($"[BotPlayer] {bot.name} can't bounce back further!");
                    break;
                }

                if (checkpoints[targetIndex] == null)
                {
                    Debug.LogError($"[BotPlayer] Checkpoint at index {targetIndex} is NULL!");
                    break;
                }

                yield return StartCoroutine(MoveToCheckpoint(bot, checkpoints[targetIndex]));
                currentCheckpointIndex = targetIndex;
                playerCheckpointIndices[bot] = currentCheckpointIndex;

                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            // Normal movement (no bounce-back needed)
            for (int i = 0; i < remainingSteps; i++)
            {
                int targetIndex = currentCheckpointIndex + 1;

                if (targetIndex >= checkpoints.Length)
                {
                    Debug.Log($"[BotPlayer] {bot.name} reached the final checkpoint!");
                    break;
                }

                if (checkpoints[targetIndex] == null)
                {
                    Debug.LogError($"[BotPlayer] Checkpoint at index {targetIndex} is NULL!");
                    break;
                }

                yield return StartCoroutine(MoveToCheckpoint(bot, checkpoints[targetIndex]));

                currentCheckpointIndex = targetIndex;
                playerCheckpointIndices[bot] = currentCheckpointIndex;

                // Check win condition after each step
                if (gameManager != null)
                {
                    gameManager.CheckBotWinCondition(bot, currentCheckpointIndex);

                    // If bot won, stop moving
                    if (gameManager.IsGameEnded())
                    {
                        Debug.Log($"[BotPlayer] {bot.name} won! Stopping movement.");
                        yield break;
                    }
                }

                yield return new WaitForSeconds(0.2f);
            }
        }
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
            t = t * t * (3f - 2f * t);

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            float arc = heightOffset * Mathf.Sin(t * Mathf.PI);
            currentPos.y += arc;

            bot.transform.position = currentPos;

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

                        // Check win condition after teleport
                        if (gameManager != null)
                        {
                            gameManager.CheckBotWinCondition(bot, applicableRule.toCheckpoint);
                        }
                    }
                    else if (applicableRule.movementType == CheckpointMovementScript.MovementType.SmoothMove)
                    {
                        yield return StartCoroutine(MoveToCheckpoint(bot, checkpoints[applicableRule.toCheckpoint]));
                        playerCheckpointIndices[bot] = applicableRule.toCheckpoint;

                        // Check win condition after smooth move
                        if (gameManager != null)
                        {
                            gameManager.CheckBotWinCondition(bot, applicableRule.toCheckpoint);
                        }
                    }
                }
            }
        }
    }

    public int GetPlayerCheckpointIndex(GameObject player)
    {
        if (playerCheckpointIndices.ContainsKey(player))
        {
            return playerCheckpointIndices[player];
        }
        return 0;
    }

    public void TriggerBotTurns()
    {
        if (!isProcessingTurn)
        {
            StartCoroutine(ProcessBotTurns());
        }
    }

    public void SetTurnIndicator(TurnIndicatorScript indicator)
    {
        turnIndicator = indicator;
        Debug.Log($"[BotPlayer] ===== SetTurnIndicator called, is null? {indicator == null} =====");
    }
}