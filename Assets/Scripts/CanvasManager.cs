using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class CanvasManager : MonoBehaviour
{
    // public properties to be accessed by the player's inventory.cs script
    public GameObject playerUI;
    public GameObject hotbarPanel;

    // public properties to be accessed by the chest's chest.cs script
    public GameObject chestUI;
    public GameObject submitButton;
    public GameObject closeButton;
    public GameObject equationPanel;
    public GameObject chestPanel;
    public GameObject solutionText;
    public GameObject victoryMenu;
    public GameObject lossMenu;
    public GameObject pauseMenu;
    public GameObject centerDot;
    public bool isPaused = false;

    // AudioSource for various sounds
    public AudioSource canvasAudioSource;
    public AudioClip lockedChestSound;
    public AudioClip unlockedChestSound;    

    // Start is called before the first frame update
    void Start()
    {
        canvasAudioSource = transform.GetComponent<AudioSource>();
        ResumeGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void showVictoryScreen(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        victoryMenu.SetActive(true);
        centerDot.SetActive(false);
        victoryMenu.transform.SetAsLastSibling();
        Time.timeScale = 0.0f;
    }
    public void showLossScreen(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        lossMenu.SetActive(true);
        centerDot.SetActive(false);
        lossMenu.transform.SetAsLastSibling();
        Time.timeScale = 0.0f;
    }
    void PauseGame()
    {
        pauseMenu.SetActive(true);
        centerDot.SetActive(false);
        isPaused = true;
        Time.timeScale = 0.0f;
        // unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        centerDot.SetActive(true);
        pauseMenu.SetActive(false);
        chestUI.SetActive(false);
        isPaused = false;
        Time.timeScale = 1.0f;
        // lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // this is the function called by the Resume button on the Pause Menu UI
    public void ResumeButton()
    {
        ResumeGame();
    }

    // this is the function called by the Main Menu button on the Pause Menu UI
    public void MainMenuButton()
    {
        // TODO: change this to return to the main menu
        isPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    // this means that opening the chest pauses/unpauses the game
    public void OpenChestMenu()
    {
        centerDot.SetActive(false);
        if (!isPaused)
        {
            isPaused = true;
            chestUI.SetActive(true);
            Time.timeScale = 0.0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
