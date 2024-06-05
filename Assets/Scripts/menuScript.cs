using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class mainMenu : MonoBehaviour
{
    public GameObject instructionsScroll;
    public GameObject mainMenuCanvas;
    public GameObject menuButton;
    public TextMeshProUGUI floorSliderText;
    public Slider floorSlider;
    public GameObject optionsPage;
    public TextMeshProUGUI pitText;
    public TextMeshProUGUI proofText;
    private bool usePits;
    private bool useProofs;
    private bool erika;
    public TextMeshProUGUI characterText;
    private int numOfFloors;
    public menuParams menuParameters;




    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        usePits = true;
        useProofs = true;
        erika = true;
        numOfFloors = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void showInstructions(){
        mainMenuCanvas.SetActive(false);
        menuButton.SetActive(true);
        instructionsScroll.SetActive(true);
    }
    public void showGameOptions(){
        mainMenuCanvas.SetActive(false);
        menuButton.SetActive(true);
        instructionsScroll.SetActive(false);
        optionsPage.SetActive(true);
    }
    public void backToMenu(){
        mainMenuCanvas.SetActive(true);
        menuButton.SetActive(false);
        instructionsScroll.SetActive(false);
        optionsPage.SetActive(false);
    }
    public void chooseNumFloors(){
        numOfFloors = (int)floorSlider.value;
        floorSliderText.text = "" + floorSlider.value;
    }
    public void setPitTraps(){
        usePits = !usePits;
        if(usePits){
            pitText.text = "Pit Traps:\nEnabled";
        }
        else{
            pitText.text = "Pit Traps:\nDisabled";
        }
    }
    public void setProofTraps(){
        useProofs = !useProofs;
        if(useProofs){
            proofText.text = "Teleport Traps:\nEnabled";
        }
        else{
            proofText.text = "Teleport Traps:\nDisabled";
        }
    }
    
    

    public void startMaze(){
        // Load maze scene
        menuParameters.useProofTraps = useProofs;
        menuParameters.useFloorTraps = usePits;
        menuParameters.numFloors = numOfFloors;
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);

    }
}
