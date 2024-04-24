using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectOnGrab : MonoBehaviour
{
    public float fadeDuration = 1f; // Duration of the fade effect in seconds

    private XRGrabInteractable grabInteractable;
    private Renderer objectRenderer;
    private Color initialColor;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            initialColor = objectRenderer.material.color;
        }
    }

    private void Update()
    {
        // Check if the object is grabbed
        if (grabInteractable.isSelected)
        {
            gameObject.SetActive(false);
        }
    }
}
