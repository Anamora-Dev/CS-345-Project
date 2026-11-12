
using System.Collections;
using UnityEngine;

public class Toaster : MonoBehaviour
{
    public Transform breadSpot;
    public Animator animator;

    public string toastState = "Toast";
    public string idleState = "Idle";

    public GameObject toastedBreadPrefab;

    private Collider2D col;
    private bool busy;
    private bool toasted;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (col == null)
            Debug.LogError("Toaster needs a Collider2D.", this);

        if (animator == null)
            Debug.LogError("Toaster needs an Animator.", this);

        if (breadSpot == null)
            Debug.LogError("Toaster needs a Bread Spot.", this);

        if (animator != null)
        {
            animator.ResetTrigger("Toast");
            animator.Play(idleState, 0, 0f);
        }
    }

    void OnEnable()
    {
        Draggable.AnyDropped += OnBreadDropped;
    }

    void OnDisable()
    {
        Draggable.AnyDropped -= OnBreadDropped;
    }

    void OnBreadDropped(Draggable b)
    {
        if (busy) return;
        if (b == null || col == null || b.Col == null) return;

        if (!b.CompareTag("WhiteBread")) return;

        if (!b.Col.Distance(col).isOverlapped) return;

        StartCoroutine(Toast(b));
    }

    IEnumerator Toast(Draggable b)
    {
        busy = true;
        b.canDrag = false;

        SpriteRenderer sr = b.GetComponent<SpriteRenderer>();

        // Hide raw bread
        if (sr != null)
            sr.enabled = false;

        // Move bread into toaster
        b.transform.position = new Vector3(
            breadSpot.position.x,
            breadSpot.position.y,
            b.transform.position.z
        );

        // Play toaster animation
        animator.ResetTrigger("Toast");
        animator.SetTrigger("Toast");

        yield return null;

        // Wait for animation to start
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(toastState))
            yield return null;

        // Wait for animation to finish
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        // Spawn toasted bread
        if (toastedBreadPrefab != null)
        {
            Vector3 spawnPosition = breadSpot.position;
            spawnPosition.z = b.transform.position.z;

            GameObject newBread = Instantiate(
                toastedBreadPrefab,
                spawnPosition,
                Quaternion.identity
            );

            Draggable newDraggable = newBread.GetComponent<Draggable>();

            if (newDraggable != null)
                newDraggable.canDrag = true;

            Destroy(b.gameObject);
        }
        else
        {
            Debug.LogError("Toasted Bread Prefab is missing!");
        }

        // Reset toaster
        busy = false;

        animator.ResetTrigger("Toast");
        animator.Play(idleState, 0, 0f);
    }
}
