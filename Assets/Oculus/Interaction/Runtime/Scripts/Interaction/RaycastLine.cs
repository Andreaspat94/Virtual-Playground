using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastLine : MonoBehaviour
{
    public int rayLength = 20;
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    private LineRenderer lineRenderer;

    void Start()
    {
        // Initialize the LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;

        // Optionally, set other properties of the LineRenderer
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        UpdateRay();
    }

    private void UpdateRay()
    {
        RaycastHit hit;
        Vector3 startPosition = transform.position;
        Vector3 endPosition;

        // Perform the raycast
        if (Physics.Raycast(startPosition, transform.forward, out hit, rayLength))
        {
            endPosition = hit.point;
        }
        else
        {
            endPosition = startPosition + transform.forward * rayLength;
        }

        // Update the LineRenderer positions
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }
}
