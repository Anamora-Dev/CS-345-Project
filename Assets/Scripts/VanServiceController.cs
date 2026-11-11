using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Runs the van scene.
/// First visit:   window opens -> OPEN sign -> customer walks up -> bubble shows order -> go to kitchen.
/// Return visit:  customer reacts -> stars + coins -> next customer.
public class VanServiceController : MonoBehaviour
{
    [Header("Van")]
    [SerializeField] Image windowImage;
    [SerializeField] Sprite windowClosed;
    [SerializeField] Sprite windowOpen;
    [SerializeField] Image signImage;
    [SerializeField] Sprite signClosed;
    [SerializeField] Sprite signOpen;

    [Header("Customer")]
    [SerializeField] Image customerImage;
    [SerializeField] Transform customerStart;   // empty object off-screen
    [SerializeField] Transform customerStop;    // empty object at the window
    [SerializeField] float walkTime = 1.5f;
    [SerializeField] Sprite[] customerSprites;  // optional: picks one at random

    [Header("Speech bubble")]
    [SerializeField] GameObject bubble;
    [SerializeField] TMP_Text bubbleText;
    [SerializeField] float typeDelay = 0.03f;
    [SerializeField] Button continueButton;     // "Take order" / "Next customer" button

    [Header("Result UI")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] GameObject[] filledStars;  // 5 star icons drawn on top of empty ones
    [SerializeField] TMP_Text rewardText;       // "+13 coins"
    [SerializeField] TMP_Text totalCoinsText;   // always-visible coin counter

    [Header("Transition")]
    [SerializeField] ScreenFader fader;

    bool continuePressed;

    void Awake()
    {
        continueButton.onClick.AddListener(() => continuePressed = true);
    }

    IEnumerator Start()
    {
        var gm = GameManager.Instance;

        bubble.SetActive(false);
        resultPanel.SetActive(false);
        continueButton.gameObject.SetActive(false);
        customerImage.transform.position = customerStart.position;
        UpdateCoinCounter();

        if (gm.State == ServiceState.ReturningFromKitchen)
        {
            // Testing aid: if you run this scene on its own with no order, fake one.
            if (gm.CurrentOrder == null)
            {
                gm.CurrentOrder = OrderDatabase.GetRandom();
                gm.FinishedCorrectDish = true;
                gm.FinishedQuality = 0.8f;
                gm.FinishedTime = 30f;
            }

            SetShopOpen(true);
            customerImage.transform.position = customerStop.position;
            yield return fader.FadeIn();
            yield return ReturnFlow(gm);
        }
        else
        {
            SetShopOpen(false);
            yield return fader.FadeIn();
            yield return NewCustomerFlow(gm);
        }
    }

    IEnumerator NewCustomerFlow(GameManager gm)
    {
        yield return new WaitForSeconds(0.5f);

        windowImage.sprite = windowOpen;            // 1. window opens
        yield return new WaitForSeconds(0.5f);

        signImage.sprite = signOpen;                // 2. OPEN sign
        yield return new WaitForSeconds(0.8f);

        if (customerSprites != null && customerSprites.Length > 0)
            customerImage.sprite = customerSprites[Random.Range(0, customerSprites.Length)];

        yield return Walk(customerStart.position, customerStop.position); // 3. customer walks up

        Order order = OrderDatabase.GetRandom();    // 4. customer says their order
        bubble.SetActive(true);
        yield return Type(order.customerLine);

        yield return WaitForContinue("Take order");

        gm.StartOrder(order);                       // timer starts now
        yield return fader.FadeOut();
        SceneManager.LoadScene(GameManager.KitchenScene); // 5. go cook
    }


    IEnumerator ReturnFlow(GameManager gm)
    {
        Payout p = gm.CalculatePayout();

        bubble.SetActive(true);
        yield return Type(p.reaction);

        resultPanel.SetActive(true);
        foreach (var s in filledStars) s.SetActive(false);
        for (int i = 0; i < p.stars && i < filledStars.Length; i++)
        {
            filledStars[i].SetActive(true);
            yield return new WaitForSeconds(0.25f);
        }

        gm.Coins += p.total;
        rewardText.text = "+" + p.total + " coins" + (p.tip > 0 ? "  (tip " + p.tip + ")" : "");
        UpdateCoinCounter();

        yield return WaitForContinue("Next customer");

        // Customer leaves, shop closes for a moment, then a new customer arrives.
        resultPanel.SetActive(false);
        bubble.SetActive(false);
        yield return Walk(customerStop.position, customerStart.position);
        gm.ResetForNextCustomer();

        yield return fader.FadeOut();
        SceneManager.LoadScene(GameManager.VanScene);
    }

    void SetShopOpen(bool open)
    {
        windowImage.sprite = open ? windowOpen : windowClosed;
        signImage.sprite = open ? signOpen : signClosed;
    }

    IEnumerator Walk(Vector3 from, Vector3 to)
    {
        float t = 0f;
        while (t < walkTime)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / walkTime);
            customerImage.transform.position = Vector3.Lerp(from, to, k);
            yield return null;
        }
        customerImage.transform.position = to;
    }

    IEnumerator Type(string line)
    {
        bubbleText.text = line;
        bubbleText.maxVisibleCharacters = 0;
        for (int i = 1; i <= line.Length; i++)
        {
            bubbleText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typeDelay);
        }
    }

    IEnumerator WaitForContinue(string label)
    {
        continuePressed = false;
        var tmp = continueButton.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.text = label;
        continueButton.gameObject.SetActive(true);
        yield return new WaitUntil(() => continuePressed);
        continueButton.gameObject.SetActive(false);
    }

    void UpdateCoinCounter()
    {
        if (totalCoinsText != null)
            totalCoinsText.text = GameManager.Instance.Coins.ToString();
    }
}