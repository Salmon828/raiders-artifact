using UnityEngine;
using UnityEngine.InputSystem;

// Handles pausing and unpausing the game, as well as showing and hiding the pause menu.
public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu; // Reference to the pause menu UI
    [SerializeField] private SettingsMenu sMenu;

    void OnEnable()
    {
         sMenu.OnClosed += UnpauseGame; // Subscribe to the OnClosed event when the script is enabled
    }

    void OnDisable()
    {
        sMenu.OnClosed -= UnpauseGame; // Unsubscribe from the OnClosed event when the script is disabled
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null) return; // Ensure that the keyboard is available before checking for input, change this if you want to support other input devices like gamepads
        if (WinZone.IsEnding) return; // Don't allow pausing during the ending sequence
        if (Application.isPlaying && pauseMenu != null)
        {
            if (PausePressed())
            {
                // If a note/book is open, the pause key closes it instead of pausing
                if (NoteUI.Current != null)
                {
                    NoteUI.Current.CloseNote();
                    return;
                }

                if (pauseMenu.activeInHierarchy)
                {
                    UnpauseGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }

    bool PausePressed()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) return true;
#if UNITY_WEBGL
        // In browsers, Esc is used to release the locked mouse and usually never reaches the game,
        // so give web players other pause keys
        if (Keyboard.current.pKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame) return true;
#endif
        return false;
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor when paused
        Cursor.visible = true; // Show the cursor when paused
    }

    void UnpauseGame()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor when unpaused
        Cursor.visible = false;
    }
}
