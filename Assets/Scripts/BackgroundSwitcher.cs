using UnityEngine;

public class BackgroundSwitcher : MonoBehaviour
{
    GameObject current;

    void OnEnable()
    {
        LocationManager.Instance.OnLocationChanged += Apply;
        if (LocationManager.Instance.Current != null)
            Apply(LocationManager.Instance.Current);
    }

    void OnDisable()
    {
        if (LocationManager.Instance != null)
            LocationManager.Instance.OnLocationChanged -= Apply;
    }

    void Apply(LocationData loc)
    {
        if (current != null) Destroy(current);
        current = Instantiate(loc.backgroundPrefab, transform);
        current.transform.localPosition = Vector3.zero;
    }
}