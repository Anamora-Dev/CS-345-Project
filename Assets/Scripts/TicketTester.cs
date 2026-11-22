using System.Collections.Generic;
using UnityEngine;

public class TicketTester : MonoBehaviour
{
    [System.Serializable]
    public class TestTicket
    {
        public int ticketNumber;
        public List<Recipe> recipes;   // 1-3
    }

    [SerializeField] private TicketView ticketPrefab;
    [SerializeField] private RectTransform ticketParent;
    [SerializeField] private List<TestTicket> testTickets;

    private void Start()
    {
        foreach (var t in testTickets)
        {
            var ticket = Instantiate(ticketPrefab, ticketParent);
            ticket.Setup(t.ticketNumber, t.recipes);
        }
    }
}