using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Circular "polishing" gesture for the crystal orbs. Circling clockwise steps to the
// next digit, counter-clockwise steps back. Replaces the old next/previous UI buttons.
public class CrystalOrbGesture : MonoBehaviour
{
    // UnityEvent<T0,T1> needs a concrete serializable subclass to show in the inspector.
    [Serializable]
    public class LetterChangedEvent : UnityEvent<int, char> { }

    [Header("References")]
    public CrystalPuzzleUI puzzleUI;

    [Header("Player Lock While Gesturing")]
    // Locks movement while close up
    public MonoBehaviour movementScript;
    public MonoBehaviour cameraLookScript;

    [Header("Gesture Tuning")]
    [Tooltip("Degrees of sweep needed to advance one digit.")]
    public float degreesPerLetter = 60f;

    [Tooltip("Multiplier on raw mouse delta.")]
    public float mouseSensitivity = 1f;

    [Tooltip("Radius in pixels the virtual pointer is clamped to.")]
    public float pointerRadius = 150f;

    [Tooltip("Frames with the pointer inside this radius are ignored - the angle is meaningless near the centre.")]
    public float deadZoneRadius = 30f;

    [Tooltip("How far the right stick must be pushed before it drives the gesture.")]
    public float stickDeadZone = 0.5f;

    [Tooltip("Flip if clockwise ends up stepping the wrong way on your setup.")]
    public bool invertDirection = false;

    [Header("Accessibility Fallbacks")]
    public bool allowKeyboardFallback = true;
    public bool allowScrollFallback = true;

    [Header("Pointer Dot")]
    [Tooltip("Show a small dot at the virtual pointer so the player can see what they are circling.")]
    public bool showPointerDot = true;

    [Tooltip("Optional UI Image to use as the dot, for your own art. Leave empty for a plain built-in one. Assumes a Screen Space - Overlay canvas.")]
    public RectTransform pointerDotImage;

    [Tooltip("Size of the built-in dot, in pixels. Ignored when Pointer Dot Image is set.")]
    public float pointerDotSize = 14f;

    [Tooltip("Colour of the built-in dot. Ignored when Pointer Dot Image is set.")]
    public Color pointerDotColor = new Color(1f, 0.95f, 0.78f, 0.9f);

    [Header("Debug")]
    [Tooltip("Draws the radius ring, the dead zone and the sweep total on screen.")]
    public bool showDebugView = false;

    [Header("Events")]
    [Tooltip("Fired on every step: (direction, new digit). Direction is +1 clockwise, -1 counter-clockwise.")]
    public LetterChangedEvent onLetterChanged;

    // Same thing as onLetterChanged, for hooking up from code instead of the inspector.
    public event Action<int, char> LetterChanged;

    private bool _active;       // between BeginGesture and EndGesture

    private Vector2 _pointer;   // offset from screen centre, in pixels
    private float _lastAngle;   // degrees, from the previous frame
    private bool _hasLastAngle;
    private float _accumulated; // running sweep total, spent one digit at a time

    private Texture2D _dotTexture;

    // --- Public API -------------------------------------------------------------

    // Call when the zoom-in starts. Picks up which orb is being edited and clears any
    // leftover sweep so the gesture always starts from nothing.
    public void BeginGesture(CrystalOrbInteractable orb)
    {
        if (puzzleUI != null)
            puzzleUI.SelectOrb(orb);

        ResetGesture();
        _active = true;
        ApplyCursorState();
        SetPlayerControlEnabled(false);
    }

    // Call when zooming back out. Commits whatever digit the player left the orb on,
    // mirroring BeginGesture's SelectOrb. Must run before anything clears currentOrb.
    public void EndGesture()
    {
        if (puzzleUI != null)
            puzzleUI.SaveLetter();

        _active = false;
        ApplyCursorState();
        SetPlayerControlEnabled(true);
    }

    public void ResetGesture()
    {
        _accumulated = 0f;
        _pointer = Vector2.zero;
        _hasLastAngle = false;
    }

    public bool IsActive()
    {
        return _active;
    }

    // --- Gesture loop -----------------------------------------------------------

    private void Update()
    {
        if (!_active) return;
        if (puzzleUI == null) return;
        if (Time.timeScale == 0f) return; // don't accumulate sweep while paused

        HandleFallbacks();

        if (!TryGetPointerAngle(out float angle))
        {
            // Nothing valid this frame - pointer is in the dead zone, or the stick was
            // released. Drop the remembered angle so the next valid frame seeds itself
            // fresh rather than measuring across the gap.
            _hasLastAngle = false;
            return;
        }

        if (!_hasLastAngle)
        {
            _lastAngle = angle;
            _hasLastAngle = true;
            return;
        }

        // DeltaAngle gives the shortest signed sweep from last to current, so crossing
        // the +180/-180 seam reads as a small step instead of a ~360 degree jump.
        float sweep = Mathf.DeltaAngle(_lastAngle, angle);
        _lastAngle = angle;

        // Atan2 grows counter-clockwise, so a clockwise circle produces a negative
        // sweep. Negate once here so positive accumulator == clockwise == next digit.
        if (!invertDirection)
            sweep = -sweep;

        _accumulated += sweep;

        // while, not if: a fast circle can cross the threshold more than once per frame.
        while (_accumulated >= degreesPerLetter)
        {
            _accumulated -= degreesPerLetter;
            Step(1);
        }

        while (_accumulated <= -degreesPerLetter)
        {
            _accumulated += degreesPerLetter;
            Step(-1);
        }
    }

    // Returns false when this frame has no usable angle.
    private bool TryGetPointerAngle(out float angle)
    {
        angle = 0f;

        // Gamepad takes priority while the stick is pushed. The stick already *is* a
        // direction, so we read its angle straight off instead of integrating it.
        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            Vector2 stick = pad.rightStick.ReadValue();
            if (stick.magnitude >= stickDeadZone)
            {
                angle = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg;
                return true;
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) return false;

        // The cursor is locked, so build our own pointer out of raw delta and clamp it
        // to a disc. Delta is already per-frame, so it must NOT be scaled by deltaTime.
        // Once the pointer hits the rim, further movement slides it *around* the rim,
        // which is what makes a straight drag read as part of a circle.
        _pointer += mouse.delta.ReadValue() * mouseSensitivity;
        _pointer = Vector2.ClampMagnitude(_pointer, pointerRadius);

        // Near the centre a tiny movement swings the angle wildly, so ignore it.
        if (_pointer.magnitude < deadZoneRadius) return false;

        angle = Mathf.Atan2(_pointer.y, _pointer.x) * Mathf.Rad2Deg;
        return true;
    }

    private void Step(int direction)
    {
        if (direction > 0)
            puzzleUI.NextLetter();
        else
            puzzleUI.PreviousLetter();

        char letter = puzzleUI.GetSelectedLetter();

        if (onLetterChanged != null)
            onLetterChanged.Invoke(direction, letter);

        if (LetterChanged != null)
            LetterChanged.Invoke(direction, letter);
    }

    // Arrow keys and the scroll wheel, kept as an accessible alternative to circling.
    private void HandleFallbacks()
    {
        if (allowKeyboardFallback && Keyboard.current != null)
        {
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) Step(1);
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame) Step(-1);
        }

        if (allowScrollFallback && Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (scroll > 0f) Step(1);
            else if (scroll < 0f) Step(-1);
        }
    }

    // The cursor stays locked and hidden throughout - both while polishing, since the
    // gesture runs on raw delta, and afterwards, which is the normal first-person state.
    private void ApplyCursorState()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetPlayerControlEnabled(bool value)
    {
        if (movementScript != null)
            movementScript.enabled = value;

        if (cameraLookScript != null)
            cameraLookScript.enabled = value;
    }

    private static Vector2 ScreenCentre()
    {
        return new Vector2(Screen.width, Screen.height) * 0.5f;
    }

    // --- Pointer dot ------------------------------------------------------------

    // Where the virtual pointer sits on screen, in normal y-up screen coordinates.
    public Vector2 GetPointerScreenPosition()
    {
        return ScreenCentre() + _pointer;
    }

    private bool ShouldShowPointerDot()
    {
        return showPointerDot && _active;
    }

    // Driven from LateUpdate so the dot lands on the pointer position this frame
    // rather than trailing it by one.
    private void LateUpdate()
    {
        if (pointerDotImage == null) return;

        bool visible = ShouldShowPointerDot();

        if (pointerDotImage.gameObject.activeSelf != visible)
            pointerDotImage.gameObject.SetActive(visible);

        if (visible)
            pointerDotImage.position = GetPointerScreenPosition();
    }

    // --- Drawing ----------------------------------------------------------------

    private void OnGUI()
    {
        // Only draw the built-in dot when no custom image was supplied.
        bool drawBuiltInDot = ShouldShowPointerDot() && pointerDotImage == null;
        bool drawDebug = showDebugView && _active;

        if (!drawBuiltInDot && !drawDebug) return;

        EnsureDotTexture();

        Color previousColor = GUI.color;
        Vector2 pointer = ToGuiPoint(GetPointerScreenPosition());

        if (drawDebug)
        {
            Vector2 centre = ScreenCentre();

            DrawDot(centre, pointerRadius * 2f, new Color(0f, 1f, 1f, 0.08f));
            DrawRing(centre, pointerRadius, 64, Color.cyan);
            DrawRing(centre, deadZoneRadius, 32, new Color(1f, 0.5f, 0f));

            GUI.color = Color.white;
            GUI.Label(
                new Rect(centre.x - 70f, centre.y + pointerRadius + 10f, 240f, 20f),
                string.Format("sweep {0:0.#} / {1:0.#} deg", _accumulated, degreesPerLetter));
        }

        if (drawBuiltInDot)
            DrawDot(pointer, pointerDotSize, pointerDotColor);
        else if (drawDebug)
            DrawDot(pointer, 10f, Color.yellow);

        GUI.color = previousColor;
    }

    // GUI y grows downward but screen y grows upward, so flip y before drawing.
    private static Vector2 ToGuiPoint(Vector2 screenPosition)
    {
        return new Vector2(screenPosition.x, Screen.height - screenPosition.y);
    }

    private void EnsureDotTexture()
    {
        if (_dotTexture != null) return;

        // A soft-edged white circle, so the dot reads as a dot rather than a square.
        const int size = 32;
        const float edge = 0.2f; // fraction of the radius spent fading out

        _dotTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        _dotTexture.wrapMode = TextureWrapMode.Clamp;

        float radius = size * 0.5f;
        Vector2 centre = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 0 at the middle, 1 at the rim.
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), centre) / radius;
                float alpha = Mathf.Clamp01((1f - distance) / edge);

                _dotTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        _dotTexture.Apply();
    }

    private void DrawRing(Vector2 centre, float radius, int segments, Color color)
    {
        for (int i = 0; i < segments; i++)
        {
            float a = (i / (float)segments) * Mathf.PI * 2f;
            DrawDot(centre + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * radius, 3f, color);
        }
    }

    private void DrawDot(Vector2 position, float size, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(new Rect(position.x - size * 0.5f, position.y - size * 0.5f, size, size), _dotTexture);
    }

    private void OnDestroy()
    {
        if (_dotTexture != null)
            Destroy(_dotTexture);
    }
}
