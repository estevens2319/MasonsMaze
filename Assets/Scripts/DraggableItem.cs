using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// used tips from this tutorial: https://www.youtube.com/watch?v=kWRyZ3hb1Vc
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentAfterDrag;
    public TMP_Text text;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // used to snap the DraggableItem back to its correct parent ItemSlot upon release
        parentAfterDrag = transform.parent;
        // ensures that the DraggableItem isn't covered by other UI elements
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        text.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // snap DraggableItem back to parentAfterDrag, which is either its original ItemSlot or a new ItemSlot
        transform.SetParent(parentAfterDrag);
        text.raycastTarget = true;
    }
}
