using UnityEngine;
using System.Collections;

public class CameraFollowScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CheckpointMovementScript checkpointMovement;
    private GameObject playerToFollow;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 8, -5);
    [SerializeField] private Vector3 followRotationOffset = new Vector3(45, 0, 0); // Euler angles offset
    [SerializeField] private bool useLookAt = true; // If true, camera looks at player. If false, uses rotation offset
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float returnDelay = 0.5f; // Delay before returning to original position

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isFollowing = false;
    private bool shouldFollow = false;

    void Start()
    {
        // Store original camera position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        Debug.Log($"[CameraFollow] Original camera position stored: {originalPosition}");

        // Try to find CheckpointMovementScript if not assigned
        if (checkpointMovement == null)
        {
            checkpointMovement = FindFirstObjectByType<CheckpointMovementScript>();
            if (checkpointMovement != null)
            {
                Debug.Log("[CameraFollow] Found CheckpointMovementScript automatically");
            }
            else
            {
                Debug.LogWarning("[CameraFollow] CheckpointMovementScript not found!");
            }
        }
    }

    // Call this from CheckpointMovementScript to set the player
    public void SetPlayerToFollow(GameObject player)
    {
        playerToFollow = player;
        Debug.Log($"[CameraFollow] Player set to follow: {player.name}");
    }

    // Call this when movement starts
    public void StartFollowing()
    {
        Debug.Log("[CameraFollow] StartFollowing() called");
        shouldFollow = true;
        if (!isFollowing)
        {
            Debug.Log("[CameraFollow] Starting follow coroutine");
            StartCoroutine(TransitionToFollow());
        }
        else
        {
            Debug.Log("[CameraFollow] Already following, skipping transition");
        }
    }

    // Call this when movement ends
    public void StopFollowing()
    {
        Debug.Log("[CameraFollow] StopFollowing() called");
        shouldFollow = false;
        StartCoroutine(ReturnToOriginalPosition());
    }

    void LateUpdate()
    {
        if (isFollowing && playerToFollow != null)
        {
            FollowPlayer();
        }

        // Additional debug in LateUpdate
        if (showDebugInfo && isFollowing)
        {
            Debug.Log($"[CameraFollow] LateUpdate - isFollowing: {isFollowing}, useLookAt: {useLookAt}, Current Euler: {transform.rotation.eulerAngles}");
        }
    }

    private void FollowPlayer()
    {
        // Calculate target position above and behind player
        Vector3 targetPosition = playerToFollow.transform.position + followOffset;

        // Smoothly move camera to follow position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Handle rotation based on mode
        Quaternion targetRotation;

        if (useLookAt)
        {
            // Look at the player
            Vector3 lookDirection = playerToFollow.transform.position - transform.position;
            if (lookDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(lookDirection);
            }
            else
            {
                targetRotation = transform.rotation;
            }
        }
        else
        {
            // Use fixed rotation offset (absolute world rotation)
            targetRotation = Quaternion.Euler(followRotationOffset);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        Debug.Log($"[CameraFollow] Following - Pos: {transform.position}, Rot: {transform.rotation.eulerAngles}, Target Rot: {targetRotation.eulerAngles}");
    }

    private IEnumerator TransitionToFollow()
    {
        Debug.Log("[CameraFollow] Transition to follow started");
        isFollowing = true;

        // Wait a brief moment before starting to follow
        yield return new WaitForSeconds(0.1f);

        // Smooth transition to follow position
        float elapsed = 0f;
        float duration = 1f / transitionSpeed;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Debug.Log($"[CameraFollow] Transitioning from {startPos} over {duration} seconds");

        while (elapsed < duration && shouldFollow)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smooth step

            if (playerToFollow != null)
            {
                Vector3 targetPos = playerToFollow.transform.position + followOffset;

                Quaternion targetRot;
                if (useLookAt)
                {
                    Vector3 lookDir = playerToFollow.transform.position - targetPos;
                    if (lookDir != Vector3.zero)
                    {
                        targetRot = Quaternion.LookRotation(lookDir);
                    }
                    else
                    {
                        targetRot = startRot;
                    }
                }
                else
                {
                    targetRot = Quaternion.Euler(followRotationOffset);
                }

                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            }
            else
            {
                Debug.LogWarning("[CameraFollow] Player to follow is null during transition!");
            }

            yield return null;
        }

        Debug.Log("[CameraFollow] Transition to follow complete, now in follow mode");
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        Debug.Log($"[CameraFollow] Returning to original position after {returnDelay}s delay");

        // Wait a moment before returning
        yield return new WaitForSeconds(returnDelay);

        isFollowing = false;

        // Smooth transition back to original position
        float elapsed = 0f;
        float duration = 1f / transitionSpeed;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Debug.Log($"[CameraFollow] Transitioning from {startPos} to {originalPosition}");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smooth step

            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, t);

            yield return null;
        }

        // Ensure exact final position
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        Debug.Log("[CameraFollow] Return to original position complete");
    }

    // Optional: Reset camera to original position manually
    public void ResetCamera()
    {
        StopAllCoroutines();
        isFollowing = false;
        shouldFollow = false;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    // Optional: Update original position (useful if you want to change the "home" position)
    public void SetNewOriginalPosition()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }
}