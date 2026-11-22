using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickDebug : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        if (EventSystem.current == null)
        {
            Debug.LogError("ClickDebug: NO EventSystem in the scene.");
            return;
        }

        var data = new PointerEventData(EventSystem.current) { position = Mouse.current.position.ReadValue() };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count == 0) Debug.Log("ClickDebug: nothing under the cursor (no Graphic Raycaster, or no raycast-target graphics).");
        foreach (var r in results) Debug.Log("ClickDebug hit: " + r.gameObject.name);
    }
}