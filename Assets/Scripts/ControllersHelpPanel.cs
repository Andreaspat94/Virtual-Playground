using UnityEngine;
using UnityEngine.UI;

public class ControllersHelpPanel : MonoBehaviour
{
    public Transform cameraTransform;
    GameObject crosshairCanvas;
    Image crosshair;
    Vector3 velocity = Vector3.zero;
    void Start()
    {
        cameraTransform = Camera.main.transform;
        crosshairCanvas = GameObject.FindWithTag("Crosshair");
        Canvas canvas = crosshairCanvas.GetComponent<Canvas>();
        crosshair = canvas.GetComponentInChildren<Image>();
    }

    void LateUpdate()
    {
        if (gameObject != null && gameObject.activeInHierarchy)
        {
            gameObject.transform.position = Vector3.SmoothDamp(
                gameObject.transform.position, crosshair.transform.position, ref velocity, 0.4f);
            
            gameObject.transform.LookAt(gameObject.transform.position + cameraTransform.rotation * Vector3.forward,
                 cameraTransform.rotation * Vector3.up);
        }
    }
}
