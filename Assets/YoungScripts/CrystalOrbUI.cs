using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrystalPuzzleUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;

    // TMP_Text, not TextMeshProUGUI, so these accept either the Canvas version
    // (TextMeshProUGUI) or the world-space one on the orbs themselves (TextMeshPro).
    public TMP_Text orbTitleText;
    public TMP_Text selectedLetterText;

    [Header("Puzzle Settings")]
    public string targetWord = "457025";

    [Header("Orb Names")]
    public string[] orbNames = new string[]
    {
        "Sloth",
        "Anger",
        "Envy",
        "Gluttony",
        "Lust",
        "Pride"
    };

    [Header("Player Lock While Panel Is Open")]
    public MonoBehaviour cameraLookScript;
    public MonoBehaviour movementScript;
    public PickUpScript pickUpScript;

    public OpenItem doorPrompt;

    private CrystalOrbInteractable currentOrb;
    private int currentLetterIndex = 0;
    private const string alphabet = "0123456789";
    private char[] selectedLetters;
    private bool isOpen = false;
    private bool hasUnlocked = false;

    private void Awake()
    {
        targetWord = targetWord.ToUpper();

        selectedLetters = new char[targetWord.Length];
        for (int i = 0; i < selectedLetters.Length; i++)
        {
            selectedLetters[i] = alphabet[0];
        }
    }

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        bool closePressed = false;

        if (Keyboard.current != null)
            closePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
        else
            closePressed = Input.GetKeyDown(KeyCode.Escape);

        if (closePressed)
            ClosePanel();
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    // Point the puzzle at an orb and load its saved digit, without showing the 2D panel.
    // The gesture path uses this on its own; OpenPanel builds on top of it.
    public void SelectOrb(CrystalOrbInteractable orb)
    {
        if (orb == null) return;

        currentOrb = orb;

        char currentLetter = GetLetter(orb.orbIndex);
        int foundIndex = alphabet.IndexOf(currentLetter);
        currentLetterIndex = foundIndex >= 0 ? foundIndex : 0;

        RefreshUI();
    }

    // The digit currently being scrolled through, which is not the same as GetLetter()
    // - that one returns the digit already committed by SaveLetter().
    public char GetSelectedLetter()
    {
        return alphabet[currentLetterIndex];
    }

    public void OpenPanel(CrystalOrbInteractable orb)
    {
        if (orb == null) return;

        SelectOrb(orb);

        if (panel != null)
            panel.SetActive(true);

        isOpen = true;

        if (cameraLookScript != null)
            cameraLookScript.enabled = false;

        if (movementScript != null)
            movementScript.enabled = false;

        if (pickUpScript != null)
            pickUpScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);

        currentOrb = null;
        isOpen = false;

        if (cameraLookScript != null)
            cameraLookScript.enabled = true;

        if (movementScript != null)
            movementScript.enabled = true;

        if (pickUpScript != null)
            pickUpScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void NextLetter()
    {
        currentLetterIndex = (currentLetterIndex + 1) % alphabet.Length;
        RefreshUI();
    }

    public void PreviousLetter()
    {
        currentLetterIndex = (currentLetterIndex - 1 + alphabet.Length) % alphabet.Length;
        RefreshUI();
    }

    public void SaveLetter()
    {
        if (currentOrb == null) return;

        char chosen = alphabet[currentLetterIndex];
        SetLetter(currentOrb.orbIndex, chosen);

        // Guarded because saving now happens on every exit, so backing out of an orb
        // while the code is already correct would replay the unlock sound each time.
        if (IsCorrectWord() && !hasUnlocked)
        {
            hasUnlocked = true;

            if (doorPrompt != null)
                doorPrompt.Unlock();
        }
        ClosePanel();
    }

    public char GetLetter(int index)
    {
        if (index < 0 || index >= selectedLetters.Length)
            return 'A';

        return selectedLetters[index];
    }

    public void SetLetter(int index, char letter)
    {
        if (index < 0 || index >= selectedLetters.Length)
            return;

        selectedLetters[index] = char.ToUpper(letter);
    }

    public string GetCurrentWord()
    {
        return new string(selectedLetters);
    }

    public bool IsCorrectWord()
    {
        return GetCurrentWord() == targetWord;
    }

    public int GetLetterCount()
    {
        return selectedLetters.Length;
    }

    private void RefreshUI()
    {
        if (currentOrb != null && orbTitleText != null)
        {
            string orbName = "Unknown";

            if (currentOrb.orbIndex >= 0 && currentOrb.orbIndex < orbNames.Length)
                orbName = orbNames[currentOrb.orbIndex];

            orbTitleText.text = "Orb of " + orbName;
        }

        if (selectedLetterText != null)
            selectedLetterText.text = alphabet[currentLetterIndex].ToString();

        // The 3D digit on the ball being polished. This is the one that matters now
        // that the puzzle is played in the world rather than through the 2D panel.
        if (currentOrb != null)
            currentOrb.ShowDigit(alphabet[currentLetterIndex]);
    }
}