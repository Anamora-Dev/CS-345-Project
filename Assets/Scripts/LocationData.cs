using UnityEngine;

[CreateAssetMenu(menuName = "FoodTruck/Location")]
public class LocationData : ScriptableObject
{
    public string displayName;
    public GameObject backgroundPrefab;   // was: Sprite background
    public Sprite mapIcon;
}