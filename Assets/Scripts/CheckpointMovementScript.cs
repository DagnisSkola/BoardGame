using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckpointMovementScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DiceRollScript diceRollScript;
    private GameObject playerCharacter; // Now set dynamically

    [Header("Checkpoint Settings")]
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Movement Settings")]
    [SerializeField] private float heightOffset = 0.5f; // Arc height during movement
    [SerializeField] private float teleportDelay = 0.5f; // Delay before special moves trigger

    [Header("Special Checkpoint Rules")]
    [SerializeField] private List<CheckpointRule> specialRules = new List<CheckpointRule>();

    private int currentCheckpointIndex = 0;
    private bool isMoving = false;
    private bool hasProcessedRoll = false;
    private int lastDiceValue = 0;

    [System.Serializable]
    public class CheckpointRule
    {
        [Tooltip("The checkpoint number where this rule applies")]
        public int fromCheckpoint;

        [Tooltip("The checkpoint to jump/teleport to")]
        public int toCheckpoint;

        [Tooltip("Optional: Message to display when this rule triggers")]
        public string message = "";

        [Tooltip("Type of movement to the destination")]
        public MovementType movementType = MovementType.Teleport;
    }

    public enum MovementType
    {
        Teleport,      // Instant jump
        SmoothMove     // Animated movement
    }

    void Start()
    {
        // Find DiceRollScript if not assigned
        if (diceRollScript == null)
        {
            diceRollScript = FindFirstObjectByType<DiceRollScript>();
        }
    }

    // Public method to set the player character from PlayerScript
    public void SetPlayerCharacter(GameObject player)
    {
        playerCharacter = player;

        // Position player at first checkpoint if available
        if (checkpoints.Length > 0 && playerCharacter != null)
        {
            playerCharacter.transform.position = checkpoints[0].position;
        }
    }

    void Update()
    {
        if (diceRollScript == null || playerCharacter == null || isMoving)
            return;

        // Check if dice has landed with a new roll
        if (diceRollScript.isLanded && !hasProcessedRoll)
        {
            int diceValue = ParseDiceValue(diceRollScript.diceFaceNum);

            if (diceValue > 0 && diceValue != lastDiceValue)
            {
                lastDiceValue = diceValue;
                hasProcessedRoll = true;
                StartCoroutine(MoveToCheckpoints(diceValue));
            }
        }

        // Reset processing flag when dice is rolled again
        if (!diceRollScript.isLanded)
        {
            hasProcessedRoll = false;
        }
    }

    private int ParseDiceValue(string diceFace)
    {
        // Try to parse the dice face number
        if (int.TryParse(diceFace, out int value))
        {
            return value;
        }

        Debug.LogWarning("Could not parse dice value: " + diceFace);
        return 0;
    }

    private IEnumerator MoveToCheckpoints(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            // Calculate target checkpoint
            int targetIndex = currentCheckpointIndex + 1;

            // Check if we've reached the end
            if (targetIndex >= checkpoints.Length)
            {
                Debug.Log("Player has reached the final checkpoint!");
                break;
            }

            // Move to next checkpoint
            yield return StartCoroutine(MoveToCheckpoint(checkpoints[targetIndex]));
            currentCheckpointIndex = targetIndex;

            // Small delay between movements
            yield return new WaitForSeconds(0.2f);
        }

        // Check for special rules after landing
        yield return StartCoroutine(CheckSpecialRules());

        isMoving = false;
    }

    private IEnumerator CheckSpecialRules()
    {
        // Check if current checkpoint has a special rule
        CheckpointRule applicableRule = specialRules.Find(rule => rule.fromCheckpoint == currentCheckpointIndex);

        if (applicableRule != null)
        {
            // Display message if there is one
            if (!string.IsNullOrEmpty(applicableRule.message))
            {
                Debug.Log(applicableRule.message);
            }

            // Wait a moment before applying the rule
            yield return new WaitForSeconds(teleportDelay);

            // Validate target checkpoint
            if (applicableRule.toCheckpoint >= 0 && applicableRule.toCheckpoint < checkpoints.Length)
            {
                // Apply the rule based on movement type
                if (applicableRule.movementType == MovementType.Teleport)
                {
                    // Instant teleport
                    playerCharacter.transform.position = checkpoints[applicableRule.toCheckpoint].position;
                    currentCheckpointIndex = applicableRule.toCheckpoint;
                    Debug.Log($"Teleported from checkpoint {applicableRule.fromCheckpoint} to {applicableRule.toCheckpoint}!");
                }
                else if (applicableRule.movementType == MovementType.SmoothMove)
                {
                    // Smooth animated movement
                    yield return StartCoroutine(MoveToCheckpoint(checkpoints[applicableRule.toCheckpoint]));
                    currentCheckpointIndex = applicableRule.toCheckpoint;
                    Debug.Log($"Moved from checkpoint {applicableRule.fromCheckpoint} to {applicableRule.toCheckpoint}!");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid target checkpoint {applicableRule.toCheckpoint} in special rule!");
            }
        }
    }

    private IEnumerator MoveToCheckpoint(Transform targetCheckpoint)
    {
        Vector3 startPos = playerCharacter.transform.position;
        Vector3 endPos = targetCheckpoint.position;
        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth step for easing
            t = t * t * (3f - 2f * t);

            // Linear interpolation with arc
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);

            // Add arc height (parabolic curve)
            float arc = heightOffset * Mathf.Sin(t * Mathf.PI);
            currentPos.y += arc;

            playerCharacter.transform.position = currentPos;

            // Rotate to face movement direction
            if (endPos != startPos)
            {
                Vector3 direction = (endPos - startPos).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                playerCharacter.transform.rotation = Quaternion.Slerp(
                    playerCharacter.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        // Ensure final position is exact
        playerCharacter.transform.position = endPos;
    }

    // Public method to reset player position
    public void ResetToStart()
    {
        currentCheckpointIndex = 0;
        lastDiceValue = 0;
        hasProcessedRoll = false;

        if (checkpoints.Length > 0 && playerCharacter != null)
        {
            playerCharacter.transform.position = checkpoints[0].position;
        }
    }

    // Optional: Visual debug to see checkpoint path and special rules
    private void OnDrawGizmos()
    {
        if (checkpoints == null || checkpoints.Length == 0)
            return;

        // Draw normal checkpoint path
        Gizmos.color = Color.green;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] != null)
            {
                // Draw sphere at checkpoint
                Gizmos.DrawWireSphere(checkpoints[i].position, 0.3f);

                // Draw line to next checkpoint
                if (i < checkpoints.Length - 1 && checkpoints[i + 1] != null)
                {
                    Gizmos.DrawLine(checkpoints[i].position, checkpoints[i + 1].position);
                }
            }
        }

        // Draw special rules
        Gizmos.color = Color.cyan;
        foreach (CheckpointRule rule in specialRules)
        {
            if (rule.fromCheckpoint >= 0 && rule.fromCheckpoint < checkpoints.Length &&
                rule.toCheckpoint >= 0 && rule.toCheckpoint < checkpoints.Length &&
                checkpoints[rule.fromCheckpoint] != null && checkpoints[rule.toCheckpoint] != null)
            {
                // Draw arrow from source to destination
                Vector3 from = checkpoints[rule.fromCheckpoint].position;
                Vector3 to = checkpoints[rule.toCheckpoint].position;

                Gizmos.DrawLine(from, to);

                // Draw small sphere at source checkpoint to indicate special rule
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(from, 0.4f);
                Gizmos.color = Color.cyan;
            }
        }
    }
}