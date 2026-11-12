using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Runs the van scene.
/// First visit:   shutter opens -> OPEN sign -> customer walks up -> bubble shows order -> go to kitchen.
/// Return visit:  customer reacts -> coins added -> stars pop above their head -> they walk away -> next customer.
public class VanServiceController : MonoBehaviour
{
    [Header("Van scene (the window is part of the background image)")]
    [SerializeField] Image windowImage;      // drag the Background object here
    [SerializeField] Sprite windowClosed;    // van_scene_closed
    [SerializeField] Sprite windowOpen;      // van_scene_open
    [SerializeField] Image signImage;
    [SerializeField] Sprite signClosed;
    [SerializeField] Sprite signOpen;

    [Header("Customer")]
    [SerializeField] Image customerImage;
    [SerializeField] Transform customerStart;   // off-screen left
    [SerializeField] Transform customerStop;    // under the serving window
    [SerializeField] float walkTime = 1.5f;
    [SerializeField] Sprite[] customerSprites;  // optional: picks one at random

    [Header("Speech bubble")]
    [SerializeField] GameObject bubble;
    [SerializeField] TMP_Text bubbleText;
    [SerializeField] float typeDelay = 0.03f;
    [SerializeField] Button continueButton;     // "Take order" button

    [Header("Rating stars (3 Images that are CHILDREN of the Customer, above the head)")]
    [SerializeField] Image[] ratingStars;
    [SerializeField] Sprite starFilled;
    [SerializeField] Sprite starEmpty;

    [Header("Coins")]
    [SerializeField] TMP_Text totalCoinsText;   // always-visible counter
    [SerializeField] TMP_Text rewardText;       // optional small "+9" that flashes briefly

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
        continueButton.gameObject.SetActive(false);
        foreach (var s in ratingStars) s.gameObject.SetActive(false);
        if (rewardText != null) rewardText.gameObject.SetActive(false);
        customerImage.transform.position = customerStart.position;
        UpdateCoinCounter();

        if (gm.State == ServiceState.ReturningFromKitchen)
        {
            // Testing aid: if you run this scene on its own with no order, fake one.
            if (gm.CurrentOrder == null)
            {
                gm.CurrentOrder = OrderDatabase.GetRandom();
                gm.FinishedCorrectDish = true;
                gm.FinishedTime = 10f;
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

        windowImage.sprite = windowOpen;            // 1. shutter opens
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

        // 1. Customer reacts
        bubble.SetActive(true);
        yield return Type(p.reaction);
        yield return new WaitForSeconds(1.3f);
        bubble.SetActive(false);

        // 2. Coins go up
        gm.Coins += p.total;
        UpdateCoinCounter();
        if (rewardText != null) StartCoroutine(FlashReward("+" + p.total));

        // 3. Stars pop above their head (they are children of the customer, so they travel with them)
        for (int i = 0; i < ratingStars.Length; i++)
        {
            ratingStars[i].sprite = (i < p.stars) ? starFilled : starEmpty;
            ratingStars[i].gameObject.SetActive(true);
            StartCoroutine(Pop(ratingStars[i].transform));
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.5f);

        // 4. Customer walks away wearing the stars
        yield return Walk(customerStop.position, customerStart.position);
        gm.ResetForNextCustomer();

        // 5. Next customer
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

    IEnumerator Pop(Transform t)
    {
        // small overshoot "pop": 0 -> 1.25 -> 1
        float d1 = 0.12f, d2 = 0.08f, time = 0f;
        while (time < d1) { time += Time.deltaTime; t.localScale = Vector3.one * Mathf.Lerp(0f, 1.25f, time / d1); yield return null; }
        time = 0f;
        while (time < d2) { time += Time.deltaTime; t.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, time / d2); yield return null; }
        t.localScale = Vector3.one;
    }

    IEnumerator FlashReward(string text)
    {
        rewardText.text = text;
        rewardText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        rewardText.gameObject.SetActive(false);
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