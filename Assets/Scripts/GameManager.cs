using UnityEngine;
using UnityEngine.SceneManagement;

public enum ServiceState
{
    NewCustomer,
    ReturningFromKitchen
}

public struct Payout
{
    public int stars;       // 1-5
    public int tip;
    public int total;       // dish price (if correct) + tip
    public string reaction; // what the customer says
}

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

    public void FinishOrder(string dishMade, float quality01)
    {
        FinishedTime = Time.time - OrderStartTime;
        FinishedCorrectDish = CurrentOrder != null && dishMade == CurrentOrder.dishName;
        FinishedQuality = Mathf.Clamp01(quality01);
        State = ServiceState.ReturningFromKitchen;
        SceneManager.LoadScene(VanScene);
    }


    public Payout CalculatePayout()
    {
        float par = (CurrentOrder != null) ? CurrentOrder.parTimeSeconds : 30f;

        // 1.0 at or under par time, falling to 0 at double par time.
        float speed = Mathf.Clamp01(1f - (FinishedTime - par) / par);

        float score = FinishedCorrectDish
            ? 0.6f * FinishedQuality + 0.4f * speed
            : 0.1f; // wrong dish = bad rating

        int stars = Mathf.Clamp(Mathf.CeilToInt(score * 5f), 1, 5);
        int tip = Mathf.RoundToInt(stars * 2f * TipMultiplier);
        int basePay = FinishedCorrectDish && CurrentOrder != null ? CurrentOrder.price : 0;

        string reaction;
        if (!FinishedCorrectDish)      reaction = "Um... this isn't what I ordered.";
        else if (stars >= 5)           reaction = "Wow, amazing! Keep the change!";
        else if (stars >= 4)           reaction = "Great, thank you!";
        else if (stars >= 3)           reaction = "Thanks, that's fine.";
        else                           reaction = "Hmm. It took a while...";

        return new Payout { stars = stars, tip = tip, total = basePay + tip, reaction = reaction };
    }
}