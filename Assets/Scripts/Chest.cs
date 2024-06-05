using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    public Canvas canvas;
    private CanvasManager canvasManager;

    private GameObject equationPanel;
    private GameObject chestPanel;

    private ItemSlot[] equationSlots;
    private ItemSlot[] chestSlots;

    private Button submitButton;

    private GameObject solutionText;

    private Camera playerCamera;

    // a random 3 digit integer in range [100,999]
    int solution;

    // The chest has 2 inventories: one is the storage, and one is the place where you solve the equation

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = Camera.main;
        canvasManager = canvas.GetComponent<CanvasManager>();

        // create a random solution and set its text
        solution = Random.Range(100, 1000);
        solutionText = canvasManager.solutionText;
        solutionText.GetComponent<TMP_Text>().text = "= " + solution.ToString();

        // retrieve the array of ItemSlots for the equation panel (8 slots)
        equationPanel = canvasManager.equationPanel;
        equationSlots = equationPanel.GetComponentsInChildren<ItemSlot>();

        // retrieve the array of ItemSlots for the chest panel (40 slots)
        chestPanel = canvasManager.chestPanel;
        chestSlots = chestPanel.GetComponentsInChildren<ItemSlot>();

        // add listener to submit button
        submitButton = canvasManager.submitButton.GetComponent<Button>();
        submitButton.onClick.AddListener(Submit);
    }

    // Update is called once per frame
    void Update()
    {
        // open chest menu on click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // max of 5.0f away from chest
            if (Physics.Raycast(ray, out hit, 5.0f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    canvas.GetComponent<CanvasManager>().OpenChestMenu();
                }
            }
        }
    }

    void Submit()
    {
        // generate a string for the equation
        string equation = "";
        for (int i = 0; i < equationSlots.Length; i++)
        {
            if (!equationSlots[i].IsEmpty())
            {
                string text = equationSlots[i].transform.GetChild(0).GetComponent<TMP_Text>().text;
                equation += text;
            }
        }
        Debug.Log("Your equation solution is: " + Equation.CalculateSolution(equation));
        Debug.Log("Intended solution is: " + solution);
        if (Equation.CheckProblem(equation, solution))
        {
            // TODO: some win state
            canvasManager.showVictoryScreen();
            canvasManager.canvasAudioSource.PlayOneShot(canvasManager.unlockedChestSound);

        }
        else
        {
            canvasManager.canvasAudioSource.PlayOneShot(canvasManager.lockedChestSound);
        }
    }
}
