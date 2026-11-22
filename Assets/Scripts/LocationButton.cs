using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationButton : MonoBehaviour
{
    public LocationData location;   // drag the asset in per button
    public TMP_Text label;          // optional
    public Image icon;              // optional

    void Start()
    {
        if (label != null) label.text = location.displayName;
        if (icon != null && location.mapIcon != null) icon.sprite = location.mapIcon;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        LocationManager.Instance.SetLocation(location);
        GetComponentInParent<PhoneController>().Close();
    }
}