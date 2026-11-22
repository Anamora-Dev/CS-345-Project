using System;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    public LocationData[] locations;   // drag all three here
    public LocationData Current { get; private set; }
    public event Action<LocationData> OnLocationChanged;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetLocation(locations[0]);   // starting spot
    }

    public void SetLocation(LocationData loc)
    {
        if (loc == null || loc == Current) return;
        Current = loc;
        OnLocationChanged?.Invoke(loc);
    }
}