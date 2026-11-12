using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Customers : MonoBehaviour
{
    public Vector2 spawnPosition; // Position where customers will spawn
    public Vector2[] customerPositions; // Array to hold the positions of customers
    public GameObject[] customerPrefabs; // Prefab for the customer objects
    //public int numberOfCustomers = 3; // Number of customers to spawn
    public RectTransform gameArea; // Reference to the game area RectTransform
    public float speed = 5f; // Speed at which customers move
    
    private GameObject[] spawnedCustomers; // Array to hold the spawned customer objects

    private void spawnCustomer(int index)
    {
        if (index < 0 || index >= customerPositions.Length)
        {
            Debug.LogError("Invalid customer index: " + index);
            return;
        }

        if (spawnedCustomers[index] == null)
        {
            // Instantiate a new customer prefab at the spawn position
            GameObject newCustomer = Instantiate(customerPrefabs[Random.Range(0, customerPrefabs.Length)], gameArea);
            RectTransform customerRect = newCustomer.GetComponent<RectTransform>();
            customerRect.anchoredPosition = spawnPosition; // Set the initial position of the customer
            spawnedCustomers[index] = newCustomer;
        }
    }

    IEnumerator SpawnCustomerCoroutine()
    {
        while (true)
        {
            int randomIndex = Random.Range(0, customerPositions.Length);
            yield return new WaitForSeconds(1f); // Wait for 1 second before spawning the customer
            spawnCustomer(randomIndex);
            yield return new WaitForSeconds(2f); // Wait for 2 seconds before spawning the next customer
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnedCustomers = new GameObject[customerPositions.Length];
        StartCoroutine(SpawnCustomerCoroutine()); // Start the customer spawning coroutine
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < customerPositions.Length; i++)
        {
            if (spawnedCustomers[i] != null)
            {
                RectTransform customerRect = spawnedCustomers[i].GetComponent<RectTransform>();
                customerRect.anchoredPosition = Vector2.MoveTowards(customerRect.anchoredPosition, customerPositions[i], speed * Time.deltaTime);

                if (Vector2.Distance(customerRect.anchoredPosition, customerPositions[i]) < 0.1f)
                {
                    // Customer has reached the target position
                    Transform bubbleTransform = spawnedCustomers[i].transform.Find("ChatBubble");
                    if (bubbleTransform != null)
                    {
                        bubbleTransform.gameObject.SetActive(true); // Show the bubble when the customer reaches the position
                    }
                }
            }
        }
    }
    //check if customer in position, if not spawn customer at position
}
