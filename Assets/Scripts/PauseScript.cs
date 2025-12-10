using UnityEngine;
using UnityEngine.InputSystem;

public class PauseScript : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;

    private bool isPaused = false;

    void Start()
    {
        // Make sure menus are hidden at start
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }
    }

    void Update()
    {
        // Check for ESC key press to toggle pause using new Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }
        Time.timeScale = 0f; // Freeze the game
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }
        Time.timeScale = 1f; // Unfreeze the game
        isPaused = false;
    }

    // Method to attach to UI buttons
    public void OnResumeButtonClick()
    {
        ResumeGame();
    }

    // Open settings menu from pause menu   
    public void OnSettingsButtonClick()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(true);
        }
    }

    // Close settings and return to pause menu
    public void OnBackToPauseMenuClick()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
    }
}