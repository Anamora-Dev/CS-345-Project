using UnityEngine;
using UnityEngine.SceneManagement;

public enum ServiceState
{
    NewCustomer,
    ReturningFromKitchen
}

public struct Payout
{
    public int stars;       // 1 or 3
    public int tip;
    public int total;       // dish price (if correct) + tip
    public string reaction; // what the customer says
}

/// Survives scene changes and carries data between the van scene and the kitchen scene.
/// It creates itself automatically, so you don't need to place it in any scene.
public class GameManager : MonoBehaviour
{
    public const string VanScene = "VanServiceScene";
    public const string KitchenScene = "KitchenScene";

    public static GameManager Instance { get; private set; }

    [Header("Player")]
    public int Coins;
    public float TipMultiplier = 1f; // later: better truck = higher multiplier

    [Header("Current order (read by the kitchen scene)")]
    public Order CurrentOrder;
    public float OrderStartTime;
    public ServiceState State = ServiceState.NewCustomer;

    [Header("Result (written by the kitchen scene)")]
    public bool FinishedCorrectDish;
    public float FinishedQuality; // 0..1
    public float FinishedTime;    // seconds

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("GameManager");
        go.AddComponent<GameManager>(); // Awake sets Instance + DontDestroyOnLoad
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void StartOrder(Order order)
    {
        CurrentOrder = order;
        OrderStartTime = Time.time;
        State = ServiceState.NewCustomer;
    }

    public void ResetForNextCustomer()
    {
        CurrentOrder = null;
        State = ServiceState.NewCustomer;
    }

    public void FinishOrder(string dishMade, float quality01 = 1f)
    {
        FinishedTime = Time.time - OrderStartTime;
        FinishedCorrectDish = CurrentOrder != null && dishMade == CurrentOrder.dishName;
        FinishedQuality = Mathf.Clamp01(quality01);
        State = ServiceState.ReturningFromKitchen;
        SceneManager.LoadScene(VanScene);
    }


    public Payout CalculatePayout()
    {
        // How long the customer is willing to wait (set per dish in Order.cs).
        float par = (CurrentOrder != null) ? CurrentOrder.parTimeSeconds : 30f;

        bool satisfied = FinishedCorrectDish;       // right dish?
        bool onTime    = FinishedTime <= par;       // fast enough?

        // Automatic rating: 3 stars if satisfied AND on time, otherwise 1 star.
        int stars = (satisfied && onTime) ? 3 : 1;

        int tip = Mathf.RoundToInt(stars * 2f * TipMultiplier);   // 3 stars = 6 tip, 1 star = 2 tip
        int basePay = satisfied && CurrentOrder != null ? CurrentOrder.price : 0;

        string reaction;
        if (!satisfied)   reaction = "Um... this isn't what I ordered.";
        else if (!onTime) reaction = "Thanks... but that took forever.";
        else              reaction = "Great, thank you!";

        return new Payout { stars = stars, tip = tip, total = basePay + tip, reaction = reaction };
    }
}