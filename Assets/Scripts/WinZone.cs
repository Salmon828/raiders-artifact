using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WinZone : MonoBehaviour
{
    public GameObject winText; // optional, shown the moment the zone is triggered

    [Header("Ending Sequence")]
    public Sprite endImage;               // image shown after the fade to black
    public float fadeToBlackDuration = 2f;
    public float holdBlackDuration = 1f;
    public float fadeInImageDuration = 2f;
    public bool unlockCursor = true;      // free the mouse once the end image is up

    private bool _triggered = false;

    private void OnTriggerEnter(Collider other) {
        if (_triggered) return;
        _triggered = true;

        if (other.CompareTag("canPickUp") && other.GetComponent<BreakableVase>() != null) {
            Debug.Log("You win! With vase");
        } else {
            Debug.Log("You win!");
        }

        if (winText != null) winText.SetActive(true);
        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        // Build a full-screen overlay canvas on top of everything else
        GameObject canvasObj = new GameObject("EndingCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        Image black = CreateFullscreenImage(canvasObj.transform, "Black");
        black.color = new Color(0f, 0f, 0f, 0f);

        Image end = CreateFullscreenImage(canvasObj.transform, "EndImage");
        end.sprite = endImage;
        end.preserveAspect = true;
        end.color = new Color(1f, 1f, 1f, 0f);

        // Fade to black
        yield return Fade(black, 0f, 1f, fadeToBlackDuration);

        yield return new WaitForSecondsRealtime(holdBlackDuration);

        // Fade in the end image
        if (endImage != null)
            yield return Fade(end, 0f, 1f, fadeInImageDuration);
        else
            Debug.LogWarning("WinZone: no endImage assigned.");

        if (unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private Image CreateFullscreenImage(Transform parent, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = obj.AddComponent<Image>();
        img.raycastTarget = true; // blocks clicks on UI underneath
        return img;
    }

    private IEnumerator Fade(Image img, float from, float to, float duration)
    {
        Color c = img.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }
}
