using UnityEngine;
using UnityEngine.InputSystem;

public class NoteInteractable : MonoBehaviour
{
    public Transform playerCameraTransform;
    public float interactRange = 4f;
    public NoteUI noteUI;
    public Sprite noteSprite;

    private void Update()
    {
        // NoteUI handles E to close; skip if open or if this E press just closed it
        if (noteUI != null && (noteUI.IsOpen() || noteUI.ClosedThisFrame())) return;

        bool interactPressed = false;

        if (Keyboard.current != null)
            interactPressed = Keyboard.current.eKey.wasPressedThisFrame;
        else
            interactPressed = Input.GetKeyDown(KeyCode.E);

        if (!interactPressed) return;

        RaycastHit hit;
        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, interactRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (noteUI != null)
                {
                    noteUI.OpenNote(noteSprite);
                }
            }
        }
    }
}