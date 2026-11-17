using System.Collections;
using UnityEngine;

public class Pan : MonoBehaviour
{
    public Transform eggSpot;
    public GameObject progressBar;
    public Transform fillPivot;
    public float cookTime = 3f;
    public GameObject friedEggPrefab;   // replaces friedEggSprite

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

        if (friedEggPrefab != null)
        {
            Vector3 spawnPosition = eggSpot.position;
            spawnPosition.z = egg.transform.position.z;

            Instantiate(friedEggPrefab, spawnPosition, Quaternion.identity);
            Destroy(egg.gameObject);   // raw egg is replaced by the fried one
        }
        else
        {
            Debug.LogError("Pan is missing its Fried Egg Prefab!", this);
            egg.canDrag = true;
        }

        currentEgg = null;
        cooking = false;
    }

    void SetProgress(float p)
    {
        p = Mathf.Clamp01(p);

        float width = originalWidth * p;

        fillPivot.localScale = new Vector3(width, originalScale.y, originalScale.z);

        fillPivot.localPosition = new Vector3(
            -(originalWidth - width) / 2f,
            fillPivot.localPosition.y,
            fillPivot.localPosition.z
        );
    }
}