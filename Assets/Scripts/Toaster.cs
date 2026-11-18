
using System.Collections;
using UnityEngine;

public class Toaster : MonoBehaviour, IDropTarget
{
    [Header("Bread")]
    [SerializeField] IngredientData rawBreadData;
    [SerializeField] Ingredient toastedBreadPrefab;
    [SerializeField] Transform breadSpot;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string toastState = "Toast";
    [SerializeField] string idleState = "Idle";

    bool busy;

    void Awake()
    {
        if (!animator)
            Debug.LogError("Toaster needs an Animator.", this);

        if (!breadSpot)
            Debug.LogError("Toaster needs a Bread Spot.", this);

        if (!rawBreadData)
            Debug.LogError("Assign Raw Bread Data.", this);

        if (!toastedBreadPrefab)
            Debug.LogError("Assign Toasted Bread Prefab.", this);

        if (animator)
        {
            animator.ResetTrigger("Toast");
            animator.Play(idleState, 0, 0f);
        }
    }

    public bool TryAccept(Ingredient item)
    {
        if (busy || item == null)
            return false;

        // Only accept raw bread
        if (item.data != rawBreadData)
            return false;

        if (!animator || !breadSpot || !toastedBreadPrefab)
            return false;

        // Keep the raw bread alive during the animation
        item.KeepAfterAccept();

        busy = true;

        StartCoroutine(Toast(item));

        return true;
    }

    IEnumerator Toast(Ingredient item)
    {
        Draggable drag = item.GetComponent<Draggable>();

        if (drag)
            drag.canDrag = false;

        // Hide raw bread
        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();

        if (sr)
            sr.enabled = false;

        // Move bread into toaster
        item.transform.position = new Vector3(
            breadSpot.position.x,
            breadSpot.position.y,
            item.transform.position.z
        );

        // Play animation
        animator.ResetTrigger("Toast");
        animator.SetTrigger("Toast");

        yield return null;

        // Wait for animation to start
        while (!animator.GetCurrentAnimatorStateInfo(0)
            .IsName(toastState))
        {
            yield return null;
        }

        // Wait for animation to finish
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        // Spawn toasted bread
        Vector3 spawnPosition = breadSpot.position;
        spawnPosition.z = item.transform.position.z;

        Ingredient newBread = Instantiate(
            toastedBreadPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Draggable newDrag = newBread.GetComponent<Draggable>();

        if (newDrag)
            newDrag.canDrag = true;

        // Remove raw bread
        Destroy(item.gameObject);

        // Reset toaster
        animator.ResetTrigger("Toast");
        animator.Play(idleState, 0, 0f);

        busy = false;
    }
}
