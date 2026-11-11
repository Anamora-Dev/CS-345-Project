using System.Collections;
using UnityEngine;

/// FadeIn = black -> clear. FadeOut = clear -> black.
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [SerializeField] float duration = 0.6f;
    CanvasGroup group;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    public IEnumerator FadeIn()  { return Fade(1f, 0f); }
    public IEnumerator FadeOut() { return Fade(0f, 1f); }

    IEnumerator Fade(float from, float to)
    {
        group.blocksRaycasts = true; // block clicks while fading
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
        group.blocksRaycasts = to > 0.5f;
    }
}