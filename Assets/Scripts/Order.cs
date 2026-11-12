using System;

/// One customer order
/// I will add more breakfast items here (or new meal types later).
[Serializable]
public class Order
{
    public string dishName;
    public int price;             // coins paid for a correct dish
    public float parTimeSeconds;  // finish within this time for max speed score
    public string customerLine;   // what the customer says in the speech bubble

    public Order(string dishName, int price, float parTimeSeconds, string customerLine)
    {
        this.dishName = dishName;
        this.price = price;
        this.parTimeSeconds = parTimeSeconds;
        this.customerLine = customerLine;
    }
}

public static class OrderDatabase
{
    static readonly Order[] Breakfast =
    {
        new Order("Pancake Stack",      8, 40f, "Can I get a stack of pancakes, please?"),
        new Order("Bacon & Eggs",       9, 45f, "Bacon and eggs for me, extra crispy!"),
        new Order("Breakfast Sandwich", 7, 35f, "One breakfast sandwich, I'm late for class!"),
        new Order("Coffee",             3, 15f, "Just a coffee. Please. I haven't slept."),
    };

    public static Order GetRandom()
    {
        return Breakfast[UnityEngine.Random.Range(0, Breakfast.Length)];
    }
}