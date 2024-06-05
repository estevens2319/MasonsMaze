using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// Placeholder script for the player. Delete or modify as needed.
public class Inventory : MonoBehaviour
{
    public Canvas canvas;
    private CanvasManager canvasManager;

    public GameObject itemPrefab;

    private GameObject hotbarPanel;
    private ItemSlot[] hotbarSlots;

    private Dictionary<string, int> items;

    // Start is called before the first frame update
    void Start()
    {
        items = new Dictionary<string, int>();
        canvasManager = canvas.GetComponent<CanvasManager>();
        hotbarPanel = canvasManager.hotbarPanel;
        hotbarSlots = hotbarPanel.GetComponentsInChildren<ItemSlot>();

        // activate on start
        canvasManager.playerUI.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsHotBarFull()
    {
        return hotbarSlots.All(slot => !slot.IsEmpty());
    }

    public void add(string pickup)
    {
        // add the item into the next open hotbar slot
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i].IsEmpty())
            {
                GameObject prefab = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                prefab.transform.SetParent(hotbarSlots[i].transform);
                prefab.transform.localScale = Vector3.one;
                prefab.name = pickup + " Item";
                prefab.GetComponent<TMP_Text>().text = pickup;
                break;
            }
        }
    }

    public bool use(string pickup)
    {
        if(items.ContainsKey(pickup) && (items[pickup] > 0))
        {
            items[pickup] -= 1;
            return true;
        }
        return false;
    }

    //public bool drop(string pickup, Vector3 position, Vector3 size)
    //{
    //    if(use(pickup))
    //    {
    //        MazeRenderer.createPickup(pickup, position, size);
    //        return true;
    //    }
    //    return false;
    //}
}
