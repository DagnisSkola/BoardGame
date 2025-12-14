using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CheckpointMovementScript : MonoBehaviour
{
    // Event that fires when movement is complete
    public event Action OnMovementComplete;

    [Header("References")]
    [SerializeField] private DiceRollScript diceRollScript;
    [SerializeField] private CameraFollowScript cameraFollow;
    private GameObject playerCharacter;

    [Header("Checkpoint Settings")]
    [SerializeField] private Transform checkpointParent;
    private Transform[] checkpoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Movement Settings")]
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private float teleportDelay = 0.5f;

    [Header("Special Checkpoint Rules")]
    [SerializeField] private List<CheckpointRule> specialRules = new List<CheckpointRule>();

    private int currentCheckpointIndex = 0;
    private bool isMoving = false;
    private bool hasProcessedRoll = false;
    private bool wasLanded = false;
    private bool isPlayerControlled = true;
    private bool shouldIgnoreNextLanding = false;

    [System.Serializable]
    public class CheckpointRule
    {
        public int fromCheckpoint;
        public int toCheckpoint;
        public string message = "";
        public MovementType movementType = MovementType.Teleport;
    }

    public enum MovementType
    {
        Teleport,
        SmoothMove
    }

    void Awake()
    {
        if (checkpointParent != null)
        {
            PopulateCheckpoints();
        }
    }

    void Start()
    {
        if (diceRollScript == null)
        {
            diceRollScript = FindFirstObjectByType<DiceRollScript>();
        }

        if (checkpointParent != null && (checkpoints == null || checkpoints.Length == 0))
        {
            PopulateCheckpoints();
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
        Debug.Log($"Automatically loaded {checkpoints.Length} checkpoints in order.");
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

    public void SetPlayerCharacter(GameObject player)
    {
        playerCharacter = player;
        Debug.Log($"[CheckpointMovement] Player character set: {player.name}");

        if (checkpoints == null || checkpoints.Length == 0)
        {
            if (checkpointParent != null)
            {
                PopulateCheckpoints();
            }
        }

        if (checkpoints != null && checkpoints.Length > 0 && playerCharacter != null)
        {
            playerCharacter.transform.position = checkpoints[0].position;
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetPlayerToFollow(player);
        }
    }

    void Update()
    {
        if (!isPlayerControlled || diceRollScript == null || playerCharacter == null || isMoving)
            return;

        // Detect dice landing
        if (diceRollScript.isLanded && !wasLanded && !hasProcessedRoll)
        {
            if (shouldIgnoreNextLanding)
            {
                shouldIgnoreNextLanding = false;
                Debug.Log("[CheckpointMovement] Ignoring bot's dice roll");
            }
            else
            {
                int diceValue = ParseDiceValue(diceRollScript.diceFaceNum);
                if (diceValue > 0)
                {
                    hasProcessedRoll = true;
                    StartCoroutine(MoveToCheckpoints(diceValue));
                }
            }
        }

        // Reset when dice is rolled again
        if (!diceRollScript.isLanded && wasLanded)
        {
            hasProcessedRoll = false;
        }

        wasLanded = diceRollScript.isLanded;
    }

    private int ParseDiceValue(string diceFace)
    {
        if (int.TryParse(diceFace, out int value))
        {
            return value;
        }
        return 0;
    }

    private IEnumerator MoveToCheckpoints(int steps)
    {
        isMoving = true;

        if (cameraFollow != null)
        {
            cameraFollow.StartFollowing();
        }

        for (int i = 0; i < steps; i++)
        {
            int targetIndex = currentCheckpointIndex + 1;

            if (targetIndex >= checkpoints.Length)
            {
                Debug.Log("Player has reached the final checkpoint!");
                break;
            }

            yield return StartCoroutine(MoveToCheckpoint(checkpoints[targetIndex]));
            currentCheckpointIndex = targetIndex;
            yield return new WaitForSeconds(0.2f);
        }

        yield return StartCoroutine(CheckSpecialRules());

        if (cameraFollow != null)
        {
            cameraFollow.StopFollowing();
        }

        isMoving = false;
        OnMovementComplete?.Invoke();
    }

    private IEnumerator CheckSpecialRules()
    {
        CheckpointRule applicableRule = specialRules.Find(rule => rule.fromCheckpoint == currentCheckpointIndex);

        if (applicableRule != null)
        {
            if (!string.IsNullOrEmpty(applicableRule.message))
            {
                Debug.Log(applicableRule.message);
            }

            yield return new WaitForSeconds(teleportDelay);

            if (applicableRule.toCheckpoint >= 0 && applicableRule.toCheckpoint < checkpoints.Length)
            {
                if (applicableRule.movementType == MovementType.Teleport)
                {
                    playerCharacter.transform.position = checkpoints[applicableRule.toCheckpoint].position;
                    currentCheckpointIndex = applicableRule.toCheckpoint;
                }
                else if (applicableRule.movementType == MovementType.SmoothMove)
                {
                    yield return StartCoroutine(MoveToCheckpoint(checkpoints[applicableRule.toCheckpoint]));
                    currentCheckpointIndex = applicableRule.toCheckpoint;
                }
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
            t = t * t * (3f - 2f * t);

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            float arc = heightOffset * Mathf.Sin(t * Mathf.PI);
            currentPos.y += arc;

            playerCharacter.transform.position = currentPos;

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

        playerCharacter.transform.position = endPos;
    }

    public void ResetToStart()
    {
        currentCheckpointIndex = 0;
        hasProcessedRoll = false;
        wasLanded = false;

        if (checkpoints.Length > 0 && playerCharacter != null)
        {
            playerCharacter.transform.position = checkpoints[0].position;
        }
    }

    public List<CheckpointRule> GetSpecialRules()
    {
        return specialRules;
    }

    private void OnDrawGizmos()
    {
        if (checkpoints == null || checkpoints.Length == 0)
            return;

        Gizmos.color = Color.green;
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] != null)
            {
                Gizmos.DrawWireSphere(checkpoints[i].position, 0.3f);
                if (i < checkpoints.Length - 1 && checkpoints[i + 1] != null)
                {
                    Gizmos.DrawLine(checkpoints[i].position, checkpoints[i + 1].position);
                }
            }
        }

        Gizmos.color = Color.cyan;
        foreach (CheckpointRule rule in specialRules)
        {
            if (rule.fromCheckpoint >= 0 && rule.fromCheckpoint < checkpoints.Length &&
                rule.toCheckpoint >= 0 && rule.toCheckpoint < checkpoints.Length &&
                checkpoints[rule.fromCheckpoint] != null && checkpoints[rule.toCheckpoint] != null)
            {
                Vector3 from = checkpoints[rule.fromCheckpoint].position;
                Vector3 to = checkpoints[rule.toCheckpoint].position;
                Gizmos.DrawLine(from, to);
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(from, 0.4f);
                Gizmos.color = Color.cyan;
            }
        }
    }

    public void SetPlayerControlEnabled(bool enabled)
    {
        isPlayerControlled = enabled;

        if (enabled)
        {
            // When re-enabling player control after bot turns, clear the ignore flag
            // and sync state with current dice
            if (diceRollScript != null)
            {
                wasLanded = diceRollScript.isLanded;
            }
            hasProcessedRoll = false;
            shouldIgnoreNextLanding = false;
        }
    }

    public void IgnoreNextDiceLanding()
    {
        shouldIgnoreNextLanding = true;
    }
}