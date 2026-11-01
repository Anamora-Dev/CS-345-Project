using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    //[SerializeField] private GameObject gameOverMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Show the main menu at the start of the game
        mainMenu.SetActive(true);
        Time.timeScale = 0f; // Pause the game
    }

    public void StartGame()
    {
        // Hide the main menu and start the game
        mainMenu.SetActive(false);
        Time.timeScale = 1f; // Resume the game
    }

    public void QuitGame()
    {
        // Quit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
