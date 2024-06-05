using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// used tips from this tutorial: https://www.youtube.com/watch?v=kWRyZ3hb1Vc
public class ItemSlot : MonoBehaviour, IDropHandler
{
    private AudioSource canvasAudioSource;
    public AudioClip dropItemClip;

    void Start()
    {
        canvasAudioSource = transform.root.GetComponent<AudioSource>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        // if a DraggableItem is dropped onto this (open) ItemSlot, set its parent to this ItemSlot
        if (IsEmpty())
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            draggableItem.parentAfterDrag = transform;
            canvasAudioSource.PlayOneShot(dropItemClip);
        }
    }

    // public method for checking the ItemSlot's emptiness
    public bool IsEmpty()
    {
        return (transform.childCount == 0);
    }
}
