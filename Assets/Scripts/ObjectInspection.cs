using System.Collections;
using UnityEngine;

public class ObjectInspection : MonoBehaviour
{
    [Header("Inspection Settings")]
    // Speed at which the object rotates
    [SerializeField] float rotateSpeed;
    // Speed at which the object zooms in and out
    [SerializeField] float zoomSpeed;
    // Maximum zoom distance
    [SerializeField] float zoomDistance = 10f;
    // The point where the object will be inspected from
    [SerializeField] Transform inspectPoint;
    // The layer that defines what can be inspected
    [SerializeField] LayerMask inspectLayer;

    // The object currently being inspected
    GameObject m_inspectObject;
    // Flag to check if the player is inspecting an object
    bool isInspecting;
    // Reference to the main camera
    Camera m_camera;
    // Reference to the player controller
    FPSController m_fpsController;
    // The original position of the object before inspection
    Vector3 m_originalObjectPos;
    // The original rotation of the object before inspection
    Quaternion m_originalObjectRot;
    // The initial position of the inspection point
    Vector3 m_initialInspectPos;

    void Awake()
    {
        // Catch the main camera reference
        m_camera = Camera.main;

        // Find and catch the player controller reference
        m_fpsController = FindObjectOfType<FPSController>();
    }

    void Update()
    {
        SelectObject();
    }

    void SelectObject()
    {
        // Cast a ray from the camera in the forward direction
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 3f, inspectLayer))
        {
            if (Input.GetMouseButtonDown(0) && !isInspecting)
            {
                // Save the original position and rotation of the selected object
                SaveObjectTransform(hit.transform.gameObject);

                // Move the object to the inspection point
                SetObjectTransform(inspectPoint.transform.position, inspectPoint.transform.rotation, inspectPoint);

                // Start inspecting the object
                InitializeInspection();
            }
        }
    }

    void InitializeInspection()
    {
        // Set the inspecting flag to true
        isInspecting = true;

        // Restrict player movement while inspecting
        m_fpsController.RestrictMovement(isInspecting);

        // Save the initial position of the inspect point
        m_initialInspectPos = inspectPoint.transform.position;

        // Begin the inspection coroutine
        StartCoroutine(StartInspection());
    }

    IEnumerator StartInspection()
    {
        // Continuously allow rotation and zoom until the right mouse button is press
        while (isInspecting == true)
        {
            // Rotate the object based on mouse input
            RotateObject();

            // Zoom the object based on mouse scroll input
            ZoomObject();

            // Check for the right mouse button press to end the inspection
            if (Input.GetMouseButtonDown(1))
                EndInspection();

            yield return null;
        }
    }

    void EndInspection()
    {
        // Set the inspecting flag to false
        isInspecting = false;

        // Allow player movement again after inspection is ended
        m_fpsController.RestrictMovement(isInspecting);

        // Restore the original position of the inspect point
        inspectPoint.transform.position = m_initialInspectPos;

        // Reset the inspect point's local rotation to zero
        inspectPoint.transform.localEulerAngles = Vector3.zero;

        // Restore the original object position and rotation and remove its parent
        SetObjectTransform(m_originalObjectPos, m_originalObjectRot, null);
    }

    void RotateObject()
    {
        // Calculate the right vector using the camera's up direction and the direction to the inspect point
        Vector3 right = Vector3.Cross(m_camera.transform.up, inspectPoint.position - m_camera.transform.position);

        // Calculate the up vector, ensuring it's perpendicular to the right vector and the direction to the inspect point
        Vector3 up = Vector3.Cross(inspectPoint.position - m_camera.transform.position, right);

        // Rotate around the up vector for horizontal movement (mouse X)
        inspectPoint.transform.rotation = Quaternion.AngleAxis(-GetMouseInput().x, up) * inspectPoint.transform.rotation;

        // Rotate around the right vector for vertical movement (mouse Y)
        inspectPoint.transform.rotation = Quaternion.AngleAxis(GetMouseInput().y, right) * inspectPoint.transform.rotation;
    }


    void ZoomObject()
    {
        // Get scroll input
        float scrollInput = GetScrollInput();

        if (scrollInput != 0)
        {
            // Store the camera's forward direction
            Vector3 camereDirection = m_camera.transform.forward;

            // Move inspectPos forward or backward based on scroll input
            inspectPoint.position += scrollInput > 0 ? camereDirection * zoomSpeed : -camereDirection * zoomSpeed;

            // Calculate the offset from the initial position
            Vector3 offset = inspectPoint.position - m_initialInspectPos;

            // Clamp the position to the specified zoom distance
            inspectPoint.position = Vector3.ClampMagnitude(offset, zoomDistance) + m_initialInspectPos;
        }
    }

    #region Utility Methods
    void SaveObjectTransform(GameObject target)
    {
        // Set the inspect object to the target object
        m_inspectObject = target.gameObject;

        // Save the target object's current position and rotation
        m_originalObjectPos = m_inspectObject.transform.position;
        m_originalObjectRot = m_inspectObject.transform.rotation;
    }

    void SetObjectTransform(Vector3 position, Quaternion rotation, Transform parent)
    {
        // Update the inspect object's position and rotation
        m_inspectObject.transform.position = position;
        m_inspectObject.transform.rotation = rotation;

        // Set the inspect object's parent to the specified parent
        m_inspectObject.transform.parent = parent;
    }
    #endregion

    #region Input Methods
    Vector2 GetMouseInput()
    {
        Vector2 input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        return input *= rotateSpeed * Time.deltaTime;
    }
    float GetScrollInput()
    {
        return Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
    }
    #endregion
}
