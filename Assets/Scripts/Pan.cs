
using System.Collections;
using UnityEngine;

public class Pan : MonoBehaviour
{
    public Transform eggSpot;
    public GameObject progressBar;
    public Transform fillPivot;
    public float cookTime = 3f;
    public Sprite friedEggSprite;

    Collider2D col;
    Draggable currentEgg;
    bool cooking;

    Vector3 originalScale;
    float originalWidth;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        progressBar.SetActive(false);

        originalScale = fillPivot.localScale;
        originalWidth = originalScale.x;
    }

    public bool TryCook(Draggable egg)
    {
        if (col == null) col = GetComponent<Collider2D>();

        if (col == null || cooking || egg == null || egg.Col == null)
            return false;

        if (!egg.Col.bounds.Intersects(col.bounds))
            return false;

        currentEgg = egg;
        StartCoroutine(Cook(egg));
        return true;
    }

    IEnumerator Cook(Draggable egg)
    {
        cooking = true;
        egg.canDrag = false;

        egg.transform.position = new Vector3(
            eggSpot.position.x,
            eggSpot.position.y,
            egg.transform.position.z
        );

        progressBar.SetActive(true);

        float t = 0f;

        while (t < cookTime)
        {
            t += Time.deltaTime;
            SetProgress(t / cookTime);
            yield return null;
        }

        SetProgress(1f);
        yield return new WaitForSeconds(0.2f);

        progressBar.SetActive(false);

        SpriteRenderer sr = egg.GetComponent<SpriteRenderer>();

        if (friedEggSprite != null)
            sr.sprite = friedEggSprite;

        egg.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
        egg.originalScale = new Vector3(0.35f, 0.35f, 1f);
        egg.tag = "CookedEgg";
        egg.canDrag = true;

        cooking = false;
    }

    void SetProgress(float p)
    {
        p = Mathf.Clamp01(p);

        float width = originalWidth * p;

        fillPivot.localScale = new Vector3(
            width,
            originalScale.y,
            originalScale.z
        );

        fillPivot.localPosition = new Vector3(
            -(originalWidth - width) / 2f,
            fillPivot.localPosition.y,
            fillPivot.localPosition.z
        );
    }
}
