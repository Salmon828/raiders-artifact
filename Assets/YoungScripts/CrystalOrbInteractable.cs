using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrystalOrbInteractable : MonoBehaviour
{
    public Transform playerCameraTransform;
    public float interactRange = 4f;
    public int orbIndex;

    public CrystalOrbGesture gesture; // Circular polishing gesture that picks the digit

    // The DigitText under this ball. Each orb owns its own, so the digit has to live
    // here rather than as one shared field on CrystalPuzzleUI.
    public TMP_Text digitText;

    public GameObject closeCam; // Virtual cam to activate when interacted with
    public ParticleSystem smokeSystem; // Smoke particle system, referenced here to change color

    private bool closeUp = false;

    private void Awake()
    {
        // Set the smoke color based on the attached material
        var main = smokeSystem.main;
        main.startColor = GetComponent<Renderer>().material.color;
    }

    public void ShowDigit(char digit)
    {
        if (digitText != null)
            digitText.text = digit.ToString();
    }
    private void Update()
    {
        if (!IsInteractPressed()) return;

        RaycastHit hit;
        if (!Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, interactRange)) return;

        if (hit.collider.GetComponentInParent<CrystalOrbInteractable>() != this) return;

        closeCam.SetActive(!closeUp);
        closeUp = !closeUp;

        // The gesture owns cursor state while it runs, so only fall back to setting it
        // here when no gesture component has been assigned yet.
        if (closeUp)
        {
            if (gesture != null)
            {
                gesture.BeginGesture(this);
            }
            else
            {
                Cursor.visible = true;
                // Unlock the cursor so it can move freely around the screen
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            if (gesture != null)
            {
                gesture.EndGesture();
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

    }

    // Input abstraction helper that should work with both input systems 
    private bool IsInteractPressed()
    {
        if (Keyboard.current != null)
            return Keyboard.current.eKey.wasPressedThisFrame;

        return Input.GetKeyDown(KeyCode.E);
    }
}