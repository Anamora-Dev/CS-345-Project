using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CustomerOrder
{
    public int ticketNumber;
    public string customerName = "Customer";
    public List<Recipe> recipes = new();
    public bool[] itemCompleted;
    public bool isExpired;

    public float TotalExpectedTime
    {
        get
        {
            if (recipes == null || recipes.Count == 0) return 0f;
            float sum = 0f;
            foreach (var r in recipes)
                if (r != null) sum += Mathf.Max(0f, r.expectedCookTime);
            return sum;
        }
    }

    public void EnsureStateArrays()
    {
        int count = recipes != null ? recipes.Count : 0;
        if (itemCompleted == null || itemCompleted.Length != count)
            itemCompleted = new bool[count];
    }
}