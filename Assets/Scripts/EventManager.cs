using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject vanPanel;
    [System.Serializable] public class StorySlide
    { 
        public Sprite image;

        [TextArea(3, 10)]
        public string dialogue;
    }

    public GameObject storyPanel;

    public Image storyImage;
    public TMP_Text storyText;
    public StorySlide[] storySlides;
    private int currentSlideIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Show the main menu on start up of the game
        mainMenu.SetActive(true);
        storyPanel.SetActive(false);
        vanPanel.SetActive(false);
        currentSlideIndex = 0;
    }

    public void StartGame()
    {
        // Hide the main menu and start the game when you press Start
        mainMenu.SetActive(false);
        storyPanel.SetActive(true);
        ShowNextSlide();
    }

    public void ShowNextSlide()
    {
        if (currentSlideIndex < storySlides.Length)
        {
            // Display the current slide's image and dialogue
            storyImage.sprite = storySlides[currentSlideIndex].image;
            storyText.text = storySlides[currentSlideIndex].dialogue;
            currentSlideIndex++;
        }
        else
        {
            // All slides have been shown, start the game?
            storyPanel.SetActive(false);
            vanPanel.SetActive(true);
        }
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
