using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarterAssets;

public class NoteUI : MonoBehaviour
{
    public GameObject notePanel;
    public Image noteImage;

    // The note currently on screen, if any. PauseManager uses this so the pause key
    // (Esc, or P/Tab on web) closes the note instead of opening the pause menu.
    public static NoteUI Current { get; private set; }

    private bool isOpen = false;

    // Frame numbers used so the same E press can't open and close (or close and reopen)
    // a note, regardless of which script's Update runs first
    private int openedFrame = -1;
    private int closedFrame = -1;

    private void Start()
    {
        if (notePanel != null)
            notePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen || openedFrame == Time.frameCount) return;

        bool togglePressed = Keyboard.current != null
            ? Keyboard.current.eKey.wasPressedThisFrame
            : Input.GetKeyDown(KeyCode.E);

        if (togglePressed)
        {
            CloseNote();
        }
    }

    public void OpenNote(Sprite spriteToShow)
    {
        if (notePanel != null)
            notePanel.SetActive(true);

        if (noteImage != null)
            noteImage.sprite = spriteToShow;

        isOpen = true;
        openedFrame = Time.frameCount;
        Current = this;

        // Keep the cursor locked and hidden, and freeze the camera while reading
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        FirstPersonController.LookBlocked = true;
    }

    public void CloseNote()
    {
        if (notePanel != null)
            notePanel.SetActive(false);

        isOpen = false;
        closedFrame = Time.frameCount;
        if (Current == this) Current = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        FirstPersonController.LookBlocked = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    // True if the note just closed this frame, so the E press that closed it doesn't reopen it
    public bool ClosedThisFrame()
    {
        return closedFrame == Time.frameCount;
    }

    private void OnDestroy()
    {
        // Don't leave stale static state behind when the scene unloads
        if (Current == this)
        {
            Current = null;
            FirstPersonController.LookBlocked = false;
        }
    }
}
