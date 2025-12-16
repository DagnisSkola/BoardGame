using System.Collections;
using UnityEngine;

public class MenuSwitcher : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject mainMenu;
    public GameObject characterCreationMenu;
    public GameObject settingsMenu;
    public GameObject leaderboardMenu;

    [Header("Optional Leaderboard Script")]
    public LeaderboardScript leaderboardScript;

    void Start()
    {
        // Make sure only main menu is active at start (only if assigned)
        if (mainMenu != null)
        {
            ShowMainMenu();
        }
    }

    public void ShowMainMenu()
    {
        if (mainMenu != null) mainMenu.SetActive(true);
        if (characterCreationMenu != null) characterCreationMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (leaderboardMenu != null) leaderboardMenu.SetActive(false);
    }

    public void ShowCharacterCreation()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (characterCreationMenu != null) characterCreationMenu.SetActive(true);
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (leaderboardMenu != null) leaderboardMenu.SetActive(false);
    }

    public void ShowSettings()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (characterCreationMenu != null) characterCreationMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(true);
        if (leaderboardMenu != null) leaderboardMenu.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (characterCreationMenu != null) characterCreationMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (leaderboardMenu != null) leaderboardMenu.SetActive(true);

        // Refresh the leaderboard when showing it
        if (leaderboardScript != null)
        {
            leaderboardScript.RefreshLeaderboard();
        }
    }

    // Optional: with delay (use Invoke instead of coroutine to avoid inactive GameObject issues)
    public void ShowMainMenuDelayed(float delay)
    {
        Invoke(nameof(ShowMainMenu), delay);
    }

    public void ShowCharacterCreationDelayed(float delay)
    {
        Invoke(nameof(ShowCharacterCreation), delay);
    }

    public void ShowSettingsDelayed(float delay)
    {
        Invoke(nameof(ShowSettings), delay);
    }

    public void ShowLeaderboardDelayed(float delay)
    {
        Invoke(nameof(ShowLeaderboard), delay);
    }
}