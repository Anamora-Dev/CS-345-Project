using UnityEngine;

public class PhoneController : MonoBehaviour
{
    public GameObject homeScreen;
    public GameObject[] appScreens;   // Maps, Bank, Depot, Webstaurant, Recipes

    public void Open()  { gameObject.SetActive(true); ShowHome(); }
    public void Close() { gameObject.SetActive(false); }

    public void ShowHome()
    {
        HideAllApps();
        homeScreen.SetActive(true);
    }

    // Wire each app icon's OnClick to this, and drag that app's screen into the slot
    public void OpenApp(GameObject screen)
    {
        homeScreen.SetActive(false);
        HideAllApps();
        screen.SetActive(true);
    }

    void HideAllApps()
    {
        foreach (var s in appScreens) s.SetActive(false);
    }
}