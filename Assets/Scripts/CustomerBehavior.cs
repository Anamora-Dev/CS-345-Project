using UnityEngine;
using UnityEngine.UI;

public enum CustomerState
{
    Entering, 
    Waiting,
    Leaving
}

public class CustomerBehavior : MonoBehaviour
{
    public CustomerAppearance appearance; // Appearance of the customer
    private Image bodyImage; // Reference to the Image component of the customer's body
    public CustomerMood mood; // Current mood of the customer
    public float moodChangeTimer; // Timer to track mood change
    private CustomerState state; // Current state of the customer

    public CustomerState State => state; // Public getter for the customer state
    private RectTransform customerRectTransform; // Reference to the customer's RectTransform

    public int orderNumber;
    public int[] orderItems;

    public void Initialize(CustomerAppearance customerAppearance)
    {
        appearance = customerAppearance;

        bodyImage = transform.Find("Body").GetComponent<Image>();
        if (bodyImage != null)
        {
            bodyImage.sprite = appearance.defaultSprite; // Set the default sprite for the customer
        }
        else
        {
            Debug.LogWarning("Body Image component not found on the customer GameObject.");
        }

        mood = CustomerMood.Default; // Initialize with a default mood
        state = CustomerState.Entering; // Initialize the customer state
        customerRectTransform = GetComponent<RectTransform>(); // Get the RectTransform component
    }

    public void startWaiting()
    {
        if (state != CustomerState.Entering)
        {
            Debug.LogWarning("Customer is not in the Entering state. Cannot start waiting.");
            return;
        }

        state = CustomerState.Waiting; // Change the state to waiting
        moodChangeTimer = 0f; // Reset the mood change timer
        Debug.Log("Customer has started waiting. Mood change timer started.");
    }

    public void ChangeMood()
    {
        // Cycle through moods: Default -> Annoyed -> Angry -> Default
        switch (mood)
        {
            case CustomerMood.Default:
                mood = CustomerMood.Annoyed;
                bodyImage.sprite = appearance.annoyedSprite; // Change sprite to annoyed
                break;
            case CustomerMood.Annoyed:
                mood = CustomerMood.Angry;
                bodyImage.sprite = appearance.angrySprite; // Change sprite to angry
                break;
            default:
                Debug.LogWarning("Invalid mood value: " + mood);
                break;
        }
    }

    public void SetOrder(int number, int[] items)
    {
        orderNumber = number;
        orderItems = items;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (state == CustomerState.Waiting)
        {
            // Increment the mood change timer only if the customer is waiting
            moodChangeTimer += Time.deltaTime;

            // Check if it's time to change the mood
            if (moodChangeTimer >= 10f && mood == CustomerMood.Default) // Change to annoyed after 10 seconds
            {
                ChangeMood();
            } else if (moodChangeTimer >= 20f && mood == CustomerMood.Annoyed) // Change to angry after 20 seconds
            {
                ChangeMood();
            } else if (moodChangeTimer >= 30f && mood == CustomerMood.Angry) // Reset to default after 30 seconds
            {
                state = CustomerState.Leaving; // Change state to leaving
            }
        }
    }
}
