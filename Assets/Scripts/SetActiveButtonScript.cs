using System.Collections;
using UnityEngine;

public class MenuSwitcher : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject mainMenu;
    public GameObject characterCreationMenu;
    public GameObject settingsMenu;

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
        mainMenu.SetActive(true);
        characterCreationMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    public void ShowCharacterCreation()
    {
        mainMenu.SetActive(false);
        characterCreationMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void ShowSettings()
    {
        mainMenu.SetActive(false);
        characterCreationMenu.SetActive(false);
        settingsMenu.SetActive(true);
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
}