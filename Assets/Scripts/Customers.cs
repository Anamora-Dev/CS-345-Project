using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum CustomerMood
{
    Default,
    Annoyed,
    Angry
}

public class Customers : MonoBehaviour
{
    public Vector2 spawnPosition; // Position where customers will spawn offscreen
    public Vector2[] customerPositions; // Array to hold the positions of customers
    public GameObject customer; //prefab for the customer
    public CustomerAppearance[] customerAppearances; // Array to hold the customer appearances
    public Sprite[] foodSprites; // Array to hold the food sprites for the customers
    public RectTransform gameArea; // Reference to the game area RectTransform
    public float speed = 5f; // Speed at which customers move
    
    private GameObject[] spawnedCustomers; // Array to hold the spawned customer objects

    private void spawnCustomer(int index)
    {
        if (index < 0 || index >= customerPositions.Length)
        {
            Debug.LogError("Invalid customer index: " + index);
            return;
        }else
        {
            Debug.Log("Valid customer index: " + index);
        }

        if (spawnedCustomers[index] == null)
        {
            // Instantiate a new customer prefab at the spawn position
            GameObject newCustomer = Instantiate(customer, gameArea);

            // Initialize the customer with a random appearance
            CustomerBehavior customerBehavior = newCustomer.GetComponent<CustomerBehavior>();
            customerBehavior.Initialize(customerAppearances[Random.Range(0, customerAppearances.Length)]);

            // Set the position of the new customer to the spawn position
            RectTransform customerRect = newCustomer.GetComponent<RectTransform>();
            customerRect.anchoredPosition = spawnPosition; 
            spawnedCustomers[index] = newCustomer;
            Debug.Log("Spawned customer at index: " + index + " at position: " + spawnPosition);
        } else 
        {
            Debug.Log("Customer already exists at index: " + index);
        }
    }

    private bool moveCustomer(int index, Vector2 targetPosition)
    {
        if (index < 0 || index >= customerPositions.Length)
        {
            Debug.LogError("Invalid customer index: " + index);
            return false;
        }

        if (spawnedCustomers[index] != null)
        {
            RectTransform customerRect = spawnedCustomers[index].GetComponent<RectTransform>();
            customerRect.anchoredPosition = Vector2.MoveTowards(customerRect.anchoredPosition, targetPosition, speed * Time.deltaTime);

            if (Vector2.Distance(customerRect.anchoredPosition, targetPosition) < 0.1f)
            {
                // Customer has reached the target position
                return true;
            }
        }
        return false;
    }

    private int getEmptyCustomerIndex()
    {
        List<int> emptyIndices = new List<int>();
        for (int i = 0; i < spawnedCustomers.Length; i++)
        {
            if (spawnedCustomers[i] == null)
            {
                emptyIndices.Add(i);
            }
        }

        if (emptyIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, emptyIndices.Count);
            return emptyIndices[randomIndex];
        }
        else
        {
            Debug.LogWarning("No empty customer slots available.");
            return -1; // No empty slots available
        }
    }

    IEnumerator SpawnCustomerCoroutine()
    {
        Debug.Log("Starting customer spawn coroutine.");
        while (true)
        {
            Debug.Log("Spawning customer...");
            int randomIndex = getEmptyCustomerIndex();
            if (randomIndex == -1)
            {
                Debug.LogWarning("No empty customer slots available.");
                yield return new WaitForSeconds(2f); // Wait for 2 seconds before trying again
                continue;
            }
            yield return new WaitForSeconds(1f); // Wait for 1 second before spawning the customer
            Debug.Log("Attempting to spawn customer at index: " + randomIndex);
            spawnCustomer(randomIndex);
            yield return new WaitForSeconds(2f); // Wait for 2 seconds before spawning the next customer
        }
    }

    public void customerLeave(int index)
    {
        if (spawnedCustomers[index] != null)
        {

            Destroy(spawnedCustomers[index]);
            spawnedCustomers[index] = null;
            Debug.Log("Customer at index " + index + " has left.");
        }
    }

    private void showCustomerOrder(int index)
    {
        if (spawnedCustomers[index] != null)
        {
            Transform bubbleTransform = spawnedCustomers[index].transform.Find("ChatBubble");
            if (bubbleTransform != null && !bubbleTransform.gameObject.activeSelf)
            {
                bubbleTransform.gameObject.SetActive(true); // Show the bubble when the customer reaches the position
                Transform food1 = bubbleTransform.Find("Food1");
                Transform food2 = bubbleTransform.Find("Food2"); 
                Image foodRenderer = food1.GetComponent<Image>();
                Image foodRenderer2 = food2.GetComponent<Image>();
                if (foodRenderer != null && foodRenderer2 != null)
                {
                    foodRenderer.sprite = foodSprites[Random.Range(0, foodSprites.Length)]; // Assign a random food sprite
                    foodRenderer2.sprite = foodSprites[Random.Range(0, foodSprites.Length)]; // Assign a random food sprite
                }
            }
        }
    } 

    private void hideCustomerOrder(int index)
    {
        if (spawnedCustomers[index] != null)
        {
            Transform bubbleTransform = spawnedCustomers[index].transform.Find("ChatBubble");
            if (bubbleTransform != null && bubbleTransform.gameObject.activeSelf)
            {
                bubbleTransform.gameObject.SetActive(false); // Hide the bubble when the customer leaves
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Starting customer spawning process.");
        spawnedCustomers = new GameObject[customerPositions.Length];

        StartCoroutine(SpawnCustomerCoroutine()); // Start the customer spawning coroutine
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < spawnedCustomers.Length; i++)
        {
            if (spawnedCustomers[i] == null)
            {
                continue; // Skip if the customer is not spawned
            }

            CustomerBehavior behavior = spawnedCustomers[i].GetComponent<CustomerBehavior>();

            if (behavior == null)
            {
                Debug.LogWarning("CustomerBehavior component not found on the customer GameObject at index: " + i);
                continue;
            }

            switch (behavior.State)
            {
                case CustomerState.Entering:
                    if (moveCustomer(i, customerPositions[i]))
                    {
                        behavior.startWaiting(); // Start waiting when the customer reaches the position
                        showCustomerOrder(i); // Show the customer's order when they reach the position
                    }
                    break;
                case CustomerState.Waiting:
                    //Customer handles waiting state internally, no movement needed
                    break;
                case CustomerState.Leaving:
                    hideCustomerOrder(i); // Hide the customer's order when they leave
                    if (moveCustomer(i, spawnPosition))
                    {
                        customerLeave(i); // Remove the customer when they leave
                    }
                    break;
                default:
                    Debug.LogWarning("Unknown customer state: " + behavior.State);
                    break;
            }
        }
    }
    //check if customer in position, if not spawn customer at position
}
